using LudeonTK;

namespace EnhancedBeliefs;

internal static class DebugActions
{
    // Force a precept debate from the clicked pawn against the nearest humanlike of a different ideoligion,
    // then report the topic and whether it flipped anyone. Drives the R2 stance write-path on demand instead
    // of waiting for the interaction to fire naturally.
    [DebugAction("Ideoligion", "Trigger precept debate", actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
    private static void TriggerPreceptDebate(Pawn initiator)
    {
        if (initiator.Ideo == null)
        {
            Messages.Message($"{initiator.LabelShort} has no ideoligion.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        var recipient = initiator.Map?.mapPawns.AllPawnsSpawned
            .Where(pawn => pawn != initiator && pawn.RaceProps.Humanlike && pawn.Ideo != null
                && pawn.Ideo != initiator.Ideo && !pawn.DevelopmentalStage.Baby())
            .OrderBy(pawn => pawn.Position.DistanceToSquared(initiator.Position))
            .FirstOrDefault();

        if (recipient == null)
        {
            Messages.Message($"No nearby humanlike of a different ideoligion for {initiator.LabelShort} to debate.",
                MessageTypeDefOf.RejectInput, false);
            return;
        }

        var worker = (InteractionWorker_IdeologicalDebatePrecept)EnhancedBeliefsDefOf.EB_IdeologicalDebatePrecept.Worker;
        worker.Interacted(initiator, recipient, [], out var letterText, out var letterLabel, out var letterDef, out var lookTargets);

        if (letterText != null)
        {
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, letterDef ?? LetterDefOf.NeutralEvent,
                lookTargets ?? new LookTargets(recipient, initiator));
        }

        var outcome = worker.topic == null
            ? "found no conflicting issue"
            : letterText != null ? $"clashed on {worker.topic.LabelCap} → conversion!" : $"clashed on {worker.topic.LabelCap}";
        Messages.Message($"{initiator.LabelShort} vs {recipient.LabelShort}: {outcome}",
            new LookTargets(initiator, recipient), MessageTypeDefOf.NeutralEvent, false);
    }
}
