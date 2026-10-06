using LudeonTK;

namespace EnhancedIdeology;

internal static partial class DebugActions
{
    [DebugAction("Ideoligion", "Reset stances to orthodox", actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
    private static void ResetStancesToOrthodox(Pawn pawn)
    {
        if (pawn.Ideo == null)
        {
            Messages.Message($"{pawn.LabelShort} has no ideoligion.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        Current.Game.GetComponent<GameComponent_EnhancedIdeology>().PawnTracker
            .EnsurePawnHasIdeoTracker(pawn).ResetStancesToOrthodox();
        Messages.Message($"{pawn.LabelShort}: stances reset to {pawn.Ideo.name}.", MessageTypeDefOf.TaskCompletion, false);
    }

    [DebugAction("Ideoligion", "Reset stances to orthodox (all pawns)", actionType = DebugActionType.Action,
        allowedGameStates = AllowedGameStates.Playing, requiresIdeology = true)]
    private static void ResetAllStancesToOrthodox()
    {
        var count = 0;
        foreach (var (pawn, tracker) in Current.Game.GetComponent<GameComponent_EnhancedIdeology>().PawnTracker)
        {
            if (pawn.Ideo == null) continue;
            tracker.ResetStancesToOrthodox();
            count++;
        }
        Messages.Message($"Reset stances to orthodox for {count} pawns.", MessageTypeDefOf.TaskCompletion, false);
    }
}
