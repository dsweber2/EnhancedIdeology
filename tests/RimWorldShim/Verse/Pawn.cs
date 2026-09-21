using RimWorld;
using UnityEngine;

namespace Verse;

public class HediffCompProperties
{
    public Type? compClass;
}

public class HediffComp
{
    public HediffCompProperties props = new();
}

public class Hediff
{
    public T? TryGetComp<T>() where T : HediffComp => null;
}

public class HediffSet
{
    public Hediff? GetFirstHediffOfDef(RimWorld.HediffDef? def) => null;
}

public class Pawn_HealthTracker
{
    public readonly HediffSet hediffSet = new();
}

public class Pawn_NeedsTracker
{
    public readonly Need_Mood mood = new();
}

public class Need_Mood
{
    public float CurLevelPercentage = 0.85f;
    public float CurLevel = 0.85f;
    public readonly ThoughtHandler thoughts = new();
}

public class MentalState
{
    public Pawn pawn = null!;
}

public class MentalBreaker
{
    public float BreakThresholdMinor = 0.2f;
    public void TryDoRandomMoodCausedMentalBreak() { }
}

public class Pawn_StoryTracker
{
    public readonly TraitSet traits = new();
}

public class Pawn_MindState
{
    public readonly MentalStateHandler mentalStateHandler = new();
    public readonly MentalBreaker mentalBreaker = new();
}

public class Pawn_AgeTracker
{
    public float AgeBiologicalYearsFloat { get; set; } = 30f;
}

public class Pawn_GeneTracker
{
    private readonly HashSet<GeneDef> _activeGenes = [];

    public bool HasActiveGene(GeneDef? gene) => gene != null && _activeGenes.Contains(gene);

    public void AddGene(GeneDef gene) => _activeGenes.Add(gene);

    public bool UniqueXenotype { get; set; } = false;
    public string xenotypeName { get; set; } = string.Empty;
    public RimWorld.XenotypeDef? Xenotype { get; set; }
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
    public Pawn_HealthTracker health = new();
    public Pawn_RelationTracker relations;
    public Pawn_InteractionsTracker interactions = new();
    public Pawn_StoryTracker story = new();
    public Pawn_SkillTracker skills = new();
    public Pawn_MindState mindState = new();
    public Pawn_AgeTracker ageTracker = new();
    public Pawn_GeneTracker? genes;

    private readonly Dictionary<StatDef, float> _stats = [];

    public Pawn()
    {
        ideo = new Pawn_IdeoTracker(this);
        relations = new Pawn_RelationTracker(this);
    }

    public RimWorld.Ideo? Ideo => ideo.ideo;

    // All sim pawns share one sentinel map so SameLocalGroup sees them as co-located.
    private static readonly Map _simMap = new();

    public bool Spawned => false;
    public bool Destroyed => false;
    public bool IsPrisoner => false;
    public Map? Map => null;
    public Map? MapHeld => _simMap;
    public RimWorld.Caravan? GetCaravan() => null;
    public Vector3 DrawPos => Vector3.zero;
    public DevelopmentalStage DevelopmentalStage => DevelopmentalStage.Adult;
    public RaceProperties RaceProps => RaceProperties.Default;

    public MentalState? MentalState { get; set; }

    public bool IsHashIntervalTick(int interval) => false;

    public bool Inhumanized() => false;

    public bool WorkTagIsDisabled(WorkTags tag) => false;

    public float GetStatValue(StatDef def, bool applyPostProcess = true)
        => _stats.GetValueOrDefault(def, 1f);

    public void SetStatValue(StatDef def, float value) => _stats[def] = value;

    public override string ToString() => string.IsNullOrEmpty(Label) ? $"Pawn_{PawnId}" : Label;
}
