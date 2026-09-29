# Source/UI/

UI components for displaying certainty and ideo opinion data.

- `CertaintyBar.cs` — reusable certainty bar widget; `DrawThreshold` draws the crisis threshold line, `DrawTargetMarker` draws the setpoint marker; used by both the `SocialCardUtility_DrawCertainty` patch and `ITab_Opinion`
- `ITab_Opinion.cs` — inspect tab listing structural, personal, and relational opinions toward each known ideo; reads `OpinionCache` directly; per-issue breakdown visible in dev mode
