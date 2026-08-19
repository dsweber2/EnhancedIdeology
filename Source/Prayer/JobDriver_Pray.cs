using Verse.AI;

namespace EnhancedIdeology;

[HotSwappable]
internal sealed class JobDriver_Pray : JobDriver
{
    private const TargetIndex PewInd = TargetIndex.A;
    private const TargetIndex AltarInd = TargetIndex.B;

    private const int ReinforcementIntervalTicks = GenDate.TicksPerHour;
    private const int SymbolMoteIntervalTicks = 90;
    internal const float PrayerArc = 0.5f;
    private const float ImpressivenessStageMax = 6f;

    // Exposed for tests: the diminishing-returns factor as conviction approaches its absolute ceiling.
    internal static float StrengthFactor(float strength) =>
        1f - (strength / IdeoTrackerData.AbsoluteMaxConvictionStrength);

    private LocalTargetInfo Pew => job.GetTarget(PewInd);
    private LocalTargetInfo Altar => job.GetTarget(AltarInd);

    // Pew is a reliquary Thing → one-pawn reservation, InteractionCell path.
    private bool IsReliquaryPrayer =>
        Pew.HasThing && Pew.Thing.def == ThingDefOf.Reliquary;

    // Moral guide praying at a lectern → single-pawn, InteractionCell path.
    private bool IsLecternPrayer =>
        Pew.HasThing && Pew.Thing.def == ThingDefOf.Lectern;

    // Pew is a room cell, Altar is a non-altar ideo building → shared cap reservation on the statue.
    private bool IsStatuePrayer =>
        !Pew.HasThing && Altar.HasThing && !Altar.Thing.def.isAltar;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (!pawn.Reserve(Pew, job, 1, -1, null, errorOnFailed))
            return false;
        if (IsStatuePrayer)
            return pawn.Reserve(Altar, job, JoyGiver_Prayer.StatuePrayerCap(Altar.Thing), 1, null, errorOnFailed);
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

        var pray = ToilMaker.MakeToil("Pray");
        pray.socialMode = RandomSocialMode.Off;
        pray.defaultCompleteMode = ToilCompleteMode.Delay;
        pray.defaultDuration = job.def.joyDuration;
        pray.handlingFacing = true;

        pray.initAction = delegate
        {
            if (Altar.IsValid)
                pawn.rotationTracker.FaceCell(Altar.Cell);
        };

        pray.FailOn(() => !MeditationUtility.CanMeditateNow(pawn));
        pray.AddPreTickAction(PrayTick);

        yield return pray;
    }

    private const float NeedFillPerTick = 1f / GenDate.TicksPerHour;

    private void PrayTick()
    {
        if (Altar.IsValid)
            pawn.rotationTracker.FaceCell(Altar.Cell);

        if (pawn.IsHashIntervalTick(SymbolMoteIntervalTicks) && pawn.Ideo != null)
            SpawnPrayerIcon(pawn);

        pawn.needs?.TryGetNeed<Need_Prayer>()?.Satisfy(NeedFillPerTick);

        if (pawn.needs?.joy != null)
        {
            JoyUtility.JoyTickCheckEnd(pawn, 1, JoyTickFullJoyAction.None);
            if (pawn.needs.joy.CurLevelPercentage >= 1f)
            {
                CompletePrayer();
                EndJobWith(JobCondition.Succeeded);
                return;
            }
        }

        if (pawn.IsHashIntervalTick(ReinforcementIntervalTicks))
            TryReinforceBeliefs();
    }

    private void CompletePrayer()
    {
        if (pawn.Ideo == null)
            return;
        Find.HistoryEventsManager.RecordEvent(
            new HistoryEvent(EnhancedIdeologyDefOf.EB_Prayed, pawn.Named(HistoryEventArgsNames.Doer)));
    }

    private static void SpawnPrayerIcon(Pawn pawn)
    {
        if (!pawn.Position.ShouldSpawnMotesAt(pawn.Map))
            return;
        var mote = (Mote_PrayerIcon)ThingMaker.MakeThing(EnhancedIdeologyDefOf.EB_Mote_PrayerIcon);
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

        var strengthFactor = 1f - (stance.strength / IdeoTrackerData.AbsoluteMaxConvictionStrength);
        var room = pawn.Position.GetRoom(pawn.Map);
        var impressivenessFactor = ImpressivenessScore(room);
        if (IsLecternPrayer)
            impressivenessFactor = Math.Max(impressivenessFactor, 0.5f);
        var fellowFactor = 1f + (FellowPrayerCount(pawn, room) * 0.1f);
        var chance = strengthFactor * impressivenessFactor * fellowFactor;

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers,
            $"Prayer check: {pawn} on {issue} str={stance.strength:F1} chance={chance:F3} (str={strengthFactor:F2} impress={impressivenessFactor:F2} fellows={fellowFactor:F2})");

        if (Rand.Value > chance)
            return;

        var targetRank = IdeoTrackerData.HeldRank(pawn.Ideo, issue);
        var arc = PrayerArc * ReliquaryArcMultiplier();
        ConvictionMath.ApplyRitualPull(comp, pawn, issue, targetRank, IdeoTrackerData.AbsoluteMaxConvictionStrength, arc);

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers,
            $"Prayer reinforced: {pawn} on {issue} toward rank {targetRank:F2}");
    }

    private float ReliquaryArcMultiplier()
    {
        if (IsLecternPrayer)
            return 1.5f;
        if (!IsReliquaryPrayer)
            return 1f;
        var container = Pew.Thing.TryGetComp<CompRelicContainer>();
        if (container?.ContainedThing?.StyleSourcePrecept is Precept_Relic rp && rp.ideo == pawn.Ideo)
            return 4f;
        return 2f;
    }

    private static float ImpressivenessScore(Room? room)
    {
        if (room == null || room.PsychologicallyOutdoors)
            return 0f;
        var stageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
        return stageIndex / ImpressivenessStageMax;
    }

    private static int FellowPrayerCount(Pawn pawn, Room? room)
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
