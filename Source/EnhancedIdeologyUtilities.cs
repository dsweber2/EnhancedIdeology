namespace EnhancedIdeology;

internal static class EnhancedIdeologyUtilities
{
    internal static List<T> TryGetComps<T>(this Precept precept) where T : PreceptComp
    {
        return precept.def.TryGetComps<T>();
    }

    internal static List<T> TryGetComps<T>(this PreceptDef precept) where T : PreceptComp
    {
        List<T> comps = [];

        foreach (var preceptComp in precept.comps)
        {
            if (preceptComp is T comp)
            {
                comps.Add(comp);
            }
        }

        return comps;
    }

    // Normalized [0,1] strictness of an ideo's apostacy stance: 0 = no stance or permissive side,
    // 1 = Abhorrent. Scales linearly across the strict rungs above the don't-care placement.
    internal static float ApostacyStrictness(Ideo? ideo)
    {
        if (ideo == null) return 0f;
        var issue = DefDatabase<IssueDef>.GetNamedSilentFail("Apostasy");
        if (issue == null) return 0f;
        var precept = ideo.precepts.FirstOrDefault(p => p.def.issue == issue);
        if (precept == null) return 0f;
        var rank = PreceptLadder.RankOf(precept.def);
        var dontCareRank = PreceptLadder.DontCareRank(issue);
        var maxRank = PreceptLadder.Rungs(issue).Count - 1f;
        if (maxRank <= dontCareRank) return 0f;
        return Mathf.Clamp01((rank - dontCareRank) / (maxRank - dontCareRank));
    }

    internal static void ShowCertaintyChangeMote(Pawn recipient, float before, float after)
    {
        if (!recipient.Spawned)
        {
            return;
        }

        string text = "Certainty".Translate() + "\n" + before.ToStringPercent() + " -> " + after.ToStringPercent();
        MoteMaker.ThrowText(recipient.DrawPos, recipient.Map, text, 8f);
    }
}
