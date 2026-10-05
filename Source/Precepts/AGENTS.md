# Source/Precepts/

Precept classification, opinion computation, and conversion draw. This layer reads `IssueStanceTracker` stances and `PreceptPolicy` classifications to produce scores and draws.

**Call flow sketch:**
- Debate win → `IdeoTrackerData.PullStance` → `ConvictionMath` (fixed-vertex cosh valley)
- Tick / ritual / brainwipe → `IdeoTrackerData.TryBackgroundConversion` → `ConversionEvaluator.CheckConversion`
- Opinion cache recalc → `StructuralOpinionCalculator.Compute` → `PreceptLadder.OpinionOnPrecept` + `PreceptPolicy.CategoryOf`

**Files:**
- `PreceptLadder.cs` — resolves precept rungs into numeric ranks (one rung per rank; tied rungs map onto their anchor); `OpinionOnPrecept` rung-distance formula; `DontCareRank` lookup; primary dependency of `StructuralOpinionCalculator`
- `PreceptPolicy.cs` — issue category classification (`Moral`/`PositiveOnly`/`Special`/`NA`), `OrderOverrides`, `TiedRungs` (rungs sharing an anchor's rank, e.g. the armour specialties), `DontCare` placements; test hooks `RegisterCategory`/`ClearOverrides`
- `PreceptPolicy.Resolvers.cs` — resolver implementations for `Special` categories (VME_Leader categorical, VME_Mood pariah model, Weapons payload, Xenotypes Sørensen); coupling tables (`InducedByPrecept`, `CouplingPenalties`); startup warning for unclassified issues
- `StructuralOpinionCalculator.cs` — static; `Compute(pawn, stances, ideo)` aggregates meme, trait, diet/xeno, induced, per-issue, and universal terms into a 0–100 structural score; reads `IssueStanceTracker` and `PreceptPolicy`. The pawn side comes from their stances, not their current faith (issue set = target's issues ∪ issues the pawn holds a non-DontCare stance on; `InducedTargets` and coupling penalties work the same way), so an ideo scores the same whether it is the pawn's or not. Exceptions: loyalty memes (applied to other faiths only) and payload specials (Weapons, Xenotypes), which compare against the current faith.
- `ConversionEvaluator.cs` — static; weighted candidate draw (`CheckConversion`, `TryBackgroundConversion`), crisis-of-faith trigger (`TriggerCrisisOfFaith`), ideo switch (`ApplyConversion`); exposed via forwarders on `IdeoTrackerData`. Both draws compare candidates against the lived own-ideo opinion; structural reaches conversion only through the certainty setpoint. `noBreakdown` leaves out the crisis candidate. `ConversionChanceAfterKnock` must stay an exact preview of the `noBreakdown` draw (a test checks this).
- `ConvictionMath.cs` — fixed-vertex cosh valley math used by `IdeoTrackerData.PullStance` after a won debate; `ApplyRitualPull` for rituals and relic finds; `ApplyMoodPull` for precept and relic moods (a vote on the faith's rung)
