# Source/HarmonyPatches/

Harmony patches on vanilla entry points. Each file patches one or a small cluster of related methods.

**Lifecycle / tick:**
- `IdeoTracker_TickInterval.cs` — tick hook: advances certainty drift, fires `TryBackgroundConversion`, applies conviction decay; interval checks take vanilla's `delta` (1.6 ticks pawns every 1–15 ticks); also compiled into the tests (`TickScheduleTests`)
- `PawnComponentsUtility_Initialize.cs` — attaches `IdeoTrackerData` on pawn spawn
- `Pawn_ExposeData.cs` — syncs tracker on save/load; holds data for pawns loaded before the game component exists (other mods' components) until PostLoadInit
- `PawnGenerator_Generate.cs` — seeds initial stances on pawn generation
- `BackCompatibility_TypeMigration.cs` — handles save-file type renames from namespace migrations

**Certainty / ideo change interception:**
- `IdeoTracker_CertaintyChange.cs` — replaces vanilla certainty delta with `CertaintyChangeRecache`
- `IdeoTracker_SetIdeo.cs` — syncs `GameComponent` membership maps on ideo swap
- `IdeoTracker_TryJoinIdeoFromExposures.cs` — reroutes child ideo assignment to opinion-weighted `CheckConversion`
- `IdeoChangeBreak_Start.cs` — suppresses vanilla ideo-change mental break when EB handles it
- `FluidIdeoTracker_Reformed.cs` — hooks fluid ideo reform to reseed stances
- `TraitSet_TraitAdded.cs` / `TraitSet_TraitRemoved.cs` — invalidate `OpinionCache` on trait change

**Conversion / ritual reroutes:**
- `RitualOutcomeEffectWorker_Conversion_Reroute.cs` — transpiler replacing vanilla ritual conversion with `CheckConversion`
- `RitualOutcomeEffectWorker_Speech_Reroute.cs` — same for speech ritual
- `CompAbilityEffect_Convert_Reroute.cs` — replaces ability conversion with EB path; also patches tooltip
- `CompAbilityEffect_Reassure_Reroute.cs` — replaces ability reassure with `AbilityReassure.Resolve`; also patches validity check and tooltip

**UI injection:**
- `SocialCardUtility_DrawCertainty.cs` — injects extended certainty bar and setpoint marker into the social card
- `NeedsCardUtility_DrawThoughtGroup.cs` — injects certainty bar into the needs card thought group
- `ITab_Book_Size.cs` — adjusts book inspect tab size for EB's extended content
- `BookUIUtility_DrawBenefits_Reroute.cs` — adjusts book benefit display for EB's custom reading outcome
- `TryInteractWith_DebateLog.cs` — adds debate play-log entry after interaction resolves
- `InteractionDef_Symbol.cs` — patches grammar symbol resolution for debate log entries

**ArrestedDebater needs blocking:**
- `ArrestedDebater_BlockNeeds.cs` — zeroes `GetPriority` for `JobGiver_GetRest` and `JobGiver_GetFood` when pawn is in `EB_ArrestedDebater`; ensures the pawn neither sleeps nor eats during the defiant rant

**Misc:**
- `Ideo_Constructor.cs` — postfix to initialize EB ideo state on construction
- `ExpectationsUtility_Override.cs` — overrides expectation thresholds based on certainty
- `Bill_PawnAllowedToStartAnew.cs` — certainty gate on the "start anew" bill
- `MemoryThoughts_TryGainMemory.cs` — intercepts meme memory gain to attach `ConvictionDeltaPerTickLong`; prefix assigns the relic precept as `sourcePrecept` on vanilla relic thoughts

**Relics** (logic in `Relics/RelicConviction`):
- `ThingStyleHelper_SetEverSeenByPlayer.cs` — first player sighting of a relic triggers `ApplyFindBoost`
- `Precept_Relic_NotifyThingLost.cs` — transpiler restricting relic lost/destroyed thoughts and letter to followers of the relic's ideo
- `PsychicRitualToil_Brainwipe_Start.cs` — hooks brainwipe start to reset stances
- `WorkGiver_DoBill_LeatherRestriction.cs` — enforces vegetarian/carnivore leather crafting restrictions
- `GenRecipe_MakeRecipeProducts_QualityBoost.cs` — quality bonus for crafting religious books
