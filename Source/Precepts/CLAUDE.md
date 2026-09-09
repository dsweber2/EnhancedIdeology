# Source/Precepts/

Precept classification, opinion computation, and conversion draw. This layer reads `IssueStanceTracker` stances and `PreceptPolicy` classifications to produce scores and draws.

**Call flow sketch:**
- Debate win → `IdeoTrackerData.PullStance` → `ConvictionMath` (fixed-vertex cosh valley)
- Tick / ritual / brainwipe → `IdeoTrackerData.TryBackgroundConversion` → `ConversionEvaluator.CheckConversion`
- Opinion cache recalc → `StructuralOpinionCalculator.Compute` → `PreceptLadder.OpinionOnPrecept` + `PreceptPolicy.CategoryOf`

**Files:**
- `PreceptLadder.cs` — resolves precept rungs into numeric ranks; `OpinionOnPrecept` rung-distance formula; `DontCareRank` lookup; primary dependency of `StructuralOpinionCalculator`
- `PreceptPolicy.cs` — issue category classification (`Moral`/`PositiveOnly`/`Special`/`NA`), `OrderOverrides`, `DontCare` placements; test hooks `RegisterCategory`/`ClearOverrides`
- `PreceptPolicy.Resolvers.cs` — resolver implementations for `Special` categories (VME_Leader categorical, VME_Mood pariah model, Weapons payload, Xenotypes Sørensen); coupling tables (`InducedByPrecept`, `CouplingPenalties`); startup warning for unclassified issues
- `StructuralOpinionCalculator.cs` — static; `Compute(pawn, stances, ideo)` aggregates meme, trait, diet/xeno, induced, per-issue, and universal terms into a 0–100 structural score; reads `IssueStanceTracker` and `PreceptPolicy`
- `ConversionEvaluator.cs` — static; weighted candidate draw (`CheckConversion`, `TryBackgroundConversion`), crisis-of-faith trigger (`TriggerCrisisOfFaith`), ideo switch (`ApplyConversion`); exposed via forwarders on `IdeoTrackerData`
- `ConvictionMath.cs` — fixed-vertex cosh valley math used by `IdeoTrackerData.PullStance` after a won debate
