using System.Diagnostics;
using System.Text;

namespace EnhancedIdeology;

[HotSwappable]
internal sealed class IdeoTrackerData(Pawn pawn) : IExposable
{
    public const float PawnOpinionFactor = 0.02f;

    private static readonly Stopwatch _profSw = new();
    private static int _profCalls;
    private static long _profStructural, _profRelational, _profPractitional;
    private static long _profMemes, _profTraits, _profDietXeno, _profInduced, _profPerIssue, _profUniversal;
    private const int ProfLogInterval = 500;

    private Pawn pawn = pawn;
    public Pawn Pawn => pawn;
    internal IssueStanceTracker Stances { get; private set; } = new IssueStanceTracker(pawn);
    public void ForceNewPawn(Pawn newPawn)
    {
        pawn = newPawn;
        Stances.SetPawn(newPawn);
    }

    public float CachedCertaintyChange { get; private set; } = -9999f;

    // Setpoint (target certainty) and its bands, all in certainty fraction (0-1), refreshed by CertaintyChangeRecache.
    public float CachedTargetCertainty { get; private set; }
    public float CachedStructural { get; private set; }
    public float CachedRelational { get; private set; }
    public float CachedPractitional { get; private set; }

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

    // Extended certainty: uncapped version of Pawn.ideo.Certainty. Vanilla certainty is always
    // Clamp01(ExtendedCertainty). Initialized lazily from vanilla if not yet set (old saves, fresh trackers).
    private float _extendedCertainty = -1f;
    public float ExtendedCertainty
    {
        get
        {
            if (_extendedCertainty < 0f)
                _extendedCertainty = Pawn.ideo.Certainty;
            return _extendedCertainty;
        }
    }

    // Set extended certainty to an absolute value (no upper cap) and sync vanilla to Clamp01.
    public void SetExtendedCertainty(float value)
    {
        _extendedCertainty = Mathf.Max(0f, value);
        Pawn.ideo.Certainty = Mathf.Clamp01(_extendedCertainty);
    }

    // Advance extended certainty by CachedCertaintyChange * deltaDays; sync vanilla = Clamp01(extended).
    // Detects external resets (crisis, knock, book burn) by comparing vanilla against Clamp01(extended):
    // if vanilla is lower than expected, something outside our tick reduced it — snap extended down to match.
    internal void AdvanceExtendedCertainty(float deltaDays)
    {
        var expectedVanilla = Mathf.Clamp01(ExtendedCertainty);
        if (Pawn.ideo.Certainty < expectedVanilla - 0.001f)
            _extendedCertainty = Pawn.ideo.Certainty;
        _extendedCertainty = Mathf.Max(0f, ExtendedCertainty + CachedCertaintyChange * deltaDays);
        Pawn.ideo.Certainty = Mathf.Clamp01(_extendedCertainty);
    }

    // A pawn spawns at equilibrium: the first setpoint computed for them seeds their certainty to it, so the
    // target sits exactly on the bar until an event pushes belief off it. Defaults true on load so existing
    // pawns keep their played-in certainty.
    private bool certaintyInitialized;

    // Transient: set when stances are seeded for a pawn that already has a played-in certainty (vanilla or old
    // EB save). On the next recache, issueStrength values are scaled so the structural band lands on
    // calibrationTargetCertainty rather than wherever random seeding happened to place it.
    private bool needsStanceCalibration;
    private float calibrationTargetCertainty;

    // Called for pawns whose saves predate EB's issue-stance model (vanilla or old EB). Marks certainty as
    // already initialized (no snap) and schedules one calibration pass to align the structural setpoint with
    // the certainty the pawn already has.
    internal void MarkAsLoadedWithoutData(float existingCertainty)
    {
        certaintyInitialized = true;
        needsStanceCalibration = true;
        calibrationTargetCertainty = existingCertainty;
        _extendedCertainty = existingCertainty;
    }

    private readonly Dictionary<Ideo, float> cachedRelationshipIdeoOpinions = [];
    private readonly Dictionary<Pawn, float> cachedRelationships = [];

    private Dictionary<MemeDef, float> memeOpinions = [];

    private List<Ideo>? cache1;
    private List<Ideo>? cache2;
    private List<MemeDef>? cache3;
    private List<float>? cache5;
    private List<float>? cache6;
    private List<float>? cache7;

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
    // practitional (current precept moods).
    // No hard upper cap: certainty can exceed 1 when structural fit is very strong.
    public void CertaintyChangeRecache(GameComponent_EnhancedIdeology comp)
    {
        // Sync extended certainty with any external write to vanilla (tests, reassure, book, entrench, etc.).
        // AdvanceExtendedCertainty handles the tick path; this catches same-tick reads like the conversion check.
        var expectedVanilla = Mathf.Clamp01(ExtendedCertainty);
        if (Mathf.Abs(Pawn.ideo.Certainty - expectedVanilla) > 0.001f)
            _extendedCertainty = Pawn.ideo.Certainty;

        var settings = EnhancedIdeologyMod.Settings;

        StructuralContributors.Clear();
        RelationalContributors.Clear();
        PractitionalContributors.Clear();

        var profiling = Prefs.DevMode;
        if (profiling) _profSw.Restart();

        // Structural band: innate fit of the pawn to their own ideo, from their per-issue precept stances.
        var structural = StructuralOpinionOf(Pawn.Ideo!, StructuralContributors) / 100f;
        CachedStructural = structural;

        if (profiling) { _profStructural += _profSw.ElapsedTicks; _profSw.Restart(); }

        // Relational band: mean opinion of co-religionists, scaled by the user's max range.
        CachedRelational = RelationalBand(settings.RelationalMaxRange, RelationalContributors, comp);

        if (profiling) { _profRelational += _profSw.ElapsedTicks; _profSw.Restart(); }

        // Practitional band: summed precept-thought mood, scaled by the user's max range.
        CachedPractitional = PractitionalBand(settings.PracticeMaxRange, PractitionalContributors);

        if (profiling)
        {
            _profPractitional += _profSw.ElapsedTicks;
            if (++_profCalls % ProfLogInterval == 0)
                LogRecacheProfile();
        }

        if (needsStanceCalibration)
        {
            needsStanceCalibration = false;
            CalibrateStancesToCertainty(structural);
            StructuralContributors.Clear();
            structural = StructuralOpinionOf(Pawn.Ideo!, StructuralContributors) / 100f;
            CachedStructural = structural;
        }

        var target = Mathf.Max(0f, structural + CachedRelational + CachedPractitional);
        CachedTargetCertainty = target;

        // Seed a fresh pawn's certainty to their net setpoint the first time it is known - relationships and
        // practices aren't "new" at spawn, so the pawn starts where those bands already place them.
        if (!certaintyInitialized)
        {
            certaintyInitialized = true;
            _extendedCertainty = target;
            Pawn.ideo.Certainty = Mathf.Clamp01(target);
        }

        CachedCertaintyChange = settings.CertaintyDriftRate * (target - ExtendedCertainty);
    }

    private static void LogRecacheProfile()
    {
        static string Ms(long ticks) => $"{ticks * 1000.0 / (Stopwatch.Frequency * ProfLogInterval):F3}ms";
        var structural = _profMemes + _profTraits + _profDietXeno + _profInduced + _profPerIssue + _profUniversal;
        Log.Message(
            $"[EI Recache/{ProfLogInterval} calls] " +
            $"structural={Ms(_profStructural)} (memes={Ms(_profMemes)} traits={Ms(_profTraits)} " +
            $"dietXeno={Ms(_profDietXeno)} induced={Ms(_profInduced)} perIssue={Ms(_profPerIssue)} " +
            $"universal={Ms(_profUniversal)} overhead={Ms(_profStructural - structural)}) " +
            $"relational={Ms(_profRelational)} practitional={Ms(_profPractitional)}");
        _profCalls = 0;
        _profStructural = _profRelational = _profPractitional = 0;
        _profMemes = _profTraits = _profDietXeno = _profInduced = _profPerIssue = _profUniversal = 0;
    }

    private float RelationalBand(float maxRange, List<(string label, float pct)> contributors, GameComponent_EnhancedIdeology comp)
    {
        CacheRelationshipIdeoOpinion(Pawn.Ideo!, comp);

        float sum = 0;
        int count = 0;
        foreach (var (_, opinion) in GetOwnIdeoRelationships())
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
            foreach (var (relPawn, opinion) in GetOwnIdeoRelationships())
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
        if (!baseIdeoOpinions.ContainsKey(ideo) || !personalIdeoOpinions.ContainsKey(ideo))
        {
            baseIdeoOpinions[ideo] = StructuralIdeoOpinion(ideo);
            personalIdeoOpinions[ideo] = 0;
        }

        if (ideo == Pawn.Ideo)
        {
            baseIdeoOpinions[ideo] = ExtendedCertainty * 100f;
        }

        return Mathf.Max(
            baseIdeoOpinions[ideo] +
            PersonalIdeoOpinion(ideo, out var _) +
            IdeoOpinionFromRelationships(ideo, false, out var _), 0) / 100f;
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
             ideo == Pawn.Ideo ? ExtendedCertainty : baseIdeoOpinions[ideo] / 100f,
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

    // Certainty-independent structural opinion (0-100) a pawn holds toward an ideo based on their traits,
    // memes and the ideo's precepts. Shared by StructuralIdeoOpinion and the certainty setpoint's structural band.
    // If contributors is supplied, each term is recorded (in certainty-fraction units) for the tooltip breakdown.
    private float StructuralOpinionOf(Ideo ideo, List<(string label, float pct)>? contributors = null)
    {
        var pawnIdeo = Pawn.Ideo!;
        var start = contributors?.Count ?? 0;
        float opinion = 0;
        var profiling = Prefs.DevMode;
        var structSw = profiling ? Stopwatch.StartNew() : null;

        // various global meme specific opinions
        var gestaltMeme = EnhancedIdeologyDefOf.VME_Gestalt;
        var nationalistMeme = EnhancedIdeologyDefOf.VME_Nationalist;
        var isolationistMeme = EnhancedIdeologyDefOf.VFEA_Isolationist;
        var violentConversionMeme = EnhancedIdeologyDefOf.VME_ViolentConversion;
        if (gestaltMeme != null && pawnIdeo.HasMeme(gestaltMeme))
        {
            opinion -= 30;
            contributors?.Add((gestaltMeme.LabelCap, -30f));
        }
        else if ((isolationistMeme != null && pawnIdeo.HasMeme(isolationistMeme))
            || (violentConversionMeme != null && pawnIdeo.HasMeme(violentConversionMeme)))
        {
            var label = (isolationistMeme != null && pawnIdeo.HasMeme(isolationistMeme))
                ? isolationistMeme.LabelCap
                : violentConversionMeme!.LabelCap;
            opinion -= 25;
            contributors?.Add((label, -25f));
        }
        else if (pawnIdeo.HasMeme(EnhancedIdeologyDefOf.Supremacist)
            || pawnIdeo.HasMeme(EnhancedIdeologyDefOf.Collectivist)
            || (nationalistMeme != null && pawnIdeo.HasMeme(nationalistMeme)))
        {
            var label = pawnIdeo.HasMeme(EnhancedIdeologyDefOf.Supremacist)
                ? EnhancedIdeologyDefOf.Supremacist.LabelCap
                : pawnIdeo.HasMeme(EnhancedIdeologyDefOf.Collectivist)
                    ? EnhancedIdeologyDefOf.Collectivist.LabelCap
                    : nationalistMeme!.LabelCap;
            opinion -= 20;
            contributors?.Add((label, -20f));
        }
        else if (pawnIdeo.HasMeme(EnhancedIdeologyDefOf.Loyalist))
        {
            opinion -= 10;
            contributors?.Add((EnhancedIdeologyDefOf.Loyalist.LabelCap, -10f));
        }
        else if (pawnIdeo.HasMeme(EnhancedIdeologyDefOf.Guilty))
        {
            opinion += 10;
            contributors?.Add((EnhancedIdeologyDefOf.Guilty.LabelCap, 10f));
        }

        // VME_Elders: elder pawns gain structural certainty in ideos that
        // venerate the old (max +20 at age 70+).
        var eldersMeme = EnhancedIdeologyDefOf.VME_Elders;
        if (eldersMeme != null && ideo.HasMeme(eldersMeme))
        {
            var ageFactor = Mathf.Clamp01(Mathf.InverseLerp(50f, 70f, Pawn.ageTracker.AgeBiologicalYearsFloat));
            if (ageFactor > 0f)
            {
                var eldersBonus = 20f * ageFactor;
                opinion += eldersBonus;
                contributors?.Add((eldersMeme.LabelCap, eldersBonus));
            }
        }

        // Cross-meme disagreements between opposing Mort's Ideologies memes.
        var empiricist = EnhancedIdeologyDefOf.MI_Empiricist;
        var faith = EnhancedIdeologyDefOf.MI_Faith;
        if (empiricist != null && faith != null
            && (pawnIdeo.HasMeme(empiricist) && ideo.HasMeme(faith)
                || pawnIdeo.HasMeme(faith) && ideo.HasMeme(empiricist)))
        {
            opinion -= 5;
            contributors?.Add((empiricist.LabelCap + " / " + faith.LabelCap, -5f));
        }
        var conservationist = EnhancedIdeologyDefOf.MI_Environmentalist;
        var polluter = EnhancedIdeologyDefOf.MI_Industrialist;
        if (conservationist != null && polluter != null
            && (pawnIdeo.HasMeme(conservationist) && ideo.HasMeme(polluter)
                || pawnIdeo.HasMeme(polluter) && ideo.HasMeme(conservationist)))
        {
            opinion -= 15;
            contributors?.Add((conservationist.LabelCap + " / " + polluter.LabelCap, -15f));
        }
        var govLiberty = EnhancedIdeologyDefOf.MI_GovernmentLiberty;
        var govAuthority = EnhancedIdeologyDefOf.MI_GovernmentAuthority;
        if (govLiberty != null && govAuthority != null
            && (pawnIdeo.HasMeme(govLiberty) && ideo.HasMeme(govAuthority)
                || pawnIdeo.HasMeme(govAuthority) && ideo.HasMeme(govLiberty)))
        {
            opinion -= 10;
            contributors?.Add((govLiberty.LabelCap + " / " + govAuthority.LabelCap, -10f));
        }
        var wealthEqual = EnhancedIdeologyDefOf.MI_WealthEquality;
        var wealthStrat = EnhancedIdeologyDefOf.MI_WealthStratification;
        if (wealthEqual != null && wealthStrat != null
            && (pawnIdeo.HasMeme(wealthEqual) && ideo.HasMeme(wealthStrat)
                || pawnIdeo.HasMeme(wealthStrat) && ideo.HasMeme(wealthEqual)))
        {
            opinion -= 10;
            contributors?.Add((wealthEqual.LabelCap + " / " + wealthStrat.LabelCap, -10f));
        }

        if (structSw != null) { _profMemes += structSw.ElapsedTicks; structSw.Restart(); }

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

        if (structSw != null) { _profTraits += structSw.ElapsedTicks; structSw.Restart(); }

        // Diet gene bonuses: obligate herbivores/carnivores feel a strong pull
        // toward ideos that share their dietary needs, and an extra pull toward
        // the Vegan meme when herbivorous (there's no corresponding carnivorous
        // meme, somehow).
        var meatEatingIssue = DefDatabase<IssueDef>.GetNamedSilentFail("MeatEating");
        if (meatEatingIssue != null)
        {
            if (IssueStanceTracker.PawnHasActiveGene(Pawn, EnhancedIdeologyDefOf.BS_Diet_Herbivore))
            {
                var ideoRank = IssueStanceTracker.HeldRank(ideo, meatEatingIssue);
                var abhorrentRank = PreceptLadder.RankOfName(meatEatingIssue, "MeatEating_Abhorrent");
                if (abhorrentRank >= 0 && ideoRank >= 0 && ideoRank <= abhorrentRank)
                {
                    opinion += 20;
                    contributors?.Add(("EnhancedIdeology.DietGeneAntiMeat".Translate(), 20f));
                }
                if (EnhancedIdeologyDefOf.VME_Vegan != null && ideo.HasMeme(EnhancedIdeologyDefOf.VME_Vegan))
                {
                    opinion += 15;
                    contributors?.Add((EnhancedIdeologyDefOf.VME_Vegan.LabelCap, 15f));
                }
            }
            else if (IssueStanceTracker.PawnHasActiveGene(Pawn, EnhancedIdeologyDefOf.BS_Diet_Carnivore))
            {
                var ideoRank = IssueStanceTracker.HeldRank(ideo, meatEatingIssue);
                var nonMeatDisapprovedRank = PreceptLadder.RankOfName(meatEatingIssue, "MeatEating_NonMeat_Disapproved");
                if (nonMeatDisapprovedRank >= 0 && ideoRank >= nonMeatDisapprovedRank)
                {
                    opinion += 20;
                    contributors?.Add(("EnhancedIdeology.DietGeneProMeat".Translate(), 20f));
                }
            }
        }

        // A pawn who is not a preferred xenotype for the target ideo faces a hard structural barrier:
        // no fixed trait is more personal than your own race being deemed lesser by a faith.
        if (ModsConfig.BiotechActive && Pawn.genes != null)
        {
            var preferredKeys = PreceptPolicy.PreferredXenotypeKeys(ideo);
            if (preferredKeys.Count > 0)
            {
                var pawnKey = Pawn.genes.UniqueXenotype
                    ? Pawn.genes.xenotypeName
                    : Pawn.genes.Xenotype?.defName;
                if (pawnKey != null && preferredKeys.Contains(pawnKey))
                {
                    opinion += 20;
                    contributors?.Add(("EnhancedIdeology.XenotypePreferred".Translate(), 20f));
                }
                else if (pawnKey != null)
                {
                    opinion -= 35;
                    contributors?.Add(("EnhancedIdeology.XenotypeDisapproved".Translate(), -35f));
                }
            }
        }

        if (structSw != null) { _profDietXeno += structSw.ElapsedTicks; structSw.Restart(); }

        // Structural precept fit: for each issue at least one of the two faiths takes a position on, how the
        // target ideo's stance compares to the pawn's own preferred stance, weighted by conviction, averaged
        // and scaled to 0-100 (R2). Issues neither faith holds are irrelevant - there is nothing to agree or
        // disagree about - so they are excluded rather than counted as (mutual don't-care) agreement.
        EnsureIssueStancesSeeded();
        var oppositionScale = EnhancedIdeologyMod.Settings.PreceptOppositionScale;
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
        if (structSw != null) { _profInduced += structSw.ElapsedTicks; structSw.Restart(); }
        foreach (var issue in relevantIssues)
        {
            var perIssue = PerIssueOpinion(ideo, issue!, inducedTargets, oppositionScale, out var graded);
            if (!graded)
            {
                continue;
            }

            preceptSum += perIssue;
            issueCount++;
            if (contributors != null && perIssue != 0f)
            {
                contributors.Add((issue!.LabelCap, perIssue));
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

        if (structSw != null) { _profPerIssue += structSw.ElapsedTicks; structSw.Restart(); }

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
        var couplingPenalty = PreceptPolicy.CouplingPenalty(pawnIdeo, ideo, issue => Stances.GetStrength(issue));
        if (couplingPenalty != 0f)
        {
            opinion -= couplingPenalty;
            contributors?.Add(("EnhancedIdeology.CouplingPenalty".Translate(), -couplingPenalty));
        }

        // Rescale the collected raw offsets into certainty-fraction units matching the band total.
        if (contributors != null)
        {
            for (var ii = start; ii < contributors.Count; ii++)
            {
                contributors[ii] = (contributors[ii].label, contributors[ii].pct / 100f);
            }
        }

        if (structSw != null) _profUniversal += structSw.ElapsedTicks;

        return Mathf.Max(opinion, 0);
    }

    // Raw per-issue opinion (roughly +/-strength) the pawn holds toward `ideo`'s stance on `issue`: the same
    // ladder-distance / special-payload grade the structural band averages, before the /issueCount mean.
    // Moral issues (and coupled targets) grade by rung distance; Special issues carry bespoke categorical logic.
    // `graded` is false when neither faith takes a comparable position, so the caller leaves it out of the mean.
    private float PerIssueOpinion(Ideo ideo, IssueDef issue, HashSet<IssueDef> inducedTargets, float oppositionScale, out bool graded)
    {
        graded = true;
        var category = PreceptPolicy.CategoryOf(issue);
        if (category == PreceptCategory.Moral || inducedTargets.Contains(issue))
        {
            var pawnRank = Stances.GetRank(issue);
            var targetRank = IssueStanceTracker.HeldRank(ideo, issue);
            // Widen the extent to any induced rank sitting past the ladder ends, so a "beyond Don't-care"
            // stance reads as the axis extreme rather than falling outside it.
            var minRank = Mathf.Min(Mathf.Min(0f, PreceptLadder.DontCareRank(issue)), Mathf.Min(pawnRank, targetRank));
            var maxRank = Mathf.Max(PreceptLadder.Rungs(issue).Count - 1, Mathf.Max(pawnRank, targetRank));
            return PreceptLadder.OpinionOnPrecept(
                pawnRank, targetRank, minRank, maxRank, Stances.GetStrength(issue), oppositionScale);
        }

        // Weapons / PreferredXenotypes compare precept payloads directly; leader / mood are rank-based.
        // Either resolver returning false means the two faiths have no stance to compare.
        if (category == PreceptCategory.Special
            && (PreceptPolicy.TryPayloadSpecialOpinion(issue, Pawn.Ideo!, ideo, Stances.GetStrength(issue), out var special)
                || PreceptPolicy.TrySpecialOpinion(
                    issue, Stances.GetRank(issue), IssueStanceTracker.HeldRank(ideo, issue), Stances.GetStrength(issue), oppositionScale, out special)))
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
            PreceptPolicy.InducedIssues(Pawn.Ideo!).Concat(PreceptPolicy.InducedIssues(ideo)));
        return PerIssueOpinion(ideo, issue, inducedTargets, EnhancedIdeologyMod.Settings.PreceptOppositionScale, out _);
    }

    // The issue on which the pawn's stance most opposes `ideo` (the most negative per-issue opinion) - the belief
    // a preacher of `ideo` would target first when trying to convert this pawn. Returns null when the pawn opposes
    // nothing `ideo` preaches (every graded issue reads >= 0).
    public IssueDef? MostOpposingIssue(Ideo ideo)
    {
        var issues = MostOpposingIssues(ideo, 1);
        return issues.Count > 0 ? issues[0] : null;
    }

    // The `n` issues the pawn's stance most opposes about `ideo`, most-opposed first, dropping any that read
    // >= 0 (nothing to argue). Fewer than `n` are returned when the pawn opposes fewer than `n` of the ideo's
    // stances. The convert ability targets this bundle; conversion targets just the first.
    // Fallback when the pawn already agrees with everything: the `n` weakest agreements (lowest opinion),
    // which are the most persuadable even without active opposition.
    // If guideTracker is supplied, issues where the pawn is already near the target rank but the guide holds
    // weaker conviction are excluded — a win there would only drain the pawn's belief strength.
    public IReadOnlyList<IssueDef> MostOpposingIssues(Ideo ideo, int n, IdeoTrackerData? guideTracker = null)
    {
        EnsureIssueStancesSeeded();
        var inducedTargets = new HashSet<IssueDef>(
            PreceptPolicy.InducedIssues(Pawn.Ideo!).Concat(PreceptPolicy.InducedIssues(ideo)));
        var oppositionScale = EnhancedIdeologyMod.Settings.PreceptOppositionScale;

        var scored = ideo.precepts.Select(precept => precept.def.issue)
            .Where(issue => issue != null).Distinct()
            .Select(issue => (issue: issue!, opinion: PerIssueOpinion(ideo, issue!, inducedTargets, oppositionScale, out var graded), graded))
            .Where(entry => entry.graded)
            .ToList();

        var opposing = scored
            .Where(entry => entry.opinion < 0f && WorthTargeting(entry.issue, IssueStanceTracker.HeldRank(ideo, entry.issue), guideTracker))
            .OrderBy(entry => entry.opinion)
            .Take(n)
            .Select(entry => entry.issue)
            .ToList();

        if (opposing.Count > 0) return opposing;

        return scored
            .Where(entry => WorthTargeting(entry.issue, IssueStanceTracker.HeldRank(ideo, entry.issue), guideTracker))
            .OrderBy(entry => entry.opinion)
            .Take(n)
            .Select(entry => entry.issue)
            .ToList();
    }

    // True when this issue is worth targeting in a debate: either the rank gap is large enough to close, or
    // the guide holds stronger conviction so a win raises rather than drains the pawn's belief strength.
    private bool WorthTargeting(IssueDef issue, float targetRank, IdeoTrackerData? guideTracker)
    {
        if (guideTracker == null) return true;
        if (Mathf.Abs(Stances.GetRank(issue) - targetRank) > InteractionWorker_IdeologicalDebatePrecept.DebateRankEpsilon) return true;
        var guideStrength = guideTracker.IssueStances().First(s => s.issue == issue).strength;
        return guideStrength > Stances.GetStrength(issue);
    }

    // Dev-only: the full extent/rank breakdown behind IssueOpinionToward, for diagnosing per-issue opinions.
    public string IssueOpinionDebug(Ideo ideo, IssueDef issue)
    {
        EnsureIssueStancesSeeded();
        var pawnRank = Stances.GetRank(issue);
        var targetRank = IssueStanceTracker.HeldRank(ideo, issue);
        var rungCount = PreceptLadder.Rungs(issue).Count;
        var dontCare = PreceptLadder.DontCareRank(issue);
        var strength = Stances.GetStrength(issue);
        var minRank = Mathf.Min(Mathf.Min(0f, dontCare), Mathf.Min(pawnRank, targetRank));
        var maxRank = Mathf.Max(rungCount - 1, Mathf.Max(pawnRank, targetRank));
        var maxDist = Mathf.Max(pawnRank - minRank, maxRank - pawnRank);
        var t = maxDist > 0f ? Mathf.Abs(targetRank - pawnRank) / maxDist : 0f;
        var oppositionScale = EnhancedIdeologyMod.Settings.PreceptOppositionScale;
        var falloff = 1f - (t * (1f + oppositionScale));
        var ladder = string.Join(", ", PreceptLadder.Rungs(issue).Select((precept, ix) => $"{ix}:{precept.defName}"));
        return $"str={strength:F1} cat={PreceptPolicy.CategoryOf(issue)}"
            + $"\n  pawn={pawnRank:F2} target={targetRank:F2} rungs={rungCount} dontCare={dontCare:F2}"
            + $"\n  extent=[{minRank:F2},{maxRank:F2}] maxDist={maxDist:F2} t={t:F2}"
            + $"\n  oppScale={oppositionScale:F2} falloff={falloff:F2}"
            + $"\n  ladder: {ladder}";
    }

    // The `n` issues where the pawn's personal stance most diverges from their own ideo's orthodox rungs,
    // most-divergent first, dropping any within DebateRankEpsilon of orthodoxy. Reassure targets this bundle.
    // Fallback when already fully orthodox: the `n` weakest-held beliefs (lowest issueStrength), which are
    // the most susceptible to drift and still worth reinforcing.
    // If guideTracker is supplied, the fallback filters to issues where the guide holds stronger conviction —
    // a win on an already-orthodox issue with weaker guide conviction would only drain belief strength.
    public IReadOnlyList<IssueDef> MostHeterodoxIssues(int n, IdeoTrackerData? guideTracker = null)
    {
        EnsureIssueStancesSeeded();

        var ideoIssues = Pawn.Ideo!.precepts.Select(precept => precept.def.issue)
            .Where(issue => issue != null).Distinct()
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
            .Where(issue => WorthTargeting(issue, IssueStanceTracker.HeldRank(Pawn.Ideo, issue), guideTracker))
            .OrderBy(issue => Stances.GetStrength(issue))
            .Take(n)
            .ToList();
    }

    // Flat opinion bonus (0-100 units) for a UniversalPositive issue the target ideo values, e.g. Charity.
    private const float UniversalPositiveBonus = 5f;

    private void EnsureIssueStancesSeeded()
    {
        if (Stances.EnsureSeeded(certaintyInitialized))
        {
            needsStanceCalibration = true;
            calibrationTargetCertainty = Pawn.ideo.Certainty;
        }
    }

    // Scale all strengths so the structural band lands on calibrationTargetCertainty.
    // naturalStructural is the already-computed pre-scale structural value.
    private void CalibrateStancesToCertainty(float naturalStructural)
    {
        if (naturalStructural <= 0f) return;
        var minCertainty = EnhancedIdeologyMod.Settings.SaveCompatMinCertainty;
        var targetCertainty = Mathf.Max(calibrationTargetCertainty, minCertainty);
        var targetStructural = Mathf.Clamp01(targetCertainty - CachedRelational - CachedPractitional);
        Stances.ScaleStrengths(targetStructural / naturalStructural);
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
        baseOpinionsDirty = true;
    }

    // Set the pawn's stance on `issue` to an absolute (rank, strength). The conviction-valley debate
    // write-path (PullStance) computes the whole new stance at once, since rank and strength are coupled
    // along the curve — unlike ShiftIssueStance's independent deltas.
    public void SetIssueStance(IssueDef issue, float rank, float strength)
    {
        EnsureIssueStancesSeeded();
        Stances.SetStance(issue, rank, strength);
        baseOpinionsDirty = true;
    }

    // Reset stances after a brainwipe. Strength floors at max(0, 3 + traitOffset/3).
    public void ApplyBrainwipe()
    {
        EnsureIssueStancesSeeded();
        Stances.ApplyBrainwipe();
        SetExtendedCertainty(0f);
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
        return Stances.IssueStances();
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

        // Makes sure personal opinions cannot drive total below zero
        var curOpinion = Mathf.Max(baseIdeoOpinion + opinion, 0);
        if (personalIdeoOpinions[ideo] < -curOpinion)
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
    public void CacheRelationshipIdeoOpinion(Ideo ideo, GameComponent_EnhancedIdeology? comp = null)
    {
        float opinion = 0;
        comp ??= Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var pawns = comp.GetIdeoPawns(ideo);

        foreach (var otherPawn in pawns)
        {
            if (!SameLocalGroup(otherPawn))
                continue;
            // Up to +-2 opinion per pawn
            float pawnOpinion = Pawn.relations.OpinionOf(otherPawn);
            opinion += pawnOpinion * PawnOpinionFactor;
            cachedRelationships[otherPawn] = pawnOpinion;
        }

        cachedRelationshipIdeoOpinions[ideo] = opinion;
    }

    public IEnumerable<(Pawn pawn, float opinion)> GetOwnIdeoRelationships()
    {
        var ideo = Pawn.Ideo!;
        foreach (var kvp in cachedRelationships)
        {
            if (kvp.Key != Pawn && kvp.Key.Ideo == ideo)
                yield return (kvp.Key, kvp.Value);
        }
    }

    // Mirrors the map/caravan check in SocialCardUtility.PawnsForSocialInfo.
    private bool SameLocalGroup(Pawn other)
    {
        if (Pawn.MapHeld != null)
            return other.MapHeld == Pawn.MapHeld;
        var caravan = Pawn.GetCaravan();
        return caravan != null && other.GetCaravan() == caravan;
    }

    // Drain all issue conviction strengths uniformly by the global decay rate, once per game day.
    internal void ApplyConvictionDecayIfNewDay()
    {
        EnsureIssueStancesSeeded();
        if (Stances.ApplyDecayIfNewDay())
            baseOpinionsDirty = true;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref pawn, "pawn");
        // Default true: a save without this key predates spawn-seeding, so its pawns are already "initialized"
        // and must not have their played-in certainty overwritten on load.
        Scribe_Values.Look(ref certaintyInitialized, "certaintyInitialized", defaultValue: true);
        // Default -1: signals "uninitialized" on load from an old save; lazy-initialized to vanilla Certainty.
        Scribe_Values.Look(ref _extendedCertainty, "extendedCertainty", defaultValue: -1f);
        Scribe_Collections.Look(ref baseIdeoOpinions, "baseIdeoOpinions", LookMode.Reference, LookMode.Value, ref cache1, ref cache5);
        Scribe_Collections.Look(ref personalIdeoOpinions, "personalIdeoOpinions", LookMode.Reference, LookMode.Value, ref cache2, ref cache6);
        Scribe_Collections.Look(ref memeOpinions, "memeOpinions", LookMode.Def, LookMode.Value, ref cache3, ref cache7);
        Stances.ExposeData();

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
            if (Pawn == null) return;
            comp.SetIdeo(Pawn, Pawn.Ideo!);
        }
    }

    // Change pawn's personal opinion of another ideo, usually positively
    public void AdjustPersonalOpinion(Ideo ideo, float power)
    {
        EnhancedIdeologyMod.Debug($"AdjustPersonalOpinion called: pawn={Pawn}, ideo={ideo}, power={power}");
        if (!baseIdeoOpinions.ContainsKey(ideo) || !personalIdeoOpinions.ContainsKey(ideo))
        {
            EnhancedIdeologyMod.Debug("AdjustPersonalOpinion: Initializing base/personal opinions.");
            baseIdeoOpinions[ideo] = StructuralIdeoOpinion(ideo);
            personalIdeoOpinions[ideo] = 0;
        }

        personalIdeoOpinions[ideo] += power * 100f;
        EnhancedIdeologyMod.Debug($"AdjustPersonalOpinion: new personalIdeoOpinion={personalIdeoOpinions[ideo]}");
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
        var current = IdeoOpinion(Pawn.Ideo!);
        return opinion > current ? (opinion - current) / opinion : 0f;
    }

    // Read-only preview of the conversion chance toward `target` after a won attempt applies its certainty knock
    // (Certainty *= knock), used by the convert-ability tooltip. Folds in the knock but not the stance pull, and
    // reduces to CheckConversion's single-candidate ratio - it ignores competing ideos and the crisis candidate,
    // so it is an estimate, not the exact draw.
    public float ConversionChanceAfterKnock(Ideo target, float knock)
    {
        var opinion = IdeoOpinion(target);
        var current = IdeoOpinion(Pawn.Ideo!) * knock;
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

        // Use the higher of lived certainty and structural alignment as the bar: a pawn whose conviction
        // hasn't caught up to their structural fit yet shouldn't be treated as a conversion target.
        var current = Mathf.Max(IdeoOpinion(Pawn.Ideo!), CachedStructural);
        var candidates = new List<(Ideo? ideo, float chance, float weight)>();

        IEnumerable<Ideo> pool = whitelistIdeos
            ?? (priorityIdeo != null ? [priorityIdeo] : Find.IdeoManager.IdeosListForReading);
        foreach (var ideo in pool)
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

            candidates.Add((ideo, (opinion - current) / opinion, opinion - current));
        }

        var crisisThreshold = EffectiveCrisisThreshold();
        AddCrisisCandidate(candidates, current, crisisThreshold, crisisWeight => crisisWeight / crisisThreshold);

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

            TriggerCrisisOfFaith();
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

        var interval = EnhancedIdeologyMod.Settings.ConversionInterval;
        // Use the higher of lived certainty and structural alignment as the bar: a pawn whose conviction
        // hasn't caught up to their structural fit yet shouldn't be treated as a conversion target.
        var current = Mathf.Max(IdeoOpinion(Pawn.Ideo!), CachedStructural);
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

        var crisisThreshold = EffectiveCrisisThreshold();
        AddCrisisCandidate(candidates, current, crisisThreshold,
            crisisWeight => HazardConversionChance(crisisWeight / crisisThreshold, deltaDays, interval));

        // CertaintyLossFactor scales the conversion/breakdown hazard: a resistant pawn (factor < 1) clings
        // to their faith, a fragile one (factor > 1) drifts away faster. Certainty itself also damps the
        // rate: a fully certain pawn (certainty >= 1) has no spontaneous drift; an uncertain one is
        // proportionally more vulnerable. An ideo that condemns apostacy further resists spontaneous
        // drift - same scaling as PullStance (0.25x at Abhorrent). Only the spontaneous path applies
        // these - acute-event callers already fold CertaintyLossFactor into the certainty they shed.
        var certaintyResistance = Mathf.Clamp01(1f - Mathf.Clamp01(ExtendedCertainty));
        var apostacyResistance = 1f - (EnhancedIdeologyUtilities.ApostacyStrictness(Pawn.Ideo) * 0.75f);
        var index = SelectWeightedConversion(candidates, Pawn.GetStatValue(StatDefOf.CertaintyLossFactor) * certaintyResistance * apostacyResistance);
        if (index < 0)
        {
            return;
        }

        var chosen = candidates[index].ideo;
        if (chosen == null)
        {
            TriggerCrisisOfFaith();
            return;
        }

        ApplyConversion(chosen);
    }

    // When the crisis-of-faith candidate wins: if the pawn's mood is already below their minor break threshold,
    // the crisis collapses into a normal mood break (their spiritual doubt compounds existing misery rather than
    // resolving it). Otherwise, we find the ideo they most prefer, convert them to it if it outrates their
    // current certainty, then land their certainty at 1.5x the crisis threshold - above the danger zone but
    // still fragile. The pawn then enters the crisis-of-faith wander state.
    internal void TriggerCrisisOfFaith()
    {
        var mood = Pawn.needs.mood;
        if (mood != null && mood.CurLevel < Pawn.mindState.mentalBreaker.BreakThresholdMinor)
        {
            Find.PlayLog.Add(new PlayLogEntry_CrisisOfFaith(Pawn, CrisisOutcome.MoodBreak));
            Pawn.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak();
            return;
        }

        var crisisThreshold = EffectiveCrisisThreshold();
        var currentOpinion = IdeoOpinion(Pawn.Ideo!);

        var bestIdeo = Find.IdeoManager.IdeosListForReading
            .Where(ii => ii != Pawn.Ideo)
            .MaxByWithFallback(ii => IdeoOpinion(ii!));

        if (bestIdeo != null && IdeoOpinion(bestIdeo) > currentOpinion)
        {
            ApplyConversion(bestIdeo);
        }

        SetExtendedCertainty(1.5f * crisisThreshold);

        Find.PlayLog.Add(new PlayLogEntry_CrisisOfFaith(Pawn, CrisisOutcome.Wander));

        _ = Pawn.mindState.mentalStateHandler.TryStartMentalState(EnhancedIdeologyDefOf.EB_CrisisOfFaith);
    }

    // Adds the crisis-of-faith pseudo-candidate (a null ideo) when the pawn now prefers doubt to their own
    // faith, i.e. their conviction has fallen below the crisis threshold. It competes in the same draw as the
    // real ideos with the same gap-based weight; only its chance differs between the one-shot and hazard paths.
    private static void AddCrisisCandidate(List<(Ideo? ideo, float chance, float weight)> candidates, float current, float crisisThreshold, Func<float, float> chanceOf)
    {
        if (current >= crisisThreshold)
        {
            return;
        }

        var weight = crisisThreshold - current;
        candidates.Add((null, chanceOf(weight), weight));
    }

    // CrisisThreshold scaled by sqrt(CertaintyLossFactor). sqrt dampens the raw stat: at 3x volatile the
    // threshold rises to ~1.73x (e.g. 25% -> 43%), not the full 75% that a linear scale would produce.
    internal float EffectiveCrisisThreshold()
    {
        var factor = Pawn.GetStatValue(StatDefOf.CertaintyLossFactor);
        return Mathf.Clamp01(EnhancedIdeologyMod.Settings.CrisisThreshold * Mathf.Sqrt(factor));
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

        // How drawn the pawn is to the new ideo, captured before SetIdeo - afterwards the own-ideo short-circuit
        // would report raw certainty instead. A convert overshoots: they arrive at their old certainty plus twice
        // the margin by which they preferred the new faith, so switching feels like a step up rather than a lateral
        // move (e.g. old certainty 0.4, new-faith opinion 0.5 -> arrives at 0.6). Preferring the new faith is a
        // precondition of conversion, so the margin is positive; the clamp only guards the top end.
        var opinionOfNew = IdeoOpinion(newIdeo);
        var newCertainty = Mathf.Clamp01(oldCertainty + (2f * (opinionOfNew - oldCertainty)));

        Pawn.ideo.SetIdeo(newIdeo);
        newIdeo.Notify_MemberGainedByConversion();

        SetExtendedCertainty(newCertainty);
        personalIdeoOpinions[newIdeo] = 0;

        // Keep current opinion of our old ideo by moving difference between new base and old base (certainty) into personal thoughts
        var oldBase = DetailedIdeoOpinion(oldIdeo!).BaseOpinion;
        AdjustPersonalOpinion(oldIdeo!, oldCertainty - oldBase);

        Find.PlayLog.Add(new PlayLogEntry_Conversion(Pawn, newIdeo));

        if (!oldIdeoContains)
        {
            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ConvertedNewMember, Pawn.Named(HistoryEventArgsNames.Doer), newIdeo.Named(HistoryEventArgsNames.Ideo)));
        }

        RecacheAllBaseOpinions();
    }
}

internal readonly struct DetailedIdeoOpinion(float baseOpinion, float personalOpinion, float relationshipOpinion, string? devModeDetails = null)
{
    public readonly float BaseOpinion => baseOpinion;
    public readonly float PersonalOpinion => personalOpinion;
    public readonly float RelationshipOpinion => relationshipOpinion;
    public readonly string? DevModeDetails => devModeDetails;
}
