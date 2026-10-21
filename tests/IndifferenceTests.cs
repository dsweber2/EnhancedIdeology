namespace EnhancedIdeology.Tests;

// Covers PreceptLadder.IndifferentRank and the indifferent trait link (TraitIssueLinks.indifferent): the link
// targets the PreceptPolicy.Indifference placement where it differs from neutral, and the neutral stance otherwise.
public class IndifferenceTests : SeededTest
{
    private static (SimWorld world, TraitDef trait) Setup(string indifferentIssue)
    {
        Rand.SetSeed(1);
        DefDatabase<PreceptDef>.Clear();
        DefDatabase<IssueDef>.Clear();
        DefDatabase<MemeDef>.Clear();
        TraitStanceTargets.ClearMemeCache();
        PreceptPolicy.ClearOverrides();

        var world = new SimWorld();
        world.Initialize();
        var trait = new TraitDef { defName = "IndifferentTrait" };
        trait.AddModExtension(new TraitIssueLinks { links = [new TraitIssueLink { indifferent = indifferentIssue }] });
        return (world, trait);
    }

    private static IssueDef Scarification()
    {
        var (issue, rungs) = SimIssues.Ladder("Scarification",
            "AM_Scarification_Insane", "Scarification_Extreme", "Scarification_Heavy", "Scarification_Minor",
            "Scarification_Horrible");
        rungs[4].defaultSelectionWeight = 1f;
        return issue;
    }

    private static Pawn PawnWithTrait(SimWorld world, TraitDef trait)
    {
        var ideo = new IdeoBuilder().WithName("Faith").Build();
        world.AddIdeo(ideo);
        return new PawnBuilder().WithIdeo(ideo).WithLabel("P").WithTrait(trait).Build(world);
    }

    [Fact]
    public void IndifferentRank_UsesTheIndifferenceTable_WhereNeutralObjects()
    {
        Setup("Scarification");
        var issue = Scarification();

        Assert.Equal(4f, PreceptLadder.DontCareRank(issue));
        Assert.Equal(3.5f, PreceptLadder.IndifferentRank(issue));
    }

    [Fact]
    public void IndifferentRank_IsTheNeutralStance_ForAnIssueWithoutAnEntry()
    {
        Setup("GrowthVat");
        var (issue, _) = SimIssues.Ladder("GrowthVat", "GrowthVat_Essential", "GrowthVat_Prohibited");

        Assert.Equal(PreceptLadder.DontCareRank(issue), PreceptLadder.IndifferentRank(issue));
    }

    [Fact]
    public void IndifferentLink_PullsTowardIndifference()
    {
        var (world, trait) = Setup("Scarification");
        var issue = Scarification();
        var pawn = PawnWithTrait(world, trait);

        var target = TraitStanceTargets.For(pawn, new Dictionary<IssueDef, float> { [issue] = 4f })[issue];

        Assert.Equal(3.5f, target.Rung);
        Assert.Equal(3.5f, target.PullFrom(4f).rank);
    }

    [Fact]
    public void IndifferentLink_ToAnAbsentIssue_IsSkipped()
    {
        var (world, trait) = Setup("NotLoadedIssue");
        var issue = Scarification();
        var pawn = PawnWithTrait(world, trait);

        Assert.Empty(TraitStanceTargets.For(pawn, new Dictionary<IssueDef, float> { [issue] = 4f }));
    }
}
