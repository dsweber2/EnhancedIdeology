namespace EnhancedIdeology;

[HotSwappable]
internal sealed class InteractionWorker_IdeologicalDebatePrecept : InteractionWorker
{
    public IssueDef? topic;
    public IssueDef? logTopic;
    public Pawn? lastWinner;
    public Pawn? lastLoser;
    public PreceptDef? lastWinnerPrecept;

    // A tie hardens both sides (design.md R3). Per pawn, base probability of digging in, the conviction points
    // gained on the contested issue, and the certainty gained - all before the same stat/jitter scaling.
    private const float DebateEntrenchBaseChance = 0.2f;
    private const float DebateEntrenchStrengthGain = 1f;
    private const float DebateEntrenchCertaintyGain = 0.01f;

    // Smallest rung gap that counts as a genuinely different position; below it the two pawns hold the same rung.
    internal const float DebateRankEpsilon = 0.01f;

    // Smallest conviction gap (0-20+ scale) that makes a same-rung issue worth arguing over.
    internal const float DebateStrengthGap = 5f;

    // standard deviation for getting a debate roll; approximately set so a skill difference of 5 still results in a 1 in 5 chance of winning
    internal const float DebateStandardDeviation = 0.75f;

    // Roll gap below which the two debaters are deemed evenly matched: a draw (mutual retrenchment / social fight) rather
    // than a decisive win for either side.
    internal const float DebateDrawThreshold = 0.1f;

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
        if (initiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Initiator's social skill is totally disabled. Returning 0.");
            return 0f;
        }
        var spreadFactor = initiator.GetStatValue(StatDefOf.SocialIdeoSpreadFrequencyFactor);
        var compatibility = initiator.relations.CompatibilityWith(recipient);
        var curveEval = CompatibilityFactorCurve.Evaluate(compatibility);
        var result = 0.03f * spreadFactor * curveEval;
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
        lastLoser = null;
        lastWinnerPrecept = null;

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Interacted called: initiator={initiator}, recipient={recipient}");

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var initiatorTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(initiator);
        var recipientTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var initiatorIdeo = initiator.Ideo;
        var recipientIdeo = recipient.Ideo;
        if (initiatorIdeo == null || recipientIdeo == null) return;

        topic = GetDebateTopic(initiatorIdeo, recipientIdeo, initiatorTracker, recipientTracker, initiator, recipient, out var initiatorPrecept, out var recipientPrecept);
        logTopic = topic;
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Debate topic selected: {topic}");
        if (initiatorPrecept == null)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "No initiator precept found. Exiting.");
            return;
        }
        if (recipientPrecept == null)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "No recipient precept found. Exiting.");
            return;
        }
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Initiator's precept: {initiatorPrecept}, recipient's precept: {recipientPrecept}");

        var initiatorRoll = GetDebateRoll(initiator);
        var recipientRoll = GetDebateRoll(recipient);
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Debate rolls: initiator={initiatorRoll}, recipient={recipientRoll}");

        if (Math.Abs(initiatorRoll - recipientRoll) <= DebateDrawThreshold)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Debate is a draw. Calling HandleDraw.");
            if (HandleDraw(interaction, initiator, recipient, initiatorTracker, recipientTracker, [initiatorPrecept], [recipientPrecept]))
            {
                EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "HandleDraw returned true (social fight). Exiting.");
                return;
            }
            extraSentencePacks.Add(EnhancedIdeologyDefOf.EB_Sentence_DebateDraw);
        }
        else
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Debate is not a draw. Adjusting opinions.");
            var (winner, loser, issue, winnerPrecept) = AdjustOpinions(initiator, recipient, comp, initiatorPrecept, recipientPrecept, initiatorRoll, recipientRoll);
            lastWinner = winner;
            lastLoser = loser;
            lastWinnerPrecept = winnerPrecept;
            extraSentencePacks.Add(winner == initiator
                ? EnhancedIdeologyDefOf.EB_Sentence_InitiatorWon
                : EnhancedIdeologyDefOf.EB_Sentence_RecipientWon);

            var loserOldIdeo = loser.Ideo;
            var loserTracker = loser == recipient ? recipientTracker : initiatorTracker;
            // Same-faith debates adjust conviction only; no cross-faith conversion can result.
            if (winner.Ideo != loser.Ideo && loserTracker.CheckConversion(winner.Ideo) == ConversionOutcome.Success
                && (PawnUtility.ShouldSendNotificationAbout(winner) || PawnUtility.ShouldSendNotificationAbout(loser)))
            {
                var loserRole = loserOldIdeo!.GetRole(loser);
                letterLabel = "EnhancedIdeology.LetterLabelIdeologicalDebateConversion".Translate();
                letterText = "EnhancedIdeology.LetterIdeologicalDebateConversionText".Translate(
                    winner.Named("CONVINCER"),
                    loser.Named("CONVINCED"),
                    loserOldIdeo.Named("OLDIDEO"),
                    loser.Ideo.Named("NEWIDEO"),
                    issue.Named("ISSUE")).Resolve();
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

        // Precept-driven social aftermath, evaluated per pawn on every non-fight outcome (design.md R3).
        ApplyDiversityAftermath(initiator, recipient);
        ApplyApostacyAftermath(initiator, recipient);
        ApplyProselytizerAftermath(initiator, crossIdeo: initiatorIdeo != recipientIdeo,
            initiatorConverted: lastWinner == initiator && letterDef == LetterDefOf.PositiveEvent);
    }

    private static IssueDef? GetDebateTopic(
        Ideo initiatorIdeo, Ideo recipientIdeo,
        IdeoTrackerData initiatorTracker, IdeoTrackerData recipientTracker,
        Pawn initiator, Pawn recipient,
        out PreceptDef? initiatorPrecept, out PreceptDef? recipientPrecept)
    {
        // Conviction is per pawn, not per precept, so pull each pawn's own stance on every issue. Two same-faith
        // pawns share every precept; what they can argue about is how firmly they each hold it.
        var initiatorStances = initiatorTracker.IssueStances()
            .ToDictionary(stance => stance.issue, stance => (stance.rank, stance.strength));
        var recipientStances = recipientTracker.IssueStances()
            .ToDictionary(stance => stance.issue, stance => (stance.rank, stance.strength));

        var sharedIssues = initiatorIdeo.precepts.Select(p => p.def.issue)
            .Intersect(recipientIdeo.precepts.Select(p => p.def.issue))
            .Distinct();

        var conflictingIssues = sharedIssues
            .Select(issue => (
                issue,
                initiatorPrecept: GetPreceptForTopic(initiatorIdeo, issue, initiator),
                recipientPrecept: GetPreceptForTopic(recipientIdeo, issue, recipient)
            ))
            .Where(ip =>
                ip.initiatorPrecept != null &&
                Disagree(initiatorStances[ip.issue!], recipientStances[ip.issue!]))
            .ToList();

        if (conflictingIssues.Count == 0)
        {
            EnhancedIdeologyMod.Warning("GetDebateTopic: No conflicting topics found. Exiting.");
            initiatorPrecept = null;
            recipientPrecept = null;
            return null;
        }

        (var selectedIssue, initiatorPrecept, recipientPrecept) = conflictingIssues.RandomElement();
        return selectedIssue;
    }

    // A topic is worth debating when the two pawns' personal stances differ: either a different rung (cross-faith,
    // or one has drifted) or the same rung held with meaningfully different conviction (same faith, different zeal).
    private static bool Disagree((float rank, float strength) a, (float rank, float strength) b) =>
        Mathf.Abs(a.rank - b.rank) > DebateRankEpsilon
        || Mathf.Abs(a.strength - b.strength) > DebateStrengthGap;

    private static PreceptDef? GetPreceptForTopic(Ideo ideo, IssueDef? topic, Pawn pawn)
    {
        var precept = ideo.precepts.Select(p => p.def).FirstOrDefault(d => d.issue == topic);
        if (precept == null)
        {
            EnhancedIdeologyMod.Error($"Could not find precept for {pawn} on topic {topic}. This should not happen.");
        }
        return precept;
    }

    // intellectual impact ranges from 0 to ~2.2 (integer-stepped by the /100)
    internal static float IntellectualImpact(Pawn pawn) => pawn.skills.GetSkill(SkillDefOf.Intellectual).Level * 11 / 100;

    // Deterministic centre of a pawn's debate roll: an average of conversion power, intellectual persuasiveness
    // and social impact (max ~2.58167). GetDebateRoll draws a Gaussian around this; the convert-ability tooltip
    // reads it directly to preview the win chance and its per-factor breakdown.
    internal static float DebateRollMean(Pawn pawn)
    {
        var convPower = StatDefOf.ConversionPower.Worker.IsDisabledFor(pawn) ? 0f : pawn.GetStatValue(StatDefOf.ConversionPower);
        return (convPower / 2f) + (IntellectualImpact(pawn) / 2f) + (pawn.GetStatValue(StatDefOf.SocialImpact) / 3f);
    }

    internal static float GetDebateRoll(Pawn pawn)
    {
        var result = Rand.Gaussian(DebateRollMean(pawn), DebateStandardDeviation);
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"GetDebateRoll: pawn={pawn}, mean={DebateRollMean(pawn)}, result={result}");
        return result;
    }

    // Probability the initiator wins the roll outright (draw excluded): P(initiatorRoll - recipientRoll > draw
    // threshold). The two rolls are independent Gaussians, so their difference is Gaussian with the summed
    // variance; this is the tail of that difference past the draw band. Read-only, for the ability tooltip.
    internal static float WinChance(Pawn initiator, Pawn recipient)
    {
        var meanDiff = DebateRollMean(initiator) - DebateRollMean(recipient);
        var sdDiff = DebateStandardDeviation * Mathf.Sqrt(2f);
        return 1f - NormalCdf((DebateDrawThreshold - meanDiff) / sdDiff);
    }

    // Standard normal CDF via an erf approximation (Abramowitz & Stegun 7.1.26, ~1e-7 max error). Mathf has no
    // erf, and this only feeds a displayed percentage, so the approximation is ample.
    internal static float NormalCdf(float x) => 0.5f * (1f + Erf(x / Mathf.Sqrt(2f)));

    private static float Erf(float x)
    {
        var sign = Mathf.Sign(x);
        x = Mathf.Abs(x);
        var t = 1f / (1f + (0.3275911f * x));
        var y = 1f - ((((((((1.061405429f * t) - 1.453152027f) * t) + 1.421413741f) * t) - 0.284496736f) * t + 0.254829592f) * t) * Mathf.Exp(-x * x);
        return sign * y;
    }

    // An evenly-matched debate. Either it boils over into a social fight (returns true), or it entrenches both
    // sides (returns false). No rung moves and no one converts on a tie.
    internal static bool HandleDraw(
        InteractionDef interaction,
        Pawn initiator,
        Pawn recipient,
        IdeoTrackerData initiatorTracker,
        IdeoTrackerData recipientTracker,
        IEnumerable<PreceptDef> initiatorPrecepts,
        IEnumerable<PreceptDef> recipientPrecepts)
    {
        // Fetch social fight multiplier
        interaction.socialFightBaseChance = 1f;
        var fightChanceModifier = initiator.interactions.SocialFightChance(interaction, recipient) + recipient.interactions.SocialFightChance(interaction, initiator);
        interaction.socialFightBaseChance = 0f;
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"HandleDraw: fightChanceModifier={fightChanceModifier}");

        // Socially adept pawns are much less likely to start a brawl over an ideological debate
        var socialFightChance = 0.05f * fightChanceModifier /
            (0.5f + (initiator.skills.GetSkill(SkillDefOf.Social).Level * 0.1f)) /
            (0.5f + (recipient.skills.GetSkill(SkillDefOf.Social).Level * 0.1f));

        // Pawns from strict-apostacy faiths have less tolerance for being stalemated by a heretic.
        // Only applies across different ideoligions — a draw against a fellow believer doesn't trigger apostacy rage.
        if (initiator.Ideo != recipient.Ideo)
        {
            var apostacyFightMultiplier = 1f
                + (EnhancedIdeologyUtilities.ApostacyStrictness(initiator.Ideo) * 0.75f)
                + (EnhancedIdeologyUtilities.ApostacyStrictness(recipient.Ideo) * 0.75f);
            socialFightChance *= apostacyFightMultiplier;
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"HandleDraw: apostacyFightMultiplier={apostacyFightMultiplier}");
        }
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"HandleDraw: socialFightChance={socialFightChance}");

        if (Rand.Value < socialFightChance)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Social fight triggered!");
            recipient.interactions.StartSocialFight(initiator, "EnhancedIdeology.IdeologicalDebateOutcomeSocialFight");
            return true;
        }

        // Neither side backs down, so each digs in. A pawn entrenches with a probability that rises with
        // intelligence (a smarter arguer rationalizes the stalemate into vindication) and with how shaky their
        // faith already is; digging in strengthens conviction on the contested issue and nudges certainty up.
        foreach (var precept in initiatorPrecepts)
            TryEntrench(initiator, initiatorTracker, precept);
        foreach (var precept in recipientPrecepts)
            TryEntrench(recipient, recipientTracker, precept);
        return false;
    }

    internal static void TryEntrench(Pawn pawn, IdeoTrackerData tracker, PreceptDef precept)
    {
        var entrenchChance = DebateEntrenchBaseChance
            * (0.75f + (pawn.skills.GetSkill(SkillDefOf.Intellectual).Level * 0.05f))
            / (0.2f + (pawn.ideo.Certainty * 0.8f));
        if (Rand.Value >= entrenchChance)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"TryEntrench: {pawn} unmoved (chance {entrenchChance}).");
            return;
        }

        // A resistant pawn (CertaintyLossFactor < 1) also hardens less; fold it in so entrenchment mirrors the
        // fragility scaling the rest of the debate uses.
        var strengthGain = DebateEntrenchStrengthGain * pawn.GetStatValue(StatDefOf.CertaintyLossFactor) * (0.8f + (Rand.Value * 0.4f));
        tracker.ShiftIssueStance(precept.issue!, 0f, 0f, strengthGain);
        pawn.ideo.Certainty = Mathf.Clamp01(pawn.ideo.Certainty + (DebateEntrenchCertaintyGain * (0.8f + (Rand.Value * 0.4f))));
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"TryEntrench: {pawn} dug in (+{strengthGain} conviction on {precept.issue}).");
    }

    // Diversity-of-thought aftermath: how a pawn feels about the person they just debated depends on their
    // faith's stance on IdeoDiversity, not their personal opinion. A tolerant faith reads a debate as a good
    // exchange (a mood lift + warmer opinion of the other pawn); a bigoted one reads it as an affront (the
    // mirror). A neutral or silent faith produces nothing. Applied to each pawn independently every outcome.
    internal static void ApplyDiversityAftermath(Pawn initiator, Pawn recipient)
    {
        GainDiversityMemory(initiator, recipient);
        GainDiversityMemory(recipient, initiator);
    }

    private static void GainDiversityMemory(Pawn pawn, Pawn other)
    {
        var thought = DiversityStance(pawn.Ideo!) switch
        {
            DiversityReaction.Tolerant => EnhancedIdeologyDefOf.EB_GoodDebate,
            DiversityReaction.Bigoted => EnhancedIdeologyDefOf.EB_BadDebate,
            _ => null,
        };
        if (thought != null)
        {
            pawn.needs.mood?.thoughts.memories.TryGainMemory(thought, other);
        }
    }

    private enum DiversityReaction { None, Bigoted, Neutral, Tolerant }

    // Classify a faith's IdeoDiversity stance relative to its Standard (neutral) rung: anything on the Approved
    // side is tolerant, anything on the Disapproved side is bigoted. Direction-agnostic (the ladder's numeric
    // orientation is derived from the Approved-vs-Standard sign), so it holds however the rungs are ordered.
    private static DiversityReaction DiversityStance(Ideo ideo)
    {
        var precept = ideo.precepts.FirstOrDefault(p => p.def.issue?.defName == "IdeoDiversity");
        if (precept == null)
        {
            return DiversityReaction.None;
        }

        var issue = precept.def.issue!;
        var standard = PreceptLadder.RankOfName(issue, "IdeoDiversity_Standard");
        var approved = PreceptLadder.RankOfName(issue, "IdeoDiversity_Approved");
        if (standard < 0f || approved < 0f)
        {
            return DiversityReaction.None;
        }

        var delta = (PreceptLadder.RankOf(precept.def) - standard) * Math.Sign(approved - standard);
        if (delta > 0.5f)
        {
            return DiversityReaction.Tolerant;
        }

        return delta < -0.5f ? DiversityReaction.Bigoted : DiversityReaction.Neutral;
    }

    // Strict-apostacy aftermath: being debated at all is an affront to a pawn from a faith that treats
    // apostasy as abhorrent. Scales with how strict the apostacy precept is.
    // Proselytizer aftermath: a pawn from a proselytizing faith finds cross-ideo debates fulfilling,
    // with an even stronger boost when they successfully convert the other pawn.
    internal static void ApplyProselytizerAftermath(Pawn initiator, bool crossIdeo, bool initiatorConverted)
    {
        if (!crossIdeo) return;
        if (initiator.Ideo?.memes.Contains(EnhancedIdeologyDefOf.Proselytizer) != true) return;

        var thought = initiatorConverted
            ? EnhancedIdeologyDefOf.EB_ProselytizerConverted
            : EnhancedIdeologyDefOf.EB_ProselytizerDebated;
        initiator.needs.mood?.thoughts.memories.TryGainMemory(thought);
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"ApplyProselytizerAftermath: {initiator} gained {thought.defName}");
    }

    internal static void ApplyApostacyAftermath(Pawn initiator, Pawn recipient)
    {
        GainApostacyDebatedMemory(initiator);
        GainApostacyDebatedMemory(recipient);
    }

    private static void GainApostacyDebatedMemory(Pawn pawn)
    {
        if (EnhancedIdeologyUtilities.ApostacyStrictness(pawn.Ideo) <= 0f)
        {
            return;
        }
        pawn.needs.mood?.thoughts.memories.TryGainMemory(EnhancedIdeologyDefOf.EB_ApostacyDebated);
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"ApplyApostacyAftermath: {pawn} gained EB_ApostacyDebated (strictness={EnhancedIdeologyUtilities.ApostacyStrictness(pawn.Ideo):F2})");
    }

    private static (Pawn winner, Pawn loser, IssueDef issue, PreceptDef winnerPrecept) AdjustOpinions(Pawn initiator, Pawn recipient, GameComponent_EnhancedIdeology comp, PreceptDef initiatorPrecept, PreceptDef recipientPrecept, float initiatorRoll, float recipientRoll)
    {
        Pawn winner, loser;
        PreceptDef winnerPrecept;
        if (initiatorRoll > recipientRoll)
        {
            winner = initiator;
            loser = recipient;
            winnerPrecept = initiatorPrecept;
        }
        else
        {
            winner = recipient;
            loser = initiator;
            winnerPrecept = recipientPrecept;
        }
        var issue = winnerPrecept.issue!;
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"AdjustOpinions: winner={winner}, loser={loser}, winnerPrecept={winnerPrecept}");
        ConvictionMath.PullStance(comp, winner, loser, issue, PreceptLadder.RankOf(winnerPrecept), 1f);
        return (winner, loser, issue, winnerPrecept);
    }
}
