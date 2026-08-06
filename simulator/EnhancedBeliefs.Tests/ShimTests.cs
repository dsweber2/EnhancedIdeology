namespace EnhancedBeliefs.Tests;

public class ShimTests : SeededTest
{
    [Fact]
    public void SimpleCurve_Evaluate_BelowRange_ClampsToFirstPoint()
    {
        var curve = new SimpleCurve { new CurvePoint(5f, 10f), new CurvePoint(10f, 20f) };
        Assert.Equal(10f, curve.Evaluate(0f));
    }

    [Fact]
    public void SimpleCurve_Evaluate_AboveRange_ClampsToLastPoint()
    {
        var curve = new SimpleCurve { new CurvePoint(5f, 10f), new CurvePoint(10f, 20f) };
        Assert.Equal(20f, curve.Evaluate(100f));
    }

    [Fact]
    public void SimpleCurve_Evaluate_Midpoint_LinearlyInterpolates()
    {
        var curve = new SimpleCurve { new CurvePoint(0f, 0f), new CurvePoint(10f, 20f) };
        Assert.Equal(10f, curve.Evaluate(5f), precision: 4);
    }

    [Fact]
    public void Mathf_Clamp01_ClampsLow()
    {
        Assert.Equal(0f, Mathf.Clamp01(-5f));
    }

    [Fact]
    public void Mathf_Clamp01_ClampsHigh()
    {
        Assert.Equal(1f, Mathf.Clamp01(5f));
    }

    [Fact]
    public void Mathf_Clamp01_Passthrough_InRange()
    {
        Assert.Equal(0.5f, Mathf.Clamp01(0.5f));
    }

    [Fact]
    public void ThoughtHandler_GetAllMoodThoughts_CopiesSimulatedThoughts()
    {
        var handler = new ThoughtHandler();
        handler.SimulatedThoughts.Add(new SimThought { MoodOffsetValue = 5f });
        handler.SimulatedThoughts.Add(new SimThought { MoodOffsetValue = -3f });

        var output = new List<Thought>();
        handler.GetAllMoodThoughts(output);

        Assert.Equal(2, output.Count);
        Assert.Equal(5f, output[0].MoodOffset());
        Assert.Equal(-3f, output[1].MoodOffset());
    }

    [Fact]
    public void Pawn_NewPawn_HasDefaultIdeoSetup()
    {
        var world = new SimWorld();
        world.Initialize();
        var ideo = new IdeoBuilder().WithName("TestIdeo").Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithLabel("P").Build(world);

        Assert.Equal(ideo, pawn.Ideo);
        Assert.Equal(0.75f, pawn.ideo.Certainty, precision: 4);
    }
}
