using System.Text;
using Verse.Sound;

namespace EnhancedBeliefs.HarmonyPatches;

// Allow the moral-guide to target themselves (self-reassurance) in addition to same-ideo colonists.
[HarmonyPatch(typeof(CompAbilityEffect_Reassure), nameof(CompAbilityEffect_Reassure.Valid))]
internal static class CompAbilityEffect_Reassure_Valid
{
    private static bool Prefix(CompAbilityEffect_Reassure __instance, LocalTargetInfo target, ref bool __result)
    {
        if (target.Pawn == __instance.parent.pawn)
        {
            __result = true;
            return false;
        }

        return true;
    }
}

// Reroute Reassure off vanilla's flat certainty-top-up onto the R3 orthodoxy-hardening mechanic:
// pulls the target's most-heterodox stances toward their own ideo's rungs on a won debate roll.
[HarmonyPatch(typeof(CompAbilityEffect_Reassure), nameof(CompAbilityEffect_Reassure.Apply))]
internal static class CompAbilityEffect_Reassure_Apply
{
    private static bool Prefix(CompAbilityEffect_Reassure __instance, LocalTargetInfo target)
    {
        if (!ModLister.CheckIdeology("Ideoligion certainty"))
        {
            return false;
        }

        var guide = __instance.parent.pawn;
        var recipient = target.Pawn;
        var props = __instance.Props;
        var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
        var recipientTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var guideTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(guide);

        var issues = AbilityReassure.RollTargetIssues(recipientTracker, guideTracker);
        if (issues.Count == 0)
        {
            Messages.Message("EnhancedBeliefs.Reassure.NoHeterodoxy".Translate(),
                new LookTargets(guide, recipient), MessageTypeDefOf.NeutralEvent);
            props.sound?.PlayOneShot(new TargetInfo(target.Cell, guide.Map));
            return false;
        }

        var certaintyBefore = recipient.ideo.Certainty;
        if (AbilityReassure.Resolve(guide, recipient, issues))
        {
            Messages.Message(props.successMessage.Formatted(
                guide.Named("INITIATOR"), recipient.Named("RECIPIENT"),
                certaintyBefore.ToStringPercent().Named("BEFORECERTAINTY"),
                recipient.ideo.Certainty.ToStringPercent().Named("AFTERCERTAINTY"),
                guide.Ideo.name.Named("IDEO")),
                new LookTargets(guide, recipient), MessageTypeDefOf.PositiveEvent);
            Find.PlayLog.Add(new PlayLogEntry_Interaction(InteractionDefOf.Reassure, guide, recipient, null));
        }
        else
        {
            Messages.Message("EnhancedBeliefs.Reassure.FailMessage".Translate(
                guide.LabelShort, recipient.LabelShort),
                new LookTargets(guide, recipient), MessageTypeDefOf.NeutralEvent);
        }

        props.sound?.PlayOneShot(new TargetInfo(target.Cell, guide.Map));
        return false;
    }
}

// Replace vanilla's tooltip (which references certainty gain from NegotiationAbility) with one describing
// the R3 mechanic: the target beliefs with their current-vs-orthodox rungs, the debate win chance, and the
// stat factors. Mirrors the convert tooltip's structure.
// CompAbilityEffect_Reassure does not override ExtraLabelMouseAttachment, so we patch the base class and
// guard on instance type — only our prefix fires for Reassure; all other ability types pass through.
[HarmonyPatch(typeof(CompAbilityEffect), nameof(CompAbilityEffect.ExtraLabelMouseAttachment))]
internal static class CompAbilityEffect_Reassure_Tooltip
{
    private static bool Prefix(CompAbilityEffect __instance, LocalTargetInfo target, ref string? __result)
    {
        if (__instance is not CompAbilityEffect_Reassure reassureComp)
            return true;

        var recipient = target.Pawn;
        if (recipient == null || !reassureComp.Valid(target))
        {
            __result = null;
            return false;
        }

        var guide = reassureComp.parent.pawn;
        var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
        var recipientTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var guideTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(guide);
        var issues = recipientTracker.MostHeterodoxIssues(AbilityReassure.MaxBundleIssues, guideTracker);
        var stanceByIssue = recipientTracker.IssueStances().ToDictionary(s => s.issue, s => s.rank);

        var sb = new StringBuilder();
        sb.AppendLine("EnhancedBeliefs.Reassure.CertaintyGain".Translate(AbilityReassure.ReassureCertaintyGain.ToStringPercent()));

        if (issues.Count == 0)
        {
            sb.AppendLine("EnhancedBeliefs.Reassure.NoHeterodoxy".Translate());
        }
        else
        {
            if (guide == recipient)
            {
                sb.AppendLine("EnhancedBeliefs.Reassure.SelfTarget".Translate());
            }
            else
            {
                sb.AppendLine("EnhancedBeliefs.Reassure.SuccessChance".Translate(
                    InteractionWorker_IdeologicalDebatePrecept.WinChance(guide, recipient).ToStringPercent()));
            }

            sb.AppendLine("EnhancedBeliefs.Reassure.TargetBeliefs".Translate(AbilityReassure.MaxBundleIssues));
            foreach (var issue in issues)
            {
                var currentRank = stanceByIssue.TryGetValue(issue, out var cr) ? cr : 0f;
                var orthodoxRank = IdeoTrackerData.HeldRank(recipient.Ideo, issue);
                var rungs = PreceptLadder.Rungs(issue);
                var currentIdx = Mathf.Clamp(Mathf.RoundToInt(currentRank), 0, rungs.Count - 1);
                var orthodoxIdx = Mathf.Clamp(Mathf.RoundToInt(orthodoxRank), 0, rungs.Count - 1);

                if (rungs.Count > 0 && currentIdx != orthodoxIdx)
                    sb.AppendLine($" -  {issue.LabelCap}: {rungs[currentIdx].LabelCap} → {rungs[orthodoxIdx].LabelCap}");
                else
                    sb.AppendLine(" -  " + issue.LabelCap);
            }
        }

        sb.AppendLine("EnhancedBeliefs.Convert.Factors".Translate());
        sb.AppendLine(" -  " + "EnhancedBeliefs.Convert.Factor.ConversionPower".Translate(
            guide.GetStatValue(StatDefOf.ConversionPower).ToStringPercent(),
            recipient.GetStatValue(StatDefOf.ConversionPower).ToStringPercent()));
        sb.AppendLine(" -  " + "EnhancedBeliefs.Convert.Factor.Intellectual".Translate(
            InteractionWorker_IdeologicalDebatePrecept.IntellectualImpact(guide).ToStringPercent(),
            InteractionWorker_IdeologicalDebatePrecept.IntellectualImpact(recipient).ToStringPercent()));
        sb.Append(" -  " + "EnhancedBeliefs.Convert.Factor.Social".Translate(
            guide.GetStatValue(StatDefOf.SocialImpact).ToStringPercent(),
            recipient.GetStatValue(StatDefOf.SocialImpact).ToStringPercent()));

        __result = sb.ToString();
        return false;
    }
}
