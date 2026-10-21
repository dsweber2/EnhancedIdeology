namespace EnhancedIdeology.Tests;

// Covers the Ideology: More Precepts issues: category, rung order from the display orders the mod ships, and the
// neutral rank (docs/preceptPolicy.md "Mod issues"). None of these issues has a default rung.
public class MorePreceptsLadderTests : SeededTest
{
    private static IssueDef Register(string issueName, params (string name, int order)[] rungs)
    {
        var issue = new IssueDef { defName = issueName };
        foreach (var (name, order) in rungs)
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order });
        return issue;
    }

    private static string[] RungNames(IssueDef issue) => PreceptLadder.Rungs(issue).Select(rung => rung.defName).ToArray();

    [Theory]
    [InlineData("Alcohol")]
    [InlineData("Barracks")]
    [InlineData("Compassion")]
    [InlineData("DrugPossession")]
    [InlineData("Elderly")]
    [InlineData("MP_NewcomerAttitude")]
    [InlineData("Nomadism")]
    [InlineData("Superstition")]
    [InlineData("TakingFromDowned")]
    [InlineData("Traps")]
    [InlineData("Violence")]
    public void CategoryOf_MorePreceptsIssues_AreMoral(string issueName)
    {
        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(new IssueDef { defName = issueName }));
    }

    [Fact]
    public void Alcohol_TiedProRungsAreOrdered_AndNeutralIsTheNeutralRung()
    {
        var issue = Register("Alcohol",
            ("Alcohol_Wanted", 0), ("Alcohol_Essential", 0), ("Alcohol_Neutral", 10), ("Alcohol_Disapproved", 20),
            ("Alcohol_Prohibited", 30));

        Assert.Equal(
            ["Alcohol_Essential", "Alcohol_Wanted", "Alcohol_Neutral", "Alcohol_Disapproved", "Alcohol_Prohibited"],
            RungNames(issue));
        Assert.Equal(2f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void Violence_PacifismPrecedesAvoided_AndNeutralSitsBeforeWanted()
    {
        var issue = Register("Violence",
            ("Violence_Avoided", 30), ("Violence_Pacifism", 30), ("Violence_Horrible", 40), ("Violence_Disapproved", 50),
            ("Violence_Defense", 55), ("Violence_DefenseMild", 57), ("Violence_Wanted", 60), ("Violence_Essential", 70));

        Assert.Equal(
            ["Violence_Pacifism", "Violence_Avoided", "Violence_Horrible", "Violence_Disapproved", "Violence_Defense",
             "Violence_DefenseMild", "Violence_Wanted", "Violence_Essential"],
            RungNames(issue));
        Assert.Equal(5.5f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void Compassion_NeutralSitsBeforeAllies_AndIndifferenceSitsPastIt()
    {
        var issue = Register("Compassion",
            ("Compassion_All", 20), ("Compassion_NonGuiltyEnemies", 30), ("Compassion_NonHostile", 40),
            ("Compassion_Allies", 50));

        Assert.Equal(2.5f, PreceptLadder.DontCareRank(issue));
        Assert.Equal(3.5f, PreceptLadder.IndifferentRank(issue));
    }

    [Theory]
    [InlineData("Barracks", 0.5f, "Barracks_Preferred", "Barracks_Despised")]
    [InlineData("DrugPossession", 2.5f, "DrugPossession_Prohibited", "DrugPossession_Alcohol", "DrugPossession_Social")]
    [InlineData("Elderly", 1.5f, "Elderly_Abhorred", "Elderly_Disrespected", "Elderly_Respected", "Elderly_Revered")]
    [InlineData("MP_NewcomerAttitude", 2.5f,
        "MP_NewcomerAttitude_Fearful", "MP_NewcomerAttitude_Wary", "MP_NewcomerAttitude_Careful", "MP_NewcomerAttitude_Excited")]
    [InlineData("Nomadism", 2.5f,
        "Nomadism_Essential", "Nomadism_Important", "Nomadism_Wanted", "Nomadism_Disapproved", "Nomadism_Horrible")]
    [InlineData("Superstition", 0.5f, "Superstition_Contempt", "Superstition_Mild", "Superstition_Strong")]
    [InlineData("TakingFromDowned", 2.5f, "TakingFromDowned_Abhorrent", "TakingFromDowned_Horrible", "TakingFromDowned_Disapproved")]
    [InlineData("Traps", -0.5f, "Traps_Disapproved", "Traps_Horrible", "Traps_Prohibited")]
    public void Neutral_SitsAtTheBaseline(string issueName, float expected, params string[] rungsInOrder)
    {
        var issue = Register(issueName, [.. rungsInOrder.Select((name, index) => (name, index * 10))]);

        Assert.Equal(rungsInOrder, RungNames(issue));
        Assert.Equal(expected, PreceptLadder.DontCareRank(issue));
    }
}
