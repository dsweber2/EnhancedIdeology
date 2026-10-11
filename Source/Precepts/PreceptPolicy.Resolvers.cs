namespace EnhancedIdeology;

// Special-opinion resolvers and cross-precept coupling tables for PreceptPolicy.
// See docs/preceptPolicy.md "Special" and "Interactions" sections.
internal static partial class PreceptPolicy
{
    // The rungs of VME_Mood that clash with everything (including each other) rather than sitting on the
    // linear high/normal/low axis.
    private static readonly HashSet<string> MoodPariahs = ["VME_Mood_Shared", "VME_Mood_DictatedByStars"];

    // Bespoke per-issue opinion for Special issues that don't follow the rung-distance model. pawnRank is
    // the pawn's preferred rung, targetRank the rung the evaluated ideo holds; rank below 0 is "no stance".
    // Returns false to skip the issue entirely rather than count it as neutral.
    internal static bool TrySpecialOpinion(
        IssueDef issue, float pawnRank, float targetRank, float strength, float oppositionScale, out float opinion)
    {
        switch (issue.defName)
        {
            case "VME_Leader":
                // How the leader is chosen is categorical: any difference is a full clash, exact match agrees.
                opinion = SameRung(pawnRank, targetRank) ? strength : -strength;
                return true;

            case "VME_Mood":
                opinion = MoodOpinion(issue, pawnRank, targetRank, strength, oppositionScale);
                return true;

            default:
                // Weapons and PreferredXenotypes compare whole precept payloads — route through
                // TryPayloadSpecialOpinion instead (the caller tries that first).
                opinion = 0f;
                return false;
        }
    }

    // Special issues whose opinion depends on the precept payloads (which weapon classes, which xenotypes)
    // rather than a rung on a ladder. Returns false when the two faiths have nothing to agree or disagree
    // about on the issue, so it is skipped entirely rather than counted as neutral.
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

    // Weapons come in noble/despised pairs. Revering (or despising) the same class is agreement; revering
    // what the other despises is conflict. Faiths whose weapon tastes don't intersect are skipped.
    private static bool TryWeaponsOpinion(Ideo pawnIdeo, Ideo targetIdeo, float strength, out float opinion)
    {
        opinion = 0f;
        var mine = pawnIdeo.precepts.OfType<Precept_Weapon>().ToList();
        var theirs = targetIdeo.precepts.OfType<Precept_Weapon>().ToList();
        if (mine.Count == 0 || theirs.Count == 0)
            return false;

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
            return false;

        // A single fully-aligned or fully-opposed pair saturates to +/-strength.
        opinion = strength * Mathf.Clamp(raw / 2f, -1f, 1f);
        return true;
    }

    // Preferring a xenotype means disliking every other. Overlapping preferences agree, disjoint ones clash;
    // scored on Sørensen similarity so identical sets read +strength and disjoint sets -strength.
    private static bool TryXenotypeOpinion(Ideo pawnIdeo, Ideo targetIdeo, float strength, out float opinion)
    {
        opinion = 0f;
        var mine = PreferredXenotypeKeys(pawnIdeo);
        var theirs = PreferredXenotypeKeys(targetIdeo);
        if (mine.Count == 0 || theirs.Count == 0)
            return false;

        var shared = mine.Count(theirs.Contains);
        var similarity = 2f * shared / (mine.Count + theirs.Count);
        opinion = strength * ((2f * similarity) - 1f);
        return true;
    }

    internal static List<string> PreferredXenotypeKeys(Ideo ideo) =>
        ideo.precepts.OfType<Precept_Xenotype>()
            .Select(precept => precept.xenotype?.defName ?? precept.customXenotype?.name)
            .Where(key => key != null)
            .Distinct()
            .ToList()!;

    // VME_Mood: linear high/normal/low ladder with two pariah rungs bolted on. Either pariah disagrees with
    // everything but its own kind; linear rungs grade by distance among themselves.
    private static float MoodOpinion(IssueDef issue, float pawnRank, float targetRank, float strength, float oppositionScale)
    {
        if (IsMoodPariah(issue, pawnRank) || IsMoodPariah(issue, targetRank))
            return SameRung(pawnRank, targetRank) ? strength : -strength;

        return PreceptLadder.OpinionOnPrecept(pawnRank, targetRank, 0f, MoodLinearMaxRank(issue), strength, oppositionScale);
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
                max = ii;
        }
        return max;
    }

    private static bool SameRung(float a, float b) => Mathf.RoundToInt(a) == Mathf.RoundToInt(b);

    [StaticConstructorOnStartup]
    private static class Startup
    {
        static Startup()
        {
            foreach (var issue in DefDatabase<IssueDef>.AllDefs)
            {
                var rungCount = DefDatabase<PreceptDef>.AllDefs.Count(p => p.issue == issue);
                if (rungCount >= 2 && CategoryOf(issue) == PreceptCategory.PositiveOnly
                    && !KnownPositiveOnlyIssues.Contains(issue.defName))
                    Log.Error($"[EnhancedBeliefs] Unclassified multi-rung issue '{issue.defName}' ({rungCount} rungs) — please let the author know the issue name and ideally the mod of origin.");
            }
        }
    }

    // Cross-precept couplings: holding the keyed source precept makes an ideo behave, on another issue,
    // as if it took the induced stance — unless it already takes an explicit one there.
    private static readonly Dictionary<string, InducedStance> InducedByPrecept = new()
    {
        ["Trees_Desired"] = InducedStance.Rung("TreeCutting", "TreeCutting_Disapproved"),
        ["AM_Trees_Despised"] = InducedStance.BeyondDontCare("TreeCutting", -1f),
        ["Pain_Idealized"] = InducedStance.Rung("RoughLiving", "RoughLiving_Welcomed"),
        ["AM_Pain_Required"] = InducedStance.Rung("RoughLiving", "RoughLiving_Welcomed"),
        ["AM_HuntFocus_Sanguophage"] = InducedStance.Rung("Bloodfeeders", "Bloodfeeders_Reviled"),
        ["AM_SanguophageCamps_RaidingDesired"] = InducedStance.Rung("Bloodfeeders", "Bloodfeeders_Reviled"),
        ["VME_LeatherApparel_Disliked"] = InducedStance.Rung("AnimalSlaughter", "AnimalSlaughter_Disapproved"),
        ["VME_LeatherApparel_Abhorrent"] = InducedStance.Rung("AnimalSlaughter", "AnimalSlaughter_Horrible"),
    };

    // Directional penalties for couplings where the target issue is single-rung (no anti-stance to grade
    // toward): holding the source precept simply sours opinion of any ideo that holds the target precept.
    private static readonly (string source, string target)[] CouplingPenalties =
    [
        ("VME_Mechanoids_Despised", "MechanoidLabor_Enhanced"),
        // Valuing ideological diversity sours a faith on xenotype supremacism.
        // Only appreciative rungs of the diversity ladder levy it.
        ("IdeoDiversity_Approved", "PreferredXenotype"),
        ("IdeoDiversity_Respected", "PreferredXenotype"),
        ("IdeoDiversity_Exalted", "PreferredXenotype"),
    ];

    // Target issues an ideo takes an induced stance on, from the coupling source precepts it holds.
    // Issues whose mod is not loaded (absent from the database) are skipped.
    internal static IEnumerable<IssueDef> InducedIssues(Ideo ideo)
    {
        foreach (var precept in ideo.precepts)
        {
            if (InducedBy(precept.def) is { } induced)
            {
                var issue = DefDatabase<IssueDef>.GetNamedSilentFail(induced.TargetIssue);
                if (issue != null)
                    yield return issue;
            }
        }
    }

    // The rank an ideo's coupling induces on targetIssue, or null if it holds no source precept coupled to it.
    internal static float? InducedRank(Ideo ideo, IssueDef targetIssue)
    {
        foreach (var precept in ideo.precepts)
        {
            if (InducedBy(precept.def) is { } induced && induced.TargetIssue == targetIssue.defName)
                return induced.Resolve(targetIssue);
        }
        return null;
    }

    // InducedByPrecept keyed by def. The precept scans above run for each issue a target ideo does not hold, and a
    // defName key hashes the string on every lookup; a def caches its hash. The value depends only on the defName.
    private static readonly Dictionary<PreceptDef, InducedStance?> InducedByDef = [];

    private static InducedStance? InducedBy(PreceptDef def)
    {
        if (!InducedByDef.TryGetValue(def, out var induced))
        {
            induced = InducedByPrecept.TryGetValue(def.defName, out var stance) ? stance : null;
            InducedByDef[def] = induced;
        }
        return induced;
    }

    // Every issue some coupling can induce a stance on. Issues whose mod is not loaded are skipped.
    internal static IEnumerable<IssueDef> InducibleIssues() =>
        InducedByPrecept.Values
            .Select(induced => DefDatabase<IssueDef>.GetNamedSilentFail(induced.TargetIssue))
            .OfType<IssueDef>()
            .Distinct();

    // Total directional-penalty magnitude levied against the target ideo by a believer who holds the source
    // precepts `holdsSource` accepts, given how strongly they hold each source issue (their conviction on it).
    internal static float CouplingPenalty(Func<PreceptDef, bool> holdsSource, Ideo target, Func<IssueDef, float> convictionOf)
    {
        float penalty = 0f;
        foreach (var (sourcePrecept, targetPrecept) in CouplingPenalties)
        {
            var source = DefDatabase<PreceptDef>.GetNamedSilentFail(sourcePrecept);
            if (source != null && holdsSource(source) && target.precepts.Any(precept => precept.def.defName == targetPrecept))
                penalty += convictionOf(source.issue!);
        }
        return penalty;
    }
}
