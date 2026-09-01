namespace Verse;

public class ThoughtDef : Def
{
    public ThoughtWorker? Worker;
}

public abstract class ThoughtWorker { }

public abstract class Thought
{
    public ThoughtDef def = new();
    public virtual RimWorld.Precept? sourcePrecept { get; set; }
    public virtual string LabelCap => def.LabelCap;
    public abstract float MoodOffset();
}

public class Thought_Memory : Thought
{
    public override float MoodOffset() => 0f;
    public virtual string Description => def.label ?? string.Empty;
    public virtual bool TryMergeWithExistingMemory(out bool showBubble) { showBubble = true; return false; }
    public virtual void ExposeData() { }
}

public static class ThoughtMaker
{
    public static Thought_Memory MakeThought(ThoughtDef def) => new();
    public static Thought_Memory MakeThought(ThoughtDef def, RimWorld.Precept? sourcePrecept) => new() { sourcePrecept = sourcePrecept };
}

public class MemoryThoughtHandler
{
    public List<Thought_Memory> Memories { get; } = [];
    public void TryGainMemory(ThoughtDef def, Pawn? otherPawn = null) { }
    public void TryGainMemory(ThoughtDef def, Pawn? otherPawn, RimWorld.Precept? sourcePrecept) { }
    public void TryGainMemory(Thought_Memory thought, Pawn? otherPawn = null) { }
}

public class ThoughtHandler
{
    public readonly List<Thought> SimulatedThoughts = [];
    public readonly MemoryThoughtHandler memories = new();

    public void GetAllMoodThoughts(List<Thought> outThoughts)
    {
        outThoughts.AddRange(SimulatedThoughts);
    }
}
