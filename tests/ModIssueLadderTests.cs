namespace EnhancedIdeology.Tests;

// Covers the War Crimes Expanded 2, Humanoid Alien Races, and Genetic Rim issues: category, rung order from the
// display orders the mods ship, and the neutral rank their default rungs give (docs/preceptPolicy.md "Mod issues").
public class ModIssueLadderTests : SeededTest
{
    private static IssueDef Register(string issueName, params (string name, int order, float weight, bool visible)[] rungs)
    {
        var issue = new IssueDef { defName = issueName };
        foreach (var (name, order, weight, visible) in rungs)
        {
            SimIssues.Register(new PreceptDef
            {
                defName = name, issue = issue, displayOrderInIssue = order, defaultSelectionWeight = weight, visible = visible,
            });
        }
        return issue;
    }

    private static string[] RungNames(IssueDef issue) => PreceptLadder.Rungs(issue).Select(rung => rung.defName).ToArray();

    [Theory]
    [InlineData("WCE2_Torture")]
    [InlineData("HAR_EatingAliens")]
    [InlineData("HAR_AlienDating")]
    [InlineData("HAR_AlienRaces")]
    [InlineData("HAR_AlienSlavery")]
    [InlineData("GR_Failures")]
    [InlineData("GR_HumanoidHybrids")]
    public void CategoryOf_ValueAxisModIssues_AreMoral(string issueName)
    {
        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(new IssueDef { defName = issueName }));
    }

    [Fact]
    public void CategoryOf_GeneticRimWorktableSpeeds_IsPositiveOnly()
    {
        Assert.Equal(PreceptCategory.PositiveOnly, PreceptPolicy.CategoryOf(new IssueDef { defName = "GR_WorktableSpeeds" }));
    }

    [Fact]
    public void Torture_TiedRungsAreOrdered_AndNeutralIsHorribleIfInnocent()
    {
        var issue = Register("WCE2_Torture",
            ("WCE2_Torture_Vanilla", 0, 0f, false), ("WCE2_Torture_Abhorrent", 50, 0f, true),
            ("WCE2_Torture_Horrible", 40, 0f, true), ("WCE2_Torture_HorribleInnocent", 40, 1f, true),
            ("WCE2_Torture_DontCare", 20, 0f, true), ("WCE2_Torture_RespectedGuilty", 10, 0f, true),
            ("WCE2_Torture_AlwaysRespected", 0, 0f, true));

        Assert.Equal(
            ["WCE2_Torture_AlwaysRespected", "WCE2_Torture_RespectedGuilty", "WCE2_Torture_DontCare",
             "WCE2_Torture_HorribleInnocent", "WCE2_Torture_Horrible", "WCE2_Torture_Abhorrent"],
            RungNames(issue));
        Assert.Equal(3f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void AlienDating_AcceptableSitsBetweenProAndAntiRungs()
    {
        var issue = Register("HAR_AlienDating",
            ("HAR_AlienDating_Prohibited", 60, 0f, true), ("HAR_AlienDating_Horrible", 50, 0f, true),
            ("HAR_AlienDating_Acceptable", 0, 0f, true), ("HAR_AlienDating_Preferred", 10, 0f, true),
            ("HAR_AlienDating_Honorable", 20, 0f, true));

        Assert.Equal(
            ["HAR_AlienDating_Honorable", "HAR_AlienDating_Preferred", "HAR_AlienDating_Acceptable",
             "HAR_AlienDating_Horrible", "HAR_AlienDating_Prohibited"],
            RungNames(issue));
        Assert.Equal(2f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void EatingAliens_NeutralIsTheDefaultAbhorrent()
    {
        var issue = Register("HAR_EatingAliens",
            ("HAR_EatingAliens_Abhorrent", 50, 1f, true), ("HAR_EatingAliens_Disapproved", 30, 0f, true),
            ("HAR_EatingAliens_Acceptable", 20, 0f, true), ("HAR_EatingAliens_Preferred", 10, 0f, true),
            ("HAR_EatingAliens_RequiredStrong", 0, 0f, true));

        Assert.Equal(4f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void AlienRaces_NeutralIsTheDefaultStandard()
    {
        var issue = Register("HAR_AlienRaces",
            ("HAR_AlienRaces_Abhorrent", 60, 0f, true), ("HAR_AlienRaces_Horrible", 50, 0f, true),
            ("HAR_AlienRaces_Standard", 30, 1f, true), ("HAR_AlienRaces_Respected", 10, 0f, true),
            ("HAR_AlienRaces_Exalted", 0, 0f, true));

        Assert.Equal(2f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void AlienSlavery_NeutralIsTheMeanOfFourDefaults()
    {
        var issue = Register("HAR_AlienSlavery",
            ("HAR_AlienSlavery_Abhorrent", 40, 1f, true), ("HAR_AlienSlavery_Horrible", 30, 1f, true),
            ("HAR_AlienSlavery_Disapproved", 20, 1f, true), ("HAR_AlienSlavery_Acceptable", 10, 1f, true),
            ("HAR_AlienSlavery_Honorable", 0, 0f, true));

        Assert.Equal(2.5f, PreceptLadder.DontCareRank(issue));
    }

    [Theory]
    [InlineData("GR_Failures", "GR_Failures_Shameful")]
    [InlineData("GR_HumanoidHybrids", "GR_HumanoidHybrids_Forbidden")]
    public void GeneticRim_UnorderedRungsRunFromAccepted_AndNeutralIsTheDefaultDisliked(string issueName, string strictest)
    {
        var accepted = issueName + "_Accepted";
        var disliked = issueName + "_Disliked";
        var issue = Register(issueName, (strictest, 0, 0f, true), (disliked, 0, 1f, true), (accepted, 0, 0f, true));

        Assert.Equal([accepted, disliked, strictest], RungNames(issue));
        Assert.Equal(1f, PreceptLadder.DontCareRank(issue));
    }
}
