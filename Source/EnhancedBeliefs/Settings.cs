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

    // Fraction of the distance from a pawn's preferred stance to the far end of an issue's ladder at which
    // their opinion of a rung crosses from positive to negative (design.md R2). Lower = less tolerant of
    // differing stances.
    private float _preceptZeroFrac = 0.5f;
    public float PreceptZeroFrac => _preceptZeroFrac;

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
        Scribe_Values.Look(ref _preceptZeroFrac, "preceptZeroFrac", 0.5f);
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
        PercentSlider(listingStandard, "EnhancedBeliefs.PreceptZeroFrac", ref _preceptZeroFrac, 0.1f, 1f);

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
