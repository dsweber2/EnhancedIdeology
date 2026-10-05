namespace EnhancedIdeology.Tests.Support;

// A plain vanilla memory with a fixed mood offset, as vanilla gives for most precept thoughts.
internal sealed class SimMemory : Thought_Memory
{
    private readonly float _moodOffset;

    public SimMemory(Precept? source, float moodOffset, ThoughtDef? thoughtDef = null)
    {
        def = thoughtDef ?? new ThoughtDef();
        sourcePrecept = source;
        _moodOffset = moodOffset;
    }

    public override float MoodOffset() => _moodOffset;
}
