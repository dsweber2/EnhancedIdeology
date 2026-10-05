namespace EnhancedIdeology.Tests;

public class CaravanDebateTests : SeededTest
{
    [Theory]
    [InlineData(0.27f, false)]
    [InlineData(0.28f, true)]
    [InlineData(1f, true)]
    public void IsAlert_FollowsRestLevel(float rest, bool expected)
    {
        var pawn = new Pawn();
        pawn.needs.rest = new Need_Rest { CurLevel = rest };

        Assert.Equal(expected, CaravanDebates.IsAlert(pawn));
    }

    [Fact]
    public void NoRestNeed_IsAlert()
    {
        Assert.True(CaravanDebates.IsAlert(new Pawn()));
    }

    [Fact]
    public void Onlookers_AreTheAlertMembersOfTheDebatersCaravan()
    {
        var (world, ideo) = Setup();
        var caravan = new Caravan();
        var winner = Member(world, ideo, caravan, "Winner");
        var loser = Member(world, ideo, caravan, "Loser");
        var alert = Member(world, ideo, caravan, "Alert");
        var tired = Member(world, ideo, caravan, "Tired", rest: 0.1f);
        _ = Member(world, ideo, new Caravan(), "Elsewhere");
        _ = new PawnBuilder().WithIdeo(ideo).WithLabel("AtHome").Build(world);

        Assert.Equal([alert], CaravanDebates.Onlookers(winner, loser));
        Assert.DoesNotContain(tired, CaravanDebates.Onlookers(winner, loser));
    }

    [Fact]
    public void Onlookers_EmptyWhenDebatersAreInDifferentCaravans()
    {
        var (world, ideo) = Setup();
        var caravan = new Caravan();
        var winner = Member(world, ideo, caravan, "Winner");
        var loser = Member(world, ideo, new Caravan(), "Loser");
        _ = Member(world, ideo, caravan, "Alert");

        Assert.Empty(CaravanDebates.Onlookers(winner, loser));
    }

    [Fact]
    public void Sway_InCaravan_PullsAlertMembersButNotTiredOnes()
    {
        DefDatabase<PreceptDef>.Clear();
        DefDatabase<IssueDef>.Clear();
        var world = new SimWorld();
        world.Initialize();
        var (issue, rungs) = SimIssues.Ladder("CampIssue", "K0", "K1", "K2", "K3");
        var winnerIdeo = new IdeoBuilder().WithName("Loud").AddPrecept(rungs[3]).Build();
        var listenerIdeo = new IdeoBuilder().WithName("Listening").AddPrecept(rungs[2]).Build();
        world.AddIdeo(winnerIdeo);
        world.AddIdeo(listenerIdeo);

        var caravan = new Caravan();
        var winner = Member(world, winnerIdeo, caravan, "Winner");
        var loser = Member(world, listenerIdeo, caravan, "Loser");
        var alert = Member(world, listenerIdeo, caravan, "Alert");
        var tired = Member(world, listenerIdeo, caravan, "Tired", rest: 0.1f);

        var winnerTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(winner);
        winnerTracker.SetIssueStance(issue, 0f, Stance(winnerTracker, issue).strength);
        var alertTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(alert);
        var tiredTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(tired);
        var alertBefore = Stance(alertTracker, issue).rank;
        var tiredBefore = Stance(tiredTracker, issue).rank;

        DebateOnlookers.Sway(world.Comp, winner, loser, [issue]);

        Assert.True(Stance(alertTracker, issue).rank < alertBefore,
            $"Expected the alert member to slide toward rung 0. before={alertBefore}, after={Stance(alertTracker, issue).rank}");
        Assert.Equal(tiredBefore, Stance(tiredTracker, issue).rank);
    }

    private static (SimWorld world, Ideo ideo) Setup()
    {
        DefDatabase<PreceptDef>.Clear();
        DefDatabase<IssueDef>.Clear();
        var world = new SimWorld();
        world.Initialize();
        var (_, rungs) = SimIssues.Ladder("CaravanIssue", "C0", "C1");
        var ideo = new IdeoBuilder().WithName("Plain").AddPrecept(rungs[1]).Build();
        world.AddIdeo(ideo);
        return (world, ideo);
    }

    private static SimPawn Member(SimWorld world, Ideo ideo, Caravan caravan, string label, float rest = 1f)
    {
        var pawn = new PawnBuilder().WithIdeo(ideo).WithLabel(label).Build(world);
        pawn.needs.rest = new Need_Rest { CurLevel = rest };
        caravan.AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny: true);
        return pawn;
    }

    private static (IssueDef issue, float rank, float strength) Stance(IdeoTrackerData tracker, IssueDef issue) =>
        tracker.IssueStances().First(stance => stance.issue == issue);
}
