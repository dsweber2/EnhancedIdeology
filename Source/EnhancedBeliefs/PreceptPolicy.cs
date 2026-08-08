namespace EnhancedBeliefs;

// Where the virtual "no opinion" rung sits for an optional Moral issue, expressed relative to named neighbour
// rungs rather than a literal rank so it survives ladder reordering and load order. Resolved against the live
// (reordered) ladder; a referenced rung absent from the ladder (mod not loaded) resolves to -1f and the spec
// degrades gracefully (see Resolve). See preceptPolicy.md's "Don't-care placement rule".
internal readonly struct DontCareSpec
{
    private enum Kind { Between, Before, After, At }

    private readonly Kind _kind;
    private readonly string _a;
    private readonly string _b;

    private DontCareSpec(Kind kind, string a, string b)
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
                var b = PreceptLadder.RankOfName(issue, _b);
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
    private readonly string _rung;
    private readonly float _offset;

    private InducedStance(string targetIssue, Kind kind, string rung, float offset)
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
        Kind.Rung => PreceptLadder.RankOfName(issue, _rung),
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
internal static class PreceptPolicy
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
    ];
    private static readonly HashSet<string> UniversalPositiveIssues = ["Charity"];
    // Special issues route through the special resolvers instead of the rung-distance model. VME_Leader /
    // VME_Mood are rank-based (categorical / hybrid, via TrySpecialOpinion); Weapons / PreferredXenotypes
    // compare whole precept payloads (via TryPayloadSpecialOpinion).
    private static readonly HashSet<string> SpecialIssues =
        ["PreferredXenotypes", "Weapons", "VME_Leader", "VME_Mood"];
    private static readonly HashSet<string> NAIssues =
        ["IdeoBuilding", "IdeoRelic", "IdeoRitualSeat", "Ritual", "MarriageName", "AM_Abilities"];

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
        ["BodyModification"] =
            ["BodyMod_Approved", "VME_BodyMod_OnlyBiological", "BodyMod_Disapproved", "BodyMod_Abhorrent"],
        ["OrganUse"] =
        [
            "OrganUse_Respected", "OrganUse_Acceptable", "VME_OrganUse_PostMortem", "OrganUse_HorribleSellOK",
            "OrganUse_HorribleNoSell", "OrganUse_Abhorrent", "AM_OrganUse_Torturous",
        ],
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

    // The rungs of VME_Mood that clash with everything (including each other) rather than sitting on the
    // linear high/normal/low axis.
    private static readonly HashSet<string> MoodPariahs = ["VME_Mood_Shared", "VME_Mood_DictatedByStars"];

    // Bespoke per-issue opinion for Special issues that don't follow the rung-distance model (preceptPolicy.md
    // "Special"). pawnRank is the pawn's preferred rung, targetRank the rung the evaluated ideo holds; a rank
    // below 0 is the "no stance" rung. Returns false to skip the issue (not yet modelled), leaving it out of
    // the structural mean entirely.
    internal static bool TrySpecialOpinion(
        IssueDef issue, float pawnRank, float targetRank, float strength, float zeroFrac, out float opinion)
    {
        switch (issue.defName)
        {
            case "VME_Leader":
                // How the leader is chosen is categorical: any difference (including one side having no leader
                // precept at all) is a full clash, only an exact match agrees.
                opinion = SameRung(pawnRank, targetRank) ? strength : -strength;
                return true;

            case "VME_Mood":
                opinion = MoodOpinion(issue, pawnRank, targetRank, strength, zeroFrac);
                return true;

            default:
                // Weapons and PreferredXenotypes compare whole precept payloads, not ranks, so they route
                // through TryPayloadSpecialOpinion instead (the caller tries that first).
                opinion = 0f;
                return false;
        }
    }

    // Special issues whose opinion depends on the precept payloads themselves (which weapon classes, which
    // xenotypes) rather than a rung on a ladder, so they need both ideos' precepts, not just ranks. Returns
    // false when the two faiths have nothing to agree or disagree about on the issue, so it is skipped entirely
    // rather than counted as neutral. strength is the pawn's conviction on the issue.
    internal static bool TryPayloadSpecialOpinion(
        IssueDef issue, Ideo pawnIdeo, Ideo targetIdeo, float strength, out float opinion)
    {
        switch (issue.defName)
        {
            case "Weapons":
                return TryWeaponsOpinion(pawnIdeo, targetIdeo, strength, out opinion);
            case "PreferredXenotypes":
                return TryXenotypeOpinion(pawnIdeo, targetIdeo, strength, out opinion);
            default:
                opinion = 0f;
                return false;
        }
    }

    // Weapons come in noble/despised pairs. Revering (or despising) the same class is agreement; revering what
    // the other despises is conflict. Faiths whose weapon tastes don't intersect at all genuinely don't care.
    private static bool TryWeaponsOpinion(Ideo pawnIdeo, Ideo targetIdeo, float strength, out float opinion)
    {
        opinion = 0f;
        var mine = pawnIdeo.precepts.OfType<Precept_Weapon>().ToList();
        var theirs = targetIdeo.precepts.OfType<Precept_Weapon>().ToList();
        if (mine.Count == 0 || theirs.Count == 0)
        {
            return false;
        }

        var raw = 0;
        foreach (var a in mine)
        {
            foreach (var b in theirs)
            {
                if (a.noble != null && (a.noble == b.noble)) raw++;
                if (a.despised != null && (a.despised == b.despised)) raw++;
                if (a.noble != null && (a.noble == b.despised)) raw--;
                if (a.despised != null && (a.despised == b.noble)) raw--;
            }
        }

        if (raw == 0)
        {
            return false;
        }

        // A single fully-aligned (both noble and despised match) or fully-opposed pair saturates to +/-strength.
        opinion = strength * Mathf.Clamp(raw / 2f, -1f, 1f);
        return true;
    }

    // Preferring a xenotype means disliking every other. Overlapping preferences agree, disjoint ones clash;
    // scored on a Sorensen similarity so identical sets read +strength and disjoint sets -strength. A faith with
    // no xenotype preference has no stance here (its friction with tolerant faiths is a coupling, not this axis).
    private static bool TryXenotypeOpinion(Ideo pawnIdeo, Ideo targetIdeo, float strength, out float opinion)
    {
        opinion = 0f;
        var mine = PreferredXenotypeKeys(pawnIdeo);
        var theirs = PreferredXenotypeKeys(targetIdeo);
        if (mine.Count == 0 || theirs.Count == 0)
        {
            return false;
        }

        var shared = mine.Count(theirs.Contains);
        var similarity = 2f * shared / (mine.Count + theirs.Count);
        opinion = strength * ((2f * similarity) - 1f);
        return true;
    }

    private static List<string> PreferredXenotypeKeys(Ideo ideo) =>
        ideo.precepts.OfType<Precept_Xenotype>()
            .Select(precept => precept.xenotype?.defName ?? precept.customXenotype?.name)
            .Where(key => key != null)
            .Distinct()
            .ToList()!;

    // VME_Mood is a linear high/normal/low ladder with two pariah rungs (shared, dictated-by-stars) bolted on.
    // Either pariah disagrees with everything but its own kind; the linear rungs grade by distance among
    // themselves. Pariahs are found by defName so a scrambled or extended ladder still classifies correctly.
    private static float MoodOpinion(IssueDef issue, float pawnRank, float targetRank, float strength, float zeroFrac)
    {
        if (IsMoodPariah(issue, pawnRank) || IsMoodPariah(issue, targetRank))
        {
            return SameRung(pawnRank, targetRank) ? strength : -strength;
        }

        return PreceptLadder.OpinionOnPrecept(pawnRank, targetRank, 0f, MoodLinearMaxRank(issue), strength, zeroFrac);
    }

    private static bool IsMoodPariah(IssueDef issue, float rank)
    {
        var rungs = PreceptLadder.Rungs(issue);
        var ix = Mathf.RoundToInt(rank);
        return ix >= 0 && ix < rungs.Count && MoodPariahs.Contains(rungs[ix].defName);
    }

    private static float MoodLinearMaxRank(IssueDef issue)
    {
        var rungs = PreceptLadder.Rungs(issue);
        var max = 0;
        for (var ii = 0; ii < rungs.Count; ii++)
        {
            if (!MoodPariahs.Contains(rungs[ii].defName))
            {
                max = ii;
            }
        }

        return max;
    }

    private static bool SameRung(float a, float b) => Mathf.RoundToInt(a) == Mathf.RoundToInt(b);

    // Cross-precept couplings (preceptPolicy.md "Interactions"): holding the keyed source precept makes an ideo
    // behave, on another issue, as if it took the induced stance - unless it already takes an explicit one.
    private static readonly Dictionary<string, InducedStance> InducedByPrecept = new()
    {
        ["Trees_Desired"] = InducedStance.Rung("TreeCutting", "TreeCutting_Disapproved"),
        ["AM_Trees_Despised"] = InducedStance.BeyondDontCare("TreeCutting", -1f),
        ["Pain_Idealized"] = InducedStance.Rung("RoughLiving", "RoughLiving_Welcomed"),
        ["VME_LeatherApparel_Disliked"] = InducedStance.Rung("AnimalSlaughter", "AnimalSlaughter_Disapproved"),
        ["VME_LeatherApparel_Abhorrent"] = InducedStance.Rung("AnimalSlaughter", "AnimalSlaughter_Horrible"),
    };

    // Directional penalties for couplings that can't be rung distance because the target issue is single-rung
    // (no anti stance to grade toward): holding the source precept simply sours opinion of any ideo that holds
    // the target precept. (source precept defName -> target precept defName.)
    private static readonly (string source, string target)[] CouplingPenalties =
    [
        ("VME_Mechanoids_Despised", "MechanoidLabor_Enhanced"),
        // Valuing ideological diversity sours a faith on xenotype supremacism (holding any PreferredXenotype).
        // Only the appreciative rungs of the diversity ladder levy it - the bigotry and neutral rungs do not.
        ("IdeoDiversity_Approved", "PreferredXenotype"),
        ("IdeoDiversity_Respected", "PreferredXenotype"),
        ("IdeoDiversity_Exalted", "PreferredXenotype"),
    ];

    // Target issues an ideo takes an induced stance on, from the coupling source precepts it holds. Issues
    // whose mod is not loaded (absent from the database) are skipped.
    internal static IEnumerable<IssueDef> InducedIssues(Ideo ideo)
    {
        foreach (var precept in ideo.precepts)
        {
            if (InducedByPrecept.TryGetValue(precept.def.defName, out var induced))
            {
                var issue = DefDatabase<IssueDef>.GetNamedSilentFail(induced.TargetIssue);
                if (issue != null)
                {
                    yield return issue;
                }
            }
        }
    }

    // The rank an ideo's coupling induces on targetIssue, or null if it holds no source precept coupled to it.
    internal static float? InducedRank(Ideo ideo, IssueDef targetIssue)
    {
        foreach (var precept in ideo.precepts)
        {
            if (InducedByPrecept.TryGetValue(precept.def.defName, out var induced)
                && induced.TargetIssue == targetIssue.defName)
            {
                return induced.Resolve(targetIssue);
            }
        }

        return null;
    }

    // Total directional-penalty magnitude the source ideo levies against the target ideo, given a way to read
    // how strongly the source ideo holds each source issue (its conviction on it).
    internal static float CouplingPenalty(Ideo source, Ideo target, Func<IssueDef, float> convictionOf)
    {
        float penalty = 0f;
        foreach (var (sourcePrecept, targetPrecept) in CouplingPenalties)
        {
            var held = source.precepts.FirstOrDefault(precept => precept.def.defName == sourcePrecept);
            if (held != null && target.precepts.Any(precept => precept.def.defName == targetPrecept))
            {
                penalty += convictionOf(held.def.issue!);
            }
        }

        return penalty;
    }
}
