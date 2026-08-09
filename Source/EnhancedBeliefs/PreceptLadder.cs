namespace EnhancedBeliefs;

// Per-issue precept "ladder" and the distance-based opinion falloff (design.md R2).
//
// An issue (IssueDef) is a ladder of mutually-exclusive stance precepts ordered by
// PreceptDef.displayOrderInIssue. Rank 0 = lowest order = most permissive/approving; the top rank is
// the most forbidding. A pawn stores one preferred rung + strength per issue, and their opinion of any
// other rung on that issue is derived from rung distance rather than stored per precept.
internal static class PreceptLadder
{
    // Stance rungs of an issue, permissive/pro -> forbidding/anti. This is the canonical filter vanilla's own
    // RandomizePrecepts uses (issue equality); reaction thoughts are ThoughtDefs, not PreceptDefs, so they
    // never pollute the ladder. Classic-mode default precepts (Lovin_Free, Cannibalism_Classic, ...) carry an
    // issue but are the no-ideology fallback, duplicating a real rung; they are excluded so they do not add a
    // phantom rung that shifts every real rank and skews the opinion falloff. For issues whose
    // displayOrderInIssue scrambles the axis once stacked, a PreceptPolicy order override pins the sequence.
    public static List<PreceptDef> Rungs(IssueDef issue)
    {
        var rungs = DefDatabase<PreceptDef>.AllDefs.Where(precept => precept.issue == issue && !precept.classic);

        if (PreceptPolicy.OrderOverrides.TryGetValue(issue.defName, out var order))
        {
            return [.. rungs.OrderBy(precept =>
            {
                var ix = Array.IndexOf(order, precept.defName);
                return ix >= 0 ? ix : order.Length + precept.displayOrderInIssue;
            })];
        }

        return [.. rungs.OrderBy(precept => precept.displayOrderInIssue)];
    }

    // Rank of a held stance within its issue ladder (index in the ordered rungs).
    public static float RankOf(PreceptDef precept) => Rungs(precept.issue!).IndexOf(precept);

    // Rank of a rung by defName within its issue's (reordered) ladder, or -1f if that rung is absent (mod
    // not loaded). Used by DontCareSpec to resolve neighbour-keyed placements against the live ladder.
    public static float RankOfName(IssueDef issue, string defName) =>
        Rungs(issue).FindIndex(precept => precept.defName == defName);

    // Rank the virtual "Don't care" rung sits at for an issue whose ideo holds no explicit stance.
    // -1f is the permissive extreme, one step below the most permissive explicit rung.
    public static float DontCareRank(IssueDef issue) =>
        PreceptPolicy.DontCare.TryGetValue(issue.defName, out var spec) ? spec.Resolve(issue) : -1f;

    // Opinion of targetRank given the pawn prefers preferredRank with the given strength, over a ladder
    // spanning [minRank, maxRank] (extent includes the virtual Don't-care rung when the issue is absent-able).
    // Falloff is linear in rung distance: +strength at the preferred rung, falling to
    // -oppositionScale·strength at the FARTHER ladder end (t = 1), crossing zero at 1/(1+oppositionScale) of
    // the way out. The nearer extreme comes out softer, scaled by its shorter distance - it is closer to the
    // pawn's view. oppositionScale (0-1) sets how strongly the far extreme is opposed: 0 fades to mere
    // indifference there, 1 is full opposition. (design.md R2)
    public static float OpinionOnPrecept(
        float preferredRank, float targetRank, float minRank, float maxRank, float strength, float oppositionScale)
    {
        var maxDist = Mathf.Max(preferredRank - minRank, maxRank - preferredRank);
        if (maxDist <= 0f)
        {
            // Single-rung issue (or preferred sits alone): no spread, holding it is pure agreement.
            return strength;
        }

        var t = Mathf.Abs(targetRank - preferredRank) / maxDist;
        return strength * (1f - (t * (1f + oppositionScale)));
    }
}
