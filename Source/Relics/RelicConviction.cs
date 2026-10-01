namespace EnhancedIdeology;

// Relic events move conviction on every Moral issue of the relic's ideo, not on a single issue.
// A relic is a token of the whole faith, so it does not belong to one belief axis.
internal static class RelicConviction
{
    // Arc length of the find boost. Equal to a positivityIndex=2 belief-reinforcement ritual.
    internal const float RelicFoundArc = 2f * ConvictionMath.RitualBaseArc;

    internal static bool IsRelicThought(ThoughtDef def) =>
        def == ThoughtDefOf.RelicLost
        || def == ThoughtDefOf.RelicDestroyed
        || def == ThoughtDefOf.RelicsCollected
        || def == ThoughtDefOf.RelicAtRitual;

    // Vanilla gives relic thoughts with no source precept. The relic precept of the pawn's ideo is used,
    // so that the practice band counts the thought and the thought stays tied to that ideo after conversion.
    // Vanilla shows only the precept def label ("relic") in the tooltip, so which relic is not visible.
    internal static Precept_Relic? SourceFor(Pawn pawn) =>
        pawn.Ideo?.precepts.OfType<Precept_Relic>().FirstOrDefault();

    internal static List<IssueDef> MoralIssues(Ideo ideo) =>
        ideo.precepts
            .Select(precept => precept.def.issue)
            .Where(issue => issue != null && PreceptPolicy.CategoryOf(issue) == PreceptCategory.Moral)
            .Distinct()
            .ToList()!;

    // Each living follower of the relic's ideo moves toward orthodoxy and higher conviction on all Moral issues.
    internal static void ApplyFindBoost(GameComponent_EnhancedIdeology comp, Precept_Relic relic)
    {
        var ideo = relic.ideo;
        var issues = MoralIssues(ideo);
        foreach (var pawn in comp.GetIdeoPawns(ideo).ToList())
        {
            if (pawn.Dead || pawn.Ideo != ideo || !pawn.RaceProps.Humanlike)
            {
                continue;
            }

            var stepLength = RelicFoundArc * pawn.GetStatValue(StatDefOf.CertaintyLossFactor);
            foreach (var issue in issues)
            {
                ConvictionMath.ApplyRitualPull(comp, pawn, issue, IssueStanceTracker.HeldRank(ideo, issue),
                    ConvictionScale.AbsoluteMaxConvictionStrength, stepLength);
            }
        }
    }

    // Per-TickLong conviction shift from one relic thought. The rate is the same as a precept moodlet of
    // equal mood, divided across the Moral issues so that the total effect on structural fit is also equal.
    internal static void ApplyMoodletShift(Pawn pawn, IdeoTrackerData tracker, Thought_Memory thought)
    {
        if (thought.sourcePrecept?.ideo is not { } ideo || ideo != pawn.Ideo)
        {
            return;
        }

        var issues = MoralIssues(ideo);
        if (issues.Count == 0)
        {
            return;
        }

        var delta = thought.MoodOffset()
            * pawn.GetStatValue(StatDefOf.CertaintyLossFactor)
            * EnhancedIdeologyMod.Settings.ConversionStancePull
            * GameComponent_EnhancedIdeology.MoodletConvictionScalar
            / issues.Count;
        if (Mathf.Abs(delta) < 0.00001f)
        {
            return;
        }

        foreach (var issue in issues)
        {
            tracker.ShiftIssueStance(issue, 0f, 0f, delta);
        }
    }
}
