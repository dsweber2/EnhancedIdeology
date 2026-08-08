namespace EnhancedBeliefs.Tests;

// Covers the crisis-of-faith pseudo-candidate: once a pawn's conviction falls below CrisisThreshold, doubt
// competes in the conversion draw, and winning that draw is the IdeoChange breakdown.
public class CrisisTests : SeededTest
{
    private static void WithCrisisThreshold(float threshold, Action body)
    {
        var settings = EnhancedBeliefsMod.Settings;
        var old = settings.CrisisThreshold;
        try { settings.CrisisThreshold = threshold; body(); }
        finally { settings.CrisisThreshold = old; }
    }

    private static (IdeoTrackerData tracker, SimPawn pawn) SoloIdeo(float certainty)
    {
        var world = new SimWorld();
        world.Initialize();
        var ideo = new IdeoBuilder().WithName("Solo").Build();
        world.AddIdeo(ideo);
        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(certainty).WithLabel("P").Build(world);
        return (world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn), pawn);
    }

    [Fact]
    public void BelowThreshold_NoAlternative_BreaksDown()
    {
        // certainty 0 < 0.25: the crisis is the only candidate and its chance is 1 → guaranteed breakdown.
        var (tracker, pawn) = SoloIdeo(certainty: 0f);
        var ownIdeo = pawn.Ideo;

        var result = tracker.CheckConversion();

        Assert.Equal(ConversionOutcome.Breakdown, result);
        Assert.Equal(ownIdeo, pawn.Ideo); // breakdown doesn't itself switch ideo
    }

    [Fact]
    public void AboveThreshold_NoAlternative_DoesNotBreakDown()
    {
        // certainty 0.5 >= 0.25: no crisis candidate and no real candidate → nothing fires.
        var (tracker, _) = SoloIdeo(certainty: 0.5f);

        Assert.Equal(ConversionOutcome.Failure, tracker.CheckConversion());
    }

    [Fact]
    public void CrisisThreshold_GatesBreakdownReachability()
    {
        // At certainty 0.3, whether a crisis can occur at all depends on where the threshold sits.
        WithCrisisThreshold(0.25f, () =>
        {
            var (tracker, _) = SoloIdeo(certainty: 0.3f);
            Assert.Equal(ConversionOutcome.Failure, tracker.CheckConversion()); // 0.3 >= 0.25 → no crisis
        });

        WithCrisisThreshold(0.5f, () =>
        {
            var breakdowns = 0;
            for (var ii = 0; ii < 200; ii++)
            {
                var (tracker, _) = SoloIdeo(certainty: 0.3f);
                if (tracker.CheckConversion() == ConversionOutcome.Breakdown) breakdowns++;
            }
            Assert.True(breakdowns > 0, $"0.3 < 0.5 threshold → crisis reachable, got {breakdowns}");
        });
    }

    [Fact]
    public void Crisis_CompetesWithConversion_BothOutcomesOccur()
    {
        // certainty 0 with a strongly-preferred alternative: conversion usually wins, but the crisis still
        // claims a share - proving doubt competes rather than gating.
        var converts = 0;
        var breakdowns = 0;

        for (var ii = 0; ii < 300; ii++)
        {
            var world = new SimWorld();
            world.Initialize();

            var own = new IdeoBuilder().WithName("Own").Build();
            var alt = new IdeoBuilder().WithName("Alt").Build();
            world.AddIdeo(own);
            world.AddIdeo(alt);

            var pawn = new PawnBuilder().WithIdeo(own).WithCertainty(0f).WithLabel("P").Build(world);
            var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
            tracker.SetIdeoBaseOpinion(alt, 60);

            var result = tracker.CheckConversion();
            if (result == ConversionOutcome.Success) converts++;
            else if (result == ConversionOutcome.Breakdown) breakdowns++;
        }

        Assert.True(converts > breakdowns, $"strong alternative should usually win: converts={converts} breakdowns={breakdowns}");
        Assert.True(breakdowns > 0, $"crisis should still claim some: breakdowns={breakdowns}");
    }
}
