using Verse.AI;

namespace EnhancedIdeology;

// Prisoners have no recreation need in vanilla, so they never pick up a book. This gives them a reading job for a
// religious book they can reach. Prisoners that have a recreation need (from another mod) use that path instead.
internal sealed class JobGiver_PrisonerReadReligiousText : ThinkNode_JobGiver
{
    protected override Job? TryGiveJob(Pawn pawn)
    {
        if (!pawn.IsPrisonerOfColony
            || pawn.needs?.joy != null
            || pawn.Ideo == null
            || pawn.Map == null
            || !ModsConfig.IdeologyActive
            || Find.IdeoManager.classicMode)
            return null;

        var now = Find.TickManager.TicksGame;
        if (pawn.mindState.thinkData.TryGetValue(UniqueSaveKey, out var nextReadTick) && now < nextReadTick)
            return null;

        var book = JobGiver_CrisisOfFaith.FindReadableReligiousBook(pawn);
        if (book == null)
            return null;

        pawn.mindState.thinkData[UniqueSaveKey] =
            now + PrisonerReading.CycleTicks(pawn.ideo.Certainty, JobDefOf.Reading.joyDuration);
        return JobMaker.MakeJob(JobDefOf.Reading, book);
    }
}
