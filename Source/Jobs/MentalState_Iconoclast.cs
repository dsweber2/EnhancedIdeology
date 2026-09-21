using Verse.AI;

namespace EnhancedIdeology;

internal sealed class MentalState_Iconoclast : MentalState
{
    // Maps each pinned pawn → the iconoclast holding them, for inspect-string display.
    internal static readonly Dictionary<Pawn, Pawn> PinnedByIconoclast = new();

    public Thing? target;
    public Pawn? fightTarget;
    public List<Pawn> debateTargets = new();
    public int booksLeft = -1;

    // Pawns who completed their 5s debate for the current target; cleared on target change.
    private readonly HashSet<Pawn> servedDebateTargets = new();
    // Tick when each pawn was first pinned; used to detect natural 5s expiry.
    private readonly Dictionary<Pawn, int> pinStartTick = new();
    public int agitationDebatesLeft;
    public IntVec3 agitationCell = IntVec3.Invalid;
    private int foiledTicks;

    // Any firefighter/repairer who wanders within this many cells of the iconoclast
    // gets swept into the debate alongside the primary target.
    private const float DebateSweepRadius = 3f;

    // How long a pinned pawn's Wait job lasts, and the threshold at which they are marked served.
    // Both must stay in sync — once the Wait expires the pawn is freed and immune to re-pinning.
    internal const int DebatePinDurationTicks = 10 * 60;
    // Give up the mental state after being foiled for this many cumulative ticks (~1 in-game day).
    internal const int FoiledGiveUpTicks = 60000;

#if v1_5
    public override void MentalStateTick()
#else
    public override void MentalStateTick(int delta)
#endif
    {
        if (pawn.IsPrisonerOfColony)
        {
            var handler = pawn.mindState.mentalStateHandler;
            RecoverFromState();
            handler.TryStartMentalState(EnhancedIdeologyDefOf.EB_ArrestedDebater);
            return;
        }

        if (booksLeft <= 0)
        {
            RecoverFromState();
            return;
        }

        if (target == null || target.Destroyed)
        {
            booksLeft -= 1;
            ClearDebateTargets();
            agitationCell = IntVec3.Invalid;
            agitationDebatesLeft = Rand.RangeInclusive(3, 5);
            foiledTicks = 0;

            if (booksLeft == 0 || !TryFindNewTarget())
            {
                RecoverFromState();
                return;
            }
        }

        // Clear fight target if the other pawn's SocialFighting state ended (PostEnd_Prefix handles
        // thought and clears fightTarget when PostEnd fires; this catches any remaining edge cases).
        if (fightTarget != null
            && (!fightTarget.Spawned || fightTarget.Dead || fightTarget.Downed
                || fightTarget.MentalStateDef != MentalStateDefOf.SocialFighting))
        {
            fightTarget = null;
        }

        if (fightTarget == null)
        {
            // Maintain existing pins: drop departed or served targets, re-force Wait otherwise.
            for (var ii = debateTargets.Count - 1; ii >= 0; ii--)
            {
                var dt = debateTargets[ii];
                var stillNear = dt.Spawned
                    && !dt.Dead
                    && !dt.Downed
                    && dt.Position.InHorDistOf(target!.Position, JobGiver_IconoclastFireGuard.FireGuardRadius)
                    && pawn.CanReach(dt, PathEndMode.Touch, Danger.Deadly);
                if (!stillNear)
                {
                    RemoveDebateTarget(dt);
                    continue;
                }
                // Expiry: mark served so they can't be re-pinned for this target.
                if (pinStartTick.TryGetValue(dt, out var startTick)
                    && Find.TickManager.TicksGame - startTick >= DebatePinDurationTicks)
                {
                    servedDebateTargets.Add(dt);
                    RemoveDebateTarget(dt);
                    continue;
                }
                EnsureWaitJob(dt);
            }

            var nearTarget = pawn.Position.InHorDistOf(target!.Position, JobGiver_IconoclastFireGuard.FireGuardRadius);
            if ((target.IsBurning() || nearTarget) && pawn.IsHashIntervalTick(30))
            {
                if (debateTargets.Count == 0)
                {
                    // No active debate: trigger the job switch so JobGiver_IconoclastFireGuard fires.
                    var hasInterlopers = pawn.Map.mapPawns.AllPawnsSpawned
                        .Any(p => JobGiver_IconoclastFireGuard.IsInterloper(p, pawn, target));
                    if (hasInterlopers)
                    {
                        // Incapable of engaging: still count as foiled toward the give-up timer.
                        if (pawn.WorkTagIsDisabled(WorkTags.Social) && pawn.WorkTagIsDisabled(WorkTags.Violent))
                            foiledTicks += 30;
                        pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                }
                else
                {
                    // Already debating: sweep nearby for any additional interlopers and pull them in.
                    foreach (var p in pawn.Map.mapPawns.AllPawnsSpawned)
                    {
                        if (!debateTargets.Contains(p)
                            && JobGiver_IconoclastFireGuard.IsInterloper(p, pawn, target)
                            && p.Position.InHorDistOf(pawn.Position, DebateSweepRadius)
                            && !pawn.WorkTagIsDisabled(WorkTags.Social))
                        {
                            PinDebateTarget(p);
                            pawn.interactions.TryInteractWith(p, EnhancedIdeologyDefOf.EB_IdeologicalDebatePrecept);
                        }
                    }
                }
            }
        }

        // target is not null here because we checked it above; if target is null or destroyed and
        // TryFindNewTarget returns false, we already returned.
        if (!pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
        {
            var thing = target!;

            if (!TryFindNewTarget())
            {
                RecoverFromState();
                return;
            }

            Messages.Message("MessageTargetedTantrumChangedTarget".Translate(pawn.LabelShort, thing.Label, target!.Label, pawn.Named("PAWN"), thing.Named("OLDTARGET"), target.Named("TARGET")).AdjustedFor(pawn), pawn, MessageTypeDefOf.NegativeEvent);
        }

        if (debateTargets.Count > 0 || fightTarget != null)
#if v1_5
            foiledTicks++;
#else
            foiledTicks += delta;
#endif
        if (foiledTicks >= FoiledGiveUpTicks)
        {
            RecoverFromState();
            return;
        }

#if v1_5
        base.MentalStateTick();
#else
        base.MentalStateTick(delta);
#endif
    }

    public override void PostEnd()
    {
        ClearDebateTargets();
        base.PostEnd();
    }

    internal bool HasServed(Pawn p) => servedDebateTargets.Contains(p);

    // Adds the pawn to the debate target list and forces them into a Wait job.
    // Safe to call if already pinned — re-forces the Wait if something else cleared their job.
    internal void PinDebateTarget(Pawn p)
    {
        if (!debateTargets.Contains(p))
        {
            debateTargets.Add(p);
            pinStartTick.TryAdd(p, Find.TickManager.TicksGame);
        }
        PinnedByIconoclast[p] = pawn;
        EnsureWaitJob(p);
    }

    // Releases a single pinned target — used when they win their debate against the iconoclast.
    internal void RemoveDebateTarget(Pawn p)
    {
        if (!debateTargets.Remove(p)) return;
        PinnedByIconoclast.Remove(p);
        pinStartTick.Remove(p);
        if (p.CurJobDef == JobDefOf.Wait)
            p.jobs.EndCurrentJob(JobCondition.Succeeded);
    }

    // Releases all pinned targets — called on mental state end or target change.
    internal void ClearDebateTargets()
    {
        foreach (var p in debateTargets)
        {
            PinnedByIconoclast.Remove(p);
            if (p.CurJobDef == JobDefOf.Wait)
                p.jobs.EndCurrentJob(JobCondition.Succeeded);
        }
        debateTargets.Clear();
        pinStartTick.Clear();
        servedDebateTargets.Clear();
    }

    private void EnsureWaitJob(Pawn p)
    {
        if (p.CurJobDef == JobDefOf.Wait) return;
        var waitJob = JobMaker.MakeJob(JobDefOf.Wait);
        waitJob.expiryInterval = DebatePinDurationTicks;
        waitJob.checkOverrideOnExpire = true;
        waitJob.reportStringOverride = "EnhancedIdeology.JobReport_Debating".Translate(pawn.LabelShort);
        p.jobs.StartJob(waitJob, JobCondition.InterruptForced, cancelBusyStances: true);
    }

    public override void PostStart(string reason)
    {
        base.PostStart(reason);
        booksLeft = Rand.RangeInclusive(2, 4);
        agitationDebatesLeft = Rand.RangeInclusive(3, 5);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            // Attempt to load old value label first.
            Scribe_Values.Look(ref booksLeft, "booksBurned");
            if (booksLeft == 0)
            {
                Scribe_Values.Look(ref booksLeft, "booksLeft");
            }
        }
        else
        {
            Scribe_Values.Look(ref booksLeft, "booksLeft");
        }
        Scribe_Values.Look(ref agitationDebatesLeft, "agitationDebatesLeft");
        Scribe_Values.Look(ref agitationCell, "agitationCell");
        Scribe_Values.Look(ref foiledTicks, "foiledTicks");
        Scribe_References.Look(ref fightTarget, "fightTarget");
    }

    private bool TryFindNewTarget()
    {
        // Priority: ground relic > reliquary containing a relic > ideo book > idol/altar > empty reliquary
        var groundRelic    = FindClosestReachable(pawn, t => t.IsRelic());
        var fullReliquary  = FindClosestReachable(pawn, t => t.TryGetComp<CompRelicContainer>() is { Full: true });
        var book           = FindClosestReachable(pawn, t => t is BookIdeo b && b.Ideo == pawn.Ideo);
        var idol           = FindClosestReachable(pawn, IsIdeoligicalStructure);
        var emptyReliquary = FindClosestReachable(pawn, t => t.TryGetComp<CompRelicContainer>() is { Full: false });
        target = groundRelic ?? fullReliquary ?? book ?? idol ?? emptyReliquary;
        return target != null;
    }

    internal static Thing? FindClosestReachable(Pawn pawn, Predicate<Thing> validator) =>
#if !v1_5
        GenClosest.ClosestThing_Global_Reachable(
#else
        GenClosest.ClosestThing_Global_Reachable_NewTemp(
#endif
            pawn.Position,
            pawn.Map,
            pawn.Map.listerThings.AllThings,
            PathEndMode.Touch,
            TraverseParms.For(pawn),
            validator: validator,
            canLookInHaulableSources: true);

    internal void EnsureAgitationCell(Pawn pawn, IntVec3 targetPos)
    {
        if (agitationCell.IsValid) return;
        agitationCell = FindAgitationCell(pawn, targetPos);
    }

    private static readonly HashSet<RoomRoleDef> AgitationPrivateRoles =
    [
        RoomRoleDefOf.Bedroom, RoomRoleDefOf.Barracks,
        RoomRoleDefOf.PrisonCell, RoomRoleDefOf.PrisonBarracks,
        RoomRoleDefOf.Hospital, RoomRoleDefOf.Workshop, RoomRoleDefOf.Laboratory,
    ];

    private static IntVec3 FindAgitationCell(Pawn pawn, IntVec3 targetPos)
    {
        // Pick the indoor public room with the most people; outdoor rooms excluded entirely.
        var bestGroup = pawn.Map.mapPawns.AllPawnsSpawned
            .Where(p => p != pawn && p.RaceProps.Humanlike && !p.Dead)
            .GroupBy(p => p.GetRoom())
            .Where(g => g.Key != null
                && g.Key.ProperRoom
                && !g.Key.PsychologicallyOutdoors
                && !AgitationPrivateRoles.Contains(g.Key.Role))
            .MaxByWithFallback(g => g.Count());

        if (bestGroup != null)
        {
            bestGroup.Key!.Cells
                .Where(c => c.Standable(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly))
                .TryRandomElement(out var cell);
            if (cell.IsValid) return cell;
        }

        // No populated indoor room — try any reachable public indoor room.
        pawn.Map.regionGrid.AllRooms
            .Where(r => r != null && r.ProperRoom && !r.PsychologicallyOutdoors
                        && !AgitationPrivateRoles.Contains(r.Role))
            .TryMinBy(r => r.Cells.Min(c => c.DistanceToSquared(pawn.Position)), out var anyRoom);

        if (anyRoom != null)
        {
            anyRoom.Cells
                .Where(c => c.Standable(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly))
                .TryRandomElement(out var cell);
            if (cell.IsValid) return cell;
        }

        // True last resort: outdoor burn at book position.
        return targetPos;
    }

    internal static bool IsIdeoligicalStructure(Thing t) =>
        t is ThingWithComps twc
        && t.def.category == ThingCategory.Building
        && !t.def.IsFrame
        && (t.def.isAltar || twc.compStyleable?.SourcePrecept != null);
}
