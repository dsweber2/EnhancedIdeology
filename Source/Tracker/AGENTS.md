# Source/Tracker/

Per-pawn belief state. External callers (patches, interactions, UI) go through `IdeoTrackerData` exclusively; the three sub-trackers are implementation detail.

**If you're in `IdeoTrackerData` and following a call into:**
- `data.Stances.*` → `IssueStanceTracker`
- `data.Certainty.*` → `CertaintyTracker`
- `data.Opinions.*` → `OpinionCache`
- `data.CheckConversion` / `data.TryBackgroundConversion` → one-line forwarders to `Precepts/ConversionEvaluator`

**Files:**
- `IdeoTrackerData.cs` — orchestrator and public API surface; owns the three sub-trackers, wires band recache (structural band cached until `InvalidateStances`/`InvalidateStructural`; `RefreshSlowInputs` every fourth long tick); `RecacheSetpointIfStale`/`RecacheBaseOpinionsIfStale` let the UI refresh only the shown pawn, by the recache tick stamps in `CertaintyTracker`/`OpinionCache` (`InvalidateStructural` clears the setpoint stamp), and hosts `PullStance` (the debate write-path calling `ConvictionMath`); conversion methods are forwarders to `ConversionEvaluator`
- `IssueStanceTracker.cs` — per-issue `(rank, strength)` store; `GetStance`/`SetStance`/`Decay`; spawn seeding (one full pass per session, `_seeded`) ends with `ApplyTraitIssueLinks` (silent faith → weak stance on the linked rung; faith holds it → conviction bonus; other rung → heterodox), and brainwipe returns linked issues to their rung; `RemapToLiveLadders` on load (see `Precepts/LadderMigration`); `ResetRanksToHeld` for the dev reset; read by `StructuralOpinionCalculator` and `InteractionWorker_IdeologicalDebatePrecept`
- `CertaintyTracker.cs` — extended certainty float, three-band setpoint, calibration scheduling, drift finalisation; advanced via the `IdeoTracker_TickInterval` patch
- `OpinionCache.cs` — base/personal/meme opinion dicts and dirty flag; invalidated on any stance or ideo change, except that mood pulls (`fromMood` on `SetIssueStance`/`ShiftIssueStance`) set `MarkMoodDirty`, which recomputes only once the oldest pull is `IdeoTrackerData.MoodRecacheTicks` (quarter day) old; `MarkRecached` clears both; relationship cache rebuilt by `RecalculateRelationshipIdeoOpinions` (drops dead and non-local pawns); read by `ITab_Opinion` and `ThoughtWorker_IdeologyOpinion`
- `ConvictionScale.cs` — named strength constants (Weak → Devoted); used as threshold literals in `IssueStanceTracker` and the debate workers
