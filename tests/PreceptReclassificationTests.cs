namespace EnhancedIdeology.Tests;

// Covers the former PositiveOnly issues moved to Moral (docs/preceptPolicy.md "PositiveOnly review"): their
// category, rung order, Don't-care placement with and without the mod that adds the opposing pole, the tied
// armour ranks, and the new induced stances.
public class PreceptReclassificationTests : SeededTest
{
    [Theory]
    [InlineData("EB_Contemplation")]
    [InlineData("Research")]
    [InlineData("Pain")]
    [InlineData("Blindness")]
    [InlineData("DarknessCombat")]
    [InlineData("Lighting")]
    [InlineData("Proselytizing")]
    [InlineData("Comfort")]
    [InlineData("AM_Armour")]
    [InlineData("AM_Barracks")]
    [InlineData("AM_PsychicSensitivity")]
    [InlineData("VME_Power")]
    [InlineData("VME_Junk")]
    [InlineData("VME_Death")]
    [InlineData("VME_AutomationEfficiency")]
    [InlineData("VME_CraftingQuality")]
    [InlineData("VME_PsychicSensitivity")]
    [InlineData("BS_AlienAppearanceTolerance")]
    [InlineData("AgeReversal")]
    [InlineData("AM_Madness")]
    [InlineData("VME_LeaderDivinity")]
    [InlineData("VFEA_Recruiting")]
    public void CategoryOf_ValueClaimIssues_AreMoral(string issueName)
    {
        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(new IssueDef { defName = issueName }));
    }

    [Theory]
    [InlineData("AM_HuntFocus")]
    [InlineData("AM_SanguophageCamps")]
    [InlineData("VME_SlaveTrading")]
    [InlineData("AnimalsVenerated")]
    [InlineData("AM_CombatProwess")]
    [InlineData("VME_CraftingSpeed")]
    public void CategoryOf_PreferencesAndCoupledIssues_StayPositiveOnly(string issueName)
    {
        Assert.Equal(PreceptCategory.PositiveOnly, PreceptPolicy.CategoryOf(new IssueDef { defName = issueName }));
    }

    [Fact]
    public void Armour_SpecialtiesShareOneRank()
    {
        var issue = new IssueDef { defName = "AM_Armour" };
        var rungs = Register(issue, ("AM_Armour_Heat", 0), ("AM_Armour_Sharp", 0), ("AM_Armour_Blunt", 0), ("AM_Armour_Forbidden", 0));

        Assert.Equal(new[] { "AM_Armour_Forbidden", "AM_Armour_Blunt" }, PreceptLadder.Rungs(issue).Select(p => p.defName));
        Assert.Equal(1f, PreceptLadder.RankOf(rungs["AM_Armour_Blunt"]));
        Assert.Equal(1f, PreceptLadder.RankOf(rungs["AM_Armour_Sharp"]));
        Assert.Equal(1f, PreceptLadder.RankOf(rungs["AM_Armour_Heat"]));
        Assert.Equal(1f, PreceptLadder.RankOfName(issue, "AM_Armour_Heat"));
        Assert.Equal(0.5f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void Armour_FaithHoldingATiedRung_SeedsTheAnchorRank()
    {
        var issue = new IssueDef { defName = "AM_Armour" };
        var rungs = Register(issue, ("AM_Armour_Forbidden", 0), ("AM_Armour_Blunt", 0), ("AM_Armour_Heat", 0));
        var ideo = new IdeoBuilder().WithName("HeatArmour").AddPrecept(rungs["AM_Armour_Heat"]).Build();

        // A Heat faith and a Blunt faith hold the same rank, so they read as full agreement.
        Assert.Equal(PreceptLadder.RankOf(rungs["AM_Armour_Blunt"]), IssueStanceTracker.HeldRank(ideo, issue));
    }

    [Fact]
    public void Blindness_OrderOverride_PutsHorribleAtTheAntiEnd()
    {
        var issue = new IssueDef { defName = "Blindness" };
        Register(issue, ("Blinding_Horrible", 0), ("Blindness_Sublime", 10), ("Blindness_Elevated", 20), ("Blindness_Respected", 30));

        Assert.Equal(
            new[] { "Blindness_Sublime", "Blindness_Elevated", "Blindness_Respected", "Blinding_Horrible" },
            PreceptLadder.Rungs(issue).Select(p => p.defName));
        Assert.Equal(2.5f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void AlienAppearanceTolerance_OrderOverride_ResolvesTie()
    {
        var issue = new IssueDef { defName = "BS_AlienAppearanceTolerance" };
        Register(issue,
            ("BS_AlienAppearanceTolerance_Default", 100), ("BS_AlienAppearanceTolerance_SomeTolerance", 10),
            ("BS_AlienAppearanceTolerance_FullTolerance", 10));

        Assert.Equal(
            new[]
            {
                "BS_AlienAppearanceTolerance_FullTolerance", "BS_AlienAppearanceTolerance_SomeTolerance",
                "BS_AlienAppearanceTolerance_Default",
            },
            PreceptLadder.Rungs(issue).Select(p => p.defName));
    }

    [Theory]
    [InlineData(true, 1.5f)]
    [InlineData(false, 0.5f)]
    public void Pain_DontCare_SitsPastIdealizedWithOrWithoutTheOtherMods(bool allMods, float expected)
    {
        var issue = new IssueDef { defName = "Pain" };
        Register(issue, ("Pain_Idealized", 0));
        if (allMods)
            Register(issue, ("VME_Pain_DontCare", 0), ("AM_Pain_Required", 0));

        Assert.Equal(expected, PreceptLadder.DontCareRank(issue));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DarknessCombat_DontCare_NeverEqualsThePreferredRung(bool withVme)
    {
        var issue = new IssueDef { defName = "DarknessCombat" };
        Register(issue, ("DarknessCombat_Preferred", 0));
        if (withVme)
            Register(issue, ("VME_DarknessCombat_Despised", 10));

        // With VME it sits between the poles; without, half a step past the only rung. Both are 0.5.
        Assert.Equal(0.5f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void Proselytizing_DontCare_SitsBetweenOccasionallyAndNever()
    {
        var issue = new IssueDef { defName = "Proselytizing" };
        Register(issue,
            ("VME_Proselytizing_Forceful", -10), ("Proselytizing_Frequently", 0), ("Proselytizing_Sometimes", 10),
            ("Proselytizing_Occasionally", 20), ("VME_Proselytizing_Never", 30));

        Assert.Equal(3.5f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void InducedRank_PainRequired_ImpliesWelcomedRoughLiving()
    {
        var (roughLiving, _) = SimIssues.Ladder("RoughLiving", "RoughLiving_Welcomed", "RoughLiving_Disliked");
        var (_, painRungs) = SimIssues.Ladder("Pain", "AM_Pain_Required", "Pain_Idealized");
        var ideo = new IdeoBuilder().WithName("Ascetics").AddPrecept(painRungs[0]).Build();

        Assert.Equal(0f, PreceptPolicy.InducedRank(ideo, roughLiving));
    }

    [Theory]
    [InlineData("AM_HuntFocus", "AM_HuntFocus_Sanguophage")]
    [InlineData("AM_SanguophageCamps", "AM_SanguophageCamps_RaidingDesired")]
    public void InducedRank_SanguophageHunting_ImpliesRevilingBloodfeeders(string issueName, string rungName)
    {
        var (bloodfeeders, _) = SimIssues.Ladder("Bloodfeeders", "Bloodfeeders_Revered", "Bloodfeeders_Reviled");
        var (_, huntRungs) = SimIssues.Ladder(issueName, rungName);
        var ideo = new IdeoBuilder().WithName("Hunters").AddPrecept(huntRungs[0]).Build();

        Assert.Equal(1f, PreceptPolicy.InducedRank(ideo, bloodfeeders));
    }

    private static Dictionary<string, PreceptDef> Register(IssueDef issue, params (string name, int order)[] rungs)
    {
        var defs = new Dictionary<string, PreceptDef>();
        foreach (var (name, order) in rungs)
        {
            var def = new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order };
            SimIssues.Register(def);
            defs[name] = def;
        }
        return defs;
    }
}
