namespace EnhancedIdeology;

// Connects LadderHeuristic to the live defs, for issues that no PreceptPolicy table lists. See
// docs/ladder-heuristic.md.
internal static partial class PreceptPolicy
{
    private static readonly Dictionary<IssueDef, LadderGuess?> GuessCache = [];

    // The heuristic's reading of an issue, or null when a table or a test registration decides the issue.
    internal static LadderGuess? GuessFor(IssueDef issue)
    {
        if (!GuessCache.TryGetValue(issue, out var guess))
        {
            guess = IsCurated(issue.defName) || Overrides.ContainsKey(issue.defName)
                ? null
                : LadderHeuristic.Classify(DefDatabase<PreceptDef>.AllDefs
                    .Where(precept => precept.issue == issue && precept.visible && !TiedRungs.ContainsKey(precept.defName))
                    .Select(HeuristicRungOf)
                    .ToList());
            GuessCache[issue] = guess;
        }
        return guess;
    }

    internal static LadderGuess? ConfidentGuess(IssueDef issue) =>
        GuessFor(issue) is { IsConfident: true } guess ? guess : null;

    // These fields must stay the same as EFFECT_FIELDS in scripts/meme_precept_catalog.py, which writes the backtest
    // fixture.
    internal static HeuristicRung HeuristicRungOf(PreceptDef precept) => new(
        precept.defName,
        precept.defaultSelectionWeight,
        !precept.comps.NullOrEmpty() || !precept.abilityStatFactors.NullOrEmpty() || !precept.statOffsets.NullOrEmpty()
            || !precept.statFactors.NullOrEmpty() || !precept.conditionalStatAffecters.NullOrEmpty()
            || !precept.opposedWorkTypes.NullOrEmpty() || !precept.enablesNeeds.NullOrEmpty()
            || !precept.disablesNeeds.NullOrEmpty(),
        !precept.requiredMemes.NullOrEmpty());

    // One block per issue that the heuristic classified, in the "Log issue ladders" format. Empty when there are none.
    internal static string DescribeAutoClassified() =>
        string.Join("\n", DefDatabase<IssueDef>.AllDefs
            .Where(issue => ConfidentGuess(issue) != null)
            .OrderBy(issue => issue.defName)
            .Select(issue => $"{issue.defName} (from {issue.modContentPack?.Name ?? "no mod"}), "
                + $"neutral rank {PreceptLadder.DontCareRank(issue):0.##}\n{DescribeRungs(issue)}"));

    // What the heuristic decided on an issue, for logs. Empty when a table decides the issue.
    internal static string DescribeGuess(IssueDef issue) => GuessFor(issue) switch
    {
        null => "",
        { IsConfident: true } => "classified by rung names",
        var guess => $"not classified by rung names: {string.Join("; ", guess.Flags)}",
    };
}
