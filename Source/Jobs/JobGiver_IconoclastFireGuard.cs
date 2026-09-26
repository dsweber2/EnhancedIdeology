using Verse.AI;

namespace EnhancedIdeology;

internal sealed class JobGiver_IconoclastFireGuard : ThinkNode_JobGiver
{
    internal const float FireGuardRadius = 4f;

    protected override Job? TryGiveJob(Pawn pawn)
    {
        if (pawn.MentalState is not MentalState_Iconoclast { target: not null } mentalState)
            return null;
        if (mentalState.fightTarget is { } ft)
        {
            if (ft.Dead || ft.Downed || !ft.Spawned || !pawn.CanReach(ft, PathEndMode.Touch, Danger.Deadly))
                return null;
            if (!SocialInteractionUtility.TryGetRandomVerbForSocialFight(pawn, out var verb))
                return null;
            var fightJob = JobMaker.MakeJob(JobDefOf.SocialFight, ft);
            fightJob.maxNumMeleeAttacks = 1;
            fightJob.verbToUse = verb;
            return fightJob;
        }
        if (mentalState.target.Destroyed)
            return null;
        var nearTarget = IsWithinWalkingDistance(pawn, mentalState.target, FireGuardRadius);
        if (!mentalState.target.IsBurning() && !nearTarget)
            return null;

        var canSocial = !pawn.WorkTagIsDisabled(WorkTags.Social);
        var canFight  = !pawn.WorkTagIsDisabled(WorkTags.Violent);
        if (!canSocial && !canFight) return null; // ignore interlopers entirely

        // Pin every interloper in range simultaneously, then physically debate the nearest.
        var reachable = FindAllInterlopersNearTarget(pawn, mentalState.target)
            .Where(p => pawn.CanReach(p, PathEndMode.Touch, Danger.Deadly))
            .ToList();
        if (reachable.Count == 0)
            return null;

        reachable.TryMinBy(p => p.Position.DistanceToSquared(pawn.Position), out var nearest);
        if (nearest == null) return null;

        if (!canSocial)
        {
            // Skip debate — initiate a social fight via the same path as StartDebateFight.
            if (!SocialInteractionUtility.TryGetRandomVerbForSocialFight(pawn, out var verb)) return null;
            mentalState.fightTarget = nearest;
            nearest.mindState.mentalStateHandler.TryStartMentalState(
                MentalStateDefOf.SocialFighting, null, forced: false, forceWake: false,
                causedByMood: false, pawn);
            if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(nearest))
                Messages.Message("MessageSocialFight".Translate(pawn.LabelShort, nearest.LabelShort, pawn.Named("PAWN1"), nearest.Named("PAWN2")), pawn, MessageTypeDefOf.ThreatSmall);
            TaleRecorder.RecordTale(TaleDefOf.SocialFight, pawn, nearest);
            var fightJob = JobMaker.MakeJob(JobDefOf.SocialFight, nearest);
            fightJob.maxNumMeleeAttacks = 1;
            fightJob.verbToUse = verb;
            return fightJob;
        }

        foreach (var interloper in reachable)
            mentalState.PinDebateTarget(interloper);

        return JobMaker.MakeJob(EnhancedIdeologyDefOf.EB_IconoclastDebate, nearest);
    }

    internal static bool IsInterloper(Pawn pawn, Pawn iconoclast, Thing target)
    {
        if (pawn == iconoclast) return false;
        if (iconoclast.MentalState is MentalState_Iconoclast ms && ms.HasServed(pawn)) return false;
        if (!IsInterferingWithTarget(pawn, target))
            return false;
        var targetRoom = target.GetRoom();
        if (targetRoom != null && pawn.GetRoom() != targetRoom) return false;
        return pawn.Position.InHorDistOf(target.Position, FireGuardRadius)
            && IsWithinWalkingDistance(pawn, target, FireGuardRadius)
            && iconoclast.CanReach(pawn, PathEndMode.Touch, Danger.Deadly);
    }

    // True when the pawn's current job would interfere with the burn target.
    // Gates on the specific object rather than a broad job category.
    private static bool IsInterferingWithTarget(Pawn pawn, Thing target)
    {
        var job = pawn.CurJob;
        if (job == null) return false;
        // Firefighter beating a fire that is burning the target or adjacent to it.
        if (job.def == JobDefOf.BeatFire)
        {
            var fire = job.GetTarget(TargetIndex.A).Thing as Fire;
            return fire != null && (fire.parent == target || fire.Position.InHorDistOf(target.Position, 2f));
        }
        // Any job that grabs or works directly on the target object.
        if (job.GetTarget(TargetIndex.A).Thing != target) return false;
        return job.def == JobDefOf.Repair
            || job.def == JobDefOf.HaulToCell
            || job.def == JobDefOf.HaulToContainer
            || job.def == JobDefOf.PrepareCaravan_GatherItems;
    }

    private static List<Pawn> FindAllInterlopersNearTarget(Pawn iconoclast, Thing target) =>
        iconoclast.Map.mapPawns.AllPawnsSpawned
            .Where(p => IsInterloper(p, iconoclast, target))
            .ToList();

    // Uses pathfinder so walls are accounted for; straight-line is a cheap pre-filter.
    private static bool IsWithinWalkingDistance(Pawn pawn, LocalTargetInfo target, float maxDist)
    {
        if (!pawn.Position.InHorDistOf(target.Cell, maxDist * 2f))
            return false;
        using var path = pawn.Map.pathFinder.FindPathNow(
            pawn.Position, target,
            TraverseParms.For(pawn),
            peMode: PathEndMode.Touch);
        return path.Found && path.NodesLeftCount <= maxDist + 1f;
    }
}
