namespace EnhancedIdeology.Tests.Support;

internal sealed class SimThought : Verse.Thought
{
    public Precept? SourcePrecept;
    public float MoodOffsetValue;

    public override Precept? sourcePrecept => SourcePrecept;
    public override float MoodOffset() => MoodOffsetValue;
}
