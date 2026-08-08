namespace EnhancedBeliefs;

internal sealed class ReadingOutcomeDoer_CertaintyChange : BookOutcomeDoer
{
    public new BookOutcomeProperties_CertaintyChange Props => (BookOutcomeProperties_CertaintyChange)props;

    public Ideo? ideo;

    // In percents, so divided by 100 when actually applied
    internal static readonly SimpleCurve certaintyGainFromQuality =
    [
        new CurvePoint(0f, 0.0003f),
        new CurvePoint(1f, 0.0006f),
        new CurvePoint(2f, 0.0009f),
        new CurvePoint(3f, 0.0013f),
        new CurvePoint(4f, 0.0017f),
        new CurvePoint(5f, 0.0022f),
        new CurvePoint(6f, 0.0027f)
    ];

    public override bool DoesProvidesOutcome(Pawn reader)
    {
        return ModsConfig.IdeologyActive
            && !Find.IdeoManager.classicMode
            && reader.Ideo != null
            && !reader.DevelopmentalStage.Baby();
    }

    public override void OnBookGenerated(Pawn? author = null)
    {
        base.OnBookGenerated(author);

        if (author != null && author.Ideo != null)
        {
            ideo = author.Ideo;
            return;
        }

        ideo = Find.IdeoManager.IdeosListForReading.RandomElement();
    }

    public override void Reset()
    {
        base.Reset();
        ideo = null;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        if (Scribe.mode == LoadSaveMode.Saving)
        {
            if (!Find.IdeoManager.IdeosListForReading.Contains(ideo))
            {
                ideo = null;
            }
        }

        Scribe_References.Look(ref ideo, "ideo");
    }

    public override IEnumerable<Dialog_InfoCard.Hyperlink> GetHyperlinks()
    {
        if (!Find.IdeoManager.IdeosListForReading.Contains(ideo) || ideo == null)
        {
            yield break;
        }

        yield return new Dialog_InfoCard.Hyperlink(ideo);
    }

    public override string GetBenefitsString(Pawn? reader = null)
    {
        return "EnhancedBeliefs.BookReadingBenefit".Translate(ideo.Named("IDEO"), (CertaintyGain(reader) * GenTicks.TicksPerRealSecond).ToStringPercent());
    }

    public float CertaintyGain(Pawn? reader = null)
    {
        var certaintyGain = certaintyGainFromQuality.Evaluate((int)Quality) / 100f;

        if (reader != null)
        {
            certaintyGain = reader.Ideo == ideo
                ? certaintyGain / reader.GetStatValue(StatDefOf.CertaintyLossFactor)
                : certaintyGain * reader.GetStatValue(StatDefOf.CertaintyLossFactor) * 0.5f;
        }

        return certaintyGain;
    }

    // Per unit of certainty-fraction gain, how far a rival faith's book tugs the reader's stance toward each
    // of its positions. Deliberately slow: belief migrates over a whole book (many ticks), not in one page,
    // and certainty follows structurally via the setpoint rather than being poked here.
    private const float SubversionPull = 1f;

    public override void OnReadingTick(Pawn reader, float factor)
    {
        base.OnReadingTick(reader, factor);

        if (reader.Ideo == null)
        {
            return;
        }

        if (!Find.IdeoManager.IdeosListForReading.Contains(ideo) || ideo == null)
        {
            return;
        }

        var gain = CertaintyGain(reader) * factor;
        var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
        var tracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(reader);

        if (reader.Ideo == ideo)
        {
            // Reading your own faith's book hardens conviction on the issues it takes a stance on. Certainty
            // 1.0 corresponds to MaxConvictionStrength conviction, so convert the gain on that footing.
            var delta = gain * IdeoTrackerData.MaxConvictionStrength;
            foreach (var precept in MoralPrecepts(ideo))
            {
                tracker.ShiftIssueStance(precept.issue!, 0f, 0f, delta);
            }

            return;
        }

        // Reading a rival faith's book tugs your stances toward its positions, warming you to it and, as your
        // fit with your own ideo erodes, letting the certainty setpoint pull your conviction down over time.
        var pull = gain * SubversionPull;
        foreach (var precept in MoralPrecepts(ideo))
        {
            tracker.ShiftIssueStance(precept.issue!, PreceptLadder.RankOf(precept), pull, 0f);
        }
    }

    private static IEnumerable<PreceptDef> MoralPrecepts(Ideo ideo) =>
        ideo.precepts
            .Select(precept => precept.def)
            .Where(def => def.issue != null && PreceptPolicy.CategoryOf(def.issue) == PreceptCategory.Moral);
}

internal sealed class BookOutcomeProperties_CertaintyChange : BookOutcomeProperties
{
    public override Type DoerClass => typeof(ReadingOutcomeDoer_CertaintyChange);
}
