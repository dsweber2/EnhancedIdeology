namespace EnhancedIdeology;

// Stances at the start of this game day and of the previous one, for the opinion card's change arrows.
// IssueStanceTracker records a snapshot at each new day, before decay. Before() reads from the start of the
// previous day, so the window is always at least one full day.
internal sealed class StanceHistory
{
    private Dictionary<IssueDef, float> _rankDayStart = [];
    private Dictionary<IssueDef, float> _strengthDayStart = [];
    private Dictionary<IssueDef, float> _rankPrevDayStart = [];
    private Dictionary<IssueDef, float> _strengthPrevDayStart = [];

    // Scribe_Collections working lists.
    private List<IssueDef>? _rankDayStartKeys;
    private List<float>? _rankDayStartValues;
    private List<IssueDef>? _strengthDayStartKeys;
    private List<float>? _strengthDayStartValues;
    private List<IssueDef>? _rankPrevDayStartKeys;
    private List<float>? _rankPrevDayStartValues;
    private List<IssueDef>? _strengthPrevDayStartKeys;
    private List<float>? _strengthPrevDayStartValues;

    internal void RecordDayStart(Dictionary<IssueDef, float> ranks, Dictionary<IssueDef, float> strengths)
    {
        _rankPrevDayStart = _rankDayStart;
        _strengthPrevDayStart = _strengthDayStart;
        _rankDayStart = new Dictionary<IssueDef, float>(ranks);
        _strengthDayStart = new Dictionary<IssueDef, float>(strengths);
    }

    // Records the current stance as the day start for each issue that has no day-start record. For a loaded save
    // made before the records existed, and for a new pawn.
    internal void FillMissing(Dictionary<IssueDef, float> ranks, Dictionary<IssueDef, float> strengths)
    {
        foreach (var (issue, strength) in strengths)
        {
            if (_rankDayStart.ContainsKey(issue) && _strengthDayStart.ContainsKey(issue)) continue;
            _rankDayStart[issue] = ranks[issue];
            _strengthDayStart[issue] = strength;
        }
    }

    // Moves the recorded ranks to the live ladders. `remap` returns null for an issue whose ladder did not change.
    internal void RemapRanks(Func<IssueDef, float, float?> remap)
    {
        foreach (var ranks in new[] { _rankDayStart, _rankPrevDayStart })
            foreach (var issue in ranks.Keys.ToList())
                if (remap(issue, ranks[issue]) is { } rank)
                    ranks[issue] = rank;
    }

    // The stance at the start of the previous day, else at the start of this day, else null.
    internal (float rank, float strength)? Before(IssueDef issue)
    {
        if (_rankPrevDayStart.TryGetValue(issue, out var rank) && _strengthPrevDayStart.TryGetValue(issue, out var strength))
            return (rank, strength);
        if (_rankDayStart.TryGetValue(issue, out rank) && _strengthDayStart.TryGetValue(issue, out strength))
            return (rank, strength);
        return null;
    }

    // Called flat from IssueStanceTracker.ExposeData.
    internal void ExposeData()
    {
        Scribe_Collections.Look(ref _rankDayStart, "issueRankDayStart", LookMode.Def, LookMode.Value, ref _rankDayStartKeys, ref _rankDayStartValues);
        Scribe_Collections.Look(ref _strengthDayStart, "issueStrengthDayStart", LookMode.Def, LookMode.Value, ref _strengthDayStartKeys, ref _strengthDayStartValues);
        Scribe_Collections.Look(ref _rankPrevDayStart, "issueRankPrevDayStart", LookMode.Def, LookMode.Value, ref _rankPrevDayStartKeys, ref _rankPrevDayStartValues);
        Scribe_Collections.Look(ref _strengthPrevDayStart, "issueStrengthPrevDayStart", LookMode.Def, LookMode.Value, ref _strengthPrevDayStartKeys, ref _strengthPrevDayStartValues);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            _rankDayStart ??= [];
            _strengthDayStart ??= [];
            _rankPrevDayStart ??= [];
            _strengthPrevDayStart ??= [];
        }
    }
}
