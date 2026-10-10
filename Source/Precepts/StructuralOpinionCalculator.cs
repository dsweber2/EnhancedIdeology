namespace EnhancedIdeology;

// Certainty-independent structural opinion (0-100) a pawn holds toward an ideo. Shared by the
// certainty setpoint's structural band and the per-ideo opinion display.
// All methods are stateless; pawn identity and stance data are passed as explicit arguments.
internal static class StructuralOpinionCalculator
{
    private const float UniversalPositiveBonus = 5f;

    // Compute the structural opinion (0-100) that `pawn` holds toward `ideo`. If `contributors` is
    // supplied, each term is appended (in certainty-fraction units) for the tooltip breakdown.
    // Caller must ensure issue stances are seeded before calling.
    internal static float Compute(
        Pawn pawn,
        IssueStanceTracker stances,
        Ideo ideo,
        List<(string label, float pct)>? contributors)
    {
        var pawnIdeo = pawn.Ideo!;
        var start = contributors?.Count ?? 0;
        float opinion = 0;

        // Loyalty memes of the pawn's own faith set their attitude toward other faiths, so the own ideo is exempt.
        if (ideo != pawnIdeo && LoyaltyMemeOpinion(pawnIdeo) is { } loyalty)
        {
            opinion += loyalty.offset;
            contributors?.Add((loyalty.label, loyalty.offset));
        }

        // VME_Elders: elder pawns gain structural certainty in ideos that venerate the old (max +20 at 70+).
        var eldersMeme = EnhancedIdeologyDefOf.VME_Elders;
        if (eldersMeme != null && ideo.HasMeme(eldersMeme))
        {
            var ageFactor = Mathf.Clamp01(Mathf.InverseLerp(50f, 70f, pawn.ageTracker.AgeBiologicalYearsFloat));
            if (ageFactor > 0f)
            {
                var eldersBonus = 20f * ageFactor;
                opinion += eldersBonus;
                contributors?.Add((eldersMeme.LabelCap, eldersBonus));
            }
        }

        // Cross-meme disagreements between opposing Mort's Ideologies memes.
        var empiricist = EnhancedIdeologyDefOf.MI_Empiricist;
        var faith = EnhancedIdeologyDefOf.MI_Faith;
        if (empiricist != null && faith != null
            && (pawnIdeo.HasMeme(empiricist) && ideo.HasMeme(faith)
                || pawnIdeo.HasMeme(faith) && ideo.HasMeme(empiricist)))
        {
            opinion -= 5;
            contributors?.Add((empiricist.LabelCap + " / " + faith.LabelCap, -5f));
        }
        var conservationist = EnhancedIdeologyDefOf.MI_Environmentalist;
        var polluter = EnhancedIdeologyDefOf.MI_Industrialist;
        if (conservationist != null && polluter != null
            && (pawnIdeo.HasMeme(conservationist) && ideo.HasMeme(polluter)
                || pawnIdeo.HasMeme(polluter) && ideo.HasMeme(conservationist)))
        {
            opinion -= 15;
            contributors?.Add((conservationist.LabelCap + " / " + polluter.LabelCap, -15f));
        }
        var govLiberty = EnhancedIdeologyDefOf.MI_GovernmentLiberty;
        var govAuthority = EnhancedIdeologyDefOf.MI_GovernmentAuthority;
        if (govLiberty != null && govAuthority != null
            && (pawnIdeo.HasMeme(govLiberty) && ideo.HasMeme(govAuthority)
                || pawnIdeo.HasMeme(govAuthority) && ideo.HasMeme(govLiberty)))
        {
            opinion -= 10;
            contributors?.Add((govLiberty.LabelCap + " / " + govAuthority.LabelCap, -10f));
        }
        var wealthEqual = EnhancedIdeologyDefOf.MI_WealthEquality;
        var wealthStrat = EnhancedIdeologyDefOf.MI_WealthStratification;
        if (wealthEqual != null && wealthStrat != null
            && (pawnIdeo.HasMeme(wealthEqual) && ideo.HasMeme(wealthStrat)
                || pawnIdeo.HasMeme(wealthStrat) && ideo.HasMeme(wealthEqual)))
        {
            opinion -= 10;
            contributors?.Add((wealthEqual.LabelCap + " / " + wealthStrat.LabelCap, -10f));
        }


        // Pawn trait compatibility with target ideo's memes.
        foreach (var meme in ideo.memes)
        {
            if (!meme.agreeableTraits.NullOrEmpty())
                foreach (var trait in meme.agreeableTraits)
                    if (trait.HasTrait(pawn))
                    {
                        opinion += 10;
                        contributors?.Add((trait.def?.LabelCap ?? meme.LabelCap, 10f));
                    }

            if (!meme.disagreeableTraits.NullOrEmpty())
                foreach (var trait in meme.disagreeableTraits)
                    if (trait.HasTrait(pawn))
                    {
                        opinion -= 10;
                        contributors?.Add((trait.def?.LabelCap ?? meme.LabelCap, -10f));
                    }
        }


        // Diet gene bonuses: obligate herbivores/carnivores feel a strong pull toward ideos that share
        // their dietary needs, and an extra pull toward the Vegan meme when herbivorous.
        var meatEatingIssue = DefDatabase<IssueDef>.GetNamedSilentFail("MeatEating");
        if (meatEatingIssue != null)
        {
            if (IssueStanceTracker.PawnHasActiveGene(pawn, EnhancedIdeologyDefOf.BS_Diet_Herbivore))
            {
                var ideoRank = IssueStanceTracker.HeldRank(ideo, meatEatingIssue);
                var abhorrentRank = PreceptLadder.RankOfName(meatEatingIssue, "MeatEating_Abhorrent");
                if (abhorrentRank >= 0 && ideoRank >= 0 && ideoRank <= abhorrentRank)
                {
                    opinion += 20;
                    contributors?.Add(("EnhancedIdeology.DietGeneAntiMeat".Translate(), 20f));
                }
                if (EnhancedIdeologyDefOf.VME_Vegan != null && ideo.HasMeme(EnhancedIdeologyDefOf.VME_Vegan))
                {
                    opinion += 15;
                    contributors?.Add((EnhancedIdeologyDefOf.VME_Vegan.LabelCap, 15f));
                }
            }
            else if (IssueStanceTracker.PawnHasActiveGene(pawn, EnhancedIdeologyDefOf.BS_Diet_Carnivore))
            {
                var ideoRank = IssueStanceTracker.HeldRank(ideo, meatEatingIssue);
                var nonMeatDisapprovedRank = PreceptLadder.RankOfName(meatEatingIssue, "MeatEating_NonMeat_Disapproved");
                if (nonMeatDisapprovedRank >= 0 && ideoRank >= nonMeatDisapprovedRank)
                {
                    opinion += 20;
                    contributors?.Add(("EnhancedIdeology.DietGeneProMeat".Translate(), 20f));
                }
            }
        }

        // Xenotype check: a pawn who is not a preferred xenotype for the target ideo faces a structural barrier.
        if (ModsConfig.BiotechActive && pawn.genes != null)
        {
            var preferredKeys = PreceptPolicy.PreferredXenotypeKeys(ideo);
            if (preferredKeys.Count > 0)
            {
                var pawnKey = pawn.genes.UniqueXenotype
                    ? pawn.genes.xenotypeName
                    : pawn.genes.Xenotype?.defName;
                if (pawnKey != null && preferredKeys.Contains(pawnKey))
                {
                    opinion += 20;
                    contributors?.Add(("EnhancedIdeology.XenotypePreferred".Translate(), 20f));
                }
                else if (pawnKey != null)
                {
                    opinion -= 35;
                    contributors?.Add(("EnhancedIdeology.XenotypeDisapproved".Translate(), -35f));
                }
            }
        }


        // Structural precept fit: for each issue the target ideo takes a position on or the pawn holds a stance
        // on, how the target's stance compares to the pawn's preferred stance, weighted by conviction, averaged
        // and scaled to 0-100 (R2). Issues neither side holds are irrelevant. The pawn side comes from their
        // stances, not their current faith, so an ideo scores the same whether it is theirs or not.
        var oppositionScale = EnhancedIdeologyMod.Settings.PreceptOppositionScale;
        var preceptStart = contributors?.Count ?? 0;
        float preceptSum = 0;
        int issueCount = 0;
        var inducedTargets = InducedTargets(stances, ideo);
        var relevantIssues = ideo.precepts.Select(precept => precept.def.issue)
            .Where(issue => issue != null)
            .Concat(stances.IssueStances().Select(stance => stance.issue).Where(issue => HoldsStance(stances, issue)))
            .Concat(inducedTargets)
            .Distinct();

        foreach (var issue in relevantIssues)
        {
            var perIssue = PerIssueOpinion(stances, pawnIdeo, ideo, issue!, inducedTargets, oppositionScale, out var graded);
            if (!graded) continue;
            preceptSum += perIssue;
            issueCount++;
            if (contributors != null && perIssue != 0f)
                contributors.Add((issue!.LabelCap, perIssue));
        }

        if (issueCount > 0)
        {
            opinion += (preceptSum / issueCount) * 5f;
            // Rescale raw per-issue values to the averaged, 5x contribution so contributors still sum to it.
            if (contributors != null)
                for (var ii = preceptStart; ii < contributors.Count; ii++)
                    contributors[ii] = (contributors[ii].label, contributors[ii].pct * 5f / issueCount);
        }


        // Universally-valued issues (Charity): a flat boost when the target ideo holds a stance on them, or a
        // flat penalty for a pawn whose trait opposes the issue.
        HashSet<IssueDef>? seenUniversal = null;
        foreach (var precept in ideo.precepts)
        {
            var issue = precept.def.issue;
            if (issue != null
                && PreceptPolicy.CategoryOf(issue) == PreceptCategory.UniversalPositive
                && (seenUniversal ??= []).Add(issue))
            {
                var bonus = TraitIssueLinks.Opposes(pawn, issue) ? -UniversalPositiveBonus : UniversalPositiveBonus;
                opinion += bonus;
                contributors?.Add((issue.LabelCap, bonus));
            }
        }

        // Directional coupling penalties: flat hit scaled by conviction on the offending issue.
        var couplingPenalty = PreceptPolicy.CouplingPenalty(
            precept => Mathf.Abs(stances.GetRank(precept.issue!) - PreceptLadder.RankOf(precept)) <= 0.5f,
            ideo,
            issue => stances.GetStrength(issue));
        if (couplingPenalty != 0f)
        {
            opinion -= couplingPenalty;
            contributors?.Add(("EnhancedIdeology.CouplingPenalty".Translate(), -couplingPenalty));
        }

        // Rescale collected raw offsets into certainty-fraction units.
        if (contributors != null)
            for (var ii = start; ii < contributors.Count; ii++)
                contributors[ii] = (contributors[ii].label, contributors[ii].pct / 100f);


        return Mathf.Max(opinion, 0);
    }

    // Flat offset the pawn's faith applies to every other faith, from the strongest loyalty meme it holds.
    // Null when it holds none.
    private static (string label, float offset)? LoyaltyMemeOpinion(Ideo pawnIdeo)
    {
        var gestaltMeme = EnhancedIdeologyDefOf.VME_Gestalt;
        var nationalistMeme = EnhancedIdeologyDefOf.VME_Nationalist;
        var isolationistMeme = EnhancedIdeologyDefOf.VFEA_Isolationist;
        var violentConversionMeme = EnhancedIdeologyDefOf.VME_ViolentConversion;
        if (gestaltMeme != null && pawnIdeo.HasMeme(gestaltMeme))
            return (gestaltMeme.LabelCap, -30f);
        if (isolationistMeme != null && pawnIdeo.HasMeme(isolationistMeme))
            return (isolationistMeme.LabelCap, -25f);
        if (violentConversionMeme != null && pawnIdeo.HasMeme(violentConversionMeme))
            return (violentConversionMeme.LabelCap, -25f);
        if (pawnIdeo.HasMeme(EnhancedIdeologyDefOf.Supremacist))
            return (EnhancedIdeologyDefOf.Supremacist.LabelCap, -20f);
        if (pawnIdeo.HasMeme(EnhancedIdeologyDefOf.Collectivist))
            return (EnhancedIdeologyDefOf.Collectivist.LabelCap, -20f);
        if (nationalistMeme != null && pawnIdeo.HasMeme(nationalistMeme))
            return (nationalistMeme.LabelCap, -20f);
        if (pawnIdeo.HasMeme(EnhancedIdeologyDefOf.Loyalist))
            return (EnhancedIdeologyDefOf.Loyalist.LabelCap, -10f);
        if (pawnIdeo.HasMeme(EnhancedIdeologyDefOf.Guilty))
            return (EnhancedIdeologyDefOf.Guilty.LabelCap, 10f);
        return null;
    }

    // Issues graded on the rung ladder even when not Moral: those the target ideo takes an induced stance on,
    // plus inducible issues the pawn holds a stance on. Depends on the pawn's stances, not their membership.
    internal static HashSet<IssueDef> InducedTargets(IssueStanceTracker stances, Ideo targetIdeo) =>
        [.. PreceptPolicy.InducedIssues(targetIdeo), .. PreceptPolicy.InducibleIssues().Where(issue => HoldsStance(stances, issue))];

    // True when the pawn's stance on `issue` is off the Don't-care rung, i.e. they hold a belief about it.
    private static bool HoldsStance(IssueStanceTracker stances, IssueDef issue) =>
        Mathf.Abs(stances.GetRank(issue) - PreceptLadder.DontCareRank(issue)) > InteractionWorker_IdeologicalDebatePrecept.DebateRankEpsilon;

    // Raw per-issue opinion (roughly +/-strength) the pawn holds toward `targetIdeo`'s stance on `issue`.
    // Moral issues and coupled targets grade by rung distance; Special issues carry bespoke categorical logic.
    // `graded` is false when neither faith takes a comparable position (caller excludes from the mean).
    internal static float PerIssueOpinion(
        IssueStanceTracker stances,
        Ideo pawnIdeo,
        Ideo targetIdeo,
        IssueDef issue,
        HashSet<IssueDef> inducedTargets,
        float oppositionScale,
        out bool graded)
    {
        graded = true;
        var category = PreceptPolicy.CategoryOf(issue);
        if (category == PreceptCategory.Moral || inducedTargets.Contains(issue))
        {
            var pawnRank = stances.GetRank(issue);
            var targetRank = IssueStanceTracker.HeldRank(targetIdeo, issue);
            var minRank = Mathf.Min(Mathf.Min(0f, PreceptLadder.DontCareRank(issue)), Mathf.Min(pawnRank, targetRank));
            var maxRank = Mathf.Max(PreceptLadder.Rungs(issue).Count - 1, Mathf.Max(pawnRank, targetRank));
            return PreceptLadder.OpinionOnPrecept(
                pawnRank, targetRank, minRank, maxRank, stances.GetStrength(issue), oppositionScale);
        }

        if (category == PreceptCategory.Special
            && (PreceptPolicy.TryPayloadSpecialOpinion(issue, pawnIdeo, targetIdeo, stances.GetStrength(issue), out var special)
                || PreceptPolicy.TrySpecialOpinion(
                    issue, stances.GetRank(issue), IssueStanceTracker.HeldRank(targetIdeo, issue), stances.GetStrength(issue), oppositionScale, out special)))
        {
            return special;
        }

        graded = false;
        return 0f;
    }

    // True when this issue is worth targeting in a debate: either the rank gap is large enough to close, or
    // the guide holds stronger conviction so a win raises rather than drains the pawn's belief strength.
    internal static bool WorthTargeting(IssueStanceTracker stances, IssueDef issue, float targetRank, IssueStanceTracker? guideStances)
    {
        if (guideStances == null) return true;
        if (Mathf.Abs(stances.GetRank(issue) - targetRank) > InteractionWorker_IdeologicalDebatePrecept.DebateRankEpsilon) return true;
        var guideStrength = guideStances.IssueStances().First(s => s.issue == issue).strength;
        return guideStrength > stances.GetStrength(issue);
    }

    // Dev-only: full extent/rank breakdown behind a per-issue opinion, for diagnosing structural opinions.
    internal static string IssueOpinionDebug(IssueStanceTracker stances, Ideo ideo, IssueDef issue)
    {
        var pawnRank = stances.GetRank(issue);
        var targetRank = IssueStanceTracker.HeldRank(ideo, issue);
        var rungCount = PreceptLadder.Rungs(issue).Count;
        var dontCare = PreceptLadder.DontCareRank(issue);
        var strength = stances.GetStrength(issue);
        var minRank = Mathf.Min(Mathf.Min(0f, dontCare), Mathf.Min(pawnRank, targetRank));
        var maxRank = Mathf.Max(rungCount - 1, Mathf.Max(pawnRank, targetRank));
        var maxDist = Mathf.Max(pawnRank - minRank, maxRank - pawnRank);
        var t = maxDist > 0f ? Mathf.Abs(targetRank - pawnRank) / maxDist : 0f;
        var oppositionScale = EnhancedIdeologyMod.Settings.PreceptOppositionScale;
        var falloff = 1f - (t * (1f + oppositionScale));
        var ladder = string.Join(", ", PreceptLadder.Rungs(issue)
            .Select((precept, ix) => (rank: (float)ix, label: precept.defName))
            .Append((rank: dontCare, label: "DontCare"))
            .OrderBy(e => e.rank)
            .Select(e => $"{e.rank:F1}:{e.label}"));
        return $"str={strength:F1} cat={PreceptPolicy.CategoryOf(issue)}"
            + $"\n  pawn={pawnRank:F2} target={targetRank:F2} rungs={rungCount} dontCare={dontCare:F2}"
            + $"\n  extent=[{minRank:F2},{maxRank:F2}] maxDist={maxDist:F2} t={t:F2}"
            + $"\n  oppScale={oppositionScale:F2} falloff={falloff:F2}"
            + $"\n  ladder: {ladder}";
    }
}
