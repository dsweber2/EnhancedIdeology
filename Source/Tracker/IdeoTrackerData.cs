using System.Text;

namespace EnhancedIdeology;

[HotSwappable]
internal sealed class IdeoTrackerData(Pawn pawn) : IExposable
{
    public const float PawnOpinionFactor = 0.02f;

    private Pawn pawn = pawn;
    public Pawn Pawn => pawn;
    internal IssueStanceTracker Stances { get; private set; } = new IssueStanceTracker(pawn);
    internal CertaintyTracker Certainty { get; private set; } = new CertaintyTracker(pawn);
    internal OpinionCache Opinions { get; private set; } = new OpinionCache(pawn);
    public void ForceNewPawn(Pawn newPawn)
    {
        pawn = newPawn;
        Stances.SetPawn(newPawn);
        Certainty.SetPawn(newPawn);
        Opinions.SetPawn(newPawn);
    }

    // Forwarding properties — state lives in CertaintyTracker.
    public float ExtendedCertainty => Certainty.ExtendedCertainty;
    public void SetExtendedCertainty(float value) => Certainty.SetExtendedCertainty(value);
    internal void AdvanceExtendedCertainty(float deltaDays) => Certainty.AdvanceExtendedCertainty(deltaDays);
    public float CachedCertaintyChange => Certainty.CachedCertaintyChange;
    public float CachedTargetCertainty => Certainty.CachedTargetCertainty;
    public float CachedStructural => Certainty.CachedStructural;
    public float CachedRelational => Certainty.CachedRelational;
    public float CachedPractitional => Certainty.CachedPractitional;
    public List<(string label, float pct)> StructuralContributors => Certainty.StructuralContributors;
    public List<(string label, float pct)> RelationalContributors => Certainty.RelationalContributors;
    public List<(string label, float pct)> PractitionalContributors => Certainty.PractitionalContributors;

    // Called for pawns whose saves predate EB's issue-stance model (vanilla or old EB). Marks certainty as
    // already initialized (no snap) and schedules one calibration pass to align the structural setpoint with
    // the certainty the pawn already has.
    internal void MarkAsLoadedWithoutData(float existingCertainty) =>
        Certainty.MarkAsLoadedWithoutData(existingCertainty);

    public void SetIdeoBaseOpinion(Ideo ideo, float opinion) => Opinions.SetIdeoBaseOpinion(ideo, opinion);
    public void AdjustMemeOpinion(MemeDef meme, float power) => Opinions.AdjustMemeOpinion(meme, power);
    public float TrueMemeOpinion(MemeDef meme) => Opinions.TrueMemeOpinion(meme);
    public void CacheRelationshipIdeoOpinion(Ideo ideo, GameComponent_EnhancedIdeology? comp = null) => Opinions.CacheRelationshipIdeoOpinion(ideo, comp);
    public void RecalculateRelationshipIdeoOpinions() => Opinions.RecalculateRelationshipIdeoOpinions();
    public IEnumerable<(Pawn pawn, float opinion)> GetOwnIdeoRelationships() => Opinions.GetOwnIdeoRelationships();

    private readonly List<Thought> _tmpThoughts = [];

    // Certainty is a first-order relaxation toward a setpoint (target certainty): dc/dt = k * (target - c).
    // The setpoint is the sum of three bands - structural (innate fit), relational (co-religionists) and
    // practitional (current precept moods).
    // No hard upper cap: certainty can exceed 1 when structural fit is very strong.
    public void CertaintyChangeRecache(GameComponent_EnhancedIdeology comp)
    {
        // Sync extended certainty with any external write to vanilla (tests, reassure, book, entrench, etc.).
        // AdvanceExtendedCertainty handles the tick path; this catches same-tick reads like the conversion check.
        Certainty.SyncFromVanillaIfNeeded(Pawn.ideo.Certainty);

        var settings = EnhancedIdeologyMod.Settings;

        Certainty.StructuralContributors.Clear();
        Certainty.RelationalContributors.Clear();
        Certainty.PractitionalContributors.Clear();

        // Structural band: innate fit of the pawn to their own ideo, from their per-issue precept stances.
        var structural = StructuralOpinionOf(Pawn.Ideo!, Certainty.StructuralContributors) / 100f;

        // Relational band: mean opinion of co-religionists, scaled by the user's max range.
        var relational = RelationalBand(settings.RelationalMaxRange, Certainty.RelationalContributors, comp);

        // Practitional band: summed precept-thought mood, scaled by the user's max range.
        var practitional = PractitionalBand(settings.PracticeMaxRange, Certainty.PractitionalContributors);

        Certainty.SetBands(structural, relational, practitional);

        if (Certainty.TryApplyCalibration(structural, Stances))
        {
            Certainty.StructuralContributors.Clear();
            structural = StructuralOpinionOf(Pawn.Ideo!, Certainty.StructuralContributors) / 100f;
            Certainty.SetBands(structural, relational, practitional);
        }

        Certainty.FinalizeRecache(structural, settings.CertaintyDriftRate);
    }

    private float RelationalBand(float maxRange, List<(string label, float pct)> contributors, GameComponent_EnhancedIdeology comp)
    {
        Opinions.CacheRelationshipIdeoOpinion(Pawn.Ideo!, comp);

        float sum = 0;
        int count = 0;
        foreach (var (_, opinion) in Opinions.GetOwnIdeoRelationships())
        {
            sum += opinion;
            count++;
        }

        if (count == 0)
        {
            return 0f;
        }

        var band = GameComponent_EnhancedIdeology.RelationalIntensityCurve.Evaluate(sum / count) * maxRange;

        if (Math.Abs(sum) > 0.001f)
        {
            foreach (var (relPawn, opinion) in Opinions.GetOwnIdeoRelationships())
            {
                if (opinion != 0f)
                {
                    contributors.Add((relPawn.LabelShort, band * (opinion / sum)));
                }
            }
        }

        return band;
    }

    private float PractitionalBand(float maxRange, List<(string label, float pct)> contributors)
    {
        _tmpThoughts.Clear();
        Pawn.needs?.mood?.thoughts?.GetAllMoodThoughts(_tmpThoughts);

        float moodSum = 0;
        foreach (var thought in _tmpThoughts)
        {
            if (thought.sourcePrecept == null && !(thought.def.Worker is ThoughtWorker_Precept))
                continue;
            var offset = thought.MoodOffset();
            // A ritual from another faith enjoyed here is a pull away from your own, not toward it.
            moodSum += thought.sourcePrecept != null && thought.sourcePrecept.ideo != Pawn.Ideo
                ? -offset
                : offset;
        }

        var band = GameComponent_EnhancedIdeology.PracticeIntensityCurve.Evaluate(moodSum) * maxRange;

        if (Math.Abs(moodSum) > 0.001f)
        {
            foreach (var thought in _tmpThoughts)
            {
                if (thought.sourcePrecept == null && !(thought.def.Worker is ThoughtWorker_Precept))
                    continue;
                var offset = thought.MoodOffset();
                if (offset == 0f) continue;
                var signedOffset = thought.sourcePrecept != null && thought.sourcePrecept.ideo != Pawn.Ideo
                    ? -offset
                    : offset;
                contributors.Add((thought.LabelCap, band * (signedOffset / moodSum)));
            }
        }

        return band;
    }

    // Form opinion based on memes, personal thoughts and experience with other pawns from that ideo
    public float IdeoOpinion(Ideo ideo)
    {
        RefreshBaseOpinionsIfDirty();
        if (!Opinions.BaseIdeoOpinions.ContainsKey(ideo) || !Opinions.PersonalIdeoOpinions.ContainsKey(ideo))
        {
            Opinions.BaseIdeoOpinions[ideo] = StructuralIdeoOpinion(ideo);
            Opinions.PersonalIdeoOpinions[ideo] = 0;
        }

        if (ideo == Pawn.Ideo)
        {
            Opinions.BaseIdeoOpinions[ideo] = ExtendedCertainty * 100f;
        }

        return Mathf.Max(
            Opinions.BaseIdeoOpinions[ideo] +
            PersonalIdeoOpinion(ideo, out var _) +
            IdeoOpinionFromRelationships(ideo, false, out var _), 0) / 100f;
    }

    // Rundown on the function above, for UI reasons
    public DetailedIdeoOpinion DetailedIdeoOpinion(Ideo ideo, bool noRelationship = false)
    {
        RefreshBaseOpinionsIfDirty();
        if (!Opinions.BaseIdeoOpinions.ContainsKey(ideo))
        {
            _ = IdeoOpinion(ideo);
        }

        string? relationshipDevModeDetails = null;
        var personalOpinion = PersonalIdeoOpinion(ideo, out var personalDevModeDetails) / 100f;
        var relationshipOpinion = noRelationship ? 0 : IdeoOpinionFromRelationships(ideo, true, out relationshipDevModeDetails) / 100f;
        return new DetailedIdeoOpinion
        (
             ideo == Pawn.Ideo ? ExtendedCertainty : Opinions.BaseIdeoOpinions[ideo] / 100f,
             personalOpinion,
             relationshipOpinion,
             personalDevModeDetails +
                (relationshipDevModeDetails != null
                    ? "\n" + relationshipDevModeDetails
                    : "")
        );
    }

    // Get pawn's basic opinion from hearing about ideos beliefs, based on their traits, relationships and current ideo.
    // Own-ideo short-circuits to current certainty; call StructuralOpinionOf directly for the certainty-independent value.
    public float StructuralIdeoOpinion(Ideo ideo)
    {
        if (ideo == Pawn.Ideo)
        {
            return ExtendedCertainty * 100f;
        }

        return StructuralOpinionOf(ideo);
    }

    // Certainty-independent structural opinion (0-100). Delegates to StructuralOpinionCalculator.
    private float StructuralOpinionOf(Ideo ideo, List<(string label, float pct)>? contributors = null)
    {
        EnsureIssueStancesSeeded();
        return StructuralOpinionCalculator.Compute(Pawn, Stances, ideo, contributors);
    }

    // Signed per-issue opinion toward `ideo` on `issue`, for the opinion tab's per-precept agreement display.
    public float IssueOpinionToward(Ideo ideo, IssueDef issue)
    {
        EnsureIssueStancesSeeded();
        var inducedTargets = StructuralOpinionCalculator.InducedTargets(Stances, ideo);
        return StructuralOpinionCalculator.PerIssueOpinion(
            Stances, Pawn.Ideo!, ideo, issue, inducedTargets, EnhancedIdeologyMod.Settings.PreceptOppositionScale, out _);
    }

    public IssueDef? MostOpposingIssue(Ideo ideo)
    {
        var issues = MostOpposingIssues(ideo, 1);
        return issues.Count > 0 ? issues[0] : null;
    }

    // The `n` issues the pawn's stance most opposes about `ideo`, sampled proportionally to opposition
    // magnitude via exponential racing (-log(U)/w). If guideTracker is supplied, issues where the guide
    // holds weaker conviction are excluded. Falls back to all eligible issues when nothing opposes.
    // `stable` skips the random draw and orders strictly by opposition magnitude instead, so repeated
    // calls against unchanged stances (e.g. a tooltip re-rendered every frame) return the same list.
    public IReadOnlyList<IssueDef> MostOpposingIssues(Ideo ideo, int n, IdeoTrackerData? guideTracker = null, bool stable = false)
    {
        EnsureIssueStancesSeeded();
        var inducedTargets = StructuralOpinionCalculator.InducedTargets(Stances, ideo);
        var oppositionScale = EnhancedIdeologyMod.Settings.PreceptOppositionScale;

        var scored = ideo.precepts.Select(precept => precept.def.issue)
            .Where(issue => issue != null).Distinct()
            .Select(issue => (issue: issue!, opinion: StructuralOpinionCalculator.PerIssueOpinion(
                Stances, Pawn.Ideo!, ideo, issue!, inducedTargets, oppositionScale, out var graded), graded))
            .Where(entry => entry.graded
                && StructuralOpinionCalculator.WorthTargeting(Stances, entry.issue,
                    IssueStanceTracker.HeldRank(ideo, entry.issue), guideTracker?.Stances))
            .ToList();

        var pool = scored.Any(e => e.opinion < 0f)
            ? scored.Where(e => e.opinion < 0f).ToList()
            : scored;

        if (stable)
        {
            return pool
                .OrderBy(entry => entry.opinion)
                .ThenBy(entry => entry.issue.defName)
                .Take(n)
                .Select(entry => entry.issue)
                .ToList();
        }

        return pool
            .OrderBy(entry => -Mathf.Log(Rand.Value) / Mathf.Max(-entry.opinion, float.Epsilon))
            .Take(n)
            .Select(entry => entry.issue)
            .ToList();
    }

    // Dev-only: full per-issue opinion breakdown for diagnosing structural opinions.
    public string IssueOpinionDebug(Ideo ideo, IssueDef issue)
    {
        EnsureIssueStancesSeeded();
        return StructuralOpinionCalculator.IssueOpinionDebug(Stances, ideo, issue);
    }

    // The `n` issues most divergent from the pawn's own ideo's orthodox rungs. Reassure targets this bundle.
    // Fallback when already fully orthodox: `n` weakest-held beliefs.
    public IReadOnlyList<IssueDef> MostHeterodoxIssues(int n, IdeoTrackerData? guideTracker = null)
    {
        EnsureIssueStancesSeeded();

        // Issues without a seeded stance (PreceptCategory.NA: buildings, ritual seats, naming) aren't a belief
        // axis and never get a personal stance recorded, so they're excluded here to stay in sync with
        // IssueStanceTracker.EnsureSeeded.
        var ideoIssues = Pawn.Ideo!.precepts.Select(precept => precept.def.issue)
            .Where(issue => issue != null && PreceptPolicy.CategoryOf(issue) != PreceptCategory.NA).Distinct()
            .Select(issue => issue!)
            .ToList();

        var divergent = ideoIssues
            .Select(issue => (issue, divergence: Mathf.Abs(Stances.GetRank(issue) - IssueStanceTracker.HeldRank(Pawn.Ideo!, issue))))
            .Where(entry => entry.divergence > InteractionWorker_IdeologicalDebatePrecept.DebateRankEpsilon)
            .OrderByDescending(entry => entry.divergence)
            .Take(n)
            .Select(entry => entry.issue)
            .ToList();

        if (divergent.Count > 0) return divergent;

        return ideoIssues
            .Where(issue => StructuralOpinionCalculator.WorthTargeting(Stances, issue, IssueStanceTracker.HeldRank(Pawn.Ideo, issue), guideTracker?.Stances))
            .OrderBy(issue => Stances.GetStrength(issue))
            .Take(n)
            .ToList();
    }

    private void EnsureIssueStancesSeeded()
    {
        if (Stances.EnsureSeeded(Certainty.IsInitialized))
            Certainty.ScheduleCalibration(Pawn.ideo.Certainty);
    }

    // Persuasion write-path (design.md R2). Nudge the pawn's personal stance on `issue`: slide the
    // preferred rung a `pull` fraction (0-1) of the remaining gap toward `targetRank`, and shift conviction
    // by `strengthDelta` points. This is how debates and books move belief - the personal preferred rung
    // drifts away from the pawn's own ideo toward whatever is being argued, eroding structural fit with their
    // faith and raising it toward the persuader's. Structural opinions cached from the old stance are now
    // stale, so base opinions are marked dirty and refreshed on the next read.
    public float BrainwipeSusceptibilityMultiplier =>
        Pawn.health.hediffSet.GetFirstHediffOfDef(EnhancedIdeologyDefOf.EB_BrainwipeRecovery)
            ?.TryGetComp<HediffComp_BrainwipeRecovery>()
            ?.SusceptibilityMultiplier ?? 1f;

    internal static bool HasBrainwipeRecovery(Pawn pawn) =>
        pawn.health.hediffSet.GetFirstHediffOfDef(EnhancedIdeologyDefOf.EB_BrainwipeRecovery) != null;

    public void ShiftIssueStance(IssueDef issue, float targetRank, float pull, float strengthDelta)
    {
        EnsureIssueStancesSeeded();
        Stances.ShiftStance(issue, targetRank, pull, strengthDelta, BrainwipeSusceptibilityMultiplier);
        Opinions.MarkDirty();
    }

    // Set the pawn's stance on `issue` to an absolute (rank, strength). The conviction-valley debate
    // write-path (PullStance) computes the whole new stance at once, since rank and strength are coupled
    // along the curve — unlike ShiftIssueStance's independent deltas.
    public void SetIssueStance(IssueDef issue, float rank, float strength)
    {
        EnsureIssueStancesSeeded();
        Stances.SetStance(issue, rank, strength);
        Opinions.MarkDirty();
    }

    // Reset stances after a brainwipe. Strength floors at max(0, 3 + traitOffset/3).
    public void ApplyBrainwipe()
    {
        EnsureIssueStancesSeeded();
        Stances.ApplyBrainwipe();
        SetExtendedCertainty(0f);
        Opinions.MarkDirty();
    }

    // Recompute the cached structural opinions if a stance shift has invalidated them. Called at the top of
    // every read that consumes base opinions, so a batch of ShiftIssueStance calls pays one recompute.
    private void RefreshBaseOpinionsIfDirty()
    {
        if (!Opinions.IsDirty) return;
        Opinions.ClearDirty();
        RecacheAllBaseOpinions();
    }

    // The pawn's personal stance on every seeded issue: (issue, preferred rung rank, conviction strength).
    // Rank can be fractional once debates have dragged it between rungs; a rank below 0 is the Don't-care rung.
    public IEnumerable<(IssueDef issue, float rank, float strength)> IssueStances()
    {
        EnsureIssueStancesSeeded();
        return Stances.IssueStances();
    }

    public float PersonalIdeoOpinion(Ideo ideo, out string? devDetails)
    {
        RefreshBaseOpinionsIfDirty();
        if (Prefs.DevMode)
        {
            var devDetailsBuilder = new StringBuilder();
            _ = devDetailsBuilder
                .AppendLine($"Base opinion: {Opinions.BaseIdeoOpinions.GetValueOrDefault(ideo, StructuralIdeoOpinion(ideo))}")
                .AppendLine($"Personal opinion: {Opinions.PersonalIdeoOpinions.GetValueOrDefault(ideo, 0)}");
            var relevantMemeCount = ideo.memes.Intersect(Opinions.MemeOpinions.Keys).Count();
            _ = devDetailsBuilder
                .AppendLine($"Meme opinions: {relevantMemeCount}");
            foreach (var meme in ideo.memes)
            {
                if (Opinions.MemeOpinions.TryGetValue(meme, out var memeOpinion))
                    _ = devDetailsBuilder.AppendLine($" - {meme.LabelCap}: {memeOpinion}");
            }
            devDetails = devDetailsBuilder.ToString();
        }
        else
        {
            devDetails = null;
        }

        if (!Opinions.BaseIdeoOpinions.TryGetValue(ideo, out var baseIdeoOpinion))
        {
            baseIdeoOpinion = StructuralIdeoOpinion(ideo);
            Opinions.BaseIdeoOpinions[ideo] = baseIdeoOpinion;
        }
        if (!Opinions.PersonalIdeoOpinions.TryGetValue(ideo, out var personalIdeoOpinion))
        {
            personalIdeoOpinion = 0;
            Opinions.PersonalIdeoOpinions[ideo] = personalIdeoOpinion;
        }

        float opinion = 0;

        foreach (var meme in ideo.memes)
        {
            if (Opinions.MemeOpinions.TryGetValue(meme, out var memeOpinion))
                opinion += memeOpinion;
        }

        // Makes sure personal opinions cannot drive total below zero
        var curOpinion = Mathf.Max(baseIdeoOpinion + opinion, 0);
        if (Opinions.PersonalIdeoOpinions[ideo] < -curOpinion)
            Opinions.PersonalIdeoOpinions[ideo] = -curOpinion;

        return opinion + Opinions.PersonalIdeoOpinions[ideo];
    }

    public float IdeoOpinionFromRelationships(Ideo ideo, bool includeDevDetails, out string? devDetails)
    {
        if (Prefs.DevMode && includeDevDetails)
        {
            Opinions.CacheRelationshipIdeoOpinion(ideo);

            var devDetailsBuilder = new StringBuilder();
            _ = devDetailsBuilder
                .AppendLine($"Relationship opinion: {Opinions.CachedRelationshipIdeoOpinions.GetValueOrDefault(ideo, 0)}")
                .AppendLine($"Relationships: {Opinions.CachedRelationships.Count(p => p.Key.Ideo == ideo)}");
            foreach (var kvp in Opinions.CachedRelationships.Where(p => p.Key.Ideo == ideo))
                _ = devDetailsBuilder.AppendLine($" - {kvp.Key.Name}: {kvp.Value * PawnOpinionFactor} (scaled from {kvp.Value})");
            devDetails = devDetailsBuilder.ToString();
        }
        else
        {
            if (!Opinions.CachedRelationshipIdeoOpinions.ContainsKey(ideo))
                Opinions.CacheRelationshipIdeoOpinion(ideo);
            devDetails = null;
        }

        return Opinions.CachedRelationshipIdeoOpinions[ideo];
    }

    // Drain all issue conviction strengths uniformly by the global decay rate, once per game day.
    internal void ApplyConvictionDecayIfNewDay()
    {
        EnsureIssueStancesSeeded();
        if (Stances.ApplyDecayIfNewDay())
            Opinions.MarkDirty();
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref pawn, "pawn");
        Certainty.ExposeData();
        Opinions.ExposeData();
        Stances.ExposeData();

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
            if (Pawn == null) return;
            comp.SetIdeo(Pawn, Pawn.Ideo!);
        }
    }

    // Change pawn's personal opinion of another ideo, usually positively.
    // Stays in IdeoTrackerData because it calls StructuralIdeoOpinion to seed the base on first access.
    public void AdjustPersonalOpinion(Ideo ideo, float power)
    {
        EnhancedIdeologyMod.Debug($"AdjustPersonalOpinion called: pawn={Pawn}, ideo={ideo}, power={power}");
        if (!Opinions.BaseIdeoOpinions.ContainsKey(ideo) || !Opinions.PersonalIdeoOpinions.ContainsKey(ideo))
        {
            EnhancedIdeologyMod.Debug("AdjustPersonalOpinion: Initializing base/personal opinions.");
            Opinions.BaseIdeoOpinions[ideo] = StructuralIdeoOpinion(ideo);
            Opinions.PersonalIdeoOpinions[ideo] = 0;
        }

        Opinions.PersonalIdeoOpinions[ideo] += power * 100f;
        EnhancedIdeologyMod.Debug($"AdjustPersonalOpinion: new personalIdeoOpinion={Opinions.PersonalIdeoOpinions[ideo]}");
    }

    public void RecacheAllBaseOpinions()
    {
        foreach (var ideo in Opinions.BaseIdeoOpinions.Keys.ToList())
            Opinions.BaseIdeoOpinions[ideo] = StructuralIdeoOpinion(ideo);
    }

    // Forwarding wrappers — logic lives in ConversionEvaluator.
    public float ConversionProbability(Ideo candidate) => ConversionEvaluator.ConversionProbability(this, candidate);
    public float ConversionChanceAfterKnock(Ideo target, float knock) => ConversionEvaluator.ConversionChanceAfterKnock(this, target, knock);
    public ConversionOutcome CheckConversion(Ideo? priorityIdeo = null, bool noBreakdown = false, List<Ideo>? excludeIdeos = null, List<Ideo>? whitelistIdeos = null) =>
        ConversionEvaluator.CheckConversion(this, priorityIdeo, noBreakdown, excludeIdeos, whitelistIdeos);
    public void TryBackgroundConversion(float deltaDays) => ConversionEvaluator.TryBackgroundConversion(this, deltaDays);
    internal void TriggerCrisisOfFaith() => ConversionEvaluator.TriggerCrisisOfFaith(this);
    internal float EffectiveCrisisThreshold() => ConversionEvaluator.EffectiveCrisisThreshold(this);
    public static float HazardConversionChance(float p, float deltaDays, float intervalDays) => ConversionEvaluator.HazardConversionChance(p, deltaDays, intervalDays);
}

internal readonly struct DetailedIdeoOpinion(float baseOpinion, float personalOpinion, float relationshipOpinion, string? devModeDetails = null)
{
    public readonly float BaseOpinion => baseOpinion;
    public readonly float PersonalOpinion => personalOpinion;
    public readonly float RelationshipOpinion => relationshipOpinion;
    public readonly string? DevModeDetails => devModeDetails;
}
