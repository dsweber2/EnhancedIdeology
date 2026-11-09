# Ladder heuristic (started 2026-11-01, live 2026-11-03)

Goal: stop hand-patching simple mod issues.
A rule-based classifier reads each rung's stance word and proposes the category, rung order and neutral placement for any multi-rung issue the `PreceptPolicy` tables do not list.
It flags anything unusual or janky for manual review.
Status: live in the game as a fallback below the hand tables, and David ran the in-game checklist in `manualTesting.md` (2026-11-03).

## What a compat patch sets

Every patch so far sets some subset of three knobs:

1. **Category**: Moral vs PositiveOnly (Special, NA and UniversalPositive stay hand-made).
2. **Order**: an `OrderOverrides` entry when `displayOrderInIssue` ties or scrambles.
3. **Neutral**: a `DontCare` entry when the issue has no default-weight rungs, or the defaults misplace neutral.

`PreceptLadder.DontCareRank` already derives neutral from default-weight rungs, so knob 3 is partly automatic.

## Decisions made

- **The heuristic runs in the game**, as a fallback below the hand tables, not as an offline script that proposes table entries.
  This follows "not my modlist": players' mods that nobody surveyed get sane behaviour instead of PositiveOnly plus a `Log.Error`.
- **The hand tables always win.** The heuristic reads only issues where `PreceptPolicy.IsCurated` is false, including issues listed only in `OrderOverrides` or `DontCare`.
- **Auto-classified issues appear in a `Log.Message` on every startup.** Flagged issues keep the `Log.Error`, which now names the flags.
- **Mechanical perk issues are flagged, not classified** (Increased/Decreased, Fast/Slow, Efficient, Speed, Yield...).
  Past hand calls on these are inconsistent (`VME_BookReadingSpeed` is Moral, `VME_CraftingSpeed` is PositiveOnly), so David reviews them later.
- **The backtest is a hard gate.** The heuristic does not ship until it is never confidently wrong on the curated set.
- **Single-rung issues are out of scope (rule R4 dropped).**
  A probe of the single-rung issues with a lexicon word found 22 hand-classified Moral, 24 kept PositiveOnly (`AM_TeaDrinking` and others), and 10 undecided.
  The outsider-objects test is semantic, and single-rung issues never trigger the startup error.
- **Ladders stacked from several mods are not flagged on their own.** David thinks they are usually fine; the backtest decides.

## Rules

- **R0**: an entry in the hand tables wins.
- **R1, category**: every rung's stance word is in the lexicon, with no mechanical words and no flags → Moral.
  Otherwise the issue stays PositiveOnly and is flagged.
  The rule is strict because PositiveOnly is the safe failure (inert), and a Moral ladder with a wrong order quietly changes opinion.
  An alternative, R1′, would make every evaluative ladder Moral and flag only the order or neutral; revisit it if ties turn out to be common.
- **R2, order**: sort by signed intensity, pro → anti.
  The direction does not matter, because `OpinionOnPrecept` gives the same result when the ladder is reversed.
  Two rungs with the same intensity → flag.
- **R3, neutral**:
  - any default-weight rung needs no meme → return null, and `DontCareRank` uses `PreceptLadder.NeutralOfDefaults` (the highest-weight rung, or the weighted mean if several share it) as it does for every issue;
  - default rungs that all need a meme are ignored, because most faiths cannot take them, and the rules below apply;
  - else a 0-intensity rung → neutral is at that rung, but flag it if the rung has XML effects (the `VME_Pain_DontCare` and `Comfort_Ignored` traps);
  - else a one-sided ladder → half a step past the mildest rung;
  - else a two-sided ladder → midway between the mildest pro rung and the mildest anti rung.
- **R5, count limits**: every rung's last segment is a limit word (`None`, `MaxOne` to `MaxTen`, `Unlimited`) → Moral, ordered from the lowest limit to `Unlimited`.
  A limit is not a value claim, so there is no baseline rung: neutral always comes from `NeutralOfDefaults`, and a count ladder with no default rung open to any faith is flagged.
  Two rungs with the same limit → flag.
  Limit words mixed with stance words stay unknown and are flagged.
  Added for Way Better Romance (`LoverCount_*`, 2026-11-07); it also reproduces the curated `SpouseCount_*` ladders.

The stance word is the lexicon word that ends the last `_` segment of the defName, matched case-sensitively, so `PersonalMountsDesired` gives `Desired`.
A qualifier ending in `Not`/`Non`/`No`/`Anti` before the word reverses it, so the rung counts as unknown.
The lexicon (intensities from +3 to −3) is in `Source/Precepts/LadderHeuristic.cs`.

## Files

- `Source/Precepts/LadderHeuristic.cs`: the classifier itself.
  `HeuristicRung(DefName, DefaultWeight, HasEffects, NeedsMeme)` goes in; `LadderGuess(Order, NeutralRank, Flags)` comes out, with `IsConfident` true when there are no flags.
  It reads no game state, so tests run it directly.
- `PreceptLadder.Rungs` and `DontCareRank` and `PreceptPolicy.ResolveCategory` read `ConfidentGuess` after their tables.
- The "Log issue ladders" debug action shows the decision and flags for each issue.
- `Source/Precepts/PreceptPolicy.Heuristic.cs`: connects the classifier to the live defs (`GuessFor`, `ConfidentGuess`, `HeuristicRungOf`) and writes the log text.
  `HeuristicRungOf` reads the same effect fields as `EFFECT_FIELDS` in the catalog script; keep the two the same.
- `tests/LadderHeuristicWiringTests.cs`: table priority, flagged issues, each effect field, saved ladders and the startup report.
- `Source/Precepts/PreceptPolicy.cs`: `IsCurated(defName)` is true when any table (Moral, UniversalPositive, Special, NA, KnownPositiveOnly, DuplicateOf) lists the issue.
- `scripts/meme_precept_catalog.py`: `make catalog` also writes `tests/Fixtures/ladders.json`, which holds every installed issue with 2 or more visible rungs (171 now).
  "Has effects" means the rung's XML sets comps, abilityStatFactors, statOffsets, statFactors, conditionalStatAffecters, opposedWorkTypes, enablesNeeds or disablesNeeds.
  Effects that a mod adds in C# are not visible (the Mount Ownership rungs read as having no effects).
- `tests/LadderHeuristicBacktest.cs`: the gate.
  It loads the fixture into the shim's `DefDatabase`, so the hand-made answers come from the real `PreceptLadder` / `PreceptPolicy` code.
  It compares the category, the order (or its reverse) and the gap that neutral sits in (before, at rung N, between N and N+1, after).
  Neutral counts as a match when it sits in the same gap, so `Before(rank 0)` = −0.5 and the default −1 agree.
- `tests/EnhancedIdeology.Tests.csproj`: includes `LadderHeuristic.cs` and copies the fixture to the output folder.

Show the full report:

```bash
dotnet test tests/EnhancedIdeology.Tests.csproj --filter LadderHeuristicBacktest --logger "console;verbosity=detailed"
```

The last run is saved in `cache/backtest.txt`.

## Backtest results

The 164 hand-classified multi-rung ladders that are installed locally (2026-11-09):

| outcome | count |
| --- | --- |
| agrees with the tables | 68 |
| confidently wrong | 0 |
| flagged | 96 |
| not classified by hand, heuristic would classify | 6 |
| not classified by hand, heuristic flags | 1 (`ROBTRG_Repopulation`, RJW only, deliberately left alone) |

The gate is green.
Every multi-rung issue that the tables keep as PositiveOnly, Special or NA was flagged, except `ARIOIAF_Royalty` (below).

### How the first 2 wrong answers were settled

- **`RS_Issue_Pregnancy`**: `PreceptLadder.DefaultRank` now uses the highest-weight rung instead of the weighted mean, so the live code and the heuristic share one rule (`NeutralOfDefaults`), and the `DontCare` table entry is gone.
  This also moved `SEX_Divorce` (0.09 → 0), `SEX_Incest` (0.05 → 0) and `BS_AlienAppearanceTolerance` (1.85 → 2) in the game, onto the rungs that `preceptPolicy.md` names.
- **`ARIOIAF_Royalty`**: the heuristic cannot see `DuplicateOf`, so the gate skips the category check for copies (`PreceptPolicy.IsDuplicate`) and still compares order and neutral.
  Both rungs have default weights but need the `ARIOIAF_Feudalism` meme, so the heuristic ignores those weights when every default rung needs a meme.

The blanket rule, which ignores every meme-locked default rung, is deliberately not used.
It would put `Skullspike` at `Disapproved`, but `preceptPolicy.md` accepts `ARIOIAF_Skullspike_DontCare` moving that neutral.

### Experiments

- Adding obvious value words seen as unknown (Holy, Blessed, Amazing, Craved, Liked, Welcomed, Valued, DontMind, Disrespected, Troubling, Ugly, Scorned, Taboo, Hated, Condemned, Despicable, Abhorred, Depraved, Abomination, Hatred, Sacrilegious) raised the agreements from 54 to 64 with no new wrong answers. **Kept.**
  Caveat: the words came from this same curated set, so the count is optimistic; the next new mod is the real test.
- Letting a 0-intensity rung beat the default rungs fixed `RS_Issue_Pregnancy` and broke 7 others (`Corpses`, `AM_Creep`, `AM_Disfigurement`, `HAR_AlienSlavery`, `Skullspike`, `VME_TaintedApparel`, `VME_TatteredApparel`). **Reverted.**

### What the flags are

- 14 same-intensity ties, mostly between rungs from different mods: `Raiding_Respected` vs `VME_Raiding_Honorable`, `Slavery_Abhorrent` vs `VME_Slavery_Forbidden`, three `FungusEating_DontCare` rungs, and `MI_Trees_Mildly_Desired` vs `Trees_Desired` (the `Mildly` modifier is in an earlier segment, and the heuristic ignores it).
- About 20 mechanical issues, as decided.
- The rest have unknown words, mostly on ladders that really need a person: MarriageName, Lovin, Compassion, the Proselytizing frequencies, VXF strictness, gendered RS rungs.

### Issues not classified by hand that the heuristic classifies (confirmed good)

- `GS_GraceTouchedIssue`: Revered — neutral — Reviled
- `NikolaisIdeology_Aphrodors`, `NikolaisIdeology_Neutors`: Sacred → Elevated — neutral — Reviled
- `LoverCount_Male`, `LoverCount_Female` (Way Better Romance, R5): [None] → MaxOne → MaxTwo → MaxThree → MaxFour → Unlimited

## Next steps

1. Show auto-classified issues in `docs/meme-precept-catalog.md`: the script cannot run the C# heuristic, so these issues still show "PositiveOnly (default)".
2. Optional cleanup: hand calls on the mechanical issues are still inconsistent (`VME_BookReadingSpeed` is Moral, `VME_CraftingSpeed` is PositiveOnly).
3. When the flags on a new mod ask for it:
   - break cross-mod ties by `displayOrderInIssue` instead of flagging them (no unclassified issue needs it today; the backtest judges the change);
   - handle strength modifiers (`Mildly_Desired`, `Abhorrent_Strict`, `SpouseOnly_Mild`).
