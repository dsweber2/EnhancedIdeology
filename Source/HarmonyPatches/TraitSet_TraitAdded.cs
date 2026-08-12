namespace EnhancedIdeology.HarmonyPatches;

[HarmonyPatch(typeof(TraitSet), nameof(TraitSet.GainTrait))]
internal static class TraitSet_TraitAdded
{
    private static void Postfix(TraitSet __instance)
    {
        Current.Game.GetComponent<GameComponent_EnhancedIdeology>().PawnTracker?.TryGetIdeoTracker(__instance.pawn)?.RecacheAllBaseOpinions();
    }
}
