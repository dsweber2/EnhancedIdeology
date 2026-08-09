namespace EnhancedBeliefs;

public class Settings : ModSettings
{
    private bool _debugInteractionWorkers;
    public bool DebugInteractionWorkers
    {
        get => _debugInteractionWorkers;
        set => _debugInteractionWorkers = value;
    }

    // How fast certainty relaxes toward its setpoint each day (dc/dt = k * (target - c)).
    private float _certaintyDriftRate = 0.10f;
    public float CertaintyDriftRate => _certaintyDriftRate;

    // Flat shift applied to every pawn's target certainty. Positive = faith is stickier.
    private float _difficultyOffset = 0f;
    public float DifficultyOffset => _difficultyOffset;

    // Maximum certainty the co-religionist relational band can add or remove.
    private float _relationalMaxRange = 0.12f;
    public float RelationalMaxRange => _relationalMaxRange;

    // Maximum certainty the current-practice band can add or remove.
    private float _practiceMaxRange = 0.15f;
    public float PracticeMaxRange => _practiceMaxRange;

    // Reference window (in days) over which a fully-tempted pawn's spontaneous-conversion odds play out
    // at 1x pace. The player-facing knob is a pace multiplier over this default; higher pace = shorter
    // effective interval = faster conversions.
    private const float DefaultConversionInterval = 3f;
    private float _conversionPace = 1f;
    public float ConversionPace => _conversionPace;
    public float ConversionInterval => DefaultConversionInterval / _conversionPace;

    // Certainty of the "crisis of faith" pseudo-ideo. When a pawn's conviction in their own ideo falls
    // below this, doubt joins the conversion draw as a competing option weighted by how far below it they
    // are - so a collapsing pawn with only weakly-preferred alternatives is likely to break down instead.
    private float _crisisThreshold = 0.25f;
    public float CrisisThreshold => _crisisThreshold;

    // How strongly a pawn opposes the stance at the opposite extreme of an issue's ladder, as a fraction of
    // their conviction (design.md R2). Opinion falls linearly from +strength at their own rung to
    // -oppositionScale·strength at the far end, crossing zero at 1/(1+oppositionScale) of the way out.
    // 0 = they merely grow indifferent toward opposite stances; 1 = full opposition. Higher = less tolerant.
    private float _preceptOppositionScale = 1f;
    public float PreceptOppositionScale => _preceptOppositionScale;

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref _debugInteractionWorkers, "debugInteractionWorkers", false);
        Scribe_Values.Look(ref _certaintyDriftRate, "certaintyDriftRate", 0.10f);
        Scribe_Values.Look(ref _difficultyOffset, "difficultyOffset", 0f);
        Scribe_Values.Look(ref _relationalMaxRange, "relationalMaxRange", 0.12f);
        Scribe_Values.Look(ref _practiceMaxRange, "practiceMaxRange", 0.15f);
        Scribe_Values.Look(ref _conversionPace, "conversionPace", 1f);
        Scribe_Values.Look(ref _crisisThreshold, "crisisThreshold", 0.25f);
        Scribe_Values.Look(ref _preceptOppositionScale, "preceptOppositionScale", 1f);
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard listingStandard = new();
        listingStandard.Begin(inRect);

        PercentSlider(listingStandard, "EnhancedBeliefs.CertaintyDriftRate", ref _certaintyDriftRate, 0.02f, 0.5f);
        PercentSlider(listingStandard, "EnhancedBeliefs.DifficultyOffset", ref _difficultyOffset, -0.5f, 0.5f);
        PercentSlider(listingStandard, "EnhancedBeliefs.RelationalMaxRange", ref _relationalMaxRange, 0f, 0.5f);
        PercentSlider(listingStandard, "EnhancedBeliefs.PracticeMaxRange", ref _practiceMaxRange, 0f, 0.5f);

        listingStandard.Gap();

        MultiplierSlider(listingStandard, "EnhancedBeliefs.ConversionPace", ref _conversionPace, 0.25f, 4f);
        PercentSlider(listingStandard, "EnhancedBeliefs.CrisisThreshold", ref _crisisThreshold, 0f, 0.5f);
        PercentSlider(listingStandard, "EnhancedBeliefs.PreceptOppositionScale", ref _preceptOppositionScale, 0f, 1f);

        listingStandard.Gap();

        listingStandard.CheckboxLabeled(
            "EnhancedBeliefs.DebugInteractionWorkers".Translate(),
            ref _debugInteractionWorkers,
            "EnhancedBeliefs.DebugInteractionWorkers.Tip".Translate());

        listingStandard.End();
    }

    private static void PercentSlider(Listing_Standard listing, string labelKey, ref float value, float min, float max)
    {
        listing.Label(labelKey.Translate(value.ToStringPercent()), tooltip: (labelKey + ".Tip").Translate());
        value = listing.Slider(value, min, max);
    }

    private static void MultiplierSlider(Listing_Standard listing, string labelKey, ref float value, float min, float max)
    {
        listing.Label(labelKey.Translate(value.ToString("0.0#") + "x"), tooltip: (labelKey + ".Tip").Translate());
        value = listing.Slider(value, min, max);
    }
}
