namespace EnhancedIdeology.Tests.Support;

internal sealed class PawnBuilder
{
    private Ideo? _ideo;
    private float _certainty = 0.75f;
    private float _conversionPower = 1f;
    private float _certaintyLossFactor = 1f;
    private float _socialImpact = 1f;
    private float _spreadFrequencyFactor = 1f;
    private float _baseMood = 0.85f;
    private float _colonyMoodOffset;
    private readonly Dictionary<SkillDef, int> _skills = [];
    private readonly List<Trait> _traits = [];
    private readonly Dictionary<StatDef, float> _extraStats = [];
    private string _label = string.Empty;

    public PawnBuilder WithIdeo(Ideo ideo) { _ideo = ideo; return this; }
    public PawnBuilder WithCertainty(float c) { _certainty = c; return this; }
    public PawnBuilder WithConversionPower(float v) { _conversionPower = v; return this; }
    public PawnBuilder WithCertaintyLossFactor(float v) { _certaintyLossFactor = v; return this; }
    public PawnBuilder WithSocialImpact(float v) { _socialImpact = v; return this; }
    public PawnBuilder WithBaseMood(float v) { _baseMood = v; return this; }
    public PawnBuilder WithColonyMoodOffset(float v) { _colonyMoodOffset = v; return this; }
    public PawnBuilder WithLabel(string label) { _label = label; return this; }

    public PawnBuilder WithSkill(SkillDef def, int level)
    {
        _skills[def] = level;
        return this;
    }

    public PawnBuilder WithTrait(TraitDef def, int degree = 0)
    {
        _traits.Add(new Trait { def = def, Degree = degree });
        return this;
    }

    public PawnBuilder WithOpinionOf(SimPawn other, float opinion)
    {
        _extraStats[StatDefOf.CertaintyLossFactor] = _certaintyLossFactor;
        // stored separately; applied in Build
        _pendingOpinions[other] = opinion;
        return this;
    }

    private readonly Dictionary<SimPawn, float> _pendingOpinions = [];

    public SimPawn Build(SimWorld world)
    {
        if (_ideo == null) throw new InvalidOperationException("PawnBuilder: ideo not set");

        var pawn = new SimPawn(_ideo, _certainty, _conversionPower, _certaintyLossFactor, _baseMood)
        {
            Label = _label,
            ColonyMoodOffset = _colonyMoodOffset,
        };

        pawn.SetStatValue(StatDefOf.SocialImpact, _socialImpact);
        pawn.SetStatValue(StatDefOf.SocialIdeoSpreadFrequencyFactor, _spreadFrequencyFactor);

        foreach (var (def, level) in _skills)
            pawn.skills.SetSkillLevel(def, level);

        foreach (var trait in _traits)
            pawn.story.traits.allTraits.Add(trait);

        foreach (var (other, opinion) in _pendingOpinions)
        {
            pawn.relations.SetOpinion(other, opinion);
            other.relations.SetOpinion(pawn, opinion);
        }

        world.AddPawn(pawn);
        pawn.UpdateThoughts();
        world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        return pawn;
    }
}
