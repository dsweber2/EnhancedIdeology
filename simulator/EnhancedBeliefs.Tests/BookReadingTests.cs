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
    public void OnReadingTick_OwnIdeo_HardensConviction()
    {
        // Reading your own faith's book raises conviction on the issues it takes a stance on, which lifts
        // structural fit and (via the setpoint) certainty over time - no direct certainty poke.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issue, rungs) = SimIssues.Ladder("BookIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("OwnFaith").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);
        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("P").Build(world);

        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var before = tracker.IssueStances().First(stance => stance.issue == issue).strength;

        var doer = new ReadingOutcomeDoer_CertaintyChange { Quality = QualityCategory.Normal, ideo = ideo };
        for (var ii = 0; ii < 200; ii++)
        {
            doer.OnReadingTick(pawn, 1f);
        }

        var after = tracker.IssueStances().First(stance => stance.issue == issue).strength;
        Assert.True(after > before, $"Expected reading to harden conviction. before={before}, after={after}");
    }

    [Fact]
    public void OnReadingTick_ForeignIdeo_WarmsReaderToBookIdeo()
    {
        // Reading a rival faith's book tugs the reader's stance toward its position, so their preferred rung
        // slides off their own ideo's rung and their opinion of the book's ideo climbs.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issue, rungs) = SimIssues.Ladder("BookIssue", "Permissive", "Forbidding");
        var ownIdeo = new IdeoBuilder().WithName("OwnFaith").AddPrecept(rungs[1]).Build();
        var bookIdeo = new IdeoBuilder().WithName("RivalFaith").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ownIdeo);
        world.AddIdeo(bookIdeo);
        var pawn = new PawnBuilder().WithIdeo(ownIdeo).WithCertainty(0.5f).WithLabel("P").Build(world);

        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var rankBefore = tracker.IssueStances().First(stance => stance.issue == issue).rank;
        var opinionBefore = tracker.IdeoOpinion(bookIdeo);

        var doer = new ReadingOutcomeDoer_CertaintyChange { Quality = QualityCategory.Normal, ideo = bookIdeo };
        for (var ii = 0; ii < 500; ii++)
        {
            doer.OnReadingTick(pawn, 1f);
        }

        var rankAfter = tracker.IssueStances().First(stance => stance.issue == issue).rank;
        Assert.True(rankAfter < rankBefore,
            $"Expected reader's stance to slide toward the book ideo's rung 0. before={rankBefore}, after={rankAfter}");
        Assert.True(tracker.IdeoOpinion(bookIdeo) > opinionBefore,
            "Expected opinion of the book's ideo to rise as the reader's stance nears it.");
    }
}
