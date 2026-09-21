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

            // Debate someone in the room.
            var debateTarget = FindDebateTargetNear(pawn);
            if (debateTarget != null)
            {
                mentalState.agitationDebatesLeft--;
                return JobMaker.MakeJob(EnhancedIdeologyDefOf.EB_IconoclastDebate, debateTarget);
            }

            // Nobody nearby right now — linger and try again next tick.
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

    private static Pawn? FindDebateTargetNear(Pawn pawn)
    {
        var candidates = pawn.Map.mapPawns.AllPawnsSpawned
            .Where(p => p != pawn
                && p.RaceProps.Humanlike
                && !p.DevelopmentalStage.Baby()
                && !p.Downed
                && !p.Dead
                && p.Awake()
                && p.Position.InHorDistOf(pawn.Position, DebateSearchRadius)
                && pawn.CanReach(p, PathEndMode.Touch, Danger.Deadly))
            .ToList();

        if (candidates.TryMinBy(p => p.Position.DistanceToSquared(pawn.Position) + (p.Ideo == pawn.Ideo ? 0f : 1000f), out var best))
            return best;
        return null;
    }
}
