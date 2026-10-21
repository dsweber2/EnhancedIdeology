using Verse.AI;

namespace EnhancedIdeology.HarmonyPatches;

// JobDriver_Reading does not override GetReport, so patch the base and filter by driver type.
[HarmonyPatch(typeof(JobDriver), nameof(JobDriver.GetReport))]
internal static class JobDriver_Reading_Report
{
    static void Postfix(JobDriver __instance, ref string __result)
    {
        if (__instance is not JobDriver_Reading { Book: { } book })
            return;

        var doer = book.BookComp.Doers.OfType<ReadingOutcomeDoer_CertaintyChange>().FirstOrDefault();
        var reader = __instance.pawn;
        if (doer?.ideo == null || !doer.DoesProvidesOutcome(reader))
            return;

        var perHour = doer.PeakArcPerTick(reader)
            * reader.GetStatValue(StatDefOf.ReadingSpeed)
            * BookUtility.GetReadingBonus(reader)
            * GenDate.TicksPerHour;
        __result = "EnhancedIdeology.BookReadingReport".Translate(
            __result.Named("REPORT"),
            doer.ideo.Named("IDEO"),
            perHour.ToString("0.###", CultureInfo.InvariantCulture).Named("RATE"));
    }
}
