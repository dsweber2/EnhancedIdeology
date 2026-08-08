namespace EnhancedBeliefs;

internal sealed class ReadingOutcomeDoer_CertaintyChange : BookOutcomeDoer
{
    public new BookOutcomeProperties_CertaintyChange Props => (BookOutcomeProperties_CertaintyChange)props;

    public Ideo? ideo;

    // Per-issue conviction the book argues its ideo's stances with, so a book is not a uniformly certain tract.
    // Derived from the author's own convictions (censored to the ideo's approved positions) when there is one,
    // otherwise rolled like a pawn's. Read at generation and lazily backfilled for books that predate it.
    private Dictionary<IssueDef, float> issueStrength = [];

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

        ideo = author?.Ideo ?? Find.IdeoManager.IdeosListForReading.RandomElement();
        SeedIssueStrengths(author);
    }

    // Fix the conviction the book argues each of its ideo's Moral positions with. An authored book inherits the
    // author's own per-issue conviction (which already carries their personality's zeal), censored to the ideo's
    // approved rungs; a found or trader book with no known author rolls a plain conviction per issue.
    private void SeedIssueStrengths(Pawn? author)
    {
        Dictionary<IssueDef, (float rank, float strength)>? authorStances = null;
        if (author != null && author.Ideo == ideo)
        {
            var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
            var authorTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(author);
            authorStances = authorTracker.IssueStances()
                .ToDictionary(stance => stance.issue, stance => (stance.rank, stance.strength));
        }

        foreach (var precept in MoralPrecepts(ideo!))
        {
            var issue = precept.issue!;
            issueStrength[issue] = AuthoredStrength(authorStances, issue, PreceptLadder.RankOf(precept));
        }
    }

    // What conviction the author writes a given orthodox position with. If the author still personally holds
    // that rung, it is their own conviction; if their stance has drifted to a different rung (they now disagree
    // with the party line they are penning), they argue it only faintly, at CensoredConviction. With no known
    // author, a plain roll stands in.
    private static float AuthoredStrength(
        Dictionary<IssueDef, (float rank, float strength)>? authorStances, IssueDef issue, float orthodoxRank)
    {
        if (authorStances != null && authorStances.TryGetValue(issue, out var stance))
        {
            return Mathf.RoundToInt(stance.rank) == Mathf.RoundToInt(orthodoxRank) ? stance.strength : CensoredConviction;
        }

        return Rand.Range(IdeoTrackerData.BaseConvictionMin, IdeoTrackerData.BaseConvictionMax);
    }

    // The book's conviction on an issue, backfilled with a fresh roll for books that predate this data or issues
    // the ideo picked up after generation.
    private float BookStrength(IssueDef issue)
    {
        if (!issueStrength.TryGetValue(issue, out var strength))
        {
            strength = Rand.Range(IdeoTrackerData.BaseConvictionMin, IdeoTrackerData.BaseConvictionMax);
            issueStrength[issue] = strength;
        }

        return strength;
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
        Scribe_Collections.Look(ref issueStrength, "issueStrength", LookMode.Def, LookMode.Value);
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

    // Conviction a heterodox author argues the orthodox line with when their own stance has drifted off it -
    // they toe the party line in the book, but faintly, since they no longer believe it.
    private const float CensoredConviction = 1f;

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
            // Reading your own faith's book hardens conviction on the issues it takes a stance on, by more where
            // the book argues that issue with more conviction. A book arguing an issue at MaxConvictionStrength
            // reproduces the old flat "certainty 1.0 == MaxConvictionStrength conviction" footing.
            foreach (var precept in MoralPrecepts(ideo))
            {
                tracker.ShiftIssueStance(precept.issue!, 0f, 0f, gain * BookStrength(precept.issue!));
            }

            return;
        }

        // Reading a rival faith's book tugs your stances toward its positions, warming you to it and, as your
        // fit with your own ideo erodes, letting the certainty setpoint pull your conviction down over time. A
        // more fervently argued position tugs harder, normalized so MaxConvictionStrength matches the old pull.
        foreach (var precept in MoralPrecepts(ideo))
        {
            var pull = gain * SubversionPull * (BookStrength(precept.issue!) / IdeoTrackerData.MaxConvictionStrength);
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
