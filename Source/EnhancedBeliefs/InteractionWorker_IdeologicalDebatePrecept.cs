namespace EnhancedBeliefs;

[HotSwappable]
internal sealed class InteractionWorker_IdeologicalDebatePrecept : InteractionWorker
{
    public IssueDef? topic;

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
        EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, $"RandomSelectionWeight called: initiator={initiator}, recipient={recipient}");
        if (initiator.Inhumanized())
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "Initiator is inhumanized. Returning 0.");
            return 0f;
        }
        if (!ModsConfig.IdeologyActive)
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "Ideology not active. Returning 0.");
            return 0f;
        }
        if (Find.IdeoManager.classicMode)
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "Classic mode enabled. Returning 0.");
            return 0f;
        }
        if (initiator.Ideo == null)
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "Initiator has no ideo. Returning 0.");
            return 0f;
        }
        if (!recipient.RaceProps.Humanlike)
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "Recipient not humanlike. Returning 0.");
            return 0f;
        }
        if (recipient.DevelopmentalStage.Baby())
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "Recipient is a baby. Returning 0.");
            return 0f;
        }
        if (initiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "Initiator's social skill is totally disabled. Returning 0.");
            return 0f;
        }
        var spreadFactor = initiator.GetStatValue(StatDefOf.SocialIdeoSpreadFrequencyFactor);
        var compatibility = initiator.relations.CompatibilityWith(recipient);
        var curveEval = CompatibilityFactorCurve.Evaluate(compatibility);
        var result = 0.03f * spreadFactor * curveEval;
        EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, $"Returning weight: {result} (spreadFactor={spreadFactor}, compatibility={compatibility}, curveEval={curveEval})");
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

        EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, $"Interacted called: initiator={initiator}, recipient={recipient}");

        var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
        var initiatorTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(initiator);
        var recipientTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var initiatorIdeo = initiator.Ideo;
        var recipientIdeo = recipient.Ideo;

        topic = GetDebateTopic(initiatorIdeo, recipientIdeo, initiatorTracker, recipientTracker, initiator, recipient, out var initiatorPrecept, out var recipientPrecept);
        EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, $"Debate topic selected: {topic}");
        if (initiatorPrecept == null)
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "No initiator precept found. Exiting.");
            return;
        }
        if (recipientPrecept == null)
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "No recipient precept found. Exiting.");
            return;
        }
        EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, $"Initiator's precept: {initiatorPrecept}, recipient's precept: {recipientPrecept}");

        var initiatorRoll = GetDebateRoll(initiator);
        var recipientRoll = GetDebateRoll(recipient);
        EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, $"Debate rolls: initiator={initiatorRoll}, recipient={recipientRoll}");

        if (Math.Abs(initiatorRoll - recipientRoll) <= DebateDrawThreshold)
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "Debate is a draw. Calling HandleDraw.");
            if (HandleDraw(initiator, recipient, initiatorTracker, recipientTracker, initiatorPrecept, recipientPrecept))
            {
                EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "HandleDraw returned true (social fight). Exiting.");
                return;
            }
        }
        else
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "Debate is not a draw. Adjusting opinions.");
            AdjustOpinions(initiator, recipient, comp, initiatorPrecept, recipientPrecept, initiatorRoll, recipientRoll);
        }

        // Precept-driven social aftermath, evaluated per pawn on every non-fight outcome (design.md R3).
        ApplyDiversityAftermath(initiator, recipient);
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
            EnhancedBeliefsMod.Warning("GetDebateTopic: No conflicting topics found. Exiting.");
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
            EnhancedBeliefsMod.Error($"Could not find precept for {pawn} on topic {topic}. This should not happen.");
        }
        return precept;
    }

    // intellectual impact ranges from 0 to ~2.2 (integer-stepped by the /100)
    internal static float IntellectualImpact(Pawn pawn) => pawn.skills.GetSkill(SkillDefOf.Intellectual).Level * 11 / 100;

    // Deterministic centre of a pawn's debate roll: an average of conversion power, intellectual persuasiveness
    // and social impact (max ~2.58167). GetDebateRoll draws a Gaussian around this; the convert-ability tooltip
    // reads it directly to preview the win chance and its per-factor breakdown.
    internal static float DebateRollMean(Pawn pawn) =>
        (pawn.GetStatValue(StatDefOf.ConversionPower) / 2f)
        + (IntellectualImpact(pawn) / 2f)
        + (pawn.GetStatValue(StatDefOf.SocialImpact) / 3f);

    internal static float GetDebateRoll(Pawn pawn)
    {
        var result = Rand.Gaussian(DebateRollMean(pawn), DebateStandardDeviation);
        EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, $"GetDebateRoll: pawn={pawn}, mean={DebateRollMean(pawn)}, result={result}");
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
    private bool HandleDraw(
        Pawn initiator,
        Pawn recipient,
        IdeoTrackerData initiatorTracker,
        IdeoTrackerData recipientTracker,
        PreceptDef initiatorPrecept,
        PreceptDef recipientPrecept)
    {
        // Fetch social fight multiplier
        interaction.socialFightBaseChance = 1f;
        var fightChanceModifier = initiator.interactions.SocialFightChance(interaction, recipient) + recipient.interactions.SocialFightChance(interaction, initiator);
        interaction.socialFightBaseChance = 0f;
        EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, $"HandleDraw: fightChanceModifier={fightChanceModifier}");

        // Socially adept pawns are much less likely to start a brawl over an ideological debate
        var socialFightChance = 0.05f * fightChanceModifier /
            (0.5f + (initiator.skills.GetSkill(SkillDefOf.Social).Level * 0.1f)) /
            (0.5f + (recipient.skills.GetSkill(SkillDefOf.Social).Level * 0.1f));
        EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, $"HandleDraw: socialFightChance={socialFightChance}");

        if (Rand.Value < socialFightChance)
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, "Social fight triggered!");
            recipient.interactions.StartSocialFight(initiator, "EnhancedBeliefs.IdeologicalDebateOutcomeSocialFight");
            return true;
        }

        // Neither side backs down, so each digs in. A pawn entrenches with a probability that rises with
        // intelligence (a smarter arguer rationalizes the stalemate into vindication) and with how shaky their
        // faith already is; digging in strengthens conviction on the contested issue and nudges certainty up.
        TryEntrench(initiator, initiatorTracker, initiatorPrecept);
        TryEntrench(recipient, recipientTracker, recipientPrecept);
        return false;
    }

    private static void TryEntrench(Pawn pawn, IdeoTrackerData tracker, PreceptDef precept)
    {
        var entrenchChance = DebateEntrenchBaseChance
            * (0.75f + (pawn.skills.GetSkill(SkillDefOf.Intellectual).Level * 0.05f))
            / (0.2f + (pawn.ideo.Certainty * 0.8f));
        if (Rand.Value >= entrenchChance)
        {
            EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, $"TryEntrench: {pawn} unmoved (chance {entrenchChance}).");
            return;
        }

        // A resistant pawn (CertaintyLossFactor < 1) also hardens less; fold it in so entrenchment mirrors the
        // fragility scaling the rest of the debate uses.
        var strengthGain = DebateEntrenchStrengthGain * pawn.GetStatValue(StatDefOf.CertaintyLossFactor) * (0.8f + (Rand.Value * 0.4f));
        tracker.ShiftIssueStance(precept.issue!, 0f, 0f, strengthGain);
        pawn.ideo.Certainty = Mathf.Clamp01(pawn.ideo.Certainty + (DebateEntrenchCertaintyGain * (0.8f + (Rand.Value * 0.4f))));
        EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, $"TryEntrench: {pawn} dug in (+{strengthGain} conviction on {precept.issue}).");
    }

    // Diversity-of-thought aftermath: how a pawn feels about the person they just debated depends on their
    // faith's stance on IdeoDiversity, not their personal opinion. A tolerant faith reads a debate as a good
    // exchange (a mood lift + warmer opinion of the other pawn); a bigoted one reads it as an affront (the
    // mirror). A neutral or silent faith produces nothing. Applied to each pawn independently every outcome.
    private static void ApplyDiversityAftermath(Pawn initiator, Pawn recipient)
    {
        GainDiversityMemory(initiator, recipient);
        GainDiversityMemory(recipient, initiator);
    }

    private static void GainDiversityMemory(Pawn pawn, Pawn other)
    {
        var thought = DiversityStance(pawn.Ideo) switch
        {
            DiversityReaction.Tolerant => EnhancedBeliefsDefOf.EB_GoodDebate,
            DiversityReaction.Bigoted => EnhancedBeliefsDefOf.EB_BadDebate,
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

    private static void AdjustOpinions(Pawn initiator, Pawn recipient, GameComponent_EnhancedBeliefs comp, PreceptDef initiatorPrecept, PreceptDef recipientPrecept, float initiatorRoll, float recipientRoll)
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
        EnhancedBeliefsMod.DebugIf(EnhancedBeliefsMod.Settings.DebugInteractionWorkers, $"AdjustOpinions: winner={winner}, loser={loser}, winnerPrecept={winnerPrecept}");
        ConvictionMath.PullStance(comp, winner, loser, winnerPrecept.issue!, PreceptLadder.RankOf(winnerPrecept), 1f);
    }
}
