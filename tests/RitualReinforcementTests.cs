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
        var orthodoxRank = IdeoTrackerData.HeldRank(ideo, issue);
        var startStrength = 5f;
        tracker.SetIssueStance(issue, orthodoxRank, startStrength);

        var stepLength = ConvictionMath.RitualBaseArc * 2f; // great ritual (positivityIndex=2), CertaintyLossFactor=1
        ConvictionMath.ApplyRitualPull(world.Comp, pawn, issue, orthodoxRank, IdeoTrackerData.AbsoluteMaxConvictionStrength, stepLength);

        var after = tracker.IssueStances().First(s => s.issue == issue);
        Assert.Equal(orthodoxRank, after.rank);
        Assert.True(after.strength > startStrength, "ritual should increase conviction");
        Assert.True(after.strength <= IdeoTrackerData.MaxConvictionStrength,
            $"a single great ritual step should not push conviction past the normal ceiling ({IdeoTrackerData.MaxConvictionStrength}). got {after.strength}");
    }
}
