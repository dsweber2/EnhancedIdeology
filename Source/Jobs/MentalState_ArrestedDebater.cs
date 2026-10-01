using Verse.AI;

namespace EnhancedIdeology;

internal sealed class MentalState_ArrestedDebater : MentalState
{
    private const int DebateIntervalTicks = 200;
    private const float DebateSearchRadius = 10f;

    public override bool AllowRestingInBed => false;

    public override void MentalStateTick(int delta)
    {
        if (pawn.IsHashIntervalTick(DebateIntervalTicks, delta))
        {
            var target = FindDebateTarget();
            if (target != null)
            {
                var def = EnhancedIdeologyDefOf.EB_IdeologicalDebatePrecept;
                if (pawn.Spawned)
                {
                    pawn.interactions.TryInteractWith(target, def);
                }
                else
                {
                    // Call Interacted() directly so the carrier can be a valid target without
                    // triggering a self-interaction. The bubble appears at the carrier's position.
                    def.Worker.Interacted(pawn, target, [], out _, out _, out _, out _);
                    var carrier = pawn.ParentHolder as Pawn_CarryTracker;
                    if (carrier != null)
                        MoteMaker.MakeInteractionBubble(carrier.pawn, target,
                            def.interactionMote,
                            def.GetSymbol(pawn.Faction, pawn.Ideo),
                            def.GetSymbolColor(pawn.Faction));
                }
            }
        }
        base.MentalStateTick(delta);
    }

    private Pawn? FindDebateTarget()
    {
        var map = pawn.MapHeld;
        if (map == null) return null;
        var pos = pawn.PositionHeld;

        // Exponential-key weighted sampling: key = -log(U) * (d+1) so closer pawns are
        // preferred but not deterministically.
        map.mapPawns.AllPawnsSpawned
            .Where(p => p != pawn
                && p.RaceProps.Humanlike
                && !p.DevelopmentalStage.Baby()
                && !p.Downed
                && !p.Dead
                && p.Awake()
                && p.Position.InHorDistOf(pos, DebateSearchRadius))
            .TryMinBy(p => -Mathf.Log(Rand.Value) * (p.Position.DistanceTo(pos) + 1f), out var target);
        return target;
    }
}
