# Source/Tracker/

Per-pawn belief state. External callers (patches, interactions, UI) go through `IdeoTrackerData` exclusively; the three sub-trackers are implementation detail.

**If you're in `IdeoTrackerData` and following a call into:**
- `data.Stances.*` → `IssueStanceTracker`
- `data.Certainty.*` → `CertaintyTracker`
- `data.Opinions.*` → `OpinionCache`
- `data.CheckConversion` / `data.TryBackgroundConversion` → one-line forwarders to `Precepts/ConversionEvaluator`

**Files:**
- `IdeoTrackerData.cs` — orchestrator and public API surface; owns the three sub-trackers, wires band recache (structural band cached until `InvalidateStances`/`InvalidateStructural`; `RefreshSlowInputs` every fourth long tick); `RecacheSetpointIfStale`/`RecacheBaseOpinionsIfStale` let the UI refresh only the shown pawn, by the recache tick stamps in `CertaintyTracker`/`OpinionCache` (`InvalidateStructural` clears the setpoint stamp), and hosts `PullStance` (the debate write-path calling `ConvictionMath`); conversion methods are forwarders to `ConversionEvaluator`
- `IssueStanceTracker.cs` — per-issue `(rank, strength)` store; `GetRank`/`GetStrength`/`ShiftStance`/`SetStance`/`ApplyDecayIfNewDay` (issues with a trait target take a `ConvictionMath.ValleyStep` toward it, others drain toward 0); spawn seeding (one full pass per session, `_seeded`) ends with `ApplyTraitTargets` on the newly seeded issues, and brainwipe returns targeted issues to their target; `RemapToLiveLadders` on load (see `Precepts/LadderMigration`); `ResetRanksToHeld` for the dev reset; `HoldsStance`/`HeldIssues` (issues off Don't-care, cached until a rank changes; every rank write must go through `SetRank`, which drops the cache); read by `StructuralOpinionCalculator` and `InteractionWorker_IdeologicalDebatePrecept`
- `TraitStanceTargets.cs` — static; `For(pawn, ranks)` turns the pawn's trait votes into one `TraitStanceTarget` per issue (rules: docs/design.md "Trait stance targets"). A target is a support rung or an opposed region; callers ask it `PullFrom(rank)` for the rank and strength to pull toward, and `OpposedWeight(rank)` for seeding's conviction penalty; the meme → rung map is cached (`ClearMemeCache` test hook). The optional `sources` sink records each vote's origin; seeding fills it and logs it when `DebugInteractionWorkers` is on. Read by `IssueStanceTracker` seeding, brainwipe and decay
- `CertaintyTracker.cs` — extended certainty float, three-band setpoint, calibration scheduling, drift finalisation; advanced via the `IdeoTracker_TickInterval` patch
- `OpinionCache.cs` — base/personal opinion dicts and dirty flag; invalidated on any stance or ideo change, except that mood pulls (`fromMood` on `SetIssueStance`/`ShiftIssueStance`) set `MarkMoodDirty`, which recomputes only once the oldest pull is `IdeoTrackerData.MoodRecacheTicks` (quarter day) old; `MarkRecached` clears both; relationship cache rebuilt by `RecalculateRelationshipIdeoOpinions` (drops dead and non-local pawns); read by `ITab_Opinion`
- `ConvictionScale.cs` — conviction strength bounds, trait/meme bonuses and the trait decay strength per vote, shared across subsystems; `StrengthFactor` is the diminishing-returns factor for reinforcing pulls (contemplation)
