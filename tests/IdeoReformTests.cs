namespace EnhancedIdeology.Tests;

public class IdeoReformTests : SeededTest
{
    private static (SimWorld world, Ideo ideo, IssueDef issue, PreceptDef[] rungs) Setup()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("IssueA", "A0", "A1", "A2");
        var ideo = new IdeoBuilder().WithName("Reformable").AddPrecept(rungs[1]).Build();
        world.AddIdeo(ideo);
        return (world, ideo, issue, rungs);
    }

    private static (float rank, float strength) Stance(IdeoTrackerData tracker, IssueDef issue)
    {
        var stance = tracker.IssueStances().First(entry => entry.issue == issue);
        return (stance.rank, stance.strength);
    }

    private static void Reform(SimWorld world, Ideo ideo, IssueDef issue, PreceptDef newRung)
    {
        var oldHeldRanks = IdeoReform.HeldRanks(ideo);
        ideo.precepts.First(precept => precept.def.issue == issue).def = newRung;
        IdeoReform.CarryOrthodoxStances(world.Comp, ideo, oldHeldRanks);
    }

    [Fact]
    public void OrthodoxBeliever_FollowsNewRungAtHalfStrength()
    {
        var (world, ideo, issue, rungs) = Setup();
        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIssueStance(issue, PreceptLadder.RankOf(rungs[1]), 20f);

        Reform(world, ideo, issue, rungs[2]);

        var (rank, strength) = Stance(tracker, issue);
        Assert.Equal(PreceptLadder.RankOf(rungs[2]), rank);
        Assert.Equal(10f, strength, 4);
    }

    [Fact]
    public void HeterodoxBeliever_KeepsStance()
    {
        var (world, ideo, issue, rungs) = Setup();
        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIssueStance(issue, PreceptLadder.RankOf(rungs[0]), 20f);

        Reform(world, ideo, issue, rungs[2]);

        Assert.Equal((PreceptLadder.RankOf(rungs[0]), 20f), Stance(tracker, issue));
    }

    [Fact]
    public void UnchangedIssue_KeepsStrength()
    {
        var (world, ideo, issue, rungs) = Setup();
        var (otherIssue, otherRungs) = SimIssues.Ladder("IssueB", "B0", "B1");
        ideo.precepts.Add(new Precept { def = otherRungs[1], ideo = ideo });
        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIssueStance(otherIssue, PreceptLadder.RankOf(otherRungs[1]), 20f);

        Reform(world, ideo, issue, rungs[2]);

        Assert.Equal((PreceptLadder.RankOf(otherRungs[1]), 20f), Stance(tracker, otherIssue));
    }

    [Fact]
    public void Schismatic_CarriesOrthodoxStancesToSplitIdeo()
    {
        var (world, ideo, issue, rungs) = Setup();
        var (otherIssue, otherRungs) = SimIssues.Ladder("IssueB", "B0", "B1", "B2");
        ideo.precepts.Add(new Precept { def = otherRungs[1], ideo = ideo });
        var split = new IdeoBuilder().WithName("Split").AddPrecept(rungs[2]).AddPrecept(otherRungs[2]).Build();
        world.AddIdeo(split);

        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIssueStance(issue, PreceptLadder.RankOf(rungs[1]), 20f);
        tracker.SetIssueStance(otherIssue, PreceptLadder.RankOf(otherRungs[0]), 20f);

        var oldHeldRanks = IdeoReform.HeldRanks(ideo);
        pawn.ideo.SetIdeo(split);
        world.Comp.SetIdeo(pawn, split);
        IdeoReform.CarryOrthodoxStances(tracker, split, oldHeldRanks);

        var (rank, strength) = Stance(tracker, issue);
        Assert.Equal(PreceptLadder.RankOf(rungs[2]), rank);
        Assert.Equal(10f, strength, 4);
        Assert.Equal((PreceptLadder.RankOf(otherRungs[0]), 20f), Stance(tracker, otherIssue));
    }

    [Fact]
    public void OtherIdeoFollower_Untouched()
    {
        var (world, ideo, issue, rungs) = Setup();
        var rival = new IdeoBuilder().WithName("Rival").AddPrecept(rungs[1]).Build();
        world.AddIdeo(rival);
        var outsider = new PawnBuilder().WithIdeo(rival).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(outsider);
        tracker.SetIssueStance(issue, PreceptLadder.RankOf(rungs[1]), 20f);

        Reform(world, ideo, issue, rungs[2]);

        Assert.Equal((PreceptLadder.RankOf(rungs[1]), 20f), Stance(tracker, issue));
    }
}
