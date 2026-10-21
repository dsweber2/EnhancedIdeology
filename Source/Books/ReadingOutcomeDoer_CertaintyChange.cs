namespace EnhancedIdeology;

internal sealed class ReadingOutcomeDoer_CertaintyChange : BookOutcomeDoer
{
    public new BookOutcomeProperties_CertaintyChange Props => (BookOutcomeProperties_CertaintyChange)props!;

    public Ideo? ideo;

    // Per-issue conviction the book argues its ideo's stances with, so a book is not a uniformly certain tract.
    // Derived from the author's own convictions (censored to the ideo's approved positions) when there is one,
    // otherwise rolled like a pawn's. Read at generation and lazily backfilled for books that predate it.
    private Dictionary<IssueDef, float> issueStrength = [];

    private float _materialBonus = 1f;
    public float MaterialBonus => _materialBonus;
    public void SetMaterialBonus(float bonus) { _materialBonus = bonus; _leatherCached = false; }

    private bool _leatherCached;
    private bool _hasLeather;
    private bool BookHasLeather()
    {
        if (!_leatherCached)
        {
            _hasLeather = VegetarianUtils.HasLeatherIngredient(Book);
            _leatherCached = true;
        }
        return _hasLeather;
    }

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

    // Fix the conviction the book argues each of its ideo's Moral positions with.
    //
    // Authored books inherit the author's non-uniform conviction distribution (already concentrated on whatever
    // they care most about), censored to the ideo's approved rungs where they've drifted. Books without a known
    // author explicitly concentrate conviction on 3-5 focal issues so the book has a clear argumentative identity
    // rather than being a uniformly mild tract on everything.
    private void SeedIssueStrengths(Pawn? author)
    {
        var allPrecepts = MoralPrecepts(ideo!).ToList();

        if (author != null && author.Ideo == ideo)
        {
            var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
            var authorTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(author);
            var authorStances = authorTracker.IssueStances()
                .ToDictionary(stance => stance.issue, stance => (stance.rank, stance.strength));

            foreach (var precept in allPrecepts)
            {
                var issue = precept.issue!;
                issueStrength[issue] = authorStances.TryGetValue(issue, out var stance)
                    ? (Mathf.RoundToInt(stance.rank) == Mathf.RoundToInt(PreceptLadder.RankOf(precept))
                        ? stance.strength
                        : CensoredConviction)
                    : Rand.Range(ConvictionScale.BaseConvictionMin, ConvictionScale.BaseConvictionMax);
            }
            return;
        }

        var focalCount = Math.Min(Rand.RangeInclusive(3, 5), allPrecepts.Count);
        var focalIssues = allPrecepts
            .InRandomOrder()
            .Take(focalCount)
            .Select(p => p.issue!)
            .ToHashSet();

        foreach (var precept in allPrecepts)
        {
            var issue = precept.issue!;
            issueStrength[issue] = focalIssues.Contains(issue)
                ? Rand.Range(ConvictionScale.MaxConvictionStrength * 0.6f, ConvictionScale.MaxConvictionStrength)
                : Rand.Range(ConvictionScale.BaseConvictionMin, ConvictionScale.MaxConvictionStrength * 0.3f);
        }
    }

    public IEnumerable<(IssueDef issue, PreceptDef stance, float strength)> IdeoStances() =>
        ideo == null
            ? []
            : MoralPrecepts(ideo)
                .Select(precept => (precept.issue!, precept, BookStrength(precept.issue!)))
                .OrderByDescending(entry => entry.Item3);

    // The book's conviction on an issue, backfilled with a fresh roll for books that predate this data or issues
    // the ideo picked up after generation.
    private float BookStrength(IssueDef issue)
    {
        if (!issueStrength.TryGetValue(issue, out var strength))
        {
            strength = Rand.Range(ConvictionScale.BaseConvictionMin, ConvictionScale.BaseConvictionMax);
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
            if (ideo != null && !Find.IdeoManager.IdeosListForReading.Contains(ideo))
            {
                ideo = null;
            }
        }

        Scribe_References.Look(ref ideo, "ideo");
        Scribe_Values.Look(ref _materialBonus, "materialBonus", 1f);
        Scribe_Collections.Look(ref issueStrength, "issueStrength", LookMode.Def, LookMode.Value);
    }

    public override IEnumerable<Dialog_InfoCard.Hyperlink> GetHyperlinks()
    {
        if (ideo == null || !Find.IdeoManager.IdeosListForReading.Contains(ideo))
        {
            yield break;
        }

        yield return new Dialog_InfoCard.Hyperlink(ideo);
    }

    // joyDuration = 4000 ticks per reading session, and the game's DaysPerSeason =
    // 15. A pawn plausibly reads ~2 in-game hours/day during recreation. That's
    // TicksPerHour * 4 * DaysPerSeason = 150,000 ticks/quadrum — or equivalently
    // TicksPerSeason / 6.
    internal const float TypicalReadingTicksPerQuadrum = GenDate.TicksPerSeason / 12f;

    public override string GetBenefitsString(Pawn? reader = null)
    {
        return "EnhancedIdeology.BookReadingBenefit".Translate(ideo.Named("IDEO"), (PeakArcPerTick(reader) * TypicalReadingTicksPerQuadrum).ToString("0.##", CultureInfo.InvariantCulture));
    }

    public float CertaintyGain(Pawn? reader = null)
    {
        var certaintyGain = certaintyGainFromQuality.Evaluate((int)Quality) / 100f;
        certaintyGain *= _materialBonus;

        if (reader != null)
        {
            certaintyGain = reader.Ideo == ideo
                ? certaintyGain / reader.GetStatValue(StatDefOf.CertaintyLossFactor)
                : certaintyGain * reader.GetStatValue(StatDefOf.CertaintyLossFactor) * 0.5f;
        }

        return certaintyGain;
    }

    // Gain per unit of vanilla reading factor (reading speed × room bonus × ticks) for this reader.
    public float ReaderGain(Pawn reader) =>
        CertaintyGain(reader) * (ReaderObjectsToLeather(reader) ? 0.5f : 1f);

    private bool ReaderObjectsToLeather(Pawn reader) => VegetarianUtils.IsVegetarian(reader) && BookHasLeather();

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

        if (ideo == null || !Find.IdeoManager.IdeosListForReading.Contains(ideo))
        {
            return;
        }

        if (ReaderObjectsToLeather(reader))
        {
            var meatPrecept = reader.Ideo!.precepts.FirstOrDefault(p => p.def.defName.StartsWith("MeatEating_NonMeat", StringComparison.Ordinal));
            reader.needs?.mood?.thoughts.memories.TryGainMemory(EnhancedIdeologyDefOf.EB_ReadingLeatherboundBook, null, meatPrecept);
        }

        var gain = ReaderGain(reader) * factor;
        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var tracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(reader);

        // A book walks each issue along the conviction valley toward its rung, by an arc of gain x the book's
        // conviction on that issue, as a debate or contemplation does. Your own faith's book pulls a drifted stance
        // back to orthodoxy and then hardens it toward full conviction. A rival faith's book pulls toward its own
        // position and conviction, so a firmly held stance gives up conviction before it moves in rank. This warms
        // you to the rival faith and, as your fit with your own ideo erodes, lets the certainty setpoint pull your
        // conviction down over time.
        var ownFaith = reader.Ideo == ideo;
        var stances = tracker.IssueStances().ToDictionary(stance => stance.issue, stance => (stance.rank, stance.strength));
        var susceptibility = tracker.BrainwipeSusceptibilityMultiplier;
        foreach (var precept in MoralPrecepts(ideo))
        {
            var issue = precept.issue!;
            if (!stances.TryGetValue(issue, out var stance))
                continue;

            var bookRank = PreceptLadder.RankOf(precept);
            var bookStrength = BookStrength(issue);
            var targetStrength = ownFaith ? ConvictionScale.AbsoluteMaxConvictionStrength : bookStrength;
            var (rank, strength) = ConvictionMath.ValleyStep(
                stance.rank, stance.strength, bookRank, ConvictionMath.LadderExtremeAwayFrom(issue, bookRank),
                targetStrength, gain * bookStrength * susceptibility);
            tracker.SetIssueStance(issue, rank, strength);
        }
    }

    // Arc per tick, before reading speed and room bonus, on the issue the book argues most fervently.
    public float PeakArcPerTick(Pawn? reader = null)
    {
        var peakStrength = IdeoStances().FirstOrDefault().strength;
        return (reader == null ? CertaintyGain() : ReaderGain(reader)) * peakStrength;
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
