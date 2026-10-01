using EnhancedIdeology.HarmonyPatches;

namespace EnhancedIdeology.Tests;

// RimWorld 1.6 calls IdeoTrackerTickInterval every `updateRate` ticks: 1 when the pawn is on screen at
// full zoom, up to 15 when off screen. Certainty dynamics must not depend on where the camera is.
public class TickScheduleTests : SeededTest
{
    private const float StartCertainty = 0.2f;

    private static float CertaintyAfterOneDay(int updateRate)
    {
        var world = new SimWorld();
        world.Initialize();

        var ideo = new IdeoBuilder().WithName("I").AddPrecept(new PreceptDef { defName = "P" }).Build();
        world.AddIdeo(ideo);
        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(StartCertainty).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        for (var tick = 0; tick < GenDate.TicksPerDay; tick++)
        {
            Find.TickManager.TicksGame = tick;
            // Same gate as vanilla Thing.DoTick: the interval tick runs when the offset tick is a multiple of the rate.
            if ((tick + pawn.HashOffset()) % updateRate == 0)
                IdeoTracker_TickInterval.Postfix(pawn.ideo, updateRate);
        }

        return tracker.ExtendedCertainty;
    }

    [Fact]
    public void EveryTick_MovesCertainty()
    {
        Assert.True(Math.Abs(CertaintyAfterOneDay(1) - StartCertainty) > 0.001f);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(13)]
    [InlineData(15)]
    public void ReducedUpdateRate_MatchesEveryTick(int updateRate)
    {
        Assert.Equal(CertaintyAfterOneDay(1), CertaintyAfterOneDay(updateRate), precision: 5);
    }
}
