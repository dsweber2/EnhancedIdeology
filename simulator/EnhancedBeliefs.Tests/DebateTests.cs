namespace EnhancedBeliefs.Tests;

public class DebateTests : SeededTest
{
    [Fact]
    public void DebateMeme_InitiatorWins_IncreasesLoserMemeOpinion()
    {
        // Initiator wins (far higher ConversionPower/stats) → topic meme is in initiator's ideo
        // wasPositiveOutcome=true → adj=+0.03*power*lossFactor → loser's meme opinion increases
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var topicMeme = new MemeBuilder().WithName("DebateMeme").Build();
        var initiatorIdeo = new IdeoBuilder().WithName("Evangelist").AddMeme(topicMeme).Build();
        var recipientIdeo = new IdeoBuilder().WithName("Skeptic").Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);

        var initiator = new PawnBuilder()
            .WithIdeo(initiatorIdeo)
            .WithCertainty(1f)
            .WithConversionPower(5f)
            .WithSocialImpact(2f)
            .WithLabel("Init")
            .Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo)
            .WithCertainty(0.3f)
            .WithConversionPower(0.1f)
            .WithLabel("Recip")
            .Build(world);

        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var worker = new InteractionWorker_IdeologicalDebateMeme();
        worker.Interacted(initiator, recipient, [], out _, out _, out _, out _);

        Assert.True(recipientTracker.TrueMemeOpinion(topicMeme) > 0f,
            $"Expected recipient's meme opinion to increase. Got: {recipientTracker.TrueMemeOpinion(topicMeme)}");
    }

    [Fact]
    public void DebateMeme_RecipientWins_DecreasesRecipientMemeOpinionForOwnMeme()
    {
        // Recipient wins → topic meme is NOT in winner's (recipient's) ideo
        // wasPositiveOutcome = recipient.Ideo.memes.Contains(topic) = false (topic is from initiator's ideo)
        // adj = -0.03 * power * lossFactor → loser (initiator) meme opinion decreases
        // BUT the loser here is the initiator, so initiatorTracker.TrueMemeOpinion decreases
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var topicMeme = new MemeBuilder().WithName("DebateMeme2").Build();
        var initiatorIdeo = new IdeoBuilder().WithName("Weak").AddMeme(topicMeme).Build();
        var recipientIdeo = new IdeoBuilder().WithName("Strong").Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);

        var initiator = new PawnBuilder()
            .WithIdeo(initiatorIdeo)
            .WithCertainty(0.3f)
            .WithConversionPower(0.1f)
            .WithLabel("Weak")
            .Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo)
            .WithCertainty(1f)
            .WithConversionPower(5f)
            .WithSocialImpact(2f)
            .WithLabel("Strong")
            .Build(world);

        var initiatorTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(initiator);
        var worker = new InteractionWorker_IdeologicalDebateMeme();
        worker.Interacted(initiator, recipient, [], out _, out _, out _, out _);

        Assert.True(initiatorTracker.TrueMemeOpinion(topicMeme) < 0f,
            $"Expected initiator's meme opinion to decrease after losing. Got: {initiatorTracker.TrueMemeOpinion(topicMeme)}");
    }

    [Fact]
    public void DebatePrecept_Winner_PullsLoserStanceTowardWinnerRung()
    {
        // Two ideos disagreeing on one issue: the decisive winner drags the loser's personal stance toward
        // the rung the winner argued for (rank 0), so the loser's preferred rank falls below its seed of 1.
        var (world, initiator, recipient, issue, _) = TwoWayDebate();
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var before = StanceRank(recipientTracker, issue);

        new InteractionWorker_IdeologicalDebatePrecept().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        var after = StanceRank(recipientTracker, issue);
        Assert.True(after < before,
            $"Expected loser stance to slide toward the winner's rung 0. before={before}, after={after}");
    }

    [Fact]
    public void RepeatedDebateLosses_RaiseStructuralOpinionOfWinnerIdeo()
    {
        // The whole point of the write-path: as the loser's stance is dragged toward the winner's rung over
        // many debates, their structural fit with the winner's ideo climbs from indifference into positive.
        var (world, initiator, recipient, _, winnerIdeo) = TwoWayDebate();
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var before = recipientTracker.StructuralIdeoOpinion(winnerIdeo);

        var worker = new InteractionWorker_IdeologicalDebatePrecept();
        for (var ii = 0; ii < 25; ii++)
        {
            worker.Interacted(initiator, recipient, [], out _, out _, out _, out _);
        }

        var after = recipientTracker.StructuralIdeoOpinion(winnerIdeo);
        Assert.True(after > before,
            $"Expected repeated losses to warm the recipient to the winner's ideo. before={before}, after={after}");
    }

    [Fact]
    public void ShiftIssueStance_MovesRankAndStrength()
    {
        var (world, _, recipient, issue, _) = TwoWayDebate();
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var (_, rank, strength) = tracker.IssueStances().First(s => s.issue == issue);

        // Pull the whole way to rung 0 and shed two conviction points.
        tracker.ShiftIssueStance(issue, 0f, 1f, -2f);

        var (_, newRank, newStrength) = tracker.IssueStances().First(s => s.issue == issue);
        Assert.Equal(0f, newRank);
        Assert.Equal(strength - 2f, newStrength, 3);
        Assert.NotEqual(rank, newRank);
    }

    // A strong initiator (rung 0) vs a weak recipient (rung 1) on one shared Moral issue, so the recipient
    // reliably loses. Returns the world, both pawns, the contested issue, and the initiator's ideo.
    private static (SimWorld world, Pawn initiator, Pawn recipient, IssueDef issue, Ideo winnerIdeo) TwoWayDebate()
    {
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var initiatorIdeo = new IdeoBuilder().WithName("StrongIdeo").AddPrecept(rungs[0]).Build();
        var recipientIdeo = new IdeoBuilder().WithName("WeakIdeo").AddPrecept(rungs[1]).Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);

        var initiator = new PawnBuilder()
            .WithIdeo(initiatorIdeo).WithCertainty(1f).WithConversionPower(5f).WithSocialImpact(2f)
            .WithLabel("Strong").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo).WithCertainty(0.3f).WithConversionPower(0.1f)
            .WithLabel("Weak").Build(world);

        return (world, initiator, recipient, issue, initiatorIdeo);
    }

    private static float StanceRank(IdeoTrackerData tracker, IssueDef issue) =>
        tracker.IssueStances().First(s => s.issue == issue).rank;
}
