namespace EnhancedBeliefs.Tests;

public class CertaintyTests
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

    [Fact]
    public void CertaintyChangeRecache_PositivePreceptMood_YieldsPositiveRate()
    {
        var (world, tracker, pawn) = Setup(withPrecept: true, colonyMoodOffset: 10f);
        pawn.UpdateThoughts();
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.True(tracker.CachedCertaintyChange > 0f,
            $"Expected positive certainty rate, got {tracker.CachedCertaintyChange}");
    }

    [Fact]
    public void CertaintyChangeRecache_NegativePreceptMood_YieldsNegativeRate()
    {
        // Mood < 0.8 also needed to allow inactivity loss; use large negative offset
        var (world, tracker, pawn) = Setup(withPrecept: true, colonyMoodOffset: -30f, baseMood: 0.5f);
        pawn.UpdateThoughts();
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.True(tracker.CachedCertaintyChange < 0f,
            $"Expected negative certainty rate, got {tracker.CachedCertaintyChange}");
    }

    [Fact]
    public void CertaintyChangeRecache_NoPreceptThought_HighMood_YieldsZeroRate()
    {
        // Ideo with no precepts → SimThought.SourcePrecept = null → moodSum = 0
        // Mood >= 0.8 → inactivity penalty also blocked
        var (world, tracker, pawn) = Setup(withPrecept: false, baseMood: 0.85f);
        pawn.UpdateThoughts();
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.Equal(0f, tracker.CachedCertaintyChange, precision: 6);
    }

    [Fact]
    public void CertaintyChangeRecache_InactivityPenalty_LowMoodAfterThreeDays_YieldsNegativeRate()
    {
        // No precept thought → moodSum = 0; but mood < 0.8 and 4 days elapsed → inactivity loss
        var (world, tracker, pawn) = Setup(withPrecept: false, baseMood: 0.7f);
        Find.TickManager.TicksGame = 4 * GenDate.TicksPerDay;

        pawn.UpdateThoughts();
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.True(tracker.CachedCertaintyChange < 0f,
            $"Expected inactivity penalty to produce negative rate, got {tracker.CachedCertaintyChange}");
    }

    [Fact]
    public void CertaintyChangeRecache_InactivityPenalty_BlockedByHighMood()
    {
        // Same timing, but mood >= 0.8 → penalty suppressed
        var (world, tracker, pawn) = Setup(withPrecept: false, baseMood: 0.85f);
        Find.TickManager.TicksGame = 4 * GenDate.TicksPerDay;

        pawn.UpdateThoughts();
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.Equal(0f, tracker.CachedCertaintyChange, precision: 6);
    }

    [Fact]
    public void CertaintyChangeRecache_InactivityPenalty_BlockedWithinThreeDays()
    {
        // Low mood but only 2 days elapsed → below 3-day threshold
        var (world, tracker, pawn) = Setup(withPrecept: false, baseMood: 0.7f);
        Find.TickManager.TicksGame = 2 * GenDate.TicksPerDay;

        pawn.UpdateThoughts();
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.Equal(0f, tracker.CachedCertaintyChange, precision: 6);
    }

    [Fact]
    public void CertaintyChangeRecache_InactivityPenalty_BlockedAtExactlyThreeDays()
    {
        // Condition is "> 3 days", so exactly 3*TicksPerDay must not trigger
        var (world, tracker, pawn) = Setup(withPrecept: false, baseMood: 0.7f);
        Find.TickManager.TicksGame = 3 * GenDate.TicksPerDay;

        pawn.UpdateThoughts();
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.Equal(0f, tracker.CachedCertaintyChange, precision: 6);
    }
}
