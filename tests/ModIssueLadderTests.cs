namespace EnhancedIdeology.Tests;

// Covers the War Crimes Expanded 2, Humanoid Alien Races, Vanilla Genetics Expanded, Suffering Expanded,
// ReSplice: Charmweavers, and Altered Carbon 2 issues: category, rung
// order from the display orders the mods ship, and the neutral rank their default rungs give
// (docs/preceptPolicy.md "Mod issues").
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

    [Theory]
    [InlineData("SUF_Torture")]
    [InlineData("VaccinePolicy")]
    [InlineData("VQE_ARCGenerators")]
    [InlineData("BGM_FungalSymbiosis")]
    [InlineData("RS_Issue_Divorce")]
    [InlineData("RS_Issue_Lovehexer")]
    [InlineData("RS_Issue_Marriage")]
    [InlineData("RS_Issue_Pregnancy")]
    [InlineData("AC_Stacking")]
    [InlineData("AC_Sleeving")]
    [InlineData("AC_CrossSleeving")]
    [InlineData("AC_StackDuplicating")]
    [InlineData("AC_XenoSleeving")]
    [InlineData("VFEP_Camaraderie")]
    [InlineData("ROBTRG_Hospitality")]
    public void CategoryOf_ReportedValueAxisModIssues_AreMoral(string issueName)
    {
        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(new IssueDef { defName = issueName }));
    }

    [Theory]
    [InlineData("GR_WorktableSpeeds")]
    [InlineData("PA_AmmoHandling")]
    [InlineData("AP_TrophyHunterPreceptIssue")]
    public void CategoryOf_StatAndStyleModIssues_ArePositiveOnly(string issueName)
    {
        Assert.Equal(PreceptCategory.PositiveOnly, PreceptPolicy.CategoryOf(new IssueDef { defName = issueName }));
    }

    [Fact]
    public void SufferingTorture_PenanceTiesToHorrible_AndNeutralIsHorribleOverTheDefaultDontCare()
    {
        var issue = Register("SUF_Torture",
            ("SUF_Torture_Classic", 0, 0f, false), ("SUF_Torture_Penance", 0, 0f, true),
            ("SUF_Torture_Sacrament", 5, 0f, true), ("SUF_Torture_Required", 10, 0f, true),
            ("SUF_Torture_Appreciated", 20, 0f, true), ("SUF_Torture_DontCare", 30, 1f, true),
            ("SUF_Torture_HorribleIfInnocent", 40, 0f, true), ("SUF_Torture_Horrible", 50, 0f, true),
            ("SUF_Torture_Abhorrent", 60, 0f, true));

        Assert.Equal(
            ["SUF_Torture_Sacrament", "SUF_Torture_Required", "SUF_Torture_Appreciated", "SUF_Torture_DontCare",
             "SUF_Torture_HorribleIfInnocent", "SUF_Torture_Horrible", "SUF_Torture_Abhorrent"],
            RungNames(issue));
        Assert.Equal(5f, PreceptLadder.DontCareRank(issue));
        Assert.Equal(PreceptLadder.RankOfName(issue, "SUF_Torture_Horrible"), PreceptLadder.RankOfName(issue, "SUF_Torture_Penance"));
    }

    [Fact]
    public void DescribeRungs_ListsEveryPreceptInLadderOrder_WithItsSource()
    {
        var issue = new IssueDef { defName = "SUF_Torture" };
        var suffering = new ModContentPack { Name = "Suffering Expanded" };
        SimIssues.Register(new PreceptDef
        {
            defName = "SUF_Torture_Horrible", label = "always horrible", issue = issue, displayOrderInIssue = 50,
            visible = true, modContentPack = suffering, fileName = "Precepts_Torture.xml",
        });
        SimIssues.Register(new PreceptDef { defName = "SUF_Torture_Extra", issue = issue, displayOrderInIssue = 70, visible = true });
        SimIssues.Register(new PreceptDef
        {
            defName = "SUF_Torture_Penance", issue = issue, visible = true, modContentPack = suffering, fileName = "Precepts_Torture.xml",
        });
        SimIssues.Register(new PreceptDef { defName = "SUF_Torture_Classic", issue = issue, visible = false, modContentPack = suffering });

        var lines = PreceptPolicy.DescribeRungs(issue).Split('\n');

        Assert.Equal(4, lines.Length);
        Assert.Equal("  - SUF_Torture_Horrible 'always horrible' (rank 0, order 50): Suffering Expanded, Precepts_Torture.xml", lines[0]);
        Assert.Contains("SUF_Torture_Extra", lines[1]);
        Assert.Contains("no mod: added by an XML patch", lines[1]);
        Assert.Contains("tied to SUF_Torture_Horrible", lines[2]);
        Assert.Contains("SUF_Torture_Classic", lines[3]);
        Assert.Contains("(hidden,", lines[3]);
    }

    [Fact]
    public void Pregnancy_DesiredLeads_AndNeutralIsDontCareOverTheWeightedDefaults()
    {
        var issue = Register("RS_Issue_Pregnancy",
            ("RS_Pregnancy_DontCare", 0, 1f, true), ("RS_Pregnancy_Abhorrent", 30, 0.5f, true),
            ("RS_Pregnancy_Desired", 50, 0f, true));

        Assert.Equal(["RS_Pregnancy_Desired", "RS_Pregnancy_DontCare", "RS_Pregnancy_Abhorrent"], RungNames(issue));
        Assert.Equal(1f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void Divorce_GenderedRungsTie_AndTheDontCareEntryOverridesTheDefaultsMean()
    {
        var issue = Register("RS_Issue_Divorce",
            ("RS_Divorce_Acceptable", 0, 1f, true), ("RS_Divorce_MaleOnly", 30, 0f, true),
            ("RS_Divorce_FemaleOnly", 30, 0f, true), ("RS_Divorce_Horrible", 35, 1f, true),
            ("RS_Divorce_Abhorrent", 45, 0f, true), ("RS_Divorce_Forbidden", 55, 0f, true));

        Assert.Equal(
            ["RS_Divorce_Acceptable", "RS_Divorce_MaleOnly", "RS_Divorce_Horrible", "RS_Divorce_Abhorrent", "RS_Divorce_Forbidden"],
            RungNames(issue));
        Assert.Equal(1f, PreceptLadder.RankOfName(issue, "RS_Divorce_FemaleOnly"));
        // The Acceptable and Horrible defaults alone average to 1.
        Assert.Equal(1.5f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void AlteredCarbonStacking_CentralLeads_AndNeutralSitsBetweenPreferredAndDespised()
    {
        var issue = Register("AC_Stacking",
            ("AC_Stacking_Central", 0, 0f, true), ("AC_Stacking_Preferred", 0, 0f, true),
            ("AC_Stacking_Despised", 20, 0f, true));

        Assert.Equal(["AC_Stacking_Central", "AC_Stacking_Preferred", "AC_Stacking_Despised"], RungNames(issue));
        Assert.Equal(1.5f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void AlteredCarbonDuplicating_UnorderedRungsRunFromEnlightened_AndNeutralIsDontCare()
    {
        var issue = Register("AC_StackDuplicating",
            ("AC_StackDuplicating_Horrible", 0, 0f, true), ("AC_StackDuplicating_DontCare", 0, 0f, true),
            ("AC_StackDuplicating_Enlightened", 0, 0f, true));

        Assert.Equal(
            ["AC_StackDuplicating_Enlightened", "AC_StackDuplicating_DontCare", "AC_StackDuplicating_Horrible"],
            RungNames(issue));
        Assert.Equal(1f, PreceptLadder.DontCareRank(issue));
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
