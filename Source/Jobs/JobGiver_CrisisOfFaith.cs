using Verse.AI;

namespace EnhancedIdeology;

internal sealed class JobGiver_CrisisOfFaith : ThinkNode_JobGiver
{
    protected override Job? TryGiveJob(Pawn pawn)
    {
        if (pawn.MentalStateDef != EnhancedIdeologyDefOf.EB_CrisisOfFaith || pawn.Map == null)
            return null;

        var prayJob = JoyGiver_Contemplation.TryBuildPrayJob(pawn);
        if (prayJob != null)
            return prayJob;

        var book = FindReadableReligiousBook(pawn);
        if (book != null)
            return JobMaker.MakeJob(JobDefOf.Reading, book);

        return null;
    }

    private static BookIdeo? FindReadableReligiousBook(Pawn pawn)
    {
#if !v1_5
        return GenClosest.ClosestThing_Global_Reachable(
#else
        return GenClosest.ClosestThing_Global_Reachable_NewTemp(
#endif
            pawn.Position,
            pawn.Map,
            pawn.Map.listerThings.AllThings,
            PathEndMode.Touch,
            TraverseParms.For(pawn),
            validator: t => t is BookIdeo book && BookUtility.CanReadBook(book, pawn, out _),
            canLookInHaulableSources: true) as BookIdeo;
    }
}
