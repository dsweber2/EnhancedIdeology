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
    public void InteractionWorker_HighConversionPower_DropsCertainty()
    {
        // A single attempt drops recipient certainty (~0.04 * power=3 = 0.12) without yet converting them -
        // their opinion of the evangelist's plain ideo (~0.55) is still below their post-drop certainty (~0.63).
        Rand.SetSeed(1);
        var world = new SimWorld();
        world.Initialize();

        var evangelistIdeo = new IdeoBuilder().WithName("Evangelist").Build();
        var recipientIdeo = new IdeoBuilder().WithName("Recipient").Build();
        world.AddIdeo(evangelistIdeo);
        world.AddIdeo(recipientIdeo);

        var evangelist = new PawnBuilder()
            .WithIdeo(evangelistIdeo)
            .WithCertainty(0.95f)
            .WithConversionPower(3f)
            .WithLabel("Evangelist")
            .Build(world);

        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo)
            .WithCertainty(0.75f)
            .WithLabel("Recipient")
            .Build(world);

        var worker = new InteractionWorker_AdvancedConversionAttempt();
        worker.Interacted(evangelist, recipient, [], out _, out _, out _, out _);

        Assert.Equal(recipientIdeo, recipient.Ideo);
        Assert.True(recipient.ideo.Certainty < 0.75f,
            $"Expected certainty to drop after conversion pressure. Got: {recipient.ideo.Certainty:F4}");
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
    public void OverrideConversionAttempt_DropsToZeroCertainty_ConvertsToTargetIdeo()
    {
        // certainty 0.3 − 0.5 = −0.2 → clamped to 0 → CheckConversion fires → converts to ideoB
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var ideoA = new IdeoBuilder().WithName("OCA").Build();
        var ideoB = new IdeoBuilder().WithName("OCB").Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithCertainty(0.3f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIdeoBaseOpinion(ideoB, 90);

        var converted = tracker.OverrideConversionAttempt(0.5f, ideoB, applyCertaintyFactor: false);

        Assert.True(converted);
        Assert.Equal(ideoB, pawn.Ideo);
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
