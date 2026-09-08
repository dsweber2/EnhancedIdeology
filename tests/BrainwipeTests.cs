namespace EnhancedIdeology.Tests;

public class BrainwipeTests : SeededTest
{
    [Fact]
    public void Properties_Constructor_SetsCompClass()
    {
        var props = new HediffCompProperties_BrainwipeRecovery();
        Assert.Equal(typeof(HediffComp_BrainwipeRecovery), props.compClass);
    }

    [Fact]
    public void Comp_SusceptibilityMultiplier_DefaultIsTwo_Five()
    {
        var comp = new HediffComp_BrainwipeRecovery { props = new HediffCompProperties_BrainwipeRecovery() };
        Assert.Equal(2.5f, comp.SusceptibilityMultiplier);
    }

    [Fact]
    public void Comp_SusceptibilityMultiplier_ReturnsCustomValue()
    {
        var props = new HediffCompProperties_BrainwipeRecovery { susceptibilityMultiplier = 4f };
        var comp = new HediffComp_BrainwipeRecovery { props = props };
        Assert.Equal(4f, comp.SusceptibilityMultiplier);
    }

    private static (SimWorld world, IdeoTrackerData tracker, SimPawn pawn, IssueDef issue) Setup()
    {
        var world = new SimWorld();
        world.Initialize();
        PreceptPolicy.RegisterCategory("BW_Issue", PreceptCategory.Moral);
        var (issue, rungs) = SimIssues.Ladder("BW_Issue", "Rung0", "Rung1");
        var ideo = new IdeoBuilder().WithName("BW").AddPrecept(rungs[0], issue, displayOrderInIssue: 0).Build();
        world.AddIdeo(ideo);
        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.8f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        return (world, tracker, pawn, issue);
    }

    [Fact]
    public void ApplyBrainwipe_ZeroesExtendedCertainty()
    {
        var (_, tracker, pawn, _) = Setup();

        tracker.ApplyBrainwipe();

        Assert.Equal(0f, tracker.ExtendedCertainty);
        Assert.Equal(0f, pawn.ideo.Certainty);
    }

    [Fact]
    public void ApplyBrainwipe_NoTraitAlignment_FloorsStrengthsAtTraitFloor()
    {
        // A pawn with no traits has ConvictionStrengthOffset = 0, so traitFloor = Max(0, 3.0 + 0/3) = 3.0.
        // All non-trait-aligned stances should be clamped to exactly that floor.
        var (_, tracker, _, issue) = Setup();
        tracker.SetIssueStance(issue, 0f, ConvictionScale.AbsoluteMaxConvictionStrength);

        tracker.ApplyBrainwipe();

        var strength = tracker.IssueStances().First(ss => ss.issue == issue).strength;
        Assert.Equal(3.0f, strength, precision: 4);
    }

    [Fact]
    public void ApplyBrainwipe_RecacheAfterwards_DoesNotThrow()
    {
        // Smoke test: dirty flag set by brainwipe must not cause an error on the next recache.
        var (world, tracker, _, _) = Setup();
        tracker.ApplyBrainwipe();
        tracker.CertaintyChangeRecache(world.Comp);
        // CachedTargetCertainty is >= 0 and finite (structural band is still computed from stance alignment,
        // not from certainty, so it can be non-zero even with certainty at 0).
        Assert.True(tracker.CachedTargetCertainty >= 0f);
    }
}
