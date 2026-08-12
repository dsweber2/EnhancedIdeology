namespace EnhancedIdeology;

// The moral-guide Convert ability, reworked (design.md R3) into a player-directed bundle of normal-strength
// conversions resolved on a single debate roll: mostly a high-skill pawn firing several ordinary conversions at
// once. Kept free of Harmony/ability types so it compiles into the simulator; the gizmo wiring (messages, sound,
// tooltip) lives in the CompAbilityEffect_Convert Harmony patch.
[HotSwappable]
internal static class AbilityConversion
{
    // Widest bundle the ability targets. Each cast picks Uniform{1..this}, capped at the number of issues the
    // recipient actually opposes about the guide's faith.
    internal const int MaxBundleIssues = 4;

    // How many of the recipient's most-opposed issues this cast will target: Uniform{1..MaxBundleIssues},
    // capped at the number actually opposed. Rolled before Resolve so the tooltip can preview the same count.
    internal static IReadOnlyList<IssueDef> RollTargetIssues(IdeoTrackerData recipientTracker, Ideo guideIdeo, IdeoTrackerData guideTracker) =>
        recipientTracker.MostOpposingIssues(guideIdeo, Rand.RangeInclusive(1, MaxBundleIssues), guideTracker);

    // Resolve one cast against a pre-rolled issue bundle. Returns true iff the recipient converted.
    // Win: every targeted issue slides toward the guide's rung at ordinary (1x) debate strength - wide but
    // shallow, unlike conversion's single-issue 2x - one certainty knock, then one conversion check.
    // Draw/loss: no conversion. On a loss only the guide's own stance on the single top issue is pulled toward
    // the recipient, so the guide doesn't hemorrhage belief across the whole bundle.
    internal static bool Resolve(Pawn guide, Pawn recipient, IReadOnlyList<IssueDef> issues)
    {
        if (issues.Count == 0)
        {
            return false;
        }

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var guideIdeo = guide.Ideo;
        var recipientTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);

        var guideRoll = InteractionWorker_IdeologicalDebatePrecept.GetDebateRoll(guide);
        var recipientRoll = InteractionWorker_IdeologicalDebatePrecept.GetDebateRoll(recipient);
        if (Math.Abs(guideRoll - recipientRoll) <= InteractionWorker_IdeologicalDebatePrecept.DebateDrawThreshold)
        {
            return false;
        }

        if (guideRoll > recipientRoll)
        {
            foreach (var issue in issues)
            {
                var guideRank = PreceptLadder.RankOf(guideIdeo.precepts.Select(precept => precept.def).First(def => def.issue == issue));
                ConvictionMath.PullStance(comp, guide, recipient, issue, guideRank, 1f);
            }

            recipient.ideo.Certainty *= EnhancedIdeologyMod.Settings.ConversionCertaintyKnock;
            return recipientTracker.CheckConversion(guideIdeo) == ConversionOutcome.Success;
        }

        var topIssue = issues[0];
        var recipientRank = recipientTracker.IssueStances().First(stance => stance.issue == topIssue).rank;
        ConvictionMath.PullStance(comp, recipient, guide, topIssue, recipientRank, 1f);
        return false;
    }
}
