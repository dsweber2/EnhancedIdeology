namespace EnhancedIdeology.Tests;

// Covers PreceptPolicy.IsMuted (docs/preceptPolicy.md "A Rim of Ice and Fire"): a pawn of a Westerosi faith is
// judged only on the issues the Westerosi faiths take a position on, in structural opinion, trait targets and
// debate topics. A pawn of any other faith counts every issue.
public class WesterosMutingTests : SeededTest
{
    private static readonly TraitDef Trait = new() { defName = "MutingTrait" };

    private static Ideo Faith(string name, bool westerosi)
    {
        var builder = new IdeoBuilder().WithName(name);
        if (westerosi)
            _ = builder.AddMeme(new MemeBuilder().WithName("ARIOIAF_Structure_OldGods").Build());
        return builder.Build();
    }

    [Theory]
    [InlineData(true, "AutonomousWeapons", true)]
    [InlineData(true, "DrugUse", true)]
    [InlineData(true, "Execution", false)]
    [InlineData(true, "ARIOIAF_Lineage", false)]
    [InlineData(true, "VME_Death", false)]
    [InlineData(false, "AutonomousWeapons", false)]
    public void IsMuted_OnlyForWesterosiFaiths_OnUnlistedIssues(bool westerosi, string issueName, bool expected)
    {
        Assert.Equal(expected, PreceptPolicy.IsMuted(Faith("Faith", westerosi), new IssueDef { defName = issueName }));
    }

    [Fact]
    public void IsWesterosi_NeedsAWesterosiStructure()
    {
        var feudalOnly = new IdeoBuilder().WithName("Feudal")
            .AddMeme(new MemeBuilder().WithName("ARIOIAF_Feudalism").Build()).Build();

        Assert.False(PreceptPolicy.IsWesterosi(feudalOnly));
        Assert.True(PreceptPolicy.IsWesterosi(Faith("OldGods", westerosi: true)));
        Assert.False(PreceptPolicy.IsWesterosi(null));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Structural_StanceOnMutedIssue_CountsOnlyOutsideWesteros(bool westerosi, bool expectChange)
    {
        var world = new SimWorld();
        world.Initialize();
        var (counted, countedRungs) = SimIssues.Ladder("Execution", "Execution_A", "Execution_B");
        var (muted, _) = SimIssues.Ladder("AutonomousWeapons", "AutonomousWeapons_A", "AutonomousWeapons_B");
        var faith = new IdeoBuilder().WithName("Faith").AddPrecept(countedRungs[0], counted, displayOrderInIssue: 0).Build();
        if (westerosi)
            faith.memes.Add(new MemeBuilder().WithName("ARIOIAF_Structure_OldGods").Build());
        world.AddIdeo(faith);
        var pawn = new PawnBuilder().WithIdeo(faith).WithCertainty(0.5f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        _ = tracker.IssueStances().ToList();

        tracker.Stances.SetStance(muted, PreceptLadder.DontCareRank(muted), 10f);
        var silent = StructuralOpinionCalculator.Compute(pawn, tracker.Stances, faith, null);
        tracker.Stances.SetStance(muted, 1f, 10f);
        var opinionated = StructuralOpinionCalculator.Compute(pawn, tracker.Stances, faith, null);

        Assert.Equal(expectChange, Mathf.Abs(silent - opinionated) > 0.0001f);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void TraitTargets_SkipMutedIssues_ForWesterosiPawns(bool westerosi, bool expectTarget)
    {
        Rand.SetSeed(1);
        var world = new SimWorld();
        world.Initialize();
        TraitStanceTargets.ClearMemeCache();
        var (muted, rungs) = SimIssues.Ladder("AutonomousWeapons", "AutonomousWeapons_A", "AutonomousWeapons_B");
        var meme = new MemeBuilder().WithName("Pro").WithAgreeableTrait(Trait).Build();
        DefDatabase<MemeDef>.Add(meme);
        rungs[1].requiredMemes.Add(meme);
        var faith = Faith("Faith", westerosi);
        world.AddIdeo(faith);
        var pawn = new PawnBuilder().WithIdeo(faith).WithLabel("P").WithTrait(Trait).Build(world);

        var targets = TraitStanceTargets.For(pawn, new Dictionary<IssueDef, float> { [muted] = 0f });

        Assert.Equal(expectTarget, targets.ContainsKey(muted));
    }

    [Fact]
    public void DebatePrecept_MutedIssue_IsNeverATopic_BetweenWesterosiPawns()
    {
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);
        var structure = new MemeBuilder().WithName("ARIOIAF_Structure_OldGods").Build();
        var (counted, countedRungs) = SimIssues.Ladder("Execution", "Execution_A", "Execution_B");
        var (muted, mutedRungs) = SimIssues.Ladder("AutonomousWeapons",
            "AutonomousWeapons_A", "AutonomousWeapons_B", "AutonomousWeapons_C", "AutonomousWeapons_D");
        // The muted issue has the wider rung gap, so without the filter it would be the likelier topic.
        var initiatorIdeo = new IdeoBuilder().WithName("IdeoA").AddMeme(structure)
            .AddPrecept(countedRungs[0]).AddPrecept(mutedRungs[0]).Build();
        var recipientIdeo = new IdeoBuilder().WithName("IdeoB").AddMeme(structure)
            .AddPrecept(countedRungs[1]).AddPrecept(mutedRungs[3]).Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);
        var initiator = new PawnBuilder()
            .WithIdeo(initiatorIdeo).WithCertainty(1f).WithConversionPower(5f).WithSocialImpact(2f)
            .WithLabel("Init").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo).WithCertainty(0.3f).WithConversionPower(0.1f)
            .WithLabel("Recip").Build(world);

        var worker = new InteractionWorker_IdeologicalDebatePrecept();
        var topics = new List<IssueDef?>();
        for (var ii = 0; ii < 20; ii++)
        {
            worker.Interacted(initiator, recipient, [], out _, out _, out _, out _);
            topics.Add(worker.logTopic);
        }

        Assert.Contains(counted, topics);
        Assert.DoesNotContain(muted, topics);
    }
}
