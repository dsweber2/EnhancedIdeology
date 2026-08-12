namespace EnhancedIdeology.HarmonyPatches;

[HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawPawnCertainty))]
[HotSwappable]
internal static class SocialCardUtility_DrawCertainty
{
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

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var data = comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        // Refresh the cached setpoint/bands (cheap - guarded to a recache per rare tick).
        var certaintyChangePerDay = pawn.ideo.CertaintyChangePerDay;

        if (Mouse.IsOver(containerRect))
        {
            Widgets.DrawHighlight(containerRect);

            var certaintyChange = (certaintyChangePerDay >= 0f ? "+" : "") + certaintyChangePerDay.ToStringPercent();

            var tip = "EnhancedIdeology.PawnCertaintyTooltip".Translate(pawn.Named("PAWN"), pawn.Ideo.Named("IDEO"), pawn.ideo.Certainty.ToStringPercent()) + "\n\n";
            tip += "EnhancedIdeology.CertaintyTarget".Translate(data.CachedTargetCertainty.ToStringPercent()) + "\n";
            tip += "EnhancedIdeology.CertainChangePerDay".Translate(certaintyChange) + "\n\n";

            tip += Band("EnhancedIdeology.CertaintyBandStructural", data.CachedStructural, data.StructuralContributors);
            tip += Band("EnhancedIdeology.CertaintyBandRelational", data.CachedRelational, data.RelationalContributors);
            tip += Band("EnhancedIdeology.CertaintyBandPractice", data.CachedPractitional, data.PractitionalContributors);
            tip += "EnhancedIdeology.CertaintyBandDifficulty".Translate(Signed(data.CachedDifficulty));

            TooltipHandler.TipRegion(containerRect, tip);
        }
        if (Widgets.ButtonInvisible(containerRect))
        {
            IdeoUIUtility.OpenIdeoInfo(pawn.Ideo);
        }

        // Draw it the way Need.DrawOnGUI does: a real FillableBar, a threshold tick in its lower half, and the
        // instant-level marker just below the bar. The bar gives up MarkerHeight of its height so the marker fits
        // inside the 40px certainty row instead of colliding with the role row beneath it.
        var certaintyBar = new Rect(barRect.x, barRect.y, barRect.width, barRect.height - CertaintyBar.MarkerHeight);
        var filled = Widgets.FillableBar(certaintyBar, Mathf.Clamp01(pawn.ideo.Certainty));

        CertaintyBar.DrawThreshold(filled, EnhancedIdeologyMod.Settings.CrisisThreshold, pawn.ideo.Certainty);
        CertaintyBar.DrawTargetMarker(filled, Mathf.Clamp01(data.CachedTargetCertainty));

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
