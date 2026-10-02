namespace EnhancedIdeology;

[HotSwappable]
internal sealed class InteractionWorker_IdeologicalDebatePrecept : InteractionWorker
{
    public IssueDef? topic;
    public IssueDef? logTopic;
    public PreceptDef? topicPrecept;
    public Pawn? lastWinner;
    public Pawn? lastLoser;
    public PreceptDef? lastWinnerPrecept;
    public Ideo? initiatorIdeo;

    // Iconoclast mental break: multiplies draw fight chance. Passionate certainty gives a roll bonus.
    internal const float IconoclastRollBonus = 0.5f;

    // A tie hardens both sides (docs/design.md "Belief change"). Per pawn, base probability of digging in, the conviction points
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
    // than a decisive win for either side. Wider band → more ties, fewer outright wins.
    internal const float DebateDrawThreshold = 0.25f;

    // Iconoclast draw threshold: sized so ~75% of debates end in a draw. With SD=0.75 per roll, the roll difference
    // is N(0, 1.06); P(|X| < 1.22) ≈ 0.75.
    internal const float IconoclastDrawThreshold = 1.22f;

    // Stance shift multiplier applied when a debate is won decisively — hits harder than a normal persuasion attempt.
    internal const float DebateWinPullMultiplier = 2.0f;

    // Penalty applied to the debate roll mean of a pawn in brainwipe recovery. At SD=0.75, a -3 shift gives the
    // brainwiped pawn a ~0.3% chance of winning against an average opponent.
    private const float BrainwipeDebatePenalty = 3.0f;

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

        this.initiatorIdeo = initiatorIdeo;
        topic = GetDebateTopic(initiatorIdeo, recipientIdeo, initiatorTracker, recipientTracker, initiator, out var initiatorPrecept);
        logTopic = topic;
        topicPrecept = initiatorPrecept;
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Debate topic selected: {topic}");
        if (topic == null || initiatorPrecept == null)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "No debate topic found. Exiting.");
            return;
        }
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Initiator's precept: {initiatorPrecept}");

        var initiatorRoll = GetDebateRoll(initiator);
        var recipientRoll = GetDebateRoll(recipient);
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"Debate rolls: initiator={initiatorRoll}, recipient={recipientRoll}");

        var drawThreshold = initiator.MentalState is MentalState_Iconoclast || recipient.MentalState is MentalState_Iconoclast
            ? IconoclastDrawThreshold
            : DebateDrawThreshold;
        if (Math.Abs(initiatorRoll - recipientRoll) <= drawThreshold)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Debate is a draw. Calling HandleDraw.");
            if (HandleDraw(interaction, initiator, recipient, initiatorTracker, recipientTracker, [topic], [topic]))
            {
                EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "HandleDraw returned true (social fight). Exiting.");
                return;
            }
            extraSentencePacks.Add(EnhancedIdeologyDefOf.EB_Sentence_DebateDraw);
        }
        else
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Debate is not a draw. Adjusting opinions.");
            var (winner, loser, issue, winnerPrecept) = AdjustOpinions(initiator, recipient, comp, topic, initiatorRoll, recipientRoll);
            lastWinner = winner;
            lastLoser = loser;

            // Interloper won the argument: release them from the debate pin.
            if (loser.MentalState is MentalState_Iconoclast iconoclastLost)
                iconoclastLost.RemoveDebateTarget(winner);
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

        // Precept-driven social aftermath, evaluated per pawn on every non-fight outcome (docs/design.md "Belief change").
        ApplyDiversityAftermath(initiator, recipient);
        ApplyApostacyAftermath(initiator, recipient);
        ApplyProselytizerAftermath(initiator, crossIdeo: initiatorIdeo != recipientIdeo,
            initiatorConverted: lastWinner == initiator && letterDef == LetterDefOf.PositiveEvent);
    }

    private static IssueDef? GetDebateTopic(
        Ideo initiatorIdeo, Ideo recipientIdeo,
        IdeoTrackerData initiatorTracker, IdeoTrackerData recipientTracker,
        Pawn initiator,
        out PreceptDef? initiatorPrecept)
    {
        // Conviction is per pawn, not per precept, so pull each pawn's own stance on every issue. Two same-faith
        // pawns share every precept; what they can argue about is how firmly they each hold it.
        var initiatorStances = initiatorTracker.IssueStances()
            .ToDictionary(stance => stance.issue, stance => (stance.rank, stance.strength));
        var recipientStances = recipientTracker.IssueStances()
            .ToDictionary(stance => stance.issue, stance => (stance.rank, stance.strength));

        // The initiator raises issues their own ideo takes a position on; the recipient's ideo can be silent on
        // them. Each side argues from their personal stance (see AdjustOpinions).
        // Issues without a seeded stance (PreceptCategory.NA: buildings, ritual seats, naming) aren't a belief
        // axis and never get a personal stance recorded, so excluding them here is just staying in sync with
        // IssueStanceTracker.EnsureSeeded rather than a bespoke category check.
        var conflictingIssues = initiatorIdeo.precepts
            .Select(p => p.def)
            .Where(def => def.issue != null
                && initiatorStances.ContainsKey(def.issue)
                && recipientStances.ContainsKey(def.issue)
                && Disagree(initiatorStances[def.issue], recipientStances[def.issue]))
            .GroupBy(def => def.issue)
            .Select(group => group.First())
            .ToList();

        if (conflictingIssues.Count == 0)
        {
            EnhancedIdeologyMod.Warning("GetDebateTopic: No conflicting topics found. Exiting.");
            initiatorPrecept = null;
            return null;
        }

        // Iconoclast: target the issue the initiator is most opposed to in the recipient's ideo,
        // sampled proportionally to opposition magnitude via exponential racing (-log(U)/w).
        if (initiator.MentalState is MentalState_Iconoclast)
        {
            initiatorPrecept = conflictingIssues.MinBy(def =>
            {
                var w = Mathf.Max(-initiatorTracker.IssueOpinionToward(recipientIdeo, def.issue), float.Epsilon);
                return -Mathf.Log(Rand.Value) / w;
            });
            return initiatorPrecept.issue;
        }

        // Sample proportionally to how far apart the two pawns are on each issue, via exponential racing.
        initiatorPrecept = conflictingIssues.MinBy(def =>
            -Mathf.Log(Rand.Value) / TopicWeight(def.issue, initiatorStances[def.issue], recipientStances[def.issue]));
        return initiatorPrecept.issue;
    }

    // Rung gap as a fraction of the full ladder span, scaled to the 0-20 conviction range, plus the conviction gap.
    // The span includes the virtual "Don't care" rung when it sits below rung 0. A single-rung ladder has no
    // span, so it is clamped to 1 to keep the weight finite.
    internal static float TopicWeight(IssueDef issue, (float rank, float strength) a, (float rank, float strength) b)
    {
        var span = PreceptLadder.Rungs(issue).Count - 1 - Mathf.Min(0f, PreceptLadder.DontCareRank(issue));
        return (Mathf.Abs(a.rank - b.rank) / Mathf.Max(span, 1f) * 20f) + Mathf.Abs(a.strength - b.strength);
    }

    // A topic is worth debating when the two pawns' personal stances differ: either a different rung (cross-faith,
    // or one has drifted) or the same rung held with meaningfully different conviction (same faith, different zeal).
    private static bool Disagree((float rank, float strength) a, (float rank, float strength) b) =>
        Mathf.Abs(a.rank - b.rank) > DebateRankEpsilon
        || Mathf.Abs(a.strength - b.strength) > DebateStrengthGap;

    // intellectual impact ranges from 0 to ~2.2 (integer-stepped by the /100)
    internal static float IntellectualImpact(Pawn pawn) => pawn.skills.GetSkill(SkillDefOf.Intellectual).Level * 11 / 100;

    // Deterministic centre of a pawn's debate roll: an average of conversion power, intellectual persuasiveness
    // and social impact (max ~2.58167). GetDebateRoll draws a Gaussian around this; the convert-ability tooltip
    // reads it directly to preview the win chance and its per-factor breakdown.
    internal static float DebateRollMean(Pawn pawn)
    {
        var convPower = StatDefOf.ConversionPower.Worker.IsDisabledFor(pawn) ? 0f : pawn.GetStatValue(StatDefOf.ConversionPower);
        var penalty = IdeoTrackerData.HasBrainwipeRecovery(pawn) ? BrainwipeDebatePenalty : 0f;
        return (convPower / 2f) + (IntellectualImpact(pawn) / 2f) + (pawn.GetStatValue(StatDefOf.SocialImpact) / 3f) - penalty;
    }

    internal static float GetDebateRoll(Pawn pawn)
    {
        var iconoclastBonus = pawn.MentalState is MentalState_Iconoclast ? IconoclastRollBonus : 0f;
        var result = Rand.Gaussian(DebateRollMean(pawn) + iconoclastBonus, DebateStandardDeviation);
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"GetDebateRoll: pawn={pawn}, mean={DebateRollMean(pawn)}, iconoclastBonus={iconoclastBonus}, result={result}");
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

    // When an iconoclast is involved, StartSocialFight fails silently — TryStartMentalState(SocialFighting) can't
    // replace an already-active mental state. Issue paired AttackMelee jobs directly instead and track the fight
    // in MentalState_Iconoclast so the job givers don't interrupt it.
    private static void StartDebateFight(Pawn initiator, Pawn recipient)
    {
        var iconoclast = initiator.MentalState as MentalState_Iconoclast ?? recipient.MentalState as MentalState_Iconoclast;
        if (iconoclast != null)
        {
            var other = iconoclast.pawn == initiator ? recipient : initiator;
            iconoclast.fightTarget = other;
            iconoclast.ClearDebateTargets();

            // Give the other pawn SocialFighting. IsOtherPawnSocialFightingWithMe is patched to return
            // true when the opponent is an iconoclast with matching fightTarget, so the state persists.
            other.mindState.mentalStateHandler.TryStartMentalState(
                MentalStateDefOf.SocialFighting, null, forced: false, forceWake: false,
                causedByMood: false, iconoclast.pawn);

            if (PawnUtility.ShouldSendNotificationAbout(iconoclast.pawn) || PawnUtility.ShouldSendNotificationAbout(other))
                Messages.Message("MessageSocialFight".Translate(iconoclast.pawn.LabelShort, other.LabelShort, iconoclast.pawn.Named("PAWN1"), other.Named("PAWN2")), iconoclast.pawn, MessageTypeDefOf.ThreatSmall);
            TaleRecorder.RecordTale(TaleDefOf.SocialFight, iconoclast.pawn, other);
        }
        else
        {
            recipient.interactions.StartSocialFight(initiator, "EnhancedIdeology.IdeologicalDebateOutcomeSocialFight");
        }
    }

    // Iconoclast draw fight chance: bypasses SocialFightPossible (which gates on WorkTags.Violent) because
    // a mental break overrides the pawn's normal pacifism. ~75% fight chance per draw.
    internal const float IconoclastDrawFightChance = 0.75f;

    // An evenly-matched debate. Either it boils over into a social fight (returns true), or it entrenches both
    // sides (returns false). No rung moves and no one converts on a tie.
    internal static bool HandleDraw(
        InteractionDef interaction,
        Pawn initiator,
        Pawn recipient,
        IdeoTrackerData initiatorTracker,
        IdeoTrackerData recipientTracker,
        IEnumerable<IssueDef> initiatorIssues,
        IEnumerable<IssueDef> recipientIssues)
    {
        float socialFightChance;
        if (initiator.MentalState is MentalState_Iconoclast || recipient.MentalState is MentalState_Iconoclast)
        {
            // Mental break overrides normal violence restrictions — use a flat chance instead of SocialFightChance.
            // If either pawn can't fight (pacifist ideology, trait, etc.) skip the fight entirely; repeated debates
            // will naturally occur because the interloper is still present when the iconoclast's next job fires.
            socialFightChance = (initiator.interactions?.SocialFightPossible(recipient) == true
                && recipient.interactions?.SocialFightPossible(initiator) == true)
                ? IconoclastDrawFightChance
                : 0f;
        }
        else
        {
            // Fetch social fight multiplier
            interaction.socialFightBaseChance = 1f;
            var fightChanceModifier = (initiator.interactions?.SocialFightChance(interaction, recipient) ?? 0f)
                + (recipient.interactions?.SocialFightChance(interaction, initiator) ?? 0f);
            interaction.socialFightBaseChance = 0f;
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"HandleDraw: fightChanceModifier={fightChanceModifier}");

            // Socially adept pawns are much less likely to start a brawl over an ideological debate
            socialFightChance = 0.05f * fightChanceModifier /
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
        }
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"HandleDraw: socialFightChance={socialFightChance}");

        if (Rand.Value < socialFightChance)
        {
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, "Social fight triggered!");
            StartDebateFight(initiator, recipient);
            return true;
        }

        // Neither side backs down, so each digs in. A pawn entrenches with a probability that rises with
        // intelligence (a smarter arguer rationalizes the stalemate into vindication) and with how shaky their
        // faith already is; digging in strengthens conviction on the contested issue and nudges certainty up.
        foreach (var issue in initiatorIssues)
            TryEntrench(initiator, initiatorTracker, issue);
        foreach (var issue in recipientIssues)
            TryEntrench(recipient, recipientTracker, issue);

        // Pin the interloper: MentalState_Iconoclast.MentalStateTick re-interrupts their job every 15 ticks
        // so they can't make progress on firefighting/repair between debate rounds.
        if (initiator.MentalState is MentalState_Iconoclast iconoclastStateI)
            iconoclastStateI.PinDebateTarget(recipient);
        else if (recipient.MentalState is MentalState_Iconoclast iconoclastStateR)
            iconoclastStateR.PinDebateTarget(initiator);
        return false;
    }

    internal static void TryEntrench(Pawn pawn, IdeoTrackerData tracker, IssueDef issue)
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
        tracker.ShiftIssueStance(issue, 0f, 0f, strengthGain);
        pawn.ideo.Certainty = Mathf.Clamp01(pawn.ideo.Certainty + (DebateEntrenchCertaintyGain * (0.8f + (Rand.Value * 0.4f))));
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"TryEntrench: {pawn} dug in (+{strengthGain} conviction on {issue}).");
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
        var thoughtDef = DiversityStance(pawn.Ideo!) switch
        {
            DiversityReaction.Tolerant => EnhancedIdeologyDefOf.EB_GoodDebate,
            DiversityReaction.Bigoted => EnhancedIdeologyDefOf.EB_BadDebate,
            _ => null,
        };
        if (thoughtDef != null)
        {
            var precept = pawn.Ideo!.precepts.FirstOrDefault(p => p.def.issue?.defName == "IdeoDiversity");
            pawn.needs.mood?.thoughts.memories.TryGainMemory(thoughtDef, other, precept);
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
        GainProselytizerMemory(initiator, thought);
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"ApplyProselytizerAftermath: {initiator} gained {thought.defName}");
    }

    internal static void GainProselytizerMemory(Pawn pawn, ThoughtDef thoughtDef)
    {
        var precept = pawn.Ideo!.precepts.FirstOrDefault(p => p.def.associatedMemes?.Contains(EnhancedIdeologyDefOf.Proselytizer) == true);
        var thought = (Thought_MemeMemory)ThoughtMaker.MakeThought(thoughtDef);
        thought.sourcePrecept = precept;
        thought.SourceMemeLabel = precept == null ? EnhancedIdeologyDefOf.Proselytizer.LabelCap : null;
        pawn.needs.mood?.thoughts.memories.TryGainMemory(thought);
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
        var apostacyIssue = DefDatabase<IssueDef>.GetNamedSilentFail("Apostasy");
        var precept = apostacyIssue != null ? pawn.Ideo!.precepts.FirstOrDefault(p => p.def.issue == apostacyIssue) : null;
        pawn.needs.mood?.thoughts.memories.TryGainMemory(EnhancedIdeologyDefOf.EB_ApostacyDebated, null, precept);
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"ApplyApostacyAftermath: {pawn} gained EB_ApostacyDebated (strictness={EnhancedIdeologyUtilities.ApostacyStrictness(pawn.Ideo):F2})");
    }

    // The loser is pulled toward the winner's personal stance, not the winner's ideo position. The returned
    // precept is the rung nearest that stance, for the play log; it is null at "Don't care" or off the ladder.
    private static (Pawn winner, Pawn loser, IssueDef issue, PreceptDef? winnerPrecept) AdjustOpinions(Pawn initiator, Pawn recipient, GameComponent_EnhancedIdeology comp, IssueDef issue, float initiatorRoll, float recipientRoll)
    {
        var (winner, loser) = initiatorRoll > recipientRoll ? (initiator, recipient) : (recipient, initiator);
        var targetRank = comp.PawnTracker.EnsurePawnHasIdeoTracker(winner).IssueStances().First(stance => stance.issue == issue).rank;
        var rungs = PreceptLadder.Rungs(issue);
        var nearestRung = Mathf.RoundToInt(targetRank);
        var winnerPrecept = nearestRung >= 0 && nearestRung < rungs.Count ? rungs[nearestRung] : null;
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"AdjustOpinions: winner={winner}, loser={loser}, targetRank={targetRank}, winnerPrecept={winnerPrecept}");

        var opinion = loser.relations.OpinionOf(winner);
        var pull = Compat_PeerPressure.AdjustStancePull(DebateWinPullMultiplier, opinion);
        ConvictionMath.PullStance(comp, winner, loser, issue, targetRank, pull);
        return (winner, loser, issue, winnerPrecept);
    }
}
