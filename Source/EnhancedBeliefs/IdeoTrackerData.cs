using System.Text;

namespace EnhancedBeliefs;

[HotSwappable]
internal sealed class IdeoTrackerData(Pawn pawn) : IExposable
{
    public const float PawnOpinionFactor = 0.02f;

    private Pawn pawn = pawn;
    public Pawn Pawn => pawn;
    public void ForceNewPawn(Pawn newPawn)
    {
        pawn = newPawn;
    }

    public float CachedCertaintyChange { get; private set; } = -9999f;

    // Setpoint (target certainty) and its bands, all in certainty fraction (0-1), refreshed by CertaintyChangeRecache.
    public float CachedTargetCertainty { get; private set; }
    public float CachedStructural { get; private set; }
    public float CachedRelational { get; private set; }
    public float CachedPractitional { get; private set; }
    public float CachedDifficulty { get; private set; }

    // Top contributors to each band for the social-card tooltip; (label, certainty-fraction contribution).
    public readonly List<(string label, float pct)> StructuralContributors = [];
    public readonly List<(string label, float pct)> RelationalContributors = [];
    public readonly List<(string label, float pct)> PractitionalContributors = [];

    // Separate because recalculating base from memes in case player's ideo is fluid cuts down on overall performance cost
    // Breaks if you multiply opinion but you really shouldn't do that
    private Dictionary<Ideo, float> baseIdeoOpinions = [];
    private Dictionary<Ideo, float> personalIdeoOpinions = [];

    // Set when a stance shift invalidates the cached structural (base) opinions. Read paths refresh lazily so
    // a per-tick caller (book reading) can shift many issues cheaply and pay the one recompute only on read.
    private bool baseOpinionsDirty;

    private readonly Dictionary<Ideo, float> cachedRelationshipIdeoOpinions = [];
    private readonly Dictionary<Pawn, float> cachedRelationships = [];

    private Dictionary<MemeDef, float> memeOpinions = [];

    // R2 structural precept model: per issue, the pawn's preferred stance (rank on the issue's ladder) and
    // how strongly they hold it. Seeded once from the pawn's own ideo; opinion of any ideo's stances is
    // derived via PreceptLadder. Separate from preceptOpinions above, which is the debate/personal delta.
    private Dictionary<IssueDef, float> issuePreferredRank = [];
    private Dictionary<IssueDef, float> issueStrength = [];

    private List<Ideo>? cache1;
    private List<Ideo>? cache2;
    private List<MemeDef>? cache3;
    private List<float>? cache5;
    private List<float>? cache6;
    private List<float>? cache7;
    private List<IssueDef>? cache9;
    private List<IssueDef>? cache10;
    private List<float>? cache11;
    private List<float>? cache12;

    public void SetIdeoBaseOpinion(Ideo ideo, float opinion)
    {
        baseIdeoOpinions[ideo] = opinion;
        // Establish the personal-opinion entry too, so IdeoOpinion's "recompute if unknown" guard treats
        // this ideo as known and does not clobber the base we just set.
        _ = personalIdeoOpinions.TryAdd(ideo, 0f);
    }

    private readonly List<Thought> _tmpThoughts = [];

    // Certainty is a first-order relaxation toward a setpoint (target certainty): dc/dt = k * (target - c).
    // The setpoint is the sum of three bands - structural (innate fit), relational (co-religionists) and
    // practitional (current precept moods) - plus a difficulty offset, all clamped to [0, 1]. There is no
    // forcing term, so certainty can never leave [0, 1] and always drifts toward where the pawn "belongs".
#pragma warning disable IDE0060 // Remove unused parameter
    // TODO: Figure out why worldComp was even passed here
    public void CertaintyChangeRecache(GameComponent_EnhancedBeliefs worldComp)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        var settings = EnhancedBeliefsMod.Settings;

        StructuralContributors.Clear();
        RelationalContributors.Clear();
        PractitionalContributors.Clear();

        // Structural band: innate fit of the pawn to their own ideo, from their per-issue precept stances.
        var structural = StructuralOpinionOf(Pawn.Ideo, StructuralContributors) / 100f;
        CachedStructural = structural;

        // Relational band: mean opinion of co-religionists, scaled by the user's max range.
        CachedRelational = RelationalBand(settings.RelationalMaxRange, RelationalContributors);

        // Practitional band: summed precept-thought mood, scaled by the user's max range.
        CachedPractitional = PractitionalBand(settings.PracticeMaxRange, PractitionalContributors);

        CachedDifficulty = settings.DifficultyOffset;
        var target = Mathf.Clamp01(structural + CachedRelational + CachedPractitional + settings.DifficultyOffset);
        CachedTargetCertainty = target;

        CachedCertaintyChange = settings.CertaintyDriftRate * (target - Pawn.ideo.Certainty);
    }

    private float RelationalBand(float maxRange, List<(string label, float pct)> contributors)
    {
        CacheRelationshipIdeoOpinion(Pawn.Ideo);

        float sum = 0;
        float absSum = 0;
        int count = 0;
        foreach (var (_, opinion) in GetOwnIdeoRelationships())
        {
            sum += opinion;
            absSum += Math.Abs(opinion);
            count++;
        }

        if (count == 0)
        {
            return 0f;
        }

        var band = GameComponent_EnhancedBeliefs.RelationalIntensityCurve.Evaluate(sum / count) * maxRange;

        if (absSum > 0f)
        {
            foreach (var (relPawn, opinion) in GetOwnIdeoRelationships())
            {
                if (opinion != 0f)
                {
                    contributors.Add((relPawn.LabelShort, band * (opinion / absSum)));
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
        float absSum = 0;
        foreach (var thought in _tmpThoughts)
        {
            if (thought.sourcePrecept != null || thought.def.Worker is ThoughtWorker_Precept)
            {
                var offset = thought.MoodOffset();
                moodSum += offset;
                absSum += Math.Abs(offset);
            }
        }

        var band = GameComponent_EnhancedBeliefs.PracticeIntensityCurve.Evaluate(moodSum) * maxRange;

        if (absSum > 0f)
        {
            foreach (var thought in _tmpThoughts)
            {
                var offset = thought.MoodOffset();
                if (offset != 0f && (thought.sourcePrecept != null || thought.def.Worker is ThoughtWorker_Precept))
                {
                    contributors.Add((thought.LabelCap, band * (offset / absSum)));
                }
            }
        }

        return band;
    }

    // Form opinion based on memes, personal thoughts and experience with other pawns from that ideo
    public float IdeoOpinion(Ideo ideo)
    {
        RefreshBaseOpinionsIfDirty();
        if (!baseIdeoOpinions.ContainsKey(ideo) || !personalIdeoOpinions.ContainsKey(ideo))
        {
            baseIdeoOpinions[ideo] = StructuralIdeoOpinion(ideo);
            personalIdeoOpinions[ideo] = 0;
        }

        if (ideo == Pawn.Ideo)
        {
            baseIdeoOpinions[ideo] = Pawn.ideo.Certainty * 100f;
        }

        return Mathf.Clamp(
            baseIdeoOpinions[ideo] +
            PersonalIdeoOpinion(ideo, out var _) +
            IdeoOpinionFromRelationships(ideo, false, out var _), 0, 100) / 100f;
    }

    // Rundown on the function above, for UI reasons
    public DetailedIdeoOpinion DetailedIdeoOpinion(Ideo ideo, bool noRelationship = false)
    {
        RefreshBaseOpinionsIfDirty();
        if (!baseIdeoOpinions.ContainsKey(ideo))
        {
            _ = IdeoOpinion(ideo);
        }

        string? relationshipDevModeDetails = null;
        var personalOpinion = PersonalIdeoOpinion(ideo, out var personalDevModeDetails) / 100f;
        var relationshipOpinion = noRelationship ? 0 : IdeoOpinionFromRelationships(ideo, true, out relationshipDevModeDetails) / 100f;
        return new DetailedIdeoOpinion
        (
             ideo == Pawn.Ideo ? Pawn.ideo.Certainty : baseIdeoOpinions[ideo] / 100f,
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
            return Pawn.ideo.Certainty * 100f;
        }

        return StructuralOpinionOf(ideo);
    }

    // Certainty-independent structural opinion (0-100) a pawn holds toward an ideo based on their traits,
    // memes and the ideo's precepts. Shared by StructuralIdeoOpinion and the certainty setpoint's structural band.
    // If contributors is supplied, each term is recorded (in certainty-fraction units) for the tooltip breakdown.
    private float StructuralOpinionOf(Ideo ideo, List<(string label, float pct)>? contributors = null)
    {
        var pawnIdeo = Pawn.Ideo;
        var start = contributors?.Count ?? 0;
        float opinion = 0;

        // various global meme specific opinions
        if (pawnIdeo.HasMeme(EnhancedBeliefsDefOf.Supremacist))
        {
            opinion -= 20;
            contributors?.Add((EnhancedBeliefsDefOf.Supremacist.LabelCap, -20f));
        }
        else if (pawnIdeo.HasMeme(EnhancedBeliefsDefOf.Loyalist))
        {
            opinion -= 10;
            contributors?.Add((EnhancedBeliefsDefOf.Loyalist.LabelCap, -10f));
        }
        else if (pawnIdeo.HasMeme(EnhancedBeliefsDefOf.Guilty))
        {
            opinion += 10;
            contributors?.Add((EnhancedBeliefsDefOf.Guilty.LabelCap, 10f));
        }

        // pawn trait compatibility
        foreach (var meme in ideo.memes)
        {
            if (!meme.agreeableTraits.NullOrEmpty())
            {
                foreach (var trait in meme.agreeableTraits)
                {
                    if (trait.HasTrait(Pawn))
                    {
                        opinion += 10;
                        contributors?.Add((trait.def?.LabelCap ?? meme.LabelCap, 10f));
                    }
                }
            }

            if (!meme.disagreeableTraits.NullOrEmpty())
            {
                foreach (var trait in meme.disagreeableTraits)
                {
                    if (trait.HasTrait(Pawn))
                    {
                        opinion -= 10;
                        contributors?.Add((trait.def?.LabelCap ?? meme.LabelCap, -10f));
                    }
                }
            }
        }

        // Structural precept fit: for each issue at least one of the two faiths takes a position on, how the
        // target ideo's stance compares to the pawn's own preferred stance, weighted by conviction, averaged
        // and scaled to 0-100 (R2). Issues neither faith holds are irrelevant - there is nothing to agree or
        // disagree about - so they are excluded rather than counted as (mutual don't-care) agreement.
        EnsureIssueStancesSeeded();
        var zeroFrac = EnhancedBeliefsMod.Settings.PreceptZeroFrac;
        var preceptStart = contributors?.Count ?? 0;
        float preceptSum = 0;
        int issueCount = 0;
        // Coupled target issues (e.g. TreeCutting, induced by a stance on Trees) join the set even when
        // neither faith holds them explicitly, and grade by rung distance like a Moral issue regardless of
        // their own category.
        var inducedTargets = new HashSet<IssueDef>(
            PreceptPolicy.InducedIssues(pawnIdeo).Concat(PreceptPolicy.InducedIssues(ideo)));
        var relevantIssues = pawnIdeo.precepts.Select(precept => precept.def.issue)
            .Concat(ideo.precepts.Select(precept => precept.def.issue))
            .Where(issue => issue != null)
            .Concat(inducedTargets)
            .Distinct();
        foreach (var issue in relevantIssues)
        {
            var perIssue = PerIssueOpinion(ideo, issue, inducedTargets, zeroFrac, out var graded);
            if (!graded)
            {
                continue;
            }

            preceptSum += perIssue;
            issueCount++;
            if (contributors != null && perIssue != 0f)
            {
                contributors.Add((issue.LabelCap, perIssue));
            }
        }

        if (issueCount > 0)
        {
            opinion += (preceptSum / issueCount) * 5f;
            // The per-issue contributors were pushed as raw perIssue values; rescale to the averaged, 5x
            // precept contribution so they still sum to it (kept in 0-100 units; the block below /100s all).
            if (contributors != null)
            {
                for (var ii = preceptStart; ii < contributors.Count; ii++)
                {
                    contributors[ii] = (contributors[ii].label, contributors[ii].pct * 5f / issueCount);
                }
            }
        }

        // Universally-valued issues (Charity): a flat boost when the target ideo holds a stance on them,
        // regardless of the pawn's own view. Added after the Moral rescale so it is not averaged in.
        foreach (var issue in DefDatabase<IssueDef>.AllDefs)
        {
            if (PreceptPolicy.CategoryOf(issue) == PreceptCategory.UniversalPositive
                && ideo.precepts.Any(precept => precept.def.issue == issue))
            {
                opinion += UniversalPositiveBonus;
                contributors?.Add((issue.LabelCap, UniversalPositiveBonus));
            }
        }

        // Directional coupling penalties (e.g. despising mechanoids sours opinion of an ideo that enhances
        // mechanoid labour) - a flat hit scaled by the pawn's conviction on the offending issue, for couplings
        // whose target issue is single-rung and so cannot be graded by ladder distance.
        var couplingPenalty = PreceptPolicy.CouplingPenalty(pawnIdeo, ideo, issue => issueStrength[issue]);
        if (couplingPenalty != 0f)
        {
            opinion -= couplingPenalty;
            contributors?.Add(("EnhancedBeliefs.CouplingPenalty".Translate(), -couplingPenalty));
        }

        // Rescale the collected raw offsets into certainty-fraction units matching the band total.
        if (contributors != null)
        {
            for (var ii = start; ii < contributors.Count; ii++)
            {
                contributors[ii] = (contributors[ii].label, contributors[ii].pct / 100f);
            }
        }

        return Mathf.Clamp(opinion, 0, 100);
    }

    // Raw per-issue opinion (roughly +/-strength) the pawn holds toward `ideo`'s stance on `issue`: the same
    // ladder-distance / special-payload grade the structural band averages, before the /issueCount mean.
    // Moral issues (and coupled targets) grade by rung distance; Special issues carry bespoke categorical logic.
    // `graded` is false when neither faith takes a comparable position, so the caller leaves it out of the mean.
    private float PerIssueOpinion(Ideo ideo, IssueDef issue, HashSet<IssueDef> inducedTargets, float zeroFrac, out bool graded)
    {
        graded = true;
        var category = PreceptPolicy.CategoryOf(issue);
        if (category == PreceptCategory.Moral || inducedTargets.Contains(issue))
        {
            var pawnRank = issuePreferredRank[issue];
            var targetRank = HeldRank(ideo, issue);
            // Widen the extent to any induced rank sitting past the ladder ends, so a "beyond Don't-care"
            // stance reads as the axis extreme rather than falling outside it.
            var minRank = Mathf.Min(Mathf.Min(0f, PreceptLadder.DontCareRank(issue)), Mathf.Min(pawnRank, targetRank));
            var maxRank = Mathf.Max(PreceptLadder.Rungs(issue).Count - 1, Mathf.Max(pawnRank, targetRank));
            return PreceptLadder.OpinionOnPrecept(
                pawnRank, targetRank, minRank, maxRank, issueStrength[issue], zeroFrac);
        }

        // Weapons / PreferredXenotypes compare precept payloads directly; leader / mood are rank-based.
        // Either resolver returning false means the two faiths have no stance to compare.
        if (category == PreceptCategory.Special
            && (PreceptPolicy.TryPayloadSpecialOpinion(issue, Pawn.Ideo, ideo, issueStrength[issue], out var special)
                || PreceptPolicy.TrySpecialOpinion(
                    issue, issuePreferredRank[issue], HeldRank(ideo, issue), issueStrength[issue], zeroFrac, out special)))
        {
            return special;
        }

        graded = false;
        return 0f;
    }

    // Signed per-issue opinion (conviction-strength units) the pawn holds toward `ideo`'s stance on `issue`,
    // for the opinion tab's per-precept agreement display: positive means the pawn's stance agrees with what
    // `ideo` preaches on the issue, negative that it clashes. Ungraded issues (nothing to compare) read 0.
    public float IssueOpinionToward(Ideo ideo, IssueDef issue)
    {
        EnsureIssueStancesSeeded();
        var inducedTargets = new HashSet<IssueDef>(
            PreceptPolicy.InducedIssues(Pawn.Ideo).Concat(PreceptPolicy.InducedIssues(ideo)));
        return PerIssueOpinion(ideo, issue, inducedTargets, EnhancedBeliefsMod.Settings.PreceptZeroFrac, out _);
    }

    // Rank of the stance `ideo` holds on `issue`: an explicit precept if it has one, otherwise a stance a
    // cross-precept coupling induces (e.g. valuing trees implies disapproving of cutting them), otherwise the
    // virtual Don't-care rank. Because seeding reads this too, a pawn's own coupled stances seed correctly.
    private static float HeldRank(Ideo ideo, IssueDef issue)
    {
        foreach (var precept in ideo.precepts)
        {
            if (precept.def.issue == issue)
            {
                return PreceptLadder.RankOf(precept.def);
            }
        }

        return PreceptPolicy.InducedRank(ideo, issue) ?? PreceptLadder.DontCareRank(issue);
    }

    // Conviction-strength seeding. The base draw averages 15 (~75% starting certainty); full certainty sits
    // at a mean of MaxConvictionStrength. Personality shifts the whole pawn up or down (2b).
    internal const float BaseConvictionMin = 5f;
    internal const float BaseConvictionMax = 25f;
    private const float MinConvictionStrength = 0f;
    public const float MaxConvictionStrength = 20f;
    public const float AbsoluteMaxConvictionStrength = 50f;
    internal const float ConvictionPerTraitDegree = 3f;

    // Flat opinion bonus (0-100 units) for a UniversalPositive issue the target ideo values, e.g. Charity.
    private const float UniversalPositiveBonus = 5f;

    // Heterodoxy: at spawn a pawn quietly diverges from their faith on up to this many of the Moral positions
    // they hold least firmly. A static (not const) so tests can disable the RNG-consuming flip; the game uses
    // the default.
    internal const int DefaultHeterodoxyMax = 3;
    internal static int HeterodoxyMax = DefaultHeterodoxyMax;

    // Seed the pawn's preferred stance and conviction strength for every issue, once. Preferred stance is
    // whatever their own ideo holds (Don't-care where it is silent); strength is a U(12, 17) draw shifted by
    // the pawn's personality. A fresh seeding then applies heterodoxy.
    private void EnsureIssueStancesSeeded()
    {
        var freshSeed = issueStrength.Count == 0;
        var traitOffset = ConvictionStrengthOffset();
        foreach (var issue in DefDatabase<IssueDef>.AllDefs)
        {
            if (issueStrength.ContainsKey(issue))
            {
                continue;
            }

            issuePreferredRank[issue] = HeldRank(Pawn.Ideo, issue);
            issueStrength[issue] = Mathf.Clamp(
                Rand.Range(BaseConvictionMin, BaseConvictionMax) + traitOffset,
                MinConvictionStrength, AbsoluteMaxConvictionStrength);
        }

        if (freshSeed)
        {
            ApplyHeterodoxy();
        }
    }

    // Quietly diverge from the pawn's own ideo on a few of the Moral issues they hold least firmly: flip each to
    // a nearby rung, so a colonist can mildly disagree with their faith from the start. This lowers their fit
    // with their own ideo on those issues and makes them a live topic for same-faith debate.
    private void ApplyHeterodoxy()
    {
        var flipCount = Rand.RangeInclusive(0, HeterodoxyMax);
        if (flipCount == 0)
        {
            return;
        }

        var candidates = issueStrength.Keys
            .Where(issue => PreceptPolicy.CategoryOf(issue) == PreceptCategory.Moral
                && Pawn.Ideo.precepts.Any(precept => precept.def.issue == issue)
                && PreceptLadder.Rungs(issue).Count > 1)
            .OrderBy(issue => issueStrength[issue])
            .Take(flipCount)
            .ToList();

        foreach (var issue in candidates)
        {
            issuePreferredRank[issue] = FlippedRank(issue, issuePreferredRank[issue]);
        }
    }

    // A rung other than the one the pawn's ideo holds, weighted toward nearby rungs so a mild dissent is common
    // and a wholesale reversal rare.
    private static float FlippedRank(IssueDef issue, float orthodoxRank)
    {
        var rungCount = PreceptLadder.Rungs(issue).Count;
        var current = Mathf.RoundToInt(orthodoxRank);
        return Enumerable.Range(0, rungCount)
            .Where(rank => rank != current)
            .RandomElementByWeight(rank => 1f / (1f + Mathf.Abs(rank - current)));
    }

    // Persuasion write-path (design.md R2). Nudge the pawn's personal stance on `issue`: slide the
    // preferred rung a `pull` fraction (0-1) of the remaining gap toward `targetRank`, and shift conviction
    // by `strengthDelta` points. This is how debates and books move belief - the personal preferred rung
    // drifts away from the pawn's own ideo toward whatever is being argued, eroding structural fit with their
    // faith and raising it toward the persuader's. Structural opinions cached from the old stance are now
    // stale, so base opinions are marked dirty and refreshed on the next read.
    public void ShiftIssueStance(IssueDef issue, float targetRank, float pull, float strengthDelta)
    {
        EnsureIssueStancesSeeded();

        var current = issuePreferredRank[issue];
        issuePreferredRank[issue] = current + ((targetRank - current) * pull);
        issueStrength[issue] = Mathf.Clamp(
            issueStrength[issue] + strengthDelta, MinConvictionStrength, AbsoluteMaxConvictionStrength);

        baseOpinionsDirty = true;
    }

    // Recompute the cached structural opinions if a stance shift has invalidated them. Called at the top of
    // every read that consumes baseIdeoOpinions, so a batch of ShiftIssueStance calls pays one recompute.
    private void RefreshBaseOpinionsIfDirty()
    {
        if (!baseOpinionsDirty)
        {
            return;
        }

        baseOpinionsDirty = false;
        RecacheAllBaseOpinions();
    }

    // The pawn's personal stance on every seeded issue: (issue, preferred rung rank, conviction strength).
    // Rank can be fractional once debates have dragged it between rungs; a rank below 0 is the Don't-care rung.
    public IEnumerable<(IssueDef issue, float rank, float strength)> IssueStances()
    {
        EnsureIssueStancesSeeded();
        foreach (var (issue, strength) in issueStrength)
        {
            yield return (issue, issuePreferredRank[issue], strength);
        }
    }

    private float ConvictionStrengthOffset() => ConvictionOffsetFromTraits(Pawn.story.traits.allTraits);

    // Per-pawn shift to conviction strength from personality: strong-willed pawns hold beliefs more firmly,
    // anxious / pessimistic / neurotic ones more weakly (design.md R2, 2b). Signed by trait degree.
    internal static float ConvictionOffsetFromTraits(IEnumerable<Trait> traits)
    {
        var offset = 0f;
        foreach (var trait in traits)
        {
            switch (trait.def.defName)
            {
                case "Nerves": // iron-willed (+2) / steadfast (+1) strengthen; nervous (-1) / volatile (-2) weaken
                    offset += trait.Degree * ConvictionPerTraitDegree;
                    break;
                case "NaturalMood": // only the down side matters: pessimist (-1) / depressive (-2) weaken
                    if (trait.Degree < 0)
                    {
                        offset += trait.Degree * ConvictionPerTraitDegree;
                    }
                    break;
                case "Neurotic": // neurotic (+1) / very neurotic (+2) weaken, so subtract
                    offset -= trait.Degree * ConvictionPerTraitDegree;
                    break;
            }
        }

        return offset;
    }

    public float PersonalIdeoOpinion(Ideo ideo, out string? devDetails)
    {
        RefreshBaseOpinionsIfDirty();
        if (Prefs.DevMode)
        {
            var devDetailsBuilder = new StringBuilder();
            _ = devDetailsBuilder
                .AppendLine($"Base opinion: {baseIdeoOpinions.GetValueOrDefault(ideo, StructuralIdeoOpinion(ideo))}")
                .AppendLine($"Personal opinion: {personalIdeoOpinions.GetValueOrDefault(ideo, 0)}");
            var relevantMemeCount = ideo.memes.Intersect(memeOpinions.Keys).Count();
            _ = devDetailsBuilder
                .AppendLine($"Meme opinions: {relevantMemeCount}");
            foreach (var meme in ideo.memes)
            {
                if (memeOpinions.TryGetValue(meme, out var memeOpinion))
                {
                    _ = devDetailsBuilder.AppendLine($" - {meme.LabelCap}: {memeOpinion}");
                }
            }
            devDetails = devDetailsBuilder.ToString();
        }
        else
        {
            devDetails = null;
        }

        if (!baseIdeoOpinions.TryGetValue(ideo, out var baseIdeoOpinion))
        {
            baseIdeoOpinion = StructuralIdeoOpinion(ideo);
            baseIdeoOpinions[ideo] = baseIdeoOpinion;
        }
        if (!personalIdeoOpinions.TryGetValue(ideo, out var personalIdeoOpinion))
        {
            personalIdeoOpinion = 0;
            personalIdeoOpinions[ideo] = personalIdeoOpinion;
        }

        float opinion = 0;

        foreach (var meme in ideo.memes)
        {
            if (memeOpinions.TryGetValue(meme, out var memeOpinion))
            {
                opinion += memeOpinion;
            }
        }

        // Makes sure that pawn's personal opinion cannot go below/above 100% purely from circlejerking
        var curOpinion = Mathf.Clamp(baseIdeoOpinion + opinion, 0, 100);
        if (personalIdeoOpinion > 100f - curOpinion)
        {
            personalIdeoOpinions[ideo] = 100f - curOpinion;
        }
        else if (personalIdeoOpinions[ideo] < -curOpinion)
        {
            personalIdeoOpinions[ideo] = -curOpinion;
        }

        return opinion + personalIdeoOpinions[ideo];
    }

    public float IdeoOpinionFromRelationships(Ideo ideo, bool includeDevDetails, out string? devDetails)
    {
        if (Prefs.DevMode && includeDevDetails)
        {
            CacheRelationshipIdeoOpinion(ideo);

            var devDetailsBuilder = new StringBuilder();
            _ = devDetailsBuilder
                .AppendLine($"Relationship opinion: {cachedRelationshipIdeoOpinions.GetValueOrDefault(ideo, 0)}")
                .AppendLine($"Relationships: {cachedRelationships.Count(p => p.Key.Ideo == ideo)}");
            foreach (var kvp in cachedRelationships.Where(p => p.Key.Ideo == ideo))
            {
                _ = devDetailsBuilder.AppendLine($" - {kvp.Key.Name}: {kvp.Value * PawnOpinionFactor} (scaled from {kvp.Value})");
            }
            devDetails = devDetailsBuilder.ToString();
        }
        else
        {
            if (!cachedRelationshipIdeoOpinions.ContainsKey(ideo))
            {
                CacheRelationshipIdeoOpinion(ideo);
            }

            devDetails = null;
        }

        return cachedRelationshipIdeoOpinions[ideo];
    }

    // Calculates ideo opinion offset based on how much pawn likes other pawns of other ideos, should have little weight overall
    // Relationships are a dynamic mess of cosmic scale so there really isn't a better way to do this
    public void RecalculateRelationshipIdeoOpinions()
    {
        foreach (var ideo in baseIdeoOpinions.Keys)
        {
            CacheRelationshipIdeoOpinion(ideo);
        }
    }

    // Caches specific ideo opinion from relationships
    public void CacheRelationshipIdeoOpinion(Ideo ideo)
    {
        float opinion = 0;
        var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
        var pawns = comp.GetIdeoPawns(ideo);

        foreach (var otherPawn in pawns)
        {
            // Up to +-2 opinion per pawn
            float pawnOpinion = Pawn.relations.OpinionOf(otherPawn);
            opinion += pawnOpinion * PawnOpinionFactor;
            cachedRelationships[otherPawn] = pawnOpinion;
        }

        cachedRelationshipIdeoOpinions[ideo] = opinion;
    }

    public IEnumerable<(Pawn pawn, float opinion)> GetOwnIdeoRelationships()
    {
        var ideo = Pawn.Ideo;
        foreach (var kvp in cachedRelationships)
        {
            if (kvp.Key != Pawn && kvp.Key.Ideo == ideo)
                yield return (kvp.Key, kvp.Value);
        }
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref pawn, "pawn");
        Scribe_Collections.Look(ref baseIdeoOpinions, "baseIdeoOpinions", LookMode.Reference, LookMode.Value, ref cache1, ref cache5);
        Scribe_Collections.Look(ref personalIdeoOpinions, "personalIdeoOpinions", LookMode.Reference, LookMode.Value, ref cache2, ref cache6);
        Scribe_Collections.Look(ref memeOpinions, "memeOpinions", LookMode.Def, LookMode.Value, ref cache3, ref cache7);
        Scribe_Collections.Look(ref issuePreferredRank, "issuePreferredRank", LookMode.Def, LookMode.Value, ref cache9, ref cache11);
        Scribe_Collections.Look(ref issueStrength, "issueStrength", LookMode.Def, LookMode.Value, ref cache10, ref cache12);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();

            if (Pawn == null)
            {
                return;
            }

            comp.SetIdeo(Pawn, Pawn.Ideo);
        }
    }

    // Change pawn's personal opinion of another ideo, usually positively
    public void AdjustPersonalOpinion(Ideo ideo, float power)
    {
        EnhancedBeliefsMod.Debug($"AdjustPersonalOpinion called: pawn={Pawn}, ideo={ideo}, power={power}");
        if (!baseIdeoOpinions.ContainsKey(ideo) || !personalIdeoOpinions.ContainsKey(ideo))
        {
            EnhancedBeliefsMod.Debug("AdjustPersonalOpinion: Initializing base/personal opinions.");
            baseIdeoOpinions[ideo] = StructuralIdeoOpinion(ideo);
            personalIdeoOpinions[ideo] = 0;
        }

        personalIdeoOpinions[ideo] += power * 100f;
        EnhancedBeliefsMod.Debug($"AdjustPersonalOpinion: new personalIdeoOpinion={personalIdeoOpinions[ideo]}");
    }

    public void AdjustMemeOpinion(MemeDef meme, float power)
    {
        memeOpinions ??= [];

        if (!memeOpinions.ContainsKey(meme))
        {
            memeOpinions[meme] = 0;
        }

        memeOpinions[meme] += power * 100f;
    }

    public float TrueMemeOpinion(MemeDef meme)
    {
        if (!memeOpinions.TryGetValue(meme, out var opinion))
        {
            opinion = 0;
            memeOpinions[meme] = opinion;
        }

        if (!meme.agreeableTraits.NullOrEmpty())
        {
            foreach (var trait in meme.agreeableTraits)
            {
                if (trait.HasTrait(Pawn))
                {
                    opinion += 10;
                }
            }
        }

        if (!meme.disagreeableTraits.NullOrEmpty())
        {
            foreach (var trait in meme.disagreeableTraits)
            {
                if (trait.HasTrait(Pawn))
                {
                    opinion -= 10;
                }
            }
        }

        return opinion;
    }

    public void RecacheAllBaseOpinions()
    {
        foreach (var ideo in baseIdeoOpinions.Keys.ToList())
        {
            baseIdeoOpinions[ideo] = StructuralIdeoOpinion(ideo);
        }
    }

    // Relative-preference conversion probability toward `candidate`: 1 - opinionOfOwn/opinionOfCandidate,
    // and 0 unless the pawn genuinely prefers the candidate. Both sides are in the same "opinion" currency
    // (opinion of the current ideo is the pawn's certainty in it), so this is scale-aware: near-total
    // conviction resists even a strongly-liked alternative, while weakly-held belief flips easily.
    public float ConversionProbability(Ideo candidate)
    {
        var opinion = IdeoOpinion(candidate);
        var current = IdeoOpinion(Pawn.Ideo);
        return opinion > current ? (opinion - current) / opinion : 0f;
    }

    // Discrete, one-shot conversion driven by acute social pressure (debates, directed attempts). The pawn's
    // real ideos and a "crisis of faith" pseudo-candidate compete in one weighted draw; if the crisis wins,
    // that is the IdeoChange breakdown. No time integration here - the event itself is the occurrence.
    public ConversionOutcome CheckConversion(
        Ideo? priorityIdeo = null,
        bool noBreakdown = false,
        List<Ideo>? excludeIdeos = null,
        List<Ideo>? whitelistIdeos = null)
    {
        if (!ModLister.CheckIdeology("Ideoligion conversion") || Pawn.DevelopmentalStage.Baby() || Find.IdeoManager.classicMode)
        {
            return ConversionOutcome.Failure;
        }

        var current = IdeoOpinion(Pawn.Ideo);
        var candidates = new List<(Ideo? ideo, float chance, float weight)>();

        foreach (var ideo in whitelistIdeos ?? Find.IdeoManager.IdeosListForReading)
        {
            if (ideo == Pawn.Ideo || (excludeIdeos != null && excludeIdeos.Contains(ideo)))
            {
                continue;
            }

            var opinion = IdeoOpinion(ideo);
            if (opinion <= current)
            {
                continue;
            }

            // Converting to a "wrong" ideo during a directed attempt is half as likely - a rare lol moment.
            var mult = priorityIdeo != null && priorityIdeo != ideo ? 0.5f : 1f;
            candidates.Add((ideo, (opinion - current) / opinion * mult, (opinion - current) * mult));
        }

        AddCrisisCandidate(candidates, current, crisisWeight => crisisWeight / EnhancedBeliefsMod.Settings.CrisisThreshold);

        var index = SelectWeightedConversion(candidates);
        if (index < 0)
        {
            return ConversionOutcome.Failure;
        }

        var chosen = candidates[index].ideo;
        if (chosen == null)
        {
            if (noBreakdown)
            {
                return ConversionOutcome.Failure;
            }

            _ = Pawn.mindState.mentalStateHandler.TryStartMentalState(EnhancedBeliefsDefOf.IdeoChange);
            return ConversionOutcome.Breakdown;
        }

        ApplyConversion(chosen);
        return ConversionOutcome.Success;
    }

    // Continuous, spontaneous conversion integrated over elapsed time. Each candidate's chance is its
    // ConversionProbability treated as a hazard over ConversionInterval days, so it is invariant to how
    // finely time is sampled - tick batching never silently sets the conversion rate. Called from the tick.
    public void TryBackgroundConversion(float deltaDays)
    {
        if (deltaDays <= 0f || !ModLister.CheckIdeology("Ideoligion conversion")
            || Pawn.DevelopmentalStage.Baby() || Find.IdeoManager.classicMode)
        {
            return;
        }

        var interval = EnhancedBeliefsMod.Settings.ConversionInterval;
        var current = IdeoOpinion(Pawn.Ideo);
        var candidates = new List<(Ideo? ideo, float chance, float weight)>();

        foreach (var ideo in Find.IdeoManager.IdeosListForReading)
        {
            if (ideo == Pawn.Ideo)
            {
                continue;
            }

            var opinion = IdeoOpinion(ideo);
            if (opinion <= current)
            {
                continue;
            }

            var chance = HazardConversionChance((opinion - current) / opinion, deltaDays, interval);
            candidates.Add((ideo, chance, opinion - current));
        }

        AddCrisisCandidate(candidates, current,
            crisisWeight => HazardConversionChance(crisisWeight / EnhancedBeliefsMod.Settings.CrisisThreshold, deltaDays, interval));

        // CertaintyLossFactor scales the conversion/breakdown hazard: a resistant pawn (factor < 1) clings
        // to their faith, a fragile one (factor > 1) drifts away faster. Only the spontaneous path applies
        // it here - the acute-event callers already fold CertaintyLossFactor into the certainty they shed.
        var index = SelectWeightedConversion(candidates, Pawn.GetStatValue(StatDefOf.CertaintyLossFactor));
        if (index < 0)
        {
            return;
        }

        var chosen = candidates[index].ideo;
        if (chosen == null)
        {
            _ = Pawn.mindState.mentalStateHandler.TryStartMentalState(EnhancedBeliefsDefOf.IdeoChange);
            return;
        }

        ApplyConversion(chosen);
    }

    // Adds the crisis-of-faith pseudo-candidate (a null ideo) when the pawn now prefers doubt to their own
    // faith, i.e. their conviction has fallen below the crisis threshold. It competes in the same draw as the
    // real ideos with the same gap-based weight; only its chance differs between the one-shot and hazard paths.
    private static void AddCrisisCandidate(List<(Ideo? ideo, float chance, float weight)> candidates, float current, Func<float, float> chanceOf)
    {
        var crisisThreshold = EnhancedBeliefsMod.Settings.CrisisThreshold;
        if (current >= crisisThreshold)
        {
            return;
        }

        var weight = crisisThreshold - current;
        candidates.Add((null, chanceOf(weight), weight));
    }

    // Weighted conversion draw. First rolls the competing-risks probability that *any* candidate fires
    // (1 minus the product of each candidate's survival), then, if it does, picks which one proportional to
    // its weight. Splitting "whether" (chance) from "which" (weight) lets the target stay discriminating by
    // opinion gap even when certainty is near zero, where the ratio-based chances all saturate toward 1.
    // Returns the chosen candidate's index, or -1 if nothing fired. chanceFactor is a plain multiplier on the
    // probability that something fires (clamped to [0,1]) - CertaintyLossFactor is a linear factor, so a
    // volatile pawn (x3) is ~3x as likely to convert, not driven toward certainty like an exponent would.
    private static int SelectWeightedConversion(List<(Ideo? ideo, float chance, float weight)> candidates, float chanceFactor = 1f)
    {
        float survival = 1f;
        foreach (var candidate in candidates)
        {
            survival *= 1f - candidate.chance;
        }

        var fireChance = Mathf.Clamp01((1f - survival) * chanceFactor);

        if (Rand.Value >= fireChance)
        {
            return -1;
        }

        float totalWeight = 0f;
        foreach (var candidate in candidates)
        {
            totalWeight += candidate.weight;
        }

        if (totalWeight <= 0f)
        {
            return -1;
        }

        var roll = Rand.Value * totalWeight;
        for (var ii = 0; ii < candidates.Count; ii++)
        {
            roll -= candidates[ii].weight;
            if (roll <= 0f)
            {
                return ii;
            }
        }

        return candidates.Count - 1;
    }

    // A per-window probability p, integrated over deltaDays as a hazard with the given interval.
    // Survival is multiplicative in time, so sampling the same span more finely yields the same total
    // probability - the roll cadence cannot silently change the conversion rate.
    public static float HazardConversionChance(float p, float deltaDays, float intervalDays)
    {
        return 1f - Mathf.Pow(1f - p, deltaDays / intervalDays);
    }

    // Performs the actual conversion to newIdeo. Side-effectful by nature: swaps the pawn's ideo, reseeds
    // certainty from opinion, preserves the old ideo's standing as personal opinion, records history, recaches.
    private void ApplyConversion(Ideo newIdeo)
    {
        var oldCertainty = Pawn.ideo.Certainty;
        var oldIdeo = Pawn.Ideo;
        var oldIdeoContains = Pawn.ideo.PreviousIdeos.Contains(newIdeo);

        // How drawn the pawn is to the new ideo, captured before SetIdeo - afterwards the own-ideo
        // short-circuit would report raw certainty instead. A convert arrives believing as strongly as
        // they preferred it, so they can't immediately be out-preferred by an ideo they just rejected.
        var newCertainty = IdeoOpinion(newIdeo);

        Pawn.ideo.SetIdeo(newIdeo);
        newIdeo.Notify_MemberGainedByConversion();

        Pawn.ideo.Certainty = newCertainty;
        personalIdeoOpinions[newIdeo] = 0;

        // Keep current opinion of our old ideo by moving difference between new base and old base (certainty) into personal thoughts
        var oldBase = DetailedIdeoOpinion(oldIdeo).BaseOpinion;
        AdjustPersonalOpinion(oldIdeo, oldCertainty - oldBase);

        if (!oldIdeoContains)
        {
            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ConvertedNewMember, Pawn.Named(HistoryEventArgsNames.Doer), newIdeo.Named(HistoryEventArgsNames.Ideo)));
        }

        RecacheAllBaseOpinions();
    }

    public bool OverrideConversionAttempt(float certaintyReduction, Ideo newIdeo, bool applyCertaintyFactor = true)
    {
        EnhancedBeliefsMod.Debug($"OverrideConversionAttempt called: pawn={Pawn}, certaintyReduction={certaintyReduction}, newIdeo={newIdeo}, applyCertaintyFactor={applyCertaintyFactor}");
        if (Find.IdeoManager.classicMode || Pawn.ideo == null || Pawn.DevelopmentalStage.Baby())
        {
            EnhancedBeliefsMod.Debug("OverrideConversionAttempt: classicMode, no ideo, or baby. Returning false.");
            return false;
        }

        var oldCertainty = Pawn.ideo.Certainty;
        var newCertainty = Mathf.Clamp01(Pawn.ideo.Certainty + (applyCertaintyFactor ? Pawn.ideo.ApplyCertaintyChangeFactor(0f - certaintyReduction) : (0f - certaintyReduction)));
        EnhancedBeliefsMod.Debug($"OverrideConversionAttempt: oldCertainty={oldCertainty}, newCertainty={newCertainty}");

        EnhancedBeliefsUtilities.ShowCertaintyChangeMote(Pawn, oldCertainty, newCertainty);

        if (newIdeo != null && newIdeo != Pawn.Ideo)
        {
            AdjustPersonalOpinion(newIdeo, certaintyReduction * 2f);
            EnhancedBeliefsMod.Debug($"OverrideConversionAttempt: Adjusted opinion of {newIdeo} by {certaintyReduction * 2f}");
        }

        var ideoOpinion = PersonalIdeoOpinion(Pawn.Ideo, out var _);
        EnhancedBeliefsMod.Debug($"OverrideConversionAttempt: ideoOpinion={ideoOpinion}");
        if (ideoOpinion > 0)
        {
            var adj = Math.Max(ideoOpinion * -0.01f, -0.25f * certaintyReduction);
            EnhancedBeliefsMod.Debug($"OverrideConversionAttempt: Adjusting personal opinion by {adj}");
            AdjustPersonalOpinion(Pawn.Ideo, adj);
        }

        Pawn.ideo.Certainty = newCertainty;
        var conversionResult = CheckConversion(newIdeo);
        EnhancedBeliefsMod.Debug($"OverrideConversionAttempt: conversionResult={conversionResult}");
        return conversionResult == ConversionOutcome.Success;
    }
}

internal readonly struct DetailedIdeoOpinion(float baseOpinion, float personalOpinion, float relationshipOpinion, string? devModeDetails = null)
{
    public readonly float BaseOpinion => baseOpinion;
    public readonly float PersonalOpinion => personalOpinion;
    public readonly float RelationshipOpinion => relationshipOpinion;
    public readonly string? DevModeDetails => devModeDetails;
}
