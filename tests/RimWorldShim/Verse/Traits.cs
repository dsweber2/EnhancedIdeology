namespace Verse;

public class TraitDef : Def { }

public class Trait
{
    public TraitDef def = new();
    public int Degree;
}

public class TraitSet
{
    public List<Trait> allTraits = [];

    public bool HasTrait(TraitDef def) => allTraits.Any(t => t.def == def);

    public Trait? GetTrait(TraitDef def) => allTraits.FirstOrDefault(t => t.def == def);
}
