namespace EnhancedIdeology.Tests;

// Covers the trait-meme conviction offset: a pawn's trait compatibility with the memes behind their
// precepts shifts the conviction strength seeded for those issues at spawn.
public class TraitMemeConvictionTests : SeededTest
{
    // Reseeds and resets global state before building so two calls within one test get the same base
    // conviction draw; only the trait configuration differs, isolating the meme offset.
    private static float SeededStrengthFor(MemeDef meme, TraitDef? traitOnPawn, out IssueDef memeIssue)
    {
        Rand.SetSeed(1);
        DefDatabase<PreceptDef>.Clear();
        DefDatabase<IssueDef>.Clear();
        PreceptPolicy.ClearOverrides();

        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("MemeIssue", "Permissive", "Forbidding");
        memeIssue = issue;
        rungs[0].requiredMemes.Add(meme);

        var ideo = new IdeoBuilder().WithName("Faith").AddMeme(meme).AddPrecept(rungs[0]).Build();
        world.AddIdeo(ideo);

        var builder = new PawnBuilder().WithIdeo(ideo).WithLabel("P");
        if (traitOnPawn != null) builder = builder.WithTrait(traitOnPawn);
        var pawn = builder.Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        return tracker.IssueStances().First(ss => ss.issue == issue).strength;
    }

    // Returns (meme-issue strength, unrelated-issue strength), both seeded from Rand seed 1 so base draws
    // are identical across calls and only the trait configuration differs.
    private static (float memeStrength, float otherStrength) TwoIssueStrengths(MemeDef meme, TraitDef? traitOnPawn)
    {
        Rand.SetSeed(1);
        DefDatabase<PreceptDef>.Clear();
        DefDatabase<IssueDef>.Clear();
        PreceptPolicy.ClearOverrides();

        var world = new SimWorld();
        world.Initialize();

        var (memeIssue, memeRungs) = SimIssues.Ladder("MemeIssue", "Permissive", "Forbidding");
        var (otherIssue, otherRungs) = SimIssues.Ladder("OtherIssue", "Permissive", "Forbidding");
        memeRungs[0].requiredMemes.Add(meme);

        var ideo = new IdeoBuilder()
            .WithName("Faith")
            .AddMeme(meme)
            .AddPrecept(memeRungs[0])
            .AddPrecept(otherRungs[0])
            .Build();
        world.AddIdeo(ideo);

        var builder = new PawnBuilder().WithIdeo(ideo).WithLabel("P");
        if (traitOnPawn != null) builder = builder.WithTrait(traitOnPawn);
        var pawn = builder.Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var stances = tracker.IssueStances().ToDictionary(ss => ss.issue, ss => ss.strength);
        return (stances[memeIssue], stances[otherIssue]);
    }

    [Fact]
    public void AgreeableTrait_BoostsConvictionByExactBonus()
    {
        var traitDef = new TraitDef { defName = "Brawler" };
        var meme = new MemeBuilder().WithName("TestMeme").WithAgreeableTrait(traitDef).Build();

        var with = SeededStrengthFor(meme, traitDef, out _);
        var without = SeededStrengthFor(meme, null, out _);

        Assert.Equal(ConvictionScale.TraitMemeConvictionBonus, with - without, precision: 4);
    }

    [Fact]
    public void DisagreeableTrait_PenalizesConvictionByExactBonus()
    {
        var traitDef = new TraitDef { defName = "Brawler" };
        var meme = new MemeBuilder().WithName("TestMeme").WithDisagreeableTrait(traitDef).Build();

        var with = SeededStrengthFor(meme, traitDef, out _);
        var without = SeededStrengthFor(meme, null, out _);

        // Clamped at MinConvictionStrength so the observable delta may be less than the full bonus.
        var expectedWith = Math.Max(without - ConvictionScale.TraitMemeConvictionBonus, 0f);
        Assert.Equal(expectedWith, with, precision: 4);
    }

    [Fact]
    public void UnrelatedTrait_HasNoEffect()
    {
        var agreeableTrait = new TraitDef { defName = "Brawler" };
        var otherTrait = new TraitDef { defName = "Beauty" };
        var meme = new MemeBuilder().WithName("TestMeme").WithAgreeableTrait(agreeableTrait).Build();

        var withOther = SeededStrengthFor(meme, otherTrait, out _);
        var withNone = SeededStrengthFor(meme, null, out _);

        Assert.Equal(withNone, withOther, precision: 4);
    }

    [Fact]
    public void AgreeableTrait_OnlyAffectsMemeIssue_NotUnrelatedIssue()
    {
        var traitDef = new TraitDef { defName = "Brawler" };
        var meme = new MemeBuilder().WithName("TestMeme").WithAgreeableTrait(traitDef).Build();

        var (memeWith, otherWith) = TwoIssueStrengths(meme, traitDef);
        var (memeWithout, otherWithout) = TwoIssueStrengths(meme, null);

        Assert.Equal(ConvictionScale.TraitMemeConvictionBonus, memeWith - memeWithout, precision: 4);
        Assert.Equal(otherWithout, otherWith, precision: 4);
    }
}
