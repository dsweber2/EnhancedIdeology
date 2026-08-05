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

        if (Mouse.IsOver(containerRect))
        {
            Widgets.DrawHighlight(containerRect);

            var certaintyChange = (pawn.ideo.CertaintyChangePerDay >= 0f ? "+" : "") + pawn.ideo.CertaintyChangePerDay.ToStringPercent();

            var tip = "EnhancedBeliefs.PawnCertaintyTooltip".Translate(pawn.Named("PAWN"), pawn.Ideo.Named("IDEO"), pawn.ideo.Certainty.ToStringPercent()) + "\n\n";
            tip += "EnhancedBeliefs.CertainChangePerDay".Translate(certaintyChange) + "\n";

            var moodOffset = data.CachedMoodCertaintyOffset;
            var moodSign = moodOffset >= 0f ? "+" : "";
            tip += "EnhancedBeliefs.CertaintyFromPreceptMoods".Translate(moodSign + moodOffset.ToStringPercent()) + "\n";

            var relMult = data.CachedRelationshipMultiplier;
            tip += "EnhancedBeliefs.CertaintyRelationshipModifier".Translate(relMult.ToStringPercent()) + "\n";

            var relationships = data.GetOwnIdeoRelationships().OrderByDescending(r => Math.Abs(r.opinion)).Take(5).ToList();
            if (relationships.Count > 0)
            {
                foreach (var (relPawn, opinion) in relationships)
                {
                    var opSign = opinion >= 0f ? "+" : "";
                    tip += $"  - {relPawn.LabelShort}: {opSign}{opinion:F0}\n";
                }
            }

            if (data.CachedInactivityLoss > 0f)
            {
                tip += "EnhancedBeliefs.CertaintyLossFromInactivity".Translate(data.CachedInactivityLoss.ToStringPercent()) + "\n";
            }

            TooltipHandler.TipRegion(containerRect, tip);
        }
        if (Widgets.ButtonInvisible(containerRect))
        {
            IdeoUIUtility.OpenIdeoInfo(pawn.Ideo);
        }

        var innerRect = barRect.ContractedBy(4f);

        // Background
        GUI.DrawTexture(innerRect, BaseContent.BlackTex);

        // Structural baseline (dark green, behind current bar)
        var baseline = data.StructuralBaselineCertainty;
        if (baseline > 0f)
        {
            var baselineRect = new Rect(innerRect.x, innerRect.y, innerRect.width * Mathf.Clamp01(baseline), innerRect.height);
            GUI.DrawTexture(baselineRect, BaselineBarTex);
        }

        // Current certainty on top
        if (pawn.ideo.Certainty > 0f)
        {
            var fillRect = new Rect(innerRect.x, innerRect.y, innerRect.width * Mathf.Clamp01(pawn.ideo.Certainty), innerRect.height);
            GUI.DrawTexture(fillRect, SocialCardUtility.BarFullTexHor);
        }

        return false;
    }
}
