using Verse.AI;

namespace EnhancedIdeology;

[HotSwappable]
internal sealed class JobDriver_Pray : JobDriver
{
    private const TargetIndex PewInd = TargetIndex.A;
    private const TargetIndex AltarInd = TargetIndex.B;

    private const int ReinforcementIntervalTicks = GenDate.TicksPerHour;
    private const int SymbolMoteIntervalTicks = 90;
    internal const float ContemplationArc = 0.5f;
    // Exposed for tests: the diminishing-returns factor as conviction approaches its absolute ceiling.
    internal static float StrengthFactor(float strength) =>
        1f - (strength / ConvictionScale.AbsoluteMaxConvictionStrength);

    private LocalTargetInfo Pew => job.GetTarget(PewInd);
    private LocalTargetInfo Altar => job.GetTarget(AltarInd);

    // Pew is a reliquary Thing → one-pawn reservation, InteractionCell path.
    private bool IsReliquaryContemplation =>
        Pew.HasThing && Pew.Thing.def == ThingDefOf.Reliquary;

    // Moral guide contemplating at a lectern → single-pawn, InteractionCell path.
    private bool IsLecternContemplation =>
        Pew.HasThing && Pew.Thing.def == ThingDefOf.Lectern;

    // Pew is a room cell, Altar is a non-altar ideo building → shared cap reservation on the statue.
    private bool IsStatueContemplation =>
        !Pew.HasThing && Altar.HasThing && !Altar.Thing.def.isAltar;

    // Pew is a room cell, no altar → private room contemplation.
    private bool IsPrivateRoomContemplation =>
        !Pew.HasThing && !Altar.IsValid;


    public override string GetReport()
    {
        var rate = ExpectedArcPerHour();
        if (rate <= 0f)
            return base.GetReport();
        return "EB_ContemplationActivityReport".Translate(rate.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
    }

    private float ExpectedArcPerHour()
    {
        if (pawn.Ideo == null || pawn.Map == null)
            return 0f;
        var room = pawn.Position.GetRoom(pawn.Map);
        var impressiveness = JoyGiver_Contemplation.ImpressivenessScore(room);
        if (IsLecternContemplation)
            impressiveness = Math.Max(impressiveness, 0.5f);
        var fellowFactor = 1f + (FellowContemplationCount(pawn, room) * 0.1f);
        var arc = ContemplationArc * ReliquaryArcMultiplier();
        return JoyGiver_Contemplation.AvgStrengthFactor(pawn) * impressiveness * fellowFactor * arc;
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (!pawn.Reserve(Pew, job, 1, -1, null, errorOnFailed))
            return false;
        if (IsStatueContemplation)
            return pawn.Reserve(Altar, job, JoyGiver_Contemplation.StatueContemplationCap(Altar.Thing), 1, null, errorOnFailed);
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        if (Altar.HasThing)
            this.FailOnDespawnedOrNull(AltarInd);
        if (Pew.HasThing)
            this.FailOnDespawnedOrNull(PewInd);

        var pathMode = Pew.HasThing ? PathEndMode.InteractionCell : PathEndMode.OnCell;
        yield return Toils_Goto.Goto(PewInd, pathMode);

        var contemplate = ToilMaker.MakeToil("Contemplate");
        contemplate.socialMode = RandomSocialMode.Off;
        contemplate.defaultCompleteMode = ToilCompleteMode.Delay;
        contemplate.defaultDuration = job.def.joyDuration;
        contemplate.handlingFacing = true;

        contemplate.initAction = delegate
        {
            if (Altar.IsValid)
                pawn.rotationTracker.FaceCell(Altar.Cell);
        };

        contemplate.FailOn(() => !MeditationUtility.CanMeditateNow(pawn));
        contemplate.AddPreTickAction(PrayTick);

        yield return contemplate;
    }

    private const float NeedFillPerTick = 1f / GenDate.TicksPerHour;

    private void PrayTick()
    {
        if (Altar.IsValid)
            pawn.rotationTracker.FaceCell(Altar.Cell);

        if (pawn.IsHashIntervalTick(SymbolMoteIntervalTicks) && pawn.Ideo != null)
            SpawnContemplationIcon(pawn);

        pawn.needs?.TryGetNeed<Need_Contemplation>()?.Satisfy(NeedFillPerTick);

        if (pawn.needs?.joy != null)
        {
            JoyUtility.JoyTickCheckEnd(pawn, 1, JoyTickFullJoyAction.None);
            bool inCrisis = pawn.MentalStateDef == EnhancedIdeologyDefOf.EB_CrisisOfFaith;
            if (!inCrisis && pawn.needs.joy.CurLevelPercentage >= 1f)
            {
                CompleteContemplation();
                EndJobWith(JobCondition.Succeeded);
                return;
            }
        }

        if (pawn.IsHashIntervalTick(ReinforcementIntervalTicks))
            TryReinforceBeliefs();
    }

    private void CompleteContemplation()
    {
        if (pawn.Ideo == null)
            return;
        Find.HistoryEventsManager.RecordEvent(
            new HistoryEvent(EnhancedIdeologyDefOf.EB_Contemplated, pawn.Named(HistoryEventArgsNames.Doer)));
    }

    private static void SpawnContemplationIcon(Pawn pawn)
    {
        if (!pawn.Position.ShouldSpawnMotesAt(pawn.Map))
            return;
        var mote = (Mote_ContemplationIcon)ThingMaker.MakeThing(EnhancedIdeologyDefOf.EB_Mote_ContemplationIcon);
        mote.exactPosition = pawn.DrawPos
            + new Vector3(0.35f, 0f, 0.35f)
            + new Vector3(Rand.Value, 0f, Rand.Value) * 0.1f;
        mote.Setup(pawn.Ideo.Icon, pawn.Ideo.Color);
        GenSpawn.Spawn(mote, pawn.Position, pawn.Map);
    }

    private void TryReinforceBeliefs()
    {
        if (pawn.Ideo == null || pawn.Map == null)
            return;

        var candidates = pawn.Ideo.precepts
            .Where(pp => pp.def.issue != null && PreceptPolicy.CategoryOf(pp.def.issue) == PreceptCategory.Moral)
            .ToList();
        if (candidates.Count == 0)
            return;

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var tracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var issue = candidates.RandomElement().def.issue;
        var stance = tracker.IssueStances().FirstOrDefault(ss => ss.issue == issue);
        if (stance.issue == null)
            return;

        var strengthFactor = 1f - (stance.strength / ConvictionScale.AbsoluteMaxConvictionStrength);
        var room = pawn.Position.GetRoom(pawn.Map);
        var impressivenessFactor = JoyGiver_Contemplation.ImpressivenessScore(room);
        if (IsLecternContemplation)
            impressivenessFactor = Math.Max(impressivenessFactor, 0.5f);
        var fellowFactor = 1f + (FellowContemplationCount(pawn, room) * 0.1f);
        var chance = strengthFactor * impressivenessFactor * fellowFactor;

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers,
            $"Contemplation check: {pawn} on {issue} str={stance.strength:F1} chance={chance:F3} (str={strengthFactor:F2} impress={impressivenessFactor:F2} fellows={fellowFactor:F2})");

        if (Rand.Value > chance)
            return;

        var targetRank = IssueStanceTracker.HeldRank(pawn.Ideo, issue);
        var arc = ContemplationArc * ReliquaryArcMultiplier();
        ConvictionMath.ApplyRitualPull(comp, pawn, issue, targetRank, ConvictionScale.AbsoluteMaxConvictionStrength, arc);

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers,
            $"Contemplation reinforced: {pawn} on {issue} toward rank {targetRank:F2}");
    }

    private float ReliquaryArcMultiplier()
    {
        if (IsLecternContemplation)
            return 1.5f;
        if (IsPrivateRoomContemplation)
            return 0.25f;
        if (!IsReliquaryContemplation)
            return 1f;
        var container = Pew.Thing.TryGetComp<CompRelicContainer>();
        if (container?.ContainedThing?.StyleSourcePrecept is Precept_Relic rp && rp.ideo == pawn.Ideo)
            return 4f;
        return 2f;
    }

    private static int FellowContemplationCount(Pawn pawn, Room? room)
    {
        if (room == null)
            return 0;
        var count = 0;
        foreach (var cell in room.Cells)
        {
            foreach (var thing in cell.GetThingList(pawn.Map))
            {
                if (thing is Pawn other && other != pawn
                    && other.Ideo == pawn.Ideo
                    && other.jobs?.curDriver is JobDriver_Pray)
                    count++;
            }
        }
        return count;
    }
}
