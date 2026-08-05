namespace EnhancedBeliefs.HarmonyPatches;

[HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawPawnCertainty))]
[HotSwappable]
internal static class SocialCardUtility_DrawCertainty
{
    private static readonly Texture2D BaselineBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.12f, 0.38f, 0.12f));

    private static Rect containerRect;
    internal static Rect ContainerRect => containerRect;

    private static bool Prefix(Pawn pawn, Rect rect)
    {
        var num = rect.x + 17f;
        Rect iconRect = new(num, rect.y + (rect.height / 2f) - 16f, 32f, 32f);
        pawn.Ideo.DrawIcon(iconRect);
        num += 42f;
        Text.Anchor = TextAnchor.MiddleLeft;
        Rect rect3 = new(num, rect.y, (rect.width / 2f) - num, rect.height);
        Widgets.Label(rect3, pawn.Ideo.name.Truncate(rect3.width));
        Text.Anchor = TextAnchor.UpperLeft;
        num += rect3.width + 10f;
        containerRect = new Rect(iconRect.x, rect.y + (rect.height / 2f) - 16f, 0f, 32f);
        Rect barRect = new(num, rect.y + (rect.height / 2f) - 16f, rect.width - num - 26f, 32f);
        containerRect.xMax = barRect.xMax;

        var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
        var data = comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        // Refresh the cached setpoint/bands (cheap - guarded to a recache per rare tick).
        var certaintyChangePerDay = pawn.ideo.CertaintyChangePerDay;

        if (Mouse.IsOver(containerRect))
        {
            Widgets.DrawHighlight(containerRect);

            var certaintyChange = (certaintyChangePerDay >= 0f ? "+" : "") + certaintyChangePerDay.ToStringPercent();

            var tip = "EnhancedBeliefs.PawnCertaintyTooltip".Translate(pawn.Named("PAWN"), pawn.Ideo.Named("IDEO"), pawn.ideo.Certainty.ToStringPercent()) + "\n\n";
            tip += "EnhancedBeliefs.CertaintyTarget".Translate(data.CachedTargetCertainty.ToStringPercent()) + "\n";
            tip += "EnhancedBeliefs.CertainChangePerDay".Translate(certaintyChange) + "\n\n";

            tip += Band("EnhancedBeliefs.CertaintyBandStructural", data.CachedStructural, data.StructuralContributors);
            tip += Band("EnhancedBeliefs.CertaintyBandRelational", data.CachedRelational, data.RelationalContributors);
            tip += Band("EnhancedBeliefs.CertaintyBandPractice", data.CachedPractitional, data.PractitionalContributors);
            tip += "EnhancedBeliefs.CertaintyBandDifficulty".Translate(Signed(data.CachedDifficulty));

            TooltipHandler.TipRegion(containerRect, tip);
        }
        if (Widgets.ButtonInvisible(containerRect))
        {
            IdeoUIUtility.OpenIdeoInfo(pawn.Ideo);
        }

        var innerRect = barRect.ContractedBy(4f);

        // Background
        GUI.DrawTexture(innerRect, BaseContent.BlackTex);

        // Target certainty (dark green, behind current bar) - where certainty is drifting toward
        var target = data.CachedTargetCertainty;
        if (target > 0f)
        {
            var targetRect = new Rect(innerRect.x, innerRect.y, innerRect.width * Mathf.Clamp01(target), innerRect.height);
            GUI.DrawTexture(targetRect, BaselineBarTex);
        }

        // Current certainty on top
        if (pawn.ideo.Certainty > 0f)
        {
            var fillRect = new Rect(innerRect.x, innerRect.y, innerRect.width * Mathf.Clamp01(pawn.ideo.Certainty), innerRect.height);
            GUI.DrawTexture(fillRect, SocialCardUtility.BarFullTexHor);
        }

        return false;
    }

    private static string Signed(float fraction)
    {
        return (fraction >= 0f ? "+" : "") + fraction.ToStringPercent();
    }

    // A band header line ("Structural  +48%") followed by its top-3 contributors, each signed.
    private static string Band(string labelKey, float total, List<(string label, float pct)> contributors)
    {
        var text = labelKey.Translate(Signed(total)) + "\n";

        foreach (var (label, pct) in contributors.OrderByDescending(c => Math.Abs(c.pct)).Take(3))
        {
            text += $"    {label}: {Signed(pct)}\n";
        }

        return text;
    }
}
