namespace EnhancedIdeology.Tests;

// Covers when the certainty setpoint reuses cached band inputs and when it recomputes them.
// The structural band is kept until its inputs change; the relational band reads opinions cached at the long tick.
public class SetpointCachingTests : SeededTest
{
    private static (SimWorld world, IdeoTrackerData tracker, SimPawn pawn, IssueDef issue) BuildStructural()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("Generosity", "Selfish", "Generous");
        var ideo = new IdeoBuilder().WithName("I").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.CertaintyChangeRecache(world.Comp);
        return (world, tracker, pawn, issue);
    }

    private static (SimWorld world, IdeoTrackerData tracker, SimPawn pawn, SimPawn friend) BuildFriendship(float opinion)
    {
        var world = new SimWorld();
        world.Initialize();

        var ideo = new IdeoBuilder().WithName("I").AddPrecept(new PreceptDef { defName = "P" }).Build();
        world.AddIdeo(ideo);

        var friend = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("Friend").Build(world);
        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("P")
            .WithOpinionOf(friend, opinion)
            .Build(world);

        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.CertaintyChangeRecache(world.Comp);
        return (world, tracker, pawn, friend);
    }

    [Fact]
    public void Structural_SilentStanceChange_KeepsCachedBand()
    {
        var (world, tracker, _, issue) = BuildStructural();
        var before = tracker.CachedStructural;
        var contributorsBefore = tracker.StructuralContributors.Count;

        // A write that skips IdeoTrackerData sends no invalidation, so the cached band stays.
        tracker.Stances.SetStance(issue, 0f, tracker.Stances.GetStrength(issue));
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.Equal(before, tracker.CachedStructural);
        Assert.Equal(contributorsBefore, tracker.StructuralContributors.Count);
    }

    [Fact]
    public void Structural_StanceWrite_RecomputesBand()
    {
        var (world, tracker, _, issue) = BuildStructural();
        var before = tracker.CachedStructural;

        tracker.SetIssueStance(issue, 0f, tracker.Stances.GetStrength(issue));
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.True(tracker.CachedStructural < before,
            $"moving off the held rung should lower structural fit: {before} -> {tracker.CachedStructural}");
    }

    [Fact]
    public void Structural_SlowRefresh_PicksUpSilentChange()
    {
        var (world, tracker, _, issue) = BuildStructural();
        var before = tracker.CachedStructural;

        tracker.Stances.SetStance(issue, 0f, tracker.Stances.GetStrength(issue));
        tracker.RefreshSlowInputs();
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.True(tracker.CachedStructural < before);
    }

    [Fact]
    public void Structural_TraitRecache_RecomputesBand()
    {
        var (world, tracker, _, issue) = BuildStructural();
        var before = tracker.CachedStructural;

        tracker.Stances.SetStance(issue, 0f, tracker.Stances.GetStrength(issue));
        tracker.RecacheAllBaseOpinions();
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.True(tracker.CachedStructural < before);
    }

    [Fact]
    public void Relational_OpinionChange_WaitsForSlowRefresh()
    {
        var (world, tracker, pawn, friend) = BuildFriendship(80f);
        var before = tracker.CachedRelational;
        Assert.True(before > 0f);

        pawn.relations.SetOpinion(friend, -80f);
        tracker.CertaintyChangeRecache(world.Comp);
        Assert.Equal(before, tracker.CachedRelational);

        tracker.RefreshSlowInputs();
        tracker.CertaintyChangeRecache(world.Comp);
        Assert.True(tracker.CachedRelational < 0f);
    }

    [Fact]
    public void Relational_DeadCoReligionist_DropsOutOnRefresh()
    {
        var (world, tracker, _, friend) = BuildFriendship(80f);
        Assert.Single(tracker.RelationalContributors);

        friend.Dead = true;
        tracker.RefreshSlowInputs();
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.Empty(tracker.RelationalContributors);
        Assert.Equal(0f, tracker.CachedRelational);
    }

    [Fact]
    public void CategoryOf_RegisterAfterLookup_TakesEffect()
    {
        var issue = new IssueDef { defName = "CacheProbe" };
        Assert.Equal(PreceptCategory.PositiveOnly, PreceptPolicy.CategoryOf(issue));

        PreceptPolicy.RegisterCategory("CacheProbe", PreceptCategory.Moral);
        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(issue));

        PreceptPolicy.ClearOverrides();
        Assert.Equal(PreceptCategory.PositiveOnly, PreceptPolicy.CategoryOf(issue));
    }
}
