namespace EnhancedIdeology.Tests;

// Covers the trait-conviction shift to per-issue strength (design.md R2, 2b): strong-willed pawns hold
// beliefs more firmly, anxious / pessimistic / neurotic ones more weakly. The offset logic is pure, so it
// is tested directly; a single integration test confirms it is actually wired into seeding.
public class TraitConvictionTests : SeededTest
{
    private static List<Trait> Traits(params (string defName, int degree)[] traits) =>
        [.. traits.Select(t => new Trait { def = new TraitDef { defName = t.defName }, Degree = t.degree })];

    [Fact]
    public void NoTraits_YieldsZeroOffset()
    {
        Assert.Equal(0f, IdeoTrackerData.ConvictionOffsetFromTraits([]));
    }

    // expectedFactor is the signed multiple of ConvictionPerTraitDegree the trait should yield, so the
    // assertion tracks the tunable constant rather than a hardcoded point value.
    [Theory]
    [InlineData("Nerves", 2, 2f)]       // iron-willed
    [InlineData("Nerves", 1, 1f)]       // steadfast
    [InlineData("Nerves", -1, -1f)]     // nervous
    [InlineData("Nerves", -2, -2f)]     // volatile
    [InlineData("NaturalMood", -1, -1f)] // pessimist
    [InlineData("NaturalMood", -2, -2f)] // depressive
    [InlineData("Neurotic", 1, -1f)]     // neurotic (positive degree, weakens)
    [InlineData("Neurotic", 2, -2f)]     // very neurotic
    public void SingleTrait_ShiftsOffsetByDegree(string defName, int degree, float expectedFactor)
    {
        Assert.Equal(expectedFactor * IdeoTrackerData.ConvictionPerTraitDegree,
            IdeoTrackerData.ConvictionOffsetFromTraits(Traits((defName, degree))), precision: 4);
    }

    [Theory]
    [InlineData(1)] // optimist
    [InlineData(2)] // sanguine
    public void PositiveNaturalMood_HasNoEffect(int degree)
    {
        // Only the down side of NaturalMood weakens conviction; optimist / sanguine leave it untouched.
        Assert.Equal(0f, IdeoTrackerData.ConvictionOffsetFromTraits(Traits(("NaturalMood", degree))));
    }

    [Fact]
    public void WeakeningTraits_Stack()
    {
        // Volatile (-2) + depressive (-2) + very neurotic (-2) compound to -6 degrees of weakening.
        Assert.Equal(-6f * IdeoTrackerData.ConvictionPerTraitDegree, IdeoTrackerData.ConvictionOffsetFromTraits(
            Traits(("Nerves", -2), ("NaturalMood", -2), ("Neurotic", 2))), precision: 4);
    }

    [Fact]
    public void UnrelatedTraits_AreIgnored()
    {
        Assert.Equal(1f * IdeoTrackerData.ConvictionPerTraitDegree, IdeoTrackerData.ConvictionOffsetFromTraits(
            Traits(("Beauty", -2), ("Nerves", 1), ("Industriousness", 2))), precision: 4);
    }

    [Fact]
    public void IsWiredIntoSeeding_IronWilledHoldsBeliefsMoreFirmly()
    {
        // Integration: an iron-willed pawn's structural opinion of a mirror ideo (= mean strength * 5) beats
        // a volatile one's. The conviction gap (+3 vs -3 strength -> 30 opinion points) dwarfs the per-issue
        // RNG jitter across the two separately-built pawns, so the ordering is stable.
        Assert.True(OwnStructural("Nerves", 2) > OwnStructural("Nerves", -2));
    }

    // Structural opinion of an ideo identical to the pawn's own (= mean(strength) * 5, every issue agrees),
    // for a pawn carrying one personality trait. Rand is reseeded at the start so the base draw is fixed.
    private static float OwnStructural(string traitDefName, int degree)
    {
        Rand.SetSeed(1);
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("Diet", "Permissive", "Middle", "Forbidding");
        var ideo = new IdeoBuilder().WithName("Own").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        var mirror = new IdeoBuilder().WithName("Mirror").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        world.AddIdeo(ideo);
        world.AddIdeo(mirror);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithTrait(new TraitDef { defName = traitDefName }, degree)
            .WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        return tracker.StructuralIdeoOpinion(mirror);
    }
}
