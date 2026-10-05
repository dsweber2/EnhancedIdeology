namespace EnhancedIdeology.Tests;

public class RitualReinforcementTests : SeededTest
{
    // A pawn already at their ideo's orthodox rung but with weak conviction gets their conviction pulled toward
    // AbsoluteMaxConvictionStrength. The near-zero rank gap triggers the half-gap snap in ValleyStep, so the
    // strength delta in a single great ritual step is (targetStrength - startStrength) / 2. With
    // targetStrength = AbsoluteMaxConvictionStrength = 50 and a low start, one step can nearly triple normal
    // MaxConvictionStrength and push structural certainty to its clamp. This test pins what the step actually
    // produces so we can reason about tuning.
    [Fact]
    public void RitualStep_OrthodoxPawnLowConviction_IncreasesStrengthByBoundedAmount()
    {
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder()
            .WithIdeo(ideo)
            .WithCertainty(0.1f)
            .WithLabel("Pawn")
            .Build(world);

        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var orthodoxRank = IssueStanceTracker.HeldRank(ideo, issue);
        var startStrength = 5f;
        tracker.SetIssueStance(issue, orthodoxRank, startStrength);

        var stepLength = ConvictionMath.RitualBaseArc * 2f; // great ritual (positivityIndex=2), CertaintyLossFactor=1
        ConvictionMath.ApplyRitualPull(world.Comp, pawn, issue, orthodoxRank, ConvictionScale.AbsoluteMaxConvictionStrength, stepLength);

        var after = tracker.IssueStances().First(s => s.issue == issue);
        Assert.Equal(orthodoxRank, after.rank);
        Assert.True(after.strength > startStrength, "ritual should increase conviction");
        Assert.True(after.strength <= ConvictionScale.MaxConvictionStrength,
            $"a single great ritual step should not push conviction past the normal ceiling ({ConvictionScale.MaxConvictionStrength}). got {after.strength}");
    }

    // Faith on rung 2 of 0..4, the ladder midpoint.
    [Theory]
    [InlineData(1f, 0f)]
    [InlineData(3f, 4f)]
    [InlineData(1.99f, 0f)]
    [InlineData(2.01f, 4f)]
    [InlineData(-1f, -1f)]
    public void AwayFromFaithRank_LeaningPawn_AimsAtLadderEndOnOwnSide(float pawnRank, float expectedRank)
    {
        var (issue, _) = SimIssues.Ladder("AwayIssue", "R0", "R1", "R2", "R3", "R4");

        Assert.Equal(expectedRank, ConvictionMath.AwayFromFaithRank(issue, 2f, pawnRank));
    }

    [Fact]
    public void AwayFromFaithRank_DontCarePawnPastFaithEnd_StaysPut()
    {
        var (issue, _) = SimIssues.Ladder("AwayIssue", "R0", "R1", "R2", "R3", "R4");

        Assert.Equal(-1f, ConvictionMath.AwayFromFaithRank(issue, 0f, -1f));
    }

    [Fact]
    public void AwayFromFaithRank_OrthodoxPawnOnMiddleRung_PicksBothEnds()
    {
        var (issue, _) = SimIssues.Ladder("AwayIssue", "R0", "R1", "R2", "R3", "R4");

        var picks = Enumerable.Range(0, 50).Select(_ => ConvictionMath.AwayFromFaithRank(issue, 2f, 2f)).ToHashSet();

        Assert.Equal([0f, 4f], picks.Order());
    }

    [Theory]
    [InlineData(0f, 4f)]
    [InlineData(4f, 0f)]
    public void AwayFromFaithRank_OrthodoxPawnOnEndRung_AimsAtOtherEnd(float heldRank, float expectedRank)
    {
        var (issue, _) = SimIssues.Ladder("AwayIssue", "R0", "R1", "R2", "R3", "R4");

        Assert.Equal(expectedRank, ConvictionMath.AwayFromFaithRank(issue, heldRank, heldRank));
    }
}
