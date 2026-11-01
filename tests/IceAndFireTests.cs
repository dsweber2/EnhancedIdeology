namespace EnhancedIdeology.Tests;

// Covers A Rim of Ice and Fire (docs/preceptPolicy.md "Mod issues"): its rungs on vanilla issues, its own value
// axes, and the issues that copy a VME issue (PreceptPolicy.DuplicateOf), along with the Alpha Memes psychic
// sensitivity copy that uses the same rule.
public class IceAndFireTests : SeededTest
{
    [Theory]
    [InlineData("ARIOIAF_Lineage")]
    [InlineData("ARIOIAF_Death")]
    [InlineData("ARIOIAF_Expectations")]
    [InlineData("ARIOIAF_Royalty")]
    [InlineData("ARIOIAF_RightfulRule")]
    [InlineData("ARIOIAF_IronPrice")]
    [InlineData("ARIOIAF_Snow")]
    public void CategoryOf_ValueAxes_AreMoral(string issueName)
    {
        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(new IssueDef { defName = issueName }));
    }

    [Theory]
    [InlineData("ARIOIAF_PermitCooldown")]
    [InlineData("ARIOIAF_PermitHonorCost")]
    public void CategoryOf_PermitPerks_ArePositiveOnly(string issueName)
    {
        Assert.Equal(PreceptCategory.PositiveOnly, PreceptPolicy.CategoryOf(new IssueDef { defName = issueName }));
    }

    // Each Moral single-rung issue needs a DontCare entry; without one, the default weight makes the rung neutral.
    [Theory]
    [InlineData("ARIOIAF_Lineage", "ARIOIAF_Lineage_Pure")]
    [InlineData("ARIOIAF_Authority", "ARIOIAF_Authority_Respected")]
    [InlineData("ARIOIAF_FeudalAllegiance", "ARIOIAF_FeudalAllegiance_Honorable")]
    [InlineData("ARIOIAF_LeaderSelection", "ARIOIAF_Leader_HighestTitle")]
    [InlineData("ARIOIAF_RightfulRule", "ARIOIAF_RightfulRule_Essential")]
    [InlineData("ARIOIAF_TitleInheritance", "ARIOIAF_TitleInheritance_Inherited")]
    [InlineData("ARIOIAF_KnightlyConduct", "ARIOIAF_KnightlyConduct_Honorable")]
    [InlineData("ARIOIAF_Oathkeeping", "ARIOIAF_Oathkeeping_Required")]
    [InlineData("ARIOIAF_IntrigueConduct", "ARIOIAF_Intrigue_Dishonorable")]
    [InlineData("ARIOIAF_IronPrice", "ARIOIAF_IronPrice_Demanded")]
    [InlineData("ARIOIAF_Sea", "ARIOIAF_Sea_Revered")]
    [InlineData("ARIOIAF_Snow", "ARIOIAF_Snow_Beautiful")]
    public void SingleRungIssue_DefaultWeight_DoesNotMakeTheRungNeutral(string issueName, string rungName)
    {
        var issue = new IssueDef { defName = issueName };
        SimIssues.Register(new PreceptDef
        {
            defName = rungName, issue = issue, displayOrderInIssue = 10, defaultSelectionWeight = 1f,
        });

        Assert.Equal(0.5f, PreceptLadder.DontCareRank(issue));
    }

    [Theory]
    [InlineData("ARIOIAF_Death", "VME_Death")]
    [InlineData("ARIOIAF_Expectations", "VME_Expectations")]
    [InlineData("ARIOIAF_Royalty", "VME_Royalty")]
    [InlineData("AM_PsychicSensitivity", "VME_PsychicSensitivity")]
    public void CategoryOf_Copy_IsPositiveOnlyWhenOriginalIsLoaded(string copyName, string originalName)
    {
        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(new IssueDef { defName = copyName }));

        DefDatabase<IssueDef>.Add(new IssueDef { defName = originalName });

        Assert.Equal(PreceptCategory.PositiveOnly, PreceptPolicy.CategoryOf(new IssueDef { defName = copyName }));
    }

    [Theory]
    [InlineData("ARIOIAF_Death_DontCare", "VME_Death", 0f)]
    [InlineData("ARIOIAF_Expectations_Low", "VME_Expectations", 1f)]
    [InlineData("ARIOIAF_Royalty_Exalted", "VME_Royalty", 0f)]
    [InlineData("ARIOIAF_Royalty_Respected", "VME_Royalty", 1f)]
    [InlineData("AM_PsychicSensitivity_Heightened", "VME_PsychicSensitivity", 0f)]
    [InlineData("AM_PsychicSensitivity_Affinity", "VME_PsychicSensitivity", 1f)]
    public void CopyRung_InducesTheOriginalRung(string copyRung, string originalIssue, float expectedRank)
    {
        SimIssues.Ladder("VME_Death", "VME_Death_DontCare", "VME_Death_Normal", "VME_Death_Troubling");
        SimIssues.Ladder("VME_Expectations", "VME_Expectations_High", "VME_Expectations_Low");
        SimIssues.Ladder("VME_Royalty",
            "VME_Royalty_Exalted", "VME_Royalty_Respected", "VME_Royalty_Disliked", "VME_Royalty_Hated");
        SimIssues.Ladder("VME_PsychicSensitivity", "VME_PsychicSensitivity_Heightened", "VME_PsychicSensitivity_Lowered");
        var (_, rungs) = SimIssues.Ladder(copyRung + "_Issue", copyRung);
        var ideo = new IdeoBuilder().WithName("Copier").AddPrecept(rungs[0]).Build();
        var original = DefDatabase<IssueDef>.GetNamedSilentFail(originalIssue)!;

        Assert.Equal(expectedRank, PreceptPolicy.InducedRank(ideo, original));
    }

    [Fact]
    public void IronPrice_InducesRespectedRaiding()
    {
        var (raiding, _) = SimIssues.Ladder("Raiding", "Raiding_Required", "Raiding_Respected");
        var (_, rungs) = SimIssues.Ladder("ARIOIAF_IronPrice", "ARIOIAF_IronPrice_Demanded");
        var ideo = new IdeoBuilder().WithName("Ironborn").AddPrecept(rungs[0]).Build();

        Assert.Equal(1f, PreceptPolicy.InducedRank(ideo, raiding));
    }

    [Fact]
    public void Royalty_RunsExaltedFirst_WithNeutralPastRespected()
    {
        var issue = new IssueDef { defName = "ARIOIAF_Royalty" };
        SimIssues.Register(new PreceptDef
        {
            defName = "ARIOIAF_Royalty_Exalted", issue = issue, displayOrderInIssue = 50, defaultSelectionWeight = 1f,
        });
        SimIssues.Register(new PreceptDef
        {
            defName = "ARIOIAF_Royalty_Respected", issue = issue, displayOrderInIssue = 40, defaultSelectionWeight = 1f,
        });

        Assert.Equal(["ARIOIAF_Royalty_Exalted", "ARIOIAF_Royalty_Respected"],
            PreceptLadder.Rungs(issue).Select(rung => rung.defName));
        Assert.Equal(1.5f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void FungusEatingDontCare_SitsBeforeDespised()
    {
        var issue = new IssueDef { defName = "FungusEating" };
        SimIssues.Register(new PreceptDef { defName = "FungusEating_Preferred", issue = issue, displayOrderInIssue = 0 });
        SimIssues.Register(new PreceptDef { defName = "ARIOIAF_FungusEating_DontCare", issue = issue, displayOrderInIssue = 5 });
        SimIssues.Register(new PreceptDef { defName = "FungusEating_Despised", issue = issue, displayOrderInIssue = 10 });

        Assert.Equal(["FungusEating_Preferred", "ARIOIAF_FungusEating_DontCare", "FungusEating_Despised"],
            PreceptLadder.Rungs(issue).Select(rung => rung.defName));
    }

    [Fact]
    public void SevenProselytizing_TiesWithVanillaOccasionally()
    {
        var (issue, rungs) = SimIssues.Ladder("Proselytizing",
            "Proselytizing_Frequently", "Proselytizing_Sometimes", "Proselytizing_Occasionally");
        var seven = new PreceptDef { defName = "ARIOIAF_Proselytizing_Occasionally", issue = issue, displayOrderInIssue = 20 };
        SimIssues.Register(seven);

        Assert.Equal(3, PreceptLadder.Rungs(issue).Count);
        Assert.Equal(PreceptLadder.RankOf(rungs[2]), PreceptLadder.RankOf(seven));
    }
}
