namespace EnhancedIdeology;

// Rules for debates inside a caravan, where vanilla runs no social interactions. Caravan_TickInterval_Debates
// starts the debates; the debate workers and DebateOnlookers call back here for the parts that differ off-map.
internal static class CaravanDebates
{
    // A caravan rests whenever it stops, so there is no fixed night. Pawns with this much rest count as awake
    // around the camp: they can debate, and they hear the debates of others.
    internal const float MinRestLevel = 0.28f;

    internal static bool IsAlert(Pawn pawn) => pawn.needs.rest == null || pawn.needs.rest.CurLevel >= MinRestLevel;

    // A caravan camps close together, so every alert member hears a debate in their caravan.
    internal static IEnumerable<Pawn> Onlookers(Pawn winner, Pawn loser)
    {
        var caravan = winner.GetCaravan();
        if (caravan == null || loser.GetCaravan() != caravan)
            return [];

        return caravan.PawnsListForReading.Where(pawn => pawn != winner && pawn != loser
            && pawn.RaceProps.Humanlike
            && !pawn.DevelopmentalStage.Baby()
            && pawn.Ideo != null
            && !pawn.Downed
            && IsAlert(pawn));
    }

    // A fight is not simulated off-map. Both pawns get the memory that vanilla gives at the end of a social fight.
    internal static void SettleFight(Pawn initiator, Pawn recipient)
    {
        if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
            Messages.Message("EnhancedIdeology.IdeologicalDebateOutcomeSocialFight".Translate(
                initiator.Named("PAWN1"), recipient.Named("PAWN2")), initiator, MessageTypeDefOf.ThreatSmall);

        GainFightMemory(initiator, recipient);
        GainFightMemory(recipient, initiator);
    }

    private static void GainFightMemory(Pawn pawn, Pawn other)
    {
        var thoughtDef = Rand.Value < 0.5f ? ThoughtDefOf.HadAngeringFight : ThoughtDefOf.HadCatharticFight;
        pawn.needs.mood?.thoughts.memories.TryGainMemory(thoughtDef, other);
    }
}
