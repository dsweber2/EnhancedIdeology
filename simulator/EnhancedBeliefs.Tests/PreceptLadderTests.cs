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
    public void Rungs_ExcludesClassicModeDefaults()
    {
        // Classic-mode default precepts (Lovin_Free, Cannibalism_Classic, ...) carry an issue but duplicate a
        // real rung. Including them adds a phantom rung that shifts every rank and skews the falloff, so they
        // must be filtered out of the ladder.
        var issue = new IssueDef { defName = "Cannibalism" };
        var acceptable = Rung(issue, "Acceptable", 0);
        var abhorrent = Rung(issue, "Abhorrent", 50);
        var classic = Rung(issue, "Cannibalism_Classic", 0);
        classic.classic = true;

        Assert.Equal(new[] { acceptable, abhorrent }, PreceptLadder.Rungs(issue));
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
        // target == preferred -> full +strength, anywhere on any ladder, for any oppositionScale.
        Assert.Equal(4f, PreceptLadder.OpinionOnPrecept(2f, 2f, minRank: 0f, maxRank: 5f, strength: 4f, oppositionScale: 1f));
    }

    [Fact]
    public void OpinionOnPrecept_FarExtreme_ScalesWithOppositionScale()
    {
        // Preferred Acceptable (rank 2) on a 0..5 ladder: far end is Abhorrent (rank 5), t = 1. The opinion
        // there is exactly -oppositionScale·strength - and continuous through 0, no discontinuity at the top.
        Assert.Equal(-4f, PreceptLadder.OpinionOnPrecept(2f, 5f, 0f, 5f, 4f, oppositionScale: 1f), precision: 4);
        Assert.Equal(-2f, PreceptLadder.OpinionOnPrecept(2f, 5f, 0f, 5f, 4f, oppositionScale: 0.5f), precision: 4);
        Assert.Equal(0f, PreceptLadder.OpinionOnPrecept(2f, 5f, 0f, 5f, 4f, oppositionScale: 0f), precision: 5);
    }

    [Fact]
    public void OpinionOnPrecept_NearExtreme_IsSofterThanFarExtreme()
    {
        // The asymmetry-of-magnitude: Acceptable-preferred pawn dislikes Required (near, rank 0)
        // less than Abhorrent (far, rank 5), because the near pole is closer to their view.
        var near = PreceptLadder.OpinionOnPrecept(2f, 0f, 0f, 5f, 4f, oppositionScale: 1f);
        var far = PreceptLadder.OpinionOnPrecept(2f, 5f, 0f, 5f, 4f, oppositionScale: 1f);

        Assert.True(near < 0f && far < 0f);
        Assert.True(near > far, $"near {near} should be softer (less negative) than far {far}");
        Assert.Equal(-4f / 3f, near, precision: 4); // maxDist 3, t = 2/3, falloff 1-2t -> -1/3 * 4
    }

    [Fact]
    public void OpinionOnPrecept_SymmetricInRungDistance()
    {
        // Equal rung distance on either side of the preferred rung -> equal opinion (distance is sign-blind).
        var down = PreceptLadder.OpinionOnPrecept(2f, 1f, 0f, 5f, 4f, oppositionScale: 1f);
        var up = PreceptLadder.OpinionOnPrecept(2f, 3f, 0f, 5f, 4f, oppositionScale: 1f);

        Assert.Equal(down, up, precision: 5);
    }

    [Fact]
    public void OpinionOnPrecept_CrossesZeroAtExpectedDistance()
    {
        // Zero-crossing sits at t = 1/(1+oppositionScale). On a 0..4 ladder (maxDist 4) that is distance 2 at
        // oppositionScale 1, and distance 3 at oppositionScale 1/3.
        Assert.Equal(0f, PreceptLadder.OpinionOnPrecept(0f, 2f, 0f, 4f, 4f, oppositionScale: 1f), precision: 5);
        Assert.Equal(0f, PreceptLadder.OpinionOnPrecept(0f, 3f, 0f, 4f, 4f, oppositionScale: 1f / 3f), precision: 5);
    }

    [Fact]
    public void OpinionOnPrecept_SingleRungIssue_IsPureAgreement()
    {
        // maxDist collapses to 0 -> holding the sole rung is agreement at full strength.
        Assert.Equal(4f, PreceptLadder.OpinionOnPrecept(0f, 0f, 0f, 0f, 4f, oppositionScale: 1f));
    }

    [Fact]
    public void OpinionOnPrecept_OppositionScaleScalesOppositionLinearly()
    {
        // The knob is the opposite-extreme opinion, applied linearly: halving it halves the opinion at every
        // rung past the pawn's own, with no discontinuity as it approaches 0.
        var full = PreceptLadder.OpinionOnPrecept(0f, 3f, 0f, 4f, 4f, oppositionScale: 1f);
        var half = PreceptLadder.OpinionOnPrecept(0f, 3f, 0f, 4f, 4f, oppositionScale: 0.5f);
        var none = PreceptLadder.OpinionOnPrecept(0f, 3f, 0f, 4f, 4f, oppositionScale: 0f);

        Assert.True(full < 0f, $"expected opposition, got {full}");
        Assert.True(none > 0f, $"at oppositionScale 0 opinion only fades toward 0, got {none}");
        Assert.Equal(full, 2f * half - none, precision: 4); // linear in oppositionScale
    }
}
