using RimWorld.Planet;

namespace EnhancedIdeology;

// Window host for OpinionCard when the Ideology tab is turned off. It follows the selected pawn, and closes when the
// selection has no certainty to show. Opened from the social card (HarmonyPatches/SocialCardUtility_OpinionButton).
[HotSwappable]
internal sealed class Window_OpinionCard : Window
{
    private readonly Vector2 dockPosition;
    private Pawn? pawn;
    private OpinionCard? card;

    private Window_OpinionCard(Pawn pawn, Vector2 dockPosition)
    {
        this.pawn = pawn;
        this.dockPosition = dockPosition;
        layer = WindowLayer.GameUI;
        draggable = true;
        preventCameraMotion = false;
        closeOnClickedOutside = false;
        absorbInputAroundWindow = false;
        soundAppear = SoundDefOf.TabOpen;
        soundClose = SoundDefOf.TabClose;
    }

    protected override float Margin => 0f;

    public override Vector2 InitialSize
    {
        get
        {
            Text.Font = GameFont.Small;
            return new OpinionCard(pawn!).Size;
        }
    }

    // Opens the window docked at dockPosition (UI coordinates). If it is already open, the same pawn closes it and a
    // different pawn moves it to that pawn.
    public static void Toggle(Pawn pawn, Vector2 dockPosition)
    {
        var open = Find.WindowStack.WindowOfType<Window_OpinionCard>();
        if (open == null)
        {
            Find.WindowStack.Add(new Window_OpinionCard(pawn, dockPosition));
        }
        else if (open.pawn == pawn)
        {
            open.Close();
        }
        else
        {
            open.pawn = pawn;
        }
    }

    protected override void SetInitialSizeAndPosition()
    {
        base.SetInitialSizeAndPosition();
        windowRect.position = dockPosition;
        KeepOnScreen();
    }

    public override void WindowOnGUI()
    {
        pawn = FollowedPawn();
        if (!OpinionCard.CanShow(pawn))
        {
            Close();
            return;
        }

        Text.Font = GameFont.Small;
        card = new OpinionCard(pawn!);
        windowRect.size = card.Size;
        KeepOnScreen();
        base.WindowOnGUI();
    }

    public override void DoWindowContents(Rect inRect)
    {
        card!.Draw();
        if (Widgets.CloseButtonFor(inRect))
        {
            Close();
        }
    }

    // On the map, the selected pawn. On the world, the current pawn while its caravan stays selected: a caravan
    // selection does not identify one pawn, so the social card button picks it.
    private Pawn? FollowedPawn()
    {
        if (WorldRendererUtility.WorldSelected)
        {
            return Find.WorldSelector.SingleSelectedObject is Caravan caravan && pawn != null && caravan.ContainsPawn(pawn) ? pawn : null;
        }
        return Find.Selector.SingleSelectedThing as Pawn;
    }

    private void KeepOnScreen()
    {
        windowRect.x = Mathf.Clamp(windowRect.x, 0f, UI.screenWidth - windowRect.width);
        windowRect.y = Mathf.Clamp(windowRect.y, 0f, UI.screenHeight - windowRect.height);
    }
}
