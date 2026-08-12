namespace EnhancedIdeology.Tests;

// Covers the payload-based Special issues (preceptPolicy.md "Special"): Weapons (noble/despised pairs) and
// PreferredXenotypes (overlapping vs disjoint preferences), which compare whole precept payloads rather than
// a rung on a ladder.
public class SpecialPayloadTests : SeededTest
{
    [Fact]
    public void Weapons_SamePair_Agrees()
    {
        var weapons = SimIssues.Special("Weapons");
        var (gun, blade) = (new WeaponClassDef { defName = "Gun" }, new WeaponClassDef { defName = "Blade" });
        var mine = new IdeoBuilder().WithName("Gunners").AddWeaponPrecept(weapons, gun, blade).Build();
        var theirs = new IdeoBuilder().WithName("AlsoGunners").AddWeaponPrecept(weapons, gun, blade).Build();

        Assert.True(PreceptPolicy.TryPayloadSpecialOpinion(weapons, mine, theirs, 10f, out var opinion));
        Assert.Equal(10f, opinion);
    }

    [Fact]
    public void Weapons_OppositePair_Clashes()
    {
        var weapons = SimIssues.Special("Weapons");
        var (gun, blade) = (new WeaponClassDef { defName = "Gun" }, new WeaponClassDef { defName = "Blade" });
        var mine = new IdeoBuilder().WithName("Gunners").AddWeaponPrecept(weapons, gun, blade).Build();
        var theirs = new IdeoBuilder().WithName("Bladers").AddWeaponPrecept(weapons, blade, gun).Build();

        Assert.True(PreceptPolicy.TryPayloadSpecialOpinion(weapons, mine, theirs, 10f, out var opinion));
        Assert.Equal(-10f, opinion);
    }

    [Fact]
    public void Weapons_DisjointClasses_DontCare()
    {
        var weapons = SimIssues.Special("Weapons");
        var (gun, blade, bow, spear) = (
            new WeaponClassDef { defName = "Gun" }, new WeaponClassDef { defName = "Blade" },
            new WeaponClassDef { defName = "Bow" }, new WeaponClassDef { defName = "Spear" });
        var mine = new IdeoBuilder().WithName("Gunners").AddWeaponPrecept(weapons, gun, blade).Build();
        var theirs = new IdeoBuilder().WithName("Archers").AddWeaponPrecept(weapons, bow, spear).Build();

        // Nothing shared or opposed -> skipped entirely, not counted as neutral.
        Assert.False(PreceptPolicy.TryPayloadSpecialOpinion(weapons, mine, theirs, 10f, out _));
    }

    [Fact]
    public void Weapons_OnlyOneSideHoldsPrecept_DontCare()
    {
        var weapons = SimIssues.Special("Weapons");
        var (gun, blade) = (new WeaponClassDef { defName = "Gun" }, new WeaponClassDef { defName = "Blade" });
        var mine = new IdeoBuilder().WithName("Gunners").AddWeaponPrecept(weapons, gun, blade).Build();
        var theirs = new IdeoBuilder().WithName("Indifferent").Build();

        Assert.False(PreceptPolicy.TryPayloadSpecialOpinion(weapons, mine, theirs, 10f, out _));
    }

    [Fact]
    public void Xenotype_SamePreference_Agrees()
    {
        var xeno = SimIssues.Special("PreferredXenotypes");
        var hussar = new XenotypeDef { defName = "Hussar" };
        var mine = new IdeoBuilder().WithName("HussarLovers").AddXenotypePrecept(xeno, hussar).Build();
        var theirs = new IdeoBuilder().WithName("AlsoHussar").AddXenotypePrecept(xeno, hussar).Build();

        Assert.True(PreceptPolicy.TryPayloadSpecialOpinion(xeno, mine, theirs, 10f, out var opinion));
        Assert.Equal(10f, opinion);
    }

    [Fact]
    public void Xenotype_DisjointPreference_Clashes()
    {
        var xeno = SimIssues.Special("PreferredXenotypes");
        var mine = new IdeoBuilder().WithName("HussarLovers")
            .AddXenotypePrecept(xeno, new XenotypeDef { defName = "Hussar" }).Build();
        var theirs = new IdeoBuilder().WithName("SanguophageLovers")
            .AddXenotypePrecept(xeno, new XenotypeDef { defName = "Sanguophage" }).Build();

        Assert.True(PreceptPolicy.TryPayloadSpecialOpinion(xeno, mine, theirs, 10f, out var opinion));
        Assert.Equal(-10f, opinion);
    }

    [Fact]
    public void Xenotype_PartialOverlap_LeansPositive()
    {
        var xeno = SimIssues.Special("PreferredXenotypes");
        var (hussar, sang) = (new XenotypeDef { defName = "Hussar" }, new XenotypeDef { defName = "Sanguophage" });
        var mine = new IdeoBuilder().WithName("Broad")
            .AddXenotypePrecept(xeno, hussar).AddXenotypePrecept(xeno, sang).Build();
        var theirs = new IdeoBuilder().WithName("Narrow").AddXenotypePrecept(xeno, hussar).Build();

        // Shared 1 of {2,1}: Sorensen 2/3 -> opinion = strength*(2*2/3 - 1) = strength/3, positive but partial.
        Assert.True(PreceptPolicy.TryPayloadSpecialOpinion(xeno, mine, theirs, 12f, out var opinion));
        Assert.Equal(4f, opinion, 3);
    }

    [Fact]
    public void Xenotype_OnlyOneSideHoldsPrecept_DontCare()
    {
        var xeno = SimIssues.Special("PreferredXenotypes");
        var mine = new IdeoBuilder().WithName("HussarLovers")
            .AddXenotypePrecept(xeno, new XenotypeDef { defName = "Hussar" }).Build();
        var theirs = new IdeoBuilder().WithName("Indifferent").Build();

        Assert.False(PreceptPolicy.TryPayloadSpecialOpinion(xeno, mine, theirs, 10f, out _));
    }

    [Fact]
    public void OppositeWeaponPairs_DepressStructuralOpinion()
    {
        Rand.SetSeed(1);
        var world = new SimWorld();
        world.Initialize();

        var weapons = SimIssues.Special("Weapons");
        var (gun, blade) = (new WeaponClassDef { defName = "Gun" }, new WeaponClassDef { defName = "Blade" });
        var mine = new IdeoBuilder().WithName("Gunners").AddWeaponPrecept(weapons, gun, blade).Build();
        var theirs = new IdeoBuilder().WithName("Bladers").AddWeaponPrecept(weapons, blade, gun).Build();
        world.AddIdeo(mine);
        world.AddIdeo(theirs);

        var pawn = new PawnBuilder().WithIdeo(mine).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        // The sole shared issue is a full weapons clash, so the target reads well below the neutral midpoint.
        Assert.True(tracker.StructuralIdeoOpinion(theirs) < 50f);
    }
}
