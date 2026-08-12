using RimWorld;
using UnityEngine;

namespace Verse;

public class Pawn_NeedsTracker
{
    public readonly Need_Mood mood = new();
}

public class Need_Mood
{
    public float CurLevelPercentage = 0.85f;
    public readonly ThoughtHandler thoughts = new();
}

public class Pawn_StoryTracker
{
    public readonly TraitSet traits = new();
}

public class Pawn_MindState
{
    public readonly MentalStateHandler mentalStateHandler = new();
}

public class RaceProperties
{
    public bool Humanlike = true;
    public static readonly RaceProperties Default = new();
}

public class Pawn
{
    private static int _nextId;
    public readonly int PawnId = System.Threading.Interlocked.Increment(ref _nextId);

    public string Label = string.Empty;
    public string Name => Label;
    public string LabelShort => Label;

    public Pawn_IdeoTracker ideo;
    public Pawn_NeedsTracker needs = new();
    public Pawn_RelationTracker relations;
    public Pawn_InteractionsTracker interactions = new();
    public Pawn_StoryTracker story = new();
    public Pawn_SkillTracker skills = new();
    public Pawn_MindState mindState = new();

    private readonly Dictionary<StatDef, float> _stats = [];

    public Pawn()
    {
        ideo = new Pawn_IdeoTracker(this);
        relations = new Pawn_RelationTracker(this);
    }

    public RimWorld.Ideo? Ideo => ideo.ideo;

    public bool Spawned => false;
    public bool Destroyed => false;
    public bool IsPrisoner => false;
    public Map? Map => null;
    public Vector3 DrawPos => Vector3.zero;
    public DevelopmentalStage DevelopmentalStage => DevelopmentalStage.Adult;
    public RaceProperties RaceProps => RaceProperties.Default;

    public bool IsHashIntervalTick(int interval) => false;

    public bool Inhumanized() => false;

    public float GetStatValue(StatDef def, bool applyPostProcess = true)
        => _stats.GetValueOrDefault(def, 1f);

    public void SetStatValue(StatDef def, float value) => _stats[def] = value;

    public override string ToString() => string.IsNullOrEmpty(Label) ? $"Pawn_{PawnId}" : Label;
}
