using System.Reflection;
using HarmonyLib;

namespace EnhancedIdeology.HarmonyPatches;

[HarmonyPatch]
internal static class Pawn_DraftController_DebatePinned_Patch
{
    [HarmonyTargetMethod]
    static MethodBase TargetMethod() =>
        AccessTools.Method(typeof(Pawn_DraftController), "GetGizmos");

    [HarmonyPostfix]
    static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn_DraftController __instance)
    {
        if (!MentalState_Iconoclast.PinnedByIconoclast.TryGetValue(__instance.pawn, out var iconoclast))
        {
            foreach (var g in __result) yield return g;
            yield break;
        }
        var reason = "EnhancedIdeology.DraftDisabled_HeldInDebate".Translate(iconoclast.LabelShort);
        foreach (var g in __result)
        {
            if (g is Command cmd) cmd.Disable(reason);
            yield return g;
        }
    }
}

[HarmonyPatch(typeof(Pawn_NativeVerbs), nameof(Pawn_NativeVerbs.TryBeatFire))]
internal static class Pawn_NativeVerbs_TryBeatFire_DebatePinned_Patch
{
    private static readonly AccessTools.FieldRef<Pawn_NativeVerbs, Pawn> PawnRef =
        AccessTools.FieldRefAccess<Pawn_NativeVerbs, Pawn>("pawn");

    [HarmonyPrefix]
    static bool Prefix(Pawn_NativeVerbs __instance) =>
        !MentalState_Iconoclast.PinnedByIconoclast.ContainsKey(PawnRef(__instance));
}
