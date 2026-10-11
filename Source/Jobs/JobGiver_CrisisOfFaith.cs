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

    private static readonly List<Thing> bookSearchSet = [];

    // GenClosest does the reach check before the validator, so give it only books and bookcases.
    private static BookIdeo? FindReadableReligiousBook(Pawn pawn)
    {
        var listerThings = pawn.Map.listerThings;
        bookSearchSet.Clear();
        foreach (var thing in listerThings.ThingsInGroup(ThingRequestGroup.Book))
        {
            if (thing is BookIdeo)
                bookSearchSet.Add(thing);
        }
        bookSearchSet.AddRange(listerThings.GetThingsOfType<Building_Bookcase>());
        if (bookSearchSet.Count == 0)
            return null;

        var book = GenClosest.ClosestThing_Global_Reachable(
            pawn.Position,
            pawn.Map,
            bookSearchSet,
            PathEndMode.Touch,
            TraverseParms.For(pawn),
            validator: t => t is BookIdeo book && BookUtility.CanReadBook(book, pawn, out _),
            canLookInHaulableSources: true) as BookIdeo;
        bookSearchSet.Clear();
        return book;
    }
}
