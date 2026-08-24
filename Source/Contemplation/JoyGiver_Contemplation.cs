using Verse.AI;

namespace EnhancedIdeology;

[HotSwappable]
internal sealed class JoyGiver_Contemplation : JoyGiver
{
    public override bool CanBeGivenTo(Pawn pawn)
    {
        if (!base.CanBeGivenTo(pawn) || pawn.Ideo == null || pawn.Map == null)
            return false;
        if (!MeditationUtility.CanMeditateNow(pawn))
            return false;
        if (!ContemplationAllowedByPrecept(pawn))
            return false;
        return FindLectern(pawn) != null
            || HasAnyWorshipRoom(pawn)
            || FindStatueContemplationSite(pawn) != null
            || FindAccessibleImpressiveReliquary(pawn) != null
            || FindWorshipRoomWithPew(pawn) != null;
    }

    internal static bool ContemplationAllowedByPrecept(Pawn pawn)
    {
        var precept = GetContemplationPrecept(pawn);
        if (precept == null)
            return true;

        var defName = precept.def.defName;
        if (defName == "Contemplation_Forbidden")
            return false;

        if (defName == "Contemplation_Disapproved")
        {
            var certainty = GetCertainty(pawn);
            return certainty < 0.25f;
        }

        if (defName == "Contemplation_Normal")
        {
            var certainty = GetCertainty(pawn);
            if (certainty > 0.75f)
                return Rand.Value < 0.3f;
            // Respected is ~1.5x more likely than Normal at the same certainty
            return Rand.Value < 0.65f;
        }

        return true;
    }

    private static Precept? GetContemplationPrecept(Pawn pawn)
    {
        if (pawn.Ideo == null)
            return null;
        foreach (var precept in pawn.Ideo.precepts)
        {
            if (precept.def.issue?.defName == "EB_Contemplation")
                return precept;
        }
        return null;
    }

    private static float GetCertainty(Pawn pawn)
    {
        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var tracker = comp?.PawnTracker?.EnsurePawnHasIdeoTracker(pawn);
        return tracker?.ExtendedCertainty ?? 1f;
    }

    internal static bool IsMoralist(Pawn pawn) =>
        pawn.Ideo?.GetRole(pawn)?.def == PreceptDefOf.IdeoRole_Moralist;

    internal static Thing? FindLectern(Pawn pawn)
    {
        if (!IsMoralist(pawn))
            return null;
        foreach (var thing in pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Lectern))
        {
            if (thing.IsForbidden(pawn))
                continue;
            if (pawn.CanReserveAndReach(thing, PathEndMode.InteractionCell, Danger.None))
                return thing;
        }
        return null;
    }

    // Any worship room where the pawn can sit in a pew, regardless of altar ideo.
    // Used as a fallback for guests whose ideo doesn't match any altar.
    private static Room? FindWorshipRoomWithPew(Pawn pawn)
    {
        foreach (var room in pawn.Map.regionGrid.AllRooms)
        {
            if (room.PsychologicallyOutdoors || room.Role != RoomRoleDefOf.WorshipRoom)
                continue;
            if (FindPew(room, pawn) != null)
                return room;
        }
        return null;
    }

    // Returns the first altar in the room, regardless of ideo, for facing purposes.
    private static LocalTargetInfo FindAnyAltarInRoom(Room room)
    {
        foreach (var thing in room.ContainedAndAdjacentThings)
        {
            if (thing.def.isAltar)
                return thing;
        }
        return LocalTargetInfo.Invalid;
    }

    // Loose check: has a worship room with the pawn's altar, ignoring room requirements.
    // Kept separate so TryGiveJob can distinguish disrespected vs fully missing.
    private static bool HasAnyWorshipRoom(Pawn pawn)
    {
        foreach (var room in pawn.Map.regionGrid.AllRooms)
        {
            if (room.PsychologicallyOutdoors || room.Role != RoomRoleDefOf.WorshipRoom)
                continue;
            foreach (var thing in room.ContainedAndAdjacentThings)
            {
                if (thing.def.isAltar
                    && thing is ThingWithComps twc
                    && twc.compStyleable?.SourcePrecept?.ideo == pawn.Ideo)
                    return true;
            }
        }
        return false;
    }

    private static readonly Dictionary<Pawn, int> noPewWarnedAt = [];
    private static readonly Dictionary<Pawn, int> disrespectedWarnedAt = [];

    public override Job? TryGiveJob(Pawn pawn)
    {
        if (pawn.Ideo == null || pawn.Map == null)
            return null;

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers,
            $"Contemplation site search: {pawn} moralist={IsMoralist(pawn)} lectern={FindLectern(pawn)?.Label ?? "none"}");

        var job = TryBuildPrayJob(pawn);
        if (job != null)
            return job;

        // Nothing usable — emit the most specific message
        var worshipRoom = FindWorshipRoom(pawn);
        if (worshipRoom != null)
        {
            if (!noPewWarnedAt.TryGetValue(pawn, out var lastWarn)
                || Find.TickManager.TicksGame - lastWarn > GenDate.TicksPerDay)
            {
                noPewWarnedAt[pawn] = Find.TickManager.TicksGame;
                Messages.Message(
                    "EB_NoPewToPrayIn".Translate(pawn.Named("PAWN"), pawn.Ideo.Named("IDEO")),
                    pawn, MessageTypeDefOf.CautionInput, historical: false);
            }
        }
        else if (HasAnyWorshipRoom(pawn))
        {
            if (!disrespectedWarnedAt.TryGetValue(pawn, out var lastDisrespect)
                || Find.TickManager.TicksGame - lastDisrespect > GenDate.TicksPerDay)
            {
                disrespectedWarnedAt[pawn] = Find.TickManager.TicksGame;
                Messages.Message(
                    "EB_ContemplationRoomDisrespected".Translate(pawn.Named("PAWN"), pawn.Ideo.Named("IDEO")),
                    pawn, MessageTypeDefOf.CautionInput, historical: false);
            }
        }
        return null;
    }

    // Core site-finding logic shared with JobGiver_PrayFromNeed (no warnings emitted).
    internal static Job? TryBuildPrayJob(Pawn pawn)
    {
        var lectern = FindLectern(pawn);

        // Priority 1: worship room — reliquary first, then lectern (for moralist), then pew
        var worshipRoom = FindWorshipRoom(pawn);
        if (worshipRoom != null)
        {
            var reliquary = FindReliquary(worshipRoom, pawn);
            if (reliquary != null)
                return JobMaker.MakeJob(EnhancedIdeologyDefOf.EB_Pray, reliquary, reliquary);

            if (lectern != null)
                return JobMaker.MakeJob(EnhancedIdeologyDefOf.EB_Pray, lectern, lectern);

            var pew = FindPew(worshipRoom, pawn);
            if (pew != null)
            {
                var altar = FindContemplationTarget(worshipRoom, pawn);
                return JobMaker.MakeJob(EnhancedIdeologyDefOf.EB_Pray, pew.Value, altar);
            }
        }

        // Priority 2: lectern when there's no worship room at all
        if (lectern != null)
            return JobMaker.MakeJob(EnhancedIdeologyDefOf.EB_Pray, lectern, lectern);

        // Priority 3: statue/ideo building in any room
        var statueSite = FindStatueContemplationSite(pawn);
        if (statueSite != null)
            return JobMaker.MakeJob(EnhancedIdeologyDefOf.EB_Pray, statueSite.Value.cell, statueSite.Value.building);

        // Priority 4: reliquary in any sufficiently impressive room
        var impressiveReliquary = FindAccessibleImpressiveReliquary(pawn);
        if (impressiveReliquary != null)
            return JobMaker.MakeJob(EnhancedIdeologyDefOf.EB_Pray, impressiveReliquary, impressiveReliquary);

        // Priority 5: any worship room with a pew (guests whose ideo doesn't match any altar)
        var guestRoom = FindWorshipRoomWithPew(pawn);
        if (guestRoom != null)
        {
            var guestPew = FindPew(guestRoom, pawn);
            if (guestPew != null)
            {
                var altar = FindAnyAltarInRoom(guestRoom);
                return JobMaker.MakeJob(EnhancedIdeologyDefOf.EB_Pray, guestPew.Value, altar);
            }
        }

        return null;
    }

    internal static Room? FindWorshipRoom(Pawn pawn)
    {
        foreach (var room in pawn.Map.regionGrid.AllRooms)
        {
            if (!room.PsychologicallyOutdoors
                && room.Role == RoomRoleDefOf.WorshipRoom
                && RoomHasValidAltar(room, pawn))
                return room;
        }
        return null;
    }

    internal static bool RoomHasValidAltar(Room room, Pawn pawn)
    {
        foreach (var thing in room.ContainedAndAdjacentThings)
        {
            if (!thing.def.isAltar || thing is not ThingWithComps twc)
                continue;
            if (twc.compStyleable?.SourcePrecept?.ideo != pawn.Ideo)
                continue;
            if (twc.compStyleable.SourcePrecept is not Precept_Building pb)
                continue;
            var demand = pb.presenceDemand;
            if (demand == null || !demand.AppliesTo(pawn.Map))
                return true;
            var effectiveRoom = demand.GetEffectiveRoom(thing);
            if (effectiveRoom == null)
                continue;
            if (demand.roomRequirements.NullOrEmpty())
                return true;
            if (demand.roomRequirements.All(r => r.MetOrDisabled(effectiveRoom, pawn)))
                return true;
        }
        return false;
    }

    // Non-altar ideo buildings (statues, etc.) in any room, subject to their own presenceDemand.
    // Returns the building + a free cell in the room. The building is reserved with a cap based on
    // impressiveness so the number of simultaneous contemplations scales with the room's quality.
    internal static (Thing building, LocalTargetInfo cell)? FindStatueContemplationSite(Pawn pawn)
    {
        foreach (var room in pawn.Map.regionGrid.AllRooms)
        {
            if (room.PsychologicallyOutdoors)
                continue;
            foreach (var thing in room.ContainedAndAdjacentThings)
            {
                if (thing.def.isAltar || thing is not ThingWithComps twc)
                    continue;
                if (twc.compStyleable?.Ideo != pawn.Ideo)
                    continue;
                if (twc.compStyleable.SourcePrecept is Precept_Building pb && pb.presenceDemand != null)
                {
                    var demand = pb.presenceDemand;
                    if (demand.AppliesTo(pawn.Map))
                    {
                        var effectiveRoom = demand.GetEffectiveRoom(thing);
                        if (effectiveRoom != null
                            && !demand.roomRequirements.NullOrEmpty()
                            && !demand.roomRequirements.All(r => r.MetOrDisabled(effectiveRoom, pawn)))
                            continue;
                    }
                }
                if (thing.IsForbidden(pawn))
                    continue;
                var cap = StatueContemplationCap(thing);
                if (!pawn.CanReserve(thing, cap, 1))
                    continue;
                var cell = FindRoomCell(room, thing, pawn);
                if (cell != null)
                    return (thing, cell.Value);
            }
        }
        return null;
    }

    // Max concurrent contemplations at a statue = 1 + impressiveness stage (0-6), so 1–7 pawns.
    internal static int StatueContemplationCap(Thing thing)
    {
        var room = thing.GetRoom();
        if (room == null || room.PsychologicallyOutdoors)
            return 1;
        return 1 + (int)RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
    }

    // Picks the nearest free cell in the room to the focus building, closest-first.
    private static LocalTargetInfo? FindRoomCell(Room room, Thing focus, Pawn pawn)
    {
        var focusPos = focus.Position;
        var occupied = focus.OccupiedRect();
        foreach (var cell in room.Cells.OrderBy(c => (c - focusPos).LengthHorizontalSquared))
        {
            if (occupied.Contains(cell))
                continue;
            if (pawn.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.None))
                return new LocalTargetInfo(cell);
        }
        return null;
    }

    // Reliquary in any room with impressiveness > 60 (single-pawn, Thing reservation).
    internal static Thing? FindAccessibleImpressiveReliquary(Pawn pawn)
    {
        foreach (var room in pawn.Map.regionGrid.AllRooms)
        {
            if (room.PsychologicallyOutdoors)
                continue;
            if (room.GetStat(RoomStatDefOf.Impressiveness) <= 60f)
                continue;
            foreach (var thing in room.ContainedAndAdjacentThings)
            {
                if (thing.def != ThingDefOf.Reliquary)
                    continue;
                if (!thing.IsForbidden(pawn) && pawn.CanReserveAndReach(thing, PathEndMode.InteractionCell, Danger.None))
                    return thing;
            }
        }
        return null;
    }

    internal static Thing? FindReliquary(Room room, Pawn pawn)
    {
        foreach (var thing in room.ContainedAndAdjacentThings)
        {
            if (thing.def != ThingDefOf.Reliquary)
                continue;
            if (!thing.IsForbidden(pawn) && pawn.CanReserveAndReach(thing, PathEndMode.InteractionCell, Danger.None))
                return thing;
        }
        return null;
    }

    // Returns a specific unreserved cell on a ritual seat so multiple pawns can use the same bench.
    internal static LocalTargetInfo? FindPew(Room room, Pawn pawn)
    {
        var seatDef = pawn.Ideo?.RitualSeatDef;
        foreach (var thing in room.ContainedAndAdjacentThings)
        {
            if (seatDef != null && thing.def != seatDef)
                continue;
            if (seatDef == null && thing.TryGetComp<CompRitualSeat>() == null)
                continue;
            if (thing.IsForbidden(pawn))
                continue;
            foreach (var cell in thing.OccupiedRect())
            {
                if (pawn.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.None))
                    return new LocalTargetInfo(cell);
            }
        }
        return null;
    }

    // Prefer altars; fall back to any ideo building in the room to face during contemplation.
    internal static LocalTargetInfo FindContemplationTarget(Room room, Pawn pawn)
    {
        Thing? fallback = null;
        foreach (var thing in room.ContainedAndAdjacentThings)
        {
            if (thing is not ThingWithComps twc || twc.compStyleable?.SourcePrecept?.ideo != pawn.Ideo)
                continue;
            if (thing.def.isAltar)
                return thing;
            fallback ??= thing;
        }
        return fallback ?? LocalTargetInfo.Invalid;
    }
}
