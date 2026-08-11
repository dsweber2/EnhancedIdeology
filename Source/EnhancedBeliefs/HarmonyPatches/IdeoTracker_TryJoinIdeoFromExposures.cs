namespace EnhancedBeliefs.HarmonyPatches;

[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.TryJoinIdeoFromExposures))]
internal static class IdeoTracker_TryJoinIdeoFromExposures
{
    [HarmonyPrefix]
    private static bool UseExposureWeightedBeliefs(Pawn_IdeoTracker __instance, ref bool __result)
    {
        if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive)
            return true;
        if (__instance.Ideo != null)
        {
            __result = false;
            return false;
        }
        if (Find.IdeoManager.classicMode)
            return true;
        if (__instance.BabyIdeoExposureTotal <= 0f)
            return true;

        var pawn = __instance.pawn;
        var exposures = __instance.BabyIdeoExposureSorted;
        var totalExposure = __instance.BabyIdeoExposureTotal;

        var stances = GenerateExposureWeightedStances(pawn, exposures, totalExposure);
        var bestIdeo = PickBestFittingIdeo(stances);
        if (bestIdeo == null)
            return true;

        __instance.SetIdeo(bestIdeo);
        if (__instance.Ideo == null)
        {
            __result = false;
            return false;
        }

        var tracker = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>()
            .PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        foreach (var (issue, rank, strength) in stances)
            tracker.SetIssueStance(issue, rank, strength);

        __result = true;
        return false;
    }

    private static List<(IssueDef issue, float rank, float strength)> GenerateExposureWeightedStances(
        Pawn pawn,
        List<Pawn_IdeoTracker.IdeoExposureWeight> exposures,
        float totalExposure)
    {
        var result = new List<(IssueDef, float, float)>();
        var traitOffset = IdeoTrackerData.ConvictionOffsetFromTraits(pawn.story.traits.allTraits);

        foreach (var issue in DefDatabase<IssueDef>.AllDefs)
        {
            float weightedRank = 0f;
            foreach (var weight in exposures)
                weightedRank += IdeoTrackerData.HeldRank(weight.ideo, issue) * (weight.exposure / totalExposure);

            var strength = Mathf.Clamp(
                Rand.Range(IdeoTrackerData.BaseConvictionMin, IdeoTrackerData.BaseConvictionMax) + traitOffset,
                0f, IdeoTrackerData.AbsoluteMaxConvictionStrength);

            result.Add((issue, weightedRank, strength));
        }

        return result;
    }

    private static Ideo? PickBestFittingIdeo(List<(IssueDef issue, float rank, float strength)> stances)
    {
        var stanceByIssue = stances.ToDictionary(s => s.issue, s => (s.rank, s.strength));
        Ideo? best = null;
        float bestScore = float.MinValue;

        foreach (var ideo in Find.IdeoManager.IdeosListForReading)
        {
            float score = FitScore(ideo, stanceByIssue);
            if (score > bestScore)
            {
                bestScore = score;
                best = ideo;
            }
        }

        return best;
    }

    private static float FitScore(Ideo ideo, Dictionary<IssueDef, (float rank, float strength)> stanceByIssue)
    {
        float total = 0f;
        int count = 0;
        var oppositionScale = EnhancedBeliefsMod.Settings.PreceptOppositionScale;

        foreach (var precept in ideo.precepts)
        {
            var issue = precept.def.issue;
            if (issue == null || !stanceByIssue.TryGetValue(issue, out var stance))
                continue;

            var ideoRank = PreceptLadder.RankOf(precept.def);
            var minRank = Mathf.Min(Mathf.Min(0f, PreceptLadder.DontCareRank(issue)),
                Mathf.Min(stance.rank, ideoRank));
            var maxRank = Mathf.Max(PreceptLadder.Rungs(issue).Count - 1,
                Mathf.Max(stance.rank, ideoRank));

            total += PreceptLadder.OpinionOnPrecept(stance.rank, ideoRank, minRank, maxRank,
                stance.strength, oppositionScale);
            count++;
        }

        return count > 0 ? total / count : 0f;
    }
}
