namespace EnhancedIdeology.HarmonyPatches;

// Vanilla refuses to discard a world pawn and returns early, so only act once the pawn is really discarded.
[HarmonyPatch(typeof(Pawn), nameof(Pawn.Discard))]
internal static class Pawn_Discard
{
    private static void Postfix(Pawn __instance)
    {
        if (__instance.Discarded)
            Current.Game?.GetComponent<GameComponent_EnhancedIdeology>()?.Notify_PawnDiscarded(__instance);
    }
}
