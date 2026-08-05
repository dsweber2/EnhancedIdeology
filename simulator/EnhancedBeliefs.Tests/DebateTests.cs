namespace EnhancedBeliefs.Tests;

public class DebateTests
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
    public void DebatePrecept_Winner_AdjustsLoserPreceptOpinion()
    {
        // Two ideos with different precepts on the same issue → debate adjusts loser's precept opinion
        var world = new SimWorld();
        world.Initialize();
        Rand.SetSeed(1);

        var issue = new IssueDef { defName = "TestIssue" };
        var preceptA = new PreceptDef { defName = "PreceptA", issue = issue };
        var preceptB = new PreceptDef { defName = "PreceptB", issue = issue };

        var initiatorIdeo = new IdeoBuilder().WithName("StrongIdeo").AddPrecept(preceptA).Build();
        var recipientIdeo = new IdeoBuilder().WithName("WeakIdeo").AddPrecept(preceptB).Build();
        world.AddIdeo(initiatorIdeo);
        world.AddIdeo(recipientIdeo);

        var initiator = new PawnBuilder()
            .WithIdeo(initiatorIdeo)
            .WithCertainty(1f)
            .WithConversionPower(5f)
            .WithSocialImpact(2f)
            .WithLabel("Strong")
            .Build(world);
        var recipient = new PawnBuilder()
            .WithIdeo(recipientIdeo)
            .WithCertainty(0.3f)
            .WithConversionPower(0.1f)
            .WithLabel("Weak")
            .Build(world);

        var recipientTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(recipient);
        var worker = new InteractionWorker_IdeologicalDebatePrecept();
        worker.Interacted(initiator, recipient, [], out _, out _, out _, out _);

        // Initiator wins → loser (recipient) gets adjusted opinion for initiator's precept (preceptA)
        Assert.True(recipientTracker.TruePreceptOpinion(preceptA) > 0f,
            $"Expected recipient's opinion of initiator's precept to increase. Got: {recipientTracker.TruePreceptOpinion(preceptA)}");
    }
}
