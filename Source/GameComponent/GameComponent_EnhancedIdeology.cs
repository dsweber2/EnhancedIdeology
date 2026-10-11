namespace EnhancedIdeology;

#pragma warning disable CS9113 // Parameter is unread.
internal sealed partial class GameComponent_EnhancedIdeology(Game game) : GameComponent
#pragma warning restore CS9113 // Parameter is unread.
{
    // Practice band shape: summed precept-thought mood offset to a normalized intensity in [-1, 1].
    // Amplitude (how much certainty this can move) is applied afterwards via PracticeMaxRange.
    internal static readonly SimpleCurve PracticeIntensityCurve =
    [
        new CurvePoint(-50f, -1.0f),
        new CurvePoint(-30f, -0.75f),
        new CurvePoint(-15f, -0.45f),
        new CurvePoint(-5f,  -0.15f),
        new CurvePoint(0f,    0f),
        new CurvePoint(5f,    0.15f),
        new CurvePoint(15f,   0.45f),
        new CurvePoint(30f,   0.75f),
        new CurvePoint(50f,   1.0f),
    ];

    // Relational band shape: mean opinion of co-religionists to a normalized intensity in [-1, 1].
    // Amplitude is applied afterwards via RelationalMaxRange.
    internal static readonly SimpleCurve RelationalIntensityCurve =
    [
        new CurvePoint(-100f, -1.0f),
        new CurvePoint(-60f,  -0.70f),
        new CurvePoint(-30f,  -0.45f),
        new CurvePoint(-10f,  -0.18f),
        new CurvePoint(0f,     0f),
        new CurvePoint(10f,    0.18f),
        new CurvePoint(30f,    0.45f),
        new CurvePoint(60f,    0.70f),
        new CurvePoint(100f,   1.0f),
    ];

    public PawnIdeoTracker PawnTracker { get; } = new();
    public IdeoPawnTracker IdeoTracker { get; } = new();

    // Ladders as they were when the loaded game was saved; null for a new game or a save made before ladders were
    // saved. Components load before the world and maps, so this is set before any pawn stance is remapped.
    private Dictionary<string, SavedLadder>? _savedLadders;

    internal SavedLadder? SavedLadderFor(IssueDef issue) => _savedLadders?.GetValueOrDefault(issue.defName);

    public override void ExposeData()
    {
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            _savedLadders = LadderMigration.CaptureAll();
        }
        Scribe_Collections.Look(ref _savedLadders, "savedLadders", LookMode.Value, LookMode.Deep);
    }

    // Set so that an offset of 8 at default
    internal const float MoodletConvictionScalar = 0.0007f;

    private readonly List<Thought> _tmpThoughts = [];

    // One pawn's mood pulls, called from that pawn's long tick. The tick patch decides which pawns drift, and the
    // hash offset spreads the pawns over the interval instead of doing all of them in one tick.
    // Memories and situational thoughts both count. Vanilla precept thoughts are plain Thought_Memory or
    // Thought_Situational, so the filter is the source precept, not the thought class.
    internal void ApplyMoodletConvictionShifts(IdeoTrackerData tracker)
    {
        var pawn = tracker.Pawn;
        _tmpThoughts.Clear();
        pawn.needs?.mood?.thoughts?.GetAllMoodThoughts(_tmpThoughts);
        foreach (var thought in _tmpThoughts)
        {
            if (RelicConviction.IsRelicThought(thought.def))
            {
                RelicConviction.ApplyMoodletShift(pawn, tracker, thought);
                continue;
            }
            // Dissonance is the response to a foreign practice, not a practice of the pawn's own faith.
            if (thought is Thought_CognitiveDissonance) continue;
            if (thought.sourcePrecept is not { } precept || precept.ideo != pawn.Ideo) continue;
            if (precept.def.issue is not { } issue) continue;
            var delta = MoodletConvictionDelta(pawn, thought.MoodOffset());
            if (Mathf.Abs(delta) < 0.00001f) continue;
            // Only Moral issues have a ladder to move along. NA issues (rituals, buildings) have no stance.
            switch (PreceptPolicy.CategoryOf(issue))
            {
                case PreceptCategory.NA:
                    break;
                case PreceptCategory.Moral:
                    ConvictionMath.ApplyMoodPull(tracker, precept.ideo, issue, delta);
                    break;
                default:
                    tracker.ShiftIssueStance(issue, 0f, 0f, delta, fromMood: true);
                    break;
            }
        }
    }

    internal static float MoodletConvictionDelta(Pawn pawn, float moodOffset) =>
        moodOffset
        * pawn.GetStatValue(StatDefOf.CertaintyLossFactor)
        * EnhancedIdeologyMod.Settings.ConversionStancePull
        * MoodletConvictionScalar;

    public void SetIdeo(Pawn pawn, Ideo ideo)
    {
        _ = PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        RemoveFromIdeoLists(pawn);

        if (ideo == null)
        {
            return;
        }

        IdeoTracker.EnsureIdeoPawnTrackerHasPawn(ideo, pawn);
    }

    // The ideo lists hold strong references, so a discarded pawn left in them is never collected, and its
    // tracker stays in the weak table too. Every scan of the lists or trackers then grows over the game.
    public void Notify_PawnDiscarded(Pawn pawn)
    {
        RemoveFromIdeoLists(pawn);
        _ = PawnTracker.RemoveTracker(pawn);
    }

    private void RemoveFromIdeoLists(Pawn pawn)
    {
        foreach (var ideo in IdeoTracker.Select(kvp => kvp.Key).ToList())
            _ = IdeoTracker.RemovePawnFromIdeoPawnTracker(ideo, pawn);
    }

    internal static int BeliefDifferences(Ideo ideo1, Ideo ideo2)
    {
        var value = 0;

        foreach (var meme1 in ideo1.memes)
        {
            foreach (var meme2 in ideo2.memes)
            {
                if (meme1 == meme2)
                {
                    value -= 1;
                }
                else if (meme1.exclusionTags.Intersect(meme2.exclusionTags).Any())
                {
                    value += 1;
                }
            }
        }

        return value;
    }

    public void BaseOpinionRecache(Ideo ideo)
    {
        foreach (var ideoTracker in PawnTracker.Select(kvp => kvp.Value).ToList())
        {
            ideoTracker.SetIdeoBaseOpinion(ideo, ideoTracker.StructuralIdeoOpinion(ideo));
            ideoTracker.InvalidateStructural();
        }
    }

    public List<Pawn> GetIdeoPawns(Ideo ideo)
    {
        if (IdeoTracker.TryGetPawnTracker(ideo, out var pawnList))
        {
            return pawnList;
        }

        pawnList = IdeoTracker.EnsureIdeoHasPawnTracker(ideo);
        foreach (var pawn in PawnsFinder.All_AliveOrDead)
        {
            if (pawn.Ideo == ideo && !pawnList.Contains(pawn))
            {
                pawnList.Add(pawn);
            }
        }

        return pawnList;
    }
}

public enum ConversionOutcome
{
    Failure = 0,
    Breakdown = 1,
    Success = 2
}
