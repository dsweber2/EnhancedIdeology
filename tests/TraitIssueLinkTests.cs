namespace EnhancedIdeology.Tests;

// Covers TraitIssueLinks: a trait linked to one rung of an issue shapes the stance seeded at spawn and survives
// a brainwipe, and a trait that opposes a UniversalPositive issue turns its flat bonus into a penalty.
public class TraitIssueLinkTests : SeededTest
{
    private static TraitDef LinkedTrait(TraitIssueLink link)
    {
        var trait = new TraitDef { defName = "LinkedTrait" };
        trait.AddModExtension(new TraitIssueLinks { links = [link] });
        return trait;
    }

    // Seeds a pawn whose faith holds `faithRung` (null = silent) on a three-rung issue, with an optional trait.
    // Reseeds first so two calls draw the same base strengths and differ only in the trait.
    private static (IdeoTrackerData tracker, IssueDef issue, PreceptDef[] rungs) Seed(
        int? faithRung, TraitDef? trait, int traitDegree = 0)
    {
        Rand.SetSeed(1);
        DefDatabase<PreceptDef>.Clear();
        DefDatabase<IssueDef>.Clear();
        PreceptPolicy.ClearOverrides();

        var world = new SimWorld();
        world.Initialize();
        var (issue, rungs) = SimIssues.Ladder("LinkIssue", "LinkIssue_A", "LinkIssue_B", "LinkIssue_C");
        var (_, otherRungs) = SimIssues.Ladder("OtherIssue", "OtherIssue_A", "OtherIssue_B");

        var ideoBuilder = new IdeoBuilder().WithName("Faith").AddPrecept(otherRungs[0]);
        if (faithRung is { } held) ideoBuilder = ideoBuilder.AddPrecept(rungs[held]);
        var ideo = ideoBuilder.Build();
        world.AddIdeo(ideo);

        var pawnBuilder = new PawnBuilder().WithIdeo(ideo).WithLabel("P");
        if (trait != null) pawnBuilder = pawnBuilder.WithTrait(trait, traitDegree);
        var pawn = pawnBuilder.Build(world);
        return (world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn), issue, rungs);
    }

    private static (float rank, float strength) StanceOn(IdeoTrackerData tracker, IssueDef issue)
    {
        var stance = tracker.IssueStances().First(ss => ss.issue == issue);
        return (stance.rank, stance.strength);
    }

    [Fact]
    public void SilentFaith_PawnTakesLinkedRungAtLowStrength()
    {
        var trait = LinkedTrait(new TraitIssueLink { rung = "LinkIssue_C" });

        var (tracker, issue, _) = Seed(faithRung: null, trait);
        var (rank, strength) = StanceOn(tracker, issue);

        Assert.Equal(2f, rank);
        Assert.InRange(strength, ConvictionScale.TraitStanceStrengthMin, ConvictionScale.TraitStanceStrengthMax);
    }

    [Fact]
    public void FaithHoldsLinkedRung_GainsConvictionBonus()
    {
        var trait = LinkedTrait(new TraitIssueLink { rung = "LinkIssue_B" });

        var (withTracker, issue, _) = Seed(faithRung: 1, trait);
        var with = StanceOn(withTracker, issue);
        var (withoutTracker, baselineIssue, _) = Seed(faithRung: 1, null);
        var without = StanceOn(withoutTracker, baselineIssue);

        Assert.Equal(1f, with.rank);
        Assert.Equal(without.strength + ConvictionScale.TraitMemeConvictionBonus, with.strength, precision: 4);
    }

    [Fact]
    public void FaithHoldsOtherRung_PawnStartsHeterodoxAtRolledStrength()
    {
        var trait = LinkedTrait(new TraitIssueLink { rung = "LinkIssue_B" });

        var (withTracker, issue, _) = Seed(faithRung: 0, trait);
        var with = StanceOn(withTracker, issue);
        var (withoutTracker, baselineIssue, _) = Seed(faithRung: 0, null);
        var without = StanceOn(withoutTracker, baselineIssue);

        Assert.Equal(1f, with.rank);
        Assert.Equal(without.strength, with.strength, precision: 4);
    }

    [Fact]
    public void DegreeMismatch_HasNoEffect()
    {
        var trait = LinkedTrait(new TraitIssueLink { rung = "LinkIssue_C", degree = 2 });

        var (tracker, issue, _) = Seed(faithRung: 0, trait, traitDegree: 1);

        Assert.Equal(0f, StanceOn(tracker, issue).rank);
    }

    [Fact]
    public void UnknownRung_IsSkipped()
    {
        var trait = LinkedTrait(new TraitIssueLink { rung = "NotARung" });

        var (tracker, issue, _) = Seed(faithRung: 0, trait);

        Assert.Equal(0f, StanceOn(tracker, issue).rank);
    }

    [Fact]
    public void Brainwipe_ReturnsLinkedIssueToLinkedRung()
    {
        var trait = LinkedTrait(new TraitIssueLink { rung = "LinkIssue_C" });
        var (tracker, issue, _) = Seed(faithRung: 0, trait);
        tracker.SetIssueStance(issue, 0f, 1f);

        tracker.ApplyBrainwipe();

        Assert.Equal(2f, StanceOn(tracker, issue).rank);
    }

    // Structural opinion of a mirror faith that holds Charity, with or without a trait opposing Charity.
    private static float OpinionOfCharitableMirror(TraitDef? trait)
    {
        Rand.SetSeed(1);
        DefDatabase<PreceptDef>.Clear();
        DefDatabase<IssueDef>.Clear();
        PreceptPolicy.ClearOverrides();

        var world = new SimWorld();
        world.Initialize();
        var (_, moralRungs) = SimIssues.Ladder("MoralIssue", "MoralIssue_A", "MoralIssue_B");
        var (_, charityRungs) = SimIssues.Ladder("Charity", "Charity_Important");
        PreceptPolicy.RegisterCategory("Charity", PreceptCategory.UniversalPositive);

        var own = new IdeoBuilder().WithName("Own").AddPrecept(moralRungs[0]).AddPrecept(charityRungs[0]).Build();
        var mirror = new IdeoBuilder().WithName("Mirror").AddPrecept(moralRungs[0]).AddPrecept(charityRungs[0]).Build();
        world.AddIdeo(own);
        world.AddIdeo(mirror);

        var pawnBuilder = new PawnBuilder().WithIdeo(own).WithLabel("P");
        if (trait != null) pawnBuilder = pawnBuilder.WithTrait(trait);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawnBuilder.Build(world));
        return tracker.StructuralIdeoOpinion(mirror);
    }

    [Fact]
    public void OpposedUniversalIssue_FlipsFlatBonusToPenalty()
    {
        var trait = LinkedTrait(new TraitIssueLink { opposes = "Charity" });

        var with = OpinionOfCharitableMirror(trait);
        var without = OpinionOfCharitableMirror(null);

        Assert.True(with > 0f);
        Assert.Equal(without - 10f, with, precision: 4);
    }
}
