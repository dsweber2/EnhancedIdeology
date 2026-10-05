namespace EnhancedIdeology.HarmonyPatches;

// Vanilla calls Notify_PreReform, overwrites the ideo, then calls Notify_Reformed.
// The held ranks are recorded before the overwrite, so orthodox believers can follow the moved rungs.
[HarmonyPatch(typeof(IdeoDevelopmentTracker), nameof(IdeoDevelopmentTracker.Notify_PreReform))]
internal static class FluidIdeoTracker_PreReform
{
    internal static Dictionary<IssueDef, float>? OldHeldRanks;

    private static void Prefix(IdeoDevelopmentTracker __instance)
    {
        OldHeldRanks = IdeoReform.HeldRanks(__instance.ideo);
    }
}

[HarmonyPatch(typeof(IdeoDevelopmentTracker), nameof(IdeoDevelopmentTracker.Notify_Reformed))]
internal static class FluidIdeoTracker_Reformed
{
    private static void Postfix(IdeoDevelopmentTracker __instance)
    {
        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        if (FluidIdeoTracker_PreReform.OldHeldRanks is { } oldHeldRanks)
        {
            IdeoReform.CarryOrthodoxStances(comp, __instance.ideo, oldHeldRanks);
            FluidIdeoTracker_PreReform.OldHeldRanks = null;
        }
        comp.BaseOpinionRecache(__instance.ideo);
    }
}
