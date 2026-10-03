namespace EnhancedIdeology.Tests;

public class DebateOnlookerTests : SeededTest
{
    [Fact]
    public void DebateRelax_MultipliesDebateWeight()
    {
        var (world, initiator, recipient) = CrossIdeoPair();
        var worker = new InteractionWorker_IdeologicalDebatePrecept();
        var relaxJob = new JobDef { defName = "EB_DebateRelax" };
        EnhancedIdeologyDefOf.EB_DebateRelax = relaxJob;

        var baseline = worker.RandomSelectionWeight(initiator, recipient);
        initiator.CurJobDef = relaxJob;
        var relaxing = worker.RandomSelectionWeight(initiator, recipient);

        Assert.True(baseline > 0f);
        Assert.Equal(baseline * InteractionWorker_IdeologicalDebatePrecept.DebateRelaxWeightFactor, relaxing, 4);
    }

    [Fact]
    public void NoDebatableIssue_WeightIsZero()
    {
        // Same ideo, same seeded stances: nothing to argue about, so the debate is never picked.
        var world = new SimWorld();
        world.Initialize();
        var (_, rungs) = SimIssues.Ladder("CalmIssue", "C0", "C1");
        var sharedIdeo = new IdeoBuilder().WithName("Calm").AddPrecept(rungs[0]).Build();
        world.AddIdeo(sharedIdeo);
        var initiator = new PawnBuilder().WithIdeo(sharedIdeo).WithLabel("Init").Build(world);
        var recipient = new PawnBuilder().WithIdeo(sharedIdeo).WithLabel("Recip").Build(world);

        var weight = new InteractionWorker_IdeologicalDebatePrecept().RandomSelectionWeight(initiator, recipient);

        Assert.Equal(0f, weight);
    }

    [Fact]
    public void Witness_SlidesTowardWinnerPersonalStance()
    {
        var world = new SimWorld();
        world.Initialize();
        var (issue, rungs) = SimIssues.Ladder("HeardIssue", "H0", "H1", "H2", "H3");
        var winnerIdeo = new IdeoBuilder().WithName("Loud").AddPrecept(rungs[3]).Build();
        var witnessIdeo = new IdeoBuilder().WithName("Listening").AddPrecept(rungs[2]).Build();
        world.AddIdeo(winnerIdeo);
        world.AddIdeo(witnessIdeo);
        var winner = new PawnBuilder().WithIdeo(winnerIdeo).WithConversionPower(2f).WithLabel("Winner").Build(world);
        var witness = new PawnBuilder().WithIdeo(witnessIdeo).WithLabel("Witness").Build(world);

        // The winner's ideo holds rung 3, but the winner personally sits at rung 0.
        var winnerTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(winner);
        winnerTracker.SetIssueStance(issue, 0f, Stance(winnerTracker, issue).strength);
        var witnessTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(witness);
        var before = Stance(witnessTracker, issue).rank;

        DebateOnlookers.SwayWitness(world.Comp, winner, witness, [issue]);

        Assert.True(Stance(witnessTracker, issue).rank < before,
            $"Expected witness to slide toward rung 0. before={before}, after={Stance(witnessTracker, issue).rank}");
    }

    [Fact]
    public void ProselytizerWinner_SwaysWitnessFurther()
    {
        var proselytizer = new MemeBuilder().WithName("Proselytizer").Build();
        EnhancedIdeologyDefOf.Proselytizer = proselytizer;

        var plainShift = WitnessShift(winnerMeme: null);
        var proselytizerShift = WitnessShift(winnerMeme: proselytizer);

        Assert.True(proselytizerShift > plainShift,
            $"Expected a Proselytizer winner to move the witness further. plain={plainShift}, proselytizer={proselytizerShift}");
    }

    [Fact]
    public void FragileWitness_SwaysFurther()
    {
        var steadyShift = WitnessShift(winnerMeme: null, witnessCertaintyLossFactor: 0.5f);
        var fragileShift = WitnessShift(winnerMeme: null, witnessCertaintyLossFactor: 2f);

        Assert.True(fragileShift > steadyShift,
            $"Expected a high CertaintyLossFactor witness to move further. steady={steadyShift}, fragile={fragileShift}");
    }

    // Rank distance a fresh witness at rung 3 moves after overhearing a winner who sits at rung 0.
    private static float WitnessShift(MemeDef? winnerMeme, float witnessCertaintyLossFactor = 1f)
    {
        DefDatabase<PreceptDef>.Clear();
        DefDatabase<IssueDef>.Clear();
        var world = new SimWorld();
        world.Initialize();
        var (issue, rungs) = SimIssues.Ladder("ShiftIssue", "S0", "S1", "S2", "S3");
        var winnerIdeoBuilder = new IdeoBuilder().WithName("Speaker").AddPrecept(rungs[0]);
        if (winnerMeme != null)
            winnerIdeoBuilder.AddMeme(winnerMeme);
        var winnerIdeo = winnerIdeoBuilder.Build();
        var witnessIdeo = new IdeoBuilder().WithName("Audience").AddPrecept(rungs[3]).Build();
        world.AddIdeo(winnerIdeo);
        world.AddIdeo(witnessIdeo);
        var winner = new PawnBuilder().WithIdeo(winnerIdeo).WithConversionPower(2f).WithLabel("Winner").Build(world);
        var witness = new PawnBuilder().WithIdeo(witnessIdeo).WithCertaintyLossFactor(witnessCertaintyLossFactor)
            .WithLabel("Witness").Build(world);

        var witnessTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(witness);
        var before = Stance(witnessTracker, issue).rank;
        DebateOnlookers.SwayWitness(world.Comp, winner, witness, [issue]);
        return before - Stance(witnessTracker, issue).rank;
    }

    private static (SimWorld world, SimPawn initiator, SimPawn recipient) CrossIdeoPair()
    {
        var world = new SimWorld();
        world.Initialize();
        var (_, rungs) = SimIssues.Ladder("PairIssue", "P0", "P1");
        var initiatorIdeo = new IdeoBuilder().WithName("PairA").AddPrecept(rungs[0]).Build();
        var recipientIdeo = new IdeoBuilder().WithName("PairB").AddPrecept(rungs[1]).Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);
        var initiator = new PawnBuilder().WithIdeo(initiatorIdeo).WithLabel("Init").Build(world);
        var recipient = new PawnBuilder().WithIdeo(recipientIdeo).WithLabel("Recip").Build(world);
        return (world, initiator, recipient);
    }

    private static (IssueDef issue, float rank, float strength) Stance(IdeoTrackerData tracker, IssueDef issue) =>
        tracker.IssueStances().First(stance => stance.issue == issue);
}
