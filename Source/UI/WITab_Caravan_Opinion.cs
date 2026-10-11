using RimWorld.Planet;
using Verse.Sound;

namespace EnhancedIdeology;

// Caravan counterpart of ITab_Opinion, laid out like WITab_Caravan_Social: a list of the caravan's pawns that have
// an ideology, and the selected pawn's opinion card in a window docked to the right of the tab.
[HotSwappable]
internal sealed class WITab_Caravan_Opinion : WITab
{
    private const float RowHeight = 34f;
    private const float ScrollViewTopMargin = 15f;
    private const float TabWidth = 300f;
    private const float ButtonSize = 24f;
    private const float PawnIconSize = 27f;
    private const float IdeoIconSize = 24f;
    private const float Gap = 4f;
    private const float PawnLabelWidth = 100f;
    private const int CardWindowId = 738201554;

    private Vector2 scrollPosition;
    private float scrollViewHeight;
    private Pawn? specificOpinionTabForPawn;

    private IEnumerable<Pawn> Pawns => SelCaravan.PawnsListForReading.Where(OpinionCard.CanShow);

    public WITab_Caravan_Opinion()
    {
        labelKey = "EnhancedIdeology.TabOpinion";
    }

    public override bool IsVisible => SelCaravan != null && Pawns.Any();

    protected override void UpdateSize()
    {
        EnsureSpecificTabPawnValid();
        base.UpdateSize();
        size.x = TabWidth;
        size.y = Mathf.Min(550f, PaneTopY - 30f);
    }

    public override void OnOpen()
    {
        base.OnOpen();
        EnsureSpecificTabPawnValid();
        specificOpinionTabForPawn ??= Pawns.FirstOrDefault();
    }

    public override void Notify_ClearingAllMapsMemory()
    {
        base.Notify_ClearingAllMapsMemory();
        specificOpinionTabForPawn = null;
    }

    protected override void FillTab()
    {
        EnsureSpecificTabPawnValid();
        Text.Font = GameFont.Small;
        var outRect = new Rect(0f, ScrollViewTopMargin, size.x, size.y - ScrollViewTopMargin).ContractedBy(10f);
        var viewRect = new Rect(0f, 0f, outRect.width - GenUI.ScrollBarWidth, scrollViewHeight);
        var curY = 0f;
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        DoSection(ref curY, viewRect.width, "CaravanColonists", Pawns.Where(pawn => pawn.IsColonist));
        DoSection(ref curY, viewRect.width, "CaravanPrisonersAndAnimals", Pawns.Where(pawn => !pawn.IsColonist));
        if (Event.current.type == EventType.Layout)
        {
            scrollViewHeight = curY + 30f;
        }
        Widgets.EndScrollView();
    }

    protected override void ExtraOnGUI()
    {
        EnsureSpecificTabPawnValid();
        base.ExtraOnGUI();
        var pawn = specificOpinionTabForPawn;
        if (pawn == null)
        {
            return;
        }

        var card = new OpinionCard(pawn);
        var tabRect = TabRect;
        var cardRect = new Rect(tabRect.xMax - 1f, tabRect.yMax - card.Size.y, card.Size.x, card.Size.y);
        Find.WindowStack.ImmediateWindow(CardWindowId, cardRect, WindowLayer.GameUI, delegate
        {
            card.Draw();
            if (Widgets.CloseButtonFor(cardRect.AtZero()))
            {
                specificOpinionTabForPawn = null;
                SoundDefOf.TabClose.PlayOneShotOnCamera();
            }
        });
    }

    private void DoSection(ref float curY, float width, string headerKey, IEnumerable<Pawn> pawns)
    {
        var first = true;
        foreach (var pawn in pawns)
        {
            if (first)
            {
                Widgets.ListSeparator(ref curY, width, headerKey.Translate());
                first = false;
            }
            DoRow(new Rect(0f, curY, width, RowHeight), pawn);
            curY += RowHeight;
        }
    }

    private void DoRow(Rect rect, Pawn pawn)
    {
        Widgets.BeginGroup(rect);
        var rowRect = rect.AtZero();
        Widgets.InfoCardButton(rowRect.width - ButtonSize, (rect.height - ButtonSize) / 2f, pawn);
        rowRect.width -= ButtonSize;
        CaravanThingsTabUtility.DoOpenSpecificTabButton(rowRect, pawn, ref specificOpinionTabForPawn);
        rowRect.width -= ButtonSize;
        CaravanThingsTabUtility.DoOpenSpecificTabButtonInvisible(rowRect, pawn, ref specificOpinionTabForPawn);
        if (Mouse.IsOver(rowRect))
        {
            Widgets.DrawHighlight(rowRect);
        }

        var pawnIconRect = new Rect(Gap, (rect.height - PawnIconSize) / 2f, PawnIconSize, PawnIconSize);
        Widgets.ThingIcon(pawnIconRect, pawn);
        var labelRect = new Rect(pawnIconRect.xMax + Gap, 8f, PawnLabelWidth, 18f);
        GenMapUI.DrawPawnLabel(pawn, labelRect, 1f, PawnLabelWidth, null, GameFont.Small, alwaysDrawBg: false, alignCenter: false);

        var ideoIconRect = new Rect(labelRect.xMax + Gap, (rect.height - IdeoIconSize) / 2f, IdeoIconSize, IdeoIconSize);
        pawn.Ideo.DrawIcon(ideoIconRect);
        var certainty = Current.Game.GetComponent<GameComponent_EnhancedIdeology>().PawnTracker.EnsurePawnHasIdeoTracker(pawn).ExtendedCertainty;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(new Rect(ideoIconRect.xMax + Gap, 0f, rowRect.xMax - ideoIconRect.xMax - Gap, rect.height), certainty.ToStringPercent());
        Text.Anchor = TextAnchor.UpperLeft;
        TooltipHandler.TipRegion(new Rect(ideoIconRect.x, 0f, rowRect.xMax - ideoIconRect.x, rect.height),
            "EnhancedIdeology.PawnCertaintyTooltip".Translate(pawn.Named("PAWN"), pawn.Ideo.Named("IDEO"), certainty.ToStringPercent()));
        Widgets.EndGroup();
    }

    private void EnsureSpecificTabPawnValid()
    {
        if (specificOpinionTabForPawn != null
            && (specificOpinionTabForPawn.Destroyed || !SelCaravan.ContainsPawn(specificOpinionTabForPawn) || !OpinionCard.CanShow(specificOpinionTabForPawn)))
        {
            specificOpinionTabForPawn = null;
        }
    }
}
