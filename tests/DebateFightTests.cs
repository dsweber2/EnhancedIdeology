namespace EnhancedIdeology.Tests;

public class DebateFightTests : SeededTest
{
    [Fact]
    public void FullLadderDraw_ZeroSocial_IsBaseChance()
    {
        var (initiator, recipient) = Pair(sameFaith: true, initiatorSocial: 0, recipientSocial: 0);

        var chance = DebateFights.Chance(new InteractionDef(), initiator, recipient, DebateFights.ReferenceTopicWeight);

        Assert.Equal(DebateFights.DrawFightBaseChance, chance, 5);
    }

    [Fact]
    public void MoreAdeptDebater_SetsSocialFalloff()
    {
        var (initiator, recipient) = Pair(sameFaith: true, initiatorSocial: 0, recipientSocial: 10);

        var chance = DebateFights.Chance(new InteractionDef(), initiator, recipient, DebateFights.ReferenceTopicWeight);

        Assert.Equal(DebateFights.DrawFightBaseChance / (1f + (DebateFights.SocialFalloff * 10f)), chance, 5);
    }

    [Fact]
    public void PawnFightChances_AreAveraged()
    {
        var (initiator, recipient) = Pair(sameFaith: true, initiatorSocial: 0, recipientSocial: 0);
        recipient.interactions.fightChanceFactor = 3f;

        var chance = DebateFights.Chance(new InteractionDef(), initiator, recipient, DebateFights.ReferenceTopicWeight);

        Assert.Equal(DebateFights.DrawFightBaseChance * 2f, chance, 5);
    }

    [Fact]
    public void PettyDisagreement_ScalesDownWithTopicWeight()
    {
        var (initiator, recipient) = Pair(sameFaith: true, initiatorSocial: 0, recipientSocial: 0);

        var chance = DebateFights.Chance(new InteractionDef(), initiator, recipient, DebateFights.ReferenceTopicWeight / 4f);

        Assert.Equal(DebateFights.DrawFightBaseChance / 4f, chance, 5);
    }

    [Theory]
    [InlineData(true, 1f)]
    [InlineData(false, 1f + (2f * DebateFights.ApostacyFightFactorPerFaith))]
    public void StrictApostasy_RaisesChanceOnlyAcrossFaiths(bool sameFaith, float expectedFactor)
    {
        var world = new SimWorld();
        world.Initialize();
        var (_, rungs) = SimIssues.Ladder("Apostasy", "VME_Apostasy_Accepted", "Apostasy_Disapproved", "ApostasyStrict");
        var ideo = new IdeoBuilder().WithName("StrictA").AddPrecept(rungs[2]).Build();
        world.AddIdeo(ideo);
        var otherIdeo = ideo;
        if (!sameFaith)
        {
            otherIdeo = new IdeoBuilder().WithName("StrictB").AddPrecept(rungs[2]).Build();
            world.AddIdeo(otherIdeo);
        }
        var initiator = new PawnBuilder().WithIdeo(ideo).WithSkill(SkillDefOf.Social, 0).WithLabel("Init").Build(world);
        var recipient = new PawnBuilder().WithIdeo(otherIdeo).WithSkill(SkillDefOf.Social, 0).WithLabel("Recip").Build(world);
        initiator.interactions.fightChanceFactor = 1f;
        recipient.interactions.fightChanceFactor = 1f;

        var chance = DebateFights.Chance(new InteractionDef(), initiator, recipient, DebateFights.ReferenceTopicWeight);

        Assert.Equal(DebateFights.DrawFightBaseChance * expectedFactor, chance, 5);
    }

    [Fact]
    public void Chance_LeavesDefBaseChanceAtZero()
    {
        // Vanilla rolls its own fight after every interaction from the def's base chance, so it must stay zero.
        var (initiator, recipient) = Pair(sameFaith: true, initiatorSocial: 0, recipientSocial: 0);
        var interaction = new InteractionDef();

        DebateFights.Chance(interaction, initiator, recipient, DebateFights.ReferenceTopicWeight);

        Assert.Equal(0f, interaction.socialFightBaseChance);
    }

    [Theory]
    [InlineData("IdeoDiversity_Standard", false, 1f)]
    [InlineData("IdeoDiversity_Approved", false, 1f - (0.8f / 3f))]
    [InlineData("IdeoDiversity_Exalted", false, DebateFights.TolerantFightFactor)]
    [InlineData("IdeoDiversity_Exalted", true, DebateFights.TolerantFightFactor)]
    [InlineData("IdeoDiversity_Abhorrent", false, 1f)]
    [InlineData("IdeoDiversity_Disapproved", true, 1f + (1f / 3f))]
    [InlineData("IdeoDiversity_Abhorrent", true, DebateFights.BigotedFightFactor)]
    public void DiversityFightFactor_TolerantAlways_BigotedOnlyCrossFaith(string rung, bool crossFaith, float expected)
    {
        var world = new SimWorld();
        world.Initialize();
        var (_, rungs) = SimIssues.Ladder("IdeoDiversity",
            "IdeoDiversity_Abhorrent", "IdeoDiversity_Horrible", "IdeoDiversity_Disapproved", "IdeoDiversity_Standard",
            "IdeoDiversity_Approved", "IdeoDiversity_Respected", "IdeoDiversity_Exalted");
        var ideo = new IdeoBuilder().WithName("Diverse").AddPrecept(rungs.First(def => def.defName == rung)).Build();
        world.AddIdeo(ideo);

        Assert.Equal(expected, DebateFights.DiversityFightFactor(ideo, crossFaith), 4);
    }

    [Fact]
    public void NoDiversityPrecept_FactorIsOne()
    {
        var world = new SimWorld();
        world.Initialize();
        var ideo = new IdeoBuilder().WithName("Silent").Build();
        world.AddIdeo(ideo);

        Assert.Equal(1f, DebateFights.DiversityFightFactor(ideo, crossFaith: true));
    }

    // Two healthy pawns at neutral opinion: vanilla's factors multiply the base chance by 1.
    private static (Pawn initiator, Pawn recipient) Pair(bool sameFaith, int initiatorSocial, int recipientSocial)
    {
        var world = new SimWorld();
        world.Initialize();
        var ideo = new IdeoBuilder().WithName("FightIdeo").Build();
        world.AddIdeo(ideo);
        var otherIdeo = sameFaith ? ideo : new IdeoBuilder().WithName("OtherFightIdeo").Build();
        if (!sameFaith)
            world.AddIdeo(otherIdeo);

        var initiator = new PawnBuilder().WithIdeo(ideo).WithSkill(SkillDefOf.Social, initiatorSocial).WithLabel("Init").Build(world);
        var recipient = new PawnBuilder().WithIdeo(otherIdeo).WithSkill(SkillDefOf.Social, recipientSocial).WithLabel("Recip").Build(world);
        initiator.interactions.fightChanceFactor = 1f;
        recipient.interactions.fightChanceFactor = 1f;
        return (initiator, recipient);
    }
}
