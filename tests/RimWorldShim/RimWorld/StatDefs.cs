using Verse;

namespace RimWorld;

public class StatWorker
{
    public virtual bool IsDisabledFor(Pawn pawn) => false;
}

public class StatDef : Def
{
    public StatWorker Worker { get; } = new StatWorker();
    public float defaultBaseValue = 1f;
}

public class StatModifier
{
    public StatDef stat = new();
    public float value;
}

public static class StatUtility
{
    public static float GetStatOffsetFromList(this List<StatModifier>? modList, StatDef stat) =>
        modList?.FirstOrDefault(mod => mod.stat == stat)?.value ?? 0f;

    public static float GetStatFactorFromList(this List<StatModifier>? modList, StatDef stat) =>
        modList?.FirstOrDefault(mod => mod.stat == stat)?.value ?? 1f;
}

public static class StatDefOf
{
    public static readonly StatDef ConversionPower = new() { defName = "ConversionPower" };
    public static readonly StatDef CertaintyLossFactor = new() { defName = "CertaintyLossFactor" };
    public static readonly StatDef SocialImpact = new() { defName = "SocialImpact" };
    public static readonly StatDef SocialIdeoSpreadFrequencyFactor = new() { defName = "SocialIdeoSpreadFrequencyFactor" };
}

public static class SkillDefOf
{
    public static readonly SkillDef Social = new() { defName = "Social" };
    public static readonly SkillDef Intellectual = new() { defName = "Intellectual" };
    public static readonly SkillDef Melee = new() { defName = "Melee" };
}
