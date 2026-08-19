namespace EnhancedIdeology.HarmonyPatches;

[HarmonyPatch(typeof(BookUIUtility), "DrawBenefits")]
internal static class BookUIUtility_DrawBenefits_Reroute
{
    private const float IdeoIconSize = 32f;
    private const float IdeoRowHeight = IdeoIconSize + 4f;
    private const float IssueIconSize = 24f;
    private const float IssueRowHeight = 35f;
    private const float IconTextGap = 4f;
    private const float Indent = IssueIconSize + IconTextGap;
    private const float SectionGap = 8f;
    private const float MaxVisibleIssueRows = 9f;

    private static Vector2 beliefScroll;

    // Skip vanilla Benefits section for ideo books; postfix draws our replacement.
    static bool Prefix(Book book) =>
        book.BookComp.Doers.OfType<ReadingOutcomeDoer_CertaintyChange>().FirstOrDefault()?.ideo == null;

    static void Postfix(Rect rect, ref float y, Book book)
    {
        var doer = book.BookComp.Doers.OfType<ReadingOutcomeDoer_CertaintyChange>().FirstOrDefault();
        if (doer?.ideo == null) return;

        var stances = doer.IdeoStances().ToList();
        if (stances.Count == 0) return;

        var gainPerQuadrum = doer.CertaintyGain() * ReadingOutcomeDoer_CertaintyChange.TypicalReadingTicksPerQuadrum;

        y += SectionGap;

        var separatorRect = new Rect(rect.x, y, rect.width, rect.height);
        GUI.BeginGroup(separatorRect);
        float localY = 0f;
        Widgets.ListSeparator(ref localY, separatorRect.width, "EnhancedIdeology.BookBeliefsHeader".Translate());
        GUI.EndGroup();
        y += localY + SectionGap;

        var ideoIconRect = new Rect(rect.x, y + ((IdeoRowHeight - IdeoIconSize) / 2f), IdeoIconSize, IdeoIconSize);
        doer.ideo.DrawIcon(ideoIconRect);
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(new Rect(rect.x + IdeoIconSize + IconTextGap, y, rect.width - IdeoIconSize - IconTextGap, IdeoRowHeight), doer.ideo.name);
        Text.Anchor = TextAnchor.UpperLeft;
        y += IdeoRowHeight;

        var contentHeight = stances.Count * IssueRowHeight;
        var viewHeight = Math.Min(contentHeight, MaxVisibleIssueRows * IssueRowHeight);
        var outerRect = new Rect(rect.x, y, rect.width, viewHeight);
        var viewRect = new Rect(0f, 0f, rect.width - GenUI.ScrollBarWidth, contentHeight);

        Widgets.BeginScrollView(outerRect, ref beliefScroll, viewRect);
        float rowY = 0f;
        foreach (var (issue, stance, strength) in stances)
        {
            var shiftPerQuadrum = "EnhancedIdeology.ShiftRatePerQuadrum".Translate((gainPerQuadrum * strength / IdeoTrackerData.MaxConvictionStrength).ToString("0.##", CultureInfo.InvariantCulture));
            if (issue.Icon != null)
            {
                var iconRect = new Rect(Indent, rowY + ((IssueRowHeight - IssueIconSize) / 2f), IssueIconSize, IssueIconSize);
                GUI.DrawTexture(iconRect, issue.Icon);
            }

            var textX = Indent + IssueIconSize + IconTextGap;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(textX, rowY, viewRect.width - textX, IssueRowHeight), $"{issue.LabelCap}: {stance.LabelCap} ({shiftPerQuadrum})");
            Text.Anchor = TextAnchor.UpperLeft;

            rowY += IssueRowHeight;
        }
        Widgets.EndScrollView();

        y += viewHeight + SectionGap;
    }
}
