namespace EnhancedIdeology.Tests;

// R3 moral-guide Convert ability: a single debate roll firing a 1-4 issue bundle of normal-strength conversions.
// Roll outcomes are forced by an extreme SocialImpact gap (a ~10-sigma mean difference), so win/loss is decided
// by the stats, not the seed - the Gaussian draw cannot flip it.
public class AbilityConversionTests : SeededTest
{
    [Fact]
    public void MostOpposingIssues_ReturnsOnlyOpposedIssues_Ordered_CappedAtN()
    {
        var (world, guide, recipient, opposed, agree) = BundleFaiths();
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);

        var all = recipientTracker.MostOpposingIssues(guide.Ideo!, 4);
        Assert.Equal(opposed.Count, all.Count);
        Assert.DoesNotContain(agree, all);
        foreach (var issue in opposed)
        {
            Assert.Contains(issue, all);
        }

        // The cap trims to n while keeping the most-opposed first.
        var capped = recipientTracker.MostOpposingIssues(guide.Ideo!, 2);
        Assert.Equal(2, capped.Count);
        Assert.Equal(all[0], capped[0]);
        Assert.Equal(all[1], capped[1]);
    }

    [Fact]
    public void Resolve_Win_PullsEveryTargetedIssueTowardGuide_AndKnocksCertainty()
    {
        var (world, guide, recipient, opposed, agree) = BundleFaiths(
            guideSocialImpact: 30f, recipientSocialImpact: 0f, recipientCertainty: 0.9f);
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var before = opposed.ToDictionary(issue => issue, issue => StanceRank(recipientTracker, issue));
        var agreeBefore = StanceRank(recipientTracker, agree);
        var certaintyBefore = recipient.ideo.Certainty;
        var ideoBefore = recipient.Ideo;

        var converted = AbilityConversion.Resolve(guide, recipient, opposed);

        Assert.False(converted);
        Assert.Equal(ideoBefore, recipient.Ideo);
        foreach (var issue in opposed)
        {
            Assert.True(StanceRank(recipientTracker, issue) < before[issue],
                $"Expected targeted issue {issue.defName} to slide toward the guide's rung.");
        }
        Assert.Equal(agreeBefore, StanceRank(recipientTracker, agree));
        Assert.True(recipient.ideo.Certainty < certaintyBefore, "Expected a won attempt to knock certainty down.");
    }

    [Fact]
    public void Resolve_Loss_PullsOnlyGuidesTopIssue_LeavesRecipientUntouched()
    {
        var (world, guide, recipient, opposed, _) = BundleFaiths(
            guideSocialImpact: 0f, recipientSocialImpact: 30f);
        var guideTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(guide);
        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var topIssue = opposed[0];
        var guideTopBefore = StanceRank(guideTracker, topIssue);
        var recipientBefore = opposed.ToDictionary(issue => issue, issue => StanceRank(recipientTracker, issue));

        var converted = AbilityConversion.Resolve(guide, recipient, opposed);

        Assert.False(converted);
        Assert.True(StanceRank(guideTracker, topIssue) > guideTopBefore,
            "Expected the guide's own top-issue stance to slide toward the recipient on a loss.");
        for (var ii = 1; ii < opposed.Count; ii++)
        {
            Assert.Equal(0f, StanceRank(guideTracker, opposed[ii]));
        }
        foreach (var issue in opposed)
        {
            Assert.Equal(recipientBefore[issue], StanceRank(recipientTracker, issue));
        }
    }

    [Fact]
    public void Resolve_EmptyBundle_IsNoOp()
    {
        var (_, guide, recipient, _, _) = BundleFaiths();
        Assert.False(AbilityConversion.Resolve(guide, recipient, []));
    }

    [Fact]
    public void WinChance_IsNearCertainForALopsidedGuide_AndMirrorsOnReversal()
    {
        var (_, strongGuide, weakRecipient, _, _) = BundleFaiths(guideSocialImpact: 30f, recipientSocialImpact: 0f);

        var forGuide = InteractionWorker_IdeologicalDebatePrecept.WinChance(strongGuide, weakRecipient);
        var reversed = InteractionWorker_IdeologicalDebatePrecept.WinChance(weakRecipient, strongGuide);

        Assert.True(forGuide > 0.99f, $"A ~10-sigma edge should be near-certain. Got {forGuide}.");
        Assert.True(reversed < 0.01f, $"The underdog's win chance should be near-zero. Got {reversed}.");
    }

    [Fact]
    public void NormalCdf_MatchesKnownValues()
    {
        Assert.Equal(0.5f, InteractionWorker_IdeologicalDebatePrecept.NormalCdf(0f), 3);
        Assert.Equal(0.84134f, InteractionWorker_IdeologicalDebatePrecept.NormalCdf(1f), 3);
        Assert.Equal(0.15866f, InteractionWorker_IdeologicalDebatePrecept.NormalCdf(-1f), 3);
    }

    [Fact]
    public void ConversionChanceAfterKnock_LowerKnock_RaisesChance()
    {
        // A harsher knock (lower retained fraction) drops the pawn's effective opinion of their own faith, so
        // the ratio toward a liked alternative rises.
        var world = new SimWorld();
        world.Initialize();
        var own = new IdeoBuilder().WithName("Own").Build();
        var target = new IdeoBuilder().WithName("Target").Build();
        world.AddIdeo(own);
        world.AddIdeo(target);
        var pawn = new PawnBuilder().WithIdeo(own).WithCertainty(0.6f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        tracker.SetIdeoBaseOpinion(target, 80);

        var gentle = tracker.ConversionChanceAfterKnock(target, 0.9f);
        var harsh = tracker.ConversionChanceAfterKnock(target, 0.5f);

        Assert.True(harsh > gentle, $"A harsher knock should raise the conversion chance. gentle={gentle}, harsh={harsh}");
    }

    // Guide preaches rung 0 on three "opposed" issues (recipient sits at the far rung) plus one "agree" issue
    // (recipient shares rung 0). Extreme SocialImpact args force the debate roll's direction.
    private static (SimWorld world, Pawn guide, Pawn recipient, IReadOnlyList<IssueDef> opposed, IssueDef agree) BundleFaiths(
        float guideSocialImpact = 6f, float recipientSocialImpact = 1f, float recipientCertainty = 0.5f)
    {
        var world = new SimWorld();
        world.Initialize();

        var (issueA, rungsA) = SimIssues.Ladder("BundleA", "Aa", "Ab", "Ac");
        var (issueB, rungsB) = SimIssues.Ladder("BundleB", "Ba", "Bb", "Bc");
        var (issueC, rungsC) = SimIssues.Ladder("BundleC", "Ca", "Cb", "Cc");
        var (agree, agreeRungs) = SimIssues.Ladder("BundleAgree", "Ga", "Gb", "Gc");

        var guideIdeo = new IdeoBuilder().WithName("Guide")
            .AddPrecept(rungsA[0], issueA, displayOrderInIssue: 0)
            .AddPrecept(rungsB[0], issueB, displayOrderInIssue: 0)
            .AddPrecept(rungsC[0], issueC, displayOrderInIssue: 0)
            .AddPrecept(agreeRungs[0], agree, displayOrderInIssue: 0).Build();
        var recipientIdeo = new IdeoBuilder().WithName("Flock")
            .AddPrecept(rungsA[2], issueA, displayOrderInIssue: 20)
            .AddPrecept(rungsB[2], issueB, displayOrderInIssue: 20)
            .AddPrecept(rungsC[2], issueC, displayOrderInIssue: 20)
            .AddPrecept(agreeRungs[0], agree, displayOrderInIssue: 0).Build();
        world.AddIdeo(guideIdeo);
        world.AddIdeo(recipientIdeo);

        var guide = new PawnBuilder().WithIdeo(guideIdeo).WithCertainty(1f)
            .WithConversionPower(0.5f).WithSocialImpact(guideSocialImpact).WithLabel("Guide").Build(world);
        var recipient = new PawnBuilder().WithIdeo(recipientIdeo).WithCertainty(recipientCertainty)
            .WithConversionPower(0.1f).WithSocialImpact(recipientSocialImpact).WithLabel("Flock").Build(world);

        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var opposed = recipientTracker.MostOpposingIssues(guideIdeo, 4);
        return (world, guide, recipient, opposed, agree);
    }

    private static float StanceRank(IdeoTrackerData tracker, IssueDef issue) =>
        tracker.IssueStances().First(stance => stance.issue == issue).rank;
}
