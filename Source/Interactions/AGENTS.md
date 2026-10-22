# Source/Interactions/

Social interaction workers, ability workers, and play-log entries. The debate workers are the main callers of `IdeoTrackerData.PullStance` and `ConversionEvaluator.CheckConversion`.

**Debate flow:**
1. `InteractionWorker_IdeologicalDebatePrecept` selects a topic (`GetDebateTopic`; only Moral and Special issues qualify, via `PreceptPolicy.IsDebatable`, and the meme debate uses the same filter), rolls (`GetDebateRoll`/`WinChance`), and on win calls `PullStance` → `ConvictionMath`; conviction flip triggers `CheckConversion`
2. `InteractionWorker_IdeologicalDebateMeme` runs at the meme level: picks a meme with a disputed issue (`DebatableMemes`, reusing the precept worker's `StanceMap`/`Disagree`), rolls with the precept worker's `GetDebateRoll`/`DrawThreshold`, may also trigger conversion
3. `InteractionWorker_AdvancedConversionAttempt` applies a certainty knock then calls `CheckConversion` directly

**Ability flow:**
- `AbilityConversion` / `AbilityReassure` — psycast wrappers; `RollTargetIssues` picks contested issues, `Resolve` applies the outcome; both rerouted from vanilla via `CompAbilityEffect_Convert/Reassure_Reroute` patches

**Files:**
- `InteractionWorker_IdeologicalDebatePrecept.cs` — precept debate; key methods: `WinChance`, `GetDebateRoll`, `DrawThreshold`, `HandleDraw`, `TryEntrench`, `ApplyDiversityAftermath`, `ApplyApostacyAftermath`
- `InteractionWorker_IdeologicalDebateMeme.cs` — meme debate; `CompatibilityFactorCurve` shapes persuasion by meme-overlap
- `DebateFights.cs` — `Chance` (a draw's fight chance: mean vanilla `SocialFightChance` at `DrawFightBaseChance`, max-social falloff, topic weight, `DiversityFightFactor`, apostasy cross-faith; flat for iconoclasts) and `Start`; `DiversityAxis` reads a faith's IdeoDiversity rung, also used by the diversity aftermath. Called from `HandleDraw`
- `InteractionWorker_AdvancedConversionAttempt.cs` — directed conversion attempt; resolves debate then calls `CheckConversion`
- `AbilityConversion.cs` / `AbilityReassure.cs` — psycast ability logic; `RollTargetIssues`, `Resolve`
- `JoyGiver_IdeologicalDebate.cs` — joy giver for `EB_DebateRelax` (vanilla social relax under its own JobDef); while a pawn runs it, the precept debate weight is multiplied by `DebateRelaxWeightFactor`
- `DebateOnlookers.cs` — after a decisive precept or meme debate, `Sway` finds pawns in hearing range with line of sight (in a caravan: `CaravanDebates.Onlookers`) and calls `SwayWitness`, a reduced `PullStance` toward the winner's personal stance
- `CaravanDebates.cs` — off-map debate rules: `IsAlert` (rest ≥ 28%, vanilla Tired line) gates debaters and onlookers, `Onlookers`, `SettleFight` (fight memories instead of a fight); the debates are started by `HarmonyPatches/Caravan_TickInterval_Debates.cs`
- `PlayLogEntry_Conversion.cs`, `PlayLogEntry_CrisisOfFaith.cs`, `PlayLogEntry_DebateInteraction.cs` — play-log entries for audit trail; `PlayLogEntry_DebateInteraction.FromLastDebate` builds the entry from a debate worker's last result
