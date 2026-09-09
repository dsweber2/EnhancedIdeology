namespace EnhancedIdeology;

// Conversion logic: evaluates ideo candidates, draws the weighted outcome, executes ideo switches.
// All methods are static and receive IdeoTrackerData explicitly so the class carries no per-pawn state.
internal static class ConversionEvaluator
{
    // Relative-preference conversion probability toward `candidate`: 1 - opinionOfOwn/opinionOfCandidate,
    // and 0 unless the pawn genuinely prefers the candidate.
    internal static float ConversionProbability(IdeoTrackerData data, Ideo candidate)
    {
        var opinion = data.IdeoOpinion(candidate);
        var current = data.IdeoOpinion(data.Pawn.Ideo!);
        return opinion > current ? (opinion - current) / opinion : 0f;
    }

    // Read-only preview of the conversion chance toward `target` after a won attempt applies its certainty
    // knock (Certainty *= knock). Folds in the knock but not the stance pull; ignores competing ideos and
    // the crisis candidate, so it is an estimate, not the exact draw.
    internal static float ConversionChanceAfterKnock(IdeoTrackerData data, Ideo target, float knock)
    {
        var opinion = data.IdeoOpinion(target);
        var current = data.IdeoOpinion(data.Pawn.Ideo!) * knock;
        return opinion > current ? (opinion - current) / opinion : 0f;
    }

    // Discrete, one-shot conversion driven by acute social pressure (debates, directed attempts). The pawn's
    // real ideos and a "crisis of faith" pseudo-candidate compete in one weighted draw; if the crisis wins,
    // that is the IdeoChange breakdown. No time integration here — the event itself is the occurrence.
    internal static ConversionOutcome CheckConversion(
        IdeoTrackerData data,
        Ideo? priorityIdeo = null,
        bool noBreakdown = false,
        List<Ideo>? excludeIdeos = null,
        List<Ideo>? whitelistIdeos = null)
    {
        var pawn = data.Pawn;
        if (!ModLister.CheckIdeology("Ideoligion conversion") || pawn.DevelopmentalStage.Baby() || Find.IdeoManager.classicMode)
            return ConversionOutcome.Failure;

        // Use the higher of lived certainty and structural alignment as the bar: a pawn whose conviction
        // hasn't caught up to their structural fit yet shouldn't be treated as a conversion target.
        var current = Mathf.Max(data.IdeoOpinion(pawn.Ideo!), data.CachedStructural);
        var candidates = new List<(Ideo? ideo, float chance, float weight)>();

        IEnumerable<Ideo> pool = whitelistIdeos
            ?? (priorityIdeo != null ? [priorityIdeo] : Find.IdeoManager.IdeosListForReading);
        foreach (var ideo in pool)
        {
            if (ideo == pawn.Ideo || (excludeIdeos != null && excludeIdeos.Contains(ideo)))
                continue;

            var opinion = data.IdeoOpinion(ideo);
            if (opinion <= current)
                continue;

            candidates.Add((ideo, (opinion - current) / opinion, opinion - current));
        }

        var crisisThreshold = EffectiveCrisisThreshold(data);
        AddCrisisCandidate(candidates, current, crisisThreshold, w => w / crisisThreshold);

        var index = SelectWeightedConversion(candidates);
        if (index < 0)
            return ConversionOutcome.Failure;

        var chosen = candidates[index].ideo;
        if (chosen == null)
        {
            if (noBreakdown)
                return ConversionOutcome.Failure;

            TriggerCrisisOfFaith(data);
            return ConversionOutcome.Breakdown;
        }

        ApplyConversion(data, chosen);
        return ConversionOutcome.Success;
    }

    // Continuous, spontaneous conversion integrated over elapsed time. Each candidate's chance is its
    // ConversionProbability treated as a hazard over ConversionInterval days, so it is invariant to how
    // finely time is sampled — tick batching never silently sets the conversion rate.
    internal static void TryBackgroundConversion(IdeoTrackerData data, float deltaDays)
    {
        var pawn = data.Pawn;
        if (deltaDays <= 0f || !ModLister.CheckIdeology("Ideoligion conversion")
            || pawn.DevelopmentalStage.Baby() || Find.IdeoManager.classicMode)
        {
            return;
        }

        var interval = EnhancedIdeologyMod.Settings.ConversionInterval;
        var current = Mathf.Max(data.IdeoOpinion(pawn.Ideo!), data.CachedStructural);
        var candidates = new List<(Ideo? ideo, float chance, float weight)>();

        foreach (var ideo in Find.IdeoManager.IdeosListForReading)
        {
            if (ideo == pawn.Ideo)
                continue;

            var opinion = data.IdeoOpinion(ideo);
            if (opinion <= current)
                continue;

            var chance = HazardConversionChance((opinion - current) / opinion, deltaDays, interval);
            candidates.Add((ideo, chance, opinion - current));
        }

        var crisisThreshold = EffectiveCrisisThreshold(data);
        AddCrisisCandidate(candidates, current, crisisThreshold,
            w => HazardConversionChance(w / crisisThreshold, deltaDays, interval));

        // CertaintyLossFactor scales the conversion/breakdown hazard: a resistant pawn (factor < 1) clings
        // to their faith, a fragile one (factor > 1) drifts away faster. Certainty itself also damps the
        // rate: a fully certain pawn (certainty >= 1) has no spontaneous drift. An ideo that condemns
        // apostacy further resists — same scaling as PullStance (0.25x at Abhorrent). Only the spontaneous
        // path applies these; acute-event callers already fold CertaintyLossFactor into the certainty shed.
        var certaintyResistance = Mathf.Clamp01(1f - Mathf.Clamp01(data.ExtendedCertainty));
        var apostacyResistance = 1f - (EnhancedIdeologyUtilities.ApostacyStrictness(pawn.Ideo) * 0.75f);
        var index = SelectWeightedConversion(candidates,
            pawn.GetStatValue(StatDefOf.CertaintyLossFactor) * certaintyResistance * apostacyResistance);
        if (index < 0)
            return;

        var chosen = candidates[index].ideo;
        if (chosen == null)
        {
            TriggerCrisisOfFaith(data);
            return;
        }

        ApplyConversion(data, chosen);
    }

    // When the crisis-of-faith candidate wins: if the pawn's mood is already below their minor break
    // threshold, the crisis collapses into a normal mood break. Otherwise, convert to the most-preferred
    // ideo if it outrates the current, land certainty at 1.5x the crisis threshold, then enter the wander
    // mental state.
    internal static void TriggerCrisisOfFaith(IdeoTrackerData data)
    {
        var pawn = data.Pawn;
        var mood = pawn.needs.mood;
        if (mood != null && mood.CurLevel < pawn.mindState.mentalBreaker.BreakThresholdMinor)
        {
            Find.PlayLog.Add(new PlayLogEntry_CrisisOfFaith(pawn, CrisisOutcome.MoodBreak));
            pawn.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak();
            return;
        }

        var crisisThreshold = EffectiveCrisisThreshold(data);
        var currentOpinion = data.IdeoOpinion(pawn.Ideo!);

        var bestIdeo = Find.IdeoManager.IdeosListForReading
            .Where(ii => ii != pawn.Ideo)
            .MaxByWithFallback(ii => data.IdeoOpinion(ii!));

        if (bestIdeo != null && data.IdeoOpinion(bestIdeo) > currentOpinion)
            ApplyConversion(data, bestIdeo);

        data.SetExtendedCertainty(1.5f * crisisThreshold);

        Find.PlayLog.Add(new PlayLogEntry_CrisisOfFaith(pawn, CrisisOutcome.Wander));
        _ = pawn.mindState.mentalStateHandler.TryStartMentalState(EnhancedIdeologyDefOf.EB_CrisisOfFaith);
    }

    // CrisisThreshold scaled by sqrt(CertaintyLossFactor). sqrt dampens the raw stat: at 3x volatile the
    // threshold rises to ~1.73x, not the full 3x a linear scale would produce.
    internal static float EffectiveCrisisThreshold(IdeoTrackerData data)
    {
        var factor = data.Pawn.GetStatValue(StatDefOf.CertaintyLossFactor);
        return Mathf.Clamp01(EnhancedIdeologyMod.Settings.CrisisThreshold * Mathf.Sqrt(factor));
    }

    // A per-window probability p, integrated over deltaDays as a hazard with the given interval.
    // Survival is multiplicative in time, so sampling the same span more finely yields the same total
    // probability — the roll cadence cannot silently change the conversion rate.
    public static float HazardConversionChance(float p, float deltaDays, float intervalDays) =>
        1f - Mathf.Pow(1f - p, deltaDays / intervalDays);

    // Adds the crisis-of-faith pseudo-candidate (null ideo) when certainty has fallen below the crisis
    // threshold. Competes in the same draw as real ideos with the same gap-based weight.
    private static void AddCrisisCandidate(
        List<(Ideo? ideo, float chance, float weight)> candidates,
        float current,
        float crisisThreshold,
        Func<float, float> chanceOf)
    {
        if (current >= crisisThreshold)
            return;

        var weight = crisisThreshold - current;
        candidates.Add((null, chanceOf(weight), weight));
    }

    // Weighted conversion draw. First rolls the competing-risks probability that *any* candidate fires
    // (1 minus the product of each candidate's survival), then picks which one proportional to weight.
    // Splitting "whether" (chance) from "which" (weight) lets the target stay discriminating by opinion
    // gap even when certainty is near zero, where ratio-based chances all saturate toward 1.
    // chanceFactor is a plain multiplier on fire probability (clamped to [0,1]).
    // Returns the chosen candidate's index, or -1 if nothing fired.
    private static int SelectWeightedConversion(
        List<(Ideo? ideo, float chance, float weight)> candidates,
        float chanceFactor = 1f)
    {
        float survival = 1f;
        foreach (var candidate in candidates)
            survival *= 1f - candidate.chance;

        var fireChance = Mathf.Clamp01((1f - survival) * chanceFactor);
        if (Rand.Value >= fireChance)
            return -1;

        float totalWeight = 0f;
        foreach (var candidate in candidates)
            totalWeight += candidate.weight;

        if (totalWeight <= 0f)
            return -1;

        var roll = Rand.Value * totalWeight;
        for (var ii = 0; ii < candidates.Count; ii++)
        {
            roll -= candidates[ii].weight;
            if (roll <= 0f)
                return ii;
        }

        return candidates.Count - 1;
    }

    // Performs the actual conversion to newIdeo. Side-effectful by nature: swaps the pawn's ideo, reseeds
    // certainty from opinion, preserves the old ideo's standing as personal opinion, records history.
    // A convert overshoots: they arrive at their old certainty plus twice the margin by which they preferred
    // the new faith, so switching feels like a step up rather than a lateral move.
    private static void ApplyConversion(IdeoTrackerData data, Ideo newIdeo)
    {
        var pawn = data.Pawn;
        var oldCertainty = pawn.ideo.Certainty;
        var oldIdeo = pawn.Ideo;
        var oldIdeoContains = pawn.ideo.PreviousIdeos.Contains(newIdeo);

        var opinionOfNew = data.IdeoOpinion(newIdeo);
        var newCertainty = Mathf.Clamp01(oldCertainty + (2f * (opinionOfNew - oldCertainty)));

        pawn.ideo.SetIdeo(newIdeo);
        newIdeo.Notify_MemberGainedByConversion();

        data.SetExtendedCertainty(newCertainty);
        data.Opinions.PersonalIdeoOpinions[newIdeo] = 0;

        var oldBase = data.DetailedIdeoOpinion(oldIdeo!).BaseOpinion;
        data.AdjustPersonalOpinion(oldIdeo!, oldCertainty - oldBase);

        Find.PlayLog.Add(new PlayLogEntry_Conversion(pawn, newIdeo));

        if (!oldIdeoContains)
        {
            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(
                HistoryEventDefOf.ConvertedNewMember,
                pawn.Named(HistoryEventArgsNames.Doer),
                newIdeo.Named(HistoryEventArgsNames.Ideo)));
        }

        data.RecacheAllBaseOpinions();
    }
}
