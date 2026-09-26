using HarmonyLib;
using Verse.AI;

namespace EnhancedIdeology;

/// <summary>
/// Block sleep and food seeking while pawn is in the ArrestedDebater mental state.
/// </summary>
[HarmonyPatch]
internal static class ArrestedDebater_BlockNeeds
{
    /// <summary>
    /// The carrier's toil calls TuckIntoBed which calls Notify_TuckedIntoBed, forcibly starting a
    /// LayDown job on the prisoner before their think tree ever runs. Skip it entirely when in this
    /// state — the pawn was already dropped at the bed position by TryDropCarriedThing, so they end
    /// up in the cell. Their think tree fires on the next tick and gives a wander job instead.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.Notify_TuckedIntoBed))]
    [HarmonyPrefix]
    private static bool SkipTuckIntoBed_ForArrestedDebater(Pawn_JobTracker __instance)
    {
        var pawn = Traverse.Create(__instance).Field<Pawn>("pawn").Value;
        return pawn?.MentalStateDef != EnhancedIdeologyDefOf.EB_ArrestedDebater;
    }

    [HarmonyPatch(typeof(JobGiver_GetRest), nameof(JobGiver_GetRest.GetPriority))]
    [HarmonyPostfix]
    private static void BlockRest_Priority(Pawn pawn, ref float __result)
    {
        if (pawn.MentalStateDef == EnhancedIdeologyDefOf.EB_ArrestedDebater)
            __result = 0f;
    }

    [HarmonyPatch(typeof(JobGiver_GetFood), nameof(JobGiver_GetFood.GetPriority))]
    [HarmonyPostfix]
    private static void BlockFood_Priority(Pawn pawn, ref float __result)
    {
        if (pawn.MentalStateDef == EnhancedIdeologyDefOf.EB_ArrestedDebater)
            __result = 0f;
    }
}
