using System.Text;
using Verse.Sound;

namespace EnhancedBeliefs.HarmonyPatches;

// Reroute the moral-guide Convert ability off the deleted OverrideConversionAttempt path onto the R3 bundle:
// a single debate roll firing 1-4 normal-strength conversions at once (AbilityConversion). Keeps vanilla's
// message / sound / play-log feedback; only the belief-change mechanic changes.
[HarmonyPatch(typeof(CompAbilityEffect_Convert), nameof(CompAbilityEffect_Convert.Apply))]
internal static class CompAbilityEffect_Convert_Apply
{
    private static bool Prefix(CompAbilityEffect_Convert __instance, LocalTargetInfo target)
    {
        if (!ModLister.CheckIdeology("Ideoligion conversion"))
        {
            return false;
        }

        var guide = __instance.parent.pawn;
        var recipient = target.Pawn;
        var props = __instance.Props;
        var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
        var recipientTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);

        var certaintyBefore = recipient.ideo.Certainty;
        var issues = AbilityConversion.RollTargetIssues(recipientTracker, guide.Ideo);

        // A win runs CheckConversion inside Resolve, which already switches the recipient's ideo - so, unlike
        // vanilla, we do not force SetIdeo here.
        if (AbilityConversion.Resolve(guide, recipient, issues))
        {
            Messages.Message(props.successMessage.Formatted(guide.Named("INITIATOR"), recipient.Named("RECIPIENT"), guide.Ideo.name.Named("IDEO")), new LookTargets(guide, recipient), MessageTypeDefOf.PositiveEvent);
            Find.PlayLog.Add(new PlayLogEntry_Interaction(InteractionDefOf.Convert_Success, guide, recipient, null));
        }
        else
        {
            guide.needs.mood?.thoughts.memories.TryGainMemory(props.failedThoughtInitiator, recipient);
            recipient.needs.mood?.thoughts.memories.TryGainMemory(props.failedThoughtRecipient, guide);
            Messages.Message(props.failMessage.Formatted(guide.Named("INITIATOR"), recipient.Named("RECIPIENT"), guide.Ideo.name.Named("IDEO"), certaintyBefore.ToStringPercent().Named("CERTAINTYBEFORE"), recipient.ideo.Certainty.ToStringPercent().Named("CERTAINTYAFTER")), new LookTargets(guide, recipient), MessageTypeDefOf.NeutralEvent);
            Find.PlayLog.Add(new PlayLogEntry_Interaction(InteractionDefOf.Convert_Failure, guide, recipient, null));
        }

        props.sound?.PlayOneShot(new TargetInfo(target.Cell, guide.Map));
        return false;
    }
}

// Replace vanilla's certainty-subtraction breakdown tooltip with one describing the R3 mechanic: the temporary
// certainty knock, the debate win chance, the conversion-on-win chance, the candidate target beliefs, and the
// per-side factors that feed the debate roll.
[HarmonyPatch(typeof(CompAbilityEffect_Convert), nameof(CompAbilityEffect_Convert.ExtraLabelMouseAttachment))]
internal static class CompAbilityEffect_Convert_Tooltip
{
    private static bool Prefix(CompAbilityEffect_Convert __instance, LocalTargetInfo target, ref string? __result)
    {
        var recipient = target.Pawn;
        if (recipient == null || !__instance.Valid(target))
        {
            __result = null;
            return false;
        }

        var guide = __instance.parent.pawn;
        var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
        var recipientTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var knock = EnhancedBeliefsMod.Settings.ConversionCertaintyKnock;
        var issues = recipientTracker.MostOpposingIssues(guide.Ideo, AbilityConversion.MaxBundleIssues);

        var sb = new StringBuilder();
        sb.AppendLine("EnhancedBeliefs.Convert.CertaintyKnock".Translate((1f - knock).ToStringPercent()));

        if (issues.Count == 0)
        {
            sb.AppendLine("EnhancedBeliefs.Convert.NoOpposition".Translate());
        }
        else
        {
            sb.AppendLine("EnhancedBeliefs.Convert.SuccessChance".Translate(
                InteractionWorker_IdeologicalDebatePrecept.WinChance(guide, recipient).ToStringPercent()));
            sb.AppendLine("EnhancedBeliefs.Convert.ConversionChance".Translate(
                recipientTracker.ConversionChanceAfterKnock(guide.Ideo, knock).ToStringPercent()));

            sb.AppendLine("EnhancedBeliefs.Convert.TargetBeliefs".Translate(AbilityConversion.MaxBundleIssues));
            foreach (var issue in issues)
            {
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
