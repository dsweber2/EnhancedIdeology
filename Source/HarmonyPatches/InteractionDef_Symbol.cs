namespace EnhancedIdeology.HarmonyPatches;

// Debates use meme/precept symbol instead for their motes
[HarmonyPatch(typeof(InteractionDef), nameof(InteractionDef.GetSymbol))]
internal static class InteractionDef_Symbol
{
    private static bool Prefix(InteractionDef __instance, ref Texture2D __result)
    {
        if (__instance.Worker is InteractionWorker_IdeologicalDebateMeme worker)
        {
            if (worker.topic != null)
            {
                var icon = worker.topic.Icon ?? worker.initiatorIdeo?.Icon;
                worker.topic = null;
                worker.initiatorIdeo = null;
                if (icon != null) { __result = icon; return false; }
            }
        }

        if (__instance.Worker is InteractionWorker_IdeologicalDebatePrecept worker2)
        {
            if (worker2.topic != null)
            {
                var icon = worker2.topic.Icon ?? worker2.initiatorIdeo?.Icon;
                worker2.topic = null;
                worker2.topicPrecept = null;
                worker2.initiatorIdeo = null;
                if (icon != null) { __result = icon; return false; }
            }
        }

        return true;
    }
}
