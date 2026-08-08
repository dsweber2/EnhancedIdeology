namespace EnhancedBeliefs.Tests;

// Covers the per-issue precept ladder and the distance-based opinion falloff (design.md R2).
public class PreceptLadderTests : SeededTest
{
    private static PreceptDef Rung(IssueDef issue, string name, int order)
    {
        var rung = new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order };
        DefDatabase<PreceptDef>.Add(rung);
        return rung;
    }

    [Fact]
    public void Rungs_SortedByDisplayOrder_RegardlessOfRegistrationOrder()
    {
        var issue = new IssueDef { defName = "Cannibalism" };
        // Register scrambled; displayOrderInIssue is the source of truth.
        var abhorrent = Rung(issue, "Abhorrent", 50);
        var required = Rung(issue, "Required", 0);
        var acceptable = Rung(issue, "Acceptable", 20);

        var rungs = PreceptLadder.Rungs(issue);

        Assert.Equal(new[] { required, acceptable, abhorrent }, rungs);
    }

    [Fact]
    public void Rungs_ExcludesOtherIssues()
    {
        var meat = new IssueDef { defName = "Cannibalism" };
        var slavery = new IssueDef { defName = "Slavery" };
        var mine = Rung(meat, "Abhorrent", 50);
        Rung(slavery, "Honorable", 0);

        Assert.Equal(new[] { mine }, PreceptLadder.Rungs(meat));
    }

    [Fact]
    public void RankOf_IsIndexInLadder()
    {
        var issue = new IssueDef { defName = "Cannibalism" };
        Rung(issue, "Required", 0);
        var acceptable = Rung(issue, "Acceptable", 20);
        Rung(issue, "Abhorrent", 50);

        Assert.Equal(1f, PreceptLadder.RankOf(acceptable));
    }

    [Fact]
    public void DontCareRank_StubDefaultsToPermissiveExtreme()
    {
        Assert.Equal(-1f, PreceptLadder.DontCareRank(new IssueDef { defName = "AnimalSlaughter" }));
    }

    [Fact]
    public void OpinionOnPrecept_PeaksAtPreferredRung()
    {
        // target == preferred -> full +strength, anywhere on any ladder.
        Assert.Equal(4f, PreceptLadder.OpinionOnPrecept(2f, 2f, minRank: 0f, maxRank: 5f, strength: 4f, zeroFrac: 0.5f));
    }

    [Fact]
    public void OpinionOnPrecept_FarExtreme_ReachesNegativeStrength()
    {
        // Preferred Acceptable (rank 2) on a 0..5 ladder: far end is Abhorrent (rank 5) -> -strength.
        Assert.Equal(-4f, PreceptLadder.OpinionOnPrecept(2f, 5f, 0f, 5f, 4f, 0.5f), precision: 4);
    }

    [Fact]
    public void OpinionOnPrecept_NearExtreme_IsSofterThanFarExtreme()
    {
        // The asymmetry-of-magnitude: Acceptable-preferred pawn dislikes Required (near, rank 0)
        // less than Abhorrent (far, rank 5), because the near pole is closer to their view.
        var near = PreceptLadder.OpinionOnPrecept(2f, 0f, 0f, 5f, 4f, 0.5f);
        var far = PreceptLadder.OpinionOnPrecept(2f, 5f, 0f, 5f, 4f, 0.5f);

        Assert.True(near < 0f && far < 0f);
        Assert.True(near > far, $"near {near} should be softer (less negative) than far {far}");
        Assert.Equal(-4f / 3f, near, precision: 4); // t = 2/3, past zeroFrac -> -(0.667-0.5)/0.5 * 4
    }

    [Fact]
    public void OpinionOnPrecept_SymmetricInRungDistance()
    {
        // Equal rung distance on either side of the preferred rung -> equal opinion (distance is sign-blind).
        var down = PreceptLadder.OpinionOnPrecept(2f, 1f, 0f, 5f, 4f, 0.5f);
        var up = PreceptLadder.OpinionOnPrecept(2f, 3f, 0f, 5f, 4f, 0.5f);

        Assert.Equal(down, up, precision: 5);
    }

    [Fact]
    public void OpinionOnPrecept_CrossesZeroAtZeroFrac()
    {
        // With maxDist 4 (preferred at rank 0, ladder 0..4) and zeroFrac 0.5, zero-crossing is at distance 2.
        Assert.Equal(0f, PreceptLadder.OpinionOnPrecept(0f, 2f, 0f, 4f, 4f, 0.5f), precision: 5);
    }

    [Fact]
    public void OpinionOnPrecept_SingleRungIssue_IsPureAgreement()
    {
        // maxDist collapses to 0 -> holding the sole rung is agreement at full strength.
        Assert.Equal(4f, PreceptLadder.OpinionOnPrecept(0f, 0f, 0f, 0f, 4f, 0.5f));
    }

    [Fact]
    public void OpinionOnPrecept_ZeroFracShiftsCrossing()
    {
        // A smaller zeroFrac makes opinion turn negative sooner. At distance-fraction 0.5:
        //   zeroFrac 0.75 -> still positive; zeroFrac 0.25 -> already negative.
        var lenient = PreceptLadder.OpinionOnPrecept(0f, 2f, 0f, 4f, 4f, 0.75f);
        var strict = PreceptLadder.OpinionOnPrecept(0f, 2f, 0f, 4f, 4f, 0.25f);

        Assert.True(lenient > 0f);
        Assert.True(strict < 0f);
    }
}
