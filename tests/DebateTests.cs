namespace EnhancedIdeology.Tests;

public class DebateTests : SeededTest
{
    [Fact]
    public void DebateMeme_InitiatorWins_PullsLoserStanceTowardWinnerRung()
    {
        // Initiator wins (far higher ConversionPower/stats). Both ideos share the topic meme so it is always
        // selected, and both hold precepts on the same issue at different rungs. The loser's stance rank
        // should slide toward the winner's rung (rung 0) from its starting position (rung 1).
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var topicMeme = new MemeBuilder().WithName("DebateMeme").Build();
        var (issue, rungs) = SimIssues.Ladder("MemeIssue1", "Rung0", "Rung1");
        rungs[0].associatedMemes.Add(topicMeme);
        rungs[1].associatedMemes.Add(topicMeme);

        var initiatorIdeo = new IdeoBuilder().WithName("Evangelist").AddMeme(topicMeme).AddPrecept(rungs[0]).Build();
        var recipientIdeo = new IdeoBuilder().WithName("Skeptic").AddMeme(topicMeme).AddPrecept(rungs[1]).Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);

        var initiator = new PawnBuilder()
            .WithIdeo(initiatorIdeo).WithCertainty(1f).WithConversionPower(5f).WithSocialImpact(2f)
            .WithLabel("Init").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo).WithCertainty(0.3f).WithConversionPower(0.1f)
            .WithLabel("Recip").Build(world);

        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var before = StanceRank(recipientTracker, issue);

        new InteractionWorker_IdeologicalDebateMeme().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        var after = StanceRank(recipientTracker, issue);
        Assert.True(after < before, $"Expected loser stance to slide toward winner's rung 0. before={before}, after={after}");
    }

    [Fact]
    public void DebateMeme_WinnerLacksPreceptForIssue_PullsLoserTowardDontCare()
    {
        // The winner's ideo holds the meme but has no precept for the contested issue (optional/associated precept
        // not taken). Before the fix AdjustOpinions iterated only the winner's precepts and did nothing; now it
        // falls back to DontCareRank so the loser is pulled away from their explicit stance.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var topicMeme = new MemeBuilder().WithName("OptionalMeme").Build();
        var (issue, rungs) = SimIssues.Ladder("OptIssue", "Permissive", "Forbidding");
        rungs[0].associatedMemes.Add(topicMeme);
        rungs[1].associatedMemes.Add(topicMeme);

        // Winner holds the meme but skipped this optional precept entirely.
        var winnerIdeo = new IdeoBuilder().WithName("NoPreceptIdeo").AddMeme(topicMeme).Build();
        // Loser holds the meme and the forbidding precept (rung 1).
        var loserIdeo = new IdeoBuilder().WithName("PreceptIdeo").AddMeme(topicMeme).AddPrecept(rungs[1]).Build();
        world.AddIdeo(winnerIdeo);
        world.AddIdeo(loserIdeo);

        var initiator = new PawnBuilder()
            .WithIdeo(winnerIdeo).WithCertainty(1f).WithConversionPower(5f).WithSocialImpact(2f)
            .WithLabel("Winner").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(loserIdeo).WithCertainty(0.3f).WithConversionPower(0.1f)
            .WithLabel("Loser").Build(world);

        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var before = StanceRank(recipientTracker, issue);

        new InteractionWorker_IdeologicalDebateMeme().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        var after = StanceRank(recipientTracker, issue);
        Assert.True(after < before,
            $"Expected loser stance to be pulled toward DontCareRank even when winner holds no precept. before={before}, after={after}");
    }

    [Fact]
    public void DebateMeme_RecipientWins_PullsLoserStanceTowardWinnerRung()
    {
        // Recipient wins (far higher ConversionPower/stats). Same shared-meme setup, but the recipient holds
        // rung 0 and the initiator holds rung 1. The initiator (loser) should be pulled toward rung 0.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var topicMeme = new MemeBuilder().WithName("DebateMeme2").Build();
        var (issue, rungs) = SimIssues.Ladder("MemeIssue2", "Rung0", "Rung1");
        rungs[0].associatedMemes.Add(topicMeme);
        rungs[1].associatedMemes.Add(topicMeme);

        var initiatorIdeo = new IdeoBuilder().WithName("Weak").AddMeme(topicMeme).AddPrecept(rungs[1]).Build();
        var recipientIdeo = new IdeoBuilder().WithName("Strong").AddMeme(topicMeme).AddPrecept(rungs[0]).Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);

        var initiator = new PawnBuilder()
            .WithIdeo(initiatorIdeo).WithCertainty(0.3f).WithConversionPower(0.1f)
            .WithLabel("Weak").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo).WithCertainty(1f).WithConversionPower(5f).WithSocialImpact(2f)
            .WithLabel("Strong").Build(world);

        var initiatorTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(initiator);
        var before = StanceRank(initiatorTracker, issue);

        new InteractionWorker_IdeologicalDebateMeme().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        var after = StanceRank(initiatorTracker, issue);
        Assert.True(after < before, $"Expected loser stance to slide toward winner's rung 0. before={before}, after={after}");
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
    public void DebatePrecept_InitiatorOnlyIssue_IsDebated_RecipientOnlyIssueIsNot()
    {
        // Two cross-ideo pawns whose ideos cover entirely different issues. The initiator raises the issue their
        // own ideo covers and wins, so the recipient slides toward rung 0 on it. The issue only the recipient's
        // ideo covers is never raised and does not move.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issueA, rungsA) = SimIssues.Ladder("IssueA", "A0", "A1");
        var (issueB, rungsB) = SimIssues.Ladder("IssueB", "B0", "B1");
        var initiatorIdeo = new IdeoBuilder().WithName("IdeoA").AddPrecept(rungsA[0]).Build();
        var recipientIdeo = new IdeoBuilder().WithName("IdeoB").AddPrecept(rungsB[1]).Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);

        var initiator = new PawnBuilder()
            .WithIdeo(initiatorIdeo).WithCertainty(1f).WithConversionPower(5f).WithSocialImpact(2f)
            .WithLabel("Init").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo).WithCertainty(0.3f).WithConversionPower(0.1f)
            .WithLabel("Recip").Build(world);

        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var beforeA = StanceRank(recipientTracker, issueA);
        var beforeB = StanceRank(recipientTracker, issueB);

        var worker = new InteractionWorker_IdeologicalDebatePrecept();
        worker.Interacted(initiator, recipient, [], out _, out _, out _, out _);

        Assert.Equal(issueA, worker.logTopic);
        Assert.True(StanceRank(recipientTracker, issueA) > beforeA,
            $"Expected recipient to slide toward rung 0 on IssueA. before={beforeA}, after={StanceRank(recipientTracker, issueA)}");
        Assert.Equal(beforeB, StanceRank(recipientTracker, issueB));
    }

    [Fact]
    public void DebatePrecept_RecipientIdeoSilent_RecipientWinPullsTowardDontCare()
    {
        // The recipient's ideo has no precept on the topic, so the recipient's personal stance is seeded at the
        // default "Don't care" rung at -1. A recipient win slides the initiator from rung 1 toward it.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issue, rungs) = SimIssues.Ladder("SilentIssue", "S0", "S1");
        var initiatorIdeo = new IdeoBuilder().WithName("Opinionated").AddPrecept(rungs[1]).Build();
        var recipientIdeo = new IdeoBuilder().WithName("Silent").Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);

        var initiator = new PawnBuilder()
            .WithIdeo(initiatorIdeo).WithCertainty(0.3f).WithConversionPower(0.1f)
            .WithLabel("Init").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo).WithCertainty(1f).WithConversionPower(5f).WithSocialImpact(2f)
            .WithLabel("Recip").Build(world);

        var initiatorTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(initiator);
        var before = StanceRank(initiatorTracker, issue);

        var worker = new InteractionWorker_IdeologicalDebatePrecept();
        worker.Interacted(initiator, recipient, [], out _, out _, out _, out _);

        Assert.Equal(recipient, worker.lastWinner);
        Assert.Null(worker.lastWinnerPrecept);
        Assert.True(StanceRank(initiatorTracker, issue) < before,
            $"Expected initiator to slide toward Don't care. before={before}, after={StanceRank(initiatorTracker, issue)}");
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
        Assert.True(Mathf.Abs(rank - 3f) <= 0.01f, $"Expected rank to converge to 3 within 0.01. got={rank}");
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

    [Fact]
    public void DebateMeme_WinnerHasDuplicatePreceptsForSameIssue_DoesNotThrow()
    {
        // Regression: a meme associated with two preceptDefs that share the same issue causes
        // MemePreceptsFor to return duplicate issue keys, crashing ToDictionary in AdjustOpinions.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var topicMeme = new MemeBuilder().WithName("DupeMeme").Build();
        var (issue, rungs) = SimIssues.Ladder("DupeIssue", "Rung0", "Rung1");
        rungs[0].associatedMemes.Add(topicMeme);
        rungs[1].associatedMemes.Add(topicMeme);

        // Winner holds both rungs on the same issue — duplicates in MemePreceptsFor's output.
        var winnerIdeo = new IdeoBuilder().WithName("DupeIdeo").AddMeme(topicMeme)
            .AddPrecept(rungs[0]).AddPrecept(rungs[1]).Build();
        var loserIdeo = new IdeoBuilder().WithName("NormalIdeo").AddMeme(topicMeme).AddPrecept(rungs[1]).Build();
        world.AddIdeo(winnerIdeo);
        world.AddIdeo(loserIdeo);

        var initiator = new PawnBuilder()
            .WithIdeo(winnerIdeo).WithCertainty(1f).WithConversionPower(5f).WithSocialImpact(2f)
            .WithLabel("Winner").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(loserIdeo).WithCertainty(0.3f).WithConversionPower(0.1f)
            .WithLabel("Loser").Build(world);

        var ex = Record.Exception(() =>
            new InteractionWorker_IdeologicalDebateMeme().Interacted(
                initiator, recipient, [], out _, out _, out _, out _));
        Assert.Null(ex);
    }

    // --- Iconoclast topic-selection tests ---

    [Fact]
    public void Iconoclast_CrossIdeo_MostRankOpposedIssueSelected()
    {
        // Two issues: IssueA has a rank gap (iconoclast at rung 0, target at rung 1) and IssueB has no rank gap
        // (both at rung 0). The iconoclast has artificially high conviction on IssueB to verify it cannot
        // override the rank-gap primary key.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issueA, rungsA) = SimIssues.Ladder("IconoIssueA", "A0", "A1");
        var (issueB, rungsB) = SimIssues.Ladder("IconoIssueB", "B0", "B1");

        var iconoIdeo   = new IdeoBuilder().WithName("IconoIdeo").AddPrecept(rungsA[0]).AddPrecept(rungsB[0]).Build();
        var targetIdeo  = new IdeoBuilder().WithName("TargetIdeo").AddPrecept(rungsA[1]).AddPrecept(rungsB[0]).Build();
        world.AddIdeo(iconoIdeo);
        world.AddIdeo(targetIdeo);

        var iconoclast = new PawnBuilder().WithIdeo(iconoIdeo).WithCertainty(1f).WithConversionPower(5f)
            .WithSocialImpact(2f).WithLabel("Iconoclast").Build(world);
        var target = new PawnBuilder().WithIdeo(targetIdeo).WithCertainty(0.5f).WithConversionPower(0.1f)
            .WithLabel("Target").Build(world);

        // Bump iconoclast conviction on IssueB well above IssueA — the distractor.
        var iconoTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(iconoclast);
        iconoTracker.ShiftIssueStance(issueB, 0f, 0f, +80f);

        iconoclast.MentalState = new MentalState_Iconoclast { pawn = iconoclast };

        var worker = new InteractionWorker_IdeologicalDebatePrecept();
        worker.Interacted(iconoclast, target, [], out _, out _, out _, out _);

        Assert.Equal(issueA, worker.logTopic);
    }

    [Fact]
    public void Iconoclast_SameIdeo_WeakestRecipientBeliefSelected()
    {
        // Same ideo, two issues, no rank gap. The iconoclast should target whichever issue the recipient
        // holds most weakly (lowest conviction), not the issue the iconoclast is most fervent about.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issueA, rungsA) = SimIssues.Ladder("SameIconoA", "SA0", "SA1");
        var (issueB, rungsB) = SimIssues.Ladder("SameIconoB", "SB0", "SB1");

        var sharedIdeo = new IdeoBuilder().WithName("SameIdeo").AddPrecept(rungsA[0]).AddPrecept(rungsB[0]).Build();
        world.AddIdeo(sharedIdeo);

        var iconoclast = new PawnBuilder().WithIdeo(sharedIdeo).WithCertainty(1f).WithConversionPower(5f)
            .WithSocialImpact(2f).WithLabel("Iconoclast").Build(world);
        var target = new PawnBuilder().WithIdeo(sharedIdeo).WithCertainty(0.5f).WithConversionPower(0.1f)
            .WithLabel("Target").Build(world);

        var iconoTracker  = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(iconoclast);
        var targetTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(target);

        // Iconoclast is equally fervent on both issues.
        iconoTracker.ShiftIssueStance(issueA, 0f, 0f, +50f);
        iconoTracker.ShiftIssueStance(issueB, 0f, 0f, +50f);

        // Recipient holds IssueA very weakly, IssueB with moderate conviction.
        // Both gaps must exceed DebateStrengthGap (5) to pass the Disagree filter.
        var baseA = targetTracker.IssueStances().First(s => s.issue == issueA).strength;
        var baseB = targetTracker.IssueStances().First(s => s.issue == issueB).strength;
        targetTracker.ShiftIssueStance(issueA, 0f, 0f, -baseA + 2f);  // crush to near-zero conviction
        targetTracker.ShiftIssueStance(issueB, 0f, 0f, -baseB + 12f); // moderate conviction

        iconoclast.MentalState = new MentalState_Iconoclast { pawn = iconoclast };

        var worker = new InteractionWorker_IdeologicalDebatePrecept();
        worker.Interacted(iconoclast, target, [], out _, out _, out _, out _);

        Assert.Equal(issueA, worker.logTopic);
    }

    [Fact]
    public void DebatePrecept_WinnerArguesPersonalStance_NotIdeoRung()
    {
        // The recipient's ideo holds rung 3, but the recipient personally sits at rung 0. The initiator holds
        // rung 2. A recipient win pulls the initiator down toward rung 0, not up toward the ideo's rung 3.
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var (issue, rungs) = SimIssues.Ladder("DriftIssue", "D0", "D1", "D2", "D3");
        var initiatorIdeo = new IdeoBuilder().WithName("DriftInit").AddPrecept(rungs[2]).Build();
        var recipientIdeo = new IdeoBuilder().WithName("DriftRecip").AddPrecept(rungs[3]).Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);

        var initiator = new PawnBuilder()
            .WithIdeo(initiatorIdeo).WithCertainty(0.3f).WithConversionPower(0.1f)
            .WithLabel("Init").Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo).WithCertainty(1f).WithConversionPower(5f).WithSocialImpact(2f)
            .WithLabel("Recip").Build(world);

        var initiatorTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(initiator);
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var recipientStrength = recipientTracker.IssueStances().First(s => s.issue == issue).strength;
        recipientTracker.SetIssueStance(issue, 0f, recipientStrength);
        var before = StanceRank(initiatorTracker, issue);

        var worker = new InteractionWorker_IdeologicalDebatePrecept();
        worker.Interacted(initiator, recipient, [], out _, out _, out _, out _);

        Assert.Equal(recipient, worker.lastWinner);
        Assert.Equal(rungs[0], worker.lastWinnerPrecept);
        Assert.True(StanceRank(initiatorTracker, issue) < before,
            $"Expected initiator to slide toward the recipient's personal rung 0. before={before}, after={StanceRank(initiatorTracker, issue)}");
    }

    [Fact]
    public void TopicWeight_FullLadderGap_ScoresTwentyPlusConvictionGap()
    {
        // Four rungs plus the default "Don't care" rung at -1: span is 4.
        var (issue, _) = SimIssues.Ladder("WeightFull", "WF0", "WF1", "WF2", "WF3");

        var weight = InteractionWorker_IdeologicalDebatePrecept.TopicWeight(issue, (-1f, 10f), (3f, 4f));

        Assert.Equal(26f, weight, 3);
    }

    [Fact]
    public void TopicWeight_PartialGap_ScalesByLadderSpan()
    {
        var (issue, _) = SimIssues.Ladder("WeightPartial", "WP0", "WP1", "WP2", "WP3");

        var weight = InteractionWorker_IdeologicalDebatePrecept.TopicWeight(issue, (0f, 8f), (1f, 8f));

        Assert.Equal(5f, weight, 3);
    }

    [Fact]
    public void TopicWeight_SameRung_IsConvictionGapOnly()
    {
        var (issue, _) = SimIssues.Ladder("WeightSame", "WS0", "WS1");

        var weight = InteractionWorker_IdeologicalDebatePrecept.TopicWeight(issue, (1f, 3f), (1f, 9f));

        Assert.Equal(6f, weight, 3);
    }

    [Fact]
    public void DebatePrecept_FavoursWiderDisagreement()
    {
        // Cross-ideo pawns on two four-rung issues: a full-ladder gap on WideIssue against a one-rung gap on
        // NarrowIssue. Proportional sampling picks the wide issue roughly 20:5 over the narrow one.
        var (wideIssue, wideRungs) = SimIssues.Ladder("WideIssue", "W0", "W1", "W2", "W3");
        var (narrowIssue, narrowRungs) = SimIssues.Ladder("NarrowIssue", "N0", "N1", "N2", "N3");
        var wideCount = 0;
        const int trials = 200;

        for (var ii = 0; ii < trials; ii++)
        {
            var world = new SimWorld();
            world.Initialize();
            Rand.SetSeed(ii);

            var initiatorIdeo = new IdeoBuilder().WithName($"WideA{ii}")
                .AddPrecept(wideRungs[0]).AddPrecept(narrowRungs[0]).Build();
            var recipientIdeo = new IdeoBuilder().WithName($"WideB{ii}")
                .AddPrecept(wideRungs[3]).AddPrecept(narrowRungs[1]).Build();
            world.AddIdeo(initiatorIdeo);
            world.AddIdeo(recipientIdeo);

            var initiator = new PawnBuilder().WithIdeo(initiatorIdeo).WithLabel("Init").Build(world);
            var recipient = new PawnBuilder().WithIdeo(recipientIdeo).WithLabel("Recip").Build(world);

            var worker = new InteractionWorker_IdeologicalDebatePrecept();
            worker.Interacted(initiator, recipient, [], out _, out _, out _, out _);
            if (worker.logTopic == wideIssue)
            {
                wideCount++;
            }
        }

        Assert.InRange(wideCount, trials * 0.6, trials * 0.95);
    }

    private static float StanceRank(IdeoTrackerData tracker, IssueDef issue) =>
        tracker.IssueStances().First(s => s.issue == issue).rank;
}
