# Source/Books/

Religious book mechanics: custom book type, reading outcomes, burning events, trader stock, and crafting.

- `BookIdeo.cs` — `Book` subclass for ideo-linked books; `PostQualitySet` converts quality to an ingredient-based certainty gain formula; `TickRare` regenerates dynamic stance text; `RegenerateName` builds the book's name from ideo data; `PostGeneratedForTrader` assigns ideo on trader spawn
- `CompReligiousBook.cs` — book comp that tracks which ideo the book belongs to; owns burning mechanics (gizmo → `ValidBurnerPawns` → `JobDriver_PlaceAndBurnUntilDestroyed`), destruction events (certainty impact on followers, history records, letters), and damage/destroy hooks
- `CompBookIngredients.cs` — crafting ingredient comp that enforces ideo-matching materials
- `ReadingOutcomeDoer_CertaintyChange.cs` — vanilla reading outcome doer; calls `CertaintyChangeRecache` on the reader's tracker
- `StockGenerator_IdeoBook.cs` — generates ideo-appropriate books in trader stock
- `InspirationWorker_ReligiousEnlightenment.cs` — inspiration that grants a certainty spike via `CertaintyChangeRecache`
