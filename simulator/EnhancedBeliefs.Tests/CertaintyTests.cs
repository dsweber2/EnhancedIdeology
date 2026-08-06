namespace EnhancedBeliefs.Tests;

public class CertaintyTests : SeededTest
{
    private static (SimWorld world, IdeoTrackerData tracker, SimPawn pawn) Setup(
        bool withPrecept,
        float colonyMoodOffset = 0f,
        float baseMood = 0.85f,
        float certainty = 0.75f)
    {
        var world = new SimWorld();
        world.Initialize();

        var builder = new IdeoBuilder().WithName("TestIdeo");
        if (withPrecept)
            builder.AddPrecept(new PreceptDef { defName = "TestPrecept" });
        var ideo = builder.Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder()
            .WithIdeo(ideo)
            .WithCertainty(certainty)
            .WithBaseMood(baseMood)
            .WithColonyMoodOffset(colonyMoodOffset)
            .WithLabel("P")
            .Build(world);

        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        return (world, tracker, pawn);
    }

    private static IdeoTrackerData Recache(SimWorld world, SimPawn pawn, IdeoTrackerData tracker)
    {
        pawn.UpdateThoughts();
        tracker.CertaintyChangeRecache(world.Comp);
        return tracker;
    }

    [Fact]
    public void Drift_CertaintyAboveTarget_YieldsNegativeRate()
    {
        var (world, tracker, pawn) = Setup(withPrecept: true, certainty: 0.95f);
        Recache(world, pawn, tracker);

        Assert.True(tracker.CachedTargetCertainty < 0.95f);
        Assert.True(tracker.CachedCertaintyChange < 0f,
            $"Expected drift down toward target {tracker.CachedTargetCertainty}, got {tracker.CachedCertaintyChange}");
    }

    [Fact]
    public void Drift_CertaintyBelowTarget_YieldsPositiveRate()
    {
        var (world, tracker, pawn) = Setup(withPrecept: true, colonyMoodOffset: 10f, certainty: 0.05f);
        Recache(world, pawn, tracker);

        Assert.True(tracker.CachedTargetCertainty > 0.05f);
        Assert.True(tracker.CachedCertaintyChange > 0f,
            $"Expected drift up toward target {tracker.CachedTargetCertainty}, got {tracker.CachedCertaintyChange}");
    }

    [Fact]
    public void Drift_AtTarget_YieldsZeroRate()
    {
        var (world, tracker, pawn) = Setup(withPrecept: true);
        Recache(world, pawn, tracker);

        pawn.ideo.Certainty = tracker.CachedTargetCertainty;
        Recache(world, pawn, tracker);

        Assert.Equal(0f, tracker.CachedCertaintyChange, precision: 5);
    }

    [Fact]
    public void Drift_RateEqualsDriftRateTimesGap()
    {
        var (world, tracker, pawn) = Setup(withPrecept: true, certainty: 0.5f);
        Recache(world, pawn, tracker);

        var expected = EnhancedBeliefsMod.Settings.CertaintyDriftRate * (tracker.CachedTargetCertainty - 0.5f);
        Assert.Equal(expected, tracker.CachedCertaintyChange, precision: 5);
    }

    [Fact]
    public void Practice_PositivePreceptMood_RaisesTarget()
    {
        var (withMood, moodTracker, moodPawn) = Setup(withPrecept: true, colonyMoodOffset: 20f);
        Recache(withMood, moodPawn, moodTracker);

        var (noMood, flatTracker, flatPawn) = Setup(withPrecept: true, colonyMoodOffset: 0f);
        Recache(noMood, flatPawn, flatTracker);

        Assert.True(moodTracker.CachedPractitional > 0f);
        Assert.True(moodTracker.CachedTargetCertainty > flatTracker.CachedTargetCertainty);
    }

    [Fact]
    public void Practice_NoPreceptThought_YieldsZeroPractitionalBand()
    {
        // Ideo with no precepts -> SimThought.SourcePrecept = null -> no practitional contribution,
        // even with a large colony mood offset.
        var (world, tracker, pawn) = Setup(withPrecept: false, colonyMoodOffset: 20f);
        Recache(world, pawn, tracker);

        Assert.Equal(0f, tracker.CachedPractitional, precision: 6);
    }

    [Fact]
    public void Practice_MaxRange_CapsPractitionalBand()
    {
        var (world, tracker, pawn) = Setup(withPrecept: true, colonyMoodOffset: 1000f);
        Recache(world, pawn, tracker);

        Assert.Equal(EnhancedBeliefsMod.Settings.PracticeMaxRange, tracker.CachedPractitional, precision: 5);
    }

    [Fact]
    public void Target_StaysWithinUnitInterval_UnderExtremeNegativeInputs()
    {
        var (world, tracker, pawn) = Setup(withPrecept: true, colonyMoodOffset: -1000f, baseMood: 0.2f);
        Recache(world, pawn, tracker);

        Assert.InRange(tracker.CachedTargetCertainty, 0f, 1f);
    }

    [Fact]
    public void Relational_LikedCoReligionists_RaiseTarget()
    {
        var (world, tracker, pawn) = RelationalSetup(opinion: 80f);
        Recache(world, pawn, tracker);

        Assert.True(tracker.CachedRelational > 0f);
    }

    [Fact]
    public void Relational_HatedCoReligionists_LowerTarget()
    {
        var (world, tracker, pawn) = RelationalSetup(opinion: -80f);
        Recache(world, pawn, tracker);

        Assert.True(tracker.CachedRelational < 0f);
    }

    [Fact]
    public void Relational_NoCoReligionists_YieldsZeroBand()
    {
        var (world, tracker, pawn) = Setup(withPrecept: true);
        Recache(world, pawn, tracker);

        Assert.Equal(0f, tracker.CachedRelational, precision: 6);
    }

    private static (SimWorld world, IdeoTrackerData tracker, SimPawn pawn) RelationalSetup(float opinion)
    {
        var world = new SimWorld();
        world.Initialize();

        var ideo = new IdeoBuilder().WithName("I").AddPrecept(new PreceptDef { defName = "P" }).Build();
        world.AddIdeo(ideo);

        var other = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("Other").Build(world);
        var pawn = new PawnBuilder()
            .WithIdeo(ideo)
            .WithCertainty(0.5f)
            .WithLabel("P")
            .WithOpinionOf(other, opinion)
            .Build(world);

        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        return (world, tracker, pawn);
    }
}
