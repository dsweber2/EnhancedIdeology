namespace EnhancedIdeology;

public class Thought_CognitiveDissonance : Thought_Memory
{
    public float StoredMoodOffset;

    public override float MoodOffset() => StoredMoodOffset;

    // Each trigger is its own entry so the offset stays accurate per event.
    public override bool TryMergeWithExistingMemory(out bool showBubble)
    {
        showBubble = false;
        return false;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref StoredMoodOffset, "storedMoodOffset");
    }
}
