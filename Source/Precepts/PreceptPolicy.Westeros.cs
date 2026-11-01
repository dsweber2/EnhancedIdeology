namespace EnhancedIdeology;

// A Rim of Ice and Fire. A pawn whose faith has a Westerosi structure is judged only on the issues that the
// Westerosi faiths take a position on. Other issues do not change the structural opinion, the debates, or the
// trait targets of that pawn. The vanilla precepts on those issues keep their usual effects.
// (docs/preceptPolicy.md "A Rim of Ice and Fire")
internal static partial class PreceptPolicy
{
    private const string WesterosiStructurePrefix = "ARIOIAF_Structure_";
    private const string WesterosiIssuePrefix = "ARIOIAF_";

    // Issues from other sources that count for a Westerosi pawn, in addition to every ARIOIAF_ issue.
    private static readonly HashSet<string> WesterosiIssues =
    [
        // Forced by a ROIAF meme (requireOne) or by its doctrine migration.
        "Execution", "Charity", "Lovin", "Apostasy", "Slavery", "IdeoDiversity", "Proselytizing", "Corpses",
        "Raiding", "Cannibalism", "Skullspike", "RoughLiving", "Temperature", "FungusEating",
        // Suggested by a ROIAF meme (associatedMemes) or unlocked by one.
        "MarriageName", "Comfort", "NutrientPasteEating", "Fishing",
        // Left to vanilla generation, but they vary between Westerosi faiths.
        "BS_AlienAppearanceTolerance", "AnimalsVenerated",
        // The VME issues that the ROIAF copies induce (DuplicateOf).
        "VME_Death", "VME_Expectations", "VME_Royalty",
    ];

    internal static bool IsWesterosi(Ideo? ideo) =>
        ideo != null && ideo.memes.Any(meme => meme.defName.StartsWith(WesterosiStructurePrefix, StringComparison.Ordinal));

    internal static bool CountsInWesteros(IssueDef issue) =>
        issue.defName.StartsWith(WesterosiIssuePrefix, StringComparison.Ordinal) || WesterosiIssues.Contains(issue.defName);

    // True when `issue` does not count for a pawn of `faith`.
    internal static bool IsMuted(Ideo? faith, IssueDef issue) => IsWesterosi(faith) && !CountsInWesteros(issue);
}
