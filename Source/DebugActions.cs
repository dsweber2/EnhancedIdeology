using LudeonTK;
using Verse.AI;

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

    [DebugAction("Ideoligion", "Trigger prayer", actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
    private static void TriggerPrayer(Pawn pawn)
    {
        if (pawn.Ideo == null || pawn.Map == null)
        {
            Messages.Message($"{pawn.LabelShort} has no ideoligion or map.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        LogPrayerSiteDiagnosis(pawn);

        var job = JoyGiver_Prayer.TryBuildPrayJob(pawn);
        if (job == null)
        {
            Messages.Message($"{pawn.LabelShort}: no valid prayer site found — see dev console for details.",
                new LookTargets(pawn), MessageTypeDefOf.RejectInput, false);
            return;
        }

        var siteName = job.targetA.HasThing ? job.targetA.Thing.LabelShort : "room cell";
        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        Messages.Message($"{pawn.LabelShort}: heading to pray at {siteName}.",
            new LookTargets(pawn), MessageTypeDefOf.NeutralEvent, false);
    }

    private static void LogPrayerSiteDiagnosis(Pawn pawn)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== Prayer site diagnosis: {pawn.LabelShort} ({pawn.Ideo!.name}) ===");
        sb.AppendLine($"CanMeditateNow={MeditationUtility.CanMeditateNow(pawn)}  isMoralist={JoyGiver_Prayer.IsMoralist(pawn)}  worshipRoom={JoyGiver_Prayer.FindWorshipRoom(pawn) != null}");

        sb.AppendLine("\n-- Lecterns (moralist prayer site) --");
        foreach (var thing in pawn.Map!.listerThings.ThingsOfDef(ThingDefOf.Lectern))
        {
            sb.AppendLine($"  {thing.LabelShort}  forbidden={thing.IsForbidden(pawn)}  canReach={pawn.CanReserveAndReach(thing, PathEndMode.InteractionCell, Danger.None)}");
        }

        sb.AppendLine("\n-- Ideo buildings (non-altar) --");
        foreach (var room in pawn.Map.regionGrid.AllRooms)
        {
            if (room.PsychologicallyOutdoors) continue;
            foreach (var thing in room.ContainedAndAdjacentThings.ToList())
            {
                if (thing is not ThingWithComps twc) continue;
                var cs = twc.compStyleable;
                if (cs == null || (cs.Ideo == null && cs.SourcePrecept == null)) continue;
                sb.AppendLine($"  {thing.LabelShort} ({thing.def.defName})  isAltar={thing.def.isAltar}  csIdeo={cs.Ideo?.name ?? "null"}  match={cs.Ideo == pawn.Ideo}");
                if (!thing.def.isAltar && cs.Ideo == pawn.Ideo)
                {
                    var cap = JoyGiver_Prayer.StatuePrayerCap(thing);
                    sb.AppendLine($"    statueCap={cap}  canReserveCap={pawn.CanReserve(thing, cap, -1)}");
                }
                if (cs.SourcePrecept is Precept_Building pb && pb.presenceDemand != null)
                {
                    var demand = pb.presenceDemand;
                    sb.Append($"    presenceDemand applies={demand.AppliesTo(pawn.Map)}");
                    if (demand.AppliesTo(pawn.Map))
                    {
                        var effRoom = demand.GetEffectiveRoom(thing);
                        sb.Append($"  effectiveRoom={effRoom?.ID.ToString() ?? "null"}");
                        if (effRoom != null)
                            foreach (var req in demand.roomRequirements ?? [])
                                sb.Append($"  [{req.GetType().Name}={req.MetOrDisabled(effRoom, pawn)}]");
                    }
                    sb.AppendLine();
                }
                sb.AppendLine($"    forbidden={thing.IsForbidden(pawn)}  canReach={pawn.CanReserveAndReach(thing, PathEndMode.InteractionCell, Danger.None)}");
            }
        }

        sb.AppendLine("\n-- Reliquaries --");
        foreach (var room in pawn.Map.regionGrid.AllRooms)
        {
            if (room.PsychologicallyOutdoors) continue;
            foreach (var thing in room.ContainedAndAdjacentThings.ToList())
            {
                if (thing.def != ThingDefOf.Reliquary) continue;
                sb.AppendLine($"  {thing.LabelShort}  impressiveness={room.GetStat(RoomStatDefOf.Impressiveness):F0}  forbidden={thing.IsForbidden(pawn)}  canReach={pawn.CanReserveAndReach(thing, PathEndMode.InteractionCell, Danger.None)}");
            }
        }

        Log.Message(sb.ToString());
    }

    [DebugAction("Ideoligion", "Trigger conversion attempt", actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
    private static void TriggerConversionAttempt(Pawn initiator)
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
            Messages.Message($"No visible cross-ideo humanlike for {initiator.LabelShort} to convert.",
                MessageTypeDefOf.RejectInput, false);
            return;
        }

        var def = DefDatabase<InteractionDef>.GetNamed("ConvertIdeoAttempt");
        var worker = (InteractionWorker_AdvancedConversionAttempt)def.Worker;
        var extraSentencePacks = new List<RulePackDef>();
        var oldIdeo = recipient.Ideo;

        worker.Interacted(initiator, recipient, extraSentencePacks, out var letterText, out var letterLabel, out var letterDef, out var lookTargets);

        if (letterDef != null)
        {
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, letterDef, lookTargets ?? new LookTargets(initiator, recipient));
        }

        var outcome = recipient.Ideo != oldIdeo
            ? $"converted {oldIdeo!.name} → {recipient.Ideo!.name}"
            : "no conversion";
        Messages.Message($"{initiator.LabelShort} → {recipient.LabelShort}: {outcome}",
            new LookTargets(initiator, recipient), MessageTypeDefOf.NeutralEvent, false);
    }

    [DebugAction("Ideoligion", "Set expectation override", actionType = DebugActionType.Action,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void SetExpectationOverride()
    {
        var options = new List<DebugMenuOption>
        {
            new("None (real value)", DebugMenuOptionMode.Action,
                () => ExpectationsUtility_Override.ForcedExpectation = null)
        };
        foreach (var def in DefDatabase<ExpectationDef>.AllDefsListForReading.OrderBy(d => d.order))
        {
            var captured = def;
            options.Add(new DebugMenuOption(def.label.CapitalizeFirst(), DebugMenuOptionMode.Action,
                () => ExpectationsUtility_Override.ForcedExpectation = captured));
        }
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
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
