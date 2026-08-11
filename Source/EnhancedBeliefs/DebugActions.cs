using LudeonTK;

namespace EnhancedBeliefs;

internal static class DebugActions
{
    // Force a precept debate from the clicked pawn against the nearest visible humanlike with a debatable issue,
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
                && !pawn.DevelopmentalStage.Baby()
                && GenSight.LineOfSight(initiator.Position, pawn.Position, initiator.Map))
            .OrderBy(pawn => pawn.Position.DistanceToSquared(initiator.Position))
            .FirstOrDefault();

        if (recipient == null)
        {
            Messages.Message($"No visible humanlike for {initiator.LabelShort} to debate.",
                MessageTypeDefOf.RejectInput, false);
            return;
        }

        var worker = (InteractionWorker_IdeologicalDebatePrecept)EnhancedBeliefsDefOf.EB_IdeologicalDebatePrecept.Worker;
        var extraSentencePacks = new List<RulePackDef>();
        worker.Interacted(initiator, recipient, extraSentencePacks, out var letterText, out var letterLabel, out var letterDef, out var lookTargets);

        var logEntry = new PlayLogEntry_DebateInteraction(EnhancedBeliefsDefOf.EB_IdeologicalDebatePrecept, initiator, recipient, extraSentencePacks, worker.topic, worker.lastWinner);
        Find.PlayLog.Add(logEntry);
        if (letterDef != null)
        {
            var logText = logEntry.ToGameStringFromPOV(initiator);
            var text = letterText.NullOrEmpty() ? logText : logText + "\n\n" + letterText;
            Find.LetterStack.ReceiveLetter(letterLabel, text, letterDef, lookTargets ?? new LookTargets(initiator, recipient));
        }

        string outcome;
        if (worker.topic == null)
        {
            outcome = "no conflicting issue found";
        }
        else if (worker.lastWinner == null)
        {
            outcome = $"drew on {worker.topic.LabelCap}";
        }
        else
        {
            var conviction = letterDef != null ? " → converted!" : "";
            outcome = $"{worker.lastWinner.LabelShort} out-argued {worker.lastLoser!.LabelShort} on {worker.topic.LabelCap}{conviction}";
        }
        Messages.Message($"{initiator.LabelShort} vs {recipient.LabelShort}: {outcome}",
            new LookTargets(initiator, recipient), MessageTypeDefOf.NeutralEvent, false);
    }
}
