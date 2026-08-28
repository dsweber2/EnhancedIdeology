namespace EnhancedIdeology.Tests;

public class BrainwipeTests : SeededTest
{
    [Fact]
    public void Properties_Constructor_SetsCompClass()
    {
        var props = new HediffCompProperties_BrainwipeRecovery();
        Assert.Equal(typeof(HediffComp_BrainwipeRecovery), props.compClass);
    }

    [Fact]
    public void Comp_SusceptibilityMultiplier_DefaultIsTwo_Five()
    {
        var comp = new HediffComp_BrainwipeRecovery { props = new HediffCompProperties_BrainwipeRecovery() };
        Assert.Equal(2.5f, comp.SusceptibilityMultiplier);
    }

    [Fact]
    public void Comp_SusceptibilityMultiplier_ReturnsCustomValue()
    {
        var props = new HediffCompProperties_BrainwipeRecovery { susceptibilityMultiplier = 4f };
        var comp = new HediffComp_BrainwipeRecovery { props = props };
        Assert.Equal(4f, comp.SusceptibilityMultiplier);
    }
}
