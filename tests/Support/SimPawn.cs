namespace EnhancedIdeology.Tests.Support;

internal sealed class SimPawn : Pawn
{
    public float ColonyMoodOffset;
    public float PersonalMoodNoise;
    public float BaseMood = 0.85f;

    private readonly Precept? _ideoAnchorPrecept;

    public SimPawn(Ideo ideology, float certainty, float conversionPower = 1f, float certaintyLossFactor = 1f, float baseMood = 0.85f)
    {
        ideo.SetIdeo(ideology);
        this.ideo.Certainty = certainty;
        BaseMood = baseMood;

        SetStatValue(StatDefOf.ConversionPower, conversionPower);
        SetStatValue(StatDefOf.CertaintyLossFactor, certaintyLossFactor);
        SetStatValue(StatDefOf.SocialImpact, 1f);
        SetStatValue(StatDefOf.SocialIdeoSpreadFrequencyFactor, 1f);

        _ideoAnchorPrecept = ideology.precepts.FirstOrDefault();
    }

    public void UpdateThoughts(float moodNoiseDelta = 0f, System.Random? rng = null)
    {
        if (rng != null)
            PersonalMoodNoise = (float)(PersonalMoodNoise * 0.95 + moodNoiseDelta * 0.05);

        var totalMoodOffset = ColonyMoodOffset + PersonalMoodNoise;
        needs.mood.thoughts.SimulatedThoughts.Clear();
        needs.mood.thoughts.SimulatedThoughts.Add(new SimThought
        {
            SourcePrecept = _ideoAnchorPrecept,
            MoodOffsetValue = totalMoodOffset,
        });
        needs.mood.CurLevelPercentage = Mathf.Clamp01(BaseMood + totalMoodOffset / 100f * 0.25f);
    }
}
