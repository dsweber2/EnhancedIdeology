namespace EnhancedIdeology.Tests;

public class IdeoTrackerTests : SeededTest
{
    [Fact]
    public void GetIdeoPawns_UnregisteredZeroPawnIdeo_ReturnsEmptyWithoutRecursion()
    {
        // An ideo the component has not seen, with no members, used to recurse until stack overflow.
        var world = new SimWorld();
        world.Initialize();
        var ideo = new IdeoBuilder().WithName("Empty").Build();

        var result = world.Comp.GetIdeoPawns(ideo);

        Assert.Empty(result);
        Assert.True(world.Comp.IdeoTracker.ContainsIdeo(ideo));
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
    public void AdjustPersonalOpinion_ExtremePositive_ExceedsOnePointZero()
    {
        // Opinions are no longer capped at 1.0: a very strong personal boost can push them above certainty,
        // which is what allows conversion to a similar-but-better-fit ideo at overcertainty.
        var world = new SimWorld();
        world.Initialize();
        var ideoA = new IdeoBuilder().WithName("A").Build();
        var ideoB = new IdeoBuilder().WithName("B").Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);

        var pawn = new PawnBuilder().WithIdeo(ideoA).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        tracker.AdjustPersonalOpinion(ideoB, 1000f);

        Assert.True(tracker.IdeoOpinion(ideoB) > 1f, $"Expected opinion > 1.0, got {tracker.IdeoOpinion(ideoB)}");
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

    [Fact]
    public void DetailedIdeoOpinion_ForeignIdeo_AllPropertiesAccessible()
    {
        // Exercises PersonalOpinion, RelationshipOpinion, and DevModeDetails on the foreign-ideo path.
        var world = new SimWorld();
        world.Initialize();

        var ownIdeo = new IdeoBuilder().WithName("Own").Build();
        var foreignIdeo = new IdeoBuilder().WithName("Foreign").Build();
        world.AddIdeo(ownIdeo);
        world.AddIdeo(foreignIdeo);

        var pawn = new PawnBuilder().WithIdeo(ownIdeo).WithCertainty(0.5f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var detail = tracker.DetailedIdeoOpinion(foreignIdeo);

        // Just confirm all three numeric components are reachable and finite.
        Assert.True(float.IsFinite(detail.BaseOpinion), $"BaseOpinion should be finite, got {detail.BaseOpinion}");
        Assert.True(float.IsFinite(detail.PersonalOpinion), $"PersonalOpinion should be finite, got {detail.PersonalOpinion}");
        Assert.True(float.IsFinite(detail.RelationshipOpinion), $"RelationshipOpinion should be finite, got {detail.RelationshipOpinion}");
        _ = detail.DevModeDetails; // must not throw
    }

    [Fact]
    public void DetailedIdeoOpinion_NoRelationship_RelationshipOpinionIsZero()
    {
        var world = new SimWorld();
        world.Initialize();

        var ownIdeo = new IdeoBuilder().WithName("Own").Build();
        var foreignIdeo = new IdeoBuilder().WithName("Foreign").Build();
        world.AddIdeo(ownIdeo);
        world.AddIdeo(foreignIdeo);

        var pawn = new PawnBuilder().WithIdeo(ownIdeo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var detail = tracker.DetailedIdeoOpinion(foreignIdeo, noRelationship: true);

        Assert.Equal(0f, detail.RelationshipOpinion);
    }
}
