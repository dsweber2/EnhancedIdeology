using Verse.AI;

namespace EnhancedIdeology;

internal sealed class JobDriver_PlaceAndBurnUntilDestroyed : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        var reserved = pawn.Reserve(TargetThingA, job, 1, -1, null, errorOnFailed, true);
        return reserved;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        _ = this.FailOn(delegate
        {
            return TargetThingA == null || TargetThingA.Destroyed;
        });
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch, true).FailOnSomeonePhysicallyInteracting(TargetIndex.A);

        yield return Toils_Haul.StartCarryThing(TargetIndex.A, canTakeFromInventory: true);
        var burnCell = job.targetB.IsValid ? job.targetB.Cell : FindPublicBurnSpot(pawn);
        yield return Toils_Goto.GotoCell(burnCell, PathEndMode.ClosestTouch);
        yield return Toils_General.Wait(90);
        yield return Toils_Haul.DropCarriedThing();
        yield return Toils_Reserve.ReserveDestinationOrThing(TargetIndex.A);
        yield return StepAdjacentToTarget(); // step off the book cell after dropping
        yield return Toils_General.Wait(90);

        var tryIgniteAgain = TryIgniteAgain();
        yield return tryIgniteAgain;

        yield return DebateNearestBystander();
        yield return StepAdjacentToTarget(); // approach for ignition without standing on the book
        var tryStartIgnite = ToilMaker.MakeToil("TryStartIgnite");
        tryStartIgnite.initAction = delegate
        {
            _ = pawn.natives.TryStartIgnite(TargetThingA);
        };
        yield return tryStartIgnite;

        yield return Toils_General.Wait(90);
        yield return Toils_Jump.JumpIf(tryIgniteAgain, () => !TargetThingA.IsBurning());
        yield return Toil_EnhancedIdeology.BurnBook().FailOn(() => !TargetThingA.IsBurning());
        yield return Toils_Jump.JumpIf(tryIgniteAgain, () => !TargetThingA.IsBurning() || !TargetThingA.Destroyed);
    }

    private static Toil DebateNearestBystander()
    {
        var toil = ToilMaker.MakeToil("DebateNearestBystander");
        toil.defaultDuration = 90;
        toil.defaultCompleteMode = ToilCompleteMode.Delay;
        toil.handlingFacing = true;
        toil.initAction = delegate
        {
            var actor = toil.actor;
            actor.Map.mapPawns.AllPawnsSpawned
                .Where(p => p != actor && p.RaceProps.Humanlike && !p.Dead && !p.Downed
                            && actor.CanReach(p, PathEndMode.Touch, Danger.Deadly))
                .TryMinBy(p => p.Position.DistanceToSquared(actor.Position), out var nearest);
            if (nearest != null)
            {
                if (!actor.WorkTagIsDisabled(WorkTags.Social))
                    actor.interactions.TryInteractWith(nearest, EnhancedIdeologyDefOf.EB_IdeologicalDebatePrecept);
                actor.rotationTracker.FaceTarget(nearest);
                // Step toward the nearest pawn so the iconoclast isn't standing on the dropped book.
                actor.pather.StartPath(nearest, PathEndMode.Touch);
            }
            else
            {
                // No one nearby — step to any adjacent standable cell.
                var bookCell = actor.jobs.curJob.GetTarget(TargetIndex.A).Cell;
                GenAdj.CardinalDirections
                    .Select(d => bookCell + d)
                    .Where(c => c.IsValid && c.Standable(actor.Map)
                                && actor.CanReach(c, PathEndMode.OnCell, Danger.Deadly))
                    .TryMinBy(c => c.DistanceToSquared(actor.Position), out var stepCell);
                if (stepCell.IsValid)
                    actor.pather.StartPath(stepCell, PathEndMode.OnCell);
            }
        };
        return toil;
    }

    // Moves the pawn to the closest standable cell adjacent to TargetThingA, never on it.
    private static Toil StepAdjacentToTarget()
    {
        var toil = ToilMaker.MakeToil("StepAdjacentToTarget");
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var book = actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
            if (book == null || !book.Spawned) return;
            GenAdj.CellsAdjacent8Way(book)
                .Where(c => c.Standable(actor.Map) && actor.CanReach(c, PathEndMode.OnCell, Danger.Deadly))
                .TryMinBy(c => c.DistanceToSquared(actor.Position), out var cell);
            if (cell.IsValid)
                actor.pather.StartPath(cell, PathEndMode.OnCell);
        };
        toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
        return toil;
    }

    private static readonly HashSet<RoomRoleDef> PrivateRoles =
    [
        RoomRoleDefOf.Bedroom, RoomRoleDefOf.Barracks,
        RoomRoleDefOf.PrisonCell, RoomRoleDefOf.PrisonBarracks,
        RoomRoleDefOf.Hospital, RoomRoleDefOf.Workshop, RoomRoleDefOf.Laboratory,
    ];

    private static IntVec3 FindPublicBurnSpot(Pawn pawn)
    {
        // Prefer indoor public rooms; gather spots (often outdoor) are a last resort.
        var bestRoom = pawn.Map.regionGrid.AllRooms
            .Where(r => r != null && r.ProperRoom && !r.PsychologicallyOutdoors && !PrivateRoles.Contains(r.Role))
            .MaxByWithFallback(r => r!.CellCount);

        if (bestRoom != null)
        {
            var cell = bestRoom.Cells
                .Where(c => pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly))
                .RandomElementWithFallback();
            if (cell.IsValid) return cell;
        }

        // Last resort: nearest outdoor gather spot.
        var activeSpots = pawn.Map.gatherSpotLister.activeSpots;
        if (activeSpots.Count > 0)
        {
            var nearest = activeSpots.MinBy(gs => gs.parent.Position.DistanceToSquared(pawn.Position));
            return nearest.parent.Position;
        }

        return pawn.Position.RandomAdjacentCell8Way().RandomAdjacentCell8Way();
    }

    private static Toil TryIgniteAgain()
    {
        return Toils_General.Label();
    }
}

internal sealed class Toil_EnhancedIdeology
{
    public static Toil BurnBook()
    {
        var toil = ToilMaker.MakeToil("BurnBook");
        toil.initAction = () => BurnBook_InitAction(toil);
        toil.tickAction = () => BurnBook_TickAction(toil);
        toil.AddFinishAction(() => BurnBook_FinishAction(toil));
        toil.AddEndCondition(() => BurnBook_EndCondition(toil));
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        toil.handlingFacing = true;
        _ = toil.WithProgressBar(TargetIndex.A, () => BurnBook_ProgressBarGetter(toil));
        return toil;
    }

    private static void BurnBook_InitAction(Toil toil)
    {
        var actor = toil.actor;
        var target = actor.jobs.curJob.GetTarget(TargetIndex.A);
        actor.rotationTracker.FaceCell(target.Cell);
        actor.jobs.curDriver.rotateToFace = TargetIndex.A;
    }

    private static void BurnBook_TickAction(Toil toil)
    {
        var actor = toil.actor;
        var book = (Thing)actor.jobs.curJob.GetTarget(TargetIndex.A);
        if (!book.Destroyed)
        {
            _ = book.TakeDamage(new DamageInfo(DamageDefOf.Burn, 0.2f, 100f, instigator: actor));
        }
    }

    private static void BurnBook_FinishAction(Toil toil)
    {
        var actor = toil.actor;
        var book = (ThingWithComps)actor.jobs.curJob.GetTarget(TargetIndex.A);
        if (book == null || !book.Destroyed)
        {
            return;
        }
        var comp = book.GetComp<CompBook>();
        Ideo? ideo = null;
        foreach (var doer in comp.doers)
        {
            if (doer is ReadingOutcomeDoer_CertaintyChange change)
            {
                ideo = change.ideo;
                break;
            }
        }
        if (ideo != null)
        {
            Messages.Message(
                "EnhancedIdeology.BookBurningSuccess".Translate(
                    actor.Named("PAWN"), book.Named("BOOK"), ideo.Named("IDEO")),
                actor,
                Find.FactionManager.OfPlayer.ideos.PrimaryIdeo == ideo
                    ? MessageTypeDefOf.NegativeEvent
                    : MessageTypeDefOf.NeutralEvent);
        }
    }

    private static JobCondition BurnBook_EndCondition(Toil toil)
    {
        var actor = toil.actor;
        var book = (Thing)actor.jobs.curJob.GetTarget(TargetIndex.A);
        return (book?.Destroyed ?? true) ? JobCondition.Succeeded : JobCondition.Ongoing;
    }

    private static float BurnBook_ProgressBarGetter(Toil toil)
    {
        var actor = toil.actor;
        var book = (Thing)actor.jobs.curJob.GetTarget(TargetIndex.A);
        return 1f - (book.HitPoints / ((float)book.MaxHitPoints));
    }
}
