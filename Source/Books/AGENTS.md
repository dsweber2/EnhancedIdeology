# Source/Books/

Religious book mechanics: custom book type, reading outcomes, burning events, and crafting.

- `BookIdeo.cs` — `Book` subclass for ideo-linked books; `PostQualitySet` converts quality to an ingredient-based certainty gain formula; `TickRare` regenerates dynamic stance text; `RegenerateName` builds the book's name from ideo data; generated books (trader, settlement, reward) get a random ideo via the doer's `OnBookGenerated`
- `CompReligiousBook.cs` — book comp that tracks which ideo the book belongs to; owns burning mechanics (gizmo → `ValidBurnerPawns` → `JobDriver_PlaceAndBurnUntilDestroyed`), destruction events (certainty impact on followers, history records, letters), and damage/destroy hooks
- `CompBookIngredients.cs` — crafting ingredient comp that enforces ideo-matching materials
- `ReadingOutcomeDoer_CertaintyChange.cs` — vanilla reading outcome doer; calls `CertaintyChangeRecache` on the reader's tracker; `ReaderGain` is the per-reader gain (with the vegetarian leather penalty); each tick takes a `ConvictionMath.ValleyStep` of arc gain × book conviction toward the book's rung, with target strength `AbsoluteMaxConvictionStrength` for an own-faith reader and the book's conviction for a rival reader; `PeakArcPerTick` (the arc on the book's strongest issue) feeds the benefits string and the job report
- `PrisonerReading.cs` — pacing for `Jobs/JobGiver_PrisonerReadReligiousText`: `ShareOfDay` (1 − certainty, clamped 0.25–0.9) and `CycleTicks` (session start to the earliest next start)
- `InspirationWorker_ReligiousEnlightenment.cs` — inspiration that grants a certainty spike via `CertaintyChangeRecache`
