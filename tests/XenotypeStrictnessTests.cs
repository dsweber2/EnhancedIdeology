namespace EnhancedIdeology.Tests;

// Covers VXF_XenotypeStrictness (docs/preceptPolicy.md "Smaller mod ladders"): the order override on its tied
// rungs, the Full neutral from its default weight, and the skip when neither faith prefers a xenotype.
public class XenotypeStrictnessTests : SeededTest
{
    private static readonly string[] RungNames =
    [
        "VXF_XenotypeStrictness_Tolerant", "VXF_XenotypeStrictness_Partial", "VXF_XenotypeStrictness_Full",
        "VXF_XenotypeStrictness_Exact", "VXF_XenotypeStrictness_Vanilla",
    ];

    // The rungs as the mod ships them: all at display order 0, default weight on Full only. Registered in
    // reverse so the order override, not registration order, sets the ladder.
    private static (IssueDef issue, PreceptDef[] rungs) RegisterLadder()
    {
        var issue = new IssueDef { defName = "VXF_XenotypeStrictness" };
        var rungs = RungNames.Select(name => new PreceptDef
        {
            defName = name, issue = issue, defaultSelectionWeight = name.EndsWith("_Full") ? 1f : 0f,
        }).ToArray();
        foreach (var rung in rungs.Reverse())
            SimIssues.Register(rung);
        return (issue, rungs);
    }

    private static Ideo Faith(string name, PreceptDef strictness, bool prefersXenotype)
    {
        var builder = new IdeoBuilder().WithName(name).AddPrecept(strictness);
        if (prefersXenotype)
            _ = builder.AddXenotypePrecept(SimIssues.Special("PreferredXenotypes"), new XenotypeDef { defName = "Hussar" });
        return builder.Build();
    }

    [Fact]
    public void Ladder_RunsLoosestToStrictest_WithFullNeutral()
    {
        var (issue, _) = RegisterLadder();

        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(issue));
        Assert.Equal(RungNames, PreceptLadder.Rungs(issue).Select(rung => rung.defName).ToArray());
        Assert.Equal(2f, PreceptLadder.DontCareRank(issue));
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    public void IsMootBetween_OnlyWhenNeitherFaithPrefersAXenotype(bool pawnPrefers, bool targetPrefers, bool expected)
    {
        var (issue, rungs) = RegisterLadder();
        var pawnIdeo = Faith("Mine", rungs[0], pawnPrefers);
        var targetIdeo = Faith("Theirs", rungs[3], targetPrefers);

        Assert.Equal(expected, PreceptPolicy.IsMootBetween(pawnIdeo, targetIdeo, issue));
    }

    [Fact]
    public void IsMootBetween_OtherIssues_NeverMoot()
    {
        var (_, rungs) = RegisterLadder();
        var (other, _) = SimIssues.Ladder("Execution", "Execution_A", "Execution_B");

        Assert.False(PreceptPolicy.IsMootBetween(Faith("Mine", rungs[0], false), Faith("Theirs", rungs[3], false), other));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void Structural_StrictnessGap_CountsOnlyWhenAFaithPrefersAXenotype(bool targetPrefers, bool expectChange)
    {
        var world = new SimWorld();
        world.Initialize();
        var (issue, rungs) = RegisterLadder();
        var (counted, countedRungs) = SimIssues.Ladder("Execution", "Execution_A", "Execution_B");
        var faith = new IdeoBuilder().WithName("Faith").AddPrecept(countedRungs[0], counted).AddPrecept(rungs[0]).Build();
        var target = Faith("Target", rungs[0], targetPrefers);
        target.precepts.Add(new Precept { def = countedRungs[0], ideo = target });
        world.AddIdeo(faith);
        world.AddIdeo(target);
        var pawn = new PawnBuilder().WithIdeo(faith).WithCertainty(0.5f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        _ = tracker.IssueStances().ToList();

        tracker.Stances.SetStance(issue, 0f, 10f);
        var agreeing = StructuralOpinionCalculator.Compute(pawn, tracker.Stances, target, null);
        tracker.Stances.SetStance(issue, 4f, 10f);
        var opposed = StructuralOpinionCalculator.Compute(pawn, tracker.Stances, target, null);

        Assert.Equal(expectChange, Mathf.Abs(agreeing - opposed) > 0.0001f);
    }

    [Fact]
    public void DebatePrecept_MootStrictness_IsNeverATopic()
    {
        var world = new SimWorld();
        world.Initialize();
        var (issue, rungs) = RegisterLadder();
        var (counted, countedRungs) = SimIssues.Ladder("Execution", "Execution_A", "Execution_B");
        // Strictness has the wider rung gap, so without the filter it would be the likelier topic.
        var initiatorIdeo = Faith("IdeoA", rungs[0], prefersXenotype: false);
        initiatorIdeo.precepts.Add(new Precept { def = countedRungs[0], ideo = initiatorIdeo });
        var recipientIdeo = Faith("IdeoB", rungs[4], prefersXenotype: false);
        recipientIdeo.precepts.Add(new Precept { def = countedRungs[1], ideo = recipientIdeo });
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
        Assert.DoesNotContain(issue, topics);
    }
}
