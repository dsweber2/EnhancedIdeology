namespace EnhancedIdeology.Tests;

public class ConvictionDecayTests : SeededTest
{
    private static (SimWorld world, IdeoTrackerData tracker, IssueDef issue) Setup()
    {
        var world = new SimWorld();
        world.Initialize();
        PreceptPolicy.RegisterCategory("Decay_Issue", PreceptCategory.Moral);
        var (issue, rungs) = SimIssues.Ladder("Decay_Issue", "Rung0", "Rung1");
        var ideo = new IdeoBuilder().WithName("Decay").AddPrecept(rungs[0], issue, displayOrderInIssue: 0).Build();
        world.AddIdeo(ideo);
        var pawn = new PawnBuilder().WithIdeo(ideo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        // Force seeding before the test touches TicksAbs so the Rand stream is consumed predictably.
        _ = tracker.IssueStances().ToList();
        return (world, tracker, issue);
    }

    [Fact]
    public void ConvictionDecay_FirstCallOnNewDay_ReducesAllStrengths()
    {
        var (_, tracker, issue) = Setup();
        var before = tracker.IssueStances().First(ss => ss.issue == issue).strength;

        // _lastDecayDay starts at -1; day 0 is a new day.
        GenTicks.TicksAbs = 0;
        tracker.ApplyConvictionDecayIfNewDay();

        var after = tracker.IssueStances().First(ss => ss.issue == issue).strength;
        Assert.True(after < before, $"Expected strength to decrease after first decay. before={before}, after={after}");
    }

    [Fact]
    public void ConvictionDecay_SameDaySecondCall_IsIdempotent()
    {
        var (_, tracker, issue) = Setup();
        GenTicks.TicksAbs = 0;
        tracker.ApplyConvictionDecayIfNewDay();
        var afterFirst = tracker.IssueStances().First(ss => ss.issue == issue).strength;

        tracker.ApplyConvictionDecayIfNewDay();

        var afterSecond = tracker.IssueStances().First(ss => ss.issue == issue).strength;
        Assert.Equal(afterFirst, afterSecond, precision: 6);
    }

    [Fact]
    public void ConvictionDecay_NextDay_ReducesStrengthAgain()
    {
        var (_, tracker, issue) = Setup();
        GenTicks.TicksAbs = 0;
        tracker.ApplyConvictionDecayIfNewDay();
        var afterDay0 = tracker.IssueStances().First(ss => ss.issue == issue).strength;

        GenTicks.TicksAbs = GenDate.TicksPerDay;
        tracker.ApplyConvictionDecayIfNewDay();

        var afterDay1 = tracker.IssueStances().First(ss => ss.issue == issue).strength;
        Assert.True(afterDay1 < afterDay0, $"Expected further decay on next day. day0={afterDay0}, day1={afterDay1}");
    }
}
