using Verse.AI;

namespace EnhancedIdeology;

internal readonly record struct ContemplationSite(
    LocalTargetInfo Seat,
    LocalTargetInfo Altar,
    float EffectiveGain
);

internal enum ContemplationSiteKind { Pew, Reliquary, Lectern }

internal sealed class CompProperties_ContemplationSite : CompProperties
{
    public ContemplationSiteKind siteKind = ContemplationSiteKind.Pew;
    public CompProperties_ContemplationSite() => compClass = typeof(Comp_ContemplationSite);
}

internal sealed class Comp_ContemplationSite : ThingComp
{
    private CompProperties_ContemplationSite Props => (CompProperties_ContemplationSite)props;

    internal float GetArcMultiplier(Pawn pawn) => Props.siteKind switch
    {
        ContemplationSiteKind.Lectern => JoyGiver_Contemplation.IsMoralist(pawn) ? 1.5f : 0f,
        ContemplationSiteKind.Reliquary => GetReliquaryMultiplier(pawn),
        _ => 1f
    };

    private float GetReliquaryMultiplier(Pawn pawn)
    {
        var container = parent.TryGetComp<CompRelicContainer>();
        if (container?.ContainedThing?.StyleSourcePrecept is Precept_Relic rp && rp.ideo == pawn.Ideo)
            return 4f;
        return 2f;
    }

    // Arc/hr for this site for a specific pawn, excluding fellow-worshipper bonus (dynamic).
    internal float GetEffectiveGain(Pawn pawn)
    {
        var arc = GetArcMultiplier(pawn);
        if (arc <= 0f) return 0f;
        return arc
            * JoyGiver_Contemplation.ImpressivenessScore(parent.GetRoom())
            * JoyGiver_Contemplation.AvgStrengthFactor(pawn)
            * JobDriver_Pray.ContemplationArc;
    }

    internal ContemplationSite? TryGetSite(Pawn pawn)
    {
        if (parent.IsForbidden(pawn)) return null;
        if (GetArcMultiplier(pawn) <= 0f) return null;
        return Props.siteKind switch
        {
            ContemplationSiteKind.Lectern => TryGetLecternSite(pawn),
            ContemplationSiteKind.Reliquary => TryGetReliquarySite(pawn),
            _ => TryGetPewSite(pawn)
        };
    }

    private ContemplationSite? TryGetLecternSite(Pawn pawn)
    {
        if (!pawn.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.None))
            return null;
        return new ContemplationSite(parent, parent, GetEffectiveGain(pawn));
    }

    private ContemplationSite? TryGetReliquarySite(Pawn pawn)
    {
        if (!pawn.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.None))
            return null;
        var room = parent.GetRoom();
        // Usable in a worship room, or in any room with impressiveness > 60.
        if (room?.Role != RoomRoleDefOf.WorshipRoom
            && (room == null || room.GetStat(RoomStatDefOf.Impressiveness) <= 60f))
            return null;
        return new ContemplationSite(parent, parent, GetEffectiveGain(pawn));
    }

    private ContemplationSite? TryGetPewSite(Pawn pawn)
    {
        var room = parent.GetRoom();
        if (room == null || room.PsychologicallyOutdoors || room.Role != RoomRoleDefOf.WorshipRoom)
            return null;
        var hasMatchingAltar = room.ContainedAndAdjacentThings
            .Any(t => t.def.isAltar && t is ThingWithComps twc && twc.compStyleable?.SourcePrecept?.ideo == pawn.Ideo);
        if (!hasMatchingAltar)
            return null;
        var altar = JoyGiver_Contemplation.FindAltarForPawn(room, pawn);
        foreach (var cell in parent.OccupiedRect().InRandomOrder())
        {
            if (pawn.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.None))
                return new ContemplationSite(new LocalTargetInfo(cell), altar, GetEffectiveGain(pawn));
        }
        return null;
    }

    private string BaseMultiplierLabel() => Props.siteKind switch
    {
        ContemplationSiteKind.Lectern => "1.5×",
        ContemplationSiteKind.Reliquary => "2× (4× with matching relic)",
        _ => "1×"
    };

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
        var pawn = Find.Selector.SelectedPawns.FirstOrDefault(pp => pp.Ideo != null);
        var value = pawn != null
            ? GetEffectiveGain(pawn).ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "/hr"
            : BaseMultiplierLabel();
        yield return new StatDrawEntry(
            StatCategoryDefOf.Building,
            "EB_ContemplationSiteStatLabel".Translate(),
            value,
            "EB_ContemplationSiteStatReport".Translate(),
            970);
    }

    public override string? CompInspectStringExtra()
    {
        var pawn = Find.Selector.SelectedPawns.FirstOrDefault(pp => pp.Ideo != null);
        var label = pawn != null
            ? GetEffectiveGain(pawn).ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "/hr"
            : BaseMultiplierLabel();
        return "EB_ContemplationSiteInspect".Translate(label);
    }
}
