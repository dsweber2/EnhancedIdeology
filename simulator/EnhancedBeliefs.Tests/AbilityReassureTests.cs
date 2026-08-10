namespace EnhancedBeliefs.Tests;

// R3 moral-guide Reassure ability: a single debate roll targeting the recipient's most-heterodox issues and
// pulling their stances back toward the ideo's orthodox rungs. Self-reassurance always wins.
public class AbilityReassureTests : SeededTest
{
    // An ideo with two Moral issues and one extra, plus a guide and a recipient who holds heterodox stances
    // on the Moral issues. guideSocialImpact/recipientSocialImpact force the debate roll direction.
    private static (SimWorld world, Pawn guide, Pawn recipient, IssueDef heterodoxA, IssueDef heterodoxB, IssueDef orthodox) OrthodoxFaith(
        float guideSocialImpact = 6f, float recipientSocialImpact = 1f, float recipientCertainty = 0.5f)
    {
        var world = new SimWorld();
        world.Initialize();

        var (issueA, rungsA) = SimIssues.Ladder("ReassureA", "Ra0", "Ra1", "Ra2");
        var (issueB, rungsB) = SimIssues.Ladder("ReassureB", "Rb0", "Rb1", "Rb2");
        var (issueC, rungsC) = SimIssues.Ladder("ReassureC", "Rc0", "Rc1");

        var ideo = new IdeoBuilder().WithName("Faith")
            .AddPrecept(rungsA[0], issueA, displayOrderInIssue: 0)
            .AddPrecept(rungsB[0], issueB, displayOrderInIssue: 0)
            .AddPrecept(rungsC[0], issueC, displayOrderInIssue: 0)
            .Build();
        world.AddIdeo(ideo);

        var guide = new PawnBuilder().WithIdeo(ideo).WithCertainty(1f)
            .WithConversionPower(0.5f).WithSocialImpact(guideSocialImpact).WithLabel("Guide").Build(world);
        var recipient = new PawnBuilder().WithIdeo(ideo).WithCertainty(recipientCertainty)
            .WithConversionPower(0.1f).WithSocialImpact(recipientSocialImpact).WithLabel("Flock").Build(world);

        // Force heterodoxy: shift recipient's stances away from orthodoxy on issueA and issueB.
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        recipientTracker.ShiftIssueStance(issueA, 2f, 1f, 0f);
        recipientTracker.ShiftIssueStance(issueB, 2f, 1f, 0f);

        return (world, guide, recipient, issueA, issueB, issueC);
    }

    private static float StanceRank(IdeoTrackerData tracker, IssueDef issue) =>
        tracker.IssueStances().First(stance => stance.issue == issue).rank;

    [Fact]
    public void MostHeterodoxIssues_ReturnsOnlyDivergentIssues_MostDivergentFirst()
    {
        var (world, _, recipient, heterodoxA, heterodoxB, orthodox) = OrthodoxFaith();
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);

        var all = tracker.MostHeterodoxIssues(4);

        Assert.Equal(2, all.Count);
        Assert.Contains(heterodoxA, all);
        Assert.Contains(heterodoxB, all);
        Assert.DoesNotContain(orthodox, all);
    }

    [Fact]
    public void MostHeterodoxIssues_Cap_TrimsMostDivergentFirst()
    {
        var (world, _, recipient, heterodoxA, heterodoxB, _) = OrthodoxFaith();
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);

        // Widen the divergence on B so it sorts last (less divergent from rung 0 target than A).
        // Both sit at rung 2 on a 3-rung ladder so divergence is equal; just verify cap trims to 1.
        var capped = tracker.MostHeterodoxIssues(1);
        Assert.Single(capped);
    }

    [Fact]
    public void MostHeterodoxIssues_OrthodoxyPawn_ReturnsWeakestBelief()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("Ortho", "O0", "O1");
        var ideo = new IdeoBuilder().WithName("Faith").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        var result = tracker.MostHeterodoxIssues(4);
        Assert.Single(result);
        Assert.Equal(issue, result[0]);
    }

    [Fact]
    public void Resolve_Win_PullsTargetedIssuesTowardOrthodoxy_AndNudgesCertainty()
    {
        var (world, guide, recipient, heterodoxA, heterodoxB, orthodox) = OrthodoxFaith(
            guideSocialImpact: 30f, recipientSocialImpact: 0f, recipientCertainty: 0.5f);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var rankABefore = StanceRank(tracker, heterodoxA);
        var rankBBefore = StanceRank(tracker, heterodoxB);
        var rankCBefore = StanceRank(tracker, orthodox);
        var certaintyBefore = recipient.ideo.Certainty;

        var issues = new[] { heterodoxA, heterodoxB };
        var won = AbilityReassure.Resolve(guide, recipient, issues);

        Assert.True(won);
        Assert.True(StanceRank(tracker, heterodoxA) < rankABefore,
            "Targeted issue A should slide toward the orthodox rung (rung 0).");
        Assert.True(StanceRank(tracker, heterodoxB) < rankBBefore,
            "Targeted issue B should slide toward the orthodox rung (rung 0).");
        Assert.Equal(rankCBefore, StanceRank(tracker, orthodox));
        Assert.True(recipient.ideo.Certainty > certaintyBefore, "Certainty should nudge up on a win.");
    }

    [Fact]
    public void Resolve_Loss_IsNoOp()
    {
        var (world, guide, recipient, heterodoxA, heterodoxB, _) = OrthodoxFaith(
            guideSocialImpact: 0f, recipientSocialImpact: 30f);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var rankABefore = StanceRank(tracker, heterodoxA);
        var certaintyBefore = recipient.ideo.Certainty;

        var issues = new[] { heterodoxA, heterodoxB };
        var won = AbilityReassure.Resolve(guide, recipient, issues);

        Assert.False(won);
        Assert.Equal(rankABefore, StanceRank(tracker, heterodoxA));
        Assert.Equal(certaintyBefore, recipient.ideo.Certainty);
    }

    [Fact]
    public void Resolve_SelfReassurance_AlwaysWins()
    {
        var (world, guide, _, heterodoxA, heterodoxB, _) = OrthodoxFaith();
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(guide);
        guide.ideo.Certainty = 0.8f;
        // Give the guide some heterodoxy to work with.
        tracker.ShiftIssueStance(heterodoxA, 2f, 1f, 0f);
        var rankBefore = StanceRank(tracker, heterodoxA);
        var certaintyBefore = guide.ideo.Certainty;

        var issues = new[] { heterodoxA };
        var won = AbilityReassure.Resolve(guide, guide, issues);

        Assert.True(won);
        Assert.True(StanceRank(tracker, heterodoxA) < rankBefore,
            "Self-reassurance should pull the guide's own heterodox stance toward orthodoxy.");
        Assert.True(guide.ideo.Certainty > certaintyBefore);
    }

    [Fact]
    public void Resolve_EmptyBundle_IsNoOp()
    {
        var (_, guide, recipient, _, _, _) = OrthodoxFaith();
        Assert.False(AbilityReassure.Resolve(guide, recipient, []));
    }
}
