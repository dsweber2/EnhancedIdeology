namespace Verse;

public class ThoughtDef : Def
{
    public ThoughtWorker? Worker;
    public Type thoughtClass = typeof(Thought_Memory);
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
    public static Thought_Memory MakeThought(ThoughtDef def)
    {
        var thought = (Thought_Memory)Activator.CreateInstance(def.thoughtClass)!;
        thought.def = def;
        return thought;
    }

    public static Thought_Memory MakeThought(ThoughtDef def, RimWorld.Precept? sourcePrecept)
    {
        var thought = MakeThought(def);
        thought.sourcePrecept = sourcePrecept;
        return thought;
    }
}

public class MemoryThoughtHandler
{
    public List<Thought_Memory> Memories { get; } = [];
    public void TryGainMemory(ThoughtDef def, Pawn? otherPawn = null) => Memories.Add(ThoughtMaker.MakeThought(def));
    public void TryGainMemory(ThoughtDef def, Pawn? otherPawn, RimWorld.Precept? sourcePrecept) => Memories.Add(ThoughtMaker.MakeThought(def, sourcePrecept));
    public void TryGainMemory(Thought_Memory thought, Pawn? otherPawn = null) => Memories.Add(thought);
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
