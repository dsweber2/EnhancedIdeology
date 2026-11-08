using System.Text;
using System.Text.Json;
using Xunit.Abstractions;

namespace EnhancedIdeology.Tests;

// Runs LadderHeuristic over every multi-rung ladder installed locally (tests/Fixtures/ladders.json, written by
// `make catalog`) and compares it with the hand tables. The heuristic may flag an issue, but where it is sure,
// it must agree with the tables on category, order (or its mirror image) and the gap that neutral sits in.
public class LadderHeuristicBacktest(ITestOutputHelper output) : SeededTest
{
    private sealed record FixtureRung(
        string DefName, int DisplayOrderInIssue, float DefaultSelectionWeight, bool HasEffects, bool NeedsMeme, string Mod);

    private static HeuristicRung Rung(string defName, float weight = 0f, bool needsMeme = false) =>
        new(defName, weight, false, needsMeme);

    [Fact]
    public void DefaultRungsThatAllNeedAMeme_AreIgnored()
    {
        var guess = LadderHeuristic.Classify(
            [Rung("Issue_Exalted", 1f, needsMeme: true), Rung("Issue_Respected", 1f, needsMeme: true)]);

        Assert.Equal(["Issue_Exalted", "Issue_Respected"], guess.Order);
        Assert.Equal(1.5f, guess.NeutralRank);
    }

    [Fact]
    public void DefaultRungOpenToAnyFaith_LeavesNeutralToTheDefaultsRule()
    {
        var guess = LadderHeuristic.Classify(
            [Rung("Issue_DontCare", 1f), Rung("Issue_Abhorrent", 0.5f, needsMeme: true)]);

        Assert.Null(guess.NeutralRank);
    }

    [Fact]
    public void CountLadder_OrdersByLimit_AndLeavesNeutralToTheDefaults()
    {
        var guess = LadderHeuristic.Classify(
        [
            Rung("LoverCount_Male_Unlimited"), Rung("LoverCount_Male_MaxThree"), Rung("LoverCount_Male_None", 1f),
            Rung("LoverCount_Male_MaxOne"), Rung("LoverCount_Male_MaxTwo"),
        ]);

        Assert.True(guess.IsConfident);
        Assert.Equal(
            ["LoverCount_Male_None", "LoverCount_Male_MaxOne", "LoverCount_Male_MaxTwo", "LoverCount_Male_MaxThree", "LoverCount_Male_Unlimited"],
            guess.Order);
        Assert.Null(guess.NeutralRank);
    }

    [Fact]
    public void CountLadder_WithoutAnOpenDefaultRung_IsFlagged()
    {
        var guess = LadderHeuristic.Classify(
            [Rung("Issue_MaxOne", 1f, needsMeme: true), Rung("Issue_MaxTwo"), Rung("Issue_Unlimited")]);

        Assert.False(guess.IsConfident);
    }

    [Fact]
    public void CountLadder_WithARepeatedLimit_IsFlagged()
    {
        var guess = LadderHeuristic.Classify([Rung("A_Issue_MaxTwo", 1f), Rung("B_Issue_MaxTwo"), Rung("Issue_None")]);

        Assert.Contains(guess.Flags, flag => flag.StartsWith("same count"));
    }

    [Fact]
    public void CountWordsMixedWithStanceWords_AreFlagged()
    {
        var guess = LadderHeuristic.Classify([Rung("Issue_MaxOne", 1f), Rung("Issue_Abhorrent")]);

        Assert.False(guess.IsConfident);
    }

    private static Dictionary<string, List<FixtureRung>> LoadFixture()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "ladders.json");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<Dictionary<string, List<FixtureRung>>>(File.ReadAllText(path), options)!;
    }

    [Fact]
    public void HeuristicIsNeverConfidentlyWrongOnCuratedIssues()
    {
        var fixture = LoadFixture();
        var issues = new Dictionary<string, IssueDef>();
        foreach (var (issueName, rungs) in fixture)
        {
            var issue = new IssueDef { defName = issueName };
            issues[issueName] = issue;
            DefDatabase<IssueDef>.Add(issue);
            foreach (var rung in rungs)
            {
                DefDatabase<PreceptDef>.Add(new PreceptDef
                {
                    defName = rung.DefName,
                    issue = issue,
                    displayOrderInIssue = rung.DisplayOrderInIssue,
                    defaultSelectionWeight = rung.DefaultSelectionWeight,
                });
            }
        }

        var agree = new List<string>();
        var wrong = new List<string>();
        var flaggedCurated = new List<string>();
        var newCoverage = new List<string>();
        var flaggedNew = new List<string>();

        foreach (var (issueName, rungs) in fixture)
        {
            var guess = LadderHeuristic.Classify(rungs
                .Select(rung => new HeuristicRung(rung.DefName, rung.DefaultSelectionWeight, rung.HasEffects, rung.NeedsMeme))
                .ToList());
            var mods = string.Join(", ", rungs.Select(rung => rung.Mod).Distinct());
            var line = $"{issueName} ({mods})";

            if (!PreceptPolicy.IsCurated(issueName))
            {
                if (guess.IsConfident)
                    newCoverage.Add($"{line}: {Describe(guess.Order, NeutralOf(guess, rungs))}");
                else
                    flaggedNew.Add($"{line}: {string.Join("; ", guess.Flags)}");
                continue;
            }

            if (!guess.IsConfident)
            {
                flaggedCurated.Add($"{line}: {string.Join("; ", guess.Flags)}");
                continue;
            }

            var mismatch = Compare(issues[issueName], guess, rungs);
            (mismatch == null ? agree : wrong).Add(mismatch == null ? line : $"{line}: {mismatch}");
        }

        var report = new StringBuilder();
        Section(report, "Curated, heuristic agrees", agree);
        Section(report, "Curated, heuristic confidently WRONG", wrong);
        Section(report, "Curated, heuristic flags", flaggedCurated);
        Section(report, "Not curated, heuristic would classify", newCoverage);
        Section(report, "Not curated, heuristic flags", flaggedNew);
        output.WriteLine(report.ToString());

        Assert.True(wrong.Count == 0, report.ToString());
    }

    // Null when the guess matches the tables, else what differs.
    private static string? Compare(IssueDef issue, LadderGuess guess, List<FixtureRung> rungs)
    {
        // A copy of a loaded issue is PositiveOnly, whatever its own ladder says. The heuristic cannot see that
        // relation, so only its order and neutral are compared.
        var category = PreceptPolicy.CategoryOf(issue);
        if (category != PreceptCategory.Moral && !PreceptPolicy.IsDuplicate(issue.defName))
            return $"tables say {category}";

        var curated = PreceptLadder.Rungs(issue).Select(precept => precept.defName).ToList();
        var mirrored = guess.Order.Reverse().ToList();
        var guessRank = NeutralOf(guess, rungs);
        if (mirrored.SequenceEqual(curated) && !guess.Order.SequenceEqual(curated))
            guessRank = curated.Count - 1 - guessRank;
        else if (!guess.Order.SequenceEqual(curated))
            return $"order: tables {Describe(curated, PreceptLadder.DontCareRank(issue))}, heuristic {Describe(guess.Order, guessRank)}";

        var curatedRank = PreceptLadder.DontCareRank(issue);
        return Gap(curatedRank, curated.Count) == Gap(guessRank, curated.Count)
            ? null
            : $"neutral: tables {Describe(curated, curatedRank)}, heuristic {Describe(curated, guessRank)}";
    }

    // The neutral rank the guess implies: its own placement, else the default-rung rule on the guessed order.
    private static float NeutralOf(LadderGuess guess, List<FixtureRung> rungs)
    {
        if (guess.NeutralRank is { } rank)
            return rank;
        var defaults = guess.Order
            .Select((name, ix) => ((float)ix, rungs.First(rung => rung.DefName == name).DefaultSelectionWeight))
            .Where(entry => entry.DefaultSelectionWeight > 0f)
            .ToList();
        return PreceptLadder.NeutralOfDefaults(defaults)!.Value;
    }

    private static string Gap(float rank, int count)
    {
        if (rank < 0f) return "before";
        if (rank > count - 1) return "after";
        var nearest = MathF.Round(rank);
        return MathF.Abs(rank - nearest) < 0.01f ? $"at {nearest}" : $"between {MathF.Floor(rank)} and {MathF.Ceiling(rank)}";
    }

    private static string Describe(IEnumerable<string> order, float neutral)
    {
        var names = order.ToList();
        var parts = new List<string>();
        for (var ii = 0; ii < names.Count; ii++)
        {
            if (neutral < ii && (ii == 0 || neutral > ii - 1))
                parts.Add("— neutral —");
            parts.Add(MathF.Abs(neutral - ii) < 0.01f ? $"[{names[ii]}]" : names[ii]);
        }
        if (neutral > names.Count - 1)
            parts.Add("— neutral —");
        return string.Join(" → ", parts);
    }

    private static void Section(StringBuilder report, string title, List<string> lines)
    {
        report.AppendLine($"## {title} ({lines.Count})");
        foreach (var line in lines.OrderBy(line => line))
            report.AppendLine($"- {line}");
        report.AppendLine();
    }
}
