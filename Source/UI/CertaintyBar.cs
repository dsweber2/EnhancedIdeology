namespace EnhancedIdeology;

// Shared rendering for a certainty bar's markers, drawn the way RimWorld's Need bars are: a crisis-of-faith
// threshold tick and the target-certainty marker. Used by the social card certainty bar and the opinion tab's
// own-ideo row, which both show the pawn's certainty drifting toward a setpoint.
[StaticConstructorOnStartup]
internal static class CertaintyBar
{
    private static readonly Texture2D TargetMarkerTex = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarker");

    // Vertical room a bar must give up below itself so the target marker fits without overflowing its row.
    public const float MarkerHeight = 12f;

    // Crisis-of-faith threshold (Need.DrawBarThreshold): a thin tick in the bar's lower half, black once
    // certainty has dropped past it, grey while it is still above.
    public static void DrawThreshold(Rect barRect, float threshPct, float curLevel)
    {
        var width = barRect.width > 60f ? 2f : 1f;
        var position = new Rect(
            barRect.x + (barRect.width * threshPct) - (width - 1f), barRect.y + (barRect.height / 2f), width, barRect.height / 2f);
        if (threshPct < curLevel)
        {
            GUI.color = new Color(1f, 1f, 1f, 0.9f);
            GUI.DrawTexture(position, BaseContent.BlackTex);
        }
        else
        {
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            GUI.DrawTexture(position, BaseContent.GreyTex);
        }
        GUI.color = Color.white;
    }

    // Target certainty (Need.DrawBarInstantMarkerAt): the instant-level marker, drawn unrotated just below the bar.
    public static void DrawTargetMarker(Rect barRect, float pct)
    {
        var size = barRect.width < 150f ? MarkerHeight / 2f : MarkerHeight;
        var x = barRect.x + (barRect.width * pct);
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(x - (size / 2f), barRect.y + barRect.height, size, size), TargetMarkerTex);
    }
}
