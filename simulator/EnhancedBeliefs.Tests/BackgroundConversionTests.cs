namespace EnhancedBeliefs.Tests;

// Covers the spontaneous (background) conversion path: the relative-preference probability, the
// time-integrated hazard, and the pace setting. See design.md R1.
public class BackgroundConversionTests : SeededTest
{
    private static void WithPace(float pace, Action body)
    {
        var settings = EnhancedBeliefsMod.Settings;
        var old = settings.ConversionPace;
        try { settings.ConversionPace = pace; body(); }
        finally { settings.ConversionPace = old; }
    }

    private static (SimWorld world, IdeoTrackerData tracker, SimPawn pawn) TwoIdeoWorld(
        float certainty, int alternativeOffset)
    {
        var world = new SimWorld();
        world.Initialize();

        var own = new IdeoBuilder().WithName("Own").Build();
        var altDef = new PreceptDef { defName = "Alt" };
        if (alternativeOffset != 0)
            altDef.AddComp(new PreceptComp_OpinionOffset_Mutable { ExternalOffsetValue = alternativeOffset });
        var alt = new IdeoBuilder().WithName("Alt").AddPrecept(altDef).Build();

        world.AddIdeo(own);
        world.AddIdeo(alt);

        var pawn = new PawnBuilder().WithIdeo(own).WithCertainty(certainty).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        return (world, tracker, pawn);
    }

    [Fact]
    public void ConversionProbability_PrefersOwnIdeo_IsZero()
    {
        // certainty 0.8 (own opinion 0.8) vs a plain alternative (0.3): no preference, no chance.
        var (world, tracker, pawn) = TwoIdeoWorld(certainty: 0.8f, alternativeOffset: 0);
        var altIdeo = FindAlt(tracker, pawn);

        Assert.Equal(0f, tracker.ConversionProbability(altIdeo), precision: 6);
    }

    [Fact]
    public void ConversionProbability_PrefersAlternative_EqualsRelativeGap()
    {
        // certainty 0.2 (own opinion 0.2) vs alternative opinion 0.8 → p = 1 - 0.2/0.8 = 0.75.
        var (world, tracker, pawn) = TwoIdeoWorld(certainty: 0.2f, alternativeOffset: 50);
        var altIdeo = FindAlt(tracker, pawn);

        var expected = 1f - 0.2f / tracker.IdeoOpinion(altIdeo);
        Assert.Equal(expected, tracker.ConversionProbability(altIdeo), precision: 5);
        Assert.True(tracker.ConversionProbability(altIdeo) > 0f);
    }

    [Fact]
    public void Hazard_IsFrequencyIndependent()
    {
        // The whole point: sampling the same span more finely gives the same total probability.
        // Survival over one D-day roll must equal survival over N rolls of D/N days.
        const float p = 0.6f;
        const float interval = 3f;
        const float span = 2f;

        var oneShot = IdeoTrackerData.HazardConversionChance(p, span, interval);

        var survivalFine = 1f;
        for (var ii = 0; ii < 8; ii++)
            survivalFine *= 1f - IdeoTrackerData.HazardConversionChance(p, span / 8f, interval);

        Assert.Equal(1f - oneShot, survivalFine, precision: 5);
    }

    [Fact]
    public void Hazard_OverFullInterval_EqualsBaseProbability()
    {
        // Integrating p over exactly one interval reproduces p.
        Assert.Equal(0.6f, IdeoTrackerData.HazardConversionChance(0.6f, 3f, 3f), precision: 5);
    }

    [Fact]
    public void ConversionInterval_ScalesInverselyWithPace()
    {
        WithPace(2f, () => Assert.Equal(1.5f, EnhancedBeliefsMod.Settings.ConversionInterval, precision: 5));
        WithPace(0.5f, () => Assert.Equal(6f, EnhancedBeliefsMod.Settings.ConversionInterval, precision: 5));
    }

    [Fact]
    public void HigherPace_YieldsHigherPerCheckChance()
    {
        // Same live gap, faster pace (shorter interval) → larger per-check conversion probability.
        float fast = 0f;
        float slow = 0f;
        WithPace(4f, () => fast = IdeoTrackerData.HazardConversionChance(0.6f, 0.03f, EnhancedBeliefsMod.Settings.ConversionInterval));
        WithPace(0.5f, () => slow = IdeoTrackerData.HazardConversionChance(0.6f, 0.03f, EnhancedBeliefsMod.Settings.ConversionInterval));

        Assert.True(fast > slow);
    }

    [Fact]
    public void TryBackgroundConversion_PrefersOwn_DoesNotConvert()
    {
        var (world, tracker, pawn) = TwoIdeoWorld(certainty: 0.8f, alternativeOffset: 0);
        var ownIdeo = pawn.Ideo;

        // A long elapsed span still cannot convert a pawn who prefers their own faith.
        tracker.TryBackgroundConversion(deltaDays: 100f);

        Assert.Equal(ownIdeo, pawn.Ideo);
    }

    [Fact]
    public void TryBackgroundConversion_StronglyPreferredAlternative_Converts()
    {
        Rand.SetSeed(1);
        var (world, tracker, pawn) = TwoIdeoWorld(certainty: 0f, alternativeOffset: 90);
        var altIdeo = FindAlt(tracker, pawn);

        // certainty 0 → p = 1 for the 0.9 alternative → converts on any elapsed time.
        tracker.TryBackgroundConversion(deltaDays: 0.03f);

        Assert.Equal(altIdeo, pawn.Ideo);
    }

    [Fact]
    public void TryBackgroundConversion_ZeroElapsed_NeverConverts()
    {
        var (world, tracker, pawn) = TwoIdeoWorld(certainty: 0f, alternativeOffset: 90);
        var ownIdeo = pawn.Ideo;

        tracker.TryBackgroundConversion(deltaDays: 0f);

        Assert.Equal(ownIdeo, pawn.Ideo);
    }

    private static Ideo FindAlt(IdeoTrackerData tracker, SimPawn pawn)
        => Find.IdeoManager.IdeosListForReading.First(i => i != pawn.Ideo);
}
