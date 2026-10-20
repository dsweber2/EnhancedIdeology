namespace EnhancedIdeology;

// A stance one issue's precept induces on another (docs/preceptPolicy.md "Interactions"): holding the source
// precept makes an ideo behave, on the target issue, as if it took the given rung - unless it already takes an
// explicit stance there. The induced rank is resolved against the live target ladder so it survives
// reordering; a "beyond" spec deliberately sits past the ladder end to read as an extreme.
internal readonly struct InducedStance
{
    private enum Kind { Rung, BeyondDontCare }

    public readonly string TargetIssue;
    private readonly Kind _kind;
    private readonly string? _rung;
    private readonly float _offset;

    private InducedStance(string targetIssue, Kind kind, string? rung, float offset)
    {
        TargetIssue = targetIssue;
        _kind = kind;
        _rung = rung;
        _offset = offset;
    }

    // Behave as if holding a named rung on the target issue.
    public static InducedStance Rung(string targetIssue, string rung) => new(targetIssue, Kind.Rung, rung, 0f);

    // Sit `offset` rungs past the target issue's Don't-care rung (negative = further into the permissive end).
    public static InducedStance BeyondDontCare(string targetIssue, float offset) =>
        new(targetIssue, Kind.BeyondDontCare, null, offset);

    public float Resolve(IssueDef issue) => _kind switch
    {
        Kind.Rung => PreceptLadder.RankOfName(issue, _rung!),
        _ => PreceptLadder.DontCareRank(issue) + _offset,
    };
}

// How an issue contributes to opinion (docs/preceptPolicy.md). Only Moral issues feed the structural
// rung-distance model; everything else is inert on the structural read path.
internal enum PreceptCategory
{
    Moral,            // two-sided disagreement axis -> rung-distance opinion
    PositiveOnly,     // 0 structural; only accrues positive via the acquired channel (the default)
    UniversalPositive,// flat + for everyone (Charity)
    Special,          // bespoke, skipped by the generic resolver for now
    NA,               // not a belief stance (buildings, ritual seats, naming) -> excluded
}

// Per-issue opinion policy: category classification plus the rung-order fixes the Moral issues need where
// stacking scrambles displayOrderInIssue. Keyed by defName so it survives load order. See docs/preceptPolicy.md.
internal static partial class PreceptPolicy
{
    // Everything not listed defaults to PositiveOnly (0 structural). This is the curated Moral set.
    private static readonly HashSet<string> MoralIssues =
    [
        "Cannibalism", "MeatEating", "AnimalSlaughter", "KillingInnocentAnimals", "Slavery", "Execution",
        "OrganUse", "DrugUse", "ChildLabor", "Lovin", "Nudity_Male", "Nudity_Female", "SpouseCount_Male",
        "SpouseCount_Female", "Scarification", "Apostasy", "IdeoDiversity", "Corpses", "FungusEating",
        "InsectMeat", "NutrientPasteEating", "BodyModification", "Raiding", "AutonomousWeapons", "Fishing",
        "GrowthVat", "Skullspike", "Bloodfeeders", "Biosculpting", "Bonding", "Trees", "RoughLiving",
        "GauranlenConnection", "Eclipse", "Ranching", "Mining", "TreeCutting",
        // Multi-rung mod issues with a genuine value axis (docs/preceptPolicy.md "Mod issues"). Single-rung and
        // mechanical (*Speed/*Yield/perk) mod issues fall through to the PositiveOnly default.
        "VME_Alcohol", "VME_Violence", "VME_Recreation", "VME_KillingWithFire", "VME_LeatherApparel",
        "VME_Scars", "VME_Elders", "VME_Royalty", "VME_Mechanoids", "VME_Insectoids",
        "VME_Fire", "VME_Firefighting", "VME_TaintedApparel", "VME_TatteredApparel", "AM_Religion",
        "AM_AnimalRelease",
        // Lifestyle/aesthetic mod issues David gave an explicit Don't-care placement (docs/preceptPolicy.md).
        "VME_Expectations", "AM_Rain", "VME_Aurora", "VME_BookReading", "VME_BookReadingSpeed",
        "VME_BookWriting", "VME_Travel", "VME_PermanentBases",
        // Mod Moral issues with a defaultSelectionWeight rung. That rung is the neutral (PreceptPolicy.DontCare).
        // A couple - VME_Recreation, VME_Illness - run reversed, but OpinionOnPrecept is symmetric under axis
        // reflection. See docs/preceptPolicy.md "Order-fix candidates (verified)".
        "AM_FertilityIssue", "AM_LearningRate", "AM_LovinFrequency", "AM_Creep", "AM_Disfigurement",
        "VME_Illness", "VME_InsectJelly", "VME_Sweets", "VME_DumbLabor", "AM_OcularTrees",
        // Anomaly DLC optional Moral issues (defNames assumed from vanilla naming conventions; verified against
        // MechanoidLabor as a reference — degrade gracefully if Anomaly is not loaded).
        "PsychicRituals", "VoidStudy", "Inhumanizing",
        // Mort's Ideologies: Conservationist and Polluter (MortStrudel.MortIdeologyEnv).
        // MI_PowerGeneration (two restriction-only rungs, no opposing pro-pollution rung) → PositiveOnly.
        "MI_Pollution", "MI_ToxicWasteDumping",
        // Mort's Ideologies: Political Compass (MortStrudel.MortIdeology).
        // MI_Homelessness (single rung) → PositiveOnly. Empiricism/Faith and Menagerist issues → PositiveOnly.
        "miHousingDistribution", "MI_Leader",
        // Questing Meme (SirMashedPotato.QuestingMeme). Both issues have all rungs at displayOrderInIssue=10;
        // order overrides establish the semantic axis.
        "QuesterMeme_QuestComplete", "QuesterMeme_QuestFail",
        // Vanilla Vehicles Expanded (VVE): Indoors (STACKED with VVE_SmallSpaces_Horrible → OrderOverride),
        // VVE_Driving/Flying/Sailing (Required→Forbidden axis, five rungs each).
        "Indoors", "VVE_Driving", "VVE_Flying", "VVE_Sailing",
        // Language Learning mod: Hatred→Condemned→Neutral→Encouraged→Exalted xenophilia axis.
        "LanguageLearning",
        // Romance on the Rim: permissiveness axes on relationship events. MarriageProposal rungs mix
        // timing (LateMarriage/FlashMarriage) with control (Forbidden/Arranged) — no clean axis → PositiveOnly.
        "RomanceOnTheRim_Issue_Breakup", "RomanceOnTheRim_Issue_RomanceAttempt",
        "RomanceOnTheRim_Issue_Cheat",
        // VME_BookWritingSpeed parallels VME_BookReadingSpeed (Increased/Decreased axis).
        "VME_BookWritingSpeed",
        // Nikolai's Ideology: Gender Works. Central (beauty is moral; pawns are judged by looks) vs Unimportant
        // (focusing on beauty corrupts).
        "NikolaisIdeology_Beauty",
        // Man's-name <-> woman's-name spectrum; see OrderOverrides.
        "MarriageName",
        // Former PositiveOnly issues whose rungs make a value claim (docs/preceptPolicy.md "PositiveOnly review").
        "EB_Contemplation", "Research", "Pain", "Blindness", "DarknessCombat", "Lighting", "Proselytizing",
        "Comfort", "AM_Armour", "AM_Barracks", "AM_PsychicSensitivity", "VME_Power", "VME_Junk", "VME_Death",
        "VME_AutomationEfficiency", "VME_CraftingQuality", "VME_PsychicSensitivity", "BS_AlienAppearanceTolerance",
        // Single-rung value claims: a silent outsider objects, so the silent Don't-care rung is the opposing pole.
        "AgeReversal", "AM_Madness", "AM_Death", "AM_DeathrestCaskets", "AM_HarbingerTrees", "AM_AnimaScreams",
        "AM_Reliquaries", "AM_RelicDestruction", "VME_Corruption", "VME_SocialInteractions", "VME_LeaderDivinity",
        "VME_Anonymity", "VFEA_Recruiting", "VFEA_BeingRecruited",
        // War Crimes Expanded 2.
        "WCE2_Torture",
        // Humanoid Alien Races: xenophilia axes on alien meat, romance, presence, and slavery.
        "HAR_EatingAliens", "HAR_AlienDating", "HAR_AlienRaces", "HAR_AlienSlavery",
        // Genetic Rim: acceptance of failed and humanoid genetic experiments.
        "GR_Failures", "GR_HumanoidHybrids",
    ];
    // UniversalPositive issue -> the rung that values it most. A trait that opposes the issue pulls away from it.
    private static readonly Dictionary<string, string> UniversalPositiveValuedRung = new()
    {
        ["Charity"] = "Charity_Essential",
        ["VME_Recreation"] = "VME_Recreation_Loved",
    };

    // Rank of the rung that values a UniversalPositive issue most, or null if the issue has none on the ladder.
    internal static float? ValuedRank(IssueDef issue)
    {
        if (!UniversalPositiveValuedRung.TryGetValue(issue.defName, out var rung)) return null;
        var rank = PreceptLadder.RankOfName(issue, rung);
        return rank >= 0f ? rank : null;
    }
    // Special issues route through the special resolvers instead of the rung-distance model. VME_Leader /
    // VME_Mood are rank-based (categorical / hybrid, via TrySpecialOpinion); Weapons / PreferredXenotypes
    // compare whole precept payloads (via TryPayloadSpecialOpinion).
    private static readonly HashSet<string> SpecialIssues =
        ["PreferredXenotypes", "Weapons", "VME_Leader", "VME_Mood"];
    private static readonly HashSet<string> NAIssues =
        ["IdeoBuilding", "IdeoRelic", "IdeoRitualSeat", "Ritual", "AM_Abilities"];
    // Multi-rung issues confirmed as PositiveOnly: no genuine belief axis, so rung-distance structural opinion
    // would be wrong. Listed here to suppress the startup warning for unclassified multi-rung issues.
    private static readonly HashSet<string> KnownPositiveOnlyIssues =
    [
        // Vanilla / DLC.
        "ApparelDesire", "IdeoRole",
        // Alpha Memes. CombatProwess rungs (ranged / melee / reduced) are trade-offs, not values.
        "AM_CombatProwess",
        // MiningYield: base High + AB_MiningYield_VeryHigh are yield bonuses at different levels.
        "MiningYield",
        // Vanilla Memes Expanded. CraftingSpeed's Slower rung ("never rush") is off the manual-labour axis.
        "VME_CraftingSpeed", "VME_PermitCooldown", "VME_PermitHonorCost", "VME_PsyfocusGain", "VME_SkilledLabor",
        // Mort's Ideologies: Conservationist (MortStrudel.MortIdeologyEnv). Two restriction-only rungs,
        // no opposing pro-pollution rung.
        "MI_PowerGeneration",
        // Vanilla Vehicles Expanded: pure performance modifiers (speed/repair-rate/fuel), no belief axis.
        // RomanceOnTheRim_Issue_MarriageProposal mixes timing (LateMarriage/FlashMarriage) with control
        // (Forbidden/Arranged) — no clean moral axis.
        "VVE_Acceleration", "VVE_VehicleRepairs", "VVE_FuelEfficiency",
        "RomanceOnTheRim_Issue_MarriageProposal",
        // Genetic Rim: two worktable speed perks, no belief axis.
        "GR_WorktableSpeeds",
        // Not locally installed — safe PositiveOnly default pending ladder verification.
        "VRE_AndroidsIssue", "SEX_Divorce",
    ];

    // Rung defName order (permissive/pro -> forbidding/anti) for issues whose displayOrderInIssue scrambles
    // the axis once stacked (docs/preceptPolicy.md "Reorder"). Rungs not listed keep their displayOrder, appended.
    public static readonly Dictionary<string, string[]> OrderOverrides = new()
    {
        ["MeatEating"] =
        [
            "MeatEating_NonMeat_Abhorrent", "MeatEating_NonMeat_Horrible", "MeatEating_NonMeat_Disapproved",
            "MeatEating_Disapproved", "MeatEating_Horrible", "MeatEating_Abhorrent", "VME_MeatEating_Abhorrent_Strict",
        ],
        ["AnimalSlaughter"] =
            ["AM_AnimalSlaughter_Desired", "AnimalSlaughter_Disapproved", "AnimalSlaughter_Horrible", "AnimalSlaughter_Prohibited"],
        // Corpses_Ugly (classic) and Corpses_DontCare tie at displayOrderInIssue=10 - break the tie explicitly
        // rather than leaning on DefDatabase iteration order (mod-load-order dependent).
        ["Corpses"] = ["Corpses_Ugly", "Corpses_DontCare", "AM_Corpses_Sublime"],
        // AM_InsectMeatEating_Required (AlphaMemes), InsectMeatEating_Loved (Ideology), and
        // VME_InsectMeatEating_DontCare (VIE-M&S) all sit at displayOrderInIssue=0 - fix the pro->neutral order.
        ["InsectMeat"] =
        [
            "AM_InsectMeatEating_Required", "InsectMeatEating_Loved", "VME_InsectMeatEating_DontCare",
            "InsectMeatEating_Despised_Classic", "VME_InsectMeatEating_Sacrilegious",
        ],
        ["SpouseCount_Male"] =
            ["SpouseCount_Male_MaxOne", "SpouseCount_Male_MaxTwo", "SpouseCount_Male_MaxThree", "SpouseCount_Male_MaxFour", "SpouseCount_Male_Unlimited"],
        ["SpouseCount_Female"] =
            ["SpouseCount_Female_MaxOne", "SpouseCount_Female_MaxTwo", "SpouseCount_Female_MaxThree", "SpouseCount_Female_MaxFour", "SpouseCount_Female_Unlimited"],
        ["MarriageName"] =
            ["MarriageName_AlwaysMans", "MarriageName_UsuallyMans", "MarriageName_Random", "MarriageName_KeepNames", "MarriageName_UsuallyWomans", "MarriageName_AlwaysWomans"],
        // MI_BodyMod_Allowed (Political Compass) sits at displayOrderInIssue=30, after Abhorrent — insert it
        // between Approved and OnlyBiological.
        ["BodyModification"] =
            ["BodyMod_Approved", "MI_BodyMod_Allowed", "VME_BodyMod_OnlyBiological", "BodyMod_Disapproved", "BodyMod_Abhorrent"],
        // MI_DrugUse_Allowed (Political Compass) sits at displayOrderInIssue=40, beyond Prohibited — fix order.
        ["DrugUse"] =
            ["DrugUse_Essential", "MI_DrugUse_Allowed", "DrugUse_MedicalOrSocial", "DrugUse_MedicalOnly", "DrugUse_Prohibited", "DrugUse_Abhorrent"],
        // AM_OrganUse_Torturous ("harvesting a still-living enemy's organs... should be encouraged") is a
        // pro-harvest extreme, not anti - belongs beyond Respected, not tacked on after Abhorrent.
        ["OrganUse"] =
        [
            "AM_OrganUse_Torturous", "OrganUse_Respected", "OrganUse_Acceptable", "VME_OrganUse_PostMortem",
            "OrganUse_HorribleSellOK", "OrganUse_HorribleNoSell", "OrganUse_Abhorrent",
        ],
        // Bonding_Disapproved (Ideology) and AM_Bonding_Abhorrent (AlphaMemes) both default to order 0.
        ["Bonding"] = ["Bonding_Disapproved", "AM_Bonding_Abhorrent"],
        // Eclipse_Beautiful (Ideology) and VME_Eclipse_Despised (VIE-M&S) both default to order 0.
        ["Eclipse"] = ["Eclipse_Beautiful", "VME_Eclipse_Despised"],
        // AM_FungusEating_Required (AlphaMemes) and FungusEating_Preferred (Ideology) both default to order 0.
        ["FungusEating"] =
        [
            "AM_FungusEating_Required", "FungusEating_Preferred", "VME_FungusEating_DontCare",
            "BMT_FungusEating_DontCare", "FungusEating_Despised",
        ],
        // GauranlenConnection_Strong (Ideology) and AM_GauranlenConnection_Forbidden (AlphaMemes) both
        // default to order 0.
        ["GauranlenConnection"] = ["GauranlenConnection_Strong", "AM_GauranlenConnection_Forbidden"],
        // Ranching_Central/AM_Ranching_CattleCentered tie at default order 0; raw order otherwise interleaves
        // VME_Ranching_Disliked (anti, order 10) before VME_Ranching_Nomadic (pro, order 30).
        ["Ranching"] =
        [
            "Ranching_Central", "AM_Ranching_CattleCentered", "VME_Ranching_Nomadic", "VME_Ranching_Discouraged",
            "VME_Ranching_Disliked",
        ],
        // Trees_Desired (Ideology) and AM_Trees_Despised (AlphaMemes) both default to order 0, and
        // MI_Trees_Mildly_Desired is a milder pro rung.
        ["Trees"] = ["Trees_Desired", "MI_Trees_Mildly_Desired", "AM_Trees_Despised"],
        // Anomaly DLC: no displayOrderInIssue set on any rung, all default to 0.
        ["PsychicRituals"] = ["PsychicRituals_Exalted", "PsychicRituals_Disapproved", "PsychicRituals_Abhorrent"],
        ["VoidStudy"] = ["VoidStudy_VeryEfficient", "VoidStudy_Efficient", "VoidStudy_Inefficient", "VoidStudy_VeryInefficient"],
        // Mort's Ideologies: both issues have two rungs each at displayOrderInIssue=0.
        ["MI_Pollution"] = ["MI_Pollution_Preferred", "MI_Pollution_Despised"],
        ["MI_ToxicWasteDumping"] = ["MI_ToxicWasteDumping_Respected", "MI_ToxicWasteDumping_Abhorrent"],
        // Questing Meme: all rungs share displayOrderInIssue=10; order established by strictness.
        ["QuesterMeme_QuestComplete"] =
            ["QuesterMeme_QuestComplete_Respected", "QuesterMeme_QuestComplete_Honourable", "QuesterMeme_QuestComplete_Daring"],
        ["QuesterMeme_QuestFail"] =
            ["QuesterMeme_QuestFail_DontCare", "QuesterMeme_QuestFail_Disliked", "QuesterMeme_QuestFail_Disapproved", "QuesterMeme_QuestFail_Dishonorable"],
        // Political Compass: all rungs share displayOrderInIssue=10/20/30; ordered as authoritarian spectrum.
        ["MI_Leader"] =
            ["MI_LeaderAnarchy", "MI_Elections_Required", "MI_LeaderCorporate", "MI_LeaderMonarchy", "MI_LeaderDictatorship"],
        // Indoors: VVE adds VVE_SmallSpaces_Horrible (no displayOrderInIssue) alongside the base-game
        // Indoors_Acceptable (also no displayOrderInIssue). Both default to 0, so order is undefined — fix it.
        // AM_Indoors_Required is the pro-indoors extreme, past Acceptable.
        ["Indoors"] = ["AM_Indoors_Required", "Indoors_Acceptable", "VVE_SmallSpaces_Horrible"],
        // Former PositiveOnly issues whose rungs all tie at displayOrderInIssue=0 (or scramble, for Blindness
        // and BS_AlienAppearanceTolerance) once stacked. Monastic (Medium impact) outranks preferred (Low).
        ["AM_Armour"] = ["AM_Armour_Forbidden", "AM_Armour_Blunt"],
        ["AM_Barracks"] = ["AM_Barracks_Preferred", "AM_Barracks_PreferredTrue", "AM_Barracks_Acceptable"],
        ["Pain"] = ["AM_Pain_Required", "Pain_Idealized", "VME_Pain_DontCare"],
        // Anti-comfort to pro-comfort. Ideology: More Precepts adds Wanted/Important/Essential.
        ["Comfort"] =
        [
            "AM_Comfort_DiscomfortPreferred", "Comfort_Ignored", "Comfort_Wanted", "Comfort_Important",
            "Comfort_Essential",
        ],
        // Exuberant (lovin interval -0.6) is past Increased (-0.2).
        ["AM_LovinFrequency"] =
            ["AM_LovinFrequency_Decreased", "AM_LovinFrequency_Normal", "AM_LovinFrequency_Increased", "AM_LovinFrequency_Exuberant"],
        ["Blindness"] = ["Blindness_Sublime", "Blindness_Elevated", "Blindness_Respected", "Blinding_Horrible"],
        ["BS_AlienAppearanceTolerance"] =
        [
            "BS_AlienAppearanceTolerance_FullTolerance", "BS_AlienAppearanceTolerance_SomeTolerance",
            "BS_AlienAppearanceTolerance_Default",
        ],
        ["VME_AutomationEfficiency"] = ["VME_AutomationEfficiency_Increased", "VME_AutomationEfficiency_Decreased"],
        ["VME_CraftingQuality"] = ["VME_CraftingQuality_Increased", "VME_CraftingQuality_Decreased"],
        ["VME_PsychicSensitivity"] = ["VME_PsychicSensitivity_Heightened", "VME_PsychicSensitivity_Lowered"],
        ["AM_PsychicSensitivity"] = ["AM_PsychicSensitivity_Heightened", "AM_PsychicSensitivity_Affinity"],
        // HorribleInnocent and Horrible tie at displayOrderInIssue=40.
        ["WCE2_Torture"] =
        [
            "WCE2_Torture_AlwaysRespected", "WCE2_Torture_RespectedGuilty", "WCE2_Torture_DontCare",
            "WCE2_Torture_HorribleInnocent", "WCE2_Torture_Horrible", "WCE2_Torture_Abhorrent",
        ],
        // Acceptable sits at displayOrderInIssue=0, before the two pro rungs.
        ["HAR_AlienDating"] =
        [
            "HAR_AlienDating_Honorable", "HAR_AlienDating_Preferred", "HAR_AlienDating_Acceptable",
            "HAR_AlienDating_Horrible", "HAR_AlienDating_Prohibited",
        ],
        // Lovin_Free and Lovin_FreeApproved tie at displayOrderInIssue=0.
        ["Lovin"] =
        [
            "Lovin_FreeApproved", "Lovin_Free", "Lovin_SpouseOnly_Mild", "Lovin_SpouseOnly_Moderate",
            "Lovin_SpouseOnly_Strict", "Lovin_Horrible", "Lovin_Prohibited",
        ],
        // VME_Sweets_Craved and VME_Sweets_Impartial tie at displayOrderInIssue=0.
        ["VME_Sweets"] = ["VME_Sweets_Craved", "VME_Sweets_Impartial"],
        // Genetic Rim sets no displayOrderInIssue.
        ["GR_Failures"] = ["GR_Failures_Accepted", "GR_Failures_Disliked", "GR_Failures_Shameful"],
        ["GR_HumanoidHybrids"] = ["GR_HumanoidHybrids_Accepted", "GR_HumanoidHybrids_Disliked", "GR_HumanoidHybrids_Forbidden"],
    };

    // Rungs that share the rank of an anchor rung on the same issue, keyed tied rung -> anchor. They express
    // the same belief and differ only in gameplay effect. Rungs() keeps only the anchor, so ranks stay
    // contiguous; RankOf and RankOfName map a tied rung onto its anchor.
    public static readonly Dictionary<string, string> TiedRungs = new()
    {
        // The three armour specialties have identical text ("Defense is paramount") and differ only in stat.
        ["AM_Armour_Sharp"] = "AM_Armour_Blunt",
        ["AM_Armour_Heat"] = "AM_Armour_Blunt",
    };

    // Sim/test hook: register a category for an issue absent from the hardcoded tables (test ladders are Moral).
    private static readonly Dictionary<string, PreceptCategory> Overrides = [];
    internal static void RegisterCategory(string issueDefName, PreceptCategory category)
    {
        Overrides[issueDefName] = category;
        CategoryCache.Clear();
    }

    internal static void ClearOverrides()
    {
        Overrides.Clear();
        CategoryCache.Clear();
    }

    // CategoryOf is on the structural opinion hot path; the table lookups hash the defName each call.
    private static readonly Dictionary<IssueDef, PreceptCategory> CategoryCache = [];

    // Only Moral and Special issues feed structural opinion, so only they are worth a debate. A won debate on
    // any other issue moves a stance that nothing reads.
    internal static bool IsDebatable(IssueDef issue) =>
        CategoryOf(issue) is PreceptCategory.Moral or PreceptCategory.Special;

    public static PreceptCategory CategoryOf(IssueDef issue)
    {
        if (!CategoryCache.TryGetValue(issue, out var category))
        {
            category = ResolveCategory(issue.defName);
            CategoryCache[issue] = category;
        }
        return category;
    }

    private static PreceptCategory ResolveCategory(string defName)
    {
        if (Overrides.TryGetValue(defName, out var overridden))
        {
            return overridden;
        }

        if (MoralIssues.Contains(defName)) return PreceptCategory.Moral;
        if (UniversalPositiveValuedRung.ContainsKey(defName)) return PreceptCategory.UniversalPositive;
        if (SpecialIssues.Contains(defName)) return PreceptCategory.Special;
        if (NAIssues.Contains(defName)) return PreceptCategory.NA;
        return PreceptCategory.PositiveOnly;
    }

}
