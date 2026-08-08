namespace EnhancedBeliefs.Tests;

// Covers the PreceptPolicy resolver: category classification, the rung-order fix for scrambled stacks, and
// how the category gates structural opinion (preceptPolicy.md).
public class PreceptPolicyTests : SeededTest
{
    [Fact]
    public void CategoryOf_ReadsHardcodedTables()
    {
        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(new IssueDef { defName = "Cannibalism" }));
        Assert.Equal(PreceptCategory.UniversalPositive, PreceptPolicy.CategoryOf(new IssueDef { defName = "Charity" }));
        Assert.Equal(PreceptCategory.Special, PreceptPolicy.CategoryOf(new IssueDef { defName = "PreferredXenotypes" }));
        Assert.Equal(PreceptCategory.NA, PreceptPolicy.CategoryOf(new IssueDef { defName = "IdeoBuilding" }));
    }

    [Fact]
    public void CategoryOf_UnknownIssue_DefaultsToPositiveOnly()
    {
        Assert.Equal(PreceptCategory.PositiveOnly,
            PreceptPolicy.CategoryOf(new IssueDef { defName = "AM_BookReadingSpeed" }));
    }

    [Fact]
    public void OrderOverride_FixesScrambledLadder()
    {
        var issue = new IssueDef { defName = "AnimalSlaughter" };
        // Register with the scrambled display order: Alpha Memes' `desired` (pro) appended at 30, above prohibited.
        foreach (var (name, order) in new[]
        {
            ("AnimalSlaughter_Disapproved", 0), ("AnimalSlaughter_Horrible", 10),
            ("AnimalSlaughter_Prohibited", 20), ("AM_AnimalSlaughter_Desired", 30),
        })
        {
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order });
        }

        var rungs = PreceptLadder.Rungs(issue).Select(p => p.defName).ToArray();

        // Policy pulls the pro rung to rank 0, restoring permissive -> forbidding.
        Assert.Equal(
            new[] { "AM_AnimalSlaughter_Desired", "AnimalSlaughter_Disapproved", "AnimalSlaughter_Horrible", "AnimalSlaughter_Prohibited" },
            rungs);
    }

    [Fact]
    public void OrderOverride_UnlistedRungs_AppendedByDisplayOrder()
    {
        // An issue with no override keeps plain display-order sorting.
        var (issue, _) = SimIssues.Ladder("Diet", "A", "B", "C");
        Assert.Equal(new[] { "A", "B", "C" }, PreceptLadder.Rungs(issue).Select(p => p.defName));
    }

    [Fact]
    public void DontCareRank_DrugUse_IsMidLadder()
    {
        var issue = new IssueDef { defName = "DrugUse" };
        foreach (var (name, order) in new[]
        {
            ("DrugUse_Essential", 0), ("DrugUse_MedicalOrSocial", 10),
            ("DrugUse_MedicalOnly", 20), ("DrugUse_Prohibited", 30),
        })
        {
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order });
        }

        // Between medical-or-social (rank 1) and medical-only (rank 2) -> midpoint 1.5.
        Assert.Equal(1.5f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void DontCareSpec_ResolvesBeforeAndBetweenOnRegisteredLadder()
    {
        var issue = new IssueDef { defName = "Fishing" };
        foreach (var (name, order) in new[]
        {
            ("Fishing_Prohibited", 0), ("Fishing_Disapproved", 10), ("Fishing_Sacred", 30),
        })
        {
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order });
        }

        // Before(rank 0) -> -0.5.
        Assert.Equal(-0.5f, DontCareSpec.Before("Fishing_Prohibited").Resolve(issue));
        // Between disapproved (rank 1) and sacred (rank 2) -> 1.5.
        Assert.Equal(1.5f, DontCareSpec.Between("Fishing_Disapproved", "Fishing_Sacred").Resolve(issue));
    }

    [Fact]
    public void DontCareRank_UnlistedIssue_IsPermissiveExtreme()
    {
        Assert.Equal(-1f, PreceptLadder.DontCareRank(new IssueDef { defName = "Nothing" }));
    }

    [Fact]
    public void SpecialOpinion_Leader_IsCategorical()
    {
        var (issue, _) = SimIssues.Ladder("VME_Leader",
            "VME_Leader_HighestTitle", "VME_Leader_BestPsycaster", "VME_Leader_Godlike");

        // Matching leader-selection agrees; any difference (or one side having none, rank -1) is a full clash.
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 0f, 0f, 10f, 0.5f, out var same));
        Assert.Equal(10f, same);
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 0f, 2f, 10f, 0.5f, out var differ));
        Assert.Equal(-10f, differ);
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, -1f, 0f, 10f, 0.5f, out var none));
        Assert.Equal(-10f, none);
    }

    [Fact]
    public void SpecialOpinion_Mood_LinearAxisGradesByDistance()
    {
        var issue = MoodLadder();

        // High(0) vs High(0): full agreement. High(0) vs Low(2): opposite ends of the linear axis -> -strength.
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 0f, 0f, 10f, 0.5f, out var agree));
        Assert.Equal(10f, agree);
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 0f, 2f, 10f, 0.5f, out var opposite));
        Assert.Equal(-10f, opposite);
    }

    [Fact]
    public void SpecialOpinion_Mood_PariahsClashWithEverythingButThemselves()
    {
        var issue = MoodLadder();

        // Shared(3) vs a linear rung -> clash; vs the other pariah -> clash; vs itself -> agreement.
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 3f, 1f, 10f, 0.5f, out var vsLinear));
        Assert.Equal(-10f, vsLinear);
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 3f, 4f, 10f, 0.5f, out var vsOtherPariah));
        Assert.Equal(-10f, vsOtherPariah);
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 3f, 3f, 10f, 0.5f, out var vsSelf));
        Assert.Equal(10f, vsSelf);
    }

    [Fact]
    public void SpecialOpinion_UnmodelledIssue_IsSkipped()
    {
        Assert.False(PreceptPolicy.TrySpecialOpinion(
            new IssueDef { defName = "Weapons" }, 0f, 1f, 10f, 0.5f, out var opinion));
        Assert.Equal(0f, opinion);
    }

    private static IssueDef MoodLadder()
    {
        var (issue, _) = SimIssues.Ladder("VME_Mood",
            "VME_Mood_HighExpectations", "VME_Mood_Normal", "VME_Mood_LowExpectations",
            "VME_Mood_Shared", "VME_Mood_DictatedByStars");
        return issue;
    }

    [Fact]
    public void MoralIssue_FeedsStructuralOpinion()
    {
        Assert.True(OwnStructuralWithCategory(PreceptCategory.Moral) > 0f);
    }

    [Fact]
    public void PositiveOnlyIssue_ContributesNothingStructural()
    {
        // No Moral issues -> nothing to average -> 0 structural.
        Assert.Equal(0f, OwnStructuralWithCategory(PreceptCategory.PositiveOnly));
    }

    [Fact]
    public void NAIssue_ContributesNothingStructural()
    {
        Assert.Equal(0f, OwnStructuralWithCategory(PreceptCategory.NA));
    }

    // Own-ideo structural opinion when the sole issue is classified as `category`. A single-issue world, so a
    // Moral issue yields ~mean(strength)*5 and a non-Moral one yields 0.
    private static float OwnStructuralWithCategory(PreceptCategory category)
    {
        Rand.SetSeed(1);
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("Diet", "A", "B", "C");
        var ideo = new IdeoBuilder().WithName("Own").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        var mirror = new IdeoBuilder().WithName("Mirror").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        world.AddIdeo(ideo);
        world.AddIdeo(mirror);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        // Override the Moral default SimIssues assigned, then read.
        PreceptPolicy.RegisterCategory("Diet", category);
        return tracker.StructuralIdeoOpinion(mirror);
    }
}
