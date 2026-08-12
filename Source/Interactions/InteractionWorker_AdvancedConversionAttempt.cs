namespace EnhancedIdeology;

internal sealed class InteractionWorker_AdvancedConversionAttempt : InteractionWorker_ConvertIdeoAttempt
{
    public override void Interacted(
        Pawn initiator,
        Pawn recipient,
        List<RulePackDef> extraSentencePacks,
        out string? letterText,
        out string? letterLabel,
        out LetterDef? letterDef,
        out LookTargets? lookTargets)
    {
        letterLabel = null;
        letterText = null;
        letterDef = null;
        lookTargets = null;

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var recipientIdeo = recipient.Ideo;
        var initiatorIdeo = initiator.Ideo;
        var recipientTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);

        var certaintyBefore = recipient.ideo.Certainty;

        // 1) Argue the belief the recipient most opposes about the preacher's faith. Like a debate, either side can
        //    win the roll; only a decisive win for the preacher persuades the recipient (and does so twice as hard
        //    as an ordinary debate). A loss shifts the preacher instead; a draw moves no one.
        var preacherPersuaded = ResolveDirectedDebate(initiator, recipient, comp, initiatorIdeo, recipientTracker);

        // 2) Only a won debate can flip the recipient's faith.
        if (preacherPersuaded && TryHandleSuccessfulConversion(initiator, recipient, recipientTracker, initiatorIdeo, recipientIdeo, extraSentencePacks,
            ref letterText, ref letterLabel, ref letterDef, ref lookTargets))
        {
            return;
        }

        // 3) Handle failure/neutral outcomes
        HandleOutcome(initiator, recipient, extraSentencePacks, certaintyBefore);
    }

    // A directed conversion is an argument over the belief the recipient most opposes about the preacher's faith,
    // resolved by the same roll as a debate. Returns true only when the preacher wins decisively - then the
    // recipient's stance is pulled toward the preacher at double strength and a conversion becomes possible. On a
    // loss the preacher's own stance is pulled toward the recipient by the ordinary debate amount; a draw (or the
    // recipient already agreeing on everything) moves no one. Either way, false means no conversion this attempt.
    private static bool ResolveDirectedDebate(
        Pawn initiator, Pawn recipient, GameComponent_EnhancedIdeology comp, Ideo initiatorIdeo, IdeoTrackerData recipientTracker)
    {
        var issue = recipientTracker.MostOpposingIssue(initiatorIdeo);
        if (issue == null)
        {
            return false;
        }

        var initiatorRoll = InteractionWorker_IdeologicalDebatePrecept.GetDebateRoll(initiator);
        var recipientRoll = InteractionWorker_IdeologicalDebatePrecept.GetDebateRoll(recipient);
        if (Math.Abs(initiatorRoll - recipientRoll) <= InteractionWorker_IdeologicalDebatePrecept.DebateDrawThreshold)
        {
            return false;
        }

        if (initiatorRoll > recipientRoll)
        {
            // Preacher wins: the recipient's most-opposed stance is dragged toward the rung the preacher's faith
            // holds, at double the per-debate pull.
            var preacherRank = PreceptLadder.RankOf(initiatorIdeo.precepts.Select(precept => precept.def).First(def => def.issue == issue));
            ConvictionMath.PullStance(comp, initiator, recipient, issue, preacherRank, EnhancedIdeologyMod.Settings.ConversionStancePull);
            // Temporary certainty knock so the recipient is more likely to switch now (a lower certainty lowers
            // their opinion of their own faith in the check below) and to spontaneously drift away afterwards.
            recipient.ideo.Certainty *= EnhancedIdeologyMod.Settings.ConversionCertaintyKnock;
            return true;
        }

        // Preacher loses: their own stance on the contested issue is pulled toward the recipient's, by the ordinary
        // debate amount. The recipient is unmoved, so no conversion follows.
        var recipientRank = recipientTracker.IssueStances().First(stance => stance.issue == issue).rank;
        ConvictionMath.PullStance(comp, recipient, initiator, issue, recipientRank, 1f);
        return false;
    }

    private static bool TryHandleSuccessfulConversion(
        Pawn initiator,
        Pawn recipient,
        IdeoTrackerData recipientTracker,
        Ideo initiatorIdeo,
        Ideo recipientIdeo,
        List<RulePackDef> extraSentencePacks,
        ref string? letterText,
        ref string? letterLabel,
        ref LetterDef? letterDef,
        ref LookTargets? lookTargets)
    {
        if (recipientTracker.CheckConversion(initiatorIdeo) == ConversionOutcome.Success)
        {
            if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
            {
                letterLabel = "LetterLabelConvertIdeoAttempt_Success".Translate();
                letterText = "LetterConvertIdeoAttempt_Success".Translate(initiator.Named("INITIATOR"), recipient.Named("RECIPIENT"), initiator.Ideo.Named("IDEO"), recipientIdeo.Named("OLDIDEO")).Resolve();
                letterDef = LetterDefOf.PositiveEvent;
                lookTargets = new LookTargets(initiator, recipient);
                var role = recipientIdeo.GetRole(recipient);

                if (role != null)
                {
                    letterText = letterText + "\n\n" + "LetterRoleLostLetterIdeoChangedPostfix".Translate(recipient.Named("PAWN"), role.Named("ROLE"), recipientIdeo.Named("OLDIDEO")).Resolve();
                }
            }

            extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptSuccess);
            return true;
        }

        return false;
    }

    private static void HandleOutcome(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, float certainty)
    {
        // For some reason vanilla calculations are completely random and don't take any social stats into consideration
        var outcome = Rand.Value *
                    (1 + (recipient.relations.OpinionOf(initiator) * 0.2f * 0.01f)) *
                    initiator.GetStatValue(StatDefOf.SocialImpact);

        // Same code as vanilla, but less janky and makes more sense. 2% to have a fight, 10% to have a negative thought, 78% for nothing to happen at base opinion and impact.
        if (outcome < 0.02f && !recipient.IsPrisoner && recipient.interactions.SocialFightPossible(initiator))
        {
            recipient.interactions.StartSocialFight(initiator, "MessageFailedConvertIdeoAttemptSocialFight");
            extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptFailSocialFight);
        }
        else if (outcome < 0.12f)
        {
            if (recipient.needs.mood != null)
            {
                if (PawnUtility.ShouldSendNotificationAbout(recipient))
                {
                    Messages.Message("MessageFailedConvertIdeoAttempt".Translate(initiator.Named("INITIATOR"), recipient.Named("RECIPIENT"), certainty.ToStringPercent().Named("CERTAINTYBEFORE"), recipient.ideo.Certainty.ToStringPercent().Named("CERTAINTYAFTER")), recipient, MessageTypeDefOf.NeutralEvent);
                }

                recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FailedConvertIdeoAttemptResentment, initiator);
            }

            extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptFailResentment);
        }
        else
        {
            extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptFail);
        }
    }
}
