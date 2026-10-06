# Source/GameComponent/

Game-level tracker registry. Maps every live pawn to its `IdeoTrackerData` and maintains the reverse ideo → pawn-set index. Nearly every HarmonyPatch does `Find.GameComponent<GameComponent_EnhancedIdeology>().GetTracker(pawn)` to reach the per-pawn tracker.

- `GameComponent_EnhancedIdeology.cs` — main component; `GetTracker(pawn)` returns the per-pawn `IdeoTrackerData`; `SetIdeo` updates membership maps; owns `RelationalIntensityCurve`/`PracticeIntensityCurve`; `ExposeData` saves a `LadderMigration` snapshot of every ladder, read back through `SavedLadderFor` when pawn stances load
- `GameComponent_EnhancedIdeology_PawnTracker.cs` — partial: per-pawn tracker dictionary, `EnsurePawnHasIdeoTracker`, expose/load
- `GameComponent_EnhancedIdeology_IdeoPawnTracker.cs` — partial: reverse index of ideo → pawn set, `GetIdeoPawns`
