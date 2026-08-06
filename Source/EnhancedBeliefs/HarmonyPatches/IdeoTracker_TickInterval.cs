namespace EnhancedBeliefs.HarmonyPatches;

#if v1_5
[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoTrackerTick))]
#else
[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoTrackerTickInterval))]
#endif
internal static class IdeoTracker_TickInterval
{
    // Fixed elapsed time between hash-gated checks; the spontaneous-conversion hazard integrates over this.
    private static readonly float CheckIntervalDays = GenTicks.TickLongInterval / 60000f;

    private static void Postfix(Pawn_IdeoTracker __instance)
    {
        var pawn = __instance.pawn;

        if (pawn.Destroyed || pawn.Map == null || __instance.ideo == null
            || Find.IdeoManager.classicMode || !pawn.IsHashIntervalTick(GenTicks.TickLongInterval))
        {
            return;
        }

        var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
        var data = comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        data.RecalculateRelationshipIdeoOpinions();

        if (!pawn.InMentalState)
        {
            data.TryBackgroundConversion(CheckIntervalDays);
        }
    }
}
