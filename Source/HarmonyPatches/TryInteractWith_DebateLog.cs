using HarmonyLib;

namespace EnhancedIdeology.HarmonyPatches;

[HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
internal static class TryInteractWith_DebateLog
{
    [HarmonyPostfix]
    static void Postfix(Pawn_InteractionsTracker __instance, Pawn recipient, InteractionDef intDef, bool __result)
    {
        if (!__result)
            return;

        var entries = Find.PlayLog.AllEntries;
        if (entries.Count == 0 || entries[0] is not PlayLogEntry_Interaction existing)
            return;

        var initiator = Traverse.Create(__instance).Field<Pawn>("pawn").Value;
        var sentencePacks = Traverse.Create(existing).Field<List<RulePackDef>>("extraSentencePacks").Value;

        PlayLogEntry_DebateInteraction? replacement = null;

        if (intDef == EnhancedIdeologyDefOf.EB_IdeologicalDebatePrecept)
        {
            var worker = (InteractionWorker_IdeologicalDebatePrecept)intDef.Worker;
            if (worker.topic == null)
                return;
            replacement = new PlayLogEntry_DebateInteraction(
                intDef, initiator, recipient, sentencePacks,
                worker.topic, worker.lastWinner, worker.lastWinnerPrecept?.label);
        }
        else if (intDef == EnhancedIdeologyDefOf.EB_IdeologicalDebateMeme)
        {
            var worker = (InteractionWorker_IdeologicalDebateMeme)intDef.Worker;
            if (worker.topic == null)
                return;
            replacement = new PlayLogEntry_DebateInteraction(
                intDef, initiator, recipient, sentencePacks,
                worker.topic, worker.lastWinner, worker.topic.label);
        }

        if (replacement != null)
            entries[0] = replacement;
    }
}
