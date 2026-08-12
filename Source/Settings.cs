namespace EnhancedIdeology;

public class Settings : ModSettings
{
    // Recommended defaults, in one place so the field initializer, the save/load fallback, and the "recommended"
    // tick drawn on each slider can never drift apart.
    private const float DefaultCertaintyDriftRate = 0.10f;
    private const float DefaultDifficultyOffset = 0f;
    private const float DefaultRelationalMaxRange = 0.12f;
    private const float DefaultPracticeMaxRange = 0.15f;
    private const float DefaultConversionPace = 1f;
    private const float DefaultDebateConvictionChange = 1f;
    private const float DefaultConversionStancePull = 2f;
    private const float DefaultConversionCertaintyKnock = 0.8f;
    private const float DefaultCrisisThreshold = 0.25f;
    private const float DefaultPreceptOppositionScale = 1f;

    private bool _debugInteractionWorkers;
    public bool DebugInteractionWorkers
    {
        get => _debugInteractionWorkers;
        set => _debugInteractionWorkers = value;
    }

    // How fast certainty relaxes toward its setpoint each day (dc/dt = k * (target - c)).
    private float _certaintyDriftRate = DefaultCertaintyDriftRate;
    public float CertaintyDriftRate => _certaintyDriftRate;

    // Flat shift applied to every pawn's target certainty. Positive = faith is stickier.
    private float _difficultyOffset = DefaultDifficultyOffset;
    public float DifficultyOffset => _difficultyOffset;

    // Maximum certainty the co-religionist relational band can add or remove.
    private float _relationalMaxRange = DefaultRelationalMaxRange;
    public float RelationalMaxRange => _relationalMaxRange;

    // Maximum certainty the current-practice band can add or remove.
    private float _practiceMaxRange = DefaultPracticeMaxRange;
    public float PracticeMaxRange => _practiceMaxRange;

    // Reference window (in days) over which a fully-tempted pawn's spontaneous-conversion odds play out
    // at 1x pace. The player-facing knob is a pace multiplier over this default; higher pace = shorter
    // effective interval = faster conversions.
    private const float DefaultConversionInterval = 3f;
    private float _conversionPace = DefaultConversionPace;
    public float ConversionPace => _conversionPace;
    public float ConversionInterval => DefaultConversionInterval / _conversionPace;

    // Certainty of the "crisis of faith" pseudo-ideo. When a pawn's conviction in their own ideo falls
    // below this, doubt joins the conversion draw as a competing option weighted by how far below it they
    // are - so a collapsing pawn with only weakly-preferred alternatives is likely to break down instead.
    private float _crisisThreshold = DefaultCrisisThreshold;
    public float CrisisThreshold => _crisisThreshold;

    // How far a single won debate moves the loser's belief on the contested issue, relative to default. Scales the
    // arc the loser's stance walks along the conviction valley toward the winner (on top of the debaters' stats).
    private float _debateConvictionChange = DefaultDebateConvictionChange;
    public float DebateConvictionChange => _debateConvictionChange;

    // How much harder a won directed conversion (a priest's action) pulls the recipient's stance than an ordinary
    // debate win. The recipient's most-opposed belief is dragged toward the preacher's rung by this multiple of the
    // per-debate pull; a lost conversion still shifts the preacher by the ordinary (1x) amount.
    private float _conversionStancePull = DefaultConversionStancePull;
    public float ConversionStancePull => _conversionStancePull;

    // Fraction of certainty a pawn retains after a won conversion attempt (a priest's action). Temporary - it
    // drifts back toward their structural setpoint - but the dip makes them more likely to convert now and to
    // spontaneously switch afterwards. 0.8 = an 80% pawn drops to 64%.
    private float _conversionCertaintyKnock = DefaultConversionCertaintyKnock;
    public float ConversionCertaintyKnock => _conversionCertaintyKnock;

    // How strongly a pawn opposes the stance at the opposite extreme of an issue's ladder, as a fraction of
    // their conviction (design.md R2). Opinion falls linearly from +strength at their own rung to
    // -oppositionScale·strength at the far end, crossing zero at 1/(1+oppositionScale) of the way out.
    // 0 = they merely grow indifferent toward opposite stances; 1 = full opposition. Higher = less tolerant.
    private float _preceptOppositionScale = DefaultPreceptOppositionScale;
    public float PreceptOppositionScale => _preceptOppositionScale;

    private Vector2 _scrollPosition;
    private float _contentHeight = 600f;

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref _debugInteractionWorkers, "debugInteractionWorkers", false);
        Scribe_Values.Look(ref _certaintyDriftRate, "certaintyDriftRate", DefaultCertaintyDriftRate);
        Scribe_Values.Look(ref _difficultyOffset, "difficultyOffset", DefaultDifficultyOffset);
        Scribe_Values.Look(ref _relationalMaxRange, "relationalMaxRange", DefaultRelationalMaxRange);
        Scribe_Values.Look(ref _practiceMaxRange, "practiceMaxRange", DefaultPracticeMaxRange);
        Scribe_Values.Look(ref _conversionPace, "conversionPace", DefaultConversionPace);
        Scribe_Values.Look(ref _debateConvictionChange, "debateConvictionChange", DefaultDebateConvictionChange);
        Scribe_Values.Look(ref _conversionStancePull, "conversionStancePull", DefaultConversionStancePull);
        Scribe_Values.Look(ref _conversionCertaintyKnock, "conversionCertaintyKnock", DefaultConversionCertaintyKnock);
        Scribe_Values.Look(ref _crisisThreshold, "crisisThreshold", DefaultCrisisThreshold);
        Scribe_Values.Look(ref _preceptOppositionScale, "preceptOppositionScale", DefaultPreceptOppositionScale);
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        var viewRect = new Rect(0f, 0f, inRect.width - 20f, _contentHeight);
        Widgets.BeginScrollView(inRect, ref _scrollPosition, viewRect);

        // The Listing draws into a rect taller than any content could be, so it never wraps into a second (offscreen)
        // column. Its measured CurHeight then feeds next frame's scroll viewRect above. Feeding the *measured* height
        // back into Begin instead would let content that overflows one frame wrap and collapse the layout.
        Listing_Standard listingStandard = new();
        listingStandard.Begin(new Rect(0f, 0f, viewRect.width, 100000f));

        Header(listingStandard, "EnhancedIdeology.Section.Certainty");
        PercentSlider(listingStandard, "EnhancedIdeology.CertaintyDriftRate", ref _certaintyDriftRate, 0.02f, 0.5f, DefaultCertaintyDriftRate);
        PercentSlider(listingStandard, "EnhancedIdeology.DifficultyOffset", ref _difficultyOffset, -0.5f, 0.5f, DefaultDifficultyOffset);
        PercentSlider(listingStandard, "EnhancedIdeology.RelationalMaxRange", ref _relationalMaxRange, 0f, 0.5f, DefaultRelationalMaxRange);
        PercentSlider(listingStandard, "EnhancedIdeology.PracticeMaxRange", ref _practiceMaxRange, 0f, 0.5f, DefaultPracticeMaxRange);
        PercentSlider(listingStandard, "EnhancedIdeology.CrisisThreshold", ref _crisisThreshold, 0f, 0.5f, DefaultCrisisThreshold);

        Header(listingStandard, "EnhancedIdeology.Section.Conversion");
        MultiplierSlider(listingStandard, "EnhancedIdeology.DebateConvictionChange", ref _debateConvictionChange, 0.25f, 4f, DefaultDebateConvictionChange);
        MultiplierSlider(listingStandard, "EnhancedIdeology.ConversionPace", ref _conversionPace, 0.25f, 4f, DefaultConversionPace);
        MultiplierSlider(listingStandard, "EnhancedIdeology.ConversionStancePull", ref _conversionStancePull, 1f, 5f, DefaultConversionStancePull);
        PercentSlider(listingStandard, "EnhancedIdeology.ConversionCertaintyKnock", ref _conversionCertaintyKnock, 0.5f, 1f, DefaultConversionCertaintyKnock);

        Header(listingStandard, "EnhancedIdeology.Section.Opinion");
        PercentSlider(listingStandard, "EnhancedIdeology.PreceptOppositionScale", ref _preceptOppositionScale, 0f, 1f, DefaultPreceptOppositionScale);

        Header(listingStandard, "EnhancedIdeology.Section.Debug");
        listingStandard.CheckboxLabeled(
            "EnhancedIdeology.DebugInteractionWorkers".Translate(),
            ref _debugInteractionWorkers,
            "EnhancedIdeology.DebugInteractionWorkers.Tip".Translate());

        _contentHeight = listingStandard.CurHeight;
        listingStandard.End();
        Widgets.EndScrollView();
    }

    private static void Header(Listing_Standard listing, string labelKey)
    {
        listing.Gap();
        Text.Font = GameFont.Medium;
        listing.Label(labelKey.Translate());
        Text.Font = GameFont.Small;
        listing.GapLine();
    }

    private static void PercentSlider(Listing_Standard listing, string labelKey, ref float value, float min, float max, float defaultValue)
    {
        listing.Label(labelKey.Translate(value.ToStringPercent()), tooltip: (labelKey + ".Tip").Translate());
        value = SliderWithDefault(listing, value, min, max, defaultValue);
    }

    private static void MultiplierSlider(Listing_Standard listing, string labelKey, ref float value, float min, float max, float defaultValue)
    {
        listing.Label(labelKey.Translate(value.ToString("0.0#") + "x"), tooltip: (labelKey + ".Tip").Translate());
        value = SliderWithDefault(listing, value, min, max, defaultValue);
    }

    // Colour of the "recommended" tick drawn at the default value.
    private static readonly Color DefaultTickColor = new(1f, 1f, 1f, 0.85f);

    // How close (as a fraction of the slider's range) a dragged value must land to the default before it snaps
    // onto it, so the recommended value is easy to return to instead of an off-by-a-hair fractional miss.
    private const float DefaultStickyFraction = 0.02f;

    // A slider that also draws a small vertical tick at the recommended default, so a tweaked value is visibly
    // off the recommendation. Mirrors Listing_Standard.Slider's rect/gap so the layout is unchanged.
    private static float SliderWithDefault(Listing_Standard listing, float value, float min, float max, float defaultValue)
    {
        var rect = listing.GetRect(22f);
        var result = Widgets.HorizontalSlider(rect, value, min, max);
        if (Mathf.Abs(result - defaultValue) < (DefaultStickyFraction * (max - min)))
        {
            result = defaultValue;
        }

        // Widgets.HorizontalSlider insets the handle track by 6px each side and the 12px handle is centred on the
        // value, so the handle centre travels over [rect.x+6, rect.x+rect.width-6]. Match that or the tick drifts
        // off the handle at the extremes. The handle spans rect.y..rect.y+12; draw the tick over that span.
        var frac = Mathf.Clamp01((defaultValue - min) / (max - min));
        var tickX = rect.x + 6f + (frac * (rect.width - 12f));
        var old = GUI.color;
        GUI.color = DefaultTickColor;
        Widgets.DrawLineVertical(tickX, rect.y, 12f);
        GUI.color = old;

        listing.Gap(listing.verticalSpacing);
        return result;
    }
}
