namespace EnhancedIdeology.Tests;

// Covers how PreceptPolicy and PreceptLadder read LadderHeuristic for an issue that no table lists.
public class LadderHeuristicWiringTests : SeededTest
{
    // Adds the defs directly: SimIssues.Register would mark the issue as Moral, and the heuristic would not read it.
    private static IssueDef Unlisted(string issueName, params PreceptDef[] rungs)
    {
        var issue = new IssueDef { defName = issueName };
        DefDatabase<IssueDef>.Add(issue);
        foreach (var rung in rungs)
        {
            rung.issue = issue;
            DefDatabase<PreceptDef>.Add(rung);
        }
        return issue;
    }

    private static PreceptDef Rung(string defName, int order = 0, float weight = 0f) =>
        new() { defName = defName, displayOrderInIssue = order, defaultSelectionWeight = weight };

    private static List<string> RungNames(IssueDef issue) =>
        PreceptLadder.Rungs(issue).Select(precept => precept.defName).ToList();

    [Fact]
    public void ConfidentIssue_IsMoralAndDebatable_WithTheHeuristicOrderAndNeutral()
    {
        var issue = Unlisted("Mounts", Rung("Mounts_Desired", 10), Rung("Mounts_Essential", 20));

        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(issue));
        Assert.True(PreceptPolicy.IsDebatable(issue));
        Assert.Equal(["Mounts_Essential", "Mounts_Desired"], RungNames(issue));
        Assert.Equal(1.5f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void ConfidentIssue_WithAnOpenDefaultRung_UsesTheDefaultsRule()
    {
        var issue = Unlisted("Birth", Rung("Birth_Abhorrent", 30, 0.5f), Rung("Birth_DontCare", 0, 1f), Rung("Birth_Desired", 50));

        Assert.Equal(["Birth_Desired", "Birth_DontCare", "Birth_Abhorrent"], RungNames(issue));
        Assert.Equal(1f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void ConfidentIssue_WithDefaultRungsThatAllNeedAMeme_IgnoresTheirWeights()
    {
        var exalted = Rung("Crown_Exalted", 0, 1f);
        var respected = Rung("Crown_Respected", 10, 1f);
        exalted.requiredMemes.Add(new MemeDef());
        respected.requiredMemes.Add(new MemeDef());

        Assert.Equal(1.5f, PreceptLadder.DontCareRank(Unlisted("Crown", exalted, respected)));
    }

    [Fact]
    public void FlaggedIssue_StaysPositiveOnly_WithDisplayOrderAndTheDefaultNeutral()
    {
        var issue = Unlisted("Speed", Rung("Speed_Slow", 20), Rung("Speed_Fast", 10));

        Assert.Equal(PreceptCategory.PositiveOnly, PreceptPolicy.CategoryOf(issue));
        Assert.False(PreceptPolicy.IsDebatable(issue));
        Assert.Equal(["Speed_Fast", "Speed_Slow"], RungNames(issue));
        Assert.Equal(-1f, PreceptLadder.DontCareRank(issue));
        Assert.Contains("mechanical: Speed_Fast", PreceptPolicy.DescribeGuess(issue));
    }

    [Fact]
    public void SingleRungIssue_StaysPositiveOnly()
    {
        Assert.Equal(PreceptCategory.PositiveOnly, PreceptPolicy.CategoryOf(Unlisted("Tea", Rung("Tea_Required"))));
    }

    [Fact]
    public void ListedIssue_IgnoresTheHeuristic()
    {
        var issue = Unlisted("MiningYield", Rung("MiningYield_Desired", 10), Rung("MiningYield_Essential", 20));

        Assert.Null(PreceptPolicy.GuessFor(issue));
        Assert.Equal(PreceptCategory.PositiveOnly, PreceptPolicy.CategoryOf(issue));
        Assert.Equal(["MiningYield_Desired", "MiningYield_Essential"], RungNames(issue));
        Assert.Equal("", PreceptPolicy.DescribeGuess(issue));
    }

    [Fact]
    public void IssueWithOnlyATableEntryForOrderOrNeutral_IsCurated()
    {
        Assert.True(PreceptPolicy.IsCurated("MeatEating"));
        Assert.True(PreceptPolicy.IsCurated("Charity"));
        Assert.False(PreceptPolicy.IsCurated("Mounts"));
    }

    [Fact]
    public void RegisteredCategory_WinsOverTheHeuristic()
    {
        var issue = Unlisted("Registered", Rung("Registered_Desired", 10), Rung("Registered_Essential", 20));
        PreceptPolicy.RegisterCategory("Registered", PreceptCategory.NA);

        Assert.Equal(PreceptCategory.NA, PreceptPolicy.CategoryOf(issue));
        Assert.Equal(["Registered_Desired", "Registered_Essential"], RungNames(issue));
    }

    [Fact]
    public void HiddenAndTiedRungs_AreNotRead()
    {
        var hidden = Rung("Lore_Hidden");
        hidden.visible = false;
        var tied = Rung("RS_Divorce_FemaleOnly");
        var issue = Unlisted("Lore", Rung("Lore_Desired"), Rung("Lore_Essential"), hidden, tied);

        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(issue));
        Assert.Equal(["Lore_Essential", "Lore_Desired"], RungNames(issue));
    }

    [Fact]
    public void EveryEffectField_MakesABaselineRungAFlag()
    {
        Action<PreceptDef>[] effects =
        [
            precept => precept.comps.Add(new SimComp()),
            precept => precept.abilityStatFactors = [new AbilityStatModifiers()],
            precept => precept.statOffsets = [new StatModifier()],
            precept => precept.statFactors = [new StatModifier()],
            precept => precept.conditionalStatAffecters = [new ConditionalStatAffecter()],
            precept => precept.opposedWorkTypes.Add(new WorkTypeDef()),
            precept => precept.enablesNeeds = [new NeedDef()],
            precept => precept.disablesNeeds = [new NeedDef()],
        ];

        for (var ii = 0; ii < effects.Length; ii++)
        {
            var baseline = Rung($"Effect{ii}_DontCare");
            effects[ii](baseline);
            var issue = Unlisted($"Effect{ii}", baseline, Rung($"Effect{ii}_Disapproved"));

            Assert.Equal(PreceptCategory.PositiveOnly, PreceptPolicy.CategoryOf(issue));
        }
    }

    [Fact]
    public void BaselineRungWithNoEffects_IsTheNeutral()
    {
        var issue = Unlisted("Plain", Rung("Plain_DontCare"), Rung("Plain_Disapproved"), Rung("Plain_Desired"));

        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(issue));
        Assert.Equal(1f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void SavedStance_FollowsItsRungOntoTheHeuristicLadder()
    {
        var issue = Unlisted("Mounts", Rung("Mounts_Desired", 10), Rung("Mounts_Essential", 20));
        // The ladder as a save from before this classification stored it: display order, neutral at the default.
        var saved = new SavedLadder(["Mounts_Desired", "Mounts_Essential"], -1f);

        Assert.Equal(1f, LadderMigration.Remap(issue, saved, 0f));
        Assert.Equal(0f, LadderMigration.Remap(issue, saved, 1f));
        Assert.Equal(1.5f, LadderMigration.Remap(issue, saved, -1f));
    }

    [Fact]
    public void AutoClassifiedReport_ListsOnlyConfidentIssues()
    {
        Unlisted("Mounts", Rung("Mounts_Desired"), Rung("Mounts_Essential"));
        Unlisted("Speed", Rung("Speed_Slow"), Rung("Speed_Fast"));

        var report = PreceptPolicy.DescribeAutoClassified();

        Assert.Contains("Mounts (from no mod), neutral rank 1.5", report);
        Assert.Contains("Mounts_Essential", report);
        Assert.DoesNotContain("Speed", report);
    }

    [Fact]
    public void AutoClassifiedReport_IsEmptyWithNoConfidentIssues()
    {
        Assert.Equal("", PreceptPolicy.DescribeAutoClassified());
    }

    private sealed class SimComp : PreceptComp { }
}
