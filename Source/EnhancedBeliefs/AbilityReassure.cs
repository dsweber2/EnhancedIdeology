namespace EnhancedBeliefs;

// The moral-guide Reassure ability, reworked (design.md R3) as conversion's same-ideo mirror: pulls the
// target's stances toward their own ideo's orthodox rungs (hardening belief) and nudges certainty up on a
// won debate roll. Self-targeting (self-reassurance) bypasses the roll and always succeeds.
[HotSwappable]
internal static class AbilityReassure
{
    internal const int MaxBundleIssues = 4;

    // Flat certainty gain on a win. TODO: pin this magnitude.
    internal const float ReassureCertaintyGain = 0.05f;

    // How many of the target's most-divergent issues this cast will target: Uniform{1..MaxBundleIssues},
    // capped at the number of heterodox issues the target actually holds.
    internal static IReadOnlyList<IssueDef> RollTargetIssues(IdeoTrackerData recipientTracker) =>
        recipientTracker.MostHeterodoxIssues(Rand.RangeInclusive(1, MaxBundleIssues));

    // Resolve one cast against a pre-rolled issue bundle. Returns true on a win.
    // Win: each targeted issue slides toward the target's own ideo's rung at ordinary (1x) debate strength,
    // then certainty nudges up by ReassureCertaintyGain. Self-reassurance always wins.
    // Loss: no effect.
    internal static bool Resolve(Pawn guide, Pawn recipient, IReadOnlyList<IssueDef> issues)
    {
        if (issues.Count == 0)
        {
            return false;
        }

        var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
        var recipientIdeo = recipient.Ideo;

        var won = guide == recipient || IsGuideWinner(guide, recipient);
        if (!won)
        {
            return false;
        }

        foreach (var issue in issues)
        {
            var orthodoxRank = IdeoTrackerData.HeldRank(recipientIdeo, issue);
            InteractionWorker_IdeologicalDebatePrecept.PullStance(comp, guide, recipient, issue, orthodoxRank, 1f);
        }

        recipient.ideo.Certainty = Mathf.Clamp01(recipient.ideo.Certainty + ReassureCertaintyGain);
        return true;
    }

    private static bool IsGuideWinner(Pawn guide, Pawn recipient)
    {
        var guideRoll = InteractionWorker_IdeologicalDebatePrecept.GetDebateRoll(guide);
        var recipientRoll = InteractionWorker_IdeologicalDebatePrecept.GetDebateRoll(recipient);
        return (guideRoll - recipientRoll) > InteractionWorker_IdeologicalDebatePrecept.DebateDrawThreshold;
    }
}
