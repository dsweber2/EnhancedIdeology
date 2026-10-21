namespace EnhancedIdeology;

[HotSwappable]
internal sealed class OpinionCard
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
    private const float MaxRungWidth = 120f;
    private const float GutterWidth = 3f;
    // Strength changes below this are not marked with an arrow. The default daily drain (about 0.007 per day)
    // stays below it over the two-day window.
    private const float ArrowThreshold = 0.02f;
    // A strength change of at least this (three won debates) or a rung change of at least LargeRungSwing
    // gets a double arrow.
    private const float LargeStrengthSwing = 3f;
    private const float LargeRungSwing = 2f;
    // Offset of the second glyph of a double arrow, as a fraction of the glyph width, so the two glyphs overlap.
    private const float DoubleArrowOffset = 0.5f;

    // The left column grades each stance against a selected ideoligion: green where the pawn agrees with what
    // it preaches on that issue, red where they clash. Default selection is the pawn's own faith. The gutter
    // stripe at the left edge of a stance row uses the same colours to grade the stance against the pawn's traits.
    private static readonly Color AgreeColor = new(0.35f, 0.7f, 0.35f);
    private static readonly Color DisagreeColor = new(0.85f, 0.35f, 0.35f);

    private static Vector2 scroll;

    // Which ideoligion the left column compares against; null (or an ideo no longer present) means the pawn's own.
    private static Ideo? selectedIdeo;

    private enum StanceSort { ByStrength, ByContributionDesc, ByContributionAsc, ByIssueAsc, ByIssueDesc }
    private static StanceSort stanceSort = StanceSort.ByStrength;

    private readonly Pawn pawn;
    private readonly IdeoTrackerData data;
    private readonly List<Ideo> ideos;
    private readonly Ideo selected;
    private readonly List<(IssueDef issue, string personalRung, float opinion, string selectedRung, float strength)> stances;
    private readonly Dictionary<IssueDef, TraitStanceTarget> traitTargets;
    private readonly float glyphWidth;
    private readonly float rungArrowWidth;
    private readonly float issueWidth;
    private readonly float rungWidth;
    private readonly List<float> rowHeights;
    private readonly float leftWidth;
    private readonly float nameWidth;
    private readonly float rightWidth;
    private readonly float contentHeight;

    public Vector2 Size { get; }

    public OpinionCard(Pawn pawn)
    {
        this.pawn = pawn;
        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        data = comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        // Base opinions first: a recache of them marks the setpoint stale.
        data.RecacheBaseOpinionsIfStale();
        data.RecacheSetpointIfStale(comp);

        ideos = Find.IdeoManager.IdeosListForReading;
        selected = selectedIdeo != null && ideos.Contains(selectedIdeo) ? selectedIdeo : pawn.Ideo;

        // Left column: the pawn's Moral stances, and how each agrees or clashes with the selected ideoligion.
        // Right column: overall opinion of every ideology (click a row to re-aim the left column at it). The two
        // share one vertical scroll, sized to whichever list is longer.
        stances = StanceRows(data, selected);
        traitTargets = data.Stances.TraitTargets();

        glyphWidth = new[] { "▲", "▼", "◀", "▶" }.Max(glyph => Text.CalcSize(glyph).x);
        rungArrowWidth = glyphWidth * (1f + DoubleArrowOffset);
        issueWidth = stances.Select(row => Text.CalcSize(row.issue.LabelCap).x).DefaultIfEmpty(0f).Max();
        rungWidth = Mathf.Min(MaxRungWidth, stances.Select(row => Text.CalcSize(row.personalRung).x).DefaultIfEmpty(0f).Max());
        rowHeights = stances.Select(row => Mathf.Max(RowHeight, Text.CalcHeight(row.personalRung, rungWidth) + 2 * Padding)).ToList();
        leftWidth = SmallPadding + IssueIconSize + SmallPadding + issueWidth + IconTextGap + rungArrowWidth + rungWidth + IconTextGap + glyphWidth + OpinionBarWidth + SmallPadding;

        nameWidth = ideos.Select(ideo => Text.CalcSize(ideo.name).x).DefaultIfEmpty(0f).Max();
        rightWidth = Padding + IconSize + IconTextGap + nameWidth + (2 * Padding) + BarWidth;

        // Each column advances at its own row height, so the scroll region is sized to whichever runs taller.
        contentHeight = Math.Max(rowHeights.Sum(), ideos.Count * IdeoRowHeight);
        var width = leftWidth + ColumnGap + rightWidth + (2 * Padding) + GenUI.ScrollBarWidth;
        var height = Math.Min(contentHeight, HeightForAtMostIdeoCount * IdeoRowHeight) + Text.LineHeight + (2 * Padding);
        Size = new Vector2(width, height);
    }

    // Draws the card at the origin of the current GUI group, filling Size.
    public void Draw()
    {
        var width = Size.x;
        var tabContentRect = new Rect(Vector2.zero, Size).ContractedBy(Padding);

        var headerH = Text.LineHeight + Padding;
        Text.Anchor = TextAnchor.MiddleLeft;

        // Issue and Strength headers are sortable; Stance is a plain label.
        var issueColX = SmallPadding + IssueIconSize + SmallPadding;
        var issueHeaderRect = new Rect(issueColX, 0f, issueWidth, headerH);
        if (Mouse.IsOver(issueHeaderRect))
            Widgets.DrawHighlight(issueHeaderRect);
        var issueHeaderLabel = "EnhancedIdeology.ColIssue".Translate().ToString();
        issueHeaderLabel += stanceSort switch { StanceSort.ByIssueAsc => " ▼", StanceSort.ByIssueDesc => " ▲", _ => "" };
        Widgets.Label(issueHeaderRect, issueHeaderLabel);
        if (Widgets.ButtonInvisible(issueHeaderRect))
            stanceSort = stanceSort switch { StanceSort.ByIssueAsc => StanceSort.ByIssueDesc, StanceSort.ByIssueDesc => StanceSort.ByStrength, _ => StanceSort.ByIssueAsc };

        var rungColX = issueColX + issueWidth + IconTextGap + rungArrowWidth;
        Widgets.Label(new Rect(rungColX, 0f, rungWidth, headerH), "EnhancedIdeology.ColStance".Translate());

        var strengthColX = rungColX + rungWidth + IconTextGap + glyphWidth;
        var strengthHeaderRect = new Rect(strengthColX, 0f, OpinionBarWidth, headerH);
        if (Mouse.IsOver(strengthHeaderRect))
            Widgets.DrawHighlight(strengthHeaderRect);
        var strengthHeaderLabel = "EnhancedIdeology.ColStrength".Translate().ToString();
        strengthHeaderLabel += stanceSort switch { StanceSort.ByContributionDesc => " ▼", StanceSort.ByContributionAsc => " ▲", _ => "" };
        Widgets.Label(strengthHeaderRect, strengthHeaderLabel);
        if (Widgets.ButtonInvisible(strengthHeaderRect))
            stanceSort = stanceSort switch { StanceSort.ByContributionDesc => StanceSort.ByContributionAsc, StanceSort.ByContributionAsc => StanceSort.ByStrength, _ => StanceSort.ByContributionDesc };

        // Right column header, with the selected ideo name appended when it differs from the pawn's own.
        var rightHeader = "EnhancedIdeology.IdeologyOpinions".Translate().ToString();
        if (selected != pawn.Ideo)
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

        DrawStanceColumn(data, stances, selected, issueWidth, rungWidth, rowHeights);
        DrawIdeoColumn(data, ideos, selected, leftWidth + ColumnGap, nameWidth, width);

        Widgets.EndScrollView();
        Widgets.EndGroup();
    }

    private void DrawStanceColumn(
        IdeoTrackerData data,
        List<(IssueDef issue, string personalRung, float opinion, string selectedRung, float strength)> stances,
        Ideo selected, float issueWidth, float rungWidth, List<float> rowHeights)
    {
        // Structural fit is the mean of the per-issue opinions, so each issue's real contribution is its opinion
        // divided by the precept count - what actually moves the overall number, not the raw per-issue swing.
        var count = Math.Max(stances.Count, 1);
        var pos = Padding;
        for (var ii = 0; ii < stances.Count; ii++)
        {
            var (issue, personalRung, opinion, selectedRung, strength) = stances[ii];
            var rowH = rowHeights[ii];
            var contribution = opinion / count;
            var rank = data.Stances.GetRank(issue);
            var before = data.Stances.StanceBefore(issue);
            traitTargets.TryGetValue(issue, out var traitTarget);

            if (traitTarget != null && TraitFitColor(traitTarget, rank) is { } fitColor)
            {
                Widgets.DrawBoxSolid(new Rect(0f, pos + SmallPadding, GutterWidth, rowH - 2 * SmallPadding), fitColor);
            }

            if (issue.Icon != null)
            {
                var iconRect = new Rect(Padding, pos + (rowH - IssueIconSize) / 2f, IssueIconSize, IssueIconSize);
                GUI.DrawTexture(iconRect, issue.Icon);
            }

            var labelX = Padding + IssueIconSize + SmallPadding;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(labelX, pos, issueWidth, rowH), issue.LabelCap);

            // A rung change over the past day, pointing along the ladder: left toward the permissive end, right
            // toward the forbidding end.
            var rungArrowX = labelX + issueWidth + IconTextGap;
            Text.Anchor = TextAnchor.MiddleLeft;
            var previousRung = before is { } previous ? RungLabel(issue, previous.rank, pawn.Ideo) : null;
            var rungChanged = previousRung != null && previousRung != personalRung;
            if (rungChanged)
            {
                var previousRank = before!.Value.rank;
                var arrow = rank > previousRank ? "▶" : "◀";
                var large = Mathf.Abs(rank - previousRank) >= LargeRungSwing;
                DrawArrow(new Rect(rungArrowX, pos, rungArrowWidth, rowH), arrow, large, stacked: false);
            }

            // The pawn's own rung, coloured by whether that stance agrees (green) or clashes (red) with the
            // selected ideoligion. Aimed at their own faith, red is exactly the drift a debate or book induced.
            var rungX = rungArrowX + rungArrowWidth;
            var rungTextH = Text.CalcHeight(personalRung, rungWidth);
            GUI.color = AgreementColor(opinion);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(new Rect(rungX, pos + (rowH - rungTextH) / 2f, rungWidth, rungTextH), personalRung);
            GUI.color = Color.white;

            // Whether the strength rose or fell over the past day, then this issue's contribution to the pawn's
            // structural fit with the selected ideoligion, coloured.
            var opinionRect = new Rect(rungX + rungWidth + IconTextGap, pos, glyphWidth + OpinionBarWidth, rowH);
            Text.Anchor = TextAnchor.MiddleLeft;
            var change = before is { } dayAgo ? strength - dayAgo.strength : 0f;
            var strengthChanged = Mathf.Abs(change) >= ArrowThreshold;
            if (strengthChanged)
            {
                var arrow = change > 0f ? "▲" : "▼";
                var large = Mathf.Abs(change) >= LargeStrengthSwing;
                DrawArrow(opinionRect.LeftPartPixels(glyphWidth), arrow, large, stacked: true);
            }
            GUI.color = AgreementColor(opinion);
            Widgets.Label(new Rect(opinionRect.x + glyphWidth, pos, OpinionBarWidth, rowH), $"{strength:F1} ({Signed(contribution)})");
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            var rowRect = new Rect(Padding, pos, opinionRect.xMax, rowH);
            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
                string tip = "EnhancedIdeology.StanceTooltip".Translate(
                    pawn.Named("PAWN"), issue.LabelCap, personalRung,
                    $"{strength:F1} ({(strength / ConvictionScale.MaxConvictionStrength).ToStringPercent()})",
                    selected.Named("IDEO"), selectedRung, Signed(contribution));
                if (rungChanged || strengthChanged)
                {
                    tip += "\n";
                    if (rungChanged)
                        tip += "\n" + "EnhancedIdeology.StanceChangedRung".Translate(pawn.Named("PAWN"), previousRung!, personalRung);
                    if (strengthChanged)
                        tip += "\n" + "EnhancedIdeology.StanceChangedStrength".Translate(pawn.Named("PAWN"), $"{strength - change:F1}", $"{strength:F1}");
                }
                if (traitTarget != null)
                {
                    tip += "\n\n" + TraitDriftText(issue, traitTarget, rank);
                }
                if (Prefs.DevMode)
                {
                    tip += $"\n[dev] vs {selected.name}: raw={opinion:F2}  " + data.IssueOpinionDebug(selected, issue);
                }
                TooltipHandler.TipRegion(rowRect, tip);
            }

            pos += rowH;
        }
    }

    private void DrawIdeoColumn(IdeoTrackerData data, List<Ideo> ideos, Ideo selected, float xOffset, float nameWidth, float width)
    {
        var pos = Padding;
        var ideoOpinions = ideos
            .Select(ideo => (ideo, opinion: ideo == pawn.Ideo ? data.ExtendedCertainty : data.IdeoOpinion(ideo)))
            .OrderByDescending(entry => entry.ideo == pawn.Ideo)
            .ThenByDescending(entry => entry.opinion)
            .ToList();

        // Scale bars to the highest opinion so overcertainty (>100%) is visible rather than clipped.
        var barMax = ideoOpinions.Count > 0
            ? Mathf.Max(1f, ideoOpinions.Max(ii => ii.opinion), data.CachedTargetCertainty)
            : 1f;

        foreach (var (ideo, opinion) in ideoOpinions)
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

            if (Mouse.IsOver(iconRect))
                Widgets.DrawHighlight(iconRect);
            ideo.DrawIcon(iconRect);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(textRect, ideo.name);
            Text.Anchor = TextAnchor.UpperLeft;

            // Every bar reserves the marker strip and carries the crisis-of-faith tick (grey while the fill is
            // still above it, black once it has dropped past), so the rows read uniformly. Only the pawn's own
            // row - which is their certainty - also gets the drift-target marker.
            var fill = opinion / barMax;
            var certaintyBar = new Rect(barRect.x, barRect.y, barRect.width, barRect.height - CertaintyBar.MarkerHeight).ContractedBy(Padding);
            var filled = Widgets.FillableBar(certaintyBar, fill, Widgets.BarFullTexHor);
            CertaintyBar.DrawThreshold(filled, data.EffectiveCrisisThreshold() / barMax, fill);
            if (ideo == pawn.Ideo)
            {
                CertaintyBar.DrawTargetMarker(filled, data.CachedTargetCertainty / barMax);
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

                string tip;
                if (ideo == pawn.Ideo)
                {
                    tip = CertaintyBar.Tooltip(pawn, data);
                }
                else
                {
                    var opinionRundown = data.DetailedIdeoOpinion(ideo);
                    tip = "EnhancedIdeology.PawnOpinionTooltip".Translate(pawn.Named("PAWN"), ideo.Named("IDEO"), opinion.ToStringPercent()) + "\n\n";
                    tip += "EnhancedIdeology.PawnOptionToolTip.FromMemesAndPrecepts".Translate(opinionRundown.BaseOpinion.ToStringPercent()) + "\n";
                    tip += "EnhancedIdeology.PawnOptionToolTip.FromPersonalBeliefs".Translate(opinionRundown.PersonalOpinion.ToStringPercent()) + "\n";
                    tip += "EnhancedIdeology.PawnOptionToolTip.FromInterpersonalRelationships".Translate(opinionRundown.RelationshipOpinion.ToStringPercent()) + "\n";
                    if (Prefs.DevMode)
                    {
                        tip += "\n== Dev Mode details ==";
                        tip += "\n" + opinionRundown.DevModeDetails;
                    }
                }

                TooltipHandler.TipRegion(selectRect, tip);
            }

            if (ideo == pawn.Ideo)
            {
                Widgets.DrawLineHorizontal(xOffset, pos + IdeoRowHeight - (Padding / 2), width - xOffset);
                pos += Padding;
            }

            pos += IdeoRowHeight;
        }
    }

    // Draws `glyph` at the left of the slot. A large swing draws two overlapping copies, offset along the
    // direction the glyph points: stacked for a strength arrow, side by side for a rung arrow.
    private void DrawArrow(Rect slot, string glyph, bool large, bool stacked)
    {
        var glyphRect = new Rect(slot.x, slot.y, glyphWidth, slot.height);
        if (!large)
        {
            Widgets.Label(glyphRect, glyph);
            return;
        }
        var offset = glyphWidth * DoubleArrowOffset;
        if (stacked)
        {
            Widgets.Label(new Rect(slot.x, slot.y - offset / 2f, glyphWidth, slot.height), glyph);
            Widgets.Label(new Rect(slot.x, slot.y + offset / 2f, glyphWidth, slot.height), glyph);
        }
        else
        {
            Widgets.Label(glyphRect, glyph);
            Widgets.Label(new Rect(slot.x + offset, slot.y, glyphWidth, slot.height), glyph);
        }
    }

    private static Color AgreementColor(float opinion) =>
        opinion > 0.01f ? AgreeColor : opinion < -0.01f ? DisagreeColor : Color.white;

    // Aligned on the rung a support target names, conflict off it or on a spot an opposition target opposes.
    // An allowed spot under an opposition target has no colour: the traits only rule spots out.
    private static Color? TraitFitColor(TraitStanceTarget target, float rank)
    {
        if (target.Rung is { } rung)
            return Mathf.Abs(rank - rung) <= InteractionWorker_IdeologicalDebatePrecept.DebateRankEpsilon
                ? AgreeColor : DisagreeColor;
        return target.OpposedWeight(rank) > 0f ? DisagreeColor : null;
    }

    // The opinion as a signed percentage of the full-conviction mark, e.g. "+80%" / "-40%".
    private static string Signed(float opinion)
    {
        var frac = opinion / ConvictionScale.MaxConvictionStrength;
        return (frac >= 0f ? "+" : "") + frac.ToStringPercent();
    }

    // A personal stance on an issue that neither faith holds shows only above this strength.
    private const float MinPersonalStanceStrength = 1f;

    // Every issue either the pawn's own faith or the selected ideoligion takes a stance on, plus the pawn's own
    // stances off Don't-care above MinPersonalStanceStrength (these also count in StructuralOpinionCalculator),
    // strongest first: the
    // issue, the rung the pawn prefers (Don't-care where their faith is silent), their signed opinion of the
    // selected ideoligion's stance on it (for the % and colouring), the rung that ideoligion preaches (for the
    // tooltip), and conviction strength (for ordering). Including the selected ideo's own issues is what surfaces
    // disagreement: on a rival's strong stance the pawn is often silent (neutral), which reads as opposition.
    private List<(IssueDef issue, string personalRung, float opinion, string selectedRung, float strength)> StanceRows(IdeoTrackerData data, Ideo selected)
    {
        var stances = data.IssueStances().ToDictionary(entry => entry.issue, entry => (entry.rank, entry.strength));
        var rows = new List<(IssueDef issue, string personalRung, float opinion, string selectedRung, float strength)>();

        var issues = pawn.Ideo.precepts.Concat(selected.precepts)
            .Where(precept => precept.def.issue != null)
            .Select(precept => precept.def.issue!)
            .Concat(data.Stances.HeldIssues().Where(issue => stances[issue].strength > MinPersonalStanceStrength))
            .Distinct();

        foreach (var issue in issues)
        {
            var category = PreceptPolicy.CategoryOf(issue);
            if ((category != PreceptCategory.Moral && category != PreceptCategory.Special)
                || !stances.TryGetValue(issue, out var stance))
            {
                continue;
            }

            rows.Add((issue, RungLabel(issue, stance.rank, pawn.Ideo), data.IssueOpinionToward(selected, issue),
                SelectedRungLabel(selected, issue), stance.strength));
        }

        var count = Math.Max(rows.Count, 1);
        return stanceSort switch
        {
            StanceSort.ByContributionDesc => [.. rows.OrderByDescending(row => row.opinion / count)],
            StanceSort.ByContributionAsc  => [.. rows.OrderBy(row => row.opinion / count)],
            StanceSort.ByIssueAsc         => [.. rows.OrderBy(row => row.issue.LabelCap.ToString())],
            StanceSort.ByIssueDesc        => [.. rows.OrderByDescending(row => row.issue.LabelCap.ToString())],
            _                             => [.. rows.OrderByDescending(row => row.strength)],
        };
    }

    // The rung the selected ideoligion preaches on the issue, or the classic-precept label (e.g. "One Only"
    // for SpouseCount) when that issue has a non-ladder classic default, or Don't-care as a last resort.
    private static string SelectedRungLabel(Ideo ideo, IssueDef issue)
    {
        var held = ideo.precepts.FirstOrDefault(precept => precept.def.issue == issue);
        if (held != null) return held.def.LabelCap;
        return PreceptLadder.ClassicFallback(issue)?.LabelCap ?? "EnhancedIdeology.StanceDontCare".Translate();
    }

    // What the pawn's traits oppose on the issue, and the stance they drift toward. An opposition target names
    // no drift while the stance is on an allowed spot, because the rank stays.
    private string TraitDriftText(IssueDef issue, TraitStanceTarget target, float rank)
    {
        var lines = new List<string>();
        if (target.Rung == null)
        {
            var opposed = target.Opposed.Keys.OrderBy(spot => spot).Select(spot => RungLabel(issue, spot, pawn.Ideo));
            lines.Add("EnhancedIdeology.TraitDriftAgainst".Translate(pawn.Named("PAWN"), string.Join(", ", opposed)));
        }
        var pullRank = target.PullFrom(rank).rank;
        if (target.Rung != null || pullRank != rank)
        {
            lines.Add("EnhancedIdeology.TraitDriftToward".Translate(pawn.Named("PAWN"), RungLabel(issue, pullRank, pawn.Ideo)));
        }
        return string.Join("\n", lines);
    }

    // A tied rung (PreceptPolicy.TiedRungs) shares its anchor's rank, so when the pawn's own faith holds a rung
    // at this rank, its label is used instead of the anchor's. Don't-care and indifference between or past the
    // rungs have no rung.
    private static string RungLabel(IssueDef issue, float rank, Ideo ownIdeo)
    {
        var rungs = PreceptLadder.Rungs(issue);
        var offRung = rank != Mathf.Round(rank)
            && (rank == PreceptLadder.DontCareRank(issue) || rank == PreceptLadder.IndifferentRank(issue));
        if (rank < 0f || rungs.Count == 0 || offRung)
        {
            return PreceptLadder.ClassicFallback(issue)?.LabelCap ?? "EnhancedIdeology.StanceDontCare".Translate();
        }

        var index = Mathf.Clamp(Mathf.RoundToInt(rank), 0, rungs.Count - 1);
        var own = ownIdeo.precepts.FirstOrDefault(precept => precept.def.issue == issue && PreceptLadder.RankOf(precept.def) == index);
        return own?.def.LabelCap ?? rungs[index].LabelCap;
    }

    // Only pawns with an ideology have a card. Classic mode has no certainty or stances to show.
    public static bool CanShow(Pawn? pawn) => pawn?.Ideo is not null && !Find.IdeoManager.classicMode;
}
