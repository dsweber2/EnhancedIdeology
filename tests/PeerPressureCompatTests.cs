namespace EnhancedIdeology.Tests;

public class PeerPressureCompatTests : SeededTest, IDisposable
{
    // PP's Multiplier.CertaintyReductionOpinion formula: 1 + 0.01 * opinion * rawMultiplier
    // At rawMultiplier=1 (default): opinion 50 → 1.5, opinion 100 → 2.0, opinion 0 → 1.0
    private static float FakePPMultiplier(int opinion)
        => opinion > 0 ? 1f + 0.01f * opinion : 1f;

    public PeerPressureCompatTests()
    {
        Compat_PeerPressure.SetForTest(FakePPMultiplier);
    }

    public void Dispose()
    {
        Compat_PeerPressure.SetForTest(null);
    }

    [Fact]
    public void AdjustCertaintyKnock_ZeroOpinion_ReturnsUnchangedKnock()
    {
        // PP factor at opinion 0 is 1.0, so adjustedKnock = 1 - (1 - knock) * 1 = knock.
        var knock = 0.8f;
        Assert.Equal(knock, Compat_PeerPressure.AdjustCertaintyKnock(knock, 0), 5);
    }

    [Fact]
    public void AdjustCertaintyKnock_PositiveOpinion_ReducesCertaintyMore()
    {
        // Opinion 100 → ppFactor = 2.0. Knock 0.8 (20% reduction) → adjustedKnock = 1 - 0.2 * 2 = 0.6.
        var knock = 0.8f;
        var adjusted = Compat_PeerPressure.AdjustCertaintyKnock(knock, 100);
        Assert.Equal(0.6f, adjusted, 5);
        Assert.True(adjusted < knock, "positive opinion should increase the certainty reduction");
    }

    [Fact]
    public void AdjustCertaintyKnock_NegativeOpinion_ReturnsUnchangedKnock()
    {
        // PP's formula returns 1.0 for non-positive opinion: no amplification below 0.
        var knock = 0.8f;
        Assert.Equal(knock, Compat_PeerPressure.AdjustCertaintyKnock(knock, -50), 5);
    }

    [Fact]
    public void AdjustCertaintyKnock_NoPP_NativeFormulaApplies()
    {
        // Without PP, native formula runs at the default multiplier (1.0): opinion 100 → factor 2.0.
        // factor = 1 + 0.01 * 100 * 1 = 2.0; adjustedKnock = 1 - 0.2 * 2 = 0.6
        Compat_PeerPressure.SetForTest(null);
        Assert.Equal(0.6f, Compat_PeerPressure.AdjustCertaintyKnock(0.8f, 100), 5);
        Compat_PeerPressure.SetForTest(FakePPMultiplier);
    }

    [Fact]
    public void InteractionWorker_WonAttempt_HighPositiveOpinion_KnocksMoreThanZeroOpinion()
    {
        // Same seeded setup as ConversionTests.InteractionWorker_WonAttempt_KnocksRecipientCertaintyDown.
        // At opinion 100, PP doubles the reduction, so certainty drops further than at opinion 0.

        float CertaintyAfterConversion(int opinion)
        {
            Rand.SetSeed(1);
            var world = new SimWorld();
            world.Initialize();

            var (issue, rungs) = SimIssues.Ladder("PPI", "Permissive", "Middle", "Forbidding");
            var initiatorIdeo = new IdeoBuilder().WithName("StrongFaith").AddPrecept(rungs[0], issue, displayOrderInIssue: 0).Build();
            var recipientIdeo = new IdeoBuilder().WithName("WeakFaith").AddPrecept(rungs[2], issue, displayOrderInIssue: 20).Build();
            world.AddIdeo(initiatorIdeo);
            world.AddIdeo(recipientIdeo);

            // High social impact forces a win without converting (low conversion power).
            var initiator = new PawnBuilder()
                .WithIdeo(initiatorIdeo).WithCertainty(1f)
                .WithConversionPower(0.2f).WithSocialImpact(12f)
                .WithLabel("E").Build(world);
            var recipient = new PawnBuilder()
                .WithIdeo(recipientIdeo).WithCertainty(0.3f)
                .WithConversionPower(0.1f).WithSocialImpact(1f)
                .WithLabel("R").Build(world);

            recipient.relations.SetOpinion(initiator, opinion);

            new InteractionWorker_AdvancedConversionAttempt().Interacted(
                initiator, recipient, [], out _, out _, out _, out _);

            return recipient.ideo.Certainty;
        }

        var certaintyAtNeutral = CertaintyAfterConversion(0);
        var certaintyAtLiked = CertaintyAfterConversion(100);

        Assert.True(certaintyAtLiked < certaintyAtNeutral,
            $"liking the preacher should amplify the certainty knock. neutral={certaintyAtNeutral:P}, liked={certaintyAtLiked:P}");
    }

    [Fact]
    public void AdjustStancePull_PositiveOpinion_ScalesPullUp()
    {
        // Opinion 100 → ppFactor 2.0; pull should double.
        Assert.Equal(2f * 1.5f, Compat_PeerPressure.AdjustStancePull(1.5f, 100), 5);
    }

    [Fact]
    public void AdjustStancePull_ZeroOrNegativeOpinion_ReturnsUnchangedPull()
    {
        Assert.Equal(1.5f, Compat_PeerPressure.AdjustStancePull(1.5f, 0), 5);
        Assert.Equal(1.5f, Compat_PeerPressure.AdjustStancePull(1.5f, -50), 5);
    }

    [Fact]
    public void InteractionWorker_WonAttempt_HighPositiveOpinion_PullsStanceFurther()
    {
        // Same seeded setup; the preacher wins both times. At opinion 100 the stance should move
        // further toward the preacher's rung than at neutral opinion.
        float StanceMovement(int opinion)
        {
            Rand.SetSeed(1);
            var world = new SimWorld();
            world.Initialize();

            var (issue, rungs) = SimIssues.Ladder("PPS" + opinion, "Permissive", "Middle", "Forbidding");
            var initiatorIdeo = new IdeoBuilder().WithName("EI" + opinion).AddPrecept(rungs[0], issue, displayOrderInIssue: 0).Build();
            var recipientIdeo = new IdeoBuilder().WithName("RI" + opinion).AddPrecept(rungs[2], issue, displayOrderInIssue: 20).Build();
            world.AddIdeo(initiatorIdeo);
            world.AddIdeo(recipientIdeo);

            var initiator = new PawnBuilder()
                .WithIdeo(initiatorIdeo).WithCertainty(1f)
                .WithConversionPower(0.2f).WithSocialImpact(12f)
                .WithLabel("E" + opinion).Build(world);
            var recipient = new PawnBuilder()
                .WithIdeo(recipientIdeo).WithCertainty(0.3f)
                .WithConversionPower(0.1f).WithSocialImpact(1f)
                .WithLabel("R" + opinion).Build(world);

            recipient.relations.SetOpinion(initiator, opinion);

            var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
            var rankBefore = tracker.IssueStances().First(stance => stance.issue == issue).rank;

            new InteractionWorker_AdvancedConversionAttempt().Interacted(
                initiator, recipient, [], out _, out _, out _, out _);

            var rankAfter = tracker.IssueStances().First(stance => stance.issue == issue).rank;
            return rankBefore - rankAfter;
        }

        var moveAtNeutral = StanceMovement(0);
        var moveAtLiked = StanceMovement(100);

        Assert.True(moveAtNeutral > 0f, "preacher win should always pull the stance");
        Assert.True(moveAtLiked > moveAtNeutral,
            $"liking the preacher should amplify the stance pull. neutral={moveAtNeutral:F4}, liked={moveAtLiked:F4}");
    }

    [Fact]
    public void InteractionWorker_WonAttempt_NegativeOpinion_SameKnockAsNeutral()
    {
        // PP doesn't amplify below opinion 0: both should produce the same certainty drop.
        float CertaintyAfterConversion(int opinion)
        {
            Rand.SetSeed(1);
            var world = new SimWorld();
            world.Initialize();

            var (issue, rungs) = SimIssues.Ladder("PPNI" + opinion, "Permissive", "Middle", "Forbidding");
            var initiatorIdeo = new IdeoBuilder().WithName("EI" + opinion).AddPrecept(rungs[0], issue, displayOrderInIssue: 0).Build();
            var recipientIdeo = new IdeoBuilder().WithName("RI" + opinion).AddPrecept(rungs[2], issue, displayOrderInIssue: 20).Build();
            world.AddIdeo(initiatorIdeo);
            world.AddIdeo(recipientIdeo);

            var initiator = new PawnBuilder()
                .WithIdeo(initiatorIdeo).WithCertainty(1f)
                .WithConversionPower(0.2f).WithSocialImpact(12f)
                .WithLabel("E" + opinion).Build(world);
            var recipient = new PawnBuilder()
                .WithIdeo(recipientIdeo).WithCertainty(0.3f)
                .WithConversionPower(0.1f).WithSocialImpact(1f)
                .WithLabel("R" + opinion).Build(world);

            recipient.relations.SetOpinion(initiator, opinion);

            new InteractionWorker_AdvancedConversionAttempt().Interacted(
                initiator, recipient, [], out _, out _, out _, out _);

            return recipient.ideo.Certainty;
        }

        var certaintyAtNeutral = CertaintyAfterConversion(0);
        var certaintyAtDisliked = CertaintyAfterConversion(-50);

        Assert.Equal(certaintyAtNeutral, certaintyAtDisliked, 5);
    }
}
