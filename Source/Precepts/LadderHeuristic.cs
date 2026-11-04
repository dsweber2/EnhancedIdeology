namespace EnhancedIdeology;

// One rung as the ladder heuristic sees it. HasEffects is true when the def changes gameplay through XML (comps,
// stat modifiers, needs, opposed work types); effects that a mod adds in code are not visible here.
internal readonly record struct HeuristicRung(
    string DefName, float DefaultWeight, bool HasEffects, bool NeedsMeme);

// The heuristic's reading of an issue the PreceptPolicy tables do not list (docs/ladder-heuristic.md). Order runs pro -> anti. NeutralRank is null when a faith with any memes can take a default rung, so
// PreceptLadder.DontCareRank applies its rule for default rungs. Any flag means the heuristic is not sure, and the
// issue stays PositiveOnly.
internal sealed record LadderGuess(IReadOnlyList<string> Order, float? NeutralRank, IReadOnlyList<string> Flags)
{
    public bool IsConfident => Flags.Count == 0;
}

// Classifies a multi-rung issue from the stance word that ends each rung's defName. Each word has a signed
// intensity: positive approves, negative objects, 0 is the plain baseline. A ladder where every rung has a
// distinct intensity is Moral, ordered by intensity, with neutral placed by the Don't-care placement rule.
internal static class LadderHeuristic
{
    private static readonly Dictionary<string, int> Intensity = new()
    {
        ["Essential"] = 3, ["Exalted"] = 3, ["Sacred"] = 3, ["Required"] = 3, ["Demanded"] = 3, ["Central"] = 3,
        ["Venerated"] = 3, ["Sublime"] = 3, ["Holy"] = 3,
        ["Desired"] = 2, ["Respected"] = 2, ["Honorable"] = 2, ["Honourable"] = 2, ["Loved"] = 2, ["Important"] = 2,
        ["Revered"] = 2, ["Elevated"] = 2, ["Idealized"] = 2, ["Beautiful"] = 2, ["Blessed"] = 2, ["Amazing"] = 2, ["Craved"] = 2,
        ["Preferred"] = 1, ["Approved"] = 1, ["Encouraged"] = 1, ["Wanted"] = 1, ["Accepted"] = 1, ["Liked"] = 1, ["Welcomed"] = 1, ["Valued"] = 1,
        ["DontCare"] = 0, ["Neutral"] = 0, ["Indifferent"] = 0, ["Acceptable"] = 0, ["Allowed"] = 0, ["Normal"] = 0, ["DontMind"] = 0,
        ["Disliked"] = -1, ["Discouraged"] = -1, ["Wary"] = -1, ["Disapproved"] = -1, ["Disrespected"] = -1, ["Troubling"] = -1, ["Ugly"] = -1,
        ["Horrible"] = -2, ["Despised"] = -2, ["Reviled"] = -2, ["Shunned"] = -2, ["Disgusting"] = -2,
        ["Dishonorable"] = -2, ["Shameful"] = -2, ["Scorned"] = -2, ["Taboo"] = -2, ["Hated"] = -2, ["Condemned"] = -2,
        ["Abhorrent"] = -3, ["Forbidden"] = -3, ["Prohibited"] = -3, ["Despicable"] = -3, ["Abhorred"] = -3, ["Depraved"] = -3, ["Abomination"] = -3,
        ["Hatred"] = -3, ["Sacrilegious"] = -3,
    };

    // Stat-level words. They name a perk, not a value claim, so a person reviews each issue that uses them.
    private static readonly string[] Mechanical =
    [
        "Increased", "Decreased", "Fast", "Faster", "Slow", "Slower", "Efficient", "Inefficient", "Improved",
        "Reduced", "Lowered", "Heightened", "High", "Low", "Doubled", "Halved", "Enhanced", "Speed", "Yield",
    ];

    // A word before the stance word that reverses it ("NotDesired").
    private static readonly string[] Negations = ["Not", "Non", "No", "Anti"];

    public static LadderGuess Classify(IReadOnlyList<HeuristicRung> rungs)
    {
        var flags = new List<string>();
        if (rungs.Count < 2)
            flags.Add("single rung");

        var scored = new List<(HeuristicRung rung, int intensity)>();
        foreach (var rung in rungs)
        {
            var word = StanceWord(rung.DefName);
            if (word == null)
                flags.Add($"unknown word: {rung.DefName}");
            else if (Mechanical.Contains(word))
                flags.Add($"mechanical: {rung.DefName}");
            else
                scored.Add((rung, Intensity[word]));
        }
        if (flags.Count > 0)
            return new LadderGuess([], null, flags);

        var order = scored.OrderByDescending(entry => entry.intensity).ToList();
        foreach (var group in order.GroupBy(entry => entry.intensity).Where(group => group.Count() > 1))
            flags.Add($"same intensity: {string.Join(", ", group.Select(entry => entry.rung.DefName))}");

        var neutral = NeutralRank(order, flags);
        return new LadderGuess(order.Select(entry => entry.rung.DefName).ToList(), neutral, flags);
    }

    // The word that ends the last defName segment, matched on a capital letter boundary, or null.
    private static string? StanceWord(string defName)
    {
        var segment = defName.Substring(defName.LastIndexOf('_') + 1);
        var word = Intensity.Keys.Concat(Mechanical)
            .Where(candidate => segment.EndsWith(candidate, StringComparison.Ordinal))
            .OrderByDescending(candidate => candidate.Length)
            .FirstOrDefault();
        if (word == null)
            return null;
        var qualifier = segment.Substring(0, segment.Length - word.Length);
        return Negations.Any(negation => qualifier.EndsWith(negation, StringComparison.Ordinal)) ? null : word;
    }

    private static float? NeutralRank(List<(HeuristicRung rung, int intensity)> order, List<string> flags)
    {
        // Default rungs that all need a meme are not available to most faiths, so they say nothing about neutral.
        if (order.Any(entry => entry.rung.DefaultWeight > 0f && !entry.rung.NeedsMeme))
            return null;

        var baseline = order.FindIndex(entry => entry.intensity == 0);
        if (baseline >= 0)
        {
            if (order[baseline].rung.HasEffects)
                flags.Add($"baseline word on a rung with effects: {order[baseline].rung.DefName}");
            return baseline;
        }

        var lastPro = order.FindLastIndex(entry => entry.intensity > 0);
        var firstAnti = order.FindIndex(entry => entry.intensity < 0);
        if (firstAnti < 0)
            return lastPro + 0.5f;
        if (lastPro < 0)
            return firstAnti - 0.5f;
        return (lastPro + firstAnti) / 2f;
    }
}
