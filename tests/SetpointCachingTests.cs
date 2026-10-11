namespace EnhancedIdeology.Tests;

// Covers when the certainty setpoint and the base opinions reuse cached inputs and when they recompute them.
// The structural band is kept until its inputs change; the relational band reads opinions cached at the long tick.
// Base opinions wait up to MoodRecacheTicks after a mood pull, and recompute at once after any other stance write.
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
    public void Display_FreshSetpoint_IsKept()
    {
        var (world, tracker, _, _) = BuildStructural();
        var before = tracker.CachedCertaintyChange;

        // Certainty moves without an invalidation, so only a recache picks it up.
        tracker.SetExtendedCertainty(tracker.ExtendedCertainty + 0.2f);
        Find.TickManager.TicksGame += IdeoTrackerData.DisplayRecacheTicks - 1;
        tracker.RecacheSetpointIfStale(world.Comp);

        Assert.Equal(before, tracker.CachedCertaintyChange);
    }

    [Fact]
    public void Display_OldSetpoint_IsRecached()
    {
        var (world, tracker, _, _) = BuildStructural();
        var before = tracker.CachedCertaintyChange;

        tracker.SetExtendedCertainty(tracker.ExtendedCertainty + 0.2f);
        Find.TickManager.TicksGame += IdeoTrackerData.DisplayRecacheTicks;
        tracker.RecacheSetpointIfStale(world.Comp);

        Assert.NotEqual(before, tracker.CachedCertaintyChange);
    }

    // A paused game does not advance ticks, so an invalidating write must make the setpoint stale by itself.
    [Fact]
    public void Display_StanceWrite_RecachesSetpointWithoutTicks()
    {
        var (world, tracker, _, issue) = BuildStructural();
        var before = tracker.CachedStructural;

        tracker.SetIssueStance(issue, 0f, tracker.Stances.GetStrength(issue));
        tracker.RecacheSetpointIfStale(world.Comp);

        Assert.True(tracker.CachedStructural < before);
    }

    [Fact]
    public void Display_BaseOpinions_RecacheOnlyWhenOld()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("Generosity", "Selfish", "Generous");
        var own = new IdeoBuilder().WithName("I").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        var rival = new IdeoBuilder().WithName("J").AddPrecept(rungs[0], issue, displayOrderInIssue: 0).Build();
        world.AddIdeo(own);
        world.AddIdeo(rival);

        var pawn = new PawnBuilder().WithIdeo(own).WithCertainty(0.5f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        _ = tracker.IdeoOpinion(rival);
        tracker.RecacheBaseOpinionsIfStale();
        var before = tracker.Opinions.BaseIdeoOpinions[rival];

        // A write that skips IdeoTrackerData sends no invalidation, so only the age check picks it up.
        var otherRung = tracker.Stances.GetRank(issue) >= 0.5f ? 0f : 1f;
        tracker.Stances.SetStance(issue, otherRung, tracker.Stances.GetStrength(issue));

        tracker.RecacheBaseOpinionsIfStale();
        Assert.Equal(before, tracker.Opinions.BaseIdeoOpinions[rival]);

        Find.TickManager.TicksGame += IdeoTrackerData.DisplayRecacheTicks;
        tracker.RecacheBaseOpinionsIfStale();
        Assert.NotEqual(before, tracker.Opinions.BaseIdeoOpinions[rival]);
    }

    // Own faith and a rival both hold the pawn's rung, so the rival's base opinion moves with the pawn's strength.
    private static (SimWorld world, IdeoTrackerData tracker, IssueDef issue, Ideo rival, Precept ownPrecept) BuildSharedRung()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("Generosity", "Selfish", "Generous");
        var own = new IdeoBuilder().WithName("I").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        var rival = new IdeoBuilder().WithName("J").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        world.AddIdeo(own);
        world.AddIdeo(rival);

        var pawn = new PawnBuilder().WithIdeo(own).WithCertainty(0.5f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIssueStance(issue, 1f, 10f);
        _ = tracker.IdeoOpinion(rival);
        return (world, tracker, issue, rival, own.precepts[0]);
    }

    private static void ApplyGoodMood(SimWorld world, IdeoTrackerData tracker, Precept precept)
    {
        tracker.Pawn.needs.mood.thoughts.memories.Memories.Add(new SimMemory(precept, 8f));
        world.Comp.ApplyMoodletConvictionShifts(tracker);
    }

    [Fact]
    public void MoodPull_BaseOpinionsWaitForMoodRecacheTicks()
    {
        var (world, tracker, _, rival, ownPrecept) = BuildSharedRung();
        var before = tracker.Opinions.BaseIdeoOpinions[rival];

        ApplyGoodMood(world, tracker, ownPrecept);

        Find.TickManager.TicksGame += IdeoTrackerData.MoodRecacheTicks - 1;
        _ = tracker.IdeoOpinion(rival);
        Assert.Equal(before, tracker.Opinions.BaseIdeoOpinions[rival]);

        Find.TickManager.TicksGame += 1;
        _ = tracker.IdeoOpinion(rival);
        Assert.True(tracker.Opinions.BaseIdeoOpinions[rival] > before);
    }

    [Fact]
    public void StanceWrite_WithMoodPullPending_RecachesAtOnce()
    {
        var (world, tracker, issue, rival, ownPrecept) = BuildSharedRung();
        var before = tracker.Opinions.BaseIdeoOpinions[rival];

        ApplyGoodMood(world, tracker, ownPrecept);
        tracker.SetIssueStance(issue, 1f, 12f);
        _ = tracker.IdeoOpinion(rival);

        Assert.True(tracker.Opinions.BaseIdeoOpinions[rival] > before);
    }

    [Fact]
    public void MoodPull_OwnStructuralBandDoesNotWait()
    {
        var (world, tracker, _, _, ownPrecept) = BuildSharedRung();
        tracker.CertaintyChangeRecache(world.Comp);
        var before = tracker.CachedStructural;

        ApplyGoodMood(world, tracker, ownPrecept);
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.True(tracker.CachedStructural > before);
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
