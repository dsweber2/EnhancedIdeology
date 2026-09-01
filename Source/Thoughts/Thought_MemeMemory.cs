namespace EnhancedIdeology;

public class Thought_MemeMemory : Thought_Memory
{
    public string? SourceMemeLabel;

    // Conviction delta applied each TickLong while this thought is active. Precomputed at gain time
    // from MoodOffset × CertaintyLossFactor × ConversionStancePull × MoodletConvictionScalar.
    public float ConvictionDeltaPerTickLong;

    public override string Description
    {
        get
        {
            var desc = base.Description;
            if (sourcePrecept != null || SourceMemeLabel.NullOrEmpty()) return desc;
            return desc + "\n\n" + "CausedBy".Translate() + ": " + SourceMemeLabel;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref SourceMemeLabel, "sourceMemeLabel");
        Scribe_Values.Look(ref ConvictionDeltaPerTickLong, "convictionDeltaPerTickLong");
    }
}
