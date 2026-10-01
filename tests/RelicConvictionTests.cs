namespace EnhancedIdeology.Tests;

public class RelicConvictionTests : SeededTest
{
    private sealed class RelicMemory : Thought_Memory
    {
        private readonly float _moodOffset;

        public RelicMemory(ThoughtDef thoughtDef, Precept source, float moodOffset)
        {
            def = thoughtDef;
            sourcePrecept = source;
            _moodOffset = moodOffset;
        }

        public override float MoodOffset() => _moodOffset;
    }

    private static (SimWorld world, Ideo ideo, Precept_Relic relic, IssueDef[] issues, PreceptDef[] rungsA) Setup()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issueA, rungsA) = SimIssues.Ladder("IssueA", "A0", "A1", "A2");
        var (issueB, rungsB) = SimIssues.Ladder("IssueB", "B0", "B1", "B2");
        var ideo = new IdeoBuilder().WithName("RelicFaith").AddPrecept(rungsA[1]).AddPrecept(rungsB[1]).Build();
        var relic = new Precept_Relic { ideo = ideo };
        ideo.precepts.Add(relic);
        world.AddIdeo(ideo);
        return (world, ideo, relic, [issueA, issueB], rungsA);
    }

    private static float Strength(IdeoTrackerData tracker, IssueDef issue) =>
        tracker.IssueStances().First(stance => stance.issue == issue).strength;

    [Fact]
    public void IsRelicThought_MatchesOnlyRelicDefs()
    {
        Assert.True(RelicConviction.IsRelicThought(ThoughtDefOf.RelicLost));
        Assert.True(RelicConviction.IsRelicThought(ThoughtDefOf.RelicDestroyed));
        Assert.True(RelicConviction.IsRelicThought(ThoughtDefOf.RelicsCollected));
        Assert.True(RelicConviction.IsRelicThought(ThoughtDefOf.RelicAtRitual));
        Assert.False(RelicConviction.IsRelicThought(ThoughtDefOf.FailedConvertIdeoAttemptResentment));
    }

    [Fact]
    public void SourceFor_ReturnsRelicPreceptOfPawnIdeo()
    {
        var (world, _, relic, _, _) = Setup();
        var follower = new PawnBuilder().WithIdeo(relic.ideo!).Build(world);

        var plainIdeo = new IdeoBuilder().WithName("NoRelics").Build();
        world.AddIdeo(plainIdeo);
        var outsider = new PawnBuilder().WithIdeo(plainIdeo).Build(world);

        Assert.Same(relic, RelicConviction.SourceFor(follower));
        Assert.Null(RelicConviction.SourceFor(outsider));
    }

    [Fact]
    public void FindBoost_RaisesConvictionOnEveryMoralIssueOfFollowers()
    {
        var (world, ideo, relic, issues, _) = Setup();
        var follower = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(follower);
        foreach (var issue in issues)
            tracker.SetIssueStance(issue, IssueStanceTracker.HeldRank(ideo, issue), 5f);

        RelicConviction.ApplyFindBoost(world.Comp, relic);

        foreach (var issue in issues)
            Assert.True(Strength(tracker, issue) > 5f, $"{issue.defName} conviction should rise on relic find");
    }

    [Fact]
    public void FindBoost_SkipsOtherIdeosAndTheDead()
    {
        var (world, ideo, relic, issues, rungsA) = Setup();
        var foreignIdeo = new IdeoBuilder().WithName("Rival").AddPrecept(rungsA[0]).Build();
        world.AddIdeo(foreignIdeo);

        var outsider = new PawnBuilder().WithIdeo(foreignIdeo).Build(world);
        var outsiderTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(outsider);
        var outsiderBefore = Strength(outsiderTracker, issues[0]);

        var corpse = new PawnBuilder().WithIdeo(ideo).Build(world);
        corpse.Dead = true;
        var corpseTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(corpse);
        var corpseBefore = issues.Select(issue => Strength(corpseTracker, issue)).ToList();

        RelicConviction.ApplyFindBoost(world.Comp, relic);

        Assert.Equal(outsiderBefore, Strength(outsiderTracker, issues[0]));
        Assert.Equal(corpseBefore, issues.Select(issue => Strength(corpseTracker, issue)).ToList());
    }

    [Theory]
    [InlineData(15f)]
    [InlineData(-5f)]
    public void MoodletShift_SpreadsMoodEquivalentDeltaAcrossMoralIssues(float moodOffset)
    {
        var (world, ideo, relic, issues, _) = Setup();
        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var before = issues.Select(issue => Strength(tracker, issue)).ToList();

        pawn.needs.mood.thoughts.memories.Memories.Add(new RelicMemory(ThoughtDefOf.RelicsCollected, relic, moodOffset));
        world.Comp.ApplyMoodletConvictionShifts();

        var expectedTotal = moodOffset
            * EnhancedIdeologyMod.Settings.ConversionStancePull
            * GameComponent_EnhancedIdeology.MoodletConvictionScalar;
        var deltas = issues.Select((issue, ii) => Strength(tracker, issue) - before[ii]).ToList();
        foreach (var delta in deltas)
            Assert.Equal(expectedTotal / issues.Length, delta, precision: 5);
        Assert.Equal(expectedTotal, deltas.Sum(), precision: 5);
    }

    [Fact]
    public void MoodletShift_IgnoresRelicThoughtFromFormerIdeo()
    {
        var (world, ideo, relic, issues, rungsA) = Setup();
        var newIdeo = new IdeoBuilder().WithName("NewFaith").AddPrecept(rungsA[2]).Build();
        world.AddIdeo(newIdeo);

        var pawn = new PawnBuilder().WithIdeo(newIdeo).Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var before = Strength(tracker, issues[0]);

        pawn.needs.mood.thoughts.memories.Memories.Add(new RelicMemory(ThoughtDefOf.RelicLost, relic, -5f));
        world.Comp.ApplyMoodletConvictionShifts();

        Assert.Equal(before, Strength(tracker, issues[0]));
        Assert.NotSame(ideo, pawn.Ideo);
    }
}
