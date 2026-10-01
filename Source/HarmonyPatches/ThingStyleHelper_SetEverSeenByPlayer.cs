namespace EnhancedIdeology.HarmonyPatches;

// A relic is "found" when the player sees it for the first time. Vanilla sets the flag again on each spawn
// on a home map, so only the false -> true change counts.
[HarmonyPatch(typeof(ThingStyleHelper), nameof(ThingStyleHelper.SetEverSeenByPlayer))]
internal static class ThingStyleHelper_SetEverSeenByPlayer
{
    static void Prefix(Thing thing, out bool __state) => __state = thing.GetEverSeenByPlayer();

    static void Postfix(Thing thing, bool everSeenByPlayer, bool __state)
    {
        if (__state || !everSeenByPlayer || Current.ProgramState != ProgramState.Playing)
            return;
        if (thing.StyleSourcePrecept is not Precept_Relic { ideo: not null } relic)
            return;

        RelicConviction.ApplyFindBoost(Current.Game.GetComponent<GameComponent_EnhancedIdeology>(), relic);
    }
}
