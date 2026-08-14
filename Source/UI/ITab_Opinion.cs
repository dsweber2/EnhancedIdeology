namespace EnhancedIdeology;

[HotSwappable]
internal sealed class ITab_Opinion : ITab
{
    private const float HeightForAtMostIdeoCount = 10f;
    private const float Padding = 4f;
    private const float SmallPadding = 1f;
    private const float BarWidth = 200f;
    private const float OpinionBarWidth = 130f;
    private const float IconSize = 32f;
    private const float IssueIconSize = 24f;
    private const float RowHeight = IconSize + (2 * Padding);
    private const float IdeoRowHeight = RowHeight;
    private const float IconTextGap = 2 * Padding;
    private const float ColumnGap = 2 * Padding;

    // The left column grades each stance against a selected ideoligion: green where the pawn agrees with what
    // it preaches on that issue, red where they clash. Default selection is the pawn's own faith.
    private static readonly Color AgreeColor = new(0.35f, 0.7f, 0.35f);
    private static readonly Color DisagreeColor = new(0.85f, 0.35f, 0.35f);

    private static Vector2 scroll;

    // Which ideoligion the left column compares against; null (or an ideo no longer present) means the pawn's own.
    private static Ideo? selectedIdeo;

    // The pawn whose opinions were last recomputed for display, so we refresh once per pawn shown rather than
    // every frame. Cleared on open so reopening the same pawn's tab also refreshes.
    private static Pawn? recachedPawn;

    public ITab_Opinion()
    {
        labelKey = "EnhancedIdeology.TabOpinion";
    }

    public override void OnOpen()
    {
        base.OnOpen();
        recachedPawn = null;
    }

    protected override void FillTab()
    {
        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var data = comp.PawnTracker.EnsurePawnHasIdeoTracker(SelPawn);

        // The cached structural opinions and certainty setpoint are refreshed on a tick, so they can be stale
        // when the tab is shown. Recompute once per pawn shown (cheap) so the numbers and target arrow are honest.
        if (SelPawn != recachedPawn)
        {
            recachedPawn = SelPawn;
            data.RecacheAllBaseOpinions();
            data.CertaintyChangeRecache(comp);
        }

        var ideos = Find.IdeoManager.IdeosListForReading;
        var selected = selectedIdeo != null && ideos.Contains(selectedIdeo) ? selectedIdeo : SelPawn.Ideo;

        // Left column: the pawn's Moral stances, and how each agrees or clashes with the selected ideoligion.
        // Right column: overall opinion of every ideology (click a row to re-aim the left column at it). The two
        // share one vertical scroll, sized to whichever list is longer.
        var stances = StanceRows(data, selected);

        var issueWidth = stances.Select(row => Text.CalcSize(row.issue.LabelCap).x).DefaultIfEmpty(0f).Max();
        var rungWidth = stances.Select(row => Text.CalcSize(row.personalRung).x).DefaultIfEmpty(0f).Max();
        var leftWidth = SmallPadding + IssueIconSize + SmallPadding + issueWidth + IconTextGap + rungWidth + IconTextGap + OpinionBarWidth + SmallPadding;

        var nameWidth = ideos.Select(ideo => Text.CalcSize(ideo.name).x).DefaultIfEmpty(0f).Max();
        var rightWidth = Padding + IconSize + IconTextGap + nameWidth + Padding + BarWidth;

        // Each column advances at its own row height, so the scroll region is sized to whichever runs taller.
        var contentHeight = Math.Max(stances.Count * RowHeight, ideos.Count * IdeoRowHeight);
        var width = leftWidth + ColumnGap + rightWidth + (2 * Padding) + GenUI.ScrollBarWidth;
        var height = Math.Min(contentHeight, HeightForAtMostIdeoCount * IdeoRowHeight) + Text.LineHeight + (2 * Padding);
        size = new Vector2(width, height);

        var tabContentRect = new Rect(0f, 0f, width, height).ContractedBy(Padding);

        var headerH = Text.LineHeight + Padding;
        Text.Anchor = TextAnchor.MiddleLeft;

        // Three column labels in place of a single header for the stance side.
        var issueColX = SmallPadding + IssueIconSize + SmallPadding;
        Widgets.Label(new Rect(issueColX, 0f, issueWidth, headerH), "EnhancedIdeology.ColIssue".Translate());
        var rungColX = issueColX + issueWidth + IconTextGap;
        Widgets.Label(new Rect(rungColX, 0f, rungWidth, headerH), "EnhancedIdeology.ColStance".Translate());
        var strengthColX = rungColX + rungWidth + IconTextGap;
        Widgets.Label(new Rect(strengthColX, 0f, OpinionBarWidth, headerH), "EnhancedIdeology.ColStrength".Translate());

        // Right column header, with the selected ideo name appended when it differs from the pawn's own.
        var rightHeader = "EnhancedIdeology.IdeologyOpinions".Translate().ToString();
        if (selected != SelPawn.Ideo)
        {
            rightHeader += $" ({selected.name})";
        }
        Widgets.Label(new Rect(leftWidth + ColumnGap, 0f, rightWidth, headerH), rightHeader);
        Text.Anchor = TextAnchor.UpperLeft;

        tabContentRect.yMin += Text.LineHeight;
        Widgets.BeginGroup(tabContentRect);

        var viewRect = new Rect()
        {
            width = tabContentRect.width - GenUI.ScrollBarWidth,
            height = contentHeight,
        };
        Widgets.BeginScrollView(tabContentRect.AtZero(), ref scroll, viewRect, true);

        DrawStanceColumn(data, stances, selected, issueWidth, rungWidth);
        DrawIdeoColumn(data, ideos, selected, leftWidth + ColumnGap, nameWidth, width);

        Widgets.EndScrollView();
        Widgets.EndGroup();
    }

    private void DrawStanceColumn(
        IdeoTrackerData data,
        List<(IssueDef issue, string personalRung, float opinion, string selectedRung, float strength)> stances,
        Ideo selected, float issueWidth, float rungWidth)
    {
        // Structural fit is the mean of the per-issue opinions, so each issue's real contribution is its opinion
        // divided by the precept count - what actually moves the overall number, not the raw per-issue swing.
        var count = Math.Max(stances.Count, 1);
        var pos = Padding;
        foreach (var (issue, personalRung, opinion, selectedRung, strength) in stances)
        {
            var contribution = opinion / count;
            if (issue.Icon != null)
            {
                var iconRect = new Rect(Padding, pos + ((IconSize - IssueIconSize) / 2), IssueIconSize, IssueIconSize);
                GUI.DrawTexture(iconRect, issue.Icon);
            }

            var labelX = Padding + IssueIconSize + SmallPadding;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(labelX, pos, issueWidth, IconSize), issue.LabelCap);

            // The pawn's own rung, coloured by whether that stance agrees (green) or clashes (red) with the
            // selected ideoligion. Aimed at their own faith, red is exactly the drift a debate or book induced.
            var rungX = labelX + issueWidth + IconTextGap;
            GUI.color = AgreementColor(opinion);
            Widgets.Label(new Rect(rungX, pos, rungWidth, IconSize), personalRung);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            // This issue's contribution to the pawn's structural fit with the selected ideoligion, coloured.
            var opinionRect = new Rect(rungX + rungWidth + IconTextGap, pos, OpinionBarWidth, IconSize);
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = AgreementColor(opinion);
            Widgets.Label(opinionRect, $"{strength:F1} ({Signed(contribution)})");
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            var rowRect = new Rect(Padding, pos, rungX + rungWidth + IconTextGap + OpinionBarWidth, IconSize);
            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
                var tip = "EnhancedIdeology.StanceTooltip".Translate(
                    SelPawn.Named("PAWN"), issue.LabelCap, personalRung,
                    $"{strength:F1} ({(strength / IdeoTrackerData.MaxConvictionStrength).ToStringPercent()})",
                    selected.Named("IDEO"), selectedRung, Signed(contribution)).ToString();
                if (Prefs.DevMode)
                {
                    tip += $"\n[dev] vs {selected.name}: raw={opinion:F2}  " + data.IssueOpinionDebug(selected, issue);
                }
                TooltipHandler.TipRegion(rowRect, tip);
            }

            pos += RowHeight;
        }
    }

    private void DrawIdeoColumn(IdeoTrackerData data, List<Ideo> ideos, Ideo selected, float xOffset, float nameWidth, float width)
    {
        var pos = Padding;
        foreach (var (ideo, opinion) in ideos
            .Select(ideo => (ideo, opinion: data.IdeoOpinion(ideo)))
            .OrderByDescending(entry => entry.ideo == SelPawn.Ideo)
            .ThenByDescending(entry => entry.opinion))
        {
            // Content sits centred in the taller ideoligion row.
            var rowY = pos + ((IdeoRowHeight - IconSize) / 2f);
            var iconRect = new Rect(xOffset + Padding, rowY, IconSize, IconSize);

            var textRect = new Rect(iconRect.xMax + IconTextGap, rowY, nameWidth, IconSize);
            var barRect = new Rect(textRect.xMax + (2 * Padding), rowY, BarWidth, IconSize);

            // Everything right of the icon selects this ideoligion as the left column's comparison target; the
            // icon itself still opens the ideoligion page.
            var selectRect = new Rect(iconRect.xMax, rowY, barRect.xMax - iconRect.xMax, IconSize);
            if (ideo == selected)
            {
                Widgets.DrawHighlightSelected(selectRect);
            }

            ideo.DrawIcon(iconRect);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(textRect, ideo.name);
            Text.Anchor = TextAnchor.UpperLeft;

            // Every bar reserves the marker strip and carries the crisis-of-faith tick (grey while the fill is
            // still above it, black once it has dropped past), so the rows read uniformly. Only the pawn's own
            // row - which is their certainty - also gets the drift-target marker.
            var certaintyBar = new Rect(barRect.x, barRect.y, barRect.width, barRect.height - CertaintyBar.MarkerHeight).ContractedBy(Padding);
            var filled = Widgets.FillableBar(certaintyBar, opinion, Widgets.BarFullTexHor);
            CertaintyBar.DrawThreshold(filled, EnhancedIdeologyMod.Settings.CrisisThreshold, opinion);
            if (ideo == SelPawn.Ideo)
            {
                CertaintyBar.DrawTargetMarker(filled, Mathf.Clamp01(data.CachedTargetCertainty));
            }

            if (Widgets.ButtonInvisible(iconRect))
            {
                IdeoUIUtility.OpenIdeoInfo(ideo);
            }
            if (Widgets.ButtonInvisible(selectRect))
            {
                selectedIdeo = ideo;
            }
            if (Mouse.IsOver(selectRect))
            {
                Widgets.DrawHighlight(selectRect);

                var opinionRundown = data.DetailedIdeoOpinion(ideo);
                var tip = "EnhancedIdeology.PawnOpinionTooltip".Translate(SelPawn.Named("PAWN"), ideo.Named("IDEO"), opinion.ToStringPercent()) + "\n\n";
                tip += "EnhancedIdeology.PawnOptionToolTip.FromMemesAndPrecepts".Translate(opinionRundown.BaseOpinion.ToStringPercent()) + "\n";
                tip += "EnhancedIdeology.PawnOptionToolTip.FromPersonalBeliefs".Translate(opinionRundown.PersonalOpinion.ToStringPercent()) + "\n";
                tip += "EnhancedIdeology.PawnOptionToolTip.FromInterpersonalRelationships".Translate(opinionRundown.RelationshipOpinion.ToStringPercent()) + "\n";

                if (Prefs.DevMode)
                {
                    tip += "\n== Dev Mode details ==";
                    tip += "\n" + opinionRundown.DevModeDetails;
                }

                TooltipHandler.TipRegion(selectRect, tip);
            }

            if (ideo == SelPawn.Ideo)
            {
                Widgets.DrawLineHorizontal(xOffset, pos + IdeoRowHeight - (Padding / 2), width - xOffset);
                pos += Padding;
            }

            pos += IdeoRowHeight;
        }
    }

    private static Color AgreementColor(float opinion) =>
        opinion > 0.01f ? AgreeColor : opinion < -0.01f ? DisagreeColor : Color.white;

    // The opinion as a signed percentage of the full-conviction mark, e.g. "+80%" / "-40%".
    private static string Signed(float opinion)
    {
        var frac = opinion / IdeoTrackerData.MaxConvictionStrength;
        return (frac >= 0f ? "+" : "") + frac.ToStringPercent();
    }

    // Every issue either the pawn's own faith or the selected ideoligion takes a stance on, strongest first: the
    // issue, the rung the pawn prefers (Don't-care where their faith is silent), their signed opinion of the
    // selected ideoligion's stance on it (for the % and colouring), the rung that ideoligion preaches (for the
    // tooltip), and conviction strength (for ordering). Including the selected ideo's own issues is what surfaces
    // disagreement: on a rival's strong stance the pawn is often silent (neutral), which reads as opposition.
    private List<(IssueDef issue, string personalRung, float opinion, string selectedRung, float strength)> StanceRows(IdeoTrackerData data, Ideo selected)
    {
        var stances = data.IssueStances().ToDictionary(entry => entry.issue, entry => (entry.rank, entry.strength));
        var rows = new List<(IssueDef issue, string personalRung, float opinion, string selectedRung, float strength)>();

        var issues = SelPawn.Ideo.precepts.Concat(selected.precepts)
            .Where(precept => precept.def.issue != null)
            .Select(precept => precept.def.issue!)
            .Distinct();

        foreach (var issue in issues)
        {
            var category = PreceptPolicy.CategoryOf(issue);
            if ((category != PreceptCategory.Moral && category != PreceptCategory.Special)
                || !stances.TryGetValue(issue, out var stance))
            {
                continue;
            }

            rows.Add((issue, RungLabel(issue, stance.rank), data.IssueOpinionToward(selected, issue),
                SelectedRungLabel(selected, issue), stance.strength));
        }

        return [.. rows.OrderByDescending(row => row.strength)];
    }

    // The rung the selected ideoligion preaches on the issue, or Don't-care if it takes no explicit stance.
    private static string SelectedRungLabel(Ideo ideo, IssueDef issue)
    {
        var held = ideo.precepts.FirstOrDefault(precept => precept.def.issue == issue);
        return held != null ? held.def.LabelCap : "EnhancedIdeology.StanceDontCare".Translate();
    }

    private static string RungLabel(IssueDef issue, float rank)
    {
        var rungs = PreceptLadder.Rungs(issue);
        if (rank < 0f || rungs.Count == 0)
        {
            return "EnhancedIdeology.StanceDontCare".Translate();
        }

        return rungs[Mathf.Clamp(Mathf.RoundToInt(rank), 0, rungs.Count - 1)].LabelCap;
    }

    // Only show tab for pawns with an ideology
    public override bool Hidden => SelPawn?.Ideo is null;
    public override bool IsVisible => SelPawn?.Ideo is not null;
}
