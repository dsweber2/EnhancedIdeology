namespace EnhancedIdeology.Tests;

public class PrayerTests : SeededTest
{
    // Must match JobDriver_Pray.PrayerArc — copied here to avoid pulling Verse.AI into the sim.
    private const float PrayerArc = 0.5f;

    // One prayer-arc step on an orthodox pawn with low conviction increases conviction strength.
    [Fact]
    public void PrayerStep_LowConviction_IncreasesStrength()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("Pawn").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var orthodoxRank = IdeoTrackerData.HeldRank(ideo, issue);
        tracker.SetIssueStance(issue, orthodoxRank, 5f);

        ConvictionMath.ApplyRitualPull(world.Comp, pawn, issue, orthodoxRank,
            IdeoTrackerData.AbsoluteMaxConvictionStrength, PrayerArc);

        var after = tracker.IssueStances().First(ss => ss.issue == issue);
        Assert.True(after.strength > 5f, $"prayer should increase conviction; got {after.strength}");
    }

    // PrayerArc (0.5) is much smaller than a ritual arc (1.0 per quality tier), so a single session
    // produces only a modest boost — conviction should still be below the normal cap after one step.
    [Fact]
    public void PrayerStep_SingleSession_ModestGain()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("Pawn").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var orthodoxRank = IdeoTrackerData.HeldRank(ideo, issue);
        tracker.SetIssueStance(issue, orthodoxRank, 5f);

        ConvictionMath.ApplyRitualPull(world.Comp, pawn, issue, orthodoxRank,
            IdeoTrackerData.AbsoluteMaxConvictionStrength, PrayerArc);

        var after = tracker.IssueStances().First(ss => ss.issue == issue);
        Assert.True(after.strength < IdeoTrackerData.MaxConvictionStrength,
            $"one prayer session should not reach the normal conviction cap; got {after.strength}");
    }

    // At AbsoluteMaxConvictionStrength the arc can't push conviction higher — strength stays clamped.
    [Fact]
    public void PrayerStep_AlreadyAtAbsoluteMax_NothingChanges()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.9f).WithLabel("Pawn").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var orthodoxRank = IdeoTrackerData.HeldRank(ideo, issue);
        tracker.SetIssueStance(issue, orthodoxRank, IdeoTrackerData.AbsoluteMaxConvictionStrength);

        ConvictionMath.ApplyRitualPull(world.Comp, pawn, issue, orthodoxRank,
            IdeoTrackerData.AbsoluteMaxConvictionStrength, PrayerArc);

        var after = tracker.IssueStances().First(ss => ss.issue == issue);
        Assert.Equal(IdeoTrackerData.AbsoluteMaxConvictionStrength, after.strength);
        Assert.Equal(orthodoxRank, after.rank, precision: 4);
    }

    // A heterodox pawn (off their ideo's orthodox rung) gets nudged toward the orthodox rung by prayer.
    [Fact]
    public void PrayerStep_HeterodoxPawn_RankMovesTowardOrthodox()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("Pawn").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var orthodoxRank = IdeoTrackerData.HeldRank(ideo, issue);
        var heterodoxRank = orthodoxRank + 1f; // one rung off
        tracker.SetIssueStance(issue, heterodoxRank, 10f);

        ConvictionMath.ApplyRitualPull(world.Comp, pawn, issue, orthodoxRank,
            IdeoTrackerData.AbsoluteMaxConvictionStrength, PrayerArc);

        var after = tracker.IssueStances().First(ss => ss.issue == issue);
        Assert.True(after.rank < heterodoxRank,
            $"prayer should pull rank toward orthodox {orthodoxRank}; got {after.rank} (started at {heterodoxRank})");
    }

    // StrengthFactor = 1 - strength/AbsMax: 1 at zero conviction, 0 at the cap, linear in between.
    [Theory]
    [InlineData(0f, 1f)]
    [InlineData(25f, 0.5f)]
    [InlineData(50f, 0f)]
    public void StrengthFactor_ScalesLinearlyToMax(float strength, float expected)
    {
        var actual = 1f - (strength / IdeoTrackerData.AbsoluteMaxConvictionStrength);
        Assert.Equal(expected, actual, precision: 4);
    }
}
