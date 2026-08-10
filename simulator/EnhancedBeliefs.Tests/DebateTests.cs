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

    [Fact]
    public void SameFaithDebate_ConvictionGap_ConvergesLoserTowardWinnerWithoutMovingRank()
    {
        // Two pawns of the SAME faith hold the same rung but with very different conviction. There is no rung
        // to argue over, only zeal: the debate drags the loser's conviction toward the winner's, so the gap
        // between them shrinks while neither one's rung moves.
        var (world, initiator, recipient, issue) = SameFaithPair();
        var initiatorTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(initiator);
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);

        // Devout initiator, shaky recipient - a wide conviction gap on the shared rung.
        initiatorTracker.ShiftIssueStance(issue, 0f, 0f, +50f);
        recipientTracker.ShiftIssueStance(issue, 0f, 0f, -50f);
        var (_, recipientRankBefore, recipientStrengthBefore) = recipientTracker.IssueStances().First(s => s.issue == issue);
        var initiatorStrengthBefore = initiatorTracker.IssueStances().First(s => s.issue == issue).strength;
        var gapBefore = Mathf.Abs(initiatorStrengthBefore - recipientStrengthBefore);

        new InteractionWorker_IdeologicalDebatePrecept().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        var (_, recipientRankAfter, recipientStrengthAfter) = recipientTracker.IssueStances().First(s => s.issue == issue);
        var initiatorStrengthAfter = initiatorTracker.IssueStances().First(s => s.issue == issue).strength;
        var gapAfter = Mathf.Abs(initiatorStrengthAfter - recipientStrengthAfter);

        Assert.True(gapAfter < gapBefore, $"Expected the conviction gap to shrink. before={gapBefore}, after={gapAfter}");
        Assert.Equal(recipientRankBefore, recipientRankAfter);
    }

    [Fact]
    public void SameFaithDebate_NoConvictionGap_YieldsNoTopicAndLeavesStancesUntouched()
    {
        // Same faith, same rung, and now matched conviction: there is nothing to argue about, so no topic is
        // selected and the interaction is a no-op on both pawns' stances.
        var (world, initiator, recipient, issue) = SameFaithPair();
        var initiatorTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(initiator);
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);

        // Equalize the recipient's conviction to the initiator's so the gap falls under DebateStrengthGap.
        var initiatorStrength = initiatorTracker.IssueStances().First(s => s.issue == issue).strength;
        var recipientStrength = recipientTracker.IssueStances().First(s => s.issue == issue).strength;
        recipientTracker.ShiftIssueStance(issue, 0f, 0f, initiatorStrength - recipientStrength);
        var (_, rankBefore, strengthBefore) = recipientTracker.IssueStances().First(s => s.issue == issue);

        new InteractionWorker_IdeologicalDebatePrecept().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        var (_, rankAfter, strengthAfter) = recipientTracker.IssueStances().First(s => s.issue == issue);
        Assert.Equal(rankBefore, rankAfter);
        Assert.Equal(strengthBefore, strengthAfter);
    }

    // The conviction valley (analysis/conviction_valley.py) drives PullStance; these exercise its geometry
    // directly on ValleyStep, independent of the debate roll. Ladder ranks 0..3, winner at rung 3.

    [Fact]
    public void ValleyStep_RepeatedWins_ConvergeToWinnerPole()
    {
        // A pawn losing every debate on the issue walks all the way to the winner's rung AND conviction over a
        // handful of steps - the point of the fixed-vertex valley: it crosses the muddle and climbs the far arm.
        var (rank, strength) = (0f, 14f);
        var steps = 0;
        for (; steps < 100 && (Mathf.Abs(rank - 3f) > 0.01f || Mathf.Abs(strength - 14f) > 0.5f); steps++)
        {
            (rank, strength) = ConvictionMath.ValleyStep(rank, strength, 3f, 0f, 14f, 3f);
        }

        Assert.True(steps < 100, "Expected convergence to the winner within a bounded number of debates.");
        Assert.Equal(3f, rank, 2);
        Assert.True(Mathf.Abs(strength - 14f) <= 0.5f, $"Expected conviction to reach the winner's. got={strength}");
    }

    [Fact]
    public void ValleyStep_FirmerStanceCrawlsSlowerThanShakyOne()
    {
        // The whole reason for the arc-length metric: from the same rung, a firmly-held stance shifts its rung
        // far less per won debate than a shaky one, because the extra conviction makes the curve steep there and
        // the fixed step is spent shedding conviction rather than moving rank.
        var shakyMove = 0.5f - ConvictionMath.ValleyStep(0.5f, 4f, 3f, 0f, 14f, 3f).rank;
        var firmMove = 0.5f - ConvictionMath.ValleyStep(0.5f, 18f, 3f, 0f, 14f, 3f).rank;

        Assert.True(shakyMove < 0f && firmMove < 0f, "both should move toward the winner (rank rises from 0.5)");
        Assert.True(Mathf.Abs(shakyMove) > Mathf.Abs(firmMove),
            $"Expected the shaky stance to move its rung further. shaky={shakyMove}, firm={firmMove}");
    }

    [Fact]
    public void ValleyStep_CrossingFromFar_DipsThroughTheConvictionFloor()
    {
        // Migrating from the far rung drags conviction down through the muddled middle before it recovers: the
        // low point of the walk sits well below both the pawn's starting conviction and the winner's.
        var (rank, strength) = (0f, 14f);
        var lowest = strength;
        for (var ii = 0; ii < 100 && Mathf.Abs(rank - 3f) > 0.01f; ii++)
        {
            (rank, strength) = ConvictionMath.ValleyStep(rank, strength, 3f, 0f, 14f, 3f);
            lowest = Mathf.Min(lowest, strength);
        }

        Assert.True(lowest < 5f, $"Expected the crossing to crater conviction toward the floor. lowest={lowest}");
    }

    [Fact]
    public void ValleyStep_AtTheWinnersRung_SnapsHomeAndOnlyMovesConviction()
    {
        // Within a hair of the winner's rung there is no arc left to integrate: the rung snaps home and only the
        // conviction closes the remaining gap.
        var (rank, strength) = ConvictionMath.ValleyStep(3f, 6f, 3f, 0f, 14f, 3f);

        Assert.Equal(3f, rank);
        Assert.True(strength > 6f && strength < 14f, $"Expected conviction to move part-way toward the winner. got={strength}");
    }

    // Two pawns of one shared faith, a strong initiator and a weak recipient, on one Moral issue they both hold
    // at the same rung. Returns the world, both pawns, and that shared issue.
    private static (SimWorld world, Pawn initiator, Pawn recipient, IssueDef issue) SameFaithPair()
    {
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issue, rungs) = SimIssues.Ladder("TestIssue", "Permissive", "Forbidding");
        var ideo = new IdeoBuilder().WithName("SharedIdeo").AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var initiator = new PawnBuilder()
            .WithIdeo(ideo).WithCertainty(1f).WithConversionPower(5f).WithSocialImpact(2f)
            .WithLabel("Strong").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(ideo).WithCertainty(0.3f).WithConversionPower(0.1f)
            .WithLabel("Weak").Build(world);

        return (world, initiator, recipient, issue);
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
