# Source/Interactions/

Social interaction workers, ability workers, and play-log entries. The debate workers are the main callers of `IdeoTrackerData.PullStance` and `ConversionEvaluator.CheckConversion`.

**Debate flow:**
1. `InteractionWorker_IdeologicalDebatePrecept` selects a topic (`GetDebateTopic`), rolls (`GetDebateRoll`/`WinChance`), and on win calls `PullStance` → `ConvictionMath`; conviction flip triggers `CheckConversion`
2. `InteractionWorker_IdeologicalDebateMeme` runs at the meme level, shifts meme opinion, may also trigger conversion
3. `InteractionWorker_AdvancedConversionAttempt` applies a certainty knock then calls `CheckConversion` directly

**Ability flow:**
- `AbilityConversion` / `AbilityReassure` — psycast wrappers; `RollTargetIssues` picks contested issues, `Resolve` applies the outcome; both rerouted from vanilla via `CompAbilityEffect_Convert/Reassure_Reroute` patches

**Files:**
- `InteractionWorker_IdeologicalDebatePrecept.cs` — precept debate; key methods: `WinChance`, `GetDebateRoll`, `HandleDraw`, `TryEntrench`, `ApplyDiversityAftermath`, `ApplyApostacyAftermath`
- `InteractionWorker_IdeologicalDebateMeme.cs` — meme debate; `CompatibilityFactorCurve` shapes persuasion by meme-overlap
- `InteractionWorker_AdvancedConversionAttempt.cs` — directed conversion attempt; resolves debate then calls `CheckConversion`
- `AbilityConversion.cs` / `AbilityReassure.cs` — psycast ability logic; `RollTargetIssues`, `Resolve`
- `JoyGiver_IdeologicalDebate.cs` — AI joy-path trigger for debates
- `PlayLogEntry_Conversion.cs`, `PlayLogEntry_CrisisOfFaith.cs`, `PlayLogEntry_DebateInteraction.cs` — play-log entries for audit trail
