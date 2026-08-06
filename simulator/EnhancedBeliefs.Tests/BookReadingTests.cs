using RimWorld;

namespace EnhancedBeliefs.Tests;

public class BookReadingTests : SeededTest
{
    private static (SimWorld world, SimPawn pawn, ReadingOutcomeDoer_CertaintyChange doer) Setup(
        QualityCategory quality = QualityCategory.Normal)
    {
        var world = new SimWorld();
        world.Initialize();

        var ideo = new IdeoBuilder().WithName("BookIdeo").Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("P").Build(world);

        var doer = new ReadingOutcomeDoer_CertaintyChange
        {
            Quality = quality,
            ideo = ideo,
        };

        return (world, pawn, doer);
    }

    [Fact]
    public void CertaintyGain_NullReader_UsesQualityCurve()
    {
        // Normal quality → certaintyGainFromQuality.Evaluate(2) = 0.0009 → /100 = 0.000009
        var (_, _, doer) = Setup(QualityCategory.Normal);

        var gain = doer.CertaintyGain(null);

        Assert.Equal(0.000009f, gain, precision: 9);
    }

    [Fact]
    public void CertaintyGain_OwnIdeoReader_DividedByCertaintyLossFactor()
    {
        // Reader follows same ideo as book → gain ÷ CertaintyLossFactor
        var (world, pawn, doer) = Setup(QualityCategory.Normal);
        pawn.SetStatValue(StatDefOf.CertaintyLossFactor, 2f);

        var gainNoReader = doer.CertaintyGain(null);
        var gainWithReader = doer.CertaintyGain(pawn);

        Assert.Equal(gainNoReader / 2f, gainWithReader, precision: 9);
    }

    [Fact]
    public void CertaintyGain_ForeignIdeoReader_MultipliedByLossFactorAndHalf()
    {
        // Reader follows a different ideo → gain × CertaintyLossFactor × 0.5
        var (world, pawn, doer) = Setup(QualityCategory.Normal);
        pawn.SetStatValue(StatDefOf.CertaintyLossFactor, 2f);

        var foreignIdeo = new IdeoBuilder().WithName("Foreign").Build();
        world.AddIdeo(foreignIdeo);
        var reader = new PawnBuilder().WithIdeo(foreignIdeo).WithLabel("R").Build(world);
        reader.SetStatValue(StatDefOf.CertaintyLossFactor, 2f);

        doer.ideo = foreignIdeo;
        var gainNoReader = doer.CertaintyGain(null);
        doer.ideo = pawn.Ideo;

        var gainForeignReader = doer.CertaintyGain(reader);

        Assert.Equal(gainNoReader * 2f * 0.5f, gainForeignReader, precision: 9);
    }

    [Fact]
    public void OnReadingTick_OwnIdeo_IncreasesCertainty()
    {
        var (_, pawn, doer) = Setup(QualityCategory.Normal);
        var before = pawn.ideo.Certainty;

        doer.OnReadingTick(pawn, 1f);

        Assert.True(pawn.ideo.Certainty > before,
            $"Expected certainty to increase. Before: {before}, After: {pawn.ideo.Certainty}");
    }

    [Fact]
    public void OnReadingTick_ForeignIdeo_DecreasesCertaintyAndIncreasesOpinion()
    {
        var (world, pawn, _) = Setup(QualityCategory.Normal);
        var foreignIdeo = new IdeoBuilder().WithName("Foreign2").Build();
        world.AddIdeo(foreignIdeo);

        var doer = new ReadingOutcomeDoer_CertaintyChange { Quality = QualityCategory.Normal, ideo = foreignIdeo };
        var beforeCertainty = pawn.ideo.Certainty;
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var opinionBefore = tracker.IdeoOpinion(foreignIdeo);

        doer.OnReadingTick(pawn, 1f);

        Assert.True(pawn.ideo.Certainty < beforeCertainty,
            $"Expected certainty to decrease. Before: {beforeCertainty}, After: {pawn.ideo.Certainty}");
        Assert.True(tracker.IdeoOpinion(foreignIdeo) > opinionBefore,
            $"Expected opinion of foreign ideo to increase.");
    }
}
