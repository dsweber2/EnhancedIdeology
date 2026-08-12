namespace Verse;

public class ThoughtDef : Def
{
    public ThoughtWorker? Worker;
}

public abstract class ThoughtWorker { }

public abstract class Thought
{
    public ThoughtDef def = new();
    public virtual RimWorld.Precept? sourcePrecept => null;
    public virtual string LabelCap => def.LabelCap;
    public abstract float MoodOffset();
}

public class MemoryThoughtHandler
{
    public void TryGainMemory(ThoughtDef def, Pawn? otherPawn = null) { }
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
