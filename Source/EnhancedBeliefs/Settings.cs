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

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref _debugInteractionWorkers, "debugInteractionWorkers", false);
        Scribe_Values.Look(ref _certaintyDriftRate, "certaintyDriftRate", 0.10f);
        Scribe_Values.Look(ref _difficultyOffset, "difficultyOffset", 0f);
        Scribe_Values.Look(ref _relationalMaxRange, "relationalMaxRange", 0.12f);
        Scribe_Values.Look(ref _practiceMaxRange, "practiceMaxRange", 0.15f);
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
}
