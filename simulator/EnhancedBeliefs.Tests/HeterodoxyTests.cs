namespace EnhancedBeliefs.Tests;

// Covers spawn heterodoxy: at seeding a pawn quietly diverges from their ideo on a few of the Moral issues
// they hold least firmly (IdeoTrackerData.ApplyHeterodoxy). Tests re-enable it since SeededTest defaults it off.
public class HeterodoxyTests : SeededTest
{
    private static (SimWorld world, Ideo ideo, IssueDef issue) SingleIssueFaith()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("HeterodoxIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("Faith").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);
        return (world, ideo, issue);
    }

    private static float StanceRank(IdeoTrackerData tracker, IssueDef issue) =>
        tracker.IssueStances().First(stance => stance.issue == issue).rank;

    [Fact]
    public void HeterodoxyDisabled_EveryPawnHoldsTheOrthodoxRung()
    {
        IdeoTrackerData.HeterodoxyMax = 0;
        var (world, ideo, issue) = SingleIssueFaith();

        for (var ii = 0; ii < 10; ii++)
        {
            var pawn = new PawnBuilder().WithIdeo(ideo).WithLabel($"P{ii}").Build(world);
            var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
            Assert.Equal(0f, StanceRank(tracker, issue));
        }
    }

    [Fact]
    public void HeterodoxyEnabled_SomePawnsDivergeAndTheirSelfFitDrops()
    {
        IdeoTrackerData.HeterodoxyMax = IdeoTrackerData.DefaultHeterodoxyMax;
        var (world, ideo, issue) = SingleIssueFaith();

        var divergent = 0;
        for (var ii = 0; ii < 30; ii++)
        {
            var pawn = new PawnBuilder().WithIdeo(ideo).WithLabel($"P{ii}").Build(world);
            var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
            var (_, rank, strength) = tracker.IssueStances().First(stance => stance.issue == issue);

            if (rank != 0f)
            {
                divergent++;
                // Diverging from your own faith on the issue lowers the structural band of your certainty
                // setpoint below the orthodox baseline (holding your rung scores +strength; a flipped rung
                // scores strictly less). CachedStructural is that band in 0-1 units.
                tracker.CertaintyChangeRecache(world.Comp);
                var orthodoxBaseline = Mathf.Min(strength * 5f, 100f) / 100f;
                Assert.True(tracker.CachedStructural < orthodoxBaseline,
                    $"Expected a divergent pawn's structural band ({tracker.CachedStructural}) below its orthodox baseline ({orthodoxBaseline}). rank={rank}");
            }
        }

        Assert.True(divergent > 0, "Expected at least one of 30 pawns to spawn heterodox.");
    }

    [Fact]
    public void HeterodoxPawn_IsADebatableTopicForAnOrthodoxSameFaithPawn()
    {
        // The payoff: a spawned dissenter and an orthodox believer of the same faith now hold different rungs
        // on the issue, so a same-faith debate has something to move. Build pawns until one of each turns up.
        IdeoTrackerData.HeterodoxyMax = IdeoTrackerData.DefaultHeterodoxyMax;
        var (world, ideo, issue) = SingleIssueFaith();

        IdeoTrackerData? orthodox = null;
        IdeoTrackerData? dissenter = null;
        for (var ii = 0; ii < 40 && (orthodox == null || dissenter == null); ii++)
        {
            var pawn = new PawnBuilder().WithIdeo(ideo).WithLabel($"P{ii}").Build(world);
            var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
            if (StanceRank(tracker, issue) == 0f)
            {
                orthodox ??= tracker;
            }
            else
            {
                dissenter ??= tracker;
            }
        }

        Assert.NotNull(orthodox);
        Assert.NotNull(dissenter);
        Assert.NotEqual(StanceRank(orthodox!, issue), StanceRank(dissenter!, issue));
    }
}
