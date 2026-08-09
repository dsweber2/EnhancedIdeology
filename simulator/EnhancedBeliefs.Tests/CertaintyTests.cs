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
        {
            // A registered issue stance gives the pawn a per-issue structural floor (own ideo agrees with
            // itself, so ~mean(strength)·5). The old flat +30 base is gone, so this rung is what now keeps
            // the structural band non-zero.
            var (issue, rungs) = SimIssues.Ladder("TestIssue", "TestPrecept", "TestPreceptHigh");
            builder.AddPrecept(rungs[0], issue, displayOrderInIssue: 0);
        }
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

    // A fresh pawn's first recache seeds certainty to its setpoint. To exercise drift we model the sequel: an
    // event pushes certainty off the setpoint, then the next recache measures the pull back.
    private static void SeedThenPush(SimWorld world, SimPawn pawn, IdeoTrackerData tracker, float certainty)
    {
        Recache(world, pawn, tracker);
        pawn.ideo.Certainty = certainty;
        Recache(world, pawn, tracker);
    }

    [Fact]
    public void Drift_CertaintyAboveTarget_YieldsNegativeRate()
    {
        var (world, tracker, pawn) = Setup(withPrecept: true);
        SeedThenPush(world, pawn, tracker, 0.95f);

        Assert.True(tracker.CachedTargetCertainty < 0.95f);
        Assert.True(tracker.CachedCertaintyChange < 0f,
            $"Expected drift down toward target {tracker.CachedTargetCertainty}, got {tracker.CachedCertaintyChange}");
    }

    [Fact]
    public void Drift_CertaintyBelowTarget_YieldsPositiveRate()
    {
        var (world, tracker, pawn) = Setup(withPrecept: true, colonyMoodOffset: 10f);
        SeedThenPush(world, pawn, tracker, 0.05f);

        Assert.True(tracker.CachedTargetCertainty > 0.05f);
        Assert.True(tracker.CachedCertaintyChange > 0f,
            $"Expected drift up toward target {tracker.CachedTargetCertainty}, got {tracker.CachedCertaintyChange}");
    }

    [Fact]
    public void Spawn_FirstRecache_SeedsCertaintyToSetpoint()
    {
        // A fresh pawn starts at equilibrium: the first setpoint computed pins certainty to it, whatever it was
        // seeded with, so there is no spurious drift on day one.
        var (world, tracker, pawn) = Setup(withPrecept: true, certainty: 0.3f);
        Recache(world, pawn, tracker);

        Assert.Equal(tracker.CachedTargetCertainty, pawn.ideo.Certainty, precision: 5);
        Assert.Equal(0f, tracker.CachedCertaintyChange, precision: 5);
    }

    [Fact]
    public void Spawn_LaterRecache_DoesNotReseed()
    {
        // Only the first recache seeds; afterwards an event that moves certainty must survive the next recache.
        var (world, tracker, pawn) = Setup(withPrecept: true);
        Recache(world, pawn, tracker);
        pawn.ideo.Certainty = 0.2f;
        Recache(world, pawn, tracker);

        Assert.Equal(0.2f, pawn.ideo.Certainty, precision: 5);
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
        var (world, tracker, pawn) = Setup(withPrecept: true);
        SeedThenPush(world, pawn, tracker, 0.5f);

        var expected = EnhancedBeliefsMod.Settings.CertaintyDriftRate * (tracker.CachedTargetCertainty - 0.5f);
        Assert.Equal(expected, tracker.CachedCertaintyChange, precision: 5);
    }

    [Fact]
    public void Practice_PositivePreceptMood_RaisesTarget()
    {
        // One pawn recached twice isolates the practitional band: the structural band comes from the same
        // once-seeded convictions, so only the precept mood changes between the two targets. A weakening trait
        // keeps the single-issue structural well under saturation, leaving headroom for the mood to move it.
        var (world, tracker, pawn) = Setup(withPrecept: true, colonyMoodOffset: 0f);
        pawn.story.traits.allTraits.Add(new Trait { def = new TraitDef { defName = "Nerves" }, Degree = -2 });

        Recache(world, pawn, tracker);
        var flatTarget = tracker.CachedTargetCertainty;

        pawn.ColonyMoodOffset = 20f;
        Recache(world, pawn, tracker);

        Assert.True(tracker.CachedPractitional > 0f);
        Assert.True(tracker.CachedTargetCertainty > flatTarget);
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
