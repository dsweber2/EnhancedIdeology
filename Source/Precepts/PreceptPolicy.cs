namespace EnhancedIdeology;

// Where the virtual "no opinion" rung sits for an optional Moral issue, expressed relative to named neighbour
// rungs rather than a literal rank so it survives ladder reordering and load order. Resolved against the live
// (reordered) ladder; a referenced rung absent from the ladder (mod not loaded) resolves to -1f and the spec
// degrades gracefully (see Resolve). See preceptPolicy.md's "Don't-care placement rule".
internal readonly struct DontCareSpec
{
    private enum Kind { Between, Before, After, At }

    private readonly Kind _kind;
    private readonly string _a;
    private readonly string? _b;

    private DontCareSpec(Kind kind, string a, string? b)
    {
        _kind = kind;
        _a = a;
        _b = b;
    }

    public static DontCareSpec Between(string a, string b) => new(Kind.Between, a, b);
    public static DontCareSpec Before(string a) => new(Kind.Before, a, null);
    public static DontCareSpec After(string a) => new(Kind.After, a, null);
    public static DontCareSpec At(string a) => new(Kind.At, a, null);

    public float Resolve(IssueDef issue)
    {
        var a = PreceptLadder.RankOfName(issue, _a);
        switch (_kind)
        {
            case Kind.Between:
                var b = PreceptLadder.RankOfName(issue, _b!);
                if (a < 0f && b < 0f) return -1f;
                if (a < 0f) return b;
                if (b < 0f) return a;
                return (a + b) / 2f;
            case Kind.Before:
                return a < 0f ? -1f : a - 0.5f;
            case Kind.After:
                return a < 0f ? -1f : a + 0.5f;
            default:
                return a;
        }
    }
}

// A stance one issue's precept induces on another (preceptPolicy.md "Interactions"): holding the source
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

// How an issue contributes to opinion (preceptPolicy.md). Only Moral issues feed the structural
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
// stacking scrambles displayOrderInIssue. Keyed by defName so it survives load order. See preceptPolicy.md.
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
        // Multi-rung mod issues with a genuine value axis (preceptPolicy.md "Mod issues"). Single-rung and
        // mechanical (*Speed/*Yield/perk) mod issues fall through to the PositiveOnly default.
        "VME_Alcohol", "VME_Violence", "VME_Recreation", "VME_KillingWithFire", "VME_LeatherApparel",
        "VME_Scars", "VME_Elders", "VME_Royalty", "VME_Mechanoids", "VME_Insectoids",
        "VME_Fire", "VME_Firefighting", "VME_TaintedApparel", "VME_TatteredApparel", "AM_Religion",
        "AM_AnimalRelease",
        // Lifestyle/aesthetic mod issues David gave an explicit Don't-care placement (preceptPolicy.md).
        "VME_Expectations", "AM_Rain", "VME_Aurora", "VME_BookReading", "VME_BookReadingSpeed",
        "VME_BookWriting", "VME_Travel", "VME_PermanentBases",
        // Mod Moral issues with a defaultSelectionWeight rung: every ideo resolves to a real rung (silent ->
        // that default, usually the centred neutral), so the -1 Don't-care default is never consulted and no
        // entry is needed. Verified each ladder is monotonic on its belief axis (a couple - VME_Recreation,
        // VME_Illness - are reversed, but OpinionOnPrecept is symmetric under axis reflection), so no order
        // override is needed either. See preceptPolicy.md "Order-fix candidates (verified)".
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
    ];
    private static readonly HashSet<string> UniversalPositiveIssues = ["Charity", "VME_Recreation"];
    // Special issues route through the special resolvers instead of the rung-distance model. VME_Leader /
    // VME_Mood are rank-based (categorical / hybrid, via TrySpecialOpinion); Weapons / PreferredXenotypes
    // compare whole precept payloads (via TryPayloadSpecialOpinion).
    private static readonly HashSet<string> SpecialIssues =
        ["PreferredXenotypes", "Weapons", "VME_Leader", "VME_Mood"];
    private static readonly HashSet<string> NAIssues =
        ["IdeoBuilding", "IdeoRelic", "IdeoRitualSeat", "Ritual", "MarriageName", "AM_Abilities"];
    // Multi-rung issues confirmed as PositiveOnly: no genuine belief axis, so rung-distance structural opinion
    // would be wrong. Listed here to suppress the startup warning for unclassified multi-rung issues.
    private static readonly HashSet<string> KnownPositiveOnlyIssues =
    [
        // Vanilla / DLC.
        "ApparelDesire", "Lighting", "Pain", "Research", "Proselytizing", "Blindness", "DarknessCombat",
        "IdeoRole",
        // EnhancedBeliefs (our own).
        "EB_Contemplation",
        // Alpha Memes.
        "AM_Armour", "AM_Barracks", "AM_CombatProwess",
        // Comfort: base Ignored + AM_Comfort_DiscomfortPreferred are both anti-comfort rungs (no pro-comfort
        // opposition). MiningYield: base High + AB_MiningYield_VeryHigh are yield bonuses at different levels.
        "Comfort", "MiningYield",
        // Better Ideology? (BS_).
        "BS_AlienAppearanceTolerance",
        // Vanilla Memes Expanded.
        "VME_AutomationEfficiency", "VME_CraftingQuality", "VME_CraftingSpeed", "VME_Death",
        "VME_Junk", "VME_PermitCooldown", "VME_PermitHonorCost", "VME_Power",
        "VME_PsychicSensitivity", "VME_PsyfocusGain", "VME_SkilledLabor",
        // Mort's Ideologies: Conservationist (MortStrudel.MortIdeologyEnv). Two restriction-only rungs,
        // no opposing pro-pollution rung.
        "MI_PowerGeneration",
        // Vanilla Vehicles Expanded: pure performance modifiers (speed/repair-rate/fuel), no belief axis.
        // RomanceOnTheRim_Issue_MarriageProposal mixes timing (LateMarriage/FlashMarriage) with control
        // (Forbidden/Arranged) — no clean moral axis.
        "VVE_Acceleration", "VVE_VehicleRepairs", "VVE_FuelEfficiency",
        "RomanceOnTheRim_Issue_MarriageProposal",
        // Not locally installed — safe PositiveOnly default pending ladder verification.
        "VRE_AndroidsIssue", "SEX_Divorce",
    ];

    // Rung defName order (permissive/pro -> forbidding/anti) for issues whose displayOrderInIssue scrambles
    // the axis once stacked (preceptPolicy.md "Reorder"). Rungs not listed keep their displayOrder, appended.
    public static readonly Dictionary<string, string[]> OrderOverrides = new()
    {
        ["MeatEating"] =
        [
            "MeatEating_NonMeat_Abhorrent", "MeatEating_NonMeat_Horrible", "MeatEating_NonMeat_Disapproved",
            "MeatEating_Disapproved", "MeatEating_Horrible", "MeatEating_Abhorrent", "VME_MeatEating_Abhorrent_Strict",
        ],
        ["AnimalSlaughter"] =
            ["AM_AnimalSlaughter_Desired", "AnimalSlaughter_Disapproved", "AnimalSlaughter_Horrible", "AnimalSlaughter_Prohibited"],
        ["SpouseCount_Male"] =
            ["SpouseCount_Male_MaxOne", "SpouseCount_Male_MaxTwo", "SpouseCount_Male_MaxThree", "SpouseCount_Male_MaxFour", "SpouseCount_Male_Unlimited"],
        ["SpouseCount_Female"] =
            ["SpouseCount_Female_MaxOne", "SpouseCount_Female_MaxTwo", "SpouseCount_Female_MaxThree", "SpouseCount_Female_MaxFour", "SpouseCount_Female_Unlimited"],
        // MI_BodyMod_Allowed (Political Compass) sits at displayOrderInIssue=30, after Abhorrent — insert it
        // between Approved and OnlyBiological.
        ["BodyModification"] =
            ["BodyMod_Approved", "MI_BodyMod_Allowed", "VME_BodyMod_OnlyBiological", "BodyMod_Disapproved", "BodyMod_Abhorrent"],
        // MI_DrugUse_Allowed (Political Compass) sits at displayOrderInIssue=40, beyond Prohibited — fix order.
        ["DrugUse"] =
            ["DrugUse_Essential", "MI_DrugUse_Allowed", "DrugUse_MedicalOrSocial", "DrugUse_MedicalOnly", "DrugUse_Prohibited", "DrugUse_Abhorrent"],
        ["OrganUse"] =
        [
            "OrganUse_Respected", "OrganUse_Acceptable", "VME_OrganUse_PostMortem", "OrganUse_HorribleSellOK",
            "OrganUse_HorribleNoSell", "OrganUse_Abhorrent", "AM_OrganUse_Torturous",
        ],
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
        ["Indoors"] = ["Indoors_Acceptable", "VVE_SmallSpaces_Horrible"],
    };

    // Where the virtual Don't-care rung sits for each OPTIONAL Moral issue (preceptPolicy.md). Mandatory
    // issues never go silent, so they carry no entry; an unlisted issue defaults to -1f (permissive extreme).
    // Keyed by IssueDef.defName; each spec is neighbour-keyed and resolved against the reordered ladder.
    public static readonly Dictionary<string, DontCareSpec> DontCare = new()
    {
        // Vanilla / DLC optional Moral issues.
        ["DrugUse"] = DontCareSpec.Between("DrugUse_MedicalOrSocial", "DrugUse_MedicalOnly"),
        ["ChildLabor"] = DontCareSpec.Between("ChildLabor_Encouraged", "ChildLabor_Disapproved"),
        ["Apostasy"] = DontCareSpec.Between("VME_Apostasy_Accepted", "Apostasy_Disapproved"),
        ["Raiding"] = DontCareSpec.Between("VME_Raiding_Honorable", "VME_Raiding_Abhorrent"),
        ["BodyModification"] = DontCareSpec.Between("BodyMod_Approved", "BodyMod_Disapproved"),
        ["MeatEating"] = DontCareSpec.Between("MeatEating_NonMeat_Disapproved", "MeatEating_Disapproved"),
        ["AnimalSlaughter"] = DontCareSpec.After("AM_AnimalSlaughter_Desired"),
        ["Fishing"] = DontCareSpec.Between("Fishing_Disapproved", "Fishing_Sacred"),
        ["GrowthVat"] = DontCareSpec.Between("GrowthVat_Essential", "GrowthVat_Prohibited"),
        ["Bloodfeeders"] = DontCareSpec.Between("Bloodfeeders_Revered", "Bloodfeeders_Reviled"),
        ["Biosculpting"] = DontCareSpec.Between("Biosculpting_Accelerated", "BioSculpter_Despised"),
        ["AutonomousWeapons"] = DontCareSpec.Between("VME_AutonomousWeapons_Accepted", "AutonomousWeapons_Disapproved"),
        ["Bonding"] = DontCareSpec.Before("Bonding_Disapproved"),
        // Mod optional Moral issues (David's placements).
        ["VME_Alcohol"] = DontCareSpec.Between("VME_Alcohol_Desired", "VME_Alcohol_MildAbstinence"),
        ["VME_KillingWithFire"] = DontCareSpec.Between("VME_KillingWithFire_Abhorrent", "VME_KillingWithFire_Preferred"),
        ["VME_LeatherApparel"] = DontCareSpec.Before("VME_LeatherApparel_Disliked"),
        ["VME_Scars"] = DontCareSpec.Between("VME_Scars_Disgusting", "VME_Scars_Honorable"),
        ["VME_Elders"] = DontCareSpec.Between("VME_Elders_Despised", "VME_Elders_Respected"),
        ["VME_Royalty"] = DontCareSpec.Between("VME_Royalty_Disliked", "VME_Royalty_Respected"),
        ["VME_Mechanoids"] = DontCareSpec.Between("VME_Mechanoids_Despised", "VME_Mechanoids_Exalted"),
        ["VME_Insectoids"] = DontCareSpec.Between("VME_Insectoids_Despised", "VME_Insectoids_Exalted"),
        ["VME_Fire"] = DontCareSpec.Between("VME_Fire_Despised", "VME_Fire_Desired"),
        ["VME_Firefighting"] = DontCareSpec.Between("VME_Firefighting_Preferred", "VME_Firefighting_Abhorrent"),
        ["AM_Religion"] = DontCareSpec.Between("AM_Religion_ProselytismDisliked", "AM_Religion_Disliked"),
        ["AM_AnimalRelease"] = DontCareSpec.Between("AM_AnimalRelease_Discouraged", "AM_AnimalRelease_Encouraged"),
        ["VME_Expectations"] = DontCareSpec.Between("VME_Expectations_High", "VME_Expectations_Low"),
        ["AM_Rain"] = DontCareSpec.Between("AM_Rain_Disliked", "AM_Rain_Blessed"),
        ["VME_Aurora"] = DontCareSpec.Between("VME_Aurora_Amazing", "VME_Aurora_Despised"),
        ["VME_BookReading"] = DontCareSpec.Between("VME_BookReading_Desired", "VME_BookReading_Disliked"),
        ["VME_BookReadingSpeed"] = DontCareSpec.Between("VME_BookReadingSpeed_Increased", "VME_BookReadingSpeed_Decreased"),
        ["VME_BookWriting"] = DontCareSpec.Between("VME_BookWriting_Disliked", "VME_BookWriting_Exalted"),
        ["VME_Travel"] = DontCareSpec.Between("VME_Travel_Desired", "VME_Travel_Despised"),
        ["VME_PermanentBases"] = DontCareSpec.Between("VME_PermanentBases_Desired", "VME_PermanentBases_Despised"),
        // Anomaly DLC optional Moral issues. Defnames are assumed; DontCareSpec degrades to -1f (issue skipped)
        // if Anomaly is not loaded. Placements assumed from the user-supplied ladder descriptions.
        ["PsychicRituals"] = DontCareSpec.Between("PsychicRituals_Disapproved", "PsychicRituals_Exalted"),
        ["VoidStudy"] = DontCareSpec.Between("VoidStudy_Inefficient", "VoidStudy_Efficient"),
        ["Inhumanizing"] = DontCareSpec.Before("Inhumanizing_Required"),
        // Mort's Ideologies (MortStrudel.MortIdeologyEnv): two opposing rungs per issue.
        ["MI_Pollution"] = DontCareSpec.Between("MI_Pollution_Preferred", "MI_Pollution_Despised"),
        ["MI_ToxicWasteDumping"] = DontCareSpec.Between("MI_ToxicWasteDumping_Respected", "MI_ToxicWasteDumping_Abhorrent"),
        // Mort's Ideologies: Political Compass (MortStrudel.MortIdeology).
        // The Ignored rung (order=20) is the explicit neutral midpoint on the equality/stratification axis.
        ["miHousingDistribution"] = DontCareSpec.At("miHousingDistribution_Ignored"),
        // DontCare sits between Democracy and Corporate — the moderate centre of the authoritarian spectrum.
        ["MI_Leader"] = DontCareSpec.Between("MI_Elections_Required", "MI_LeaderCorporate"),
        // Questing Meme (SirMashedPotato.QuestingMeme): no-opinion sits before the mildest positive stance.
        ["QuesterMeme_QuestComplete"] = DontCareSpec.Before("QuesterMeme_QuestComplete_Respected"),
        ["QuesterMeme_QuestFail"] = DontCareSpec.Before("QuesterMeme_QuestFail_DontCare"),
        // Vanilla Vehicles Expanded: Indoors is STACKED between pro-indoor and anti-indoor rungs.
        // VVE_Driving/Flying/Sailing: the Allowed rung (defaultSelectionWeight=1) is the explicit neutral.
        ["Indoors"] = DontCareSpec.Between("Indoors_Acceptable", "VVE_SmallSpaces_Horrible"),
        ["VVE_Driving"] = DontCareSpec.At("VVE_Driving_Allowed"),
        ["VVE_Flying"] = DontCareSpec.At("VVE_Flying_Allowed"),
        ["VVE_Sailing"] = DontCareSpec.At("VVE_Sailing_Allowed"),
        // Language Learning: Neutral rung (defaultSelectionWeight=1) is the explicit centre.
        ["LanguageLearning"] = DontCareSpec.At("LanguageLearning_Neutral"),
        // Romance on the Rim: no opinion on cheating/breakups sits before the most permissive rung.
        ["RomanceOnTheRim_Issue_Cheat"] = DontCareSpec.Before("RomanceOnTheRim_Cheat_Encouraged"),
        ["RomanceOnTheRim_Issue_Breakup"] = DontCareSpec.Before("RomanceOnTheRim_Breakup_Encouraged"),
        ["RomanceOnTheRim_Issue_RomanceAttempt"] = DontCareSpec.Before("RomanceOnTheRim_RomanceAttempt_Encouraged"),
        // VME_BookWritingSpeed parallels VME_BookReadingSpeed.
        ["VME_BookWritingSpeed"] = DontCareSpec.Between("VME_BookWritingSpeed_Increased", "VME_BookWritingSpeed_Decreased"),
    };

    // Sim/test hook: register a category for an issue absent from the hardcoded tables (test ladders are Moral).
    private static readonly Dictionary<string, PreceptCategory> Overrides = [];
    internal static void RegisterCategory(string issueDefName, PreceptCategory category) => Overrides[issueDefName] = category;
    internal static void ClearOverrides() => Overrides.Clear();

    public static PreceptCategory CategoryOf(IssueDef issue)
    {
        if (Overrides.TryGetValue(issue.defName, out var overridden))
        {
            return overridden;
        }

        if (MoralIssues.Contains(issue.defName)) return PreceptCategory.Moral;
        if (UniversalPositiveIssues.Contains(issue.defName)) return PreceptCategory.UniversalPositive;
        if (SpecialIssues.Contains(issue.defName)) return PreceptCategory.Special;
        if (NAIssues.Contains(issue.defName)) return PreceptCategory.NA;
        return PreceptCategory.PositiveOnly;
    }

}
