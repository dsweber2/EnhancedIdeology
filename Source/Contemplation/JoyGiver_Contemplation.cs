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
        return FindSites(pawn).Count > 0;
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

    // Normalized impressiveness (0–1) for a room, used in gain calculations.
    internal static float ImpressivenessScore(Room? room)
    {
        if (room == null || room.PsychologicallyOutdoors)
            return 0f;
        return RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness)) / 6f;
    }

    // Average conviction strength factor across all moral issues for a pawn.
    internal static float AvgStrengthFactor(Pawn pawn)
    {
        if (pawn.Ideo == null) return 0f;
        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        if (comp == null) return 0f;
        var tracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var moralIssues = pawn.Ideo.precepts
            .Where(pp => pp.def.issue != null && PreceptPolicy.CategoryOf(pp.def.issue) == PreceptCategory.Moral)
            .Select(pp => pp.def.issue)
            .Distinct()
            .ToList();
        if (moralIssues.Count == 0) return 0f;
        var stances = moralIssues
            .Select(issue => tracker.IssueStances().FirstOrDefault(ss => ss.issue == issue))
            .Where(ss => ss.issue != null)
            .ToList();
        return stances.Count > 0
            ? stances.Average(ss => 1f - ss.strength / ConvictionScale.AbsoluteMaxConvictionStrength)
            : 0f;
    }

    // Finds an altar in the room for the pawn to face.
    // Prefers the pawn's own ideo; falls back to any altar (guests, cross-ideo visitors).
    internal static LocalTargetInfo FindAltarForPawn(Room room, Pawn pawn)
    {
        LocalTargetInfo? any = null;
        foreach (var thing in room.ContainedAndAdjacentThings)
        {
            if (!thing.def.isAltar) continue;
            if (thing is ThingWithComps twc && twc.compStyleable?.SourcePrecept?.ideo == pawn.Ideo)
                return thing;
            any ??= thing;
        }
        return any ?? LocalTargetInfo.Invalid;
    }

    // Returns all reachable contemplation sites for the pawn, ordered by effective gain descending.
    internal static List<ContemplationSite> FindSites(Pawn pawn)
    {
        var sites = new List<ContemplationSite>();
        if (pawn.Ideo == null || pawn.Map == null)
            return sites;

        foreach (var room in pawn.Map.regionGrid.AllRooms.ToList())
        {
            if (room.PsychologicallyOutdoors) continue;
            foreach (var thing in room.ContainedAndAdjacentThings.ToList())
            {
                var comp = thing.TryGetComp<Comp_ContemplationSite>();
                if (comp == null) continue;
                var site = comp.TryGetSite(pawn);
                if (site.HasValue) sites.Add(site.Value);
            }
        }

        // Non-altar ideo buildings (statues, monoliths, etc.) — no comp, scanned separately.
        var statueSite = FindStatueContemplationSite(pawn);
        if (statueSite.HasValue)
        {
            var room = statueSite.Value.building.GetRoom();
            var gain = ImpressivenessScore(room) * AvgStrengthFactor(pawn) * JobDriver_Pray.ContemplationArc;
            sites.Add(new ContemplationSite(statueSite.Value.cell, statueSite.Value.building, gain));
        }

        // Private room fallback: 0.25× arc, no altar to face.
        var ownedRoom = pawn.ownership?.OwnedRoom;
        if (ownedRoom != null && !ownedRoom.PsychologicallyOutdoors && ownedRoom.Role != RoomRoleDefOf.WorshipRoom)
        {
            var cell = ownedRoom.Cells
                .Where(c => c.Standable(pawn.Map) && !c.IsForbidden(pawn)
                    && pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None))
                .RandomElementWithFallback(IntVec3.Invalid);
            if (cell.IsValid)
            {
                var gain = 0.25f * ImpressivenessScore(ownedRoom) * AvgStrengthFactor(pawn) * JobDriver_Pray.ContemplationArc;
                sites.Add(new ContemplationSite(new LocalTargetInfo(cell), LocalTargetInfo.Invalid, gain));
            }
        }

        sites.Sort((aa, bb) => bb.EffectiveGain.CompareTo(aa.EffectiveGain));
        return sites;
    }

    private static readonly Dictionary<Pawn, int> noPewWarnedAt = [];
    private static readonly Dictionary<Pawn, int> disrespectedWarnedAt = [];

    public override Job? TryGiveJob(Pawn pawn)
    {
        if (pawn.Ideo == null || pawn.Map == null)
            return null;

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers,
            $"Contemplation site search: {pawn} moralist={IsMoralist(pawn)}");

        var job = TryBuildPrayJob(pawn);
        if (job != null)
            return job;

        // Suppress warnings for guests (they can't own worship rooms).
        if (!pawn.IsColonist)
            return null;

        var worshipRoom = FindWorshipRoomWithValidAltar(pawn);
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
        else if (HasWorshipRoomAnyAltar(pawn))
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

    // Core site-finding logic; shared with JobGiver_PrayFromNeed. No warnings emitted.
    internal static Job? TryBuildPrayJob(Pawn pawn)
    {
        var sites = FindSites(pawn);
        if (sites.Count == 0) return null;
        var best = WeightedRandom(sites);
        return JobMaker.MakeJob(EnhancedIdeologyDefOf.EB_Pray, best.Seat, best.Altar);
    }

    private static ContemplationSite WeightedRandom(List<ContemplationSite> sites)
    {
        var total = sites.Sum(ss => ss.EffectiveGain);
        if (total <= 0f) return sites[0];
        var roll = Rand.Value * total;
        var cumulative = 0f;
        foreach (var site in sites)
        {
            cumulative += site.EffectiveGain;
            if (roll <= cumulative) return site;
        }
        return sites[^1];
    }

    // Non-altar ideo buildings (statues, monoliths): scanned across all rooms.
    internal static (Thing building, LocalTargetInfo cell)? FindStatueContemplationSite(Pawn pawn)
    {
        foreach (var room in pawn.Map.regionGrid.AllRooms.ToList())
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

    // Max concurrent contemplations at a statue, scaled by room impressiveness.
    internal static int StatueContemplationCap(Thing thing)
    {
        var room = thing.GetRoom();
        if (room == null || room.PsychologicallyOutdoors)
            return 1;
        return 1 + (int)RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
    }

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

    // Worship room where the pawn's altar's requirements are met (for warning logic only).
    private static Room? FindWorshipRoomWithValidAltar(Pawn pawn)
    {
        foreach (var room in pawn.Map.regionGrid.AllRooms.ToList())
        {
            if (!room.PsychologicallyOutdoors
                && room.Role == RoomRoleDefOf.WorshipRoom
                && RoomHasValidAltar(room, pawn))
                return room;
        }
        return null;
    }

    // Any worship room that contains at least one altar styled for the pawn's ideo,
    // regardless of room requirements (for warning logic only).
    private static bool HasWorshipRoomAnyAltar(Pawn pawn)
    {
        foreach (var room in pawn.Map.regionGrid.AllRooms.ToList())
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

    private static bool RoomHasValidAltar(Room room, Pawn pawn)
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
}
