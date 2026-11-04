namespace EnhancedIdeology.HarmonyPatches;

// When the Ideology tab is turned off, the social card's role row gets an "Ideology..." button left of vanilla's
// "Choose role..." button. It opens Window_OpinionCard docked to the right of the card.
internal static class SocialCardUtility_OpinionButton
{
    // Vanilla puts "Choose role..." at [width - 150, width - 30] of the role row.
    private const float RoleButtonLeftInset = 150f;
    private const float ButtonGap = 4f;
    private const float ButtonHeight = 28f;
    private const float ButtonLabelPadding = 20f;

    // Vanilla DrawPawnRole sizes its tooltip region to end at (width - 115) when the pawn has no role. The role row
    // gives up this much more than the button width, so that region ends left of the button.
    private const float RoleTooltipOverlap = RoleButtonLeftInset + ButtonGap - 115f;

    internal static bool Shown(Pawn pawn) => !EnhancedIdeologyMod.Settings.ShowOpinionTab && OpinionCard.CanShow(pawn);

    internal static float ReservedWidth => ButtonWidth + RoleTooltipOverlap;

    private static float ButtonWidth
    {
        get
        {
            Text.Font = GameFont.Small;
            return Text.CalcSize("EnhancedIdeology.OpenOpinionCard".Translate()).x + ButtonLabelPadding;
        }
    }

    // rect is the role row in the social card group, which starts at the card's top-left corner.
    internal static void Draw(Pawn pawn, Rect rect)
    {
        var width = ButtonWidth;
        var buttonRect = new Rect(rect.width - RoleButtonLeftInset - ButtonGap - width, rect.y + (rect.height / 2f) - 14f, width, ButtonHeight);
        if (Widgets.ButtonText(buttonRect, "EnhancedIdeology.OpenOpinionCard".Translate()))
        {
            Window_OpinionCard.Toggle(pawn, UI.GUIToScreenPoint(new Vector2(rect.width, 0f)));
        }
    }
}

// Narrows the role row while the button is shown, so vanilla's role tooltip and highlight stop at the button. The
// divider line is drawn here at full width instead. Only the social card passes drawLine = true; the role change
// ritual dialog passes false and is left alone.
[HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawPawnRole))]
[HotSwappable]
internal static class SocialCardUtility_DrawPawnRole_MakeRoom
{
    private static void Prefix(Pawn pawn, ref Rect rect, ref bool drawLine, out float? __state)
    {
        __state = null;
        if (!drawLine || !SocialCardUtility_OpinionButton.Shown(pawn))
        {
            return;
        }
        __state = rect.width;
        rect.width -= SocialCardUtility_OpinionButton.ReservedWidth;
        drawLine = false;
    }

    private static void Postfix(Rect rect, float? __state)
    {
        if (__state is not float fullWidth)
        {
            return;
        }
        GUI.color = new Color(1f, 1f, 1f, 0.5f);
        Widgets.DrawLineHorizontal(0f, rect.yMax, fullWidth);
        GUI.color = Color.white;
    }
}

[HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawPawnRoleSelection))]
[HotSwappable]
internal static class SocialCardUtility_DrawPawnRoleSelection_OpinionButton
{
    private static void Postfix(Pawn pawn, Rect rect)
    {
        if (SocialCardUtility_OpinionButton.Shown(pawn))
        {
            SocialCardUtility_OpinionButton.Draw(pawn, rect);
        }
    }
}
