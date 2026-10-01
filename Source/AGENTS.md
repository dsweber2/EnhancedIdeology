# Source/

## Navigation

| If you're looking for… | Go to… |
|---|---|
| Per-pawn stance, certainty, or opinion data | `Tracker/` |
| Debate write-path, conviction valley math | `Tracker/IdeoTrackerData.cs` → `Precepts/ConvictionMath.cs` |
| Structural opinion score | `Precepts/StructuralOpinionCalculator.cs` |
| Precept classification / category | `Precepts/PreceptPolicy.cs` |
| Conversion draw, crisis of faith | `Precepts/ConversionEvaluator.cs` |
| Debate / conversion / reassure interaction | `Interactions/` |
| Vanilla method hooks | `HarmonyPatches/` |
| Component that owns tracker-by-pawn lookup | `GameComponent/` |

## Subdirectories
- `Tracker/` — per-pawn belief state: `IdeoTrackerData` orchestrator + `IssueStanceTracker`, `CertaintyTracker`, `OpinionCache`, `ConvictionScale`
- `Precepts/` — precept classification, ladder resolution, structural opinion computation, conversion draw
- `GameComponent/` — game-level pawn↔ideo tracker registry; entry point for patches needing `GetTracker(pawn)`
- `Interactions/` — debate, conversion, and reassure interaction workers; play-log entries
- `HarmonyPatches/` — Harmony patches on vanilla entry points; grouped by system (tick, certainty, conversion, UI, misc)
- `Books/` — religious book type (`BookIdeo`), reading outcomes, burning mechanics, trader stock
- `Contemplation/` — contemplation need, job, site, and visuals
- `Jobs/` — mental states and job givers for crisis-of-faith and iconoclast
- `Rituals/` — ritual outcome effect workers
- `Relics/` — conviction effects of finding relics and of vanilla relic thoughts
- `Thoughts/` — custom thought classes and workers
- `UI/` — certainty bar widget and opinion inspect tab

## Root-level files
Infrastructure and entry points that don't belong to a single subsystem:
- `EnhancedIdeologyMod.cs` — mod entry point, Harmony setup, debug logging
- `Settings.cs` — mod settings (drift rate, band ranges, thresholds)
- `EnhancedIdeologyDefOf.cs` — `[DefOf]` references
- `EnhancedIdeologyUtilities.cs` — shared helpers (apostasy strictness, etc.)
- `HediffComp_BrainwipeRecovery.cs` — hediff comp for brainwipe susceptibility multiplier
- `Compat_PeerPressure.cs` — peer-pressure mod compatibility shim
- `DebugActions.cs` — dev-mode debug action menu entries
- `HotSwappableAttribute.cs`, `GlobalSuppressions.cs`, `globalusings.cs` — infrastructure
