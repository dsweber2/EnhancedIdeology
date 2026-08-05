namespace EnhancedBeliefs.Tests;

public class StructuralOpinionTests
{
    [Fact]
    public void StructuralIdeoOpinion_SupremacistMeme_DecreasesBaseOpinion()
    {
        // Supremacist on own ideo subtracts 20 from foreign ideo opinion (30 base → 10)
        var world = new SimWorld();
        world.Initialize();

        var supremacistMeme = new MemeDef { defName = "Supremacist" };
        EnhancedBeliefsDefOf.Supremacist = supremacistMeme;

        var pawnIdeo = new IdeoBuilder().WithName("Supremacist").AddMeme(supremacistMeme).Build();
        var foreignIdeo = new IdeoBuilder().WithName("Foreign").Build();
        world.AddIdeo(pawnIdeo);
        world.AddIdeo(foreignIdeo);

        var pawn = new PawnBuilder().WithIdeo(pawnIdeo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var opinion = tracker.StructuralIdeoOpinion(foreignIdeo);

        Assert.True(opinion < 30f, $"Expected opinion < 30 (base) for Supremacist, got {opinion}");
    }

    [Fact]
    public void StructuralIdeoOpinion_GuiltyMeme_IncreasesBaseOpinion()
    {
        // Guilty on own ideo adds 10 to foreign ideo opinion (30 base → 40)
        var world = new SimWorld();
        world.Initialize();

        var guiltyMeme = new MemeDef { defName = "Guilty" };
        EnhancedBeliefsDefOf.Guilty = guiltyMeme;

        var pawnIdeo = new IdeoBuilder().WithName("Guilty").AddMeme(guiltyMeme).Build();
        var foreignIdeo = new IdeoBuilder().WithName("Foreign2").Build();
        world.AddIdeo(pawnIdeo);
        world.AddIdeo(foreignIdeo);

        var pawn = new PawnBuilder().WithIdeo(pawnIdeo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var opinion = tracker.StructuralIdeoOpinion(foreignIdeo);

        Assert.True(opinion > 30f, $"Expected opinion > 30 (base) for Guilty, got {opinion}");
    }

    [Fact]
    public void StructuralIdeoOpinion_InternalOffset_AppliesToForeignIdeoOpinion()
    {
        // InternalOffset on own ideo's precept adds to opinion of ALL foreign ideos
        var world = new SimWorld();
        world.Initialize();

        var ownPreceptDef = new PreceptDef { defName = "OwnPrecept" };
        var pawnIdeo = new IdeoBuilder().WithName("OwnIdeo").AddPrecept(ownPreceptDef, internalOffset: 20).Build();
        var foreignIdeo = new IdeoBuilder().WithName("ForeignIdeo").Build();
        world.AddIdeo(pawnIdeo);
        world.AddIdeo(foreignIdeo);

        var pawn = new PawnBuilder().WithIdeo(pawnIdeo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var opinion = tracker.StructuralIdeoOpinion(foreignIdeo);

        Assert.Equal(50f, opinion, precision: 4); // 30 base + 20 internal offset
    }

    [Fact]
    public void BeliefDifferences_SharedMeme_ReturnsNegativeOne()
    {
        var meme = new MemeDef { defName = "SharedMeme" };
        var ideoA = new IdeoBuilder().WithName("A").AddMeme(meme).Build();
        var ideoB = new IdeoBuilder().WithName("B").AddMeme(meme).Build();

        var diff = GameComponent_EnhancedBeliefs.BeliefDifferences(ideoA, ideoB);

        Assert.Equal(-1, diff);
    }

    [Fact]
    public void BeliefDifferences_ConflictingExclusionTags_ReturnsPositiveOne()
    {
        var memeA = new MemeBuilder().WithName("MemeA").WithExclusionTag("tag1").Build();
        var memeB = new MemeBuilder().WithName("MemeB").WithExclusionTag("tag1").Build();
        var ideoA = new IdeoBuilder().WithName("A").AddMeme(memeA).Build();
        var ideoB = new IdeoBuilder().WithName("B").AddMeme(memeB).Build();

        var diff = GameComponent_EnhancedBeliefs.BeliefDifferences(ideoA, ideoB);

        Assert.Equal(1, diff);
    }
}
