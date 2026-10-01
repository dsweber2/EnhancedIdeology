# Source/Tracker/

Per-pawn belief state. External callers (patches, interactions, UI) go through `IdeoTrackerData` exclusively; the three sub-trackers are implementation detail.

**If you're in `IdeoTrackerData` and following a call into:**
- `data.Stances.*` → `IssueStanceTracker`
- `data.Certainty.*` → `CertaintyTracker`
- `data.Opinions.*` → `OpinionCache`
- `data.CheckConversion` / `data.TryBackgroundConversion` → one-line forwarders to `Precepts/ConversionEvaluator`

**Files:**
- `IdeoTrackerData.cs` — orchestrator and public API surface; owns the three sub-trackers, wires band recache, and hosts `PullStance` (the debate write-path calling `ConvictionMath`); conversion methods are forwarders to `ConversionEvaluator`
- `IssueStanceTracker.cs` — per-issue `(rank, strength)` store; `GetStance`/`SetStance`/`Decay`; read by `StructuralOpinionCalculator` and `InteractionWorker_IdeologicalDebatePrecept`
- `CertaintyTracker.cs` — extended certainty float, three-band setpoint, calibration scheduling, drift finalisation; advanced via the `IdeoTracker_TickInterval` patch
- `OpinionCache.cs` — base/personal/meme opinion dicts and dirty flag; invalidated on any stance or ideo change; read by `ITab_Opinion` and `ThoughtWorker_IdeologyOpinion`
- `ConvictionScale.cs` — named strength constants (Weak → Devoted); used as threshold literals in `IssueStanceTracker` and the debate workers
