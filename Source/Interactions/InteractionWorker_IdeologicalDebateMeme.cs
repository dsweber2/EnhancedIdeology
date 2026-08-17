namespace EnhancedIdeology;

internal sealed class InteractionWorker_IdeologicalDebateMeme : InteractionWorker
{
    public MemeDef? topic;
    public MemeDef? logTopic;
    public Pawn? lastWinner;

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
        if (initiator.Ideo == recipient.Ideo)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Initiator and recipient have same ideo. Returning 0.");
            return 0f;
        }
        if (recipient.DevelopmentalStage.Baby())
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Recipient is a baby. Returning 0.");
            return 0f;
        }
        if (initiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Initiator's social skill is totally disabled. Returning 0.");
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

        // Ideo may have changed since this interaction was queued (same-tick double-conversion).
        if (initiator.Ideo == recipient.Ideo)
            return;

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var initiatorTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(initiator);
        var recipientTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var initiatorIdeo = initiator.Ideo;
        var recipientIdeo = recipient.Ideo;
        if (initiatorIdeo == null || recipientIdeo == null) return;

        topic = initiatorIdeo.memes.Union(recipientIdeo.memes).RandomElement();
        logTopic = topic;
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Debate topic selected: {topic}");

        var initiatorRoll = GetDebateRoll(initiator);
        var recipientRoll = GetDebateRoll(recipient);
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Debate rolls: initiator={initiatorRoll}, recipient={recipientRoll}");

        if (Math.Abs(initiatorRoll - recipientRoll) <= 0.1f)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Debate is a draw. Calling HandleDraw.");
            if (InteractionWorker_IdeologicalDebatePrecept.HandleDraw(
                interaction, initiator, recipient, initiatorTracker, recipientTracker,
                MemePreceptsFor(initiatorIdeo, topic),
                MemePreceptsFor(recipientIdeo, topic)))
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
        InteractionWorker_IdeologicalDebatePrecept.ApplyProselytizerAftermath(initiator, crossIdeo: true,
            initiatorConverted: lastWinner == initiator && letterDef == LetterDefOf.PositiveEvent);
    }

    private static float GetDebateRoll(Pawn pawn)
    {
        var rand = Rand.Value;
        var convPower = pawn.GetStatValue(StatDefOf.ConversionPower);
        var certaintyLoss = pawn.GetStatValue(StatDefOf.CertaintyLossFactor);
        var socialImpact = pawn.GetStatValue(StatDefOf.SocialImpact);
        var certainty = pawn.ideo.Certainty;
        var result = rand * convPower / certaintyLoss * socialImpact * (1f + ((certainty - 0.6f) * 0.5f));
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"GetDebateRoll: pawn={pawn}, rand={rand}, convPower={convPower}, certaintyLoss={certaintyLoss}, socialImpact={socialImpact}, certainty={certainty}, result={result}");
        return result;
    }

    private static IEnumerable<PreceptDef> MemePreceptsFor(Ideo ideo, MemeDef meme) =>
        ideo.precepts
            .Where(p => p.def.issue != null && (p.def.requiredMemes.Contains(meme) || p.def.associatedMemes.Contains(meme)))
            .Select(p => p.def);

    // Finds all issues touched by the topic meme in EITHER ideo, then pulls the loser's stance toward the
    // winner's position. If the winner's ideo has no precept for an issue the loser holds, the target is
    // DontCareRank — the winner is arguing "I have no stake in this" which weakens the loser's conviction.
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

        var winnerPreceptsByIssue = MemePreceptsFor(winner.Ideo!, topic).ToDictionary(p => p.issue!);
        var loserIssues = MemePreceptsFor(loser.Ideo!, topic).Select(p => p.issue!).ToHashSet();
        var allIssues = winnerPreceptsByIssue.Keys.Union(loserIssues).ToList();

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers,
            $"AdjustOpinions: meme covers {allIssues.Count} issues total " +
            $"({winnerPreceptsByIssue.Count} from winner, {loserIssues.Count} from loser): " +
            $"{string.Join(", ", allIssues.Select(i => i.defName))}");

        foreach (var issue in allIssues)
        {
            var targetRank = winnerPreceptsByIssue.TryGetValue(issue, out var winnerPrecept)
                ? PreceptLadder.RankOf(winnerPrecept)
                : PreceptLadder.DontCareRank(issue);
            ConvictionMath.PullStance(comp, winner, loser, issue, targetRank, MemeDebatePullMultiplier);
        }

        return (winner, loser);
    }
}
