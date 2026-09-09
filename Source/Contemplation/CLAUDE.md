# Source/Contemplation/

Contemplation need, job, site, and visuals. Contemplation is a belief-reinforcement activity gated by precept (`JoyGiver_Contemplation.ContemplationAllowedByPrecept`); completing the job calls `CertaintyTracker` via `CertaintyChangeRecache`.

- `Need_Contemplation.cs` — need that decays over time; `CurCategory` (Satisfied/Low/Critical); fulfilled by calling `Satisfy`
- `JobDriver_Contemplate.cs` — walks to site, applies certainty arc per tick, spawns icon mote; `StrengthFactor` scales gain by pawn conviction strength
- `JobGiver_ContemplateFromNeed.cs` — AI job giver; triggers when `Need_Contemplation` falls below threshold
- `JoyGiver_Contemplation.cs` — joy-path entry; `FindSites`, `ImpressivenessScore`, `AvgStrengthFactor`; checks precept permission via `ContemplationAllowedByPrecept`
- `Comp_ContemplationSite.cs` — building comp marking a structure as a valid site; `GetEffectiveGain`, `TryGetSite`, `GetArcMultiplier` vary by site kind
- `Mote_ContemplationIcon.cs` — visual mote displayed above pawns while contemplating
