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

    public override void GameComponentTick()
    {
        base.GameComponentTick();
    }

#pragma warning disable IDE0079
#pragma warning disable IDE0060 // Remove unused parameter
    // TODO: This method seems... lacking. Investigate if it should be doing something more.
#pragma warning disable CA1822 // Mark members as static
    public float ConversionFactor(Pawn initiator, Pawn recipient)
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079
    {
        return 1f;
    }

    public void SetIdeo(Pawn pawn, Ideo ideo)
    {
        _ = PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        foreach (var ideo2 in IdeoTracker.Select(kvp => kvp.Key).ToList())
        {
            _ = IdeoTracker.RemovePawnFromIdeoPawnTracker(ideo2, pawn);
        }

        if (ideo == null)
        {
            return;
        }

        IdeoTracker.EnsureIdeoPawnTrackerHasPawn(ideo, pawn);
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
        }
    }

    public List<Pawn> GetIdeoPawns(Ideo ideo)
    {
        if (IdeoTracker.TryGetPawnTracker(ideo, out var pawnList))
        {
            return pawnList;
        }

        foreach (var pawn in PawnsFinder.All_AliveOrDead)
        {
            if (pawn.Ideo == ideo)
            {
                IdeoTracker.EnsureIdeoPawnTrackerHasPawn(ideo, pawn);
            }
        }

        return GetIdeoPawns(ideo);
    }
}

public enum ConversionOutcome
{
    Failure = 0,
    Breakdown = 1,
    Success = 2
}
