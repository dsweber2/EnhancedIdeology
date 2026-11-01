namespace Verse;

public class TraitDef : Def { }

public class Trait
{
    public TraitDef def = new();
    public int Degree;
    public bool Suppressed;
    public Dictionary<RimWorld.StatDef, float> statOffsets = [];
    public Dictionary<RimWorld.StatDef, float> statFactors = [];

    public float OffsetOfStat(RimWorld.StatDef stat) => statOffsets.GetValueOrDefault(stat, 0f);

    public float MultiplierOfStat(RimWorld.StatDef stat) => statFactors.GetValueOrDefault(stat, 1f);
}

public class TraitSet
{
    public List<Trait> allTraits = [];

    public bool HasTrait(TraitDef def) => allTraits.Any(t => t.def == def);

    public Trait? GetTrait(TraitDef def) => allTraits.FirstOrDefault(t => t.def == def);
}
