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

    private readonly Dictionary<Ideo, float> cachedRelationshipIdeoOpinions = [];
    private readonly Dictionary<Pawn, float> cachedRelationships = [];

    private Dictionary<MemeDef, float> memeOpinions = [];
    private Dictionary<PreceptDef, float> preceptOpinions = [];

    private List<Ideo>? cache1;
    private List<Ideo>? cache2;
    private List<MemeDef>? cache3;
    private List<PreceptDef>? cache4;
    private List<float>? cache5;
    private List<float>? cache6;
    private List<float>? cache7;
    private List<float>? cache8;

    private float cachedOpinionMultiplier = -1f;
    private int lastMultiplierCacheTick = -1;

    public void SetIdeoBaseOpinion(Ideo ideo, float opinion)
    {
        if (!baseIdeoOpinions.TryAdd(ideo, opinion))
        {
            baseIdeoOpinions[ideo] = opinion;
        }
    }

    public float OpinionMultiplier
    {
        get
        {
            if (cachedOpinionMultiplier >= 0f && Find.TickManager.TicksGame - lastMultiplierCacheTick < 2000)
            {
                return cachedOpinionMultiplier;
            }

            cachedOpinionMultiplier = 1f;
            lastMultiplierCacheTick = Find.TickManager.TicksGame;

            if (Pawn.story == null || Pawn.story.traits == null)
            {
                return cachedOpinionMultiplier;
            }

            foreach (var trait in Pawn.story.traits.allTraits)
            {
                var ext = trait.def.GetModExtension<IdeoTraitExtension>();

                if (ext != null)
                {
                    cachedOpinionMultiplier *= ext.opinionMultiplier;
                }
            }

            return cachedOpinionMultiplier;
        }
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

        // Structural band: innate fit of the pawn to their own ideo, including the flat base faith.
        var structural = StructuralOpinionOf(Pawn.Ideo, StructuralContributors) / 100f;
        StructuralContributors.Add(("EnhancedBeliefs.Certainty.BaseFaith".Translate(), 0.30f));
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

        foreach (var precept in pawnIdeo.precepts)
        {
            var comps = precept.TryGetComps<PreceptComp_OpinionOffset>();

            foreach (var comp in comps)
            {
                opinion += comp.InternalOffset;
                if (comp.InternalOffset != 0)
                {
                    contributors?.Add((precept.def.LabelCap, comp.InternalOffset));
                }
            }
        }

        foreach (var precept in ideo.precepts)
        {
            var comps = precept.TryGetComps<PreceptComp_OpinionOffset>();

            foreach (var comp in comps)
            {
                var value = comp.ExternalOffset + comp.GetTraitOpinion(Pawn);
                opinion += value;
                if (value != 0f)
                {
                    contributors?.Add((precept.def.LabelCap, value));
                }
            }
        }

        // -5 opinion per incompatible meme, +5 per shared meme (own ideo is maximally similar to itself)
        var cohesion = -GameComponent_EnhancedBeliefs.BeliefDifferences(pawnIdeo, ideo) * 5f;
        opinion += cohesion;
        if (cohesion != 0f)
        {
            contributors?.Add(("EnhancedBeliefs.Certainty.MemeCohesion".Translate(), cohesion));
        }

        // Only decrease opinion if we don't like getting converted, shouldn't go the other way
        var scale = Mathf.Clamp01(Pawn.GetStatValue(StatDefOf.CertaintyLossFactor)) * OpinionMultiplier;
        opinion *= scale;

        // Rescale the collected raw offsets into certainty-fraction units matching the band total.
        if (contributors != null)
        {
            for (var ii = start; ii < contributors.Count; ii++)
            {
                contributors[ii] = (contributors[ii].label, contributors[ii].pct * scale / 100f);
            }
        }

        // 30 base opinion
        return Mathf.Clamp(opinion + 30f, 0, 100);
    }

    public float PersonalIdeoOpinion(Ideo ideo, out string? devDetails)
    {
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
            var relevantPreceptCount = ideo.precepts.Select(p => p.def).Intersect(preceptOpinions.Keys).Count();
            _ = devDetailsBuilder.AppendLine($"Precept opinions: {relevantPreceptCount}");
            foreach (var preceptDef in ideo.precepts.Select(p => p.def))
            {
                if (preceptOpinions.TryGetValue(preceptDef, out var preceptOpinion))
                {
                    _ = devDetailsBuilder.AppendLine($" - {preceptDef.LabelCap}: {preceptOpinion}");
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

        foreach (var preceptDef in ideo.precepts.Select(p => p.def))
        {
            if (preceptOpinions.TryGetValue(preceptDef, out var preceptOpinion))
            {
                opinion += preceptOpinion;
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

        return (opinion + personalIdeoOpinions[ideo]) * OpinionMultiplier;
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

        return cachedRelationshipIdeoOpinions[ideo] * OpinionMultiplier;
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
        Scribe_Collections.Look(ref preceptOpinions, "preceptOpinions", LookMode.Def, LookMode.Value, ref cache4, ref cache8);

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

    public void AdjustPreceptOpinion(PreceptDef precept, float power)
    {
        preceptOpinions ??= [];

        if (!preceptOpinions.ContainsKey(precept))
        {
            preceptOpinions[precept] = 0;
        }

        preceptOpinions[precept] += power * 100f;
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

    public float TruePreceptOpinion(PreceptDef precept)
    {
        if (!preceptOpinions.TryGetValue(precept, out var opinion))
        {
            opinion = 0;
            preceptOpinions[precept] = opinion;
        }

        var comps = precept.TryGetComps<PreceptComp_OpinionOffset>();

        foreach (var comp in comps)
        {
            opinion += comp.ExternalOffset + comp.GetTraitOpinion(Pawn);
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

    // Check if pawn should get converted to a new ideo after losing certainty in some way.
    // TODO: This sounds like a method without side-effects, but it actually makes changes,
    //       so go through the code and figure out what it actually does and then rename it to something more appropriate.
    public ConversionOutcome CheckConversion(
        Ideo? priorityIdeo = null,
        bool noBreakdown = false,
        List<Ideo>? excludeIdeos = null,
        List<Ideo>? whitelistIdeos = null,
        float? opinionThreshold = null)
    {
        EnhancedBeliefsMod.Debug($"CheckConversion called: pawn={Pawn}, priorityIdeo={priorityIdeo}, noBreakdown={noBreakdown}, excludeIdeos={excludeIdeos}, whitelistIdeos={whitelistIdeos}, opinionThreshold={opinionThreshold}");
        if (!ModLister.CheckIdeology("Ideoligion conversion") || Pawn.DevelopmentalStage.Baby())
        {
            EnhancedBeliefsMod.Debug("CheckConversion: ideology not enabled or pawn is baby. Returning Failure.");
            return ConversionOutcome.Failure;
        }
        if (Find.IdeoManager.classicMode)
        {
            EnhancedBeliefsMod.Debug("CheckConversion: classicMode enabled. Returning Failure.");
            return ConversionOutcome.Failure;
        }

        var certainty = Pawn.ideo.Certainty;
        EnhancedBeliefsMod.Debug($"CheckConversion: certainty={certainty}");

        if (certainty > 0.2f)
        {
            EnhancedBeliefsMod.Debug("CheckConversion: certainty > 0.2. Returning Failure.");
            return ConversionOutcome.Failure;
        }

        var threshold = certainty <= 0f ? 0.6f : 0.85f; //Drastically lower conversion threshold if we're about to have a breakdown

        if (opinionThreshold.HasValue) // Or if we're already having one
        {
            threshold = opinionThreshold.Value;
        }

        var currentOpinion = IdeoOpinion(Pawn.Ideo);
        List<Ideo> ideos = [.. whitelistIdeos ?? Find.IdeoManager.IdeosListForReading];
        ideos.SortBy(IdeoOpinion);

        if (excludeIdeos != null)
        {
            ideos = [.. ideos.Except(excludeIdeos)];
        }

        // Moves priority ideo up to the top of the list so if the pawn is being converted and not having a random breakdown, they're gonna probably get converted to the target ideology
        if (priorityIdeo != null)
        {
            _ = ideos.Remove(priorityIdeo);
            ideos.Add(priorityIdeo);
        }

        foreach (var ideo in ideos.Reverse<Ideo>())
        {
            if (ideo == Pawn.Ideo)
            {
                continue;
            }

            var opinion = IdeoOpinion(ideo);
            EnhancedBeliefsMod.Debug($"CheckConversion: considering ideo={ideo}, opinion={opinion}, threshold={threshold}, currentOpinion={currentOpinion}");

            // Also don't convert in case we somehow like our current ideo significantly more than the new one
            // Either we have VERY high relationships with a lot of people or very strong personal opinions on current ideology for this to even be possible
            if (opinion < threshold || currentOpinion > opinion)
            {
                EnhancedBeliefsMod.Debug("CheckConversion: opinion below threshold or currentOpinion > opinion. Skipping.");
                continue;
            }

            // 17% minimal chance of conversion at 20% certrainty and 85% opinion, half that if we're being converted and this is a wrong ideology. Randomly converting to a wrong ideology should be just a rare lol moment
            var rand = Rand.Value;
            var chance = (1 - (certainty * 4f)) * (opinion + (certainty <= 0f ? 0.3f : 0)) * ((priorityIdeo != null && priorityIdeo != ideo) ? 0.5f : 1f);
            EnhancedBeliefsMod.Debug($"CheckConversion: rand={rand}, chance={chance}");
            if (rand > chance)
            {
                EnhancedBeliefsMod.Debug("CheckConversion: random roll failed. Skipping.");
                continue;
            }

            var oldIdeoContains = Pawn.ideo.PreviousIdeos.Contains(ideo);
            var oldIdeo = Pawn.Ideo;
            EnhancedBeliefsMod.Debug($"CheckConversion: converting from {oldIdeo} to {ideo}");
            Pawn.ideo.SetIdeo(ideo);
            ideo.Notify_MemberGainedByConversion();

            // Move personal opinion into certainty i.e. base opinion, then zero it, since base opinions are fixed and personal beliefs are what is usually meant by certainty anyways
            var rundown = DetailedIdeoOpinion(ideo);
            Pawn.ideo.Certainty = Mathf.Min(rundown.BaseOpinion, 0.2f) + rundown.PersonalOpinion;
            personalIdeoOpinions[ideo] = 0;

            // Keep current opinion of our old ideo by moving difference between new base and old base (certainty) into personal thoughts
            var oldBase = DetailedIdeoOpinion(oldIdeo).BaseOpinion;
            EnhancedBeliefsMod.Debug($"CheckConversion: Adjusting personal opinion for oldIdeo {oldIdeo} by {certainty - oldBase}");
            AdjustPersonalOpinion(oldIdeo, certainty - oldBase);

            if (!oldIdeoContains)
            {
                EnhancedBeliefsMod.Debug($"CheckConversion: recording conversion event for {Pawn} to {ideo}");
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ConvertedNewMember, Pawn.Named(HistoryEventArgsNames.Doer), ideo.Named(HistoryEventArgsNames.Ideo)));
            }

            RecacheAllBaseOpinions();
            EnhancedBeliefsMod.Debug("CheckConversion: ConversionOutcome.Success");
            return ConversionOutcome.Success;
        }

        if (certainty > 0f || noBreakdown)
        {
            EnhancedBeliefsMod.Debug("CheckConversion: certainty > 0 or noBreakdown. Returning Failure.");
            return ConversionOutcome.Failure;
        }

        // Oops
        EnhancedBeliefsMod.Debug("CheckConversion: triggering IdeoChange mental state. Returning Breakdown.");
        _ = Pawn.mindState.mentalStateHandler.TryStartMentalState(EnhancedBeliefsDefOf.IdeoChange);
        return ConversionOutcome.Breakdown;
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
