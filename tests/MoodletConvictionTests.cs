namespace EnhancedIdeology.Tests;

public class MoodletConvictionTests : SeededTest
{
    private static (SimWorld world, SimPawn pawn, IdeoTrackerData tracker, IssueDef issue, Precept precept) Setup(
        float certaintyLossFactor = 1f)
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertaintyLossFactor(certaintyLossFactor).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        return (world, pawn, tracker, issue, ideo.precepts.First(p => p.def == rungs[0]));
    }

    private static float Strength(IdeoTrackerData tracker, IssueDef issue) =>
        tracker.IssueStances().First(stance => stance.issue == issue).strength;

    private static float ExpectedDelta(float moodOffset, float certaintyLossFactor = 1f) =>
        moodOffset
        * certaintyLossFactor
        * EnhancedIdeologyMod.Settings.ConversionStancePull
        * GameComponent_EnhancedIdeology.MoodletConvictionScalar;

    [Fact]
    public void VanillaPreceptMemory_GoodMood_RaisesStrengthByDelta()
    {
        var (world, pawn, tracker, issue, precept) = Setup();
        var before = Strength(tracker, issue);

        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(precept, 8f));
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.Equal(before + ExpectedDelta(8f), Strength(tracker, issue), precision: 5);
    }

    [Fact]
    public void VanillaPreceptMemory_BadMood_LowersStrength()
    {
        var (world, pawn, tracker, issue, precept) = Setup();
        var before = Strength(tracker, issue);

        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(precept, -8f));
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.True(Strength(tracker, issue) < before);
    }

    [Fact]
    public void SituationalPreceptThought_GoodMood_RaisesStrengthByDelta()
    {
        var (world, pawn, tracker, issue, precept) = Setup();
        var before = Strength(tracker, issue);

        pawn.needs.mood.thoughts.SimulatedThoughts.Add(new SimThought { SourcePrecept = precept, MoodOffsetValue = 8f });
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.Equal(before + ExpectedDelta(8f), Strength(tracker, issue), precision: 5);
    }

    [Fact]
    public void SituationalPreceptThought_BadMood_LowersStrength()
    {
        var (world, pawn, tracker, issue, precept) = Setup();
        var before = Strength(tracker, issue);

        pawn.needs.mood.thoughts.SimulatedThoughts.Add(new SimThought { SourcePrecept = precept, MoodOffsetValue = -8f });
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.True(Strength(tracker, issue) < before);
    }

    [Fact]
    public void CertaintyLossFactor_ScalesShift()
    {
        var (world, pawn, tracker, issue, precept) = Setup(certaintyLossFactor: 2f);
        var before = Strength(tracker, issue);

        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(precept, 8f));
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.Equal(before + ExpectedDelta(8f, certaintyLossFactor: 2f), Strength(tracker, issue), precision: 5);
    }

    [Fact]
    public void CrossIdeoThought_DoesNotShiftConviction()
    {
        var (world, pawn, tracker, issue, _) = Setup();
        var (_, foreignRungs) = SimIssues.Ladder("ForeignIssue", "Permissive", "Forbidding");
        var foreignIdeo = new IdeoBuilder().WithName("ForeignIdeo").AddPrecept(foreignRungs[1]).Build();
        world.AddIdeo(foreignIdeo);
        var before = Strength(tracker, issue);

        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(foreignIdeo.precepts[0], 8f));
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.Equal(before, Strength(tracker, issue));
    }

    [Fact]
    public void CognitiveDissonance_DoesNotShiftConviction()
    {
        var (world, pawn, tracker, issue, precept) = Setup();
        var before = Strength(tracker, issue);

        pawn.needs.mood.thoughts.memories.Memories.Add(
            new Thought_CognitiveDissonance { sourcePrecept = precept, StoredMoodOffset = -8f });
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.Equal(before, Strength(tracker, issue));
    }

    [Fact]
    public void ZeroMood_DoesNotShiftConviction()
    {
        var (world, pawn, tracker, issue, precept) = Setup();
        var before = Strength(tracker, issue);

        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(precept, 0f));
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.Equal(before, Strength(tracker, issue));
    }

    [Fact]
    public void MemoryAndSituationalOnSameIssue_Accumulate()
    {
        var (world, pawn, tracker, issue, precept) = Setup();
        var before = Strength(tracker, issue);

        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(precept, 5f));
        pawn.needs.mood.thoughts.SimulatedThoughts.Add(new SimThought { SourcePrecept = precept, MoodOffsetValue = 3f });
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.Equal(before + ExpectedDelta(8f), Strength(tracker, issue), precision: 5);
    }

    private static (SimWorld world, SimPawn pawn, IdeoTrackerData tracker, IssueDef issue, Precept precept) HeterodoxSetup(
        float pawnRank)
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("HeterodoxIssue", "R0", "R1", "R2", "R3");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[2]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIssueStance(issue, pawnRank, 10f);
        return (world, pawn, tracker, issue, ideo.precepts.First(p => p.def == rungs[2]));
    }

    private static float Rank(IdeoTrackerData tracker, IssueDef issue) =>
        tracker.IssueStances().First(stance => stance.issue == issue).rank;

    [Fact]
    public void HeterodoxPawn_GoodMood_PullsRankTowardFaith()
    {
        var (world, pawn, tracker, issue, precept) = HeterodoxSetup(pawnRank: 0f);

        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(precept, 30f));
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.True(Rank(tracker, issue) > 0f);
    }

    [Fact]
    public void HeterodoxPawn_BadMood_HardensDissent()
    {
        var (world, pawn, tracker, issue, precept) = HeterodoxSetup(pawnRank: 1f);

        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(precept, -30f));
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.True(Rank(tracker, issue) < 1f);
        Assert.True(Strength(tracker, issue) > 10f);
    }

    [Fact]
    public void HeterodoxPawn_BadMood_StaysOnOwnSideOfFaith()
    {
        var (world, pawn, tracker, issue, precept) = HeterodoxSetup(pawnRank: 2.5f);

        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(precept, -30f));
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.True(Rank(tracker, issue) > 2.5f);
    }

    [Fact]
    public void OrthodoxPawn_RepeatedBadMoods_KeepOneSide()
    {
        var (world, pawn, tracker, issue, precept) = HeterodoxSetup(pawnRank: 2f);
        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(precept, -30f));

        var offsets = new List<float>();
        for (var ii = 0; ii < 20; ii++)
        {
            world.Comp.ApplyMoodletConvictionShifts();
            offsets.Add(Rank(tracker, issue) - 2f);
        }

        Assert.All(offsets, offset => Assert.Equal(Math.Sign(offsets[0]), Math.Sign(offset)));
        Assert.True(Math.Abs(offsets[^1]) > Math.Abs(offsets[0]));
    }

    [Fact]
    public void OrthodoxPawn_BadMood_LeavesRungAndWeakens()
    {
        var (world, pawn, tracker, issue, precept) = HeterodoxSetup(pawnRank: 2f);

        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(precept, -30f));
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.NotEqual(2f, Rank(tracker, issue));
        Assert.True(Strength(tracker, issue) < 10f);
    }

    [Fact]
    public void NaIssueThought_IsSkipped()
    {
        var world = new SimWorld();
        world.Initialize();

        var (naIssue, naRungs) = SimIssues.Ladder("TestNaIssue", "Only");
        PreceptPolicy.RegisterCategory(naIssue.defName, PreceptCategory.NA);
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(naRungs[0]).Build();
        world.AddIdeo(ideo);
        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);

        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(ideo.precepts[0], 8f));
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.DoesNotContain(world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn).IssueStances(), stance => stance.issue == naIssue);
    }

    [Fact]
    public void PreceptlessThought_OnPawnWithoutIdeo_IsSkipped()
    {
        var (world, pawn, _, _, _) = Setup();
        pawn.ideo.ideo = null;

        pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(null, 8f));
        world.Comp.ApplyMoodletConvictionShifts();
    }
}
