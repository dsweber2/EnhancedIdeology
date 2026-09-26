using HarmonyLib;
using Verse.AI;

namespace EnhancedIdeology.HarmonyPatches;

// MentalState_SocialFighting exits immediately if IsOtherPawnSocialFightingWithMe returns false.
// Since the iconoclast already has EB_Iconoclast active, TryStartMentalState(SocialFighting) on them
// would no-op (forced: false). These two patches let the other pawn stay in SocialFighting while
// the iconoclast holds fightTarget, and prevent PostEnd from calling RecoverFromState on EB_Iconoclast.

[HarmonyPatch(typeof(MentalState_SocialFighting), "IsOtherPawnSocialFightingWithMe", MethodType.Getter)]
internal static class SocialFighting_IsOtherPawnFightingWithMe_Patch
{
    [HarmonyPostfix]
    static void Postfix(MentalState_SocialFighting __instance, ref bool __result)
    {
        if (!__result
            && __instance.otherPawn.MentalState is MentalState_Iconoclast iconoclastState
            && iconoclastState.fightTarget == __instance.pawn)
        {
            __result = true;
        }
    }
}

// PostEnd checks IsOtherPawnSocialFightingWithMe and calls RecoverFromState on the other pawn
// if true — which would end EB_Iconoclast. Clear fightTarget first so the check returns false,
// then add the iconoclast's fight-end thought ourselves since PostEnd won't.
[HarmonyPatch(typeof(MentalState_SocialFighting), nameof(MentalState_SocialFighting.PostEnd))]
internal static class SocialFighting_PostEnd_Patch
{
    [HarmonyPrefix]
    static void Prefix(MentalState_SocialFighting __instance)
    {
        if (__instance.otherPawn.MentalState is not MentalState_Iconoclast iconoclastState
            || iconoclastState.fightTarget != __instance.pawn)
        {
            return;
        }

        iconoclastState.fightTarget = null;

        var iconoclast = __instance.otherPawn;
        if (!iconoclast.Dead && iconoclast.needs?.mood != null && !__instance.pawn.Dead)
        {
            var thoughtDef = Rand.Value < 0.5f ? ThoughtDefOf.HadCatharticFight : ThoughtDefOf.HadAngeringFight;
            iconoclast.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, __instance.pawn);
        }
    }
}
