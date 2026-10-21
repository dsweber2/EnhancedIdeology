namespace EnhancedIdeology.Tests;

// Covers TraitStanceTargets: how rung links, opposes links, and meme (dis)agreement add up to one target per
// issue, and how seeding applies a target the faith does or does not share.
public class TraitStanceTargetTests : SeededTest
{
    private static readonly TraitDef Trait = new() { defName = "TargetTrait" };

    private static (SimWorld world, IssueDef issue, PreceptDef[] rungs) Setup()
    {
        Rand.SetSeed(1);
        DefDatabase<PreceptDef>.Clear();
        DefDatabase<IssueDef>.Clear();
        DefDatabase<MemeDef>.Clear();
        TraitStanceTargets.ClearMemeCache();
        PreceptPolicy.ClearOverrides();

        var world = new SimWorld();
        world.Initialize();
        var (issue, rungs) = SimIssues.Ladder("TargetIssue", "TargetIssue_A", "TargetIssue_B", "TargetIssue_C");
        return (world, issue, rungs);
    }

    private static MemeDef Meme(string name, bool agrees, int listings = 1)
    {
        var builder = new MemeBuilder().WithName(name);
        for (var ii = 0; ii < listings; ii++)
            builder = agrees ? builder.WithAgreeableTrait(Trait) : builder.WithDisagreeableTrait(Trait);
        var meme = builder.Build();
        DefDatabase<MemeDef>.Add(meme);
        return meme;
    }

    private static Pawn PawnWithTrait(SimWorld world, Ideo ideo, TraitDef? trait = null)
    {
        world.AddIdeo(ideo);
        return new PawnBuilder().WithIdeo(ideo).WithLabel("P").WithTrait(trait ?? Trait).Build(world);
    }

    private static Dictionary<IssueDef, TraitStanceTarget> TargetsAt(Pawn pawn, IssueDef issue, float rank) =>
        TraitStanceTargets.For(pawn, new Dictionary<IssueDef, float> { [issue] = rank });

    [Fact]
    public void AgreeAndDisagreeOnSameRung_Cancel()
    {
        var (world, issue, rungs) = Setup();
        rungs[0].requiredMemes.Add(Meme("Pro", agrees: true));
        rungs[0].requiredMemes.Add(Meme("Con", agrees: false, listings: 2));
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build());

        Assert.Empty(TargetsAt(pawn, issue, 1f));
    }

    [Fact]
    public void DisagreeableListing_CountsHalfAnAgreeableOne()
    {
        var (world, issue, rungs) = Setup();
        rungs[0].requiredMemes.Add(Meme("Pro", agrees: true));
        rungs[0].requiredMemes.Add(Meme("Con", agrees: false));
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build());

        var target = TargetsAt(pawn, issue, 1f)[issue];

        Assert.Equal(0f, target.Rung);
        Assert.Equal(1f - TraitStanceTargets.DisagreeableMemeWeight, target.Votes);
    }

    [Fact]
    public void DoubleListing_DoublesVotes_StrengthGrowsSublinearly()
    {
        var (world, issue, rungs) = Setup();
        rungs[2].requiredMemes.Add(Meme("Pro", agrees: true, listings: 2));
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build());

        var target = TargetsAt(pawn, issue, 0f)[issue];

        Assert.Equal(2f, target.Rung);
        Assert.Equal(2f, target.Votes);
        Assert.Equal(ConvictionScale.TraitVoteStrength(2f), target.PullFrom(0f).strength);
        Assert.True(target.PullFrom(0f).strength < 2f * ConvictionScale.TraitVoteStrength(1f));
    }

    [Theory]
    [InlineData(0.5f, 5.04f)]
    [InlineData(1f, 8f)]
    [InlineData(2f, 12.70f)]
    [InlineData(5f, 23.39f)]
    public void TraitVoteStrength_FollowsTwoThirdsPower(float votes, float expected)
    {
        Assert.Equal(expected, ConvictionScale.TraitVoteStrength(votes), precision: 2);
    }

    [Fact]
    public void IdeoRole_GetsNoTarget()
    {
        var (world, _, _) = Setup();
        var (roles, roleRungs) = SimIssues.Ladder("IdeoRole", "IdeoRole_Leader", "IdeoRole_ShootingSpecialist");
        roleRungs[1].requiredMemes.Add(Meme("Pro", agrees: true));
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build());

        Assert.Empty(TargetsAt(pawn, roles, 0f));
    }

    [Fact]
    public void RequiredAndAssociatedOnOneRung_CountOnceAtFullWeight()
    {
        var (world, issue, rungs) = Setup();
        var meme = Meme("Pro", agrees: true);
        rungs[1].requiredMemes.Add(meme);
        rungs[1].associatedMemes.Add(meme);
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build());

        Assert.Equal(1f, TargetsAt(pawn, issue, 0f)[issue].Votes);
    }

    [Fact]
    public void RequireOneGroup_AimsAtClosestAllowedRung()
    {
        var (world, issue, rungs) = Setup();
        var meme = Meme("Pro", agrees: true);
        meme.requireOne.Add([rungs[0], rungs[2]]);
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build());

        Assert.Equal(2f, TargetsAt(pawn, issue, 1.6f)[issue].Rung);
        Assert.Equal(0f, TargetsAt(pawn, issue, 0.4f)[issue].Rung);
    }

    [Fact]
    public void DisagreedMeme_OnTheCentroid_ExitsTowardDontCare()
    {
        var (world, issue, rungs) = Setup();
        rungs[0].requiredMemes.Add(Meme("Con", agrees: false));
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build());

        var target = TargetsAt(pawn, issue, 0f)[issue];

        Assert.Null(target.Rung);
        Assert.Equal(-TraitStanceTargets.DisagreeableMemeWeight, target.Votes);
        Assert.Equal(PreceptLadder.DontCareRank(issue), target.PullFrom(0f).rank);
    }

    [Fact]
    public void OpposedCharity_ExitsEssentialOneRung()
    {
        var (world, _, _) = Setup();
        var (charity, _) = SimIssues.Ladder("Charity", "Charity_Essential", "Charity_Important", "Charity_Worthwhile");
        var greedy = new TraitDef { defName = "Greedy" };
        greedy.AddModExtension(new TraitIssueLinks { links = [new TraitIssueLink { opposes = "Charity" }] });
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build(), greedy);

        Assert.Equal(1f, TargetsAt(pawn, charity, 0f)[charity].PullFrom(0f).rank);
    }

    private static (SimWorld world, IssueDef cannibalism, PreceptDef[] rungs) CannibalismSetup()
    {
        var (world, _, _) = Setup();
        var (cannibalism, rungs) = SimIssues.Ladder("Cannibalism",
            "Cannibalism_RequiredRavenous", "Cannibalism_RequiredStrong", "Cannibalism_Preferred",
            "Cannibalism_Acceptable", "Cannibalism_Disapproved", "Cannibalism_Horrible", "Cannibalism_Abhorrent");
        rungs[6].defaultSelectionWeight = 1f;
        return (world, cannibalism, rungs);
    }

    [Theory]
    [InlineData(6f, 6f)] // Abhorrent: allowed
    [InlineData(1f, 1f)] // RequiredStrong: allowed, although the path to Don't-care crosses Preferred
    [InlineData(2f, 3f)] // Preferred: exits one rung toward Don't-care, not to Don't-care
    public void OpposingPreferredCannibalism_NeverPullsTowardIt(float stance, float expected)
    {
        var (world, cannibalism, rungs) = CannibalismSetup();
        rungs[2].requiredMemes.Add(Meme("Con", agrees: false));
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build());

        var (rank, strength) = TargetsAt(pawn, cannibalism, stance)[cannibalism].PullFrom(stance);

        Assert.Equal(expected, rank);
        if (stance == expected)
            Assert.Equal(ConvictionScale.TraitAllowedStrength, strength);
    }

    [Fact]
    public void SupportTarget_PullsStraightPastAnOpposedRung()
    {
        var (world, issue, rungs) = Setup();
        var linked = new TraitDef { defName = "Linked" };
        linked.AddModExtension(new TraitIssueLinks { links = [new TraitIssueLink { rung = rungs[2].defName }] });
        var meme = new MemeBuilder().WithName("Con").WithDisagreeableTrait(linked).Build();
        DefDatabase<MemeDef>.Add(meme);
        rungs[1].requiredMemes.Add(meme);
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build(), linked);

        var target = TargetsAt(pawn, issue, 0f)[issue];

        Assert.Equal(2f, target.PullFrom(0f).rank);
        Assert.Equal(1f, target.Votes);
    }

    [Fact]
    public void OppositionToOtherRungs_DoesNotAddToSupport()
    {
        var (world, _, _) = Setup();
        var (slavery, rungs) = SimIssues.Ladder("Slavery",
            "Slavery_Honorable", "Slavery_Acceptable", "Slavery_Disapproved", "Slavery_Horrible", "Slavery_Abhorrent");
        var linked = new TraitDef { defName = "Linked" };
        linked.AddModExtension(new TraitIssueLinks { links = [new TraitIssueLink { rung = rungs[4].defName }] });
        var meme = new MemeBuilder().WithName("Con").WithDisagreeableTrait(linked).Build();
        DefDatabase<MemeDef>.Add(meme);
        rungs[0].requiredMemes.Add(meme);
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build(), linked);

        var target = TargetsAt(pawn, slavery, 1f)[slavery];

        Assert.Equal(4f, target.Rung);
        Assert.Equal(1f, target.Votes);
    }

    [Fact]
    public void EverySpotOpposed_GivesNoTarget()
    {
        var (world, _, _) = Setup();
        var (corpses, rungs) = SimIssues.Ladder("Corpses", "Corpses_Ugly", "Corpses_DontCare");
        rungs[0].defaultSelectionWeight = 1f;
        var meme = Meme("Con", agrees: false);
        rungs[0].requiredMemes.Add(meme);
        rungs[1].requiredMemes.Add(meme);
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build());

        Assert.Empty(TargetsAt(pawn, corpses, 0f));
    }

    [Theory]
    [InlineData(20f)]
    [InlineData(2f)]
    public void Decay_OnAnAllowedRung_KeepsRankAndMovesStrengthTowardTen(float startStrength)
    {
        var (world, cannibalism, rungs) = CannibalismSetup();
        rungs[2].requiredMemes.Add(Meme("Con", agrees: false));
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").AddPrecept(rungs[6]).Build());
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIssueStance(cannibalism, 6f, startStrength);

        for (var day = 0; day < 10; day++)
        {
            GenTicks.TicksAbs = day * GenDate.TicksPerDay;
            tracker.ApplyConvictionDecayIfNewDay();
        }

        var stance = tracker.IssueStances().First(ss => ss.issue == cannibalism);
        Assert.Equal(6f, stance.rank);
        Assert.InRange(stance.strength,
            Mathf.Min(startStrength, ConvictionScale.TraitAllowedStrength) + 0.01f,
            Mathf.Max(startStrength, ConvictionScale.TraitAllowedStrength) - 0.01f);
    }

    [Fact]
    public void Seeding_SilentFaithOnAnOpposedDontCare_ExitsIt()
    {
        var (world, cannibalism, rungs) = CannibalismSetup();
        rungs[6].requiredMemes.Add(Meme("Con", agrees: false));
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build());

        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        Assert.Equal(5f, tracker.IssueStances().First(ss => ss.issue == cannibalism).rank);
    }

    [Fact]
    public void Decay_OpposedCharity_DriftsOffEssential()
    {
        var (world, _, _) = Setup();
        var (charity, charityRungs) = SimIssues.Ladder("Charity", "Charity_Essential", "Charity_Important", "Charity_Worthwhile");
        var greedy = new TraitDef { defName = "Greedy" };
        greedy.AddModExtension(new TraitIssueLinks { links = [new TraitIssueLink { opposes = "Charity" }] });
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").AddPrecept(charityRungs[0]).Build(), greedy);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIssueStance(charity, 0f, 10f);

        for (var day = 0; day < 10; day++)
        {
            GenTicks.TicksAbs = day * GenDate.TicksPerDay;
            tracker.ApplyConvictionDecayIfNewDay();
        }

        Assert.True(tracker.IssueStances().First(ss => ss.issue == charity).rank > 0f);
    }

    [Fact]
    public void Seeding_AgreedMemeOnOtherRung_MakesPawnHeterodox()
    {
        var (world, issue, rungs) = Setup();
        rungs[2].requiredMemes.Add(Meme("Pro", agrees: true));
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").AddPrecept(rungs[0]).Build());

        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        Assert.Equal(2f, tracker.IssueStances().First(ss => ss.issue == issue).rank);
    }

    [Fact]
    public void OpposingMachismoPainRung_AimsAtNeutral()
    {
        var (world, _, _) = Setup();
        var (pain, painRungs) = SimIssues.Ladder("Pain", "AM_Pain_Required", "Pain_Idealized", "VME_Pain_DontCare");
        painRungs[2].associatedMemes.Add(Meme("Bushido", agrees: false));
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").Build());
        var neutral = PreceptLadder.DontCareRank(pain);

        Assert.Equal(2.5f, neutral);
        Assert.Equal((neutral, ConvictionScale.TraitAllowedStrength), TargetsAt(pawn, pain, neutral)[pain].PullFrom(neutral));
        Assert.Equal(neutral, TargetsAt(pawn, pain, 2f)[pain].PullFrom(2f).rank);
    }

    [Fact]
    public void DescribeTraitTargets_NamesEachVoteSource()
    {
        var (world, issue, rungs) = Setup();
        var linked = new TraitDef { defName = "Linked" };
        linked.AddModExtension(new TraitIssueLinks { links = [new TraitIssueLink { rung = rungs[2].defName }] });
        var meme = new MemeBuilder().WithName("Pro").WithAgreeableTrait(linked).Build();
        DefDatabase<MemeDef>.Add(meme);
        rungs[2].requiredMemes.Add(meme);
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").AddPrecept(rungs[0]).Build(), linked);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.EnsureIssueStancesSeeded();

        var description = tracker.Stances.DescribeTraitTargets();

        Assert.Contains($"{issue.defName}: votes +2, target TargetIssue_C", description);
        Assert.Contains("+1 trait Linked -> TargetIssue_Cx1", description);
        Assert.Contains("+1 meme Pro via +Linked -> TargetIssue_Cx1", description);
    }

    [Fact]
    public void Seeding_DisagreedMemeOnRungFaithDoesNotHold_LeavesStance()
    {
        static (float rank, float strength) Seed(bool withTrait)
        {
            var (world, issue, rungs) = Setup();
            rungs[0].requiredMemes.Add(Meme("Con", agrees: false));
            var ideo = new IdeoBuilder().WithName("Faith").AddPrecept(rungs[2]).Build();
            var pawn = PawnWithTrait(world, ideo, withTrait ? Trait : new TraitDef { defName = "Unrelated" });
            var stance = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn).IssueStances().First(ss => ss.issue == issue);
            return (stance.rank, stance.strength);
        }

        Assert.Equal(Seed(withTrait: false), Seed(withTrait: true));
    }

    [Fact]
    public void Brainwipe_DisagreedMeme_ExitsOnTheSideTheStanceLeans()
    {
        var (world, issue, rungs) = Setup();
        rungs[0].requiredMemes.Add(Meme("Con", agrees: false));
        var pawn = PawnWithTrait(world, new IdeoBuilder().WithName("Faith").AddPrecept(rungs[0]).Build());
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIssueStance(issue, 0.2f, 10f);

        tracker.ApplyBrainwipe();

        Assert.Equal(1f, tracker.IssueStances().First(ss => ss.issue == issue).rank);
    }
}
