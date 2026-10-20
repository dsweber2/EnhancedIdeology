namespace EnhancedIdeology;

// Where the neutral rank sits on an issue, expressed relative to named neighbour rungs rather than a literal
// rank so it survives ladder reordering and load order. Resolved against the live (reordered) ladder; a
// referenced rung absent from the ladder (mod not loaded) resolves to -1f, and Or() gives a fallback for that
// case. See docs/preceptPolicy.md's "Don't-care placement rule".
internal sealed class DontCareSpec
{
    private enum Kind { Between, Before, After, At }

    // Resolve result when the referenced rungs are absent. Before() on rank 0 gives -0.5, which is a real rank.
    private const float AbsentRank = -1f;

    private readonly Kind _kind;
    private readonly string _a;
    private readonly string? _b;
    private DontCareSpec? _fallback;

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

    // Use `fallback` when this spec's rung is absent (the rung comes from a mod that is not loaded).
    public DontCareSpec Or(DontCareSpec fallback)
    {
        _fallback = fallback;
        return this;
    }

    public float Resolve(IssueDef issue)
    {
        var rank = ResolveOwn(issue);
        return rank == AbsentRank && _fallback != null ? _fallback.Resolve(issue) : rank;
    }

    private float ResolveOwn(IssueDef issue)
    {
        var a = PreceptLadder.RankOfName(issue, _a);
        switch (_kind)
        {
            case Kind.Between:
                var b = PreceptLadder.RankOfName(issue, _b!);
                if (a < 0f && b < 0f) return AbsentRank;
                if (a < 0f) return b;
                if (b < 0f) return a;
                return (a + b) / 2f;
            case Kind.Before:
                return a < 0f ? AbsentRank : a - 0.5f;
            case Kind.After:
                return a < 0f ? AbsentRank : a + 0.5f;
            default:
                return a;
        }
    }
}

internal static partial class PreceptPolicy
{
    // Neutral rank per issue: the vanilla baseline, the stance of a pawn with no precept on the issue (classic
    // mode behaviour). A rung named "don't care" is a stance if it changes anything from that baseline.
    // Mandatory issues need an entry too: pawns read neutral for trait targets, held stances and spans.
    // An unlisted issue defaults to -1f, one step past rank 0. That is correct only for single-rung and
    // restriction-only ladders. Specs that name a mod rung carry an Or() fallback for when the mod is absent.
    public static readonly Dictionary<string, DontCareSpec> DontCare = new()
    {
        // Vanilla / DLC mandatory issues: the classic rung, or the rung that matches the classic effects.
        ["Slavery"] = DontCareSpec.At("Slavery_Acceptable"),
        // Execution_Classic has only the "know" thoughts of HorribleIfInnocent.
        ["Execution"] = DontCareSpec.Before("Execution_HorribleIfInnocent"),
        // Cannibalism_Classic: ate human meat -20, the same as Abhorrent.
        ["Cannibalism"] = DontCareSpec.At("Cannibalism_Abhorrent"),
        // OrganUse_Classic: thoughts only for harvests from colonists and guests.
        ["OrganUse"] = DontCareSpec.Before("OrganUse_HorribleSellOK"),
        ["Scarification"] = DontCareSpec.After("Scarification_Minor"),
        ["Skullspike"] = DontCareSpec.After("Skullspike_Desired"),
        ["IdeoDiversity"] = DontCareSpec.At("IdeoDiversity_Standard"),
        // Lovin_Free (classic) has no thoughts. It is not in the ladder.
        ["Lovin"] = DontCareSpec.After("Lovin_FreeApproved"),
        ["Nudity_Male"] = DontCareSpec.At("Nudity_Male_UncoveredGroinDisapproved"),
        ["Nudity_Female"] = DontCareSpec.At("Nudity_Female_UncoveredGroinOrChestDisapproved"),
        ["SpouseCount_Male"] = DontCareSpec.At("SpouseCount_Male_MaxOne"),
        ["SpouseCount_Female"] = DontCareSpec.At("SpouseCount_Female_MaxOne"),
        ["MarriageName"] = DontCareSpec.At("MarriageName_UsuallyMans"),
        ["Corpses"] = DontCareSpec.At("Corpses_Ugly"),
        ["NutrientPasteEating"] = DontCareSpec.At("NutrientPasteEating_Disgusting"),
        ["InsectMeat"] = DontCareSpec.At("InsectMeatEating_Despised_Classic"),
        ["FungusEating"] = DontCareSpec.At("VME_FungusEating_DontCare")
            .Or(DontCareSpec.Between("FungusEating_Preferred", "FungusEating_Despised")),
        ["Research"] = DontCareSpec.At("Research_Normal"),

        // Vanilla / DLC optional issues.
        // No precept means no drug thoughts, as MI_DrugUse_Allowed. MedicalOrSocial already forbids hard drugs.
        ["DrugUse"] = DontCareSpec.At("MI_DrugUse_Allowed").Or(DontCareSpec.Before("DrugUse_MedicalOrSocial")),
        ["ChildLabor"] = DontCareSpec.Between("ChildLabor_Encouraged", "ChildLabor_Disapproved"),
        ["Apostasy"] = DontCareSpec.Before("Apostasy_Disapproved"),
        ["Raiding"] = DontCareSpec.After("VME_Raiding_Honorable").Or(DontCareSpec.After("Raiding_Respected")),
        ["BodyModification"] = DontCareSpec.At("MI_BodyMod_Allowed").Or(DontCareSpec.After("BodyMod_Approved")),
        ["MeatEating"] = DontCareSpec.Between("MeatEating_NonMeat_Disapproved", "MeatEating_Disapproved"),
        ["AnimalSlaughter"] = DontCareSpec.After("AM_AnimalSlaughter_Desired"),
        ["Fishing"] = DontCareSpec.Between("Fishing_Disapproved", "Fishing_Sacred"),
        ["GrowthVat"] = DontCareSpec.Between("GrowthVat_Essential", "GrowthVat_Prohibited"),
        ["Bloodfeeders"] = DontCareSpec.Between("Bloodfeeders_Revered", "Bloodfeeders_Reviled"),
        ["Biosculpting"] = DontCareSpec.Between("Biosculpting_Accelerated", "BioSculpter_Despised"),
        ["AutonomousWeapons"] = DontCareSpec.Before("AutonomousWeapons_Disapproved"),
        ["Bonding"] = DontCareSpec.Before("Bonding_Disapproved"),
        ["RoughLiving"] = DontCareSpec.Before("RoughLiving_Welcomed"),
        ["Eclipse"] = DontCareSpec.After("Eclipse_Beautiful"),
        ["GauranlenConnection"] = DontCareSpec.After("GauranlenConnection_Strong"),
        ["Trees"] = DontCareSpec.After("MI_Trees_Mildly_Desired").Or(DontCareSpec.After("Trees_Desired")),
        ["Indoors"] = DontCareSpec.After("Indoors_Acceptable"),
        // Central, CattleCentered and Nomadic are degrees of pro-ranching culture.
        ["Ranching"] = DontCareSpec.After("VME_Ranching_Nomadic")
            .Or(DontCareSpec.After("AM_Ranching_CattleCentered").Or(DontCareSpec.After("Ranching_Central"))),
        // UniversalPositive. Indifference sits past the weakest pro rung.
        ["Charity"] = DontCareSpec.After("Charity_Worthwhile"),
        // VME_Pain_DontCare is a machismo rung ("pain can be endured"), so neutral sits past it.
        ["Pain"] = DontCareSpec.After("VME_Pain_DontCare").Or(DontCareSpec.After("Pain_Idealized")),
        ["Blindness"] = DontCareSpec.Between("Blindness_Respected", "Blinding_Horrible"),
        ["DarknessCombat"] = DontCareSpec.After("DarknessCombat_Preferred"),
        ["Lighting"] = DontCareSpec.Before("Darklight_Preferred"),
        // No precept spreads the faith at x1: between Occasionally (x3) and Never (x0).
        ["Proselytizing"] = DontCareSpec.After("Proselytizing_Occasionally"),
        // Ignored drops the comfort need; neutral keeps it.
        ["Comfort"] = DontCareSpec.After("Comfort_Ignored"),
        // Anomaly DLC. Defnames are assumed.
        ["PsychicRituals"] = DontCareSpec.Between("PsychicRituals_Disapproved", "PsychicRituals_Exalted"),
        ["VoidStudy"] = DontCareSpec.Between("VoidStudy_Inefficient", "VoidStudy_Efficient"),
        ["Inhumanizing"] = DontCareSpec.Before("Inhumanizing_Required"),

        // Our own issue.
        ["EB_Contemplation"] = DontCareSpec.At("Contemplation_Normal"),

        // Mod issues with a defaultSelectionWeight rung: that rung is the baseline.
        ["AM_FertilityIssue"] = DontCareSpec.At("AM_FertilityIssue_Normal"),
        ["AM_LearningRate"] = DontCareSpec.At("AM_LearningRate_Normal"),
        ["AM_LovinFrequency"] = DontCareSpec.At("AM_LovinFrequency_Normal"),
        ["BS_AlienAppearanceTolerance"] = DontCareSpec.At("BS_AlienAppearanceTolerance_Default"),
        ["VME_Death"] = DontCareSpec.At("VME_Death_Normal"),
        ["VME_DumbLabor"] = DontCareSpec.At("VME_DumbLabor_Indifferent"),
        ["VME_Recreation"] = DontCareSpec.At("VME_Recreation_Normal"),
        ["VME_Violence"] = DontCareSpec.At("VME_Violence_Acceptable"),
        ["VME_Illness"] = DontCareSpec.At("VME_Illness_Indifferent"),
        // Vanilla already gives a moodlet for tainted and tattered apparel, as Disapproved.
        ["VME_TaintedApparel"] = DontCareSpec.At("VME_TaintedApparel_Disapproved"),
        ["VME_TatteredApparel"] = DontCareSpec.At("VME_TatteredApparel_Disapproved"),
        // VME_InsectJellyEaten_Neutral_Classic is classic-only and not in the ladder.
        ["VME_InsectJelly"] = DontCareSpec.Between("VME_InsectJellyEaten_Despised", "VME_InsectJellyEaten_Loved_Classic"),
        // VVE_*_Allowed is classic-only and not in the ladder. It sits between Approved and Horrible.
        ["VVE_Driving"] = DontCareSpec.Between("VVE_Driving_Approved", "VVE_Driving_Horrible"),
        ["VVE_Flying"] = DontCareSpec.Between("VVE_Flying_Approved", "VVE_Flying_Horrible"),
        ["VVE_Sailing"] = DontCareSpec.Between("VVE_Sailing_Approved", "VVE_Sailing_Horrible"),

        // Mod optional issues.
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
        ["VME_Expectations"] = DontCareSpec.Between("VME_Expectations_High", "VME_Expectations_Low"),
        ["VME_Aurora"] = DontCareSpec.Between("VME_Aurora_Amazing", "VME_Aurora_Despised"),
        ["VME_BookReading"] = DontCareSpec.Between("VME_BookReading_Desired", "VME_BookReading_Disliked"),
        ["VME_BookReadingSpeed"] = DontCareSpec.Between("VME_BookReadingSpeed_Increased", "VME_BookReadingSpeed_Decreased"),
        ["VME_BookWriting"] = DontCareSpec.Between("VME_BookWriting_Disliked", "VME_BookWriting_Exalted"),
        ["VME_BookWritingSpeed"] = DontCareSpec.Between("VME_BookWritingSpeed_Increased", "VME_BookWritingSpeed_Decreased"),
        ["VME_Travel"] = DontCareSpec.Between("VME_Travel_Desired", "VME_Travel_Despised"),
        ["VME_PermanentBases"] = DontCareSpec.Between("VME_PermanentBases_Desired", "VME_PermanentBases_Despised"),
        ["VME_AutomationEfficiency"] = DontCareSpec.Between("VME_AutomationEfficiency_Increased", "VME_AutomationEfficiency_Decreased"),
        ["VME_CraftingQuality"] = DontCareSpec.Between("VME_CraftingQuality_Increased", "VME_CraftingQuality_Decreased"),
        ["VME_PsychicSensitivity"] = DontCareSpec.Between("VME_PsychicSensitivity_Heightened", "VME_PsychicSensitivity_Lowered"),
        // ProselytismDisliked has its own thought, so it is a stance.
        ["AM_Religion"] = DontCareSpec.After("AM_Religion_Loved"),
        ["AM_AnimalRelease"] = DontCareSpec.Between("AM_AnimalRelease_Discouraged", "AM_AnimalRelease_Encouraged"),
        ["AM_Rain"] = DontCareSpec.Between("AM_Rain_Disliked", "AM_Rain_Blessed"),
        ["AM_Armour"] = DontCareSpec.Between("AM_Armour_Forbidden", "AM_Armour_Blunt"),
        ["AM_Barracks"] = DontCareSpec.After("AM_Barracks_Acceptable"),
        ["AM_PsychicSensitivity"] = DontCareSpec.Between("AM_PsychicSensitivity_Heightened", "AM_PsychicSensitivity_Affinity"),
        // Mort's Ideologies.
        ["MI_Pollution"] = DontCareSpec.Between("MI_Pollution_Preferred", "MI_Pollution_Despised"),
        ["MI_ToxicWasteDumping"] = DontCareSpec.Between("MI_ToxicWasteDumping_Respected", "MI_ToxicWasteDumping_Abhorrent"),
        ["miHousingDistribution"] = DontCareSpec.At("miHousingDistribution_Ignored"),
        // The moderate centre of the authoritarian spectrum.
        ["MI_Leader"] = DontCareSpec.Between("MI_Elections_Required", "MI_LeaderCorporate"),
        // Questing Meme.
        ["QuesterMeme_QuestComplete"] = DontCareSpec.Before("QuesterMeme_QuestComplete_Respected"),
        ["QuesterMeme_QuestFail"] = DontCareSpec.At("QuesterMeme_QuestFail_DontCare"),
        ["LanguageLearning"] = DontCareSpec.At("LanguageLearning_Neutral"),
        // Romance on the Rim: the precepts scale chances (Encouraged x1.5, Discouraged x0.5); no precept is x1.
        ["RomanceOnTheRim_Issue_Cheat"] = DontCareSpec.After("RomanceOnTheRim_Cheat_Encouraged"),
        ["RomanceOnTheRim_Issue_Breakup"] = DontCareSpec.After("RomanceOnTheRim_Breakup_Encouraged"),
        ["RomanceOnTheRim_Issue_RomanceAttempt"] = DontCareSpec.After("RomanceOnTheRim_RomanceAttempt_Encouraged"),
        ["NikolaisIdeology_Beauty"] = DontCareSpec.Between("NikolaisIdeology_Beauty_Central", "NikolaisIdeology_Beauty_Unimportant"),
    };
}
