namespace EnhancedIdeology;

// Per-pawn certainty state: extended certainty, the three setpoint bands, calibration flags, and cached
// band values. Callers compute the structural/relational/practitional bands and hand them in via SetBands;
// this class owns the post-band logic (calibration, setpoint, drift, initialization snap).
internal sealed class CertaintyTracker
{
    private Pawn _pawn;

    // Extended certainty: uncapped version of Pawn.ideo.Certainty. Vanilla certainty is always
    // Clamp01(ExtendedCertainty). Initialized lazily from vanilla if not yet set (old saves, fresh trackers).
    private float _extendedCertainty = -1f;
    public float ExtendedCertainty
    {
        get
        {
            if (_extendedCertainty < 0f)
                _extendedCertainty = _pawn.ideo.Certainty;
            return _extendedCertainty;
        }
    }

    // True once the first setpoint is known and certainty has been seeded to it.
    // Defaults true on load (old saves already have a played-in certainty).
    private bool _certaintyInitialized;

    // Set when stances were seeded for a pawn that already has a played-in certainty. On the next
    // recache, strengths are scaled so the structural band lands on _calibrationTargetCertainty.
    private bool _needsStanceCalibration;
    private float _calibrationTargetCertainty;

    // Setpoint and its components, refreshed by IdeoTrackerData.CertaintyChangeRecache.
    public float CachedCertaintyChange { get; private set; } = -9999f;
    public float CachedTargetCertainty { get; private set; }
    public float CachedStructural { get; private set; }
    public float CachedRelational { get; private set; }
    public float CachedPractitional { get; private set; }

    // Top contributors to each band for the social-card tooltip; (label, certainty-fraction contribution).
    public readonly List<(string label, float pct)> StructuralContributors = [];
    public readonly List<(string label, float pct)> RelationalContributors = [];
    public readonly List<(string label, float pct)> PractitionalContributors = [];

    internal CertaintyTracker(Pawn pawn) { _pawn = pawn; }
    internal void SetPawn(Pawn pawn) { _pawn = pawn; }

    internal bool IsInitialized => _certaintyInitialized;

    // Signal that stances were just freshly seeded for a pawn that already has a played-in certainty.
    internal void ScheduleCalibration(float targetCertainty)
    {
        _needsStanceCalibration = true;
        _calibrationTargetCertainty = targetCertainty;
    }

    // Set extended certainty to an absolute value (no upper cap) and sync vanilla to Clamp01.
    public void SetExtendedCertainty(float value)
    {
        _extendedCertainty = Mathf.Max(0f, value);
        _pawn.ideo.Certainty = Mathf.Clamp01(_extendedCertainty);
    }

    // Advance extended certainty by CachedCertaintyChange * deltaDays; sync vanilla = Clamp01(extended).
    // Detects external resets (crisis, knock, book burn) by comparing vanilla against Clamp01(extended):
    // if vanilla is lower than expected, something outside our tick reduced it — snap extended down to match.
    internal void AdvanceExtendedCertainty(float deltaDays)
    {
        var expectedVanilla = Mathf.Clamp01(ExtendedCertainty);
        if (_pawn.ideo.Certainty < expectedVanilla - 0.001f)
            _extendedCertainty = _pawn.ideo.Certainty;
        _extendedCertainty = Mathf.Max(0f, ExtendedCertainty + CachedCertaintyChange * deltaDays);
        _pawn.ideo.Certainty = Mathf.Clamp01(_extendedCertainty);
    }

    // Detect external writes to vanilla certainty (reassure, book, entrench) and snap extended down to match.
    internal void SyncFromVanillaIfNeeded(float vanillaCertainty)
    {
        if (Mathf.Abs(vanillaCertainty - Mathf.Clamp01(ExtendedCertainty)) > 0.001f)
            _extendedCertainty = vanillaCertainty;
    }

    // Mark this tracker as belonging to a pawn whose save predates EB's issue-stance model. Certainty is
    // already "initialized" (no snap), and one calibration pass will align the structural setpoint with
    // the certainty the pawn already has.
    internal void MarkAsLoadedWithoutData(float existingCertainty)
    {
        _certaintyInitialized = true;
        _needsStanceCalibration = true;
        _calibrationTargetCertainty = existingCertainty;
        _extendedCertainty = existingCertainty;
    }

    // Store the freshly-computed band values. Must be called before TryApplyCalibration, since
    // CalibrateStancesToCertainty reads CachedRelational and CachedPractitional.
    internal void SetBands(float structural, float relational, float practitional)
    {
        CachedStructural = structural;
        CachedRelational = relational;
        CachedPractitional = practitional;
    }

    // If calibration was scheduled, apply it now and return true (caller must recompute structural and
    // call SetBands again before FinalizeRecache).
    internal bool TryApplyCalibration(float structural, IssueStanceTracker stances)
    {
        if (!_needsStanceCalibration) return false;
        _needsStanceCalibration = false;
        CalibrateStancesToCertainty(structural, stances);
        return true;
    }

    // Finalise the setpoint from the post-calibration structural value, seed certainty on first call,
    // and recompute the drift rate. Caller must have called SetBands with the latest relational/practitional
    // before calling this.
    internal void FinalizeRecache(float structural, float driftRate)
    {
        var target = Mathf.Max(0f, structural + CachedRelational + CachedPractitional);
        CachedTargetCertainty = target;

        if (!_certaintyInitialized)
        {
            _certaintyInitialized = true;
            _extendedCertainty = target;
            _pawn.ideo.Certainty = Mathf.Clamp01(target);
        }

        CachedCertaintyChange = driftRate * (target - ExtendedCertainty);
    }

    // Called flat from IdeoTrackerData.ExposeData (no Scribe_Deep wrapper) to preserve XML structure.
    internal void ExposeData()
    {
        Scribe_Values.Look(ref _certaintyInitialized, "certaintyInitialized", defaultValue: true);
        Scribe_Values.Look(ref _extendedCertainty, "extendedCertainty", defaultValue: -1f);
    }

    // Scale all strengths so the structural band lands on _calibrationTargetCertainty. Called when a pawn
    // with a played-in certainty gets stances seeded for the first time (vanilla saves, old EB saves).
    private void CalibrateStancesToCertainty(float naturalStructural, IssueStanceTracker stances)
    {
        if (naturalStructural <= 0f) return;
        var minCertainty = EnhancedIdeologyMod.Settings.SaveCompatMinCertainty;
        var targetCertainty = Mathf.Max(_calibrationTargetCertainty, minCertainty);
        var targetStructural = Mathf.Clamp01(targetCertainty - CachedRelational - CachedPractitional);
        stances.ScaleStrengths(targetStructural / naturalStructural);
    }
}
