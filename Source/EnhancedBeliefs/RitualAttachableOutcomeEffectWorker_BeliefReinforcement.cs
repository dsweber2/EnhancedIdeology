namespace EnhancedBeliefs;

// Ritual side-effect: all participants' per-issue stances are nudged toward their ideo's orthodox rank and
// toward maximum conviction (design.md R3). Quality scales the effect: positive outcomes harden belief,
// negative outcomes erode it (moving toward heterodoxy and doubt). The step uses the same conviction-valley
// arc as debates, so conviction craters through the muddled middle before recovering at the new rung.
//
// Meme boost: if the ritual's def lists requiredMemeAny, precepts whose associatedMemes overlaps that list
// receive a 2x step - the ritual is directly about those beliefs.
internal sealed class RitualAttachableOutcomeEffectWorker_BeliefReinforcement : RitualAttachableOutcomeEffectWorker
{
    public override void Apply(
        Dictionary<Pawn, int> totalPresence,
        LordJob_Ritual jobRitual,
        RitualOutcomePossibility outcome,
        out string? extraOutcomeDesc,
        ref LookTargets letterLookTargets)
    {
        extraOutcomeDesc = null;
        if (outcome.positivityIndex == 0)
        {
            return;
        }

        var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
        var gateMemes = def.requiredMemeAny;

        foreach (var pawn in totalPresence.Keys)
        {
            if (pawn.Ideo == null || !pawn.RaceProps.Humanlike)
            {
                continue;
            }

            var baseStep = ConvictionMath.RitualBaseArc
                * Math.Abs(outcome.positivityIndex)
                * pawn.GetStatValue(StatDefOf.CertaintyLossFactor);

            foreach (var precept in pawn.Ideo.precepts)
            {
                var issue = precept.def.issue;
                if (issue == null || PreceptPolicy.CategoryOf(issue) != PreceptCategory.Moral)
                {
                    continue;
                }

                var boost = !gateMemes.NullOrEmpty() && precept.def.associatedMemes.Any(m => gateMemes.Contains(m)) ? 2f : 1f;
                var stepLength = baseStep * boost;

                float targetRank, targetStrength;
                if (outcome.positivityIndex > 0)
                {
                    targetRank = IdeoTrackerData.HeldRank(pawn.Ideo, issue);
                    targetStrength = IdeoTrackerData.AbsoluteMaxConvictionStrength;
                }
                else
                {
                    targetRank = ConvictionMath.LadderExtremeAwayFrom(issue, IdeoTrackerData.HeldRank(pawn.Ideo, issue));
                    targetStrength = 0f;
                }

                ConvictionMath.ApplyRitualPull(comp, pawn, issue, targetRank, targetStrength, stepLength);
            }
        }
    }
}
