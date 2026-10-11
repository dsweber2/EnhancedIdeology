namespace EnhancedIdeology.HarmonyPatches;

[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoTrackerTickInterval))]
internal static class IdeoTracker_TickInterval
{
    private const float CheckIntervalDays = GenTicks.TickLongInterval / 60000f;

    // The slow-input refresh calls vanilla OpinionOf once for each local pawn, and each call recomputes that
    // pair's situational social thoughts. It runs on every fourth long tick. The pawn's hash offset spreads the
    // refreshes of different pawns across the interval.
    internal const int SlowInputsInterval = GenTicks.TickLongInterval * 4;

    // Vanilla calls this every `delta` ticks (1 on screen, up to 15 off screen), not every tick.
    // The interval check uses the delta overload, so each window fires once even when its exact tick is skipped.
    // Certainty advances only on the long tick, from the setpoint recached just before it. A recache between
    // long ticks would only move the social card's setpoint marker, so all work runs on the long tick.
    internal static void Postfix(Pawn_IdeoTracker __instance, int delta)
    {
        var pawn = __instance.pawn;

        // Fast exit: skip all lookups on the calls where nothing fires.
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta))
            return;

        // Caravan members have no map but must still drift. Suspended pawns (cryptosleep) are frozen, although
        // vanilla still calls this tick for them.
        if (pawn.Destroyed || pawn.Suspended || (pawn.MapHeld == null && !pawn.IsCaravanMember())
            || __instance.ideo == null || Find.IdeoManager.classicMode)
            return;

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var data = comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        // Refresh slow inputs and apply mood pulls before recaching so the bands use fresh data and moved stances.
        if (pawn.IsHashIntervalTick(SlowInputsInterval, delta))
            data.RefreshSlowInputs();
        comp.ApplyMoodletConvictionShifts(data);
        data.ApplyConvictionDecayIfNewDay();
        data.CertaintyChangeRecache(comp);
        data.AdvanceExtendedCertainty(CheckIntervalDays);

        if (!pawn.InMentalState)
            data.TryBackgroundConversion(CheckIntervalDays);
    }
}
