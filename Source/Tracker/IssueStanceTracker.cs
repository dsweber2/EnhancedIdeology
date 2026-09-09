namespace EnhancedIdeology;

// Per-pawn issue stance storage: preferred rank and conviction strength for every known issue.
// Owns seeding, heterodoxy, decay, and brainwipe. Callers are responsible for invalidating
// downstream caches (e.g. structural opinion) after any write operation.
internal sealed class IssueStanceTracker
{
    private Pawn _pawn;

    internal const int DefaultHeterodoxyMax = 3;
    internal static int HeterodoxyMax = DefaultHeterodoxyMax;

    private Dictionary<IssueDef, float> _preferredRank = [];
    private Dictionary<IssueDef, float> _strength = [];

    // Scribe_Collections working lists.
    private List<IssueDef>? _preferredRankKeys;
    private List<IssueDef>? _strengthKeys;
    private List<float>? _preferredRankValues;
    private List<float>? _strengthValues;

    private int _lastDecayDay = -1;

    private static Dictionary<MemeDef, HashSet<IssueDef>>? _memeGrantableIssues;
    private static Dictionary<MemeDef, HashSet<IssueDef>> MemeGrantableIssues
    {
        get
        {
            if (_memeGrantableIssues != null) return _memeGrantableIssues;
            _memeGrantableIssues = [];
            foreach (var meme in DefDatabase<MemeDef>.AllDefs)
            {
                var issues = new HashSet<IssueDef>();
                if (!meme.requireOne.NullOrEmpty())
                    foreach (var group in meme.requireOne)
                        foreach (var precept in group)
                            if (precept?.issue != null) issues.Add(precept.issue);
                if (meme.selectOneOrNone?.preceptThingPairs != null)
                    foreach (var pair in meme.selectOneOrNone.preceptThingPairs)
                        if (pair?.precept?.issue != null) issues.Add(pair.precept.issue);
                if (issues.Count > 0) _memeGrantableIssues[meme] = issues;
            }
            return _memeGrantableIssues;
        }
    }

    internal IssueStanceTracker(Pawn pawn) { _pawn = pawn; }
    internal void SetPawn(Pawn pawn) { _pawn = pawn; }

    internal float GetRank(IssueDef issue) => _preferredRank[issue];
    internal float GetStrength(IssueDef issue) => _strength[issue];

    // Seed all issues for this pawn once. Returns true when a fresh seed occurred for a pawn that
    // already has a played-in certainty, signalling the caller to schedule stance calibration.
    internal bool EnsureSeeded(bool certaintyInitialized)
    {
        var freshSeed = _strength.Count == 0;
        var traitOffset = ConvictionStrengthOffset();
        Dictionary<IssueDef, float>? memeOffsets = null;
        foreach (var issue in DefDatabase<IssueDef>.AllDefs)
        {
            if (_strength.ContainsKey(issue)) continue;
            memeOffsets ??= TraitMemeConvictionOffsets();
            _preferredRank[issue] = HeldRank(_pawn.Ideo!, issue);
            _strength[issue] = Mathf.Clamp(
                Rand.Range(ConvictionScale.BaseConvictionMin, ConvictionScale.BaseConvictionMax) + traitOffset + memeOffsets.GetValueOrDefault(issue),
                ConvictionScale.MinConvictionStrength, ConvictionScale.AbsoluteMaxConvictionStrength);
        }

        if (freshSeed)
        {
            if (!certaintyInitialized)
                ApplyHeterodoxy();
            ApplyDietGeneStanceOverride();
        }

        return freshSeed && certaintyInitialized;
    }

    // Rank of the stance `ideo` holds on `issue`: explicit precept, induced coupling, or Don't-care.
    internal static float HeldRank(Ideo ideo, IssueDef issue)
    {
        foreach (var precept in ideo.precepts)
        {
            if (precept.def.issue == issue)
                return PreceptLadder.RankOf(precept.def);
        }
        return PreceptPolicy.InducedRank(ideo, issue) ?? PreceptLadder.DontCareRank(issue);
    }

    // Per-pawn shift to conviction strength from personality traits (design.md R2, 2b).
    internal static float ConvictionOffsetFromTraits(IEnumerable<Trait> traits)
    {
        var offset = 0f;
        foreach (var trait in traits)
        {
            switch (trait.def.defName)
            {
                case "Nerves": // iron-willed (+2) / steadfast (+1) strengthen; nervous (-1) / volatile (-2) weaken
                    offset += trait.Degree * ConvictionScale.ConvictionPerTraitDegree;
                    break;
                case "NaturalMood": // only the down side: pessimist (-1) / depressive (-2) weaken
                    if (trait.Degree < 0)
                        offset += trait.Degree * ConvictionScale.ConvictionPerTraitDegree;
                    break;
                case "Neurotic": // neurotic (+1) / very neurotic (+2) weaken, so subtract
                    offset -= trait.Degree * ConvictionScale.ConvictionPerTraitDegree;
                    break;
            }
        }
        return offset;
    }

    internal float ConvictionStrengthOffset() => ConvictionOffsetFromTraits(_pawn.story.traits.allTraits);

    // Nudge stance toward `targetRank` by `pull` fraction; scale by brainwipe susceptibility multiplier.
    internal void ShiftStance(IssueDef issue, float targetRank, float pull, float strengthDelta, float brainwipeMultiplier)
    {
        var current = _preferredRank[issue];
        _preferredRank[issue] = current + ((targetRank - current) * pull * brainwipeMultiplier);
        _strength[issue] = Mathf.Clamp(
            _strength[issue] + strengthDelta * brainwipeMultiplier,
            ConvictionScale.MinConvictionStrength, ConvictionScale.AbsoluteMaxConvictionStrength);
    }

    // Set stance to an absolute (rank, strength).
    internal void SetStance(IssueDef issue, float rank, float strength)
    {
        _preferredRank[issue] = rank;
        _strength[issue] = Mathf.Clamp(strength, ConvictionScale.MinConvictionStrength, ConvictionScale.AbsoluteMaxConvictionStrength);
    }

    // Reset stances after a brainwipe. Does not zero certainty; caller handles that.
    internal void ApplyBrainwipe()
    {
        var traitFloor = Mathf.Max(0f, 3.0f + ConvictionStrengthOffset() / 3.0f);
        var memeOffsets = TraitMemeConvictionOffsets();
        var memeOffsetsFromMemes = TraitMemeConvictionOffsetsFromMemes();

        foreach (var issue in _strength.Keys.ToList())
        {
            var traitAligned = memeOffsets.ContainsKey(issue) || memeOffsetsFromMemes.ContainsKey(issue);
            _strength[issue] = traitAligned ? Mathf.Max(traitFloor, 5f) : traitFloor;

            var keepStance = traitFloor > 1f || traitAligned;
            if (!keepStance)
            {
                var rungCount = PreceptLadder.Rungs(issue).Count;
                if (rungCount > 1)
                    _preferredRank[issue] = Rand.Range(0, rungCount);
            }
        }
    }

    // Apply conviction decay for the current game day. Returns true when decay was applied (i.e. caller
    // should mark structural opinion caches dirty).
    internal bool ApplyDecayIfNewDay()
    {
        var currentDay = GenTicks.TicksAbs / GenDate.TicksPerDay;
        if (currentDay == _lastDecayDay) return false;
        _lastDecayDay = currentDay;

        var lossFactor = _pawn.GetStatValue(StatDefOf.CertaintyLossFactor);
        var drainPerDay = EnhancedIdeologyMod.Settings.ConvictionDecayRate
            / (GenDate.TicksPerSeason / (float)GenDate.TicksPerDay) / lossFactor;
        if (drainPerDay <= 0f) return false;

        foreach (var issue in _strength.Keys.ToList())
            _strength[issue] = Mathf.Max(ConvictionScale.MinConvictionStrength, _strength[issue] - drainPerDay);

        return true;
    }

    // Scale all strengths by `scale`, clamped to conviction bounds. Used by certainty calibration.
    internal void ScaleStrengths(float scale)
    {
        foreach (var issue in _strength.Keys.ToList())
            _strength[issue] = Mathf.Clamp(
                _strength[issue] * scale,
                ConvictionScale.MinConvictionStrength, ConvictionScale.AbsoluteMaxConvictionStrength);
    }

    internal IEnumerable<(IssueDef issue, float rank, float strength)> IssueStances()
    {
        foreach (var (issue, strength) in _strength)
            yield return (issue, _preferredRank[issue], strength);
    }

    internal static bool PawnHasActiveGene(Pawn pawn, GeneDef? gene) =>
        gene != null && (pawn.genes?.HasActiveGene(gene) ?? false);

    // Called flat from IdeoTrackerData.ExposeData (no Scribe_Deep wrapper) to preserve XML structure.
    internal void ExposeData()
    {
        Scribe_Values.Look(ref _lastDecayDay, "lastDecayDay", defaultValue: -1);
        Scribe_Collections.Look(ref _preferredRank, "issuePreferredRank", LookMode.Def, LookMode.Value, ref _preferredRankKeys, ref _preferredRankValues);
        Scribe_Collections.Look(ref _strength, "issueStrength", LookMode.Def, LookMode.Value, ref _strengthKeys, ref _strengthValues);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            _preferredRank ??= [];
            _strength ??= [];
        }
    }

    private void ApplyHeterodoxy()
    {
        var flipCount = Rand.RangeInclusive(0, HeterodoxyMax);
        if (flipCount == 0) return;

        var candidates = _strength.Keys
            .Where(issue => PreceptPolicy.CategoryOf(issue) == PreceptCategory.Moral
                && _pawn.Ideo!.precepts.Any(precept => precept.def.issue == issue)
                && PreceptLadder.Rungs(issue).Count > 1)
            .OrderBy(issue => _strength[issue])
            .Take(flipCount)
            .ToList();

        foreach (var issue in candidates)
            _preferredRank[issue] = FlippedRank(issue, _preferredRank[issue]);
    }

    private static float FlippedRank(IssueDef issue, float orthodoxRank)
    {
        var rungCount = PreceptLadder.Rungs(issue).Count;
        var current = Mathf.RoundToInt(orthodoxRank);
        return Enumerable.Range(0, rungCount)
            .Where(rank => rank != current)
            .RandomElementByWeight(rank => 1f / (1f + Mathf.Abs(rank - current)));
    }

    private void ApplyDietGeneStanceOverride()
    {
        var meatEatingIssue = DefDatabase<IssueDef>.GetNamedSilentFail("MeatEating");
        if (meatEatingIssue == null) return;

        if (PawnHasActiveGene(_pawn, EnhancedIdeologyDefOf.BS_Diet_Herbivore))
        {
            var rank = PreceptLadder.RankOfName(meatEatingIssue, "MeatEating_Abhorrent");
            if (rank >= 0)
                SetStance(meatEatingIssue, rank, Rand.Range(8f, 30f));
        }
        else if (PawnHasActiveGene(_pawn, EnhancedIdeologyDefOf.BS_Diet_Carnivore))
        {
            var rank = PreceptLadder.RankOfName(meatEatingIssue, "MeatEating_NonMeat_Abhorrent");
            if (rank >= 0)
                SetStance(meatEatingIssue, rank, Rand.Range(8f, 30f));
        }
    }

    private Dictionary<IssueDef, float> TraitMemeConvictionOffsets()
    {
        var offsets = new Dictionary<IssueDef, float>();
        foreach (var precept in _pawn.Ideo!.precepts)
        {
            var issue = precept.def.issue;
            if (issue == null || precept.def.requiredMemes.NullOrEmpty()) continue;

            foreach (var meme in precept.def.requiredMemes)
            {
                float delta = 0f;
                if (!meme.agreeableTraits.NullOrEmpty())
                    foreach (var trait in meme.agreeableTraits)
                        if (trait.HasTrait(_pawn)) delta += ConvictionScale.TraitMemeConvictionBonus;
                if (!meme.disagreeableTraits.NullOrEmpty())
                    foreach (var trait in meme.disagreeableTraits)
                        if (trait.HasTrait(_pawn)) delta -= ConvictionScale.TraitMemeConvictionBonus;

                if (delta != 0f)
                    offsets[issue] = offsets.GetValueOrDefault(issue) + delta;
            }
        }

        if (offsets.Count > 0)
        {
            var parts = offsets.Select(kv => $"{kv.Key.defName}={kv.Value:+0.#;-0.#}");
            EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers,
                $"TraitMemeConvictionOffsets: {_pawn.LabelShort} [{string.Join(", ", parts)}]");
        }

        return offsets;
    }

    private float MemeTraitDelta(MemeDef meme)
    {
        var delta = 0f;
        if (!meme.agreeableTraits.NullOrEmpty())
            foreach (var trait in meme.agreeableTraits)
                if (trait.HasTrait(_pawn)) delta += ConvictionScale.TraitMemeConvictionBonus;
        if (!meme.disagreeableTraits.NullOrEmpty())
            foreach (var trait in meme.disagreeableTraits)
                if (trait.HasTrait(_pawn)) delta -= ConvictionScale.TraitMemeConvictionBonus;
        return delta;
    }

    private Dictionary<IssueDef, float> TraitMemeConvictionOffsetsFromMemes()
    {
        var offsets = new Dictionary<IssueDef, float>();
        foreach (var meme in _pawn.Ideo!.memes)
        {
            if (!MemeGrantableIssues.TryGetValue(meme, out var grantable)) continue;
            var delta = MemeTraitDelta(meme);
            if (delta == 0f) continue;
            foreach (var issue in grantable)
            {
                if (HeldRank(_pawn.Ideo!, issue) != PreceptLadder.DontCareRank(issue))
                    offsets[issue] = offsets.GetValueOrDefault(issue) + delta;
            }
        }
        return offsets;
    }
}
