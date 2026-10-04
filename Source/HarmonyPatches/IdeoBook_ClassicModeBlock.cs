namespace EnhancedIdeology.HarmonyPatches;

// Religious books only change certainty and stances. Classic mode has neither, so the books must not
// appear: not in trader stock, not in random book or reward sets, and the writing desk is not buildable.
internal static class IdeoBook_ClassicModeBlock
{
    // Some callers run at startup or in the main menu, when there is no world.
    private static bool ClassicMode => Find.World?.ideoManager?.classicMode == true;

    internal static bool Blocked(ThingDef def) =>
        def == EnhancedIdeologyDefOf.EB_Ideobook && ClassicMode;

    // StockGenerator_SingleDef and StockGenerator_Category both make their stock through this method.
    [HarmonyPatch(typeof(StockGeneratorUtility), nameof(StockGeneratorUtility.TryMakeForStock))]
    private static class TraderStock
    {
        private static bool Prefix(ThingDef thingDef, ref IEnumerable<Thing> __result)
        {
            if (!Blocked(thingDef)) return true;
            __result = [];
            return false;
        }
    }

    // ThingSetMaker_Books picks from this list.
    [HarmonyPatch(typeof(BookUtility), "GetBookDefs")]
    private static class RandomBooks
    {
        private static void Postfix(List<ThingDef> __result) => __result.RemoveAll(Blocked);
    }

    // Filters for reward and loot thing sets.
    [HarmonyPatch(typeof(ThingSetMakerUtility), nameof(ThingSetMakerUtility.CanGenerate))]
    private static class ThingSets
    {
        private static void Postfix(ThingDef thingDef, ref bool __result)
        {
            if (__result && Blocked(thingDef)) __result = false;
        }
    }

    // The writing desk has no other recipes.
    [HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.Visible), MethodType.Getter)]
    private static class WritingDesk
    {
        private static void Postfix(Designator_Build __instance, ref bool __result)
        {
            if (__result && __instance.PlacingDef == EnhancedIdeologyDefOf.EB_WritingDesk && ClassicMode)
                __result = false;
        }
    }
}
