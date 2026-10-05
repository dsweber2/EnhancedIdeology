namespace EnhancedIdeology;

// Reforming an ideo moves its held rungs. A believer who was orthodox on an issue follows the faith to the
// new rung, at half their old conviction. Heterodox believers keep their stance.
// A schism is the same move for the pawns who split off: the old ideo's rungs become the new ideo's.
internal static class IdeoReform
{
    internal const float CarriedStrengthFactor = 0.5f;

    // Held rank of every stance-bearing issue. Call before the reform overwrites the ideo.
    internal static Dictionary<IssueDef, float> HeldRanks(Ideo ideo) =>
        DefDatabase<IssueDef>.AllDefs
            .Where(issue => PreceptPolicy.CategoryOf(issue) != PreceptCategory.NA)
            .ToDictionary(issue => issue, issue => IssueStanceTracker.HeldRank(ideo, issue));

    internal static void CarryOrthodoxStances(GameComponent_EnhancedIdeology comp, Ideo ideo, Dictionary<IssueDef, float> oldHeldRanks)
    {
        foreach (var pawn in comp.GetIdeoPawns(ideo).ToList())
        {
            if (pawn.Dead || pawn.Ideo != ideo || !pawn.RaceProps.Humanlike)
            {
                continue;
            }

            CarryOrthodoxStances(comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn), ideo, oldHeldRanks);
        }
    }

    internal static void CarryOrthodoxStances(IdeoTrackerData tracker, Ideo newIdeo, Dictionary<IssueDef, float> oldHeldRanks)
    {
        var epsilon = InteractionWorker_IdeologicalDebatePrecept.DebateRankEpsilon;
        foreach (var (issue, rank, strength) in tracker.IssueStances().ToList())
        {
            if (!oldHeldRanks.TryGetValue(issue, out var oldRank))
            {
                continue;
            }

            var newRank = IssueStanceTracker.HeldRank(newIdeo, issue);
            if (Mathf.Abs(newRank - oldRank) <= epsilon || Mathf.Abs(rank - oldRank) > epsilon)
            {
                continue;
            }

            tracker.SetIssueStance(issue, newRank, strength * CarriedStrengthFactor);
        }
    }
}
