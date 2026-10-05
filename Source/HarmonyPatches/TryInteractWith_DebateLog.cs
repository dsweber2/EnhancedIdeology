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

        var replacement = PlayLogEntry_DebateInteraction.FromLastDebate(intDef, initiator, recipient, sentencePacks);
        if (replacement != null)
            entries[0] = replacement;
    }
}
