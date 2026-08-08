namespace EnhancedBeliefs;

[HotSwappable]
internal sealed class ITab_Opinion : ITab
{
    private const float HeightForAtMostIdeoCount = 10f;
    private const float Padding = 4f;
    private const float BarWidth = 200f;
    private const float StrengthBarWidth = 90f;
    private const float IconSize = 32f;
    private const float IssueIconSize = 24f;
    private const float RowHeight = IconSize + (2 * Padding);
    private const float IconTextGap = 2 * Padding;
    private const float ColumnGap = 4 * Padding;

    // A stance the pawn no longer shares with their own ideoligion is flagged in red.
    private static readonly Color DisagreeColor = new(0.85f, 0.35f, 0.35f);

    private static Vector2 scroll;

    public ITab_Opinion()
    {
        labelKey = "EnhancedBeliefs.TabOpinion";
    }

    protected override void FillTab()
    {
        var comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
        var data = comp.PawnTracker.EnsurePawnHasIdeoTracker(SelPawn);

        // Left column: the pawn's own convictions (Moral stances), strongest first. Right column: how they
        // feel about every ideology. The two share one vertical scroll, sized to whichever list is longer.
        var stances = StanceRows(data);
        var ideos = Find.IdeoManager.IdeosListForReading;

        var issueWidth = stances.Select(row => Text.CalcSize(row.issue.LabelCap).x).DefaultIfEmpty(0f).Max();
        var rungWidth = stances.Select(row => Text.CalcSize(row.personalRung).x).DefaultIfEmpty(0f).Max();
        var leftWidth = Padding + IssueIconSize + Padding + issueWidth + IconTextGap + rungWidth + IconTextGap + StrengthBarWidth + Padding;

        var nameWidth = ideos.Select(ideo => Text.CalcSize(ideo.name).x).DefaultIfEmpty(0f).Max();
        var rightWidth = Padding + IconSize + IconTextGap + nameWidth + (2 * Padding) + BarWidth;

        var rowCount = Math.Max(stances.Count, ideos.Count);
        var width = leftWidth + ColumnGap + rightWidth + (2 * Padding) + GenUI.ScrollBarWidth;
        var height = (Math.Min(rowCount, HeightForAtMostIdeoCount) * RowHeight) + Text.LineHeight + (2 * Padding);
        size = new Vector2(width, height);

        var tabContentRect = new Rect(0f, 0f, width, height).ContractedBy(Padding);

        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(new Rect(2 * Padding, 0f, leftWidth, Text.LineHeight + Padding), "EnhancedBeliefs.BeliefsHeader".Translate());
        Widgets.Label(new Rect(leftWidth + ColumnGap, 0f, rightWidth, Text.LineHeight + Padding), "EnhancedBeliefs.IdeologyOpinions".Translate());
        Text.Anchor = TextAnchor.UpperLeft;

        tabContentRect.yMin += Text.LineHeight;
        Widgets.BeginGroup(tabContentRect);

        var viewRect = new Rect()
        {
            width = tabContentRect.width - GenUI.ScrollBarWidth - Padding,
            height = rowCount * RowHeight,
        };
        Widgets.BeginScrollView(tabContentRect.AtZero(), ref scroll, viewRect, true);

        DrawStanceColumn(stances, issueWidth, rungWidth);
        DrawIdeoColumn(data, ideos, leftWidth + ColumnGap, nameWidth, width);

        Widgets.EndScrollView();
        Widgets.EndGroup();
    }

    private void DrawStanceColumn(
        List<(IssueDef issue, string personalRung, float personalRank, string ideoRung, float ideoRank, float strength)> stances,
        float issueWidth, float rungWidth)
    {
        var pos = Padding;
        foreach (var (issue, personalRung, personalRank, ideoRung, ideoRank, strength) in stances)
        {
            if (issue.Icon != null)
            {
                var iconRect = new Rect(Padding, pos + ((IconSize - IssueIconSize) / 2), IssueIconSize, IssueIconSize);
                GUI.DrawTexture(iconRect, issue.Icon);
            }

            var labelX = Padding + IssueIconSize + Padding;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(labelX, pos, issueWidth, IconSize), issue.LabelCap);

            // The stance is red when the pawn has drifted off the rung their ideoligion preaches.
            var rungX = labelX + issueWidth + IconTextGap;
            GUI.color = Mathf.RoundToInt(personalRank) == Mathf.RoundToInt(ideoRank) ? Color.white : DisagreeColor;
            Widgets.Label(new Rect(rungX, pos, rungWidth, IconSize), personalRung);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            var barRect = new Rect(rungX + rungWidth + IconTextGap, pos, StrengthBarWidth, IconSize).ContractedBy(Padding);
            _ = Widgets.FillableBar(barRect, strength / IdeoTrackerData.AbsoluteMaxConvictionStrength, SocialCardUtility.BarFullTexHor);

            var rowRect = new Rect(Padding, pos, rungX + rungWidth + IconTextGap + StrengthBarWidth, IconSize);
            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
                TooltipHandler.TipRegion(rowRect, "EnhancedBeliefs.StanceTooltip".Translate(
                    SelPawn.Named("PAWN"), issue.LabelCap, personalRung,
                    (strength / IdeoTrackerData.MaxConvictionStrength).ToStringPercent(), ideoRung));
            }

            pos += RowHeight;
        }
    }

    private void DrawIdeoColumn(IdeoTrackerData data, List<Ideo> ideos, float xOffset, float nameWidth, float width)
    {
        var pos = Padding;
        foreach (var (ideo, opinion) in ideos
            .Select(ideo => (ideo, opinion: data.IdeoOpinion(ideo)))
            .OrderByDescending(entry => entry.ideo == SelPawn.Ideo)
            .ThenByDescending(entry => entry.opinion))
        {
            var iconRect = new Rect(xOffset + Padding, pos, IconSize, IconSize);
            ideo.DrawIcon(iconRect);

            Text.Anchor = TextAnchor.MiddleLeft;
            var textRect = new Rect(iconRect.xMax + IconTextGap, pos, nameWidth, IconSize);
            Widgets.Label(textRect, ideo.name);
            Text.Anchor = TextAnchor.UpperLeft;

            var barRect = new Rect(textRect.xMax + (2 * Padding), pos, BarWidth, IconSize);
            _ = Widgets.FillableBar(barRect.ContractedBy(Padding), opinion, SocialCardUtility.BarFullTexHor);

            var tooltipRect = new Rect(xOffset + Padding, pos, barRect.xMax - xOffset - Padding, IconSize);
            if (Widgets.ButtonInvisible(tooltipRect))
            {
                IdeoUIUtility.OpenIdeoInfo(ideo);
            }
            if (Mouse.IsOver(tooltipRect))
            {
                Widgets.DrawHighlight(tooltipRect);

                var opinionRundown = data.DetailedIdeoOpinion(ideo);
                var tip = "EnhancedBeliefs.PawnOpinionTooltip".Translate(SelPawn.Named("PAWN"), ideo.Named("IDEO"), opinion.ToStringPercent()) + "\n\n";
                tip += "EnhancedBeliefs.PawnOptionToolTip.FromMemesAndPrecepts".Translate(opinionRundown.BaseOpinion.ToStringPercent()) + "\n";
                tip += "EnhancedBeliefs.PawnOptionToolTip.FromPersonalBeliefs".Translate(opinionRundown.PersonalOpinion.ToStringPercent()) + "\n";
                tip += "EnhancedBeliefs.PawnOptionToolTip.FromInterpersonalRelationships".Translate(opinionRundown.RelationshipOpinion.ToStringPercent()) + "\n";

                if (Prefs.DevMode)
                {
                    tip += "\n== Dev Mode details ==";
                    tip += "\n" + opinionRundown.DevModeDetails;
                }

                TooltipHandler.TipRegion(tooltipRect, tip);
            }

            if (ideo == SelPawn.Ideo)
            {
                Widgets.DrawLineHorizontal(xOffset, pos + RowHeight - (Padding / 2), width - xOffset);
                pos += Padding;
            }

            pos += RowHeight;
        }
    }

    // The pawn's Moral convictions their own ideo takes a stance on, strongest first: the issue, the rung they
    // personally prefer (which debates/books can drag off their ideo's rung) and its rank, the rung their
    // ideoligion preaches and its rank (for the drift colouring and tooltip), and conviction strength.
    private List<(IssueDef issue, string personalRung, float personalRank, string ideoRung, float ideoRank, float strength)> StanceRows(IdeoTrackerData data)
    {
        var stances = data.IssueStances().ToDictionary(entry => entry.issue, entry => (entry.rank, entry.strength));
        var rows = new List<(IssueDef issue, string personalRung, float personalRank, string ideoRung, float ideoRank, float strength)>();

        foreach (var group in SelPawn.Ideo.precepts.Where(precept => precept.def.issue != null).GroupBy(precept => precept.def.issue))
        {
            var issue = group.Key;
            var category = PreceptPolicy.CategoryOf(issue);
            if ((category != PreceptCategory.Moral && category != PreceptCategory.Special)
                || !stances.TryGetValue(issue, out var stance))
            {
                continue;
            }

            var ideoDef = group.First().def;
            rows.Add((issue, RungLabel(issue, stance.rank), stance.rank,
                ideoDef.LabelCap, PreceptLadder.RankOf(ideoDef), stance.strength));
        }

        return [.. rows.OrderByDescending(row => row.strength)];
    }

    private static string RungLabel(IssueDef issue, float rank)
    {
        var rungs = PreceptLadder.Rungs(issue);
        if (rank < 0f || rungs.Count == 0)
        {
            return "EnhancedBeliefs.StanceDontCare".Translate();
        }

        return rungs[Mathf.Clamp(Mathf.RoundToInt(rank), 0, rungs.Count - 1)].LabelCap;
    }

    // Only show tab for pawns with an ideology
    public override bool Hidden => SelPawn?.Ideo is null;
    public override bool IsVisible => SelPawn?.Ideo is not null;
}
