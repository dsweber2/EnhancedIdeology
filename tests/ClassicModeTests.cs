namespace EnhancedIdeology.Tests;

public class ClassicModeTests : SeededTest
{
    private static (SimWorld world, IdeoTrackerData tracker, IssueDef issue, Precept precept) Setup()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        Find.IdeoManager.classicMode = true;
        return (world, tracker, issue, ideo.precepts.First(p => p.def == rungs[0]));
    }

    [Fact]
    public void SetIssueStance_DoesNothing()
    {
        var (_, tracker, issue, _) = Setup();
        var before = tracker.IssueStances().First(s => s.issue == issue);

        tracker.SetIssueStance(issue, 1f, before.strength + 3f);

        Assert.Equal(before, tracker.IssueStances().First(s => s.issue == issue));
    }

    [Fact]
    public void ShiftIssueStance_DoesNothing()
    {
        var (_, tracker, issue, _) = Setup();
        var before = tracker.IssueStances().First(s => s.issue == issue);

        tracker.ShiftIssueStance(issue, 1f, 0.5f, 3f);

        Assert.Equal(before, tracker.IssueStances().First(s => s.issue == issue));
    }

    [Fact]
    public void MoodletShift_DoesNothing()
    {
        var (world, tracker, issue, precept) = Setup();
        var before = tracker.IssueStances().First(s => s.issue == issue);
        tracker.Pawn.needs.mood.thoughts.memories.Memories.Add(
            new SimMemory(precept, 8f));

        world.Comp.ApplyMoodletConvictionShifts(tracker);

        Assert.Equal(before, tracker.IssueStances().First(s => s.issue == issue));
    }
}
