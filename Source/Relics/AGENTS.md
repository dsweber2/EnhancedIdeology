# Source/Relics/

Relic events move conviction on every Moral issue of the relic's ideo, because a relic is a token of the whole faith.

- `RelicConviction.cs` — all relic logic:
  - `ApplyFindBoost` — on first sighting by the player, every living follower gets a ritual-style valley pull (`RelicFoundArc`, 2× `RitualBaseArc`) toward orthodoxy on each Moral issue; called by `HarmonyPatches/ThingStyleHelper_SetEverSeenByPlayer`
  - `ApplyMoodletShift` — per-TickLong strength shift from a vanilla relic thought (`RelicLost`, `RelicDestroyed`, `RelicsCollected`, `RelicAtRitual`); same rate as a precept moodlet of equal mood, split across the Moral issues; called by `GameComponent_EnhancedIdeology.ApplyMoodletConvictionShifts`
  - `SourceFor` — the relic precept assigned as `sourcePrecept` to relic thoughts (by the `MemoryThoughts_TryGainMemory` prefix), so the practice band counts them and they stay tied to the ideo
- The ideo filter on relic loss thoughts is a transpiler in `HarmonyPatches/Precept_Relic_NotifyThingLost`.
