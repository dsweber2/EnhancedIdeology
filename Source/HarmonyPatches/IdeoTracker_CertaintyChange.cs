namespace EnhancedIdeology.HarmonyPatches;

// Intercepts vanilla's certainty integration: we return 0 so vanilla applies no change.
// Recaching and actual integration are handled by IdeoTracker_TickInterval at TickRare/TickLong.
[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.CertaintyChangePerDay), MethodType.Getter)]
internal static class IdeoTracker_CertaintyChange
{
    private static bool Prefix(ref float __result)
    {
        __result = 0;
        return false;
    }
}
