# tests/

xunit tests that run the real mod source against a hand-written RimWorld stand-in. Run with `make test`.

- `EnhancedIdeology.Tests.csproj` — `<Compile Include>`s selected files from `../Source/` directly. To test a new source file, add it there; every RimWorld member it touches must exist in the shim.
- `RimWorldShim/` — fake `Verse`/`RimWorld`/`UnityEngine` types with the same names and signatures as the game. Only members the compiled source uses. `Pawn.IsHashIntervalTick` mirrors vanilla's hash-offset maths.
- `Support/` — test-side replacements and builders:
  - `EnhancedIdeologyModStub.cs` — stands in for `EnhancedIdeologyMod` and `Settings` (`SimSettings`); any setting the source reads must exist here
  - `HarmonyStubs.cs` — no-op Harmony attributes so patch classes compile
  - `VegetarianUtils.cs` — stub for the helper that lives in a non-compiled patch file
  - `SimWorld.cs` — sets up `Find`/`Current` statics and the game component; `AddIdeo` mirrors the `Ideo` constructor patch
  - `PawnBuilder.cs`, `IdeoBuilder.cs` (+ `SimIssues` ladders, `MemeBuilder`), `SimPawn.cs`, `SimThought.cs` — fixtures
- `SeededTest.cs` — base class; reseeds `Rand` and resets global state before every test
- `*Tests.cs` — one file per subsystem
