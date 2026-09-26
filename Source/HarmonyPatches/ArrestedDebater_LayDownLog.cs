using HarmonyLib;
using Verse.AI;

namespace EnhancedIdeology;

/// <summary>Temporary: log when a prisoner gets a LayDown job to diagnose sleep-blocking.</summary>
[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
internal static class ArrestedDebater_LayDownLog
{
    [HarmonyPrefix]
    private static void LogLayDown(Pawn_JobTracker __instance, Job newJob)
    {
        var pawn = Traverse.Create(__instance).Field<Pawn>("pawn").Value;
        if (pawn?.IsPrisonerOfColony != true) return;
        if (newJob?.def != JobDefOf.LayDown) return;
        Log.Message($"[EB] LayDown started on prisoner {pawn.LabelShort}: " +
                    $"mentalState={pawn.MentalStateDef?.defName ?? "null"} " +
                    $"AllowRestingInBed={pawn.MentalState?.AllowRestingInBed.ToString() ?? "N/A"} " +
                    $"target={newJob.targetA}");
        Log.Message(new System.Diagnostics.StackTrace().ToString());
    }
}
