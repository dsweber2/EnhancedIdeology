namespace EnhancedIdeology.Tests;

public class ProselytizerAftermathTests : SeededTest
{
    [Fact]
    public void SameFaithMemeDebate_GivesNoProselytizerMoodlet()
    {
        var (world, proselytizerIdeo, _) = Setup();
        var initiator = new PawnBuilder().WithIdeo(proselytizerIdeo).WithLabel("Init").Build(world);
        var recipient = new PawnBuilder().WithIdeo(proselytizerIdeo).WithLabel("Recip").Build(world);

        new InteractionWorker_IdeologicalDebateMeme().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        Assert.DoesNotContain(initiator.needs.mood.thoughts.memories.Memories, IsProselytizerMoodlet);
    }

    [Fact]
    public void CrossFaithMemeDebate_GivesProselytizerMoodlet()
    {
        var (world, proselytizerIdeo, otherIdeo) = Setup();
        var initiator = new PawnBuilder().WithIdeo(proselytizerIdeo).WithLabel("Init").Build(world);
        var recipient = new PawnBuilder().WithIdeo(otherIdeo).WithLabel("Recip").Build(world);

        new InteractionWorker_IdeologicalDebateMeme().Interacted(initiator, recipient, [], out _, out _, out _, out _);

        Assert.Contains(initiator.needs.mood.thoughts.memories.Memories, IsProselytizerMoodlet);
    }

    private static bool IsProselytizerMoodlet(Thought_Memory thought) =>
        thought.def == EnhancedIdeologyDefOf.EB_ProselytizerDebated || thought.def == EnhancedIdeologyDefOf.EB_ProselytizerConverted;

    private static (SimWorld world, Ideo proselytizerIdeo, Ideo otherIdeo) Setup()
    {
        var proselytizer = new MemeBuilder().WithName("Proselytizer").Build();
        EnhancedIdeologyDefOf.Proselytizer = proselytizer;
        EnhancedIdeologyDefOf.EB_ProselytizerDebated = new ThoughtDef { defName = "EB_ProselytizerDebated", thoughtClass = typeof(Thought_MemeMemory) };
        EnhancedIdeologyDefOf.EB_ProselytizerConverted = new ThoughtDef { defName = "EB_ProselytizerConverted", thoughtClass = typeof(Thought_MemeMemory) };

        var world = new SimWorld();
        world.Initialize();
        var proselytizerIdeo = new IdeoBuilder().WithName("Preaching").AddMeme(proselytizer).Build();
        var otherIdeo = new IdeoBuilder().WithName("Quiet").AddMeme(new MemeBuilder().WithName("Quietude").Build()).Build();
        world.AddIdeo(proselytizerIdeo);
        world.AddIdeo(otherIdeo);
        return (world, proselytizerIdeo, otherIdeo);
    }
}
