namespace EnhancedIdeology;

internal sealed class InteractionWorker_IdeologicalDebateMeme : InteractionWorker
{
    public MemeDef? topic;
    public MemeDef? logTopic;
    public Pawn? lastWinner;
    public Ideo? initiatorIdeo;

    // Per-precept pull is weaker than a focused precept debate since multiple precepts are affected at once.
    private const float MemeDebatePullMultiplier = 0.5f;

    internal static readonly SimpleCurve CompatibilityFactorCurve =
    [
        new CurvePoint(-1.5f, 0.1f),
        new CurvePoint(-0.5f, 0.5f),
        new CurvePoint(0f, 1f),
        new CurvePoint(0.5f, 1.3f),
        new CurvePoint(1f, 1.8f),
        new CurvePoint(2f, 3f)
    ];

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"RandomSelectionWeight called: initiator={initiator}, recipient={recipient}");
        if (initiator.Inhumanized())
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Initiator is inhumanized. Returning 0.");
            return 0f;
        }
        if (!ModsConfig.IdeologyActive)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Ideology not active. Returning 0.");
            return 0f;
        }
        if (Find.IdeoManager.classicMode)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Classic mode enabled. Returning 0.");
            return 0f;
        }
        if (initiator.Ideo == null)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Initiator has no ideo. Returning 0.");
            return 0f;
        }
        if (!recipient.RaceProps.Humanlike)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Recipient not humanlike. Returning 0.");
            return 0f;
        }
        if (recipient.DevelopmentalStage.Baby())
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Recipient is a baby. Returning 0.");
            return 0f;
        }
        if (initiator.WorkTagIsDisabled(WorkTags.Social))
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Initiator is incapable of social. Returning 0.");
            return 0f;
        }
        if (recipient.Ideo == null)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Recipient has no ideo. Returning 0.");
            return 0f;
        }
        var pawnTracker = Current.Game.GetComponent<GameComponent_EnhancedIdeology>().PawnTracker;
        if (DebatableMemes(initiator.Ideo, recipient.Ideo,
                pawnTracker.EnsurePawnHasIdeoTracker(initiator),
                pawnTracker.EnsurePawnHasIdeoTracker(recipient)).Count == 0)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "No debatable meme. Returning 0.");
            return 0f;
        }
        var spreadFactor = initiator.GetStatValue(StatDefOf.SocialIdeoSpreadFrequencyFactor);
        var compatibility = initiator.relations.CompatibilityWith(recipient);
        var curveEval = CompatibilityFactorCurve.Evaluate(compatibility);
        var result = 0.015f * spreadFactor * curveEval;
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Returning weight: {result} (spreadFactor={spreadFactor}, compatibility={compatibility}, curveEval={curveEval})");
        return result;
    }

    public override void Interacted(
        Pawn initiator,
        Pawn recipient,
        List<RulePackDef> extraSentencePacks,
        out string? letterText,
        out string? letterLabel,
        out LetterDef? letterDef,
        out LookTargets? lookTargets)
    {
        letterText = null;
        letterLabel = null;
        letterDef = null;
        lookTargets = null;
        lastWinner = null;

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Interacted called: initiator={initiator}, recipient={recipient}");

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var initiatorTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(initiator);
        var recipientTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var initiatorIdeo = initiator.Ideo;
        var recipientIdeo = recipient.Ideo;
        if (initiatorIdeo == null || recipientIdeo == null) return;

        this.initiatorIdeo = initiatorIdeo;
        var debatableMemes = DebatableMemes(initiatorIdeo, recipientIdeo, initiatorTracker, recipientTracker);
        topic = debatableMemes.Count == 0 ? null : debatableMemes.RandomElement();
        logTopic = topic;
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Debate topic selected: {topic}");
        if (topic == null)
            return;

        var initiatorRoll = InteractionWorker_IdeologicalDebatePrecept.GetDebateRoll(initiator);
        var recipientRoll = InteractionWorker_IdeologicalDebatePrecept.GetDebateRoll(recipient);
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Debate rolls: initiator={initiatorRoll}, recipient={recipientRoll}");

        if (Math.Abs(initiatorRoll - recipientRoll) <= InteractionWorker_IdeologicalDebatePrecept.DrawThreshold(initiator, recipient))
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Debate is a draw. Calling HandleDraw.");
            if (InteractionWorker_IdeologicalDebatePrecept.HandleDraw(
                interaction, initiator, recipient, initiatorTracker, recipientTracker,
                MemePreceptsFor(initiatorIdeo, topic).Select(precept => precept.issue!),
                MemePreceptsFor(recipientIdeo, topic).Select(precept => precept.issue!)))
            {
                EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "HandleDraw returned true (social fight). Exiting.");
                return;
            }
            extraSentencePacks.Add(EnhancedIdeologyDefOf.EB_Sentence_DebateDraw);
        }
        else
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Debate is not a draw. Adjusting opinions.");
            var (winner, loser) = AdjustOpinions(initiator, recipient, comp, topic, initiatorRoll, recipientRoll);
            lastWinner = winner;
            extraSentencePacks.Add(winner == initiator
                ? EnhancedIdeologyDefOf.EB_Sentence_InitiatorWon
                : EnhancedIdeologyDefOf.EB_Sentence_RecipientWon);

            var loserOldIdeo = loser.Ideo;
            var loserTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(loser);
            if (loserTracker.CheckConversion(winner.Ideo) == ConversionOutcome.Success
                && (PawnUtility.ShouldSendNotificationAbout(winner) || PawnUtility.ShouldSendNotificationAbout(loser)))
            {
                var loserRole = loserOldIdeo!.GetRole(loser);
                letterLabel = "EnhancedIdeology.LetterLabelIdeologicalDebateConversion".Translate();
                letterText = "EnhancedIdeology.LetterIdeologicalDebateConversionText".Translate(
                    winner.Named("CONVINCER"),
                    loser.Named("CONVINCED"),
                    loserOldIdeo.Named("OLDIDEO"),
                    loser.Ideo.Named("NEWIDEO"),
                    topic.Named("ISSUE")).Resolve();
                if (loserRole != null)
                {
                    letterText += "\n\n" + "LetterRoleLostLetterIdeoChangedPostfix".Translate(
                        loser.Named("PAWN"), loserRole.Named("ROLE"), loserOldIdeo.Named("OLDIDEO")).Resolve();
                }
                letterDef = LetterDefOf.PositiveEvent;
                lookTargets = new LookTargets(winner, loser);
                extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptSuccess);
            }
        }

        InteractionWorker_IdeologicalDebatePrecept.ApplyDiversityAftermath(initiator, recipient);
        InteractionWorker_IdeologicalDebatePrecept.ApplyApostacyAftermath(initiator, recipient);
        InteractionWorker_IdeologicalDebatePrecept.ApplyProselytizerAftermath(initiator, crossIdeo: initiatorIdeo != recipientIdeo,
            initiatorConverted: lastWinner == initiator && letterDef == LetterDefOf.PositiveEvent);
    }

    // The memes either faith holds that touch at least one debatable issue the two pawns' personal stances
    // disagree on. Co-believers with matching stances have none, so they never debate memes.
    private static List<MemeDef> DebatableMemes(
        Ideo initiatorIdeo, Ideo recipientIdeo, IdeoTrackerData initiatorTracker, IdeoTrackerData recipientTracker)
    {
        var initiatorStances = InteractionWorker_IdeologicalDebatePrecept.StanceMap(initiatorTracker);
        var recipientStances = InteractionWorker_IdeologicalDebatePrecept.StanceMap(recipientTracker);
        return [.. initiatorIdeo.memes.Union(recipientIdeo.memes)
            .Where(meme => MemePreceptsFor(initiatorIdeo, meme).Concat(MemePreceptsFor(recipientIdeo, meme))
                .Any(def => initiatorStances.TryGetValue(def.issue!, out var initiatorStance)
                    && recipientStances.TryGetValue(def.issue!, out var recipientStance)
                    && InteractionWorker_IdeologicalDebatePrecept.Disagree(initiatorStance, recipientStance)))];
    }

    // Only issues that feed structural opinion are moved by a meme debate (PreceptPolicy.IsDebatable).
    private static IEnumerable<PreceptDef> MemePreceptsFor(Ideo ideo, MemeDef meme) =>
        ideo.precepts
            .Where(p => p.def.issue != null && PreceptPolicy.IsDebatable(p.def.issue)
                && (p.def.requiredMemes.Contains(meme) || p.def.associatedMemes.Contains(meme)))
            .Select(p => p.def);

    // Finds all issues touched by the topic meme in EITHER ideo, then pulls the loser's stance toward the
    // winner's personal stance on each, not the winner's ideo position.
    private static (Pawn winner, Pawn loser) AdjustOpinions(
        Pawn initiator, Pawn recipient,
        GameComponent_EnhancedIdeology comp,
        MemeDef topic,
        float initiatorRoll, float recipientRoll)
    {
        Pawn winner, loser;
        if (initiatorRoll > recipientRoll)
        {
            winner = initiator;
            loser = recipient;
        }
        else
        {
            winner = recipient;
            loser = initiator;
        }

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"AdjustOpinions: winner={winner}, loser={loser}");

        var allIssues = MemePreceptsFor(winner.Ideo!, topic)
            .Concat(MemePreceptsFor(loser.Ideo!, topic))
            .Select(p => p.issue!)
            .Distinct()
            .ToList();

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers,
            $"AdjustOpinions: meme covers {allIssues.Count} issues: {string.Join(", ", allIssues.Select(i => i.defName))}");

        var winnerStances = comp.PawnTracker.EnsurePawnHasIdeoTracker(winner).IssueStances()
            .ToDictionary(stance => stance.issue, stance => stance.rank);
        var opinion = loser.relations.OpinionOf(winner);
        var pull = Compat_PeerPressure.AdjustStancePull(MemeDebatePullMultiplier, opinion);
        foreach (var issue in allIssues)
        {
            ConvictionMath.PullStance(comp, winner, loser, issue, winnerStances[issue], pull);
        }
        DebateOnlookers.Sway(comp, winner, loser, allIssues);

        return (winner, loser);
    }
}
