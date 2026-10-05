namespace EnhancedIdeology.Tests;

// Covers PreceptPolicy.IsDebatable: precept and meme debates only argue over issues that feed structural
// opinion (Moral and Special). A PositiveOnly stance must never be picked as a topic or moved by a win.
public class DebateTopicFilterTests : SeededTest
{
    [Theory]
    [InlineData(nameof(PreceptCategory.Moral), true)]
    [InlineData(nameof(PreceptCategory.Special), true)]
    [InlineData(nameof(PreceptCategory.PositiveOnly), false)]
    [InlineData(nameof(PreceptCategory.UniversalPositive), false)]
    [InlineData(nameof(PreceptCategory.NA), false)]
    public void IsDebatable_OnlyMoralAndSpecial(string category, bool expected)
    {
        PreceptPolicy.RegisterCategory("FilterIssue", Enum.Parse<PreceptCategory>(category));
        Assert.Equal(expected, PreceptPolicy.IsDebatable(new IssueDef { defName = "FilterIssue" }));
    }

    [Fact]
    public void DebatePrecept_PositiveOnlyIssue_IsNeverATopic()
    {
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (moral, moralRungs) = SimIssues.Ladder("MoralIssue", "M0", "M1");
        var (perk, perkRungs) = SimIssues.Ladder("PerkIssue", "P0", "P1", "P2", "P3");
        PreceptPolicy.RegisterCategory("PerkIssue", PreceptCategory.PositiveOnly);
        // The perk issue has the wider rung gap, so without the filter it would be the likelier topic.
        var initiatorIdeo = new IdeoBuilder().WithName("IdeoA").AddPrecept(moralRungs[0]).AddPrecept(perkRungs[0]).Build();
        var recipientIdeo = new IdeoBuilder().WithName("IdeoB").AddPrecept(moralRungs[1]).AddPrecept(perkRungs[3]).Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);
        var (initiator, recipient) = Debaters(world, initiatorIdeo, recipientIdeo);

        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var perkBefore = StanceRank(recipientTracker, perk);

        // Repeated wins close the moral gap, after which no topic is left; the perk issue is never picked even then.
        var worker = new InteractionWorker_IdeologicalDebatePrecept();
        var topics = new List<IssueDef?>();
        for (var ii = 0; ii < 20; ii++)
        {
            worker.Interacted(initiator, recipient, [], out _, out _, out _, out _);
            topics.Add(worker.logTopic);
        }

        Assert.Contains(moral, topics);
        Assert.DoesNotContain(perk, topics);
        Assert.Equal(perkBefore, StanceRank(recipientTracker, perk));
    }

    [Fact]
    public void DebatePrecept_OnlyPositiveOnlyDisagreement_LeavesStancesUntouched()
    {
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (perk, perkRungs) = SimIssues.Ladder("PerkIssue", "P0", "P1");
        PreceptPolicy.RegisterCategory("PerkIssue", PreceptCategory.PositiveOnly);
        var initiatorIdeo = new IdeoBuilder().WithName("IdeoA").AddPrecept(perkRungs[0]).Build();
        var recipientIdeo = new IdeoBuilder().WithName("IdeoB").AddPrecept(perkRungs[1]).Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);
        var (initiator, recipient) = Debaters(world, initiatorIdeo, recipientIdeo);

        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var (_, rankBefore, strengthBefore) = recipientTracker.IssueStances().First(s => s.issue == perk);

        new InteractionWorker_IdeologicalDebatePrecept().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        var (_, rankAfter, strengthAfter) = recipientTracker.IssueStances().First(s => s.issue == perk);
        Assert.Equal(rankBefore, rankAfter);
        Assert.Equal(strengthBefore, strengthAfter);
    }

    [Fact]
    public void DebateMeme_Win_MovesMoralIssueButNotPositiveOnlyIssue()
    {
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var topicMeme = new MemeBuilder().WithName("MixedMeme").Build();
        var (moral, moralRungs) = SimIssues.Ladder("MoralIssue", "M0", "M1");
        var (perk, perkRungs) = SimIssues.Ladder("PerkIssue", "P0", "P1");
        PreceptPolicy.RegisterCategory("PerkIssue", PreceptCategory.PositiveOnly);
        foreach (var rung in moralRungs.Concat(perkRungs))
            rung.associatedMemes.Add(topicMeme);

        var initiatorIdeo = new IdeoBuilder().WithName("IdeoA").AddMeme(topicMeme)
            .AddPrecept(moralRungs[0]).AddPrecept(perkRungs[0]).Build();
        var recipientIdeo = new IdeoBuilder().WithName("IdeoB").AddMeme(topicMeme)
            .AddPrecept(moralRungs[1]).AddPrecept(perkRungs[1]).Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);
        var (initiator, recipient) = Debaters(world, initiatorIdeo, recipientIdeo);

        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var moralBefore = StanceRank(recipientTracker, moral);
        var perkBefore = StanceRank(recipientTracker, perk);

        new InteractionWorker_IdeologicalDebateMeme().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        Assert.True(StanceRank(recipientTracker, moral) < moralBefore);
        Assert.Equal(perkBefore, StanceRank(recipientTracker, perk));
    }

    // An initiator who reliably wins against a weak recipient.
    private static (Pawn initiator, Pawn recipient) Debaters(SimWorld world, Ideo initiatorIdeo, Ideo recipientIdeo)
    {
        var initiator = new PawnBuilder()
            .WithIdeo(initiatorIdeo).WithCertainty(1f).WithConversionPower(5f).WithSocialImpact(2f)
            .WithLabel("Init").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo).WithCertainty(0.3f).WithConversionPower(0.1f)
            .WithLabel("Recip").Build(world);
        return (initiator, recipient);
    }

    private static float StanceRank(IdeoTrackerData tracker, IssueDef issue) =>
        tracker.IssueStances().First(s => s.issue == issue).rank;
}
