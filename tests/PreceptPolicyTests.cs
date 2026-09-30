namespace EnhancedIdeology.Tests;

// Covers the PreceptPolicy resolver: category classification, the rung-order fix for scrambled stacks, and
// how the category gates structural opinion (preceptPolicy.md).
public class PreceptPolicyTests : SeededTest
{
    [Fact]
    public void CategoryOf_ReadsHardcodedTables()
    {
        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(new IssueDef { defName = "Cannibalism" }));
        Assert.Equal(PreceptCategory.UniversalPositive, PreceptPolicy.CategoryOf(new IssueDef { defName = "Charity" }));
        Assert.Equal(PreceptCategory.Special, PreceptPolicy.CategoryOf(new IssueDef { defName = "PreferredXenotypes" }));
        Assert.Equal(PreceptCategory.NA, PreceptPolicy.CategoryOf(new IssueDef { defName = "IdeoBuilding" }));
        Assert.Equal(PreceptCategory.Moral, PreceptPolicy.CategoryOf(new IssueDef { defName = "MarriageName" }));
    }

    [Fact]
    public void CategoryOf_UnknownIssue_DefaultsToPositiveOnly()
    {
        Assert.Equal(PreceptCategory.PositiveOnly,
            PreceptPolicy.CategoryOf(new IssueDef { defName = "AM_BookReadingSpeed" }));
    }

    [Fact]
    public void OrderOverride_FixesScrambledLadder()
    {
        var issue = new IssueDef { defName = "AnimalSlaughter" };
        // Register with the scrambled display order: Alpha Memes' `desired` (pro) appended at 30, above prohibited.
        foreach (var (name, order) in new[]
        {
            ("AnimalSlaughter_Disapproved", 0), ("AnimalSlaughter_Horrible", 10),
            ("AnimalSlaughter_Prohibited", 20), ("AM_AnimalSlaughter_Desired", 30),
        })
        {
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order });
        }

        var rungs = PreceptLadder.Rungs(issue).Select(p => p.defName).ToArray();

        // Policy pulls the pro rung to rank 0, restoring permissive -> forbidding.
        Assert.Equal(
            new[] { "AM_AnimalSlaughter_Desired", "AnimalSlaughter_Disapproved", "AnimalSlaughter_Horrible", "AnimalSlaughter_Prohibited" },
            rungs);
    }

    [Fact]
    public void EveryOptionalOrderOverrideIssueHasADontCareEntry()
    {
        // OrderOverrides and DontCare are two separate tables keyed by the same issue defName, ~50 lines
        // apart, with nothing forcing them to stay in sync. A mandatory issue (every ideo always holds a
        // precept) intentionally has no DontCare entry; anything else needs one, or an ideo silent on it
        // silently falls back to the hardcoded -1f default instead of a deliberate placement.
        var mandatoryIssues = new HashSet<string>
        {
            "Corpses", "InsectMeat", "MarriageName", "SpouseCount_Male", "SpouseCount_Female",
            "OrganUse", "FungusEating",
        };

        var missingDontCare = PreceptPolicy.OrderOverrides.Keys
            .Where(issue => !mandatoryIssues.Contains(issue) && !PreceptPolicy.DontCare.ContainsKey(issue))
            .ToList();

        Assert.Empty(missingDontCare);
    }

    [Fact]
    public void OrderOverride_ResolvesTwoWayTiesAtDefaultOrder()
    {
        // Each pair shares no <displayOrderInIssue> (defaults to 0 for both). Registering the second-listed
        // rung first proves the override - not DefDatabase/registration order - decides the sequence.
        foreach (var (issueName, first, second) in new[]
        {
            ("Bonding", "Bonding_Disapproved", "AM_Bonding_Abhorrent"),
            ("Eclipse", "Eclipse_Beautiful", "VME_Eclipse_Despised"),
            ("GauranlenConnection", "GauranlenConnection_Strong", "AM_GauranlenConnection_Forbidden"),
            ("Trees", "Trees_Desired", "AM_Trees_Despised"),
        })
        {
            var issue = new IssueDef { defName = issueName };
            SimIssues.Register(new PreceptDef { defName = second, issue = issue });
            SimIssues.Register(new PreceptDef { defName = first, issue = issue });
            Assert.Equal(new[] { first, second }, PreceptLadder.Rungs(issue).Select(p => p.defName));
        }
    }

    [Fact]
    public void OrderOverride_FungusEating_ResolvesTieAndOrder()
    {
        var issue = new IssueDef { defName = "FungusEating" };
        foreach (var (name, order) in new[]
        {
            ("VME_FungusEating_DontCare", 5), ("FungusEating_Despised", 10),
            ("AM_FungusEating_Required", 0), ("FungusEating_Preferred", 0),
        })
        {
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order });
        }

        Assert.Equal(
            new[] { "AM_FungusEating_Required", "FungusEating_Preferred", "VME_FungusEating_DontCare", "FungusEating_Despised" },
            PreceptLadder.Rungs(issue).Select(p => p.defName));
    }

    [Fact]
    public void OrderOverride_Ranching_ResolvesTieAndInterleave()
    {
        var issue = new IssueDef { defName = "Ranching" };
        foreach (var (name, order) in new[]
        {
            ("VME_Ranching_Disliked", 10), ("AM_Ranching_CattleCentered", 0),
            ("Ranching_Central", 0), ("VME_Ranching_Nomadic", 30),
        })
        {
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order });
        }

        // Raw order would put the anti rung (Disliked, order 10) before the pro rung (Nomadic, order 30) -
        // the override fixes the axis.
        Assert.Equal(
            new[] { "Ranching_Central", "AM_Ranching_CattleCentered", "VME_Ranching_Nomadic", "VME_Ranching_Disliked" },
            PreceptLadder.Rungs(issue).Select(p => p.defName));
    }

    [Fact]
    public void OrderOverride_OrganUse_PutsTorturousAtProExtremeNotAfterAbhorrent()
    {
        var issue = new IssueDef { defName = "OrganUse" };
        foreach (var (name, order) in new[]
        {
            ("OrganUse_Acceptable", 0), ("OrganUse_HorribleSellOK", 10), ("OrganUse_HorribleNoSell", 20),
            ("OrganUse_Abhorrent", 30), ("AM_OrganUse_Torturous", 40), ("VME_OrganUse_PostMortem", 40),
            ("OrganUse_Respected", 0),
        })
        {
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order });
        }

        // Torturous ("harvesting a still-living enemy's organs... should be encouraged") is a pro-harvest
        // extreme - it must sort at the front, ahead of Respected, not tacked on after Abhorrent.
        Assert.Equal(
            new[]
            {
                "AM_OrganUse_Torturous", "OrganUse_Respected", "OrganUse_Acceptable", "VME_OrganUse_PostMortem",
                "OrganUse_HorribleSellOK", "OrganUse_HorribleNoSell", "OrganUse_Abhorrent",
            },
            PreceptLadder.Rungs(issue).Select(p => p.defName));
    }

    [Fact]
    public void OrderOverride_UnlistedRungs_AppendedByDisplayOrder()
    {
        // An issue with no override keeps plain display-order sorting.
        var (issue, _) = SimIssues.Ladder("Diet", "A", "B", "C");
        Assert.Equal(new[] { "A", "B", "C" }, PreceptLadder.Rungs(issue).Select(p => p.defName));
    }

    [Fact]
    public void InsectMeat_LadderResolvesTieAndIncludesClassicDespised()
    {
        var issue = new IssueDef { defName = "InsectMeat" };
        foreach (var (name, order, classic) in new (string, int, bool)[]
        {
            ("AM_InsectMeatEating_Required", 0, false), ("InsectMeatEating_Loved", 0, false),
            ("VME_InsectMeatEating_DontCare", 0, false), ("InsectMeatEating_Despised_Classic", 10, true),
            ("VME_InsectMeatEating_Sacrilegious", 50, false),
        })
        {
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order, classic = classic });
        }

        // Despised_Classic is the only rung between neutral and Sacrilegious - dropping it (or leaving the
        // three-way order-0 tie unresolved) would silently collapse or misorder the ladder.
        Assert.Equal(
            new[]
            {
                "AM_InsectMeatEating_Required", "InsectMeatEating_Loved", "VME_InsectMeatEating_DontCare",
                "InsectMeatEating_Despised_Classic", "VME_InsectMeatEating_Sacrilegious",
            },
            PreceptLadder.Rungs(issue).Select(p => p.defName));
    }

    [Fact]
    public void Corpses_LadderResolvesTieAndIncludesClassicUgly()
    {
        var issue = new IssueDef { defName = "Corpses" };
        foreach (var (name, order, classic) in new (string, int, bool)[]
        {
            ("Corpses_Ugly", 10, true), ("Corpses_DontCare", 10, false), ("AM_Corpses_Sublime", 20, false),
        })
        {
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order, classic = classic });
        }

        // Ugly (classic) and DontCare tie at order 10 - the override breaks the tie explicitly rather than
        // relying on DefDatabase iteration order.
        Assert.Equal(
            new[] { "Corpses_Ugly", "Corpses_DontCare", "AM_Corpses_Sublime" },
            PreceptLadder.Rungs(issue).Select(p => p.defName));
    }

    [Fact]
    public void NutrientPasteEating_LadderIncludesClassicDisgusting()
    {
        var issue = new IssueDef { defName = "NutrientPasteEating" };
        foreach (var (name, order, classic) in new (string, int, bool)[]
        {
            ("AM_NutrientPasteEating_Preferred", -20, false), ("AM_NutrientPasteEating_Indifferent", -10, false),
            ("NutrientPasteEating_DontMind", 0, false), ("NutrientPasteEating_Disgusting", 10, true),
            ("AM_NutrientPasteEating_Forbidden", 30, false),
        })
        {
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order, classic = classic });
        }

        // Raw display order is already monotonic once the classic rung is kept - no OrderOverride needed here.
        Assert.Equal(
            new[]
            {
                "AM_NutrientPasteEating_Preferred", "AM_NutrientPasteEating_Indifferent",
                "NutrientPasteEating_DontMind", "NutrientPasteEating_Disgusting", "AM_NutrientPasteEating_Forbidden",
            },
            PreceptLadder.Rungs(issue).Select(p => p.defName));
    }

    [Theory]
    [InlineData("Male")]
    [InlineData("Female")]
    public void SpouseCount_LadderIncludesClassicMaxOne(string gender)
    {
        var issue = new IssueDef { defName = $"SpouseCount_{gender}" };
        foreach (var (suffix, order, classic) in new (string, int, bool)[]
        {
            ("MaxOne", 0, true), ("Unlimited", 10, false), ("MaxFour", 30, false),
            ("MaxThree", 50, false), ("MaxTwo", 70, false),
        })
        {
            SimIssues.Register(new PreceptDef
            {
                defName = $"SpouseCount_{gender}_{suffix}", issue = issue, displayOrderInIssue = order, classic = classic,
            });
        }

        // MaxOne is classic-only; the existing OrderOverride was dead code until IncludeClassicInLadder kept
        // it in the ladder.
        Assert.Equal(
            new[] { "MaxOne", "MaxTwo", "MaxThree", "MaxFour", "Unlimited" }.Select(s => $"SpouseCount_{gender}_{s}"),
            PreceptLadder.Rungs(issue).Select(p => p.defName));
    }

    [Theory]
    [InlineData("Male")]
    [InlineData("Female")]
    public void Nudity_LadderIncludesClassicMiddleRung(string gender)
    {
        var issue = new IssueDef { defName = $"Nudity_{gender}" };
        foreach (var (suffix, order, classic) in new (string, int, bool)[]
        {
            ("UncoveredGroinChestHairOrFaceDisapproved", 0, false), ("UncoveredGroinChestOrHairDisapproved", 20, false),
            ("UncoveredGroinOrChestDisapproved", 40, false), ("UncoveredGroinDisapproved", 60, false),
            ("NoRules", 80, false), ("CoveringAnythingButGroinDisapproved", 100, false), ("Mandatory", 120, false),
        })
        {
            // Vanilla flags a different one of these classic per gender (Female: UncoveredGroinOrChest; Male:
            // UncoveredGroinDisapproved) - each gender's own ladder still loses its own middle rung either way.
            var isClassic = classic || suffix == (gender == "Male" ? "UncoveredGroinDisapproved" : "UncoveredGroinOrChestDisapproved");
            SimIssues.Register(new PreceptDef
            {
                defName = $"Nudity_{gender}_{suffix}", issue = issue, displayOrderInIssue = order, classic = isClassic,
            });
        }

        Assert.Equal(
            new[]
            {
                "UncoveredGroinChestHairOrFaceDisapproved", "UncoveredGroinChestOrHairDisapproved",
                "UncoveredGroinOrChestDisapproved", "UncoveredGroinDisapproved", "NoRules",
                "CoveringAnythingButGroinDisapproved", "Mandatory",
            }.Select(s => $"Nudity_{gender}_{s}"),
            PreceptLadder.Rungs(issue).Select(p => p.defName));
    }

    [Fact]
    public void MarriageName_LadderIncludesClassicUsuallyMans()
    {
        var issue = new IssueDef { defName = "MarriageName" };
        var defs = new Dictionary<string, PreceptDef>();
        foreach (var (name, order, classic) in new (string, int, bool)[]
        {
            ("MarriageName_AlwaysMans", 40, false), ("MarriageName_UsuallyMans", 0, true),
            ("MarriageName_Random", 0, false), ("MarriageName_KeepNames", 10, false),
            ("MarriageName_UsuallyWomans", 20, false), ("MarriageName_AlwaysWomans", 30, false),
        })
        {
            var def = new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order, classic = classic };
            SimIssues.Register(def);
            defs[name] = def;
        }

        // UsuallyMans is classic-only (no real Ideology-selectable equivalent) but must stay in the ladder
        // (PreceptPolicy.IncludeClassicInLadder) to anchor the spacing correctly.
        Assert.Equal(
            new[]
            {
                "MarriageName_AlwaysMans", "MarriageName_UsuallyMans", "MarriageName_Random",
                "MarriageName_KeepNames", "MarriageName_UsuallyWomans", "MarriageName_AlwaysWomans",
            },
            PreceptLadder.Rungs(issue).Select(p => p.defName));

        // Ranks 0-5: Random/KeepNames (2/3) sit symmetric around the true center, rather than being squeezed
        // off-center by silently dropping the unselectable UsuallyMans slot.
        Assert.Equal(0f, PreceptLadder.RankOf(defs["MarriageName_AlwaysMans"]));
        Assert.Equal(2f, PreceptLadder.RankOf(defs["MarriageName_Random"]));
        Assert.Equal(3f, PreceptLadder.RankOf(defs["MarriageName_KeepNames"]));
        Assert.Equal(5f, PreceptLadder.RankOf(defs["MarriageName_AlwaysWomans"]));
    }

    [Fact]
    public void DontCareRank_DrugUse_IsMidLadder()
    {
        var issue = new IssueDef { defName = "DrugUse" };
        foreach (var (name, order) in new[]
        {
            ("DrugUse_Essential", 0), ("DrugUse_MedicalOrSocial", 10),
            ("DrugUse_MedicalOnly", 20), ("DrugUse_Prohibited", 30),
        })
        {
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order });
        }

        // Between medical-or-social (rank 1) and medical-only (rank 2) -> midpoint 1.5.
        Assert.Equal(1.5f, PreceptLadder.DontCareRank(issue));
    }

    [Fact]
    public void DontCareSpec_ResolvesBeforeAndBetweenOnRegisteredLadder()
    {
        var issue = new IssueDef { defName = "Fishing" };
        foreach (var (name, order) in new[]
        {
            ("Fishing_Prohibited", 0), ("Fishing_Disapproved", 10), ("Fishing_Sacred", 30),
        })
        {
            SimIssues.Register(new PreceptDef { defName = name, issue = issue, displayOrderInIssue = order });
        }

        // Before(rank 0) -> -0.5.
        Assert.Equal(-0.5f, DontCareSpec.Before("Fishing_Prohibited").Resolve(issue));
        // Between disapproved (rank 1) and sacred (rank 2) -> 1.5.
        Assert.Equal(1.5f, DontCareSpec.Between("Fishing_Disapproved", "Fishing_Sacred").Resolve(issue));
    }

    [Fact]
    public void DontCareRank_UnlistedIssue_IsPermissiveExtreme()
    {
        Assert.Equal(-1f, PreceptLadder.DontCareRank(new IssueDef { defName = "Nothing" }));
    }

    [Fact]
    public void SpecialOpinion_Leader_IsCategorical()
    {
        var (issue, _) = SimIssues.Ladder("VME_Leader",
            "VME_Leader_HighestTitle", "VME_Leader_BestPsycaster", "VME_Leader_Godlike");

        // Matching leader-selection agrees; any difference (or one side having none, rank -1) is a full clash.
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 0f, 0f, 10f, 0.5f, out var same));
        Assert.Equal(10f, same);
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 0f, 2f, 10f, 0.5f, out var differ));
        Assert.Equal(-10f, differ);
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, -1f, 0f, 10f, 0.5f, out var none));
        Assert.Equal(-10f, none);
    }

    [Fact]
    public void SpecialOpinion_Mood_LinearAxisGradesByDistance()
    {
        var issue = MoodLadder();

        // High(0) vs High(0): full agreement. High(0) vs Low(2): opposite ends of the linear axis -> full
        // opposition (-strength) at oppositionScale 1.
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 0f, 0f, 10f, 1f, out var agree));
        Assert.Equal(10f, agree);
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 0f, 2f, 10f, 1f, out var opposite));
        Assert.Equal(-10f, opposite);
    }

    [Fact]
    public void SpecialOpinion_Mood_PariahsClashWithEverythingButThemselves()
    {
        var issue = MoodLadder();

        // Shared(3) vs a linear rung -> clash; vs the other pariah -> clash; vs itself -> agreement.
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 3f, 1f, 10f, 0.5f, out var vsLinear));
        Assert.Equal(-10f, vsLinear);
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 3f, 4f, 10f, 0.5f, out var vsOtherPariah));
        Assert.Equal(-10f, vsOtherPariah);
        Assert.True(PreceptPolicy.TrySpecialOpinion(issue, 3f, 3f, 10f, 0.5f, out var vsSelf));
        Assert.Equal(10f, vsSelf);
    }

    [Fact]
    public void SpecialOpinion_UnmodelledIssue_IsSkipped()
    {
        Assert.False(PreceptPolicy.TrySpecialOpinion(
            new IssueDef { defName = "Weapons" }, 0f, 1f, 10f, 0.5f, out var opinion));
        Assert.Equal(0f, opinion);
    }

    private static IssueDef MoodLadder()
    {
        var (issue, _) = SimIssues.Ladder("VME_Mood",
            "VME_Mood_HighExpectations", "VME_Mood_Normal", "VME_Mood_LowExpectations",
            "VME_Mood_Shared", "VME_Mood_DictatedByStars");
        return issue;
    }

    [Fact]
    public void MoralIssue_FeedsStructuralOpinion()
    {
        Assert.True(OwnStructuralWithCategory(PreceptCategory.Moral) > 0f);
    }

    [Fact]
    public void PositiveOnlyIssue_ContributesNothingStructural()
    {
        // No Moral issues -> nothing to average -> 0 structural.
        Assert.Equal(0f, OwnStructuralWithCategory(PreceptCategory.PositiveOnly));
    }

    [Fact]
    public void NAIssue_ContributesNothingStructural()
    {
        Assert.Equal(0f, OwnStructuralWithCategory(PreceptCategory.NA));
    }

    // Own-ideo structural opinion when the sole issue is classified as `category`. A single-issue world, so a
    // Moral issue yields ~mean(strength)*5 and a non-Moral one yields 0.
    private static float OwnStructuralWithCategory(PreceptCategory category)
    {
        Rand.SetSeed(1);
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("Diet", "A", "B", "C");
        var ideo = new IdeoBuilder().WithName("Own").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        var mirror = new IdeoBuilder().WithName("Mirror").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        world.AddIdeo(ideo);
        world.AddIdeo(mirror);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        // Override the Moral default SimIssues assigned, then read.
        PreceptPolicy.RegisterCategory("Diet", category);
        return tracker.StructuralIdeoOpinion(mirror);
    }
}
