namespace EnhancedIdeology.Tests;

public class StructuralOpinionTests : SeededTest
{
    [Fact]
    public void StructuralIdeoOpinion_SupremacistMeme_LowersOpinionVsNonSupremacist()
    {
        // Supremacist on the pawn's own ideo subtracts 20 from their opinion of any foreign ideo. There is
        // no fixed base any more, so we compare against an otherwise-identical non-Supremacist pawn instead
        // of a magic number: the Supremacist must land strictly lower.
        var supremacistMeme = new MemeDef { defName = "Supremacist" };
        EnhancedIdeologyDefOf.Supremacist = supremacistMeme;

        var supremacist = StructuralOpinionTowardPlainForeign(pawnMeme: supremacistMeme);
        var plain = StructuralOpinionTowardPlainForeign(pawnMeme: null);

        Assert.True(supremacist < plain,
            $"Supremacist pawn should hold a lower structural opinion of a foreign ideo: {supremacist} < {plain}");
    }

    [Fact]
    public void StructuralIdeoOpinion_GuiltyMeme_RaisesOpinionVsNonGuilty()
    {
        // Guilty on the pawn's own ideo adds 10 to their opinion of any foreign ideo. Assert the sign of the
        // relationship (higher than an otherwise-identical non-Guilty pawn), not the vanished +30 base.
        var guiltyMeme = new MemeDef { defName = "Guilty" };
        EnhancedIdeologyDefOf.Guilty = guiltyMeme;

        var guilty = StructuralOpinionTowardPlainForeign(pawnMeme: guiltyMeme);
        var plain = StructuralOpinionTowardPlainForeign(pawnMeme: null);

        Assert.True(guilty > plain,
            $"Guilty pawn should hold a higher structural opinion of a foreign ideo: {guilty} > {plain}");
    }

    [Fact]
    public void LoyaltyMeme_AppliesToOtherFaithsOnly_NotTheOwnStructuralBand()
    {
        // A Loyalist faith sours its members on other faiths, but their fit with their own faith is unchanged:
        // an otherwise-identical pawn in a non-Loyalist faith has the same own-ideo structural band.
        EnhancedIdeologyMod.Settings.CrisisThreshold = 0f;
        var loyalistMeme = new MemeDef { defName = "Loyalist" };
        EnhancedIdeologyDefOf.Loyalist = loyalistMeme;

        var (issue, rungs) = SimIssues.Ladder("Generosity", "Selfish", "Generous");

        float OwnStructural(bool loyalist)
        {
            Rand.SetSeed(1);
            var world = new SimWorld();
            world.Initialize();
            var builder = new IdeoBuilder().WithName("Own").AddPrecept(rungs[1], issue, displayOrderInIssue: 10);
            if (loyalist)
                _ = builder.AddMeme(loyalistMeme);
            var own = builder.Build();
            world.AddIdeo(own);
            var pawn = new PawnBuilder().WithIdeo(own).WithCertainty(0.5f).WithLabel("P").Build(world);
            var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
            tracker.CertaintyChangeRecache(world.Comp);
            return tracker.CachedStructural;
        }

        var plain = OwnStructural(loyalist: false);
        Assert.True(plain > 0f, $"precondition: own structural band should be positive, got {plain}");
        Assert.Equal(plain, OwnStructural(loyalist: true), precision: 5);
    }

    [Fact]
    public void StructuralOpinion_OfAnIdeo_DoesNotDependOnWhichFaithThePawnIsIn()
    {
        // The pawn's side of the comparison is their stances, not their faith's precepts: switching faiths
        // (stances unchanged) must not move the structural score of either ideo. Faith B also holds an issue
        // faith A is silent on, which used to widen the averaged issue set only while the pawn was in B.
        var world = new SimWorld();
        world.Initialize();
        var (shared, sharedRungs) = SimIssues.Ladder("Shared", "Low", "Mid", "High");
        var (extra, extraRungs) = SimIssues.Ladder("Extra", "Low", "High");
        var ideoA = new IdeoBuilder().WithName("A").AddPrecept(sharedRungs[2], shared, displayOrderInIssue: 20).Build();
        var ideoB = new IdeoBuilder().WithName("B")
            .AddPrecept(sharedRungs[0], shared, displayOrderInIssue: 0)
            .AddPrecept(extraRungs[1], extra, displayOrderInIssue: 10).Build();
        world.AddIdeo(ideoA);
        world.AddIdeo(ideoB);
        var pawn = new PawnBuilder().WithIdeo(ideoA).WithCertainty(0.5f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        _ = tracker.IssueStances().ToList();

        var scoreAInA = StructuralOpinionCalculator.Compute(pawn, tracker.Stances, ideoA, null);
        var scoreBInA = StructuralOpinionCalculator.Compute(pawn, tracker.Stances, ideoB, null);
        pawn.ideo.SetIdeo(ideoB);
        var scoreAInB = StructuralOpinionCalculator.Compute(pawn, tracker.Stances, ideoA, null);
        var scoreBInB = StructuralOpinionCalculator.Compute(pawn, tracker.Stances, ideoB, null);

        Assert.Equal(scoreAInA, scoreAInB, precision: 4);
        Assert.Equal(scoreBInA, scoreBInB, precision: 4);
    }

    [Fact]
    public void StructuralIdeoOpinion_ForeignHoldsDistantStance_IsLowerThanMatchingStance()
    {
        // Two foreign ideos on the same issue ladder: one holds the pawn's own preferred rung, the other the
        // far extreme. Distance is disagreement, so the pawn's structural opinion of the far one is lower.
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("Diet", "Permissive", "Middle", "Forbidding");

        // The pawn's own ideo holds the permissive rung, so that becomes their preferred stance.
        var pawnIdeo = new IdeoBuilder().WithName("Own").AddPrecept(rungs[0], issue, displayOrderInIssue: 0).Build();
        var matching = new IdeoBuilder().WithName("Matching").AddPrecept(rungs[0], issue, displayOrderInIssue: 0).Build();
        var distant = new IdeoBuilder().WithName("Distant").AddPrecept(rungs[2], issue, displayOrderInIssue: 20).Build();
        world.AddIdeo(pawnIdeo);
        world.AddIdeo(matching);
        world.AddIdeo(distant);

        var pawn = new PawnBuilder().WithIdeo(pawnIdeo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var matchingOpinion = tracker.StructuralIdeoOpinion(matching);
        var distantOpinion = tracker.StructuralIdeoOpinion(distant);

        Assert.True(distantOpinion < matchingOpinion,
            $"ideo holding a distant rung should score lower: distant={distantOpinion} matching={matchingOpinion}");
    }

    [Fact]
    public void StructuralIdeoOpinion_OwnIdeo_SitsNearSeededFloor()
    {
        // For the own ideo every issue's target rung equals the pawn's preferred rung, so each per-issue
        // opinion returns +strength and the structural value is strength·5 (one issue here). Strength is drawn
        // from U(BaseConvictionMin, BaseConvictionMax), so assert that band (·5, clamped to 100), not a number.
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("Diet", "Permissive", "Middle", "Forbidding");
        var pawnIdeo = new IdeoBuilder().WithName("Own").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        world.AddIdeo(pawnIdeo);

        var pawn = new PawnBuilder().WithIdeo(pawnIdeo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        // Own-ideo goes through StructuralOpinionOf directly (not the certainty short-circuit) via the
        // certainty setpoint; read it through a foreign-free structural computation by asking about the own
        // ideo's stance set on a second, identical ideo (same rung → full agreement, no meme terms).
        var mirror = new IdeoBuilder().WithName("Mirror").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        world.AddIdeo(mirror);
        var structural = tracker.StructuralIdeoOpinion(mirror);

        Assert.InRange(structural,
            ConvictionScale.BaseConvictionMin * 5f,
            Mathf.Min(ConvictionScale.BaseConvictionMax * 5f, 100f));
    }

    [Fact]
    public void BeliefDifferences_SharedMeme_ReturnsNegativeOne()
    {
        var meme = new MemeDef { defName = "SharedMeme" };
        var ideoA = new IdeoBuilder().WithName("A").AddMeme(meme).Build();
        var ideoB = new IdeoBuilder().WithName("B").AddMeme(meme).Build();

        var diff = GameComponent_EnhancedIdeology.BeliefDifferences(ideoA, ideoB);

        Assert.Equal(-1, diff);
    }

    [Fact]
    public void BeliefDifferences_ConflictingExclusionTags_ReturnsPositiveOne()
    {
        var memeA = new MemeBuilder().WithName("MemeA").WithExclusionTag("tag1").Build();
        var memeB = new MemeBuilder().WithName("MemeB").WithExclusionTag("tag1").Build();
        var ideoA = new IdeoBuilder().WithName("A").AddMeme(memeA).Build();
        var ideoB = new IdeoBuilder().WithName("B").AddMeme(memeB).Build();

        var diff = GameComponent_EnhancedIdeology.BeliefDifferences(ideoA, ideoB);

        Assert.Equal(1, diff);
    }

    // Builds a pawn whose own ideo optionally carries pawnMeme and returns their structural opinion of a
    // foreign ideo that holds the pawn's own preferred stance. The shared stance lifts the base structural
    // value clear of the 0 floor, so the meme term's sign (Supremacist -20, Guilty +10) is observable
    // instead of being clamped away.
    private static float StructuralOpinionTowardPlainForeign(MemeDef? pawnMeme)
    {
        // Reseed so the two worlds compared in a test draw identical per-issue strengths - the only thing
        // that should differ between them is the meme term under test.
        Rand.SetSeed(1);
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("Diet", "Permissive", "Forbidding");

        var pawnBuilder = new IdeoBuilder().WithName("Own").AddPrecept(rungs[0], issue, displayOrderInIssue: 0);
        if (pawnMeme != null)
            pawnBuilder.AddMeme(pawnMeme);
        var pawnIdeo = pawnBuilder.Build();
        var foreignIdeo = new IdeoBuilder().WithName("Foreign").AddPrecept(rungs[0], issue, displayOrderInIssue: 0).Build();
        world.AddIdeo(pawnIdeo);
        world.AddIdeo(foreignIdeo);

        var pawn = new PawnBuilder().WithIdeo(pawnIdeo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        return tracker.StructuralIdeoOpinion(foreignIdeo);
    }
}
