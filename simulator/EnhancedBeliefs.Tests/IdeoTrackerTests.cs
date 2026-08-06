namespace EnhancedBeliefs.Tests;

public class IdeoTrackerTests : SeededTest
{
    [Fact]
    public void GetIdeoPawns_ZeroPawnIdeo_ReturnsEmptyWithoutRecursion()
    {
        // Regression: AddIdeo must pre-register so GetIdeoPawns doesn't recurse when no pawns match
        var world = new SimWorld();
        world.Initialize();
        var ideo = new IdeoBuilder().WithName("Empty").Build();
        world.AddIdeo(ideo);

        var result = world.Comp.GetIdeoPawns(ideo);

        Assert.Empty(result);
    }

    [Fact]
    public void SetIdeo_SwitchesIdeo_UpdatesBothTrackers()
    {
        var world = new SimWorld();
        world.Initialize();
        var ideoA = new IdeoBuilder().WithName("A").Build();
        var ideoB = new IdeoBuilder().WithName("B").Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithLabel("P").Build(world);

        Assert.Contains(pawn, world.Comp.GetIdeoPawns(ideoA));
        Assert.DoesNotContain(pawn, world.Comp.GetIdeoPawns(ideoB));

        world.Comp.SetIdeo(pawn, ideoB);

        Assert.DoesNotContain(pawn, world.Comp.GetIdeoPawns(ideoA));
        Assert.Contains(pawn, world.Comp.GetIdeoPawns(ideoB));
    }

    [Fact]
    public void RecalculateRelationshipIdeoOpinions_MultiIdeoPawn_UpdatesAllTrackedIdeos()
    {
        // Regression: iterates baseIdeoOpinions.Keys, each of which calls GetIdeoPawns —
        // all must be pre-registered or the call recurses infinitely
        var world = new SimWorld();
        world.Initialize();
        var ideoA = new IdeoBuilder().WithName("A").Build();
        var ideoB = new IdeoBuilder().WithName("B").Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        _ = tracker.IdeoOpinion(ideoA);
        _ = tracker.IdeoOpinion(ideoB);

        tracker.RecalculateRelationshipIdeoOpinions();

        Assert.Equal(0f, tracker.IdeoOpinionFromRelationships(ideoA, false, out _));
        Assert.Equal(0f, tracker.IdeoOpinionFromRelationships(ideoB, false, out _));
    }

    [Fact]
    public void AdjustPersonalOpinion_ExtremePositive_ClampsToMaxOpinion()
    {
        var world = new SimWorld();
        world.Initialize();
        var ideoA = new IdeoBuilder().WithName("A").Build();
        var ideoB = new IdeoBuilder().WithName("B").Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        tracker.AdjustPersonalOpinion(ideoB, 1000f);

        Assert.Equal(1f, tracker.IdeoOpinion(ideoB), precision: 4);
    }

    [Fact]
    public void AdjustPersonalOpinion_ExtremeNegative_ClampsToZeroOpinion()
    {
        var world = new SimWorld();
        world.Initialize();
        var ideoA = new IdeoBuilder().WithName("A").Build();
        var ideoB = new IdeoBuilder().WithName("B").Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        tracker.AdjustPersonalOpinion(ideoB, -1000f);

        Assert.Equal(0f, tracker.IdeoOpinion(ideoB), precision: 4);
    }

    [Fact]
    public void TrueMemeOpinion_AgreeableTrait_IncreasesOpinion()
    {
        // Pawn has a trait matching meme.agreeableTraits → base 0 + 10 = 10
        var world = new SimWorld();
        world.Initialize();

        var traitDef = new TraitDef { defName = "AgreeableForMeme" };
        var meme = new MemeBuilder().WithName("TMO_Agreeable").WithAgreeableTrait(traitDef).Build();
        var ideo = new IdeoBuilder().WithName("TM").AddMeme(meme).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithTrait(traitDef).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var opinion = tracker.TrueMemeOpinion(meme);

        Assert.True(opinion > 0f, $"Expected positive meme opinion for agreeable trait. Got: {opinion}");
    }

    [Fact]
    public void TrueMemeOpinion_DisagreeableTrait_DecreasesOpinion()
    {
        // Pawn has a trait matching meme.disagreeableTraits → base 0 - 10 = -10
        var world = new SimWorld();
        world.Initialize();

        var traitDef = new TraitDef { defName = "DisagreeableForMeme" };
        var meme = new MemeBuilder().WithName("TMO_Disagreeable").WithDisagreeableTrait(traitDef).Build();
        var ideo = new IdeoBuilder().WithName("TMD").AddMeme(meme).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithTrait(traitDef).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var opinion = tracker.TrueMemeOpinion(meme);

        Assert.True(opinion < 0f, $"Expected negative meme opinion for disagreeable trait. Got: {opinion}");
    }

    [Fact]
    public void TruePreceptOpinion_AgreeableTrait_AddsTraitBonus()
    {
        // PreceptComp_OpinionOffset.GetTraitOpinion: 1 agreeable trait match → 1 * opinionPerTrait(2f) = +2
        var world = new SimWorld();
        world.Initialize();

        var traitDef = new TraitDef { defName = "AgreeableForPrecept" };
        var preceptDef = new PreceptDef { defName = "TraitPrecept" };
        var comp = new PreceptComp_OpinionOffset_Mutable();
        comp.agreeableTraits.Add(new TraitRequirement { def = traitDef });
        preceptDef.AddComp(comp);

        var ideo = new IdeoBuilder().WithName("TP").AddPrecept(preceptDef).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithTrait(traitDef).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var opinion = tracker.TruePreceptOpinion(preceptDef);

        Assert.Equal(2f, opinion, precision: 4);
    }

    [Fact]
    public void OverrideConversionAttempt_HighCertainty_ReturnsFalse()
    {
        // certainty=0.8, reduction=0.1 → newCertainty=0.7 > 0.2 → CheckConversion returns Failure
        var world = new SimWorld();
        world.Initialize();

        var ideoA = new IdeoBuilder().WithName("HighC").Build();
        var ideoB = new IdeoBuilder().WithName("Target").Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithCertainty(0.8f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var converted = tracker.OverrideConversionAttempt(0.1f, ideoB, applyCertaintyFactor: false);

        Assert.False(converted);
        Assert.Equal(ideoA, pawn.Ideo);
    }

    [Fact]
    public void DetailedIdeoOpinion_OwnIdeo_ReturnsCurrentCertaintyAsBase()
    {
        // Own-ideo branch: BaseOpinion = Pawn.ideo.Certainty (not baseIdeoOpinions lookup)
        var world = new SimWorld();
        world.Initialize();

        var ideo = new IdeoBuilder().WithName("Own").Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.65f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var detail = tracker.DetailedIdeoOpinion(ideo);

        Assert.Equal(0.65f, detail.BaseOpinion, precision: 4);
    }
}
