namespace EnhancedBeliefs.Tests;

public class ConversionTests
{
    [Fact]
    public void CheckConversion_CertaintyAboveThreshold_ReturnsFail()
    {
        var world = new SimWorld();
        world.Initialize();

        var ideo = new IdeoBuilder().WithName("I").Build();
        world.AddIdeo(ideo);
        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("P").Build(world);

        var result = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn).CheckConversion();

        Assert.Equal(ConversionOutcome.Failure, result);
    }

    [Fact]
    public void CheckConversion_ZeroCertainty_HighOpinionAlternative_Converts()
    {
        // ideoB has a precept with ExternalOffset=90 → DefaultIdeoOpinion returns 90
        // certainty=0 → threshold=0.6, and opinion=0.9 → random chance always succeeds (1.2 > any [0,1] roll)
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var ideoA = new IdeoBuilder().WithName("IdeoA").Build();
        var friendlyPreceptDef = new PreceptDef { defName = "FriendlyPrecept" };
        friendlyPreceptDef.AddComp(new PreceptComp_OpinionOffset_Mutable { ExternalOffsetValue = 90 });
        var ideoB = new IdeoBuilder().WithName("IdeoB").AddPrecept(friendlyPreceptDef).Build();

        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithCertainty(0f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var result = tracker.CheckConversion();

        Assert.Equal(ConversionOutcome.Success, result);
        Assert.Equal(ideoB, pawn.Ideo);
    }

    [Fact]
    public void InteractionWorker_HighConversionPower_DropsCertainty()
    {
        // Verify the worker directly reduces recipient certainty (5 * 0.04 * power=3 = 0.6 drop from 0.75)
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

        // 3 iterations: certainty → 0.75 − 3×0.12 = 0.39, which keeps chance formula negative
        // ((1 − 0.39×4) = −0.56) so conversion is mathematically impossible regardless of rand
        var worker = new InteractionWorker_AdvancedConversionAttempt();
        for (int ii = 0; ii < 3; ii++)
            worker.Interacted(evangelist, recipient, [], out _, out _, out _, out _);

        Assert.True(recipient.ideo.Certainty < 0.75f,
            $"Expected certainty to drop after conversion pressure. Got: {recipient.ideo.Certainty:F4}");
    }

    [Fact]
    public void CheckConversion_MultipleIdeos_PicksHighestOpinion()
    {
        // SortBy(IdeoOpinion) + Reverse means highest-opinion ideo is tried first
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var lowPreceptDef = new PreceptDef { defName = "Low" };
        lowPreceptDef.AddComp(new PreceptComp_OpinionOffset_Mutable { ExternalOffsetValue = 10 });
        var highPreceptDef = new PreceptDef { defName = "High" };
        highPreceptDef.AddComp(new PreceptComp_OpinionOffset_Mutable { ExternalOffsetValue = 90 });

        var ideoA = new IdeoBuilder().WithName("IdeoA").Build();
        var ideoB = new IdeoBuilder().WithName("IdeoB").AddPrecept(lowPreceptDef).Build();
        var ideoC = new IdeoBuilder().WithName("IdeoC").AddPrecept(highPreceptDef).Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);
        world.AddIdeo(ideoC);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithCertainty(0f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var result = tracker.CheckConversion();

        Assert.Equal(ConversionOutcome.Success, result);
        Assert.Equal(ideoC, pawn.Ideo);
    }

    [Fact]
    public void CheckConversion_PriorityIdeo_ConvertsToTargetOverHigherOpinionAlternative()
    {
        // Without priority, pawn converts to ideoC (opinion=0.9). With priorityIdeo=ideoB,
        // ideoB is moved to the front of the reversed list and tried first despite lower opinion.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var preceptB = new PreceptDef { defName = "PB" };
        preceptB.AddComp(new PreceptComp_OpinionOffset_Mutable { ExternalOffsetValue = 30 });
        var preceptC = new PreceptDef { defName = "PC" };
        preceptC.AddComp(new PreceptComp_OpinionOffset_Mutable { ExternalOffsetValue = 60 });

        var ideoA = new IdeoBuilder().WithName("A").Build();
        var ideoB = new IdeoBuilder().WithName("B").AddPrecept(preceptB).Build();
        var ideoC = new IdeoBuilder().WithName("C").AddPrecept(preceptC).Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);
        world.AddIdeo(ideoC);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithCertainty(0f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var result = tracker.CheckConversion(priorityIdeo: ideoB);

        Assert.Equal(ConversionOutcome.Success, result);
        Assert.Equal(ideoB, pawn.Ideo);
    }

    [Fact]
    public void CheckConversion_ExcludeIdeos_SkipsExcludedIdeo()
    {
        // ideoC would normally win (highest opinion) but is excluded → converts to ideoB instead
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var preceptB = new PreceptDef { defName = "PB2" };
        preceptB.AddComp(new PreceptComp_OpinionOffset_Mutable { ExternalOffsetValue = 45 });
        var preceptC = new PreceptDef { defName = "PC2" };
        preceptC.AddComp(new PreceptComp_OpinionOffset_Mutable { ExternalOffsetValue = 90 });

        var ideoA = new IdeoBuilder().WithName("A2").Build();
        var ideoB = new IdeoBuilder().WithName("B2").AddPrecept(preceptB).Build();
        var ideoC = new IdeoBuilder().WithName("C2").AddPrecept(preceptC).Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);
        world.AddIdeo(ideoC);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithCertainty(0f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var result = tracker.CheckConversion(excludeIdeos: [ideoC]);

        Assert.Equal(ConversionOutcome.Success, result);
        Assert.Equal(ideoB, pawn.Ideo);
    }

    [Fact]
    public void CheckConversion_NoCandidateAboveThreshold_ReturnsBreakdown()
    {
        // ideoB opinion = (30+0)/100 = 0.3, below threshold 0.6 for certainty=0 → breakdown path
        var world = new SimWorld();
        world.Initialize();

        var ideoA = new IdeoBuilder().WithName("Aa").Build();
        var ideoB = new IdeoBuilder().WithName("Ba").Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);

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

        var preceptDef = new PreceptDef { defName = "OCAp" };
        preceptDef.AddComp(new PreceptComp_OpinionOffset_Mutable { ExternalOffsetValue = 90 });

        var ideoA = new IdeoBuilder().WithName("OCA").Build();
        var ideoB = new IdeoBuilder().WithName("OCB").AddPrecept(preceptDef).Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithCertainty(0.3f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

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
