namespace EnhancedIdeology.Tests;

// Covers PreceptLadder.DontCareRank: an issue with default rungs takes their weighted mean rank, and every other
// issue uses PreceptPolicy.DontCare, where each spec that names a mod rung still lands correctly when that mod is
// absent.
public class DontCarePlacementTests : SeededTest
{
    private static float NeutralOf(string issueName, params string[] rungsInDisplayOrder)
    {
        var issue = new IssueDef { defName = issueName };
        for (var ii = 0; ii < rungsInDisplayOrder.Length; ii++)
            SimIssues.Register(new PreceptDef { defName = rungsInDisplayOrder[ii], issue = issue, displayOrderInIssue = ii * 10 });
        return PreceptLadder.DontCareRank(issue);
    }

    // Rungs in display order with their defaultSelectionWeight, as vanilla ships them.
    private static float DefaultNeutralOf(string issueName, params (string name, float weight)[] rungsInDisplayOrder)
    {
        var issue = new IssueDef { defName = issueName };
        for (var ii = 0; ii < rungsInDisplayOrder.Length; ii++)
        {
            var (name, weight) = rungsInDisplayOrder[ii];
            SimIssues.Register(new PreceptDef
            {
                defName = name, issue = issue, displayOrderInIssue = ii * 10, defaultSelectionWeight = weight,
            });
        }
        return PreceptLadder.DontCareRank(issue);
    }

    [Fact]
    public void SingleDefaultRung_IsTheNeutral()
    {
        Assert.Equal(3f, DefaultNeutralOf("Execution",
            ("Execution_Required", 0f), ("Execution_RespectedIfGuilty", 0f), ("Execution_DontCare", 0f),
            ("Execution_HorribleIfInnocent", 1f), ("Execution_Horrible", 0f), ("Execution_Abhorrent", 0f)));
    }

    [Fact]
    public void SeveralDefaultRungs_GiveTheirMeanRank()
    {
        Assert.Equal(2.5f, DefaultNeutralOf("Slavery",
            ("Slavery_Honorable", 0f), ("Slavery_Acceptable", 1f), ("Slavery_Disapproved", 1f),
            ("Slavery_Horrible", 1f), ("Slavery_Abhorrent", 1f)));
    }

    [Fact]
    public void DefaultRungs_AreWeightedByDefaultSelectionWeight()
    {
        Assert.Equal(0.25f, DefaultNeutralOf("WeightedIssue",
            ("WeightedIssue_A", 3f), ("WeightedIssue_B", 1f), ("WeightedIssue_C", 0f)));
    }

    [Fact]
    public void InvisibleDefaultRung_IsIgnored()
    {
        var issue = new IssueDef { defName = "HiddenDefaultIssue" };
        SimIssues.Register(new PreceptDef { defName = "HiddenDefault_A", issue = issue, displayOrderInIssue = 0 });
        SimIssues.Register(new PreceptDef { defName = "HiddenDefault_B", issue = issue, displayOrderInIssue = 10, defaultSelectionWeight = 1f });
        SimIssues.Register(new PreceptDef
        {
            defName = "HiddenDefault_Classic", issue = issue, displayOrderInIssue = 20, defaultSelectionWeight = 1f, visible = false,
        });

        Assert.Equal(1f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void TableEntry_OverridesTheDefaultRungs()
    {
        // HAR_AlienDating has a table entry at Acceptable; it wins over a default rung elsewhere.
        Assert.Equal(2f, DefaultNeutralOf("HAR_AlienDating",
            ("HAR_AlienDating_Acceptable", 0f), ("HAR_AlienDating_Preferred", 0f), ("HAR_AlienDating_Honorable", 0f),
            ("HAR_AlienDating_Horrible", 0f), ("HAR_AlienDating_Prohibited", 1f)));
    }

    [Fact]
    public void Lovin_FreeIsALadderRung_AndNeutralIsTheMeanOfFourDefaults()
    {
        var issue = new IssueDef { defName = "Lovin" };
        foreach (var (name, order, weight, classic) in new (string, int, float, bool)[]
        {
            ("Lovin_Free", 0, 1f, true), ("Lovin_FreeApproved", 0, 0f, false), ("Lovin_SpouseOnly_Mild", 10, 1f, false),
            ("Lovin_SpouseOnly_Moderate", 20, 1f, false), ("Lovin_SpouseOnly_Strict", 30, 1f, false),
            ("Lovin_Horrible", 40, 0f, false), ("Lovin_Prohibited", 50, 0f, false),
        })
        {
            SimIssues.Register(new PreceptDef
            {
                defName = name, issue = issue, displayOrderInIssue = order, defaultSelectionWeight = weight, classic = classic,
            });
        }

        Assert.Equal(1f, PreceptLadder.RankOfName(issue, "Lovin_Free"));
        Assert.Equal(2.5f, PreceptLadder.DontCareRank(issue));
    }

    [Theory]
    [InlineData(true, 2.5f)]
    [InlineData(false, 1.5f)]
    public void Raiding_NeutralSitsPastTheMildestRaider(bool withVme, float expected)
    {
        var rungs = withVme
            ? new[] { "Raiding_Required", "Raiding_Respected", "VME_Raiding_Honorable", "VME_Raiding_Abhorrent" }
            : ["Raiding_Required", "Raiding_Respected"];
        Assert.Equal(expected, NeutralOf("Raiding", rungs));
    }

    [Theory]
    [InlineData(true, 0.5f)]
    [InlineData(false, -0.5f)]
    public void Apostasy_NeutralNeverCollapsesOntoDisapproved(bool withVme, float expected)
    {
        var rungs = withVme
            ? new[] { "VME_Apostasy_Accepted", "Apostasy_Disapproved", "Apostasy_Horrible" }
            : ["Apostasy_Disapproved", "Apostasy_Horrible"];
        Assert.Equal(expected, NeutralOf("Apostasy", rungs));
    }

    [Theory]
    [InlineData(true, 2.5f)]
    [InlineData(false, 0.5f)]
    public void Ranching_NeutralSitsPastTheMildestProRung(bool withMods, float expected)
    {
        var rungs = withMods
            ? new[] { "Ranching_Central", "AM_Ranching_CattleCentered", "VME_Ranching_Nomadic", "VME_Ranching_Discouraged", "VME_Ranching_Disliked" }
            : ["Ranching_Central"];
        Assert.Equal(expected, NeutralOf("Ranching", rungs));
    }

    [Fact]
    public void Ranching_DiscouragedIsMilderThanDisliked()
    {
        var issue = new IssueDef { defName = "Ranching" };
        foreach (var name in new[] { "Ranching_Central", "VME_Ranching_Disliked", "VME_Ranching_Discouraged" })
            SimIssues.Register(new PreceptDef { defName = name, issue = issue });

        Assert.Equal(
            ["Ranching_Central", "VME_Ranching_Discouraged", "VME_Ranching_Disliked"],
            PreceptLadder.Rungs(issue).Select(rung => rung.defName));
    }

    [Fact]
    public void VehicleDriving_NeutralIsTheClassicAllowedRung()
    {
        Assert.Equal(2f, DefaultNeutralOf("VVE_Driving",
            ("VVE_Driving_Required", 0f), ("VVE_Driving_Approved", 0f), ("VVE_Driving_Allowed", 1f),
            ("VVE_Driving_Horrible", 0f), ("VVE_Driving_Forbidden", 0f)));
    }

    [Fact]
    public void Comfort_IgnoredSitsBetweenDiscomfortAndNeutral()
    {
        var issue = new IssueDef { defName = "Comfort" };
        foreach (var name in new[] { "Comfort_Ignored", "AM_Comfort_DiscomfortPreferred", "Comfort_Wanted" })
            SimIssues.Register(new PreceptDef { defName = name, issue = issue });

        Assert.Equal(
            ["AM_Comfort_DiscomfortPreferred", "Comfort_Ignored", "Comfort_Wanted"],
            PreceptLadder.Rungs(issue).Select(rung => rung.defName));
        Assert.Equal(1.5f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void Indoors_RequiredSitsPastAcceptable()
    {
        var issue = new IssueDef { defName = "Indoors" };
        foreach (var name in new[] { "Indoors_Acceptable", "VVE_SmallSpaces_Horrible", "AM_Indoors_Required" })
            SimIssues.Register(new PreceptDef { defName = name, issue = issue });

        Assert.Equal(
            ["AM_Indoors_Required", "Indoors_Acceptable", "VVE_SmallSpaces_Horrible"],
            PreceptLadder.Rungs(issue).Select(rung => rung.defName));
        Assert.Equal(1.5f, PreceptLadder.DontCareRank(issue));
    }

    [Theory]
    [InlineData("RomanceOnTheRim_Issue_Cheat", "RomanceOnTheRim_Cheat")]
    [InlineData("RomanceOnTheRim_Issue_Breakup", "RomanceOnTheRim_Breakup")]
    [InlineData("RomanceOnTheRim_Issue_RomanceAttempt", "RomanceOnTheRim_RomanceAttempt")]
    public void RomanceOnTheRim_NeutralSitsBetweenEncouragedAndDiscouraged(string issueName, string prefix)
    {
        Assert.Equal(0.5f, NeutralOf(issueName,
            $"{prefix}_Encouraged", $"{prefix}_Discouraged", $"{prefix}_Forbidden"));
    }

    [Fact]
    public void Or_KeepsARealRankBelowZero()
    {
        var issue = new IssueDef { defName = "OrTestIssue" };
        foreach (var (name, order) in new[] { ("OrTest_A", 0), ("OrTest_B", 10) })
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order });

        var spec = DontCareSpec.Before("OrTest_A").Or(DontCareSpec.After("OrTest_B"));

        Assert.Equal(-0.5f, spec.Resolve(issue));
    }
}
