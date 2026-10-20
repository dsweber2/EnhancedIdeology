namespace EnhancedIdeology;

// Per-issue precept "ladder" and the distance-based opinion falloff (docs/design.md "Opinion of a rung").
//
// An issue (IssueDef) is a ladder of mutually-exclusive stance precepts ordered by
// PreceptDef.displayOrderInIssue. Rank 0 = lowest order = most permissive/approving; the top rank is
// the most forbidding. A pawn stores one preferred rung + strength per issue, and their opinion of any
// other rung on that issue is derived from rung distance rather than stored per precept.
internal static class PreceptLadder
{
    private static readonly Dictionary<IssueDef, List<PreceptDef>> _rungsCache = [];
    private static readonly Dictionary<IssueDef, float> _dontCareCache = [];
    private static readonly Dictionary<PreceptDef, float> _rankCache = [];

    // Stance rungs of an issue, permissive/pro -> forbidding/anti. This is the canonical filter vanilla's own
    // RandomizePrecepts uses (issue equality); reaction thoughts are ThoughtDefs, not PreceptDefs, so they
    // never pollute the ladder. Invisible precepts (Cannibalism_Classic, Execution_Classic, ...) are the
    // classic-mode fallback that no ideo can select; they are excluded so they do not add a phantom rung that
    // shifts every real rank. Visible classic precepts (Lovin_Free, SpouseCount_Male_MaxOne, ...) are ordinary
    // rungs: "classic" only means the precept exists without the Ideology DLC. For issues whose
    // displayOrderInIssue scrambles the axis once stacked, a PreceptPolicy order override pins the sequence.
    // Rungs in PreceptPolicy.TiedRungs are left out: they share their anchor's rank, so the ladder holds one
    // rung per rank.
    public static List<PreceptDef> Rungs(IssueDef issue)
    {
        if (_rungsCache.TryGetValue(issue, out var cached))
            return cached;

        var rungs = DefDatabase<PreceptDef>.AllDefs.Where(precept => precept.issue == issue
            && precept.visible
            && !PreceptPolicy.TiedRungs.ContainsKey(precept.defName));
        List<PreceptDef> result;

        if (PreceptPolicy.OrderOverrides.TryGetValue(issue.defName, out var order))
        {
            result = [.. rungs.OrderBy(precept =>
            {
                var ix = Array.IndexOf(order, precept.defName);
                return ix >= 0 ? ix : order.Length + precept.displayOrderInIssue;
            })];
        }
        else
        {
            result = [.. rungs.OrderBy(precept => precept.displayOrderInIssue)];
        }

        _rungsCache[issue] = result;
        return result;
    }

    // Rank of a held stance within its issue ladder (index in the ordered rungs). Memoized like Rungs, which it
    // derives from: HeldRank calls it for every issue an ideo holds, on the structural opinion hot path.
    public static float RankOf(PreceptDef precept)
    {
        if (_rankCache.TryGetValue(precept, out var cached))
            return cached;

        var rank = PreceptPolicy.TiedRungs.TryGetValue(precept.defName, out var anchor)
            ? RankOfName(precept.issue!, anchor)
            : Rungs(precept.issue!).IndexOf(precept);
        _rankCache[precept] = rank;
        return rank;
    }

    // Rank of a rung by defName within its issue's (reordered) ladder, or -1f if that rung is absent (mod
    // not loaded). Used by DontCareSpec to resolve neighbour-keyed placements against the live ladder.
    public static float RankOfName(IssueDef issue, string defName)
    {
        var target = PreceptPolicy.TiedRungs.GetValueOrDefault(defName, defName);
        return Rungs(issue).FindIndex(precept => precept.defName == target);
    }

    // Rank of the neutral stance on an issue (docs/preceptPolicy.md "Don't-care placement rule"). An issue with
    // default rungs uses their weighted mean rank. Other issues use the PreceptPolicy.DontCare table, else -1f,
    // one step below the most permissive explicit rung.
    public static float DontCareRank(IssueDef issue)
    {
        if (_dontCareCache.TryGetValue(issue, out var cached))
            return cached;

        var rank = DefaultRank(issue)
            ?? (PreceptPolicy.DontCare.TryGetValue(issue.defName, out var spec) ? spec.Resolve(issue) : -1f);
        _dontCareCache[issue] = rank;
        return rank;
    }

    // When an ideo loses its rung on an issue with default rungs, vanilla adds one of them back, picked at random
    // by defaultSelectionWeight (Ideo.RemovePrecept). The neutral stance is the expected rank of that pick.
    private static float? DefaultRank(IssueDef issue)
    {
        var weightSum = 0f;
        var rankSum = 0f;
        foreach (var precept in DefDatabase<PreceptDef>.AllDefs)
        {
            if (precept.issue != issue || !precept.visible || precept.defaultSelectionWeight <= 0f)
                continue;
            weightSum += precept.defaultSelectionWeight;
            rankSum += precept.defaultSelectionWeight * RankOf(precept);
        }
        return weightSum > 0f ? rankSum / weightSum : null;
    }

    // Invisible classic precept for an issue, or null if none exists. Display code uses its label for an ideo with
    // no rung on the issue (classic mode, or an issue added by a mod after the ideo was made).
    public static PreceptDef? ClassicFallback(IssueDef issue) =>
        DefDatabase<PreceptDef>.AllDefs.FirstOrDefault(precept =>
            precept.issue == issue && precept.classic && !precept.visible && !precept.label.NullOrEmpty());

    // Opinion of targetRank given the pawn prefers preferredRank with the given strength, over a ladder
    // spanning [minRank, maxRank] (extent includes the virtual Don't-care rung when the issue is absent-able).
    // Falloff is linear in rung distance: +strength at the preferred rung, falling to
    // -oppositionScale·strength at the FARTHER ladder end (t = 1), crossing zero at 1/(1+oppositionScale) of
    // the way out. The nearer extreme comes out softer, scaled by its shorter distance - it is closer to the
    // pawn's view. oppositionScale (0-1) sets how strongly the far extreme is opposed: 0 fades to mere
    // indifference there, 1 is full opposition. (docs/design.md "Opinion of a rung")
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
