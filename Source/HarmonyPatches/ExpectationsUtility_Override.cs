namespace EnhancedIdeology;

[HarmonyPatch]
internal static class ExpectationsUtility_Override
{
    internal static ExpectationDef? ForcedExpectation;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ExpectationsUtility), nameof(ExpectationsUtility.CurrentExpectationFor), typeof(Pawn))]
    private static bool OverrideForPawn(ref ExpectationDef __result)
    {
        if (ForcedExpectation == null)
            return true;
        __result = ForcedExpectation;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ExpectationsUtility), nameof(ExpectationsUtility.CurrentExpectationFor), typeof(Map))]
    private static bool OverrideForMap(ref ExpectationDef __result)
    {
        if (ForcedExpectation == null)
            return true;
        __result = ForcedExpectation;
        return false;
    }
}
