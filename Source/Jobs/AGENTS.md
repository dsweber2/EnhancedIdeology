# Source/Jobs/

Mental states and job givers for crisis-of-faith, iconoclast, and arrested-debater behaviours. Crisis of faith is triggered by `ConversionEvaluator.TriggerCrisisOfFaith`; iconoclast is a separate mental break unrelated to conversion; arrested debater is a secondary state that fires when an iconoclast pawn is arrested mid-break.

- `MentalState_CrisisOfFaith.cs` — mental state: pawn wanders aimlessly; clears on timer or certainty recovery
- `JobGiver_CrisisOfFaith.cs` — AI job giver: issues wander jobs during active crisis state
- `MentalState_Iconoclast.cs` — mental state that tracks target structure and burn progress; `PostStart` records the break in the play log
- `MentalStateWorker_Iconoclast.cs` — checks whether the iconoclast break can occur (pawn has ideo, burnable structures exist)
- `MentalBreakWorker_Iconoclast.cs` — mental break worker that triggers `MentalState_Iconoclast` with a chosen target
- `JobGiver_Iconoclast.cs` — AI job giver: finds ideoligion-linked structures and issues the burn job
- `JobDriver_PlaceAndBurnUntilDestroyed.cs` — places an incendiary on target and waits for destruction; also contains `Toil_EnhancedIdeology` helper toil
- `MentalState_ArrestedDebater.cs` — tick-based state: debates the nearest reachable pawn every 200 ticks; entered from `MentalState_Iconoclast` when the pawn becomes `IsPrisonerOfColony`
- `MentalStateWorker_ArrestedDebater.cs` — gates `StateCanOccur`/`CanInitiateNow` to `IsPrisonerOfColony`; no natural break path
