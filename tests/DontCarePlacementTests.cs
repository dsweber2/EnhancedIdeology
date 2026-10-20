namespace EnhancedIdeology.Tests;

// Covers PreceptPolicy.DontCare: neutral is the vanilla baseline (the classic-mode stance), mandatory issues
// included, and every spec that names a mod rung still lands correctly when that mod is absent.
public class DontCarePlacementTests : SeededTest
{
    private static float NeutralOf(string issueName, params string[] rungsInDisplayOrder)
    {
        var issue = new IssueDef { defName = issueName };
        for (var ii = 0; ii < rungsInDisplayOrder.Length; ii++)
            SimIssues.Register(new PreceptDef { defName = rungsInDisplayOrder[ii], issue = issue, displayOrderInIssue = ii * 10 });
        return PreceptLadder.DontCareRank(issue);
    }

    [Fact]
    public void Slavery_NeutralIsAcceptable()
    {
        Assert.Equal(1f, NeutralOf("Slavery",
            "Slavery_Honorable", "Slavery_Acceptable", "Slavery_Disapproved", "Slavery_Horrible", "Slavery_Abhorrent"));
    }

    [Fact]
    public void Cannibalism_NeutralIsAbhorrent_AsVanillaRevulsion()
    {
        Assert.Equal(6f, NeutralOf("Cannibalism",
            "Cannibalism_RequiredRavenous", "Cannibalism_RequiredStrong", "Cannibalism_Preferred",
            "Cannibalism_Acceptable", "Cannibalism_Disapproved", "Cannibalism_Horrible", "Cannibalism_Abhorrent"));
    }

    [Fact]
    public void Execution_NeutralSitsJustBeforeHorribleIfInnocent()
    {
        Assert.Equal(2.5f, NeutralOf("Execution",
            "Execution_Required", "Execution_RespectedIfGuilty", "Execution_DontCare",
            "Execution_HorribleIfInnocent", "Execution_Horrible", "Execution_Abhorrent"));
    }

    [Fact]
    public void Corpses_NeutralIsTheClassicUglyRung()
    {
        Assert.Equal(0f, NeutralOf("Corpses", "Corpses_Ugly", "Corpses_DontCare", "AM_Corpses_Sublime"));
    }

    [Fact]
    public void Lovin_NeutralSitsPastFreeApproved()
    {
        Assert.Equal(0.5f, NeutralOf("Lovin",
            "Lovin_FreeApproved", "Lovin_SpouseOnly_Mild", "Lovin_SpouseOnly_Moderate", "Lovin_Prohibited"));
    }

    [Fact]
    public void Research_NeutralIsTheDefaultRung()
    {
        Assert.Equal(2f, NeutralOf("Research",
            "Research_VeryFast", "Research_Fast", "Research_Normal", "Research_Slow", "Research_None"));
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
    public void VehicleDriving_NeutralIsWhereTheClassicAllowedRungSits()
    {
        Assert.Equal(1.5f, NeutralOf("VVE_Driving",
            "VVE_Driving_Required", "VVE_Driving_Approved", "VVE_Driving_Horrible", "VVE_Driving_Forbidden"));
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
