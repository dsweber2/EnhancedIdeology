namespace EnhancedIdeology;

// Shared conviction-valley geometry used by all stance-move operations (debate, conversion, reassure, ritual).
// The valley keeps the belief arc consistent: conviction craters through the "muddled middle" and recovers once
// a new rung is adopted, so belief migrates over repeated events rather than in a single jump.
// See analysis/conviction_valley.py for the derivation and plots.
[HotSwappable]
internal static class ConvictionMath
{
    private const float ValleyFloor = 1f;
    private const float ValleyWidthWinner = 0.3f;
    private const float ValleyWidthHome = 0.1f;

    // Below this rank gap an arm is treated as at its vertex rather than solving a near-singular amplitude.
    // Gates all three degeneracy guards: bowl-mode switch, arm-amplitude clamp, endpoint snap.
    internal const float ValleyMinGap = 0.15f;

    private const int ValleyArcSamples = 64;

    // Arc length a 1x debate covers before the settings multiplier and pawn stats scale it.
    internal const float DebateBaseArc = 4f;

    // Arc length covered per unit of |positivityIndex| at ritual quality=1, before pawn stats scale it.
    // At 1, a great (positivityIndex=2) unbooted ritual is 2 arc unit per issue — 1 debate.
    internal const float RitualBaseArc = 1.0f;

    // The ladder end farthest from towardRank: the far pole the conviction valley hangs from.
    internal static float LadderExtremeAwayFrom(IssueDef issue, float towardRank)
    {
        var top = PreceptLadder.Rungs(issue).Count - 1;
        return (towardRank - 0f) >= (top - towardRank) ? 0f : top;
    }

    // Amplitude that makes a cosh arm hang from (vertex, floor) with zero slope, passing through (anchorRank, anchorStrength).
    private static float ArmAmplitude(float anchorRank, float anchorStrength, float vertex, float floor, float width)
    {
        var denom = Mathf.Max(
            (float)Math.Cosh((anchorRank - vertex) / width) - 1f,
            (float)Math.Cosh(ValleyMinGap / width) - 1f);
        return (anchorStrength - floor) / denom;
    }

    // Advance a stance one step along the conviction valley: from (ri, si) toward rw/sw, far pole at rm.
    // Arc metric: ds² = (rankWeight·drank)² + dstrength². A firm stance crawls in rank; a shaky one races.
    internal static (float rank, float strength) ValleyStep(float ri, float si, float rw, float rm, float sw, float stepLength)
    {
        if (Mathf.Abs(rw - ri) < ValleyMinGap)
        {
            return (rw, si + ((sw - si) * 0.5f));
        }

        var vertex = 0.5f * (rw + rm);
        float floor, ampWinner, ampHome, widthWinner, widthHome;

        if (Mathf.Sign(ri - vertex) == Mathf.Sign(rw - vertex) || Mathf.Abs(ri - vertex) < ValleyMinGap)
        {
            vertex = ri;
            floor = si;
            ampWinner = ampHome = ArmAmplitude(rw, sw, vertex, floor, ValleyWidthWinner);
            widthWinner = widthHome = ValleyWidthWinner;
        }
        else
        {
            floor = ValleyFloor;
            ampWinner = ArmAmplitude(rw, sw, vertex, floor, ValleyWidthWinner);
            ampHome = ArmAmplitude(ri, si, vertex, floor, ValleyWidthHome);
            widthWinner = ValleyWidthWinner;
            widthHome = ValleyWidthHome;
        }

        var winnerSide = Mathf.Sign(rw - vertex);
        float Curve(float rank)
        {
            var offset = rank - vertex;
            var onWinnerSide = Mathf.Sign(offset) == winnerSide;
            var amp = onWinnerSide ? ampWinner : ampHome;
            var width = onWinnerSide ? widthWinner : widthHome;
            return floor + (amp * ((float)Math.Cosh(offset / width) - 1f));
        }

        var rankWeight = IdeoTrackerData.MaxConvictionStrength / Mathf.Max(Mathf.Abs(rw - rm), ValleyMinGap);

        var prevRank = ri;
        var prevStrength = Curve(ri);
        var cumulative = 0f;
        for (var ii = 1; ii <= ValleyArcSamples; ii++)
        {
            var rank = ri + ((rw - ri) * ((float)ii / ValleyArcSamples));
            var strength = Curve(rank);
            var dRank = rankWeight * (rank - prevRank);
            var dStrength = strength - prevStrength;
            var segment = Mathf.Sqrt((dRank * dRank) + (dStrength * dStrength));
            if (cumulative + segment >= stepLength)
            {
                var landedRank = prevRank + ((rank - prevRank) * ((stepLength - cumulative) / segment));
                return (landedRank, Curve(landedRank));
            }
            cumulative += segment;
            prevRank = rank;
            prevStrength = strength;
        }

        return (rw, sw);
    }

    // The loser of an argument walks a fixed arc toward the winner's rung and conviction. Step length scales with
    // the winner's ConversionPower, the loser's fragility (CertaintyLossFactor), and pullMultiplier (1x = ordinary
    // debate; conversion ability can pass higher values for directed casts).
    internal static void PullStance(
        GameComponent_EnhancedIdeology comp, Pawn winner, Pawn loser, IssueDef issue, float targetRank, float pullMultiplier)
    {
        var winnerTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(winner);
        var loserTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(loser);

        var loserStance = loserTracker.IssueStances().First(stance => stance.issue == issue);
        var winnerStrength = winnerTracker.IssueStances().First(stance => stance.issue == issue).strength;

        var stepLength = DebateBaseArc
            * EnhancedIdeologyMod.Settings.DebateConvictionChange
            * winner.GetStatValue(StatDefOf.ConversionPower)
            * loser.GetStatValue(StatDefOf.CertaintyLossFactor)
            * pullMultiplier;

        var farRank = LadderExtremeAwayFrom(issue, targetRank);
        var (newRank, newStrength) = ValleyStep(loserStance.rank, loserStance.strength, targetRank, farRank, winnerStrength, stepLength);

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers, $"PullStance: {loser} on {issue} ({loserStance.rank:F2},{loserStance.strength:F1}) -> ({newRank:F2},{newStrength:F1}) toward winner ({targetRank},{winnerStrength:F1}), arc {stepLength:F2}");
        loserTracker.SetIssueStance(issue, newRank, newStrength);
    }

    // Ritual variant: no winner pawn. The ideo itself is the "winner". targetRank/targetStrength are passed
    // directly; stepLength is computed by the caller from ritual quality and pawn susceptibility.
    internal static void ApplyRitualPull(
        GameComponent_EnhancedIdeology comp, Pawn pawn, IssueDef issue, float targetRank, float targetStrength, float stepLength)
    {
        var tracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        var stance = tracker.IssueStances().FirstOrDefault(s => s.issue == issue);
        if (stance.issue == null)
        {
            return;
        }

        var farRank = LadderExtremeAwayFrom(issue, targetRank);
        var (newRank, newStrength) = ValleyStep(stance.rank, stance.strength, targetRank, farRank, targetStrength, stepLength);
        tracker.SetIssueStance(issue, newRank, newStrength);
    }
}
