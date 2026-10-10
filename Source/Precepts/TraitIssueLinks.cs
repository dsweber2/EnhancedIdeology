namespace EnhancedIdeology;

// Links from a trait straight to one rung of an issue, or against a universally-valued issue such as Charity
// (docs/trait-meme-affinity.md "Traits with no meme affinity"). XML patches add it to TraitDefs.
public class TraitIssueLinks : DefModExtension
{
    public List<TraitIssueLink> links = [];

    // Links that apply to the pawn's traits at their current degrees.
    internal static IEnumerable<TraitIssueLink> ActiveFor(Pawn pawn)
    {
        foreach (var trait in pawn.story.traits.allTraits)
        {
            var extension = trait.def.GetModExtension<TraitIssueLinks>();
            if (extension == null) continue;
            foreach (var link in extension.links)
                if (link.degree == null || link.degree == trait.Degree)
                    yield return link;
        }
    }

    // True when one of the pawn's traits turns them against `issue`.
    internal static bool Opposes(Pawn pawn, IssueDef issue) =>
        ActiveFor(pawn).Any(link => link.opposes == issue.defName);
}

public class TraitIssueLink
{
    // Trait degree the link applies to; null applies to every degree.
    public int? degree;
    // Full defName of the rung the trait favours. A name that does not resolve (mod not loaded) is skipped.
    public string? rung;
    // defName of a UniversalPositive issue the trait opposes.
    public string? opposes;

    internal PreceptDef? Rung => rung == null ? null : DefDatabase<PreceptDef>.GetNamedSilentFail(rung);
}
