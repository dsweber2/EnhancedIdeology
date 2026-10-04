# Source/UI/

UI components for displaying certainty and ideo opinion data.

- `CertaintyBar.cs` — reusable certainty bar widget; `DrawThreshold` draws the crisis threshold line, `DrawTargetMarker` draws the setpoint marker; used by both the `SocialCardUtility_DrawCertainty` patch and `OpinionCard`
- `OpinionCard.cs` — one pawn's stance list and opinion of each known ideo; the constructor does layout (`Size`), `Draw()` paints at the group origin; `CanShow` gates both tabs; per-issue breakdown visible in dev mode
- `ITab_Opinion.cs` — pawn inspect tab; thin host for `OpinionCard`
- `WITab_Caravan_Opinion.cs` — caravan world tab (patched in after Social); lists pawns with an ideo, and docks the selected pawn's `OpinionCard` beside the tab like `WITab_Caravan_Social`
