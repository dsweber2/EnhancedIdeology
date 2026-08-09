namespace EnhancedBeliefs.Tests;

public class ConversionTests : SeededTest
{
    [Fact]
    public void CheckConversion_PrefersOwnIdeo_ReturnsFail()
    {
        // High certainty in a plain ideo (opinion 0.8) vs a plain alternative (base opinion 0.3):
        // the pawn prefers their own, so ConversionProbability is 0 and nothing fires.
        var world = new SimWorld();
        world.Initialize();

        var ideo = new IdeoBuilder().WithName("I").Build();
        var other = new IdeoBuilder().WithName("Other").Build();
        world.AddIdeo(ideo);
        world.AddIdeo(other);
        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.8f).WithLabel("P").Build(world);

        var result = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn).CheckConversion();

        Assert.Equal(ConversionOutcome.Failure, result);
        Assert.Equal(ideo, pawn.Ideo);
    }

    [Fact]
    public void CheckConversion_ZeroCertainty_HighOpinionAlternative_Converts()
    {
        // Pawn's opinion of ideoB is pinned to 0.9.
        // certainty=0 → opinion of own ideo is 0, so p = (0.9 - 0)/0.9 = 1 → always converts.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var ideoA = new IdeoBuilder().WithName("IdeoA").Build();
        var ideoB = new IdeoBuilder().WithName("IdeoB").Build();

        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithCertainty(0f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIdeoBaseOpinion(ideoB, 90);

        var result = tracker.CheckConversion();

        Assert.Equal(ConversionOutcome.Success, result);
        Assert.Equal(ideoB, pawn.Ideo);
    }

    [Fact]
    public void InteractionWorker_PullsRecipientStanceTowardPreachersRung()
    {
        // Directed conversion is a debate the preacher always wins: the recipient's stance on the contested issue
        // slides toward the rung the evangelist's faith holds (rung 0), without converting them outright here.
        Rand.SetSeed(1);
        var (world, evangelist, recipient, issue, _) = OpposedFaiths();
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var rankBefore = recipientTracker.IssueStances().First(stance => stance.issue == issue).rank;

        new InteractionWorker_AdvancedConversionAttempt().Interacted(evangelist, recipient, [], out _, out _, out _, out _);

        var rankAfter = recipientTracker.IssueStances().First(stance => stance.issue == issue).rank;
        Assert.True(rankAfter < rankBefore,
            $"Expected recipient's stance to slide toward the preacher's rung 0. before={rankBefore}, after={rankAfter}");
    }

    [Fact]
    public void InteractionWorker_TargetsTheMostOpposedIssue()
    {
        // The evangelist's faith preaches rung 0 on two issues; the recipient sits at rung 0 on one (agreement)
        // and the far rung on the other (opposition). Conversion must target the opposed issue and leave the
        // agreed-upon one untouched.
        Rand.SetSeed(1);
        var world = new SimWorld();
        world.Initialize();

        var (agree, agreeRungs) = SimIssues.Ladder("AgreeIssue", "Aa", "Ab", "Ac");
        var (clash, clashRungs) = SimIssues.Ladder("ClashIssue", "Ca", "Cb", "Cc");
        var evangelistIdeo = new IdeoBuilder().WithName("Preacher")
            .AddPrecept(agreeRungs[0], agree, displayOrderInIssue: 0)
            .AddPrecept(clashRungs[0], clash, displayOrderInIssue: 0).Build();
        var recipientIdeo = new IdeoBuilder().WithName("Convert")
            .AddPrecept(agreeRungs[0], agree, displayOrderInIssue: 0)
            .AddPrecept(clashRungs[2], clash, displayOrderInIssue: 20).Build();
        world.AddIdeo(evangelistIdeo);
        world.AddIdeo(recipientIdeo);

        var evangelist = new PawnBuilder().WithIdeo(evangelistIdeo).WithCertainty(1f).WithConversionPower(5f).WithLabel("E").Build(world);
        var recipient = new PawnBuilder().WithIdeo(recipientIdeo).WithCertainty(0.5f).WithConversionPower(0.1f).WithLabel("R").Build(world);
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);

        var agreeBefore = recipientTracker.IssueStances().First(stance => stance.issue == agree).rank;
        var clashBefore = recipientTracker.IssueStances().First(stance => stance.issue == clash).rank;

        new InteractionWorker_AdvancedConversionAttempt().Interacted(evangelist, recipient, [], out _, out _, out _, out _);

        var agreeAfter = recipientTracker.IssueStances().First(stance => stance.issue == agree).rank;
        var clashAfter = recipientTracker.IssueStances().First(stance => stance.issue == clash).rank;

        Assert.Equal(agreeBefore, agreeAfter);
        Assert.True(clashAfter < clashBefore, $"Expected the opposed issue to move. before={clashBefore}, after={clashAfter}");
    }

    [Fact]
    public void InteractionWorker_WinPull_ScalesWithConversionStancePullSetting()
    {
        // A won conversion reuses the per-debate pull times the ConversionStancePull setting, scaled by the
        // preacher's ConversionPower and the recipient's CertaintyLossFactor. With a modest power the clamp does
        // not bite, so the rung move is exactly (target - before) * setting * StancePullPerDebate * CP * CLF.
        // Derive the expectation from the setting and constant so retuning either keeps this honest.
        EnhancedBeliefsMod.Settings.ConversionStancePull = 3f;
        Rand.SetSeed(1);
        var (world, initiator, recipient, issue, initiatorIdeo) = OpposedFaiths(initiatorConversionPower: 1.5f);
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var before = recipientTracker.IssueStances().First(stance => stance.issue == issue).rank;
        var targetRank = PreceptLadder.RankOf(initiatorIdeo.precepts.Select(precept => precept.def).First(def => def.issue == issue));

        new InteractionWorker_AdvancedConversionAttempt().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        var after = recipientTracker.IssueStances().First(stance => stance.issue == issue).rank;
        var pull = EnhancedBeliefsMod.Settings.ConversionStancePull * InteractionWorker_IdeologicalDebatePrecept.StancePullPerDebate
            * initiator.GetStatValue(StatDefOf.ConversionPower)
            * recipient.GetStatValue(StatDefOf.CertaintyLossFactor);
        Assert.Equal(before + ((targetRank - before) * pull), after, 3);
    }

    [Fact]
    public void InteractionWorker_PreacherLoses_ShiftsPreacherByOrdinaryDebateAmount()
    {
        // Flip the stats so the recipient dominates the roll: the preacher loses. Their own stance on the contested
        // issue is pulled toward the recipient's rung by the ordinary (1x) debate pull, the recipient does not budge,
        // and no conversion happens.
        Rand.SetSeed(1);
        var (world, initiator, recipient, issue, _) = OpposedFaiths(
            initiatorConversionPower: 0.1f, initiatorSocialImpact: 1f,
            recipientConversionPower: 5f, recipientSocialImpact: 6f);
        var initiatorTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(initiator);
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var initiatorBefore = initiatorTracker.IssueStances().First(stance => stance.issue == issue).rank;
        var recipientRank = recipientTracker.IssueStances().First(stance => stance.issue == issue).rank;
        var recipientIdeoBefore = recipient.Ideo;

        new InteractionWorker_AdvancedConversionAttempt().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        var initiatorAfter = initiatorTracker.IssueStances().First(stance => stance.issue == issue).rank;
        var pull = InteractionWorker_IdeologicalDebatePrecept.StancePullPerDebate
            * recipient.GetStatValue(StatDefOf.ConversionPower)
            * initiator.GetStatValue(StatDefOf.CertaintyLossFactor);

        Assert.Equal(initiatorBefore + ((recipientRank - initiatorBefore) * pull), initiatorAfter, 3);
        Assert.Equal(recipientRank, recipientTracker.IssueStances().First(stance => stance.issue == issue).rank);
        Assert.Equal(recipientIdeoBefore, recipient.Ideo);
    }

    [Fact]
    public void InteractionWorker_WonAttempt_KnocksRecipientCertaintyDown()
    {
        // The preacher wins the roll (high SocialImpact) but barely moves the stance (low ConversionPower), so the
        // recipient still prefers their own faith and does not flip. Their certainty is knocked down by
        // ConversionCertaintyKnock, leaving them more convertible later.
        Rand.SetSeed(1);
        var (world, initiator, recipient, issue, _) = OpposedFaiths(
            initiatorConversionPower: 0.2f, initiatorSocialImpact: 12f);
        var recipientIdeoBefore = recipient.Ideo;
        var certaintyBefore = recipient.ideo.Certainty;

        new InteractionWorker_AdvancedConversionAttempt().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        Assert.Equal(recipientIdeoBefore, recipient.Ideo); // did not convert
        Assert.Equal(certaintyBefore * EnhancedBeliefsMod.Settings.ConversionCertaintyKnock, recipient.ideo.Certainty, 3);
    }

    [Fact]
    public void ShiftIssueStance_CrossesMultipleRungs_WithoutSnapping()
    {
        // A stance is a continuous rank, not a whole-rung slot: one pull can carry it across several rungs, and a
        // partial pull lands squarely between them. The per-issue opinion tracks that fractional rank - strong
        // opposition four rungs from the target's stance, full agreement once it lands on it.
        Rand.SetSeed(1);
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("FiveStep", "R0", "R1", "R2", "R3", "R4");
        var ownIdeo = new IdeoBuilder().WithName("Own").AddPrecept(rungs[4], issue, displayOrderInIssue: 40).Build();
        var targetIdeo = new IdeoBuilder().WithName("Target").AddPrecept(rungs[0], issue, displayOrderInIssue: 0).Build();
        world.AddIdeo(ownIdeo);
        world.AddIdeo(targetIdeo);

        var pawn = new PawnBuilder().WithIdeo(ownIdeo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        Assert.Equal(4f, tracker.IssueStances().First(stance => stance.issue == issue).rank); // seeds to own rung 4
        Assert.True(tracker.IssueOpinionToward(targetIdeo, issue) < 0f, "a four-rung gap should read as opposition");

        // A tenth of the way from rung 4 toward rung 0 lands at rank 3.6 - a fractional rung, no snapping.
        tracker.ShiftIssueStance(issue, PreceptLadder.RankOf(rungs[0]), 0.1f, 0f);
        Assert.Equal(3.6f, tracker.IssueStances().First(stance => stance.issue == issue).rank, 3);

        // The rest of the way in one pull crosses the remaining rungs and lands on the target's stance.
        tracker.ShiftIssueStance(issue, PreceptLadder.RankOf(rungs[0]), 1f, 0f);
        Assert.Equal(0f, tracker.IssueStances().First(stance => stance.issue == issue).rank, 3);
        Assert.True(tracker.IssueOpinionToward(targetIdeo, issue) > 0f, "landing on the target's rung should agree");
    }

    [Fact]
    public void Conversion_OvershootsPastOpinionOfNewFaith_AndKeepsHeterodoxStance()
    {
        // A convert overshoots: they arrive at their old certainty plus twice the margin by which they preferred
        // the new faith (old + 2*(opinion - old)), clamped - always above the raw opinion, since preferring the new
        // faith is a precondition of converting. And they keep their personal stances: joining a rung-2 faith does
        // not snap their rung-0 conviction to rung 2, so a convert with a clashing belief still holds it.
        EnhancedBeliefsMod.Settings.CrisisThreshold = 0f; // drop the crisis pseudo-candidate so the draw is deterministic
        Rand.SetSeed(1);
        var world = new SimWorld();
        world.Initialize();

        // Own and target agree on two issues and clash on a third (own/pawn rung 0, target rung 2), so the target
        // is a net-positive but imperfect fit.
        var (agreeA, aRungs) = SimIssues.Ladder("AgreeA", "Aa", "Ab", "Ac");
        var (agreeB, bRungs) = SimIssues.Ladder("AgreeB", "Ba", "Bb", "Bc");
        var (clash, cRungs) = SimIssues.Ladder("Clash", "Ca", "Cb", "Cc");
        var ownIdeo = new IdeoBuilder().WithName("Own")
            .AddPrecept(aRungs[0], agreeA, displayOrderInIssue: 0)
            .AddPrecept(bRungs[0], agreeB, displayOrderInIssue: 0)
            .AddPrecept(cRungs[0], clash, displayOrderInIssue: 0).Build();
        var targetIdeo = new IdeoBuilder().WithName("Target")
            .AddPrecept(aRungs[0], agreeA, displayOrderInIssue: 0)
            .AddPrecept(bRungs[0], agreeB, displayOrderInIssue: 0)
            .AddPrecept(cRungs[2], clash, displayOrderInIssue: 20).Build();
        world.AddIdeo(ownIdeo);
        world.AddIdeo(targetIdeo);

        // Zero starting certainty guarantees the conversion draw fires deterministically. The overshoot formula
        // is still checked in general form below (it just reduces to 2*opinion when old certainty is 0).
        var pawn = new PawnBuilder().WithIdeo(ownIdeo).WithCertainty(0f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var oldCertainty = pawn.ideo.Certainty;
        var opinionOfTarget = tracker.IdeoOpinion(targetIdeo);
        var clashStanceBefore = tracker.IssueStances().First(stance => stance.issue == clash).rank;

        var result = tracker.CheckConversion();

        Assert.Equal(ConversionOutcome.Success, result);
        Assert.Equal(targetIdeo, pawn.Ideo);
        // Arrival certainty overshoots the raw opinion by the preference margin.
        Assert.Equal(Mathf.Clamp01(oldCertainty + (2f * (opinionOfTarget - oldCertainty))), pawn.ideo.Certainty, 3);
        Assert.True(pawn.ideo.Certainty > opinionOfTarget, $"convert should overshoot the raw opinion {opinionOfTarget}, got {pawn.ideo.Certainty}");
        // The clashing stance is retained, not snapped to the new faith's rung.
        Assert.Equal(clashStanceBefore, tracker.IssueStances().First(stance => stance.issue == clash).rank);
    }

    // An initiator whose faith preaches rung 0 vs a recipient whose faith preaches the far rung on one shared Moral
    // issue. By default the initiator is a dominant debater (high ConversionPower and SocialImpact) so the roll is
    // a reliable win; the loss test flips the stats so the recipient dominates instead.
    private static (SimWorld world, Pawn initiator, Pawn recipient, IssueDef issue, Ideo initiatorIdeo) OpposedFaiths(
        float initiatorConversionPower = 5f, float initiatorSocialImpact = 6f,
        float recipientConversionPower = 0.1f, float recipientSocialImpact = 1f)
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("ConvIssue", "Permissive", "Middle", "Forbidding");
        var initiatorIdeo = new IdeoBuilder().WithName("StrongFaith").AddPrecept(rungs[0], issue, displayOrderInIssue: 0).Build();
        var recipientIdeo = new IdeoBuilder().WithName("WeakFaith").AddPrecept(rungs[2], issue, displayOrderInIssue: 20).Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);

        var initiator = new PawnBuilder()
            .WithIdeo(initiatorIdeo).WithCertainty(1f).WithConversionPower(initiatorConversionPower).WithSocialImpact(initiatorSocialImpact)
            .WithLabel("Strong").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo).WithCertainty(0.3f).WithConversionPower(recipientConversionPower).WithSocialImpact(recipientSocialImpact)
            .WithLabel("Weak").Build(world);

        return (world, initiator, recipient, issue, initiatorIdeo);
    }

    [Fact]
    public void CheckConversion_WeightedByGap_FavoursHigherOpinionButBothReachable()
    {
        // Target selection is a weighted draw by opinion gap, not a guaranteed pick of the top ideo.
        // The higher-gap ideo (0.9) should win clearly more than the lower (0.5) - roughly 1.8:1 - but
        // the lower one must remain reachable, unlike the old deterministic "always highest" behaviour.
        Rand.SetSeed(1);
        const int trials = 400;
        var high = 0;
        var low = 0;

        for (var ii = 0; ii < trials; ii++)
        {
            var world = new SimWorld();
            world.Initialize();

            var own = new IdeoBuilder().WithName("Own").Build();
            var lowIdeo = new IdeoBuilder().WithName("Low").Build();
            var highIdeo = new IdeoBuilder().WithName("High").Build();
            world.AddIdeo(own);
            world.AddIdeo(lowIdeo);
            world.AddIdeo(highIdeo);

            var pawn = new PawnBuilder().WithIdeo(own).WithCertainty(0f).WithLabel("P").Build(world);
            var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
            tracker.SetIdeoBaseOpinion(lowIdeo, 50);
            tracker.SetIdeoBaseOpinion(highIdeo, 90);

            tracker.CheckConversion();

            if (pawn.Ideo == highIdeo) high++;
            else if (pawn.Ideo == lowIdeo) low++;
        }

        // Some trials break down (the crisis pseudo-candidate wins) so high + low < trials, but the two
        // real ideos keep their gap ratio among the conversions that do happen.
        Assert.True(high > low, $"higher-gap ideo should be favoured: high={high} low={low}");
        Assert.True(low > 0, $"lower-gap ideo must remain reachable: low={low}");
        Assert.InRange((float)high / low, 1.4f, 2.3f); // gap ratio 0.9:0.5 = 1.8
    }

    [Fact]
    public void CheckConversion_PriorityIdeo_FavouredOverHigherOpinionAlternative()
    {
        // A directed attempt halves every *other* candidate's chance and weight, so the priority ideo
        // (opinion 0.6) is favoured over a higher-opinion alternative (0.9) - a bias now, not a guarantee.
        Rand.SetSeed(1);
        const int trials = 400;
        var priority = 0;
        var other = 0;

        for (var ii = 0; ii < trials; ii++)
        {
            var world = new SimWorld();
            world.Initialize();

            var ideoA = new IdeoBuilder().WithName("A").Build();
            var ideoB = new IdeoBuilder().WithName("B").Build();
            var ideoC = new IdeoBuilder().WithName("C").Build();
            world.AddIdeo(ideoA);
            world.AddIdeo(ideoB);
            world.AddIdeo(ideoC);

            var pawn = new PawnBuilder().WithIdeo(ideoA).WithCertainty(0f).WithLabel("P").Build(world);
            var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
            tracker.SetIdeoBaseOpinion(ideoB, 60);
            tracker.SetIdeoBaseOpinion(ideoC, 90);

            tracker.CheckConversion(priorityIdeo: ideoB);

            if (pawn.Ideo == ideoB) priority++;
            else if (pawn.Ideo == ideoC) other++;
        }

        Assert.True(priority > other, $"priority ideo should be favoured: priority={priority} other={other}");
    }

    [Fact]
    public void CheckConversion_ExcludeIdeos_SkipsExcludedIdeo()
    {
        // ideoC would normally win (highest opinion) but is excluded → converts to ideoB instead
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var ideoA = new IdeoBuilder().WithName("A2").Build();
        var ideoB = new IdeoBuilder().WithName("B2").Build();
        var ideoC = new IdeoBuilder().WithName("C2").Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);
        world.AddIdeo(ideoC);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithCertainty(0f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIdeoBaseOpinion(ideoB, 45);
        tracker.SetIdeoBaseOpinion(ideoC, 90);

        var result = tracker.CheckConversion(excludeIdeos: [ideoC]);

        Assert.Equal(ConversionOutcome.Success, result);
        Assert.Equal(ideoB, pawn.Ideo);
    }

    [Fact]
    public void CheckConversion_NoPreferableAlternative_ZeroCertainty_ReturnsBreakdown()
    {
        // With no absolute threshold, breakdown only fires when the pawn prefers no other ideo over their
        // (collapsed) own. Here the only ideo is their own, so there is nothing to convert to.
        var world = new SimWorld();
        world.Initialize();

        var ideoA = new IdeoBuilder().WithName("Aa").Build();
        world.AddIdeo(ideoA);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithCertainty(0f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var result = tracker.CheckConversion();

        Assert.Equal(ConversionOutcome.Breakdown, result);
        Assert.Equal(ideoA, pawn.Ideo);
    }

    [Fact]
    public void ConversionPowerFactor_AgreeableTrait_IncreasesAboveOne()
    {
        var world = new SimWorld();
        world.Initialize();

        var traitDef = new TraitDef { defName = "AgreeableTrait" };
        var meme = new MemeBuilder()
            .WithName("TestMeme")
            .WithAgreeableTrait(traitDef)
            .Build();

        var initiatorIdeo = new IdeoBuilder().WithName("Initiator").AddMeme(meme).Build();
        var recipientIdeo = new IdeoBuilder().WithName("Recipient").Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);

        var initiator = new PawnBuilder().WithIdeo(initiatorIdeo).WithLabel("I").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo)
            .WithTrait(traitDef)
            .WithLabel("R")
            .Build(world);

        var factor = ConversionUtility.ConversionPowerFactor_MemesVsTraits(initiator, recipient);

        Assert.True(factor > 1f,
            $"Expected factor > 1 for agreeable trait match, got {factor}");
    }

    [Fact]
    public void ConversionPowerFactor_DisagreeableTrait_DecreasesFactorBelowOne()
    {
        var world = new SimWorld();
        world.Initialize();

        var traitDef = new TraitDef { defName = "DisagreeableTrait" };
        var meme = new MemeBuilder()
            .WithName("DisagreeMeme")
            .WithDisagreeableTrait(traitDef)
            .Build();

        var initiatorIdeo = new IdeoBuilder().WithName("Initiator2").AddMeme(meme).Build();
        var recipientIdeo = new IdeoBuilder().WithName("Recipient2").Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);

        var initiator = new PawnBuilder().WithIdeo(initiatorIdeo).WithLabel("I").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo)
            .WithTrait(traitDef)
            .WithLabel("R")
            .Build(world);

        var factor = ConversionUtility.ConversionPowerFactor_MemesVsTraits(initiator, recipient);

        Assert.True(factor < 1f,
            $"Expected factor < 1 for disagreeable trait match, got {factor}");
    }
}
