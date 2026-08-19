namespace EnhancedIdeology.HarmonyPatches;

#if v1_5
[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoTrackerTick))]
#else
[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoTrackerTickInterval))]
#endif
internal static class IdeoTracker_TickInterval
{
    private const float CheckIntervalDays = GenTicks.TickLongInterval / 60000f;

    private static void Postfix(Pawn_IdeoTracker __instance)
    {
        var pawn = __instance.pawn;

        // Fast exit: skip all lookups the 499/500 ticks where nothing fires.
        if (!pawn.IsHashIntervalTick(GenTicks.TickRareInterval))
            return;

        if (pawn.Destroyed || pawn.Map == null || __instance.ideo == null || Find.IdeoManager.classicMode)
            return;

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var data = comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        // Refresh relationship opinions before recaching so the relational band uses fresh data.
        if (pawn.IsHashIntervalTick(GenTicks.TickLongInterval))
            data.RecalculateRelationshipIdeoOpinions();

        data.CertaintyChangeRecache(comp);

        if (pawn.IsHashIntervalTick(GenTicks.TickLongInterval))
        {
            data.AdvanceExtendedCertainty(CheckIntervalDays);
            if (!pawn.InMentalState)
                data.TryBackgroundConversion(CheckIntervalDays);
        }
    }
}
