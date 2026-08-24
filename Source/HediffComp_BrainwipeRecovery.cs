using Verse;

namespace EnhancedIdeology;

public class HediffCompProperties_BrainwipeRecovery : HediffCompProperties
{
#pragma warning disable CA1051 // RimWorld XML deserialization requires public fields
    public float susceptibilityMultiplier = 2.5f;
#pragma warning restore CA1051

    public HediffCompProperties_BrainwipeRecovery()
    {
        compClass = typeof(HediffComp_BrainwipeRecovery);
    }
}

public class HediffComp_BrainwipeRecovery : HediffComp
{
    public float SusceptibilityMultiplier => ((HediffCompProperties_BrainwipeRecovery)props).susceptibilityMultiplier;
}
