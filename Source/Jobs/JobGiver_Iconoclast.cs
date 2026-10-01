using Verse.AI;

namespace EnhancedIdeology;

internal sealed class JobGiver_Iconoclast : ThinkNode_JobGiver
{
    private IntRange waitTicks = new(80, 140);

    private const float FireStartChance = 0.65f;
    private const float AgitationArrivalRadius = 8f;
    private const float DebateSearchRadius = 20f;

    public override ThinkNode DeepCopy(bool resolve = true)
    {
        var obj = (JobGiver_Iconoclast)base.DeepCopy(resolve);
        obj.waitTicks = waitTicks;
        return obj;
    }

    protected override Job? TryGiveJob(Pawn pawn)
    {
        if (pawn.MentalState is not MentalState_Iconoclast mentalState)
            return null;
        if (mentalState.fightTarget != null)
            return null;
        if (mentalState.target == null)
            return null;

        if (!pawn.CanReach(mentalState.target, PathEndMode.Touch, Danger.Deadly))
        {
            var job = JobMaker.MakeJob(JobDefOf.Wait_Wander);
            job.expiryInterval = waitTicks.RandomInRange;
            pawn.mindState.nextMoveOrderIsWait = false;
            return job;
        }

        mentalState.EnsureAgitationCell(pawn, mentalState.target.Position);

        if (mentalState.agitationDebatesLeft > 0)
        {
            // Walk to the agitation spot first.
            if (!pawn.Position.InHorDistOf(mentalState.agitationCell, AgitationArrivalRadius))
                return JobMaker.MakeJob(JobDefOf.Goto, mentalState.agitationCell);

            // Debate someone nearby; if nobody is nearby, go to the closest pawn on the map.
            var debateTarget = FindDebateTarget(pawn);
            if (debateTarget != null)
            {
                mentalState.agitationDebatesLeft--;
                return JobMaker.MakeJob(EnhancedIdeologyDefOf.EB_IconoclastAgitate, debateTarget);
            }

            // Nobody reachable right now — linger and try again next tick.
            var linger = JobMaker.MakeJob(JobDefOf.Wait_Wander);
            linger.expiryInterval = 120;
            pawn.mindState.nextMoveOrderIsWait = true;
            return linger;
        }

        if (Rand.Value < FireStartChance)
        {
            if (mentalState.target is BookIdeo)
            {
                var burnJob = JobMaker.MakeJob(EnhancedIdeologyDefOf.EB_PlaceAndBurnUntilDestroyed, mentalState.target);
                burnJob.count = 1;
                burnJob.targetB = new LocalTargetInfo(mentalState.agitationCell);
                return burnJob;
            }

            var smashJob = JobMaker.MakeJob(JobDefOf.AttackMelee, mentalState.target);
            smashJob.maxNumMeleeAttacks = 10;
            return smashJob;
        }

        return null;
    }

    // Region-wise search: distance follows walls and doors, not a straight line.
    // Prefer a nearby pawn of another ideo; else take the closest pawn on the map.
    private static Pawn? FindDebateTarget(Pawn pawn)
    {
        bool IsCandidate(Thing t) =>
            t is Pawn p
            && p != pawn
            && p.RaceProps.Humanlike
            && !p.DevelopmentalStage.Baby()
            && !p.Downed
            && !p.Dead
            && p.Awake();

        Pawn? Closest(float maxDistance, Predicate<Thing> validator) =>
            (Pawn?)GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.Pawn),
                PathEndMode.Touch,
                TraverseParms.For(pawn, Danger.Deadly),
                maxDistance,
                validator);

        return Closest(DebateSearchRadius, t => IsCandidate(t) && ((Pawn)t).Ideo != pawn.Ideo)
            ?? Closest(9999f, IsCandidate);
    }
}
