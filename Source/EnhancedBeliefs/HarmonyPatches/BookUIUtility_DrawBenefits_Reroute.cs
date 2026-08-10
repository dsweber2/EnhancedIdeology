namespace EnhancedBeliefs.HarmonyPatches;

[HarmonyPatch(typeof(BookUIUtility), "DrawBenefits")]
internal static class BookUIUtility_DrawBenefits_Reroute
{
    private const float IdeoIconSize = 32f;
    private const float IdeoRowHeight = IdeoIconSize + 4f;
    private const float IssueIconSize = 24f;
    private const float IssueRowHeight = IssueIconSize + 4f;
    private const float IconTextGap = 4f;
    private const float Indent = IssueIconSize + IconTextGap;
    private const float SectionGap = 8f;

    static void Postfix(Rect rect, ref float y, Book book)
    {
        var doer = book.BookComp.Doers.OfType<ReadingOutcomeDoer_CertaintyChange>().FirstOrDefault();
        if (doer?.ideo == null) return;

        var stances = doer.IdeoStances().ToList();
        if (stances.Count == 0) return;

        y += SectionGap;

        var separatorRect = new Rect(rect.x, y, rect.width, rect.height);
        GUI.BeginGroup(separatorRect);
        float localY = 0f;
        Widgets.ListSeparator(ref localY, separatorRect.width, "EnhancedBeliefs.BookBeliefsHeader".Translate());
        GUI.EndGroup();
        y += localY + SectionGap;

        // Ideo header row: icon + name
        var ideoIconRect = new Rect(rect.x, y + ((IdeoRowHeight - IdeoIconSize) / 2f), IdeoIconSize, IdeoIconSize);
        doer.ideo.DrawIcon(ideoIconRect);
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(new Rect(rect.x + IdeoIconSize + IconTextGap, y, rect.width - IdeoIconSize - IconTextGap, IdeoRowHeight), doer.ideo.name);
        Text.Anchor = TextAnchor.UpperLeft;
        y += IdeoRowHeight;

        // Per-issue stance rows, indented under the ideo
        foreach (var (issue, stance, strength) in stances)
        {
            var pct = (strength / IdeoTrackerData.MaxConvictionStrength).ToStringPercent();

            if (issue.Icon != null)
            {
                var iconRect = new Rect(rect.x + Indent, y + ((IssueRowHeight - IssueIconSize) / 2f), IssueIconSize, IssueIconSize);
                GUI.DrawTexture(iconRect, issue.Icon);
            }

            var textX = rect.x + Indent + IssueIconSize + IconTextGap;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(textX, y, rect.xMax - textX, IssueRowHeight), $"{issue.LabelCap}: {stance.LabelCap} ({pct})");
            Text.Anchor = TextAnchor.UpperLeft;

            y += IssueRowHeight;
        }

        y += SectionGap;
    }
}
