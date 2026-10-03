namespace EnhancedIdeology;

// Pawns near a decisive debate hear the argument and drift toward the winner's personal stance on the debated
// issues. The pull goes through ConvictionMath.PullStance, so the winner's ConversionPower and each onlooker's
// CertaintyLossFactor already scale it.
internal static class DebateOnlookers
{
    // Range in cells from either debater inside which a pawn with line of sight hears the debate.
    internal const float HearingRadius = 8f;

    // Onlooker pull relative to an ordinary 1x stance pull. It is small because a busy rec room hears many debates.
    internal const float OnlookerPullMultiplier = 0.25f;

    // A winner from a Proselytizer faith argues to persuade the whole room, not only the opponent.
    internal const float ProselytizerOnlookerFactor = 2f;

    internal static void Sway(GameComponent_EnhancedIdeology comp, Pawn winner, Pawn loser, IEnumerable<IssueDef> issues)
    {
        var issueList = issues.ToList();
        foreach (var witness in Witnesses(winner, loser))
            SwayWitness(comp, winner, witness, issueList);
    }

    internal static void SwayWitness(GameComponent_EnhancedIdeology comp, Pawn winner, Pawn witness, IEnumerable<IssueDef> issues)
    {
        var winnerStances = comp.PawnTracker.EnsurePawnHasIdeoTracker(winner).IssueStances()
            .ToDictionary(stance => stance.issue, stance => stance.rank);
        var proselytizerFactor = winner.Ideo?.memes.Contains(EnhancedIdeologyDefOf.Proselytizer) == true ? ProselytizerOnlookerFactor : 1f;
        var pull = Compat_PeerPressure.AdjustStancePull(OnlookerPullMultiplier * proselytizerFactor, witness.relations.OpinionOf(winner));
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"DebateOnlookers: {witness} overheard {winner} win (pull {pull:F2}).");
        foreach (var issue in issues)
            ConvictionMath.PullStance(comp, winner, witness, issue, winnerStances[issue], pull);
    }

    private static List<Pawn> Witnesses(Pawn winner, Pawn loser)
    {
        if (!winner.Spawned || !loser.Spawned || winner.Map != loser.Map)
            return [];

        var map = winner.Map!;
        return [.. map.mapPawns.AllPawnsSpawned.Where(pawn => pawn != winner && pawn != loser
            && pawn.RaceProps.Humanlike
            && !pawn.DevelopmentalStage.Baby()
            && pawn.Ideo != null
            && pawn.Awake()
            && !pawn.Downed
            && (Hears(pawn, winner, map) || Hears(pawn, loser, map)))];
    }

    private static bool Hears(Pawn listener, Pawn speaker, Map map) =>
        listener.Position.InHorDistOf(speaker.Position, HearingRadius)
        && GenSight.LineOfSight(listener.Position, speaker.Position, map);
}
