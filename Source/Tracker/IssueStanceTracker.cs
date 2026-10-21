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

    // IssueDefs do not change after load, so one full seeding pass per session is enough.
    private bool _seeded;

    internal IssueStanceTracker(Pawn pawn) { _pawn = pawn; }
    internal void SetPawn(Pawn pawn) { _pawn = pawn; }

    internal float GetRank(IssueDef issue) => _preferredRank[issue];
    internal float GetStrength(IssueDef issue) => _strength[issue];

    // Issues the pawn holds a stance on, in stance order. Structural opinion reads this once per target ideo,
    // so it is kept until a rank changes. Every rank write goes through SetRank.
    private List<IssueDef>? _heldIssues;

    private void SetRank(IssueDef issue, float rank)
    {
        _preferredRank[issue] = rank;
        _heldIssues = null;
    }

    // True when the stance on `issue` is off the Don't-care rung, i.e. the pawn holds a belief about it.
    internal bool HoldsStance(IssueDef issue) =>
        Mathf.Abs(_preferredRank[issue] - PreceptLadder.DontCareRank(issue)) > InteractionWorker_IdeologicalDebatePrecept.DebateRankEpsilon;

    internal IReadOnlyList<IssueDef> HeldIssues()
    {
        if (_heldIssues != null) return _heldIssues;
        _heldIssues = [];
        foreach (var issue in _strength.Keys)
            if (HoldsStance(issue))
                _heldIssues.Add(issue);
        return _heldIssues;
    }

    // Seed all issues for this pawn once. Returns true when a fresh seed occurred for a pawn that
    // already has a played-in certainty, signalling the caller to schedule stance calibration.
    internal bool EnsureSeeded(bool certaintyInitialized)
    {
        if (_seeded) return false;
        _seeded = true;

        var freshSeed = _strength.Count == 0;
        float? traitOffset = null;
        var seededNow = new List<IssueDef>();
        foreach (var issue in DefDatabase<IssueDef>.AllDefs)
        {
            if (_strength.ContainsKey(issue)) continue;
            if (PreceptPolicy.CategoryOf(issue) == PreceptCategory.NA) continue;
            traitOffset ??= ConvictionStrengthOffset();
            SetRank(issue, HeldRank(_pawn.Ideo!, issue));
            _strength[issue] = Mathf.Clamp(
                Rand.Range(ConvictionScale.BaseConvictionMin, ConvictionScale.BaseConvictionMax) + traitOffset.Value,
                ConvictionScale.MinConvictionStrength, ConvictionScale.AbsoluteMaxConvictionStrength);
            seededNow.Add(issue);
        }

        if (freshSeed)
        {
            if (!certaintyInitialized)
                ApplyHeterodoxy();
            ApplyDietGeneStanceOverride();
        }
        if (seededNow.Count > 0)
            ApplyTraitTargets(seededNow);

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

    // Per-pawn shift to conviction strength from personality traits (docs/design.md "Strength").
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
        SetRank(issue, current + ((targetRank - current) * pull * brainwipeMultiplier));
        _strength[issue] = Mathf.Clamp(
            _strength[issue] + strengthDelta * brainwipeMultiplier,
            ConvictionScale.MinConvictionStrength, ConvictionScale.AbsoluteMaxConvictionStrength);
    }

    // Set stance to an absolute (rank, strength).
    internal void SetStance(IssueDef issue, float rank, float strength)
    {
        SetRank(issue, rank);
        _strength[issue] = Mathf.Clamp(strength, ConvictionScale.MinConvictionStrength, ConvictionScale.AbsoluteMaxConvictionStrength);
    }

    // Carry every stored rank from the ladder it was saved against to the live ladder. Returns true if a rank changed.
    internal bool RemapToLiveLadders(Func<IssueDef, SavedLadder?> savedLadderFor)
    {
        var changed = false;
        foreach (var issue in _preferredRank.Keys.ToList())
        {
            if (savedLadderFor(issue) is not { } saved) continue;
            var rank = LadderMigration.Remap(issue, saved, _preferredRank[issue]);
            changed |= rank != _preferredRank[issue];
            SetRank(issue, rank);
        }
        return changed;
    }

    // Move every stance onto the rung `ideo` holds. Strengths do not change.
    internal void ResetRanksToHeld(Ideo ideo)
    {
        foreach (var issue in _preferredRank.Keys.ToList())
            SetRank(issue, HeldRank(ideo, issue));
    }

    // Reset stances after a brainwipe. Does not zero certainty; caller handles that.
    internal void ApplyBrainwipe()
    {
        var traitFloor = Mathf.Max(0f, 3.0f + ConvictionStrengthOffset() / 3.0f);
        var alignedFloor = Mathf.Max(traitFloor, 5f);
        var targets = TraitStanceTargets.For(_pawn, _preferredRank);

        foreach (var issue in _strength.Keys.ToList())
        {
            var traitAligned = targets.ContainsKey(issue);
            _strength[issue] = traitAligned ? alignedFloor : traitFloor;

            var keepStance = traitFloor > 1f || traitAligned;
            if (!keepStance)
            {
                var rungCount = PreceptLadder.Rungs(issue).Count;
                if (rungCount > 1)
                    SetRank(issue, Rand.Range(0, rungCount));
            }
        }

        // Traits survive a brainwipe, so issues they target return to the target rank.
        foreach (var (issue, target) in targets)
            SetStance(issue, target.PullFrom(_preferredRank[issue]).rank, alignedFloor);
    }

    // Apply conviction decay for the current game day. Returns true when decay was applied (i.e. caller
    // should mark structural opinion caches dirty). An issue with a trait target (TraitStanceTargets) moves along
    // the conviction valley toward that target. All other issues drain toward zero strength.
    internal bool ApplyDecayIfNewDay()
    {
        var currentDay = GenTicks.TicksAbs / GenDate.TicksPerDay;
        if (currentDay == _lastDecayDay) return false;
        _lastDecayDay = currentDay;

        var lossFactor = _pawn.GetStatValue(StatDefOf.CertaintyLossFactor);
        var drainPerDay = EnhancedIdeologyMod.Settings.ConvictionDecayRate
            / (GenDate.TicksPerSeason / (float)GenDate.TicksPerDay) / lossFactor;
        if (drainPerDay <= 0f) return false;

        var traitTargets = TraitStanceTargets.For(_pawn, _preferredRank);
        foreach (var issue in _strength.Keys.ToList())
        {
            if (traitTargets.TryGetValue(issue, out var target))
            {
                var (targetRank, targetStrength) = target.PullFrom(_preferredRank[issue]);
                var farRank = ConvictionMath.LadderExtremeAwayFrom(issue, targetRank);
                var (rank, strength) = ConvictionMath.ValleyStep(
                    _preferredRank[issue], _strength[issue], targetRank, farRank, targetStrength, drainPerDay);
                SetStance(issue, rank, strength);
            }
            else
            {
                _strength[issue] = Mathf.Max(ConvictionScale.MinConvictionStrength, _strength[issue] - drainPerDay);
            }
        }

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
            _seeded = false;
            _heldIssues = null;
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
            SetRank(issue, FlippedRank(issue, _preferredRank[issue]));
    }

    private static float FlippedRank(IssueDef issue, float orthodoxRank)
    {
        var rungCount = PreceptLadder.Rungs(issue).Count;
        var current = Mathf.RoundToInt(orthodoxRank);
        return Enumerable.Range(0, rungCount)
            .Where(rank => rank != current)
            .RandomElementByWeight(rank => 1f / (1f + Mathf.Abs(rank - current)));
    }

    // Trait targets (TraitStanceTargets), applied after the rest of generation to the issues seeded now.
    // Strengths come from ConvictionScale.TraitVoteStrength of the votes; a new stance varies by ±TraitSeedSpread.
    // Toward a rung: a faith that is silent or holds another rung gives the pawn a new stance on the target; a faith
    // that holds it adds the vote strength. Against rungs: a stance on an opposed rung loses the vote strength, or,
    // when the faith is silent, takes the exit as a new stance. A stance on an allowed spot does not change.
    private void ApplyTraitTargets(List<IssueDef> issues)
    {
        var ideo = _pawn.Ideo!;
        var sources = EnhancedIdeologyMod.Settings.DebugInteractionWorkers ? new Dictionary<IssueDef, List<string>>() : null;
        var targets = TraitStanceTargets.For(_pawn, _preferredRank, sources);
        var log = new List<string>();
        foreach (var issue in issues)
        {
            if (!targets.TryGetValue(issue, out var target)) continue;
            var faithHolds = ideo.precepts.Any(precept => precept.def.issue == issue)
                || PreceptPolicy.InducedRank(ideo, issue) != null;
            var heldRank = HeldRank(ideo, issue);
            var current = _preferredRank[issue];
            var opposedWeight = target.OpposedWeight(current);
            string outcome;
            if (target.Rung is { } rung && !faithHolds)
            {
                SetStance(issue, rung, Spread(ConvictionScale.TraitVoteStrength(target.Votes)));
                outcome = "faith silent, new stance";
            }
            else if (target.Rung is { } heldTarget && heldRank == heldTarget)
            {
                var bonus = ConvictionScale.TraitVoteStrength(target.Votes);
                SetStance(issue, heldTarget, _strength[issue] + bonus);
                outcome = $"faith holds target, strength {bonus:+0.#;-0.#}";
            }
            else if (target.Rung is { } otherRung)
            {
                SetStance(issue, otherRung, Spread(ConvictionScale.TraitVoteStrength(target.Votes)));
                outcome = $"heterodox, faith holds {TraitStanceTargets.RungName(issue, heldRank)}";
            }
            else if (opposedWeight == 0f)
            {
                outcome = "stance not opposed, unchanged";
            }
            else if (!faithHolds)
            {
                var (exitRank, exitStrength) = target.PullFrom(current);
                SetStance(issue, exitRank, Spread(exitStrength));
                outcome = "faith silent on an opposed Don't-care, new stance";
            }
            else
            {
                var penalty = ConvictionScale.TraitVoteStrength(opposedWeight);
                SetStance(issue, current, _strength[issue] - penalty);
                outcome = $"stance on opposed rung, strength {-penalty:+0.#;-0.#}";
            }

            if (sources != null)
                log.Add(DescribeTarget(issue, target, sources[issue], outcome));
        }

        if (log.Count > 0)
            EnhancedIdeologyMod.Debug($"{TraitTargetsHeader()}\n{string.Join("\n", log)}");
    }

    private static float Spread(float strength) =>
        strength * Rand.Range(1f - ConvictionScale.TraitSeedSpread, 1f + ConvictionScale.TraitSeedSpread);

    // The trait target on `issue`, or null when the pawn's traits do not vote on it. For the opinion card tooltip.
    internal TraitStanceTarget? TraitTarget(IssueDef issue) =>
        TraitStanceTargets.For(_pawn, new Dictionary<IssueDef, float> { [issue] = _preferredRank[issue] })
            .GetValueOrDefault(issue);

    // The pawn's trait targets on all seeded issues and the votes behind each, for the debug action.
    internal string DescribeTraitTargets()
    {
        var sources = new Dictionary<IssueDef, List<string>>();
        var targets = TraitStanceTargets.For(_pawn, _preferredRank, sources);
        if (targets.Count == 0) return $"{TraitTargetsHeader()}\n  no trait votes";
        return $"{TraitTargetsHeader()}\n"
            + string.Join("\n", targets.Select(kv => DescribeTarget(kv.Key, kv.Value, sources[kv.Key], null)));
    }

    private string TraitTargetsHeader() =>
        $"TraitStanceTargets: {_pawn.LabelShort} ({_pawn.Ideo?.name}), traits "
        + string.Join(" ", _pawn.story.traits.allTraits.Select(trait => $"{trait.def.defName}({trait.Degree})"));

    private string DescribeTarget(IssueDef issue, TraitStanceTarget target, List<string> sources, string? outcome)
    {
        var aim = target.Rung is { } rung
            ? $"target {TraitStanceTargets.RungName(issue, rung)}"
            : "avoids " + string.Join(" ", target.Opposed.Keys.OrderBy(rank => rank)
                .Select(rank => TraitStanceTargets.RungName(issue, rank)));
        var (pullRank, pullStrength) = target.PullFrom(_preferredRank[issue]);
        return $"  {issue.defName}: votes {target.Votes:+0.##;-0.##}, {aim}"
            + (outcome == null ? "" : $" -> {outcome}")
            + $"; now {TraitStanceTargets.RungName(issue, _preferredRank[issue])} @ {_strength[issue]:0.#}"
            + $", pull {TraitStanceTargets.RungName(issue, pullRank)} @ {pullStrength:0.#}"
            + string.Concat(sources.Select(line => "\n    " + line));
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
}
