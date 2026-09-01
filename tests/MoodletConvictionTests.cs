namespace EnhancedIdeology.Tests;

public class MoodletConvictionTests : SeededTest
{
    private static Thought_MemeMemory MakeThought(Precept precept, float delta) => new()
    {
        sourcePrecept = precept,
        ConvictionDeltaPerTickLong = delta,
    };

    [Fact]
    public void SameIdeoThought_PositiveDelta_IncreasesIssueStrength()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var startStrength = tracker.IssueStances().First(s => s.issue == issue).strength;

        var precept = ideo.precepts.First(p => p.def == rungs[0]);
        pawn.needs.mood.thoughts.memories.Memories.Add(MakeThought(precept, 0.5f));

        world.Comp.ApplyMoodletConvictionShifts();

        var after = tracker.IssueStances().First(s => s.issue == issue);
        Assert.Equal(startStrength + 0.5f, after.strength, precision: 4);
    }

    [Fact]
    public void SameIdeoThought_NegativeDelta_DecreasesIssueStrength()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var startStrength = tracker.IssueStances().First(s => s.issue == issue).strength;

        var precept = ideo.precepts.First(p => p.def == rungs[0]);
        pawn.needs.mood.thoughts.memories.Memories.Add(MakeThought(precept, -0.5f));

        world.Comp.ApplyMoodletConvictionShifts();

        var after = tracker.IssueStances().First(s => s.issue == issue);
        Assert.Equal(startStrength - 0.5f, after.strength, precision: 4);
    }

    [Fact]
    public void CrossIdeoThought_DoesNotShiftConviction()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[0]).Build();
        var foreignIdeo = new IdeoBuilder().WithName("ForeignIdeo").AddPrecept(rungs[1]).Build();
        world.AddIdeo(ideo);
        world.AddIdeo(foreignIdeo);

        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var startStrength = tracker.IssueStances().First(s => s.issue == issue).strength;

        var foreignPrecept = foreignIdeo.precepts.First(p => p.def == rungs[1]);
        pawn.needs.mood.thoughts.memories.Memories.Add(MakeThought(foreignPrecept, 0.5f));

        world.Comp.ApplyMoodletConvictionShifts();

        var after = tracker.IssueStances().First(s => s.issue == issue);
        Assert.Equal(startStrength, after.strength, precision: 4);
    }

    [Fact]
    public void ZeroDelta_DoesNotShiftConviction()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var startStrength = tracker.IssueStances().First(s => s.issue == issue).strength;

        var precept = ideo.precepts.First(p => p.def == rungs[0]);
        pawn.needs.mood.thoughts.memories.Memories.Add(MakeThought(precept, 0f));

        world.Comp.ApplyMoodletConvictionShifts();

        var after = tracker.IssueStances().First(s => s.issue == issue);
        Assert.Equal(startStrength, after.strength, precision: 4);
    }

    [Fact]
    public void MultipleThoughtsOnSameIssue_DeltasAccumulate()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var startStrength = tracker.IssueStances().First(s => s.issue == issue).strength;

        var precept = ideo.precepts.First(p => p.def == rungs[0]);
        pawn.needs.mood.thoughts.memories.Memories.Add(MakeThought(precept, 0.3f));
        pawn.needs.mood.thoughts.memories.Memories.Add(MakeThought(precept, 0.2f));

        world.Comp.ApplyMoodletConvictionShifts();

        var after = tracker.IssueStances().First(s => s.issue == issue);
        Assert.Equal(startStrength + 0.5f, after.strength, precision: 4);
    }
}
