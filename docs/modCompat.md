# Mod compatibility: composed precept / issue ladders

Issue-centric, **merged across all loaded sources** (RimWorld Core/Royalty/Ideology/Biotech/Odyssey, then
Alpha Memes and VIE - Memes & Structures). Each issue shows its full stacked ladder; ranks are the sorted
position by `displayOrderInIssue` (unset = treated as 0 at runtime).

- **STACKED** = rungs from >1 source (ladder depends on load order). **Optional** = no rung has dsw>0 (ideo
  can be silent). rungs with `order = —` have no `displayOrderInIssue`.

## ⚠ Key finding: stacking can scramble the semantic axis

`displayOrderInIssue` orders rungs along the belief axis *within a single source* but not always across a
stack seam. `AnimalSlaughter` (Alpha Memes' `desired` appended above `prohibited`) and `MeatEating`
(`vegetarian disliked` next to `abhorrent`) are scrambled; `NutrientPasteEating`/`Apostasy` stack cleanly
via deliberate negative/in-between orders. Scrambled ones need a per-issue semantic remap, not just a
Don't-care rank.

## Base-game / DLC issues (not extended by Alpha Memes or VIE)

45 issue(s) shipping with Core/DLC that neither mod touches (optional and mandatory both — e.g.
`Cannibalism`, `Execution`, `Slavery` are mandatory but listed so the reference is complete).

### `AgeReversal` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | AgeReversal_Demanded | Demanded | — |

### `AnimalConnection` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | AnimalConnection_Strong | strong | — |

### `AnimalsVenerated` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | AnimalVenerated | venerated | — |

### `ApparelDesire` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | ApparelDesired_Strong_Subordinate | strong | — |
| 1 | — | Ideology | ApparelDesired_Soft_Subordinate | relaxed | — |

### `Biosculpting` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | Biosculpting_Accelerated | Accelerated | — |
| 1 | 10 | Ideology | BioSculpter_Despised | despised | — |

### `BlindPsysense` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | Blind_Psysense | strong | 0 |

### `Blindness` — mandatory (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | Blinding_Horrible | horrible | 1 |
| 1 | 10 | Ideology | Blindness_Sublime | sublime | 0 |
| 2 | 20 | Ideology | Blindness_Elevated | elevated | 0 |
| 3 | 30 | Ideology | Blindness_Respected | respected | 0 |

### `Bloodfeeders` — optional (Biotech)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Biotech | Bloodfeeders_Revered | revered | — |
| 1 | — | Biotech | Bloodfeeders_Reviled | reviled | — |

### `Cannibalism` — mandatory (Core, Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | Cannibalism_RequiredStrong | required (strong) | — |
| 1 | 10 | Ideology | Cannibalism_Preferred | preferred | — |
| 2 | 20 | Ideology | Cannibalism_Acceptable | acceptable | — |
| 3 | 30 | Ideology | Cannibalism_Disapproved | disapproved | — |
| 4 | 40 | Ideology | Cannibalism_Horrible | horrible | 1 |
| 5 | 50 | Ideology | Cannibalism_Abhorrent | abhorrent | 1 |
| 6 | — | Core | Cannibalism_Classic |  | — |
| 7 | — | Ideology | Cannibalism_RequiredRavenous | required (ravenous) | — |

### `Charity` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | Charity_Essential | essential | — |
| 1 | 10 | Ideology | Charity_Important | important | — |
| 2 | 20 | Ideology | Charity_Worthwhile | worthwhile | — |

### `ChildLabor` — optional (Biotech)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Biotech | ChildLabor_Encouraged | encouraged | — |
| 1 | — | Biotech | ChildLabor_Disapproved | disapproved | — |

### `Comfort` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | Comfort_Ignored | ignored | — |

### `DrugUse` — STACKED, optional, OrderOverride applied
sources: Ideology, MortPolitical

Semantic axis (post-override): Essential → MI_DrugUse_Allowed → MedicalOrSocial → MedicalOnly → Prohibited → Abhorrent

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | DrugUse_Essential | essential | — |
| 1 | 10 | Ideology | DrugUse_MedicalOrSocial | medical or social only | — |
| 2 | 20 | Ideology | DrugUse_MedicalOnly | medical only | — |
| 3 | 30 | Ideology | DrugUse_Prohibited | prohibited | — |
| 4 | 40 | MortPolitical | MI_DrugUse_Allowed | allowed | — |
| 5 | — | Ideology | DrugUse_Abhorrent | abhorrent strict | — |

Currently inert: Political Compass (MortPolitical) is downloaded but not enabled in this install, and
DrugUse_Abhorrent is commented out in vanilla XML (never loaded). Harmless - the override still resolves
correctly for whichever rungs are actually active - but don't take this table as proof either rung exists.

### `Execution` — mandatory (Core, Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | Execution_Required | required | — |
| 1 | 10 | Ideology | Execution_RespectedIfGuilty | respected if guilty | — |
| 2 | 20 | Ideology | Execution_DontCare | don't care | — |
| 3 | 30 | Ideology | Execution_HorribleIfInnocent | horrible if innocent | 1 |
| 4 | 40 | Ideology | Execution_Horrible | always horrible | — |
| 5 | 50 | Ideology | Execution_Abhorrent | always abhorrent | — |
| 6 | — | Core | Execution_Classic |  | — |

### `Fishing` — optional (Odyssey)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Odyssey | Fishing_Prohibited | prohibited | — |
| 1 | 10 | Odyssey | Fishing_Disapproved | disapproved | — |
| 2 | 30 | Odyssey | Fishing_Sacred | sacred | — |

### `GrowthVat` — optional (Biotech)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Biotech | GrowthVat_Essential | essential | — |
| 1 | — | Biotech | GrowthVat_Prohibited | prohibited | — |

### `IdeoBuilding` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | IdeoBuilding | building | — |

### `IdeoDiversity` — mandatory (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | IdeoDiversity_Exalted | exalted | — |
| 1 | 10 | Ideology | IdeoDiversity_Respected | highly appreciated | — |
| 2 | 20 | Ideology | IdeoDiversity_Approved | appreciated | — |
| 3 | 30 | Ideology | IdeoDiversity_Standard | neutral | 1 |
| 4 | 40 | Ideology | IdeoDiversity_Disapproved | mild bigotry | — |
| 5 | 50 | Ideology | IdeoDiversity_Horrible | moderate bigotry | — |
| 6 | 60 | Ideology | IdeoDiversity_Abhorrent | intense bigotry | — |

### `IdeoRelic` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | IdeoRelic | relic | — |

### `IdeoRitualSeat` — mandatory (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | IdeoRitualSeat | ritual seat | 1 |

### `Indoors` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | Indoors_Acceptable | preferred | — |

### `KillingInnocentAnimals` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | KillingInnocentAnimals_Disapproved | disapproved | — |
| 1 | 10 | Ideology | KillingInnocentAnimals_Horrible | horrible | — |
| 2 | 20 | Ideology | KillingInnocentAnimals_Abhorrent | abhorrent | — |

### `Lovin` — mandatory (Core, Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Core | Lovin_Free | free | 1 |
| 1 | 0 | Ideology | Lovin_FreeApproved | free and approved | — |
| 2 | 10 | Ideology | Lovin_SpouseOnly_Mild | spouse only (mild) | 1 |
| 3 | 20 | Ideology | Lovin_SpouseOnly_Moderate | spouse only (moderate) | 1 |
| 4 | 30 | Ideology | Lovin_SpouseOnly_Strict | spouse only (strict) | 1 |
| 5 | 40 | Ideology | Lovin_Horrible | horrible | — |
| 6 | 50 | Ideology | Lovin_Prohibited | prohibited | — |

### `MarriageName` — mandatory (Core, Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Core | MarriageName_UsuallyMans | usually man's | 1 |
| 1 | 0 | Ideology | MarriageName_Random | random | 1 |
| 2 | 10 | Ideology | MarriageName_KeepNames | keep names | 1 |
| 3 | 20 | Ideology | MarriageName_UsuallyWomans | usually woman's | — |
| 4 | 30 | Ideology | MarriageName_AlwaysWomans | always woman's | — |
| 5 | 40 | Ideology | MarriageName_AlwaysMans | always man's | — |

### `MechanoidLabor` — optional (Biotech)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Biotech | MechanoidLabor_Enhanced | enhanced | — |

### `Mining` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | Mining_Disapproved | disapproved | — |
| 1 | 10 | Ideology | Mining_Horrible | horrible | — |
| 2 | 20 | Ideology | Mining_Prohibited | prohibited | — |

### `MiningYield` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | MiningYield_High | high | — |

### `NeuralSupercharge` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | NeuralSupercharge_Preferred | preferred | — |

### `Nomadic` — optional (Odyssey)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Odyssey | Nomadic_Preferred | desired | — |

### `Nudity_Female` — mandatory (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | Nudity_Female_UncoveredGroinChestHairOrFaceDisapproved | totally covered | — |
| 1 | 20 | Ideology | Nudity_Female_UncoveredGroinChestOrHairDisapproved | pants, shirt, and hat | — |
| 2 | 40 | Ideology | Nudity_Female_UncoveredGroinOrChestDisapproved | pants and shirt | 1 |
| 3 | 60 | Ideology | Nudity_Female_UncoveredGroinDisapproved | pants | — |
| 4 | 80 | Ideology | Nudity_Female_NoRules | no rules | — |
| 5 | 100 | Ideology | Nudity_Female_CoveringAnythingButGroinDisapproved | pants at most | — |
| 6 | 120 | Ideology | Nudity_Female_Mandatory | fully nude | — |
UncoveredGroinOrChestDisapproved is classic-only; order is already monotonic once it's kept (no
OrderOverride needed). Fixed via `PreceptPolicy.IncludeClassicInLadder`. Ranks 7-8 above are commented out
in the actual vanilla XML (never loaded) - stale rows, removed.

### `Nudity_Male` — mandatory (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | Ideology | Nudity_Male_UncoveredGroinChestHairOrFaceDisapproved | totally covered | — |
| 1 | 30 | Ideology | Nudity_Male_UncoveredGroinChestOrHairDisapproved | pants, shirt, and hat | — |
| 2 | 50 | Ideology | Nudity_Male_UncoveredGroinOrChestDisapproved | pants and shirt | — |
| 3 | 70 | Ideology | Nudity_Male_UncoveredGroinDisapproved | pants | 1 |
| 4 | 90 | Ideology | Nudity_Male_NoRules | no rules | — |
| 5 | 110 | Ideology | Nudity_Male_CoveringAnythingButGroinDisapproved | pants at most | — |
| 6 | 130 | Ideology | Nudity_Male_Mandatory | fully nude | — |
UncoveredGroinDisapproved is classic-only; order is already monotonic once it's kept (no OrderOverride
needed). Fixed via `PreceptPolicy.IncludeClassicInLadder`. Ranks 7-8 above are commented out in the actual
vanilla XML (never loaded) - stale rows, removed.

### `PreferredXenotypes` — optional (Biotech)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Biotech | PreferredXenotype | preferred | — |

### `Research` — mandatory (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | Research_VeryFast | very fast | — |
| 1 | 10 | Ideology | Research_Fast | fast | — |
| 2 | 20 | Ideology | Research_Normal | normal | 1 |
| 3 | 30 | Ideology | Research_Slow | slow | — |
| 4 | 40 | Ideology | Research_VerySlow | very slow | — |
| 5 | 50 | Ideology | Research_ExtremelySlow | extremely slow | — |
| 6 | 60 | Ideology | Research_None | not allowed | — |

### `Scarification` — mandatory (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | Scarification_Extreme | extreme | — |
| 1 | 10 | Ideology | Scarification_Heavy | heavy | — |
| 2 | 20 | Ideology | Scarification_Minor | minor | — |
| 3 | 30 | Ideology | Scarification_Horrible | horrible | 1 |

### `Skullspike` — mandatory (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | Skullspike_Desired | desired | — |
| 1 | 10 | Ideology | Skullspike_Disapproved | disapproved | 1 |

### `SlabBed` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | SlabBed_Preferred | preferred | — |

### `SlaveTrading` — optional (Core)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Core | Slavery_Classic |  | — |

### `SleepAccelerator` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | SleepAccelerator_Preferred | preferred | — |

### `SpaceHabitat` — optional (Odyssey)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 30 | Odyssey | SpaceHabitat_Preferred | preferred | — |

### `SpouseCount_Female` — mandatory (Core, Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Core | SpouseCount_Female_MaxOne | one only | 1 |
| 1 | 0 | Ideology | SpouseCount_Female_Unlimited | unlimited | — |
| 2 | 20 | Ideology | SpouseCount_Female_MaxFour | four or fewer | — |
| 3 | 40 | Ideology | SpouseCount_Female_MaxThree | three or fewer | — |
| 4 | 60 | Ideology | SpouseCount_Female_MaxTwo | two or fewer | — |

MaxOne is classic-only. The `PreceptPolicy.OrderOverrides` entry for this issue was already correct but
was dead code (classic dropped before the override ran) until `IncludeClassicInLadder` fixed it.

### `SpouseCount_Male` — mandatory (Core, Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Core | SpouseCount_Male_MaxOne | one only | 1 |
| 1 | 10 | Ideology | SpouseCount_Male_Unlimited | unlimited | — |
| 2 | 30 | Ideology | SpouseCount_Male_MaxFour | four or fewer | — |
| 3 | 50 | Ideology | SpouseCount_Male_MaxThree | three or fewer | — |
| 4 | 70 | Ideology | SpouseCount_Male_MaxTwo | two or fewer | — |

Same as SpouseCount_Female: MaxOne is classic-only, override was dead code until IncludeClassicInLadder.

### `Temperature` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 20 | Ideology | Temperature_Tough | tough | — |

### `TreeCutting` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | TreeCutting_Disapproved | disapproved | — |
| 1 | 10 | Ideology | TreeCutting_Horrible | horrible | — |
| 2 | 20 | Ideology | TreeCutting_Prohibited | prohibited | — |

### `Weapons` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | NobleDespisedWeapons | noble and despised | — |

### `WorkDrive` — optional (Ideology)

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | WorkDrive_Tripled | tripled | — |

## Issues added or extended by the mods

131 issue(s) the mods contribute rungs to.

### `AM_Abilities` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | AlphaMemes | AM_Abilities_AnalyzeCreature | analyze creature | — |
| 1 | 20 | AlphaMemes | AM_Abilities_DeathKnell | death knell | — |

### `AM_AnimaScreams` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_AnimaScreams_Delightful | delightful | — |

### `AM_AnimalAnalysis` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 20 | AlphaMemes | AM_AnimalAnalysis_Expected | expected | — |

### `AM_AnimalRelease` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | AlphaMemes | AM_AnimalRelease_Discouraged | discouraged | — |
| 1 | 20 | AlphaMemes | AM_AnimalRelease_Encouraged | encouraged | — |

### `AM_AnimalsDespised` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_AnimalDespised | despised | — |

### `AM_Armour` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_Armour_Blunt | blunt | — |
| 1 | — | AlphaMemes | AM_Armour_Sharp | sharp | — |
| 2 | — | AlphaMemes | AM_Armour_Heat | heat | — |

### `AM_Art` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_Art_Desired | desired | — |

### `AM_ArtProductionSpeed` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_ArtProductionSpeed_Increased | increased | — |

### `AM_ArtQuality` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_ArtQuality_Expected | expected | — |

### `AM_Barracks` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_Barracks_Preferred | monastic | — |
| 1 | — | AlphaMemes | AM_Barracks_PreferredTrue | preferred | — |

### `AM_Baths` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | AlphaMemes | AM_Baths_Desired | desired | — |

### `AM_Cattle` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_Cattle_Improved | improved | — |

### `AM_CombatProwess` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | AlphaMemes | AM_CombatProwess_Melee | melee | — |
| 1 | 20 | AlphaMemes | AM_CombatProwess_Increased | distance | — |
| 2 | 30 | AlphaMemes | AM_CombatProwess_Reduced | reduced | — |

### `AM_Creep` — single-source, mandatory
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | AlphaMemes | AM_Creep_Disliked | disliked | 1 |
| 1 | 10 | AlphaMemes | AM_Creep_DontCare | don't care | — |
| 2 | 20 | AlphaMemes | AM_Creep_Sublime | sublime | — |

### `AM_Death` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | AlphaMemes | AM_Death_Desired | desired | — |

### `AM_DeathrestCaskets` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_DeathrestCaskets_Abhorrent | abhorrent | — |

### `AM_Disfigurement` — single-source, mandatory
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | AlphaMemes | AM_Disfigurement_Disliked | disliked | 1 |
| 1 | 10 | AlphaMemes | AM_Disfigurement_DontCare | don't care | — |

### `AM_Dryads` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 20 | AlphaMemes | AM_Dryads_Enhanced | enhanced | — |

### `AM_FertilityIssue` — single-source, mandatory
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | -10 | AlphaMemes | AM_FertilityIssue_Decreased | decreased | — |
| 1 | 0 | AlphaMemes | AM_FertilityIssue_Normal | normal | 1 |
| 2 | 10 | AlphaMemes | AM_FertilityIssue_Increased | increased | — |

### `AM_HarbingerTrees` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_HarbingerTrees_Disgusting | disgusting | — |

### `AM_Horses` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_Horses_Desired | desired | — |

### `AM_HuntFocus` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | AlphaMemes | AM_HuntFocus_Sanguophage | sanguophage | — |
| 1 | 10 | AlphaMemes | AM_HuntFocus_Strigoi | strigoi | — |
| 2 | 10 | AlphaMemes | AM_HuntFocus_Malachai | malachai | — |
| 3 | 20 | AlphaMemes | AM_HuntFocus_Ekkimian | ekkimian | — |
| 4 | 30 | AlphaMemes | AM_HuntFocus_Bruxa | bruxa | — |

### `AM_Hydroagriculture` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_Hydroagriculture_Revered | revered | — |

### `AM_KitchenProficiency` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_KitchenProficiency_Extreme | extreme | — |

### `AM_LearningRate` — single-source, mandatory
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | -10 | AlphaMemes | AM_LearningRate_Decreased | decreased | — |
| 1 | 0 | AlphaMemes | AM_LearningRate_Normal | normal | 1 |
| 2 | 10 | AlphaMemes | AM_LearningRate_Increased | increased | — |

### `AM_LovinFrequency` — single-source, mandatory
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | -10 | AlphaMemes | AM_LovinFrequency_Decreased | decreased | — |
| 1 | 0 | AlphaMemes | AM_LovinFrequency_Normal | normal | 1 |
| 2 | 10 | AlphaMemes | AM_LovinFrequency_Exuberant | exuberant | — |
| 3 | 10 | AlphaMemes | AM_LovinFrequency_Increased | increased | — |

### `AM_Madness` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_Madness_Exalted | exalted | — |

### `AM_MaternalMortality` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_MaternalMortality_Nullified | nullified | — |

### `AM_Meals` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_Meals_QualityDesired | desired | — |

### `AM_Megaliths` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_Megaliths_Desired | desired | — |

### `AM_Mood` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 60 | AlphaMemes | AM_Mood_Volatile | volatile | — |

### `AM_OcularTrees` — single-source, mandatory
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | AlphaMemes | AM_OcularTrees_Indifferent | indifferent | 1 |
| 1 | — | AlphaMemes | AM_OcularTrees_Desired | desired | — |

### `AM_PrefabAcquisition` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | AlphaMemes | AM_PrefabAcquisition_Easy | easy | — |

### `AM_PrefabBuying` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | AlphaMemes | AM_PrefabBuying_Preferred | preferred | — |

### `AM_PsychicSensitivity` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_PsychicSensitivity_Affinity | affinity | — |
| 1 | — | AlphaMemes | AM_PsychicSensitivity_Heightened | increased | — |

### `AM_PsyfocusGain` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_PsyfocusGain_Dampened | dampened | — |

### `AM_Rain` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | -10 | AlphaMemes | AM_Rain_Disliked | disliked | — |
| 1 | 10 | AlphaMemes | AM_Rain_Blessed | blessed | — |

### `AM_RelicDestruction` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_RelicDestruction_Desired | desired | — |

### `AM_Religion` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 20 | AlphaMemes | AM_Religion_Loved | loved | — |
| 1 | 30 | AlphaMemes | AM_Religion_ProselytismDisliked | proselytism disliked | — |
| 2 | 40 | AlphaMemes | AM_Religion_Disliked | disliked | — |
| 3 | 50 | AlphaMemes | AM_Religion_Despised | despised | — |

### `AM_Reliquaries` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_Reliquaries_Forbidden | forbidden | — |

### `AM_Reputation` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_Reputation_Lowered | lowered | — |

### `AM_SanguophageCamps` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_SanguophageCamps_RaidingDesired | raiding desired | — |

### `AM_TableQuality` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_TableQuality_Desired | desired | — |

### `AM_TeaCultivation` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_TeaCultivation_Improved | improved | — |

### `AM_TeaDrinking` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_TeaDrinking_Required | required | — |

### `AM_TeaYield` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | AlphaMemes | AM_TeaYield_Increased | increased | — |

### `AM_Water` — single-source, optional
sources: AlphaMemes

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | AlphaMemes | AM_Water_Desired | desired | — |

### `AnimalSlaughter` — STACKED, optional
sources: AlphaMemes, Ideology

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | AnimalSlaughter_Disapproved | disapproved | — |
| 1 | 10 | Ideology | AnimalSlaughter_Horrible | horrible | — |
| 2 | 20 | Ideology | AnimalSlaughter_Prohibited | prohibited | — |
| 3 | 30 | AlphaMemes | AM_AnimalSlaughter_Desired | desired | — |

### `Apostasy` — STACKED, optional
sources: Ideology, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | -10 | VIE-M&S | VME_Apostasy_Accepted | accepted | — |
| 1 | 0 | Ideology | Apostasy_Disapproved | disapproved | — |
| 2 | 10 | Ideology | Apostasy_Horrible | horrible | — |
| 3 | 19 | VIE-M&S | Apostasy_Despicable | despicable | — |
| 4 | 20 | Ideology | Apostasy_Abhorrent | abhorrent | — |

### `AutonomousWeapons` — STACKED, optional
sources: Ideology, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | -20 | VIE-M&S | VME_AutonomousWeapons_Exalted | exalted | — |
| 1 | -10 | VIE-M&S | VME_AutonomousWeapons_Accepted | accepted | — |
| 2 | 0 | Ideology | AutonomousWeapons_Disapproved | disapproved | — |
| 3 | 10 | Ideology | AutonomousWeapons_Horrible | horrible | — |
| 4 | 20 | Ideology | AutonomousWeapons_Prohibited | prohibited | — |

### `BodyModification` — STACKED, optional, OrderOverride applied
sources: Ideology, MortPolitical, VIE-M&S

Semantic axis (post-override): Approved → MI_BodyMod_Allowed → OnlyBiological → Disapproved → Abhorrent

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | BodyMod_Approved | approved | — |
| 1 | 10 | Ideology | BodyMod_Disapproved | disapproved | — |
| 2 | 20 | Ideology | BodyMod_Abhorrent | abhorrent | — |
| 3 | 20 | VIE-M&S | VME_BodyMod_OnlyBiological | only biological | — |
| 4 | 30 | MortPolitical | MI_BodyMod_Allowed | allowed | — |

Currently inert: Political Compass (MortPolitical) is downloaded but not enabled in this install. Harmless -
the override still resolves correctly for the active rungs.

### `Bonding` — STACKED, optional
sources: AlphaMemes, Ideology

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | Bonding_Disapproved | disapproved | — |
| 1 | — | AlphaMemes | AM_Bonding_Abhorrent | abhorrent | — |

Both default to order 0 (tied) - fixed via `PreceptPolicy.OrderOverrides`.

### `Corpses` — STACKED, mandatory
sources: AlphaMemes, Core, Ideology

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | Core | Corpses_Ugly | ugly | 1 |
| 1 | 10 | Ideology | Corpses_DontCare | don't care | — |
| 2 | 20 | AlphaMemes | AM_Corpses_Sublime | sublime | — |

Ugly is classic-only and ties with DontCare at order 10; both were a live bug (classic dropped, tie
unresolved). Fixed via `PreceptPolicy.OrderOverrides` + `IncludeClassicInLadder`.

### `DarknessCombat` — STACKED, optional
sources: Ideology, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | DarknessCombat_Preferred | Preferred | — |
| 1 | 10 | VIE-M&S | VME_DarknessCombat_Despised | despised | — |

### `Eclipse` — STACKED, optional
sources: Ideology, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | Eclipse_Beautiful | Beautiful | — |
| 1 | — | VIE-M&S | VME_Eclipse_Despised | despised | — |

Both default to order 0 (tied) - fixed via `PreceptPolicy.OrderOverrides`.

### `FungusEating` — STACKED, mandatory
sources: AlphaMemes, Ideology, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | AlphaMemes | AM_FungusEating_Required | required | — |
| 1 | 0 | Ideology | FungusEating_Preferred | preferred | — |
| 2 | 5 | VIE-M&S | VME_FungusEating_DontCare | don't care | — |
| 3 | 10 | Ideology | FungusEating_Despised | despised | 1 |

Missing an AlphaMemes row (AM_FungusEating_Required) that ties with Preferred at order 0 - fixed via
`PreceptPolicy.OrderOverrides`.

### `GauranlenConnection` — STACKED, optional
sources: AlphaMemes, Ideology

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | GauranlenConnection_Strong | strong | — |
| 1 | — | AlphaMemes | AM_GauranlenConnection_Forbidden | forbidden | — |

Both default to order 0 (tied) - fixed via `PreceptPolicy.OrderOverrides`.

### `InsectMeat` — STACKED, mandatory
sources: Core, Ideology, AlphaMemes, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | AlphaMemes | AM_InsectMeatEating_Required | required | — |
| 1 | 0 | Ideology | InsectMeatEating_Loved | loved | — |
| 2 | 0 | VIE-M&S | VME_InsectMeatEating_DontCare | don't care | — |
| 3 | 10 | Core | InsectMeatEating_Despised_Classic | despised | 1 |
| 4 | 50 | VIE-M&S | VME_InsectMeatEating_Sacrilegious | abhorrent | — |

Three-way tie at order 0 (Required/Loved/DontCare) plus the classic rung being excluded by default made this
a live bug (`Source/Precepts/PreceptLadder.cs`'s classic filter): fixed via `PreceptPolicy.OrderOverrides` +
`IncludeClassicInLadder`.

### `Lighting` — STACKED, optional
sources: Ideology, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | -10 | VIE-M&S | VME_Darklight_NormalPreferred | normal preferred | — |
| 1 | 0 | Ideology | Darklight_Preferred | darklight preferred | — |

### `MeatEating` — STACKED, optional
sources: Ideology, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | MeatEating_Disapproved | disapproved | — |
| 1 | 10 | Ideology | MeatEating_Horrible | horrible | — |
| 2 | 20 | Ideology | MeatEating_Abhorrent | abhorrent | — |
| 3 | 30 | Ideology | MeatEating_NonMeat_Disapproved | vegetarian disliked | — |
| 4 | 30 | VIE-M&S | VME_MeatEating_Abhorrent_Strict | abhorrent (strict) | — |
| 5 | 40 | Ideology | MeatEating_NonMeat_Horrible | vegetarian hated | — |
| 6 | 50 | Ideology | MeatEating_NonMeat_Abhorrent | vegetarian abhorrent | — |

### `NutrientPasteEating` — STACKED, mandatory
sources: AlphaMemes, Core, Ideology

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | -20 | AlphaMemes | AM_NutrientPasteEating_Preferred | preferred | 0 |
| 1 | -10 | AlphaMemes | AM_NutrientPasteEating_Indifferent | indifferent | 0 |
| 2 | 0 | Ideology | NutrientPasteEating_DontMind | don't mind | 0 |
| 3 | 10 | Core | NutrientPasteEating_Disgusting | disgusting | 1 |
| 4 | 30 | AlphaMemes | AM_NutrientPasteEating_Forbidden | forbidden | 0 |

Disgusting is classic-only; order is already monotonic once it's kept in the ladder (no OrderOverride
needed). Fixed via `PreceptPolicy.IncludeClassicInLadder`.

### `OrganUse` — STACKED, mandatory
sources: AlphaMemes, Core, Ideology, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | OrganUse_Acceptable | acceptable | — |
| 1 | 10 | Ideology | OrganUse_HorribleSellOK | no harvest | 1 |
| 2 | 20 | Ideology | OrganUse_HorribleNoSell | no harvest or sell | — |
| 3 | 30 | Ideology | OrganUse_Abhorrent | totally abhorrent | — |
| 4 | 40 | AlphaMemes | AM_OrganUse_Torturous | torturous | — |
| 5 | 40 | VIE-M&S | VME_OrganUse_PostMortem | post mortem | — |
| 6 | — | Core | OrganUse_Classic |  | — |
| 7 | — | Ideology | OrganUse_Respected | respected | — |

AM_OrganUse_Torturous ("harvesting a still-living enemy's organs... should be encouraged") is a pro-harvest
extreme, not anti - the previous `PreceptPolicy.OrderOverrides` entry wrongly placed it after Abhorrent
(the anti extreme). Fixed to sort ahead of Respected instead.

### `Pain` — STACKED, optional
sources: Ideology, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | Pain_Idealized | idealized | — |
| 1 | — | VIE-M&S | VME_Pain_DontCare | don't care | — |

Also missing an AlphaMemes row (AM_Pain_Required, also order 0, three-way tie). `Pain` is classified
PositiveOnly, not Moral, so `Rungs()` order has no effect on opinion today - left undocumented in
OrderOverrides deliberately (would be dead code); revisit if this issue's category ever changes.

### `Proselytizing` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | -10 | VIE-M&S | VME_Proselytizing_Forceful | forceful | — |
| 1 | 30 | VIE-M&S | VME_Proselytizing_Never | never | — |

### `Raiding` — STACKED, optional
sources: Ideology, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | Raiding_Required | required | — |
| 1 | 10 | Ideology | Raiding_Respected | respected | — |
| 2 | 20 | VIE-M&S | VME_Raiding_Honorable | honorable | — |
| 3 | 40 | VIE-M&S | VME_Raiding_Abhorrent | abhorrent | — |

### `Ranching` — STACKED, optional
sources: AlphaMemes, Ideology, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | VIE-M&S | VME_Ranching_Disliked | disliked | — |
| 1 | 30 | VIE-M&S | VME_Ranching_Nomadic | nomadic | — |
| 2 | — | Ideology | Ranching_Central | central | — |
| 3 | — | AlphaMemes | AM_Ranching_CattleCentered | cattle centered | — |

VME_Ranching_Discouraged (row removed above) belongs to a VME sub-patch for Vanilla Fishing Expanded, which
isn't installed/enabled - not a real loaded def. Central/CattleCentered tie at default order 0, and raw
order otherwise puts the anti rung (Disliked, 10) before the pro rung (Nomadic, 30) - fixed via
`PreceptPolicy.OrderOverrides`.

### `Ritual` — STACKED, optional
sources: AlphaMemes, Biotech, Ideology, Odyssey, Royalty, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Royalty | AnimaTreeLinking | anima tree linking | — |
| 1 | — | Ideology | Conversion | conversion ritual | — |
| 2 | — | Ideology | ScarificationCeremony | scarification | — |
| 3 | — | Ideology | BlindingCeremony | blinding | — |
| 4 | — | Ideology | TreeConnection | tree connection ritual | — |
| 5 | — | Ideology | Execution | public execution | — |
| 6 | — | Ideology | GladiatorDuel | gladiator duel | — |
| 7 | — | Ideology | RoleChange | role change | — |
| 8 | — | Biotech | ChildBirth | child birth | — |
| 9 | — | Odyssey | GravshipLaunch | gravship launch | — |
| 10 | — | AlphaMemes | GR_ExtractorFuneral | extraction funeral | — |
| 11 | — | AlphaMemes | AM_BanquetPrecept | banquet | — |
| 12 | — | AlphaMemes | AM_AnimaBurial | anima tree burial | — |
| 13 | — | AlphaMemes | AM_DreadnoughtFuneral | dreadnought funeral | — |
| 14 | — | AlphaMemes | AM_FleshCraftingFuneral | fleshcrafting funeral | — |
| 15 | — | AlphaMemes | AM_TeaCeremonyPrecept | tea ceremony | — |
| 16 | — | AlphaMemes | AM_BaptismPrecept | baptism | — |
| 17 | — | AlphaMemes | AM_RelicDestructionPrecept | relic destruction | — |
| 18 | — | AlphaMemes | AM_SparringMatch | sparring match | — |
| 19 | — | AlphaMemes | AM_OcularWarping | ocular warping | — |
| 20 | — | AlphaMemes | AM_MaddeningChantPrecept | maddening chant | — |
| 21 | — | AlphaMemes | AM_ScrapRitual | scrapping ritual | 0 |
| 22 | — | AlphaMemes | AM_TantrumRitual | tantrum ritual | 0 |
| 23 | — | AlphaMemes | AM_CremateFuneral | cremation funeral | — |
| 24 | — | AlphaMemes | AM_PastedFuneral | nutrient paste funeral | — |
| 25 | — | AlphaMemes | AM_RumBurial | rum burial | — |
| 26 | — | AlphaMemes | AM_FuneralNoCorpse | funeral (no corpse) | — |
| 27 | — | AlphaMemes | AM_SkyBurial | sky burial | — |
| 28 | — | AlphaMemes | AM_BlastOffFuneral | blast off funeral | — |
| 29 | — | AlphaMemes | AM_InsectoidBurial | hive burial | — |
| 30 | — | AlphaMemes | AM_PyramidBurial | pyramid burial funeral | — |
| 31 | — | AlphaMemes | AM_OcularFuneral | ocular funeral | — |
| 32 | — | AlphaMemes | AM_Mummification | mummification funeral | — |
| 33 | — | VIE-M&S | VME_CeremonialSuicidePrecept | ceremonial suicide | — |
| 34 | — | VIE-M&S | VME_LeadershipChallengePrecept | leadership challenge | — |
| 35 | — | VIE-M&S | VME_OrgyPrecept | orgy | — |
| 36 | — | VIE-M&S | VME_PlagueFestivalPrecept | plague festival | — |
| 37 | — | VIE-M&S | VME_ViolentConversionPrecept | violent conversion | — |
| 38 | — | VIE-M&S | VME_DivineStarsPrecept | divine the stars | — |
| 39 | — | VIE-M&S | VME_WickerManBurningPrecept | wicker man burning | — |
| 40 | — | VIE-M&S | VME_IncantationPrecept | incantation | — |
| 41 | — | VIE-M&S | VME_SlaveEmancipationPrecept | slave emancipation | — |
| 42 | — | VIE-M&S | VME_LeaderConversion | conversion ritual (leader) | — |
| 43 | — | VIE-M&S | VME_TradingFairPrecept | trading fair | — |
| 44 | — | VIE-M&S | VME_InsectoidHymnPrecept | insectoid hymn | — |
| 45 | — | VIE-M&S | VME_BonfirePrecept | bonfire | — |

### `RoughLiving` — STACKED, optional
sources: AlphaMemes, Ideology

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | AlphaMemes | AM_RoughLiving_Disliked | shunned | — |
| 1 | 20 | Ideology | RoughLiving_Welcomed | welcomed | — |

### `Slavery` — STACKED, mandatory
sources: Ideology, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Ideology | Slavery_Honorable | honorable | — |
| 1 | 10 | Ideology | Slavery_Acceptable | acceptable | 1 |
| 2 | 20 | Ideology | Slavery_Disapproved | disapproved | 1 |
| 3 | 30 | Ideology | Slavery_Horrible | horrible | 1 |
| 4 | 40 | Ideology | Slavery_Abhorrent | abhorrent | 1 |
| 5 | 50 | VIE-M&S | VME_Slavery_Forbidden | forbidden | — |

### `Trees` — STACKED, optional
sources: AlphaMemes, Ideology

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | Ideology | Trees_Desired | desired | — |
| 1 | — | AlphaMemes | AM_Trees_Despised | despised | — |

Both default to order 0 (tied) - fixed via `PreceptPolicy.OrderOverrides`.

### `VFEA_BeingRecruited` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | VIE-M&S | VFEA_BeingRecruited_Forbidden | forbidden | — |

### `VFEA_Recruiting` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | VIE-M&S | VFEA_Recruiting_Forbidden | forbidden | — |

### `VME_Alcohol` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | VIE-M&S | VME_Alcohol_Demanded | demanded | — |
| 1 | 10 | VIE-M&S | VME_Alcohol_Desired | desired | — |
| 2 | 20 | VIE-M&S | VME_Alcohol_MildAbstinence | mild abstinence | — |

### `VME_Anonymity` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 50 | VIE-M&S | VME_Anonymity_Required | required | — |

### `VME_Aurora` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_Aurora_Amazing | amazing | — |
| 1 | — | VIE-M&S | VME_Aurora_Despised | despised | — |

### `VME_AutomationEfficiency` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_AutomationEfficiency_Increased | increased | — |
| 1 | — | VIE-M&S | VME_AutomationEfficiency_Decreased | decreased | — |

### `VME_BookQuality` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | VIE-M&S | VME_BookQuality_Desired | desired | — |

### `VME_BookReading` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_BookReading_Desired | desired | — |
| 1 | — | VIE-M&S | VME_BookReading_Disliked | disliked | — |

### `VME_BookReadingSpeed` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | VIE-M&S | VME_BookReadingSpeed_Increased | increased | — |
| 1 | 10 | VIE-M&S | VME_BookReadingSpeed_Decreased | decreased | — |

### `VME_BookWriting` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_BookWriting_Exalted | exalted | — |
| 1 | — | VIE-M&S | VME_BookWriting_Disliked | disliked | — |

### `VME_BookWritingSpeed` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | VIE-M&S | VME_BookWritingSpeed_Increased | increased | — |
| 1 | 10 | VIE-M&S | VME_BookWritingSpeed_Decreased | decreased | — |

### `VME_Corruption` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 50 | VIE-M&S | VME_Corruption_Essential | essential | — |

### `VME_CraftingQuality` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_CraftingQuality_Increased | increased | — |
| 1 | — | VIE-M&S | VME_CraftingQuality_Decreased | decreased | — |

### `VME_CraftingSpeed` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_CraftingSpeed_Slower | slower | — |
| 1 | — | VIE-M&S | VME_CraftingSpeed_SlowerForManual | slower for manual | — |
| 2 | — | VIE-M&S | VME_CraftingSpeed_FasterForManual | faster for manual | — |

### `VME_Death` — single-source, mandatory
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 30 | VIE-M&S | VME_Death_Troubling | troubling | — |
| 1 | 40 | VIE-M&S | VME_Death_Normal | normal | 1 |
| 2 | 50 | VIE-M&S | VME_Death_DontCare | don't care | — |

### `VME_Defeat` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 50 | VIE-M&S | VME_Defeat_Dishonorable | dishonorable | — |

### `VME_DumbLabor` — single-source, mandatory
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | VIE-M&S | VME_DumbLabor_Exalted | exalted | — |
| 1 | 20 | VIE-M&S | VME_DumbLabor_Liked | liked | — |
| 2 | 30 | VIE-M&S | VME_DumbLabor_Indifferent | indifferent | 1 |
| 3 | 40 | VIE-M&S | VME_DumbLabor_Disliked | disliked | — |
| 4 | 50 | VIE-M&S | VME_DumbLabor_Horrible | horrible | — |

### `VME_Elders` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 30 | VIE-M&S | VME_Elders_Despised | despised | — |
| 1 | 40 | VIE-M&S | VME_Elders_Respected | respected | — |
| 2 | 50 | VIE-M&S | VME_Elders_Holy | holy | — |

### `VME_Expectations` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 20 | VIE-M&S | VME_Expectations_High | high | — |
| 1 | 30 | VIE-M&S | VME_Expectations_Low | low | — |

### `VME_FarmingYield` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_FarmingYield_High | high | — |

### `VME_Fire` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 40 | VIE-M&S | VME_Fire_Despised | despised | — |
| 1 | 50 | VIE-M&S | VME_Fire_Desired | desired | — |

### `VME_Firefighting` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 40 | VIE-M&S | VME_Firefighting_Preferred | preferred | — |
| 1 | 50 | VIE-M&S | VME_Firefighting_Abhorrent | abhorrent | — |

### `VME_FishingYield` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_Fishing_Adept | adept | — |
| 1 | — | VIE-M&S | VME_Fishing_Forbidden | forbidden | — |

### `VME_Hospital` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_Hospital_Required | required | — |

### `VME_Illness` — single-source, mandatory
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 30 | VIE-M&S | VME_Illness_Indifferent | indifferent | 1 |
| 1 | 40 | VIE-M&S | VME_Illness_Preferred | preferred | — |
| 2 | 50 | VIE-M&S | VME_Illness_Exalted | exalted | — |

### `VME_Immunity` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_Immunity_Enhanced | enhanced | — |

### `VME_InsectJelly` — single-source, mandatory
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | -10 | VIE-M&S | VME_InsectJellyEaten_Despised | despised | — |
| 1 | 0 | VIE-M&S | VME_InsectJellyEaten_Neutral_Classic | neutral | 1 |
| 2 | 10 | VIE-M&S | VME_InsectJellyEaten_Loved_Classic | loved | — |

### `VME_Insectoids` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 40 | VIE-M&S | VME_Insectoids_Despised | despised | — |
| 1 | 50 | VIE-M&S | VME_Insectoids_Exalted | exalted | — |

### `VME_Junk` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 40 | VIE-M&S | VME_Junk_Preferred | preferred | — |
| 1 | 50 | VIE-M&S | VME_Junk_Beautiful | beautiful | — |

### `VME_JunkDeconstructionYield` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 50 | VIE-M&S | VME_JunkDeconstructionYield_High | high | — |

### `VME_KillingWithFire` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 40 | VIE-M&S | VME_KillingWithFire_Abhorrent | abhorrent | — |
| 1 | 50 | VIE-M&S | VME_KillingWithFire_Preferred | preferred | — |

### `VME_Leader` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 5 | VIE-M&S | VME_Leader_HighestTitle | highest title | — |
| 1 | 10 | VIE-M&S | VME_Leader_BestPsycaster | best psycaster | — |
| 2 | 25 | VIE-M&S | VME_Leader_Moralist | moralist | — |
| 3 | 30 | VIE-M&S | VME_Leader_Three | three | — |
| 4 | 35 | VIE-M&S | VME_Leader_BestFighter | best fighter | — |
| 5 | 40 | VIE-M&S | VME_Leader_BestCrafter | best crafter | — |
| 6 | 50 | VIE-M&S | VME_Leader_Godlike | godlike | — |

### `VME_LeaderAbilities` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 25 | VIE-M&S | VME_Leader_Moralist | moralist | — |

### `VME_LeaderDivinity` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 50 | VIE-M&S | VME_Leader_Godlike | godlike | — |

### `VME_LeatherApparel` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 20 | VIE-M&S | VME_LeatherApparel_Disliked | disliked | — |
| 1 | 30 | VIE-M&S | VME_LeatherApparel_Abhorrent | abhorrent | — |

### `VME_Library` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_Library_Required | required | — |

### `VME_Mechanoids` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 40 | VIE-M&S | VME_Mechanoids_Despised | despised | — |
| 1 | 50 | VIE-M&S | VME_Mechanoids_Exalted | exalted | — |

### `VME_Meditation` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_Meditation_Exquisite | exquisite | — |

### `VME_Mood` — single-source, mandatory
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | VIE-M&S | VME_Mood_HighExpectations | high spirits | — |
| 1 | 20 | VIE-M&S | VME_Mood_Normal | normal | 1 |
| 2 | 30 | VIE-M&S | VME_Mood_LowExpectations | low spirits | — |
| 3 | 40 | VIE-M&S | VME_Mood_Shared | shared | — |
| 4 | 50 | VIE-M&S | VME_Mood_DictatedByStars | dictated by stars | — |

### `VME_PermanentBases` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_PermanentBases_Despised | despised | — |
| 1 | — | VIE-M&S | VME_PermanentBases_Desired | desired | — |

### `VME_PermitCooldown` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 40 | VIE-M&S | VME_PermitCooldown_Increased | increased | — |
| 1 | 50 | VIE-M&S | VME_PermitCooldown_Lowered | lowered | — |

### `VME_PermitHonorCost` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 40 | VIE-M&S | VME_PermitHonorCost_Doubled | doubled | — |
| 1 | 50 | VIE-M&S | VME_PermitHonorCost_Halved | halved | — |

### `VME_Power` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 30 | VIE-M&S | VME_Power_Preferred | preferred | — |
| 1 | 40 | VIE-M&S | VME_Power_Desired | desired | — |
| 2 | 50 | VIE-M&S | VME_Power_Exalted | exalted | — |

### `VME_PsychicSensitivity` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_PsychicSensitivity_Lowered | lowered | — |
| 1 | — | VIE-M&S | VME_PsychicSensitivity_Heightened | heightened | — |

### `VME_PsyfocusGain` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_PsyfocusGain_Doubled | doubled | — |

### `VME_Recreation` — STACKED, mandatory
sources: AlphaMemes, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | AlphaMemes | AM_Recreation_Abhorrent | abhorrent | — |
| 1 | 20 | VIE-M&S | VME_Recreation_Disapproved | disapproved | — |
| 2 | 30 | VIE-M&S | VME_Recreation_Normal | normal | 1 |
| 3 | 50 | VIE-M&S | VME_Recreation_Loved | loved | — |
| 4 | 60 | VIE-M&S | VME_Recreation_Fishing | fishing | — |

### `VME_Royalty` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 20 | VIE-M&S | VME_Royalty_Hated | hated | — |
| 1 | 30 | VIE-M&S | VME_Royalty_Disliked | disliked | — |
| 2 | 40 | VIE-M&S | VME_Royalty_Respected | respected | — |
| 3 | 50 | VIE-M&S | VME_Royalty_Exalted | exalted | — |

### `VME_Scars` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 40 | VIE-M&S | VME_Scars_Disgusting | disgusting | — |
| 1 | 50 | VIE-M&S | VME_Scars_Honorable | honorable | — |

### `VME_SkilledLabor` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_SkilledLabor_Slow | slow | — |
| 1 | — | VIE-M&S | VME_SkilledLabor_Relaxed | relaxed | — |

### `VME_SlaveTrading` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 50 | VIE-M&S | VME_SlaveTrading_OnlyBuying | only buying | — |

### `VME_SocialInteractions` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 50 | VIE-M&S | VME_SocialInteractions_Disallowed | disallowed | — |

### `VME_Sweets` — single-source, mandatory
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_Sweets_Craved | craved | — |
| 1 | — | VIE-M&S | VME_Sweets_Impartial | impartial | 1 |

### `VME_TaintedApparel` — single-source, mandatory
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 30 | VIE-M&S | VME_TaintedApparel_Abhorrent | abhorrent | — |
| 1 | 40 | VIE-M&S | VME_TaintedApparel_Disapproved | disapproved | 1 |
| 2 | 50 | VIE-M&S | VME_TaintedApparel_DontCare | don't care | — |

### `VME_TatteredApparel` — single-source, mandatory
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 30 | VIE-M&S | VME_TatteredApparel_Abhorrent | abhorrent | — |
| 1 | 40 | VIE-M&S | VME_TatteredApparel_Disapproved | disapproved | 1 |
| 2 | 50 | VIE-M&S | VME_TatteredApparel_DontCare | don't care | — |

### `VME_TendQuality` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_TendQuality_Increased | increased | — |

### `VME_TitleInheritance` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 50 | VIE-M&S | VME_TitleInheritance_Inherited | inherited | — |

### `VME_Trading` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_Trading_Required | required | — |

### `VME_TradingPrice` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_TradingPrice_Improved | improved | — |

### `VME_Travel` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_Travel_Desired | desired | — |
| 1 | — | VIE-M&S | VME_Travel_Despised | despised | — |

### `VME_Violence` — STACKED, mandatory
sources: AlphaMemes, VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 20 | VIE-M&S | VME_Violence_Honorable | honorable | — |
| 1 | 30 | VIE-M&S | VME_Violence_Acceptable | acceptable | 1 |
| 2 | 40 | VIE-M&S | VME_Violence_Disapproved | disapproved | — |
| 3 | 50 | VIE-M&S | VME_Violence_Abhorrent | abhorrent | — |
| 4 | 70 | AlphaMemes | AM_Violence_Abhorrent_Strict | abhorrent (strict) | — |

### `VME_WoodcuttingYield` — single-source, optional
sources: VIE-M&S

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | VIE-M&S | VME_WoodcuttingYield_High | high | — |

## Issues from additional third-party mods

These issues appear only when the listed mod is loaded. Source abbreviations: **MortEnv** = `MortStrudel.MortIdeologyEnv`, **MortPolitical** = `MortStrudel.MortIdeology`, **MortSciFai** = `MortStrudel.MortIdeologySciFai`, **Menagerist** = `MortStrudel.MortIdeologySpecWork`, **QuestingMeme** = `SirMashedPotato.QuestingMeme`, **Waymakers** = `Mx.Waymakers`.

### Mort's Ideologies: Conservationist & Polluter (`MortStrudel.MortIdeologyEnv`)

`MI_Pollution` and `MI_ToxicWasteDumping` are both Moral with both rungs at `displayOrderInIssue=0`; OrderOverride establishes the semantic direction.

#### `MI_CleaningSpeed` — single-source, optional (PositiveOnly)
sources: MortEnv

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | — | MortEnv | MI_CleaningSpeed_Enhanced | enhanced | — |

#### `MI_Pollution` — single-source, **Moral**, OrderOverride applied
sources: MortEnv

Semantic axis: Preferred → (DontCare) → Despised

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | MortEnv | MI_Pollution_Preferred | preferred | — |
| 1 | 0 | MortEnv | MI_Pollution_Despised | despised | — |

#### `MI_PowerGeneration` — single-source, optional (PositiveOnly)
sources: MortEnv

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 20 | MortEnv | MI_PowerGeneration_CleanOnly | clean power only | — |
| 1 | 20 | MortEnv | MI_PowerGeneration_NoTox | no toxifier generators | — |

#### `MI_ToxicWasteDumping` — single-source, **Moral**, OrderOverride applied
sources: MortEnv

Semantic axis: Respected → (DontCare) → Abhorrent

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | MortEnv | MI_ToxicWasteDumping_Respected | respected | — |
| 1 | 0 | MortEnv | MI_ToxicWasteDumping_Abhorrent | abhorrent | — |

#### `MI_WorldPollution` — single-source, optional (PositiveOnly)
sources: MortEnv

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | MortEnv | MI_WorldPollution_Despised | despised | — |
| 1 | 0 | MortEnv | MI_WorldPollution_Preferred | desired | — |

**Cross-meme penalty**: Conservationist (`MI_Environmentalist`) vs Industrialist (`MI_Industrialist`): **−15**.

### Mort's Ideologies: Political Compass (`MortStrudel.MortIdeology`)

Also extends `BodyModification` and `DrugUse`; see those entries in the stacked section above.

#### `miHousingDistribution` — single-source, **Moral**
sources: MortPolitical

Semantic axis: StrictEquality → MildEquality → Ignored (DontCare) → MildStratification → StrictStratification

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | MortPolitical | miHousingDistribution_StrictEquality | strict equality | — |
| 1 | 10 | MortPolitical | miHousingDistribution_MildEquality | mild equality | — |
| 2 | 20 | MortPolitical | miHousingDistribution_Ignored | ignored | — |
| 3 | 30 | MortPolitical | miHousingDistribution_MildStratification | mild stratification | — |
| 4 | 40 | MortPolitical | miHousingDistribution_StrictStratification | strict stratification | — |

#### `MI_Homelessness` — single-source, optional (PositiveOnly)
sources: MortPolitical

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 40 | MortPolitical | MI_Homelessness_Abhorrent | abhorrent | — |

#### `MI_Leader` — single-source, **Moral**, OrderOverride applied
sources: MortPolitical

Scrambled: Corporate, Monarchy, Dictatorship all at 10; Elections at 20; Anarchy at 30.
Semantic axis: Anarchy → Elections_Required → (DontCare Between Elections and Corporate) → Corporate → Monarchy → Dictatorship

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | MortPolitical | MI_LeaderCorporate | corporate | — |
| 1 | 10 | MortPolitical | MI_LeaderMonarchy | monarchy | — |
| 2 | 10 | MortPolitical | MI_LeaderDictatorship | dictatorship | — |
| 3 | 20 | MortPolitical | MI_Elections_Required | democracy | — |
| 4 | 30 | MortPolitical | MI_LeaderAnarchy | anarchy | — |

**Cross-meme penalties**: Gov. Liberty (`MI_GovernmentLiberty`) vs Gov. Authority (`MI_GovernmentAuthority`): **−10**. Wealth Equality (`MI_WealthEquality`) vs Wealth Stratification (`MI_WealthStratification`): **−10**.

### Questing Meme (`SirMashedPotato.QuestingMeme`)

Both issues are Moral with all rungs at `displayOrderInIssue=10`; OrderOverride establishes the semantic direction.

#### `QuesterMeme_QuestComplete` — single-source, **Moral**, OrderOverride applied
sources: QuestingMeme

Semantic axis: (DontCare before Respected) → Respected → Honourable → Daring

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | QuestingMeme | QuesterMeme_QuestComplete_Respected | respected | — |
| 1 | 10 | QuestingMeme | QuesterMeme_QuestComplete_Honourable | honourable | — |
| 2 | 10 | QuestingMeme | QuesterMeme_QuestComplete_Daring | daring | — |

#### `QuesterMeme_QuestFail` — single-source, **Moral**, OrderOverride applied
sources: QuestingMeme

Semantic axis: (DontCare before DontCare rung) → DontCare → Disliked → Disapproved → Dishonorable

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 10 | QuestingMeme | QuesterMeme_QuestFail_DontCare | don't care | — |
| 1 | 10 | QuestingMeme | QuesterMeme_QuestFail_Disliked | disliked | — |
| 2 | 10 | QuestingMeme | QuesterMeme_QuestFail_Disapproved | disapproved | — |
| 3 | 10 | QuestingMeme | QuesterMeme_QuestFail_Dishonorable | dishonourable | — |

### Waymakers (`Mx.Waymakers`)

#### `WM_Infrastructure` — single-source, optional (PositiveOnly)
sources: Waymakers

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | Waymakers | WM_Infrastructure_Required | required | — |

### Mort's Ideologies: Menagerist (`MortStrudel.MortIdeologySpecWork`)

#### `MI_AnimalVariety` — single-source, optional (PositiveOnly)
sources: Menagerist

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 20 | Menagerist | MI_AnimalVariety_Required | required | — |

#### `MI_Zookeeping` — single-source, optional (PositiveOnly)
sources: Menagerist

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 20 | Menagerist | MI_Zookeeping_Required | required | — |

### Mort's Ideologies: Empiricism & Faith (`MortStrudel.MortIdeologySciFai`)

All issues are PositiveOnly — each rung is locked to one meme (`MI_Faith` or `MI_Empiricist`) and never co-exists with its opposing rung in the same ideo, so there is no meaningful precept axis. Cross-ideo tension is captured by a cross-meme penalty instead.

**Cross-meme penalty**: Empiricist (`MI_Empiricist`) vs Faith (`MI_Faith`): **−5**.

#### `MI_Certainty` — single-source, optional (PositiveOnly)
sources: MortSciFai

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | MortSciFai | MI_Certainty_Increased | increased | — |

#### `MI_ConversionPower` — single-source, optional (PositiveOnly)
sources: MortSciFai

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | MortSciFai | MI_ConversionPower_Increased | increased | — |
| 1 | 0 | MortSciFai | MI_ConversionPower_Reduced | reduced | — |

#### `MI_Laboratory` — single-source, optional (PositiveOnly)
sources: MortSciFai

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | MortSciFai | MI_Laboratory_Demanded | demanded | — |

#### `MI_MeditationFocusGain` — single-source, optional (PositiveOnly)
sources: MortSciFai

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | MortSciFai | MI_MeditationFocusGain_Increased | increased | — |
| 1 | 0 | MortSciFai | MI_MeditationFocusGain_Reduced | reduced | — |

#### `MI_PruningSpeed` — single-source, optional (PositiveOnly)
sources: MortSciFai

| rank | order | source | defName | label | dsw |
|--:|--:|---|---|---|--:|
| 0 | 0 | MortSciFai | MI_PruningSpeed_Increased | increased | — |
| 1 | 0 | MortSciFai | MI_PruningSpeed_Reduced | reduced | — |
