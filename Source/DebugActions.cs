using LudeonTK;

namespace EnhancedIdeology;

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

        var worker = (InteractionWorker_IdeologicalDebatePrecept)EnhancedIdeologyDefOf.EB_IdeologicalDebatePrecept.Worker;
        var extraSentencePacks = new List<RulePackDef>();
        worker.Interacted(initiator, recipient, extraSentencePacks, out var letterText, out var letterLabel, out var letterDef, out var lookTargets);

        var logEntry = new PlayLogEntry_DebateInteraction(EnhancedIdeologyDefOf.EB_IdeologicalDebatePrecept, initiator, recipient, extraSentencePacks, worker.topic, worker.lastWinner, worker.lastWinnerPrecept?.label);
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

    [DebugAction("Ideoligion", "Trigger meme debate", actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
    private static void TriggerMemeDebate(Pawn initiator)
    {
        if (initiator.Ideo == null)
        {
            Messages.Message($"{initiator.LabelShort} has no ideoligion.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        var recipient = initiator.Map?.mapPawns.AllPawnsSpawned
            .Where(pawn => pawn != initiator && pawn.RaceProps.Humanlike && pawn.Ideo != null
                && pawn.Ideo != initiator.Ideo
                && !pawn.DevelopmentalStage.Baby()
                && GenSight.LineOfSight(initiator.Position, pawn.Position, initiator.Map))
            .OrderBy(pawn => pawn.Position.DistanceToSquared(initiator.Position))
            .FirstOrDefault();

        if (recipient == null)
        {
            Messages.Message($"No visible cross-ideo humanlike for {initiator.LabelShort} to debate.",
                MessageTypeDefOf.RejectInput, false);
            return;
        }

        var worker = (InteractionWorker_IdeologicalDebateMeme)EnhancedIdeologyDefOf.EB_IdeologicalDebateMeme.Worker;
        var extraSentencePacks = new List<RulePackDef>();
        worker.Interacted(initiator, recipient, extraSentencePacks, out var letterText, out var letterLabel, out var letterDef, out var lookTargets);

        var logEntry = new PlayLogEntry_DebateInteraction(EnhancedIdeologyDefOf.EB_IdeologicalDebateMeme, initiator, recipient, extraSentencePacks, worker.logTopic, worker.lastWinner, worker.logTopic?.label);
        Find.PlayLog.Add(logEntry);
        if (letterDef != null)
        {
            var logText = logEntry.ToGameStringFromPOV(initiator);
            var text = letterText.NullOrEmpty() ? logText : logText + "\n\n" + letterText;
            Find.LetterStack.ReceiveLetter(letterLabel, text, letterDef, lookTargets ?? new LookTargets(initiator, recipient));
        }

        string outcome;
        if (worker.logTopic == null)
        {
            outcome = "no meme topic selected (ideos may have no memes)";
        }
        else if (worker.lastWinner == null)
        {
            outcome = $"drew on {worker.logTopic.LabelCap}";
        }
        else
        {
            var conviction = letterDef != null ? " → converted!" : "";
            outcome = $"{worker.lastWinner.LabelShort} out-argued on {worker.logTopic.LabelCap}{conviction}";
        }
        Messages.Message($"{initiator.LabelShort} vs {recipient.LabelShort}: {outcome}",
            new LookTargets(initiator, recipient), MessageTypeDefOf.NeutralEvent, false);
    }

    [DebugAction("Ideoligion", "Trigger crisis of faith", actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
    private static void TriggerCrisisOfFaith(Pawn pawn)
    {
        if (pawn.Ideo == null || pawn.DevelopmentalStage.Baby())
        {
            Messages.Message($"{pawn.LabelShort} cannot have a crisis of faith.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var data = comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var oldIdeo = pawn.Ideo;
        var oldCertainty = pawn.ideo.Certainty;
        var moodGated = pawn.needs.mood != null
            && pawn.needs.mood.CurLevel < pawn.mindState.mentalBreaker.BreakThresholdMinor;

        data.TriggerCrisisOfFaith();

        if (moodGated)
        {
            Messages.Message($"{pawn.LabelShort}: mood too low — triggered normal break instead.",
                new LookTargets(pawn), MessageTypeDefOf.NeutralEvent, false);
        }
        else if (pawn.Ideo != oldIdeo)
        {
            Messages.Message($"{pawn.LabelShort}: crisis resolved — converted {oldIdeo.name} → {pawn.Ideo.name}, certainty {oldCertainty:P0} → {pawn.ideo.Certainty:P0}.",
                new LookTargets(pawn), MessageTypeDefOf.NeutralEvent, false);
        }
        else
        {
            Messages.Message($"{pawn.LabelShort}: crisis resolved — stayed {pawn.Ideo.name}, certainty {oldCertainty:P0} → {pawn.ideo.Certainty:P0}.",
                new LookTargets(pawn), MessageTypeDefOf.NeutralEvent, false);
        }
    }
}
