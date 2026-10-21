using RimWorld;

namespace EnhancedIdeology.Tests;

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
    public void BookOutcomeProperties_DoerClass_IsReadingOutcomeDoer()
    {
        var props = new BookOutcomeProperties_CertaintyChange();
        Assert.Equal(typeof(ReadingOutcomeDoer_CertaintyChange), props.DoerClass);
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
    public void ReaderGain_NonVegetarianReader_MatchesCertaintyGain()
    {
        var (_, pawn, doer) = Setup(QualityCategory.Good);
        pawn.SetStatValue(StatDefOf.CertaintyLossFactor, 1.5f);

        Assert.Equal(doer.CertaintyGain(pawn), doer.ReaderGain(pawn), precision: 9);
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

    [Fact]
    public void OnReadingTick_ForeignIdeo_FirmStanceResistsInRank()
    {
        // A rival book walks the conviction valley like a debate: a firmly held stance spends the arc losing
        // conviction, so it moves less in rank than a shaky stance on the same rung.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issue, rungs) = SimIssues.Ladder("BookIssue", "Permissive", "Forbidding");
        var ownIdeo = new IdeoBuilder().WithName("OwnFaith").AddPrecept(rungs[1]).Build();
        var bookIdeo = new IdeoBuilder().WithName("RivalFaith").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ownIdeo);
        world.AddIdeo(bookIdeo);

        var ownRank = PreceptLadder.RankOf(rungs[1]);
        var shaky = new PawnBuilder().WithIdeo(ownIdeo).WithCertainty(0.5f).WithLabel("S").Build(world);
        var firm = new PawnBuilder().WithIdeo(ownIdeo).WithCertainty(0.5f).WithLabel("F").Build(world);
        var shakyTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(shaky);
        var firmTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(firm);
        shakyTracker.SetIssueStance(issue, ownRank, 2f);
        firmTracker.SetIssueStance(issue, ownRank, 18f);

        var doer = new ReadingOutcomeDoer_CertaintyChange { Quality = QualityCategory.Normal, ideo = bookIdeo };
        for (var ii = 0; ii < 2000; ii++)
        {
            doer.OnReadingTick(shaky, 1f);
            doer.OnReadingTick(firm, 1f);
        }

        var shakyStance = shakyTracker.IssueStances().First(stance => stance.issue == issue);
        var firmStance = firmTracker.IssueStances().First(stance => stance.issue == issue);
        var shakyMove = Mathf.Abs(shakyStance.rank - ownRank);
        var firmMove = Mathf.Abs(firmStance.rank - ownRank);
        Assert.True(firmMove > 0f, "Expected the firm reader to move at least a little.");
        Assert.True(shakyMove > firmMove * 2f, $"Expected the shaky reader to move further in rank. shaky={shakyMove}, firm={firmMove}");
        Assert.True(firmStance.strength < 18f, $"Expected the firm reader to lose conviction. strength={firmStance.strength}");
    }

    [Fact]
    public void OnReadingTick_OwnIdeo_PullsHeterodoxStanceBackToOrthodoxy()
    {
        // A believer who drifted off the faith's rung reads their own scripture: the stance walks back toward the
        // faith's rung instead of hardening where it drifted to.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issue, rungs) = SimIssues.Ladder("BookIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("Faith").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);
        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("P").Build(world);

        var heterodoxRank = PreceptLadder.RankOf(rungs[1]);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIssueStance(issue, heterodoxRank, 4f);

        var doer = new ReadingOutcomeDoer_CertaintyChange { Quality = QualityCategory.Normal, ideo = ideo };
        for (var ii = 0; ii < 2000; ii++)
        {
            doer.OnReadingTick(pawn, 1f);
        }

        var rankAfter = tracker.IssueStances().First(stance => stance.issue == issue).rank;
        var orthodoxRank = PreceptLadder.RankOf(rungs[0]);
        Assert.True(Mathf.Abs(rankAfter - orthodoxRank) < Mathf.Abs(heterodoxRank - orthodoxRank),
            $"Expected the stance to move toward the faith's rung. before={heterodoxRank}, after={rankAfter}");
    }

    [Fact]
    public void PeakArcPerTick_IsGainTimesStrongestBookConviction()
    {
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (_, firstRungs) = SimIssues.Ladder("FirstIssue", "Permissive", "Forbidding");
        var (_, secondRungs) = SimIssues.Ladder("SecondIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("Faith").AddPrecept(firstRungs[0]).AddPrecept(secondRungs[1]).Build();
        world.AddIdeo(ideo);
        var pawn = new PawnBuilder().WithIdeo(ideo).WithLabel("P").Build(world);

        var doer = new ReadingOutcomeDoer_CertaintyChange { Quality = QualityCategory.Normal, ideo = ideo };
        var peak = doer.IdeoStances().Max(entry => entry.strength);

        Assert.Equal(doer.ReaderGain(pawn) * peak, doer.PeakArcPerTick(pawn), precision: 9);
    }

    [Fact]
    public void OnBookGenerated_AuthoredBook_HardensReaderByAuthorsConviction()
    {
        // A book written by an author carries that author's own per-issue conviction, so it hardens a same-faith
        // reader by exactly gain * (the author's strength on that issue), not a flat amount.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issue, rungs) = SimIssues.Ladder("BookIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("Faith").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var author = new PawnBuilder().WithIdeo(ideo).WithLabel("A").Build(world);
        var authorStrength = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(author)
            .IssueStances().First(stance => stance.issue == issue).strength;

        var doer = new ReadingOutcomeDoer_CertaintyChange { Quality = QualityCategory.Normal };
        doer.OnBookGenerated(author);
        Assert.Equal(ideo, doer.ideo);

        var reader = new PawnBuilder().WithIdeo(ideo).WithLabel("R").Build(world);
        var readerTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(reader);
        // Zero the reader's conviction on the issue first, so the tiny hardening is measured from 0 and free of
        // the float cancellation that subtracting two ~15-scale strengths would introduce.
        readerTracker.ShiftIssueStance(issue, 0f, 0f, -ConvictionScale.AbsoluteMaxConvictionStrength);
        var before = readerTracker.IssueStances().First(stance => stance.issue == issue).strength;

        var gain = doer.CertaintyGain(reader);
        doer.OnReadingTick(reader, 1f);
        var after = readerTracker.IssueStances().First(stance => stance.issue == issue).strength;

        var expected = gain * authorStrength;
        var actual = after - before;
        Assert.True(Mathf.Abs(actual - expected) < expected * 0.01f,
            $"Expected hardening ~= gain * author conviction ({expected}), got {actual}.");
    }

    [Fact]
    public void OnBookGenerated_HeterodoxAuthor_WritesPartyLineFaintly()
    {
        // An author whose personal stance has drifted to another rung still pens the ideo's orthodox line, but
        // only faintly: the book argues that issue at conviction 1, not the author's own (high) conviction. A
        // same-faith reader is hardened by that token amount, far less than a devout author's book would.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issue, rungs) = SimIssues.Ladder("BookIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("Faith").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var author = new PawnBuilder().WithIdeo(ideo).WithLabel("A").Build(world);
        var authorTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(author);
        // Drag the author fully onto the opposite rung and harden them there - a fervent dissenter.
        authorTracker.ShiftIssueStance(issue, PreceptLadder.RankOf(rungs[1]), 1f, +50f);

        var doer = new ReadingOutcomeDoer_CertaintyChange { Quality = QualityCategory.Normal };
        doer.OnBookGenerated(author);

        var reader = new PawnBuilder().WithIdeo(ideo).WithLabel("R").Build(world);
        var readerTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(reader);
        // Measure the faint hardening from a zeroed base, free of float cancellation on a ~15-scale strength.
        readerTracker.ShiftIssueStance(issue, 0f, 0f, -ConvictionScale.AbsoluteMaxConvictionStrength);
        var before = readerTracker.IssueStances().First(stance => stance.issue == issue).strength;

        var gain = doer.CertaintyGain(reader);
        const int ticks = 200;
        for (var ii = 0; ii < ticks; ii++)
        {
            doer.OnReadingTick(reader, 1f);
        }
        var after = readerTracker.IssueStances().First(stance => stance.issue == issue).strength;

        // Hardened by ticks * gain * 1 (the censored conviction), not gain * ~50. Accumulated over many ticks
        // so the token per-tick delta clears float cancellation on the ~15 base.
        var expected = ticks * gain * 1f;
        var actual = after - before;
        Assert.True(Mathf.Abs(actual - expected) < expected * 0.01f,
            $"Expected a censored (faint) hardening ~= ticks * gain * 1 ({expected}), got {actual}.");
    }

    [Fact]
    public void OnReadingTick_AuthorlessBook_RollsConvictionAndStillHardens()
    {
        // A found / trader book with no known author was never seeded at generation; the first read backfills a
        // random conviction per issue, so it still hardens a same-faith reader.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issue, rungs) = SimIssues.Ladder("BookIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("Faith").AddPrecept(rungs[0]).Build();
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
    public void AuthoredBook_ConvictionScalesRivalPull()
    {
        // The pull a rival book exerts scales with the author's conviction: a book from an author who holds the
        // issue at zero conviction moves the reader's stance not at all, while a fervent author's book does.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issue, rungs) = SimIssues.Ladder("BookIssue", "Permissive", "Forbidding");
        var bookIdeo = new IdeoBuilder().WithName("Rival").AddPrecept(rungs[0]).Build();
        var ownIdeo = new IdeoBuilder().WithName("Own").AddPrecept(rungs[1]).Build();
        world.AddIdeo(bookIdeo);
        world.AddIdeo(ownIdeo);

        var timid = new PawnBuilder().WithIdeo(bookIdeo).WithLabel("T").Build(world);
        world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(timid).ShiftIssueStance(issue, 0f, 0f, -50f);
        var zealot = new PawnBuilder().WithIdeo(bookIdeo).WithLabel("Z").Build(world);
        world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(zealot).ShiftIssueStance(issue, 0f, 0f, +50f);

        var timidBook = new ReadingOutcomeDoer_CertaintyChange { Quality = QualityCategory.Normal };
        timidBook.OnBookGenerated(timid);
        var zealotBook = new ReadingOutcomeDoer_CertaintyChange { Quality = QualityCategory.Normal };
        zealotBook.OnBookGenerated(zealot);

        var reader = new PawnBuilder().WithIdeo(ownIdeo).WithCertainty(0.5f).WithLabel("R").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(reader);
        var start = tracker.IssueStances().First(stance => stance.issue == issue).rank;

        for (var ii = 0; ii < 200; ii++)
        {
            timidBook.OnReadingTick(reader, 1f);
        }
        var afterTimid = tracker.IssueStances().First(stance => stance.issue == issue).rank;
        Assert.Equal(start, afterTimid);

        for (var ii = 0; ii < 200; ii++)
        {
            zealotBook.OnReadingTick(reader, 1f);
        }
        var afterZealot = tracker.IssueStances().First(stance => stance.issue == issue).rank;
        Assert.True(afterZealot < afterTimid,
            $"Expected the fervent author's book to drag the reader's stance. timid={afterTimid}, zealot={afterZealot}");
    }
}
