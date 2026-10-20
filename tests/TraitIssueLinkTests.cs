namespace EnhancedIdeology.Tests;

// Covers TraitIssueLinks: a trait linked to one rung of an issue shapes the stance seeded at spawn and survives
// a brainwipe, and a trait that opposes a UniversalPositive issue turns its flat bonus into a penalty.
public class TraitIssueLinkTests : SeededTest
{
    private static TraitDef LinkedTrait(TraitIssueLink link, string defName = "LinkedTrait")
    {
        var trait = new TraitDef { defName = defName };
        trait.AddModExtension(new TraitIssueLinks { links = [link] });
        return trait;
    }

    // Seeds a pawn whose faith holds `faithRung` (null = silent) on a three-rung issue, with an optional trait.
    // Reseeds first so two calls draw the same base strengths and differ only in the trait.
    private static (IdeoTrackerData tracker, IssueDef issue, PreceptDef[] rungs) Seed(
        int? faithRung, TraitDef? trait, int traitDegree = 0, TraitDef? secondTrait = null)
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
        if (secondTrait != null) pawnBuilder = pawnBuilder.WithTrait(secondTrait);
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

    private static void DecayDays(IdeoTrackerData tracker, int days)
    {
        for (var day = 0; day < days; day++)
        {
            GenTicks.TicksAbs = day * GenDate.TicksPerDay;
            tracker.ApplyConvictionDecayIfNewDay();
        }
    }

    [Theory]
    [InlineData(30f)]
    [InlineData(2f)]
    public void Decay_PullsLinkedRungTowardSingleTraitStrength(float startStrength)
    {
        var trait = LinkedTrait(new TraitIssueLink { rung = "LinkIssue_C" });
        var (tracker, issue, _) = Seed(faithRung: null, trait);
        tracker.SetIssueStance(issue, 2f, startStrength);

        DecayDays(tracker, 10);

        var (rank, strength) = StanceOn(tracker, issue);
        Assert.Equal(2f, rank);
        Assert.True(Mathf.Abs(strength - ConvictionScale.TraitDecayStrengthPerVote) < Mathf.Abs(startStrength - ConvictionScale.TraitDecayStrengthPerVote),
            $"Expected strength to move toward {ConvictionScale.TraitDecayStrengthPerVote}. start={startStrength}, after={strength}");
    }

    [Fact]
    public void Decay_TwoTraitsOnOneRung_PullTowardDoubleStrength()
    {
        var link = new TraitIssueLink { rung = "LinkIssue_C" };
        var startStrength = 1.5f * ConvictionScale.TraitDecayStrengthPerVote;

        var (singleTracker, singleIssue, _) = Seed(faithRung: null, LinkedTrait(link));
        singleTracker.SetIssueStance(singleIssue, 2f, startStrength);
        DecayDays(singleTracker, 10);
        var (doubleTracker, doubleIssue, _) = Seed(faithRung: null, LinkedTrait(link), secondTrait: LinkedTrait(link, "LinkedTrait2"));
        doubleTracker.SetIssueStance(doubleIssue, 2f, startStrength);
        DecayDays(doubleTracker, 10);

        Assert.True(StanceOn(singleTracker, singleIssue).strength < startStrength);
        Assert.True(StanceOn(doubleTracker, doubleIssue).strength > startStrength);
    }

    [Fact]
    public void Decay_MovesOffRungStanceTowardLinkedRung()
    {
        var trait = LinkedTrait(new TraitIssueLink { rung = "LinkIssue_C" });
        var (tracker, issue, _) = Seed(faithRung: 0, trait);
        tracker.SetIssueStance(issue, 0f, 10f);

        DecayDays(tracker, 10);

        Assert.True(StanceOn(tracker, issue).rank > 0f);
    }

    [Fact]
    public void Decay_UnlinkedIssueDrainsTowardZero()
    {
        var trait = LinkedTrait(new TraitIssueLink { rung = "LinkIssue_C" });
        var (tracker, _, _) = Seed(faithRung: 0, trait);
        var otherIssue = DefDatabase<IssueDef>.GetNamedSilentFail("OtherIssue")!;
        var before = StanceOn(tracker, otherIssue);

        DecayDays(tracker, 10);

        var after = StanceOn(tracker, otherIssue);
        Assert.Equal(before.rank, after.rank);
        Assert.True(after.strength < before.strength);
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
