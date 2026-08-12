namespace EnhancedIdeology.HarmonyPatches;

[HarmonyPatch(typeof(Ideo), MethodType.Constructor)]
internal static class Ideo_Constructor
{
    private static void Postfix(Ideo __instance)
    {
        _ = Current.Game.GetComponent<GameComponent_EnhancedIdeology>()
            .IdeoTracker.AddPawnTrackerToIdeo(__instance);
    }
}
