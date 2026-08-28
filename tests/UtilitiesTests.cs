namespace EnhancedIdeology.Tests;

public class UtilitiesTests : SeededTest
{
    private sealed class ConcreteComp : PreceptComp { }
    private sealed class OtherComp : PreceptComp { }

    // --- TryGetComps ---

    [Fact]
    public void TryGetComps_PreceptDef_NoComps_ReturnsEmpty()
    {
        var def = new PreceptDef();
        Assert.Empty(def.TryGetComps<ConcreteComp>());
    }

    [Fact]
    public void TryGetComps_PreceptDef_WithMatchingComp_ReturnsIt()
    {
        var def = new PreceptDef();
        var comp = new ConcreteComp();
        def.AddComp(comp);
        var result = def.TryGetComps<ConcreteComp>();
        Assert.Single(result);
        Assert.Same(comp, result[0]);
    }

    [Fact]
    public void TryGetComps_PreceptDef_FiltersToCorrectType()
    {
        var def = new PreceptDef();
        def.AddComp(new ConcreteComp());
        def.AddComp(new OtherComp());
        Assert.Single(def.TryGetComps<ConcreteComp>());
        Assert.Single(def.TryGetComps<OtherComp>());
    }

    [Fact]
    public void TryGetComps_Precept_DelegatesToDef()
    {
        var def = new PreceptDef();
        def.AddComp(new ConcreteComp());
        var precept = new Precept { def = def };
        Assert.Single(precept.TryGetComps<ConcreteComp>());
    }

    // --- ApostacyStrictness ---

    [Fact]
    public void ApostacyStrictness_NullIdeo_ReturnsZero()
    {
        Assert.Equal(0f, EnhancedIdeologyUtilities.ApostacyStrictness(null));
    }

    [Fact]
    public void ApostacyStrictness_NoApostasyIssueRegistered_ReturnsZero()
    {
        // DefDatabase has no "Apostasy" issue — the lookup returns null and the method early-outs.
        var ideo = new IdeoBuilder().WithName("NoApostasy").Build();
        Assert.Equal(0f, EnhancedIdeologyUtilities.ApostacyStrictness(ideo));
    }

    [Fact]
    public void ApostacyStrictness_IdeoHasNoApostasyPrecept_ReturnsZero()
    {
        SimIssues.Ladder("Apostasy", "VME_Apostasy_Accepted", "Apostasy_Disapproved", "ApostasyStrict");
        var ideo = new IdeoBuilder().WithName("NoApostasyPrecept").Build();
        Assert.Equal(0f, EnhancedIdeologyUtilities.ApostacyStrictness(ideo));
    }

    [Fact]
    public void ApostacyStrictness_PermissiveRung_ReturnsZero()
    {
        // Rung 0 ("VME_Apostasy_Accepted") is below DontCareRank (0.5), so Clamp01 floors it to 0.
        var (_, rungs) = SimIssues.Ladder("Apostasy", "VME_Apostasy_Accepted", "Apostasy_Disapproved", "ApostasyStrict");
        var ideo = new IdeoBuilder().WithName("Permissive").AddPrecept(rungs[0]).Build();
        Assert.Equal(0f, EnhancedIdeologyUtilities.ApostacyStrictness(ideo));
    }

    [Fact]
    public void ApostacyStrictness_StrictestRung_ReturnsOne()
    {
        // Rung 2 ("ApostasyStrict") is the max; (2 - 0.5) / (2 - 0.5) = 1.0.
        var (_, rungs) = SimIssues.Ladder("Apostasy", "VME_Apostasy_Accepted", "Apostasy_Disapproved", "ApostasyStrict");
        var ideo = new IdeoBuilder().WithName("Strict").AddPrecept(rungs[2]).Build();
        Assert.Equal(1f, EnhancedIdeologyUtilities.ApostacyStrictness(ideo), precision: 5);
    }

    [Fact]
    public void ApostacyStrictness_MiddleRung_ReturnsFractional()
    {
        // Rung 1 ("Apostasy_Disapproved") is at rank 1, dontCareRank 0.5, max 2: (1-0.5)/(2-0.5) ≈ 0.333.
        var (_, rungs) = SimIssues.Ladder("Apostasy", "VME_Apostasy_Accepted", "Apostasy_Disapproved", "ApostasyStrict");
        var ideo = new IdeoBuilder().WithName("Middle").AddPrecept(rungs[1]).Build();
        var strictness = EnhancedIdeologyUtilities.ApostacyStrictness(ideo);
        Assert.True(strictness > 0f && strictness < 1f,
            $"Expected strictness in (0, 1) for middle rung, got {strictness}");
    }
}
