namespace EnhancedIdeology.HarmonyPatches;

[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.CertaintyChangePerDay), MethodType.Getter)]
internal static class IdeoTracker_CertaintyChange
{
    private static Game? _cachedGame;
    private static GameComponent_EnhancedIdeology? _comp;

    private static bool Prefix(Pawn_IdeoTracker __instance, ref float __result)
    {
        __result = 0;

        var pawn = __instance.pawn;
        var game = Current.Game;
        if (!ReferenceEquals(game, _cachedGame))
        {
            _cachedGame = game;
            _comp = game.GetComponent<GameComponent_EnhancedIdeology>();
        }
        var comp = _comp!;

        var data = comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        // 1 recache per rare tick should be enough
        if (data.CachedCertaintyChange == -9999f || pawn.IsHashIntervalTick(GenTicks.TickRareInterval))
        {
            data.CertaintyChangeRecache(comp);
        }

        __result += data.CachedCertaintyChange;

        return false;
    }
}
