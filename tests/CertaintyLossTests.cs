namespace EnhancedIdeology.Tests;

// Covers CertaintyLoss: the clamped stat, the innate (trait and gene) part of it, the curves built on both, and
// the debate outcomes of a pawn that cannot be moved.
public class CertaintyLossTests : SeededTest
{
    private static readonly StatDef Stat = StatDefOf.CertaintyLossFactor;

    private static SimPawn Pawn(SimWorld world, float certaintyLossFactor, string label = "P")
    {
        var ideo = new IdeoBuilder().WithName("Faith_" + label).Build();
        world.AddIdeo(ideo);
        return new PawnBuilder().WithIdeo(ideo).WithCertaintyLossFactor(certaintyLossFactor).WithLabel(label).Build(world);
    }

    private static SimWorld World()
    {
        var world = new SimWorld();
        world.Initialize();
        return world;
    }

    [Theory]
    [InlineData(1.5f, 1.5f)]
    [InlineData(0f, 0f)]
    [InlineData(-0.75f, 0f)]
    public void Factor_ClampsAtZero(float stat, float expected)
    {
        Assert.Equal(expected, CertaintyLoss.Factor(Pawn(World(), stat)));
    }

    [Theory]
    [InlineData(1f, 1f)]
    [InlineData(2f, 0.5f)]
    [InlineData(0.5f, 2f)]
    [InlineData(0.1f, 4f)]
    [InlineData(0f, 4f)]
    public void Resistance_IsInverseFactorCappedAtFloor(float factor, float expected)
    {
        Assert.Equal(expected, CertaintyLoss.Resistance(factor), precision: 5);
    }

    [Fact]
    public void InnateFactor_WithoutTraitsOrGenes_IsBaseValue()
    {
        Assert.Equal(1f, CertaintyLoss.InnateFactor(Pawn(World(), certaintyLossFactor: 0f)));
    }

    [Fact]
    public void InnateFactor_AppliesOffsetsBeforeFactors()
    {
        var pawn = Pawn(World(), certaintyLossFactor: 0f);
        var trait = new Trait { def = new TraitDef { defName = "Stubborn" } };
        trait.statOffsets[Stat] = -0.5f;
        pawn.story.traits.allTraits.Add(trait);
        pawn.genes = new Pawn_GeneTracker();
        pawn.genes.AddGene(new GeneDef
        {
            defName = "Steadfast",
            statFactors = [new StatModifier { stat = Stat, value = 0.5f }],
        });

        Assert.Equal(0.25f, CertaintyLoss.InnateFactor(pawn), precision: 5);
    }

    [Fact]
    public void InnateFactor_IgnoresSuppressedTraits()
    {
        var pawn = Pawn(World(), certaintyLossFactor: 1f);
        var trait = new Trait { def = new TraitDef { defName = "Suppressed" }, Suppressed = true };
        trait.statFactors[Stat] = 0.5f;
        pawn.story.traits.allTraits.Add(trait);

        Assert.Equal(1f, CertaintyLoss.InnateFactor(pawn));
    }

    [Theory]
    [InlineData(1f, 1f, 1f)]
    [InlineData(0.5f, 0.5f, 2f)]
    [InlineData(2f, 2f, 0.5f)]
    [InlineData(0.5f, 1f, 0.5f)]
    [InlineData(0.25f, 0.5f, 1f)]
    [InlineData(0f, 1f, 0f)]
    [InlineData(0f, 0f, 0f)]
    [InlineData(0.1f, 0.1f, 1.6f)]
    public void InnatePull_SpeedsUpForInnateAndSlowsForOtherResistance(float factor, float innate, float expected)
    {
        Assert.Equal(expected, CertaintyLoss.InnatePull(factor, innate), precision: 5);
    }

    [Theory]
    [InlineData(1f, 0f)]
    [InlineData(0.25f, 0f)]
    [InlineData(0.125f, 0.5f)]
    [InlineData(0f, 1f)]
    public void UnmovedChance_RampsFromFloorToZero(float factor, float expected)
    {
        Assert.Equal(expected, CertaintyLoss.UnmovedChance(factor), precision: 5);
    }

    [Fact]
    public void IsDraw_UnmovableLoserAlwaysDraws()
    {
        var world = World();
        var initiator = Pawn(world, certaintyLossFactor: 1f, label: "I");
        var recipient = Pawn(world, certaintyLossFactor: 0f, label: "R");

        Assert.True(InteractionWorker_IdeologicalDebatePrecept.IsDraw(initiator, recipient, 3f, 0f));
        Assert.False(InteractionWorker_IdeologicalDebatePrecept.IsDraw(initiator, recipient, 0f, 3f));
    }

    [Fact]
    public void IsDraw_OrdinaryLoserLosesOutsideTheBand()
    {
        var world = World();
        var initiator = Pawn(world, certaintyLossFactor: 1f, label: "I");
        var recipient = Pawn(world, certaintyLossFactor: 1f, label: "R");

        Assert.False(InteractionWorker_IdeologicalDebatePrecept.IsDraw(initiator, recipient, 3f, 0f));
        Assert.True(InteractionWorker_IdeologicalDebatePrecept.IsDraw(initiator, recipient, 0.1f, 0f));
    }

    [Fact]
    public void TryEntrench_ResistantPawnHardensMore()
    {
        float Gain(float certaintyLossFactor)
        {
            Rand.SetSeed(1);
            var world = World();
            var (issue, rungs) = SimIssues.Ladder("EntrenchIssue_" + certaintyLossFactor, "Rung0", "Rung1");
            var ideo = new IdeoBuilder().WithName("Faith").AddPrecept(rungs[0]).Build();
            world.AddIdeo(ideo);
            // Intellectual 20 at certainty 0 puts the entrench chance above 1, so the pawn always digs in.
            var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0f).WithCertaintyLossFactor(certaintyLossFactor)
                .WithSkill(SkillDefOf.Intellectual, 20).WithLabel("P").Build(world);
            var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
            tracker.Stances.SetStance(issue, 0f, 10f);

            InteractionWorker_IdeologicalDebatePrecept.TryEntrench(pawn, tracker, issue);
            return tracker.Stances.GetStrength(issue) - 10f;
        }

        var ordinary = Gain(1f);
        Assert.True(ordinary > 0f);
        Assert.Equal(ordinary * 2f, Gain(0.5f), precision: 4);
        Assert.Equal(ordinary * 4f, Gain(-1f), precision: 4);
    }
}
