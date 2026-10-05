# Precept opinion policy (hand-maintained)

How each issue contributes to a pawn's opinion of an ideo. **This is edited by hand** — unlike
`modCompat.md` (regenerated from the modlist), nothing overwrites it. It is the source of truth the
`PreceptLadder` resolver will encode.

Markers: **✓** = David's explicit call · **(p)** = my proposal, review · **⚑** = borderline, needs a decision· **a** = David agreed· **m** = David moved to this section.

## Categories

- **Moral** — genuine two-sided disagreement axis; opinion = rung-distance (`getOpinionOnPrecept`). These
  are the only issues that need the fiddly **order fix** (for stack scrambles) and an explicit **Don't-care
  rank**. Everything below contributes to the structural read path; the rest do not.
- **PositiveOnly** — no innate disagreement axis, so it contributes **0 to the structural (innate) opinion
  band and never anything negative**. But a pawn *can* develop a positive opinion of it through interaction
  (winning a debate, reading a book, exposure), which lands in the **acquired** opinion channel (the
  personal per-precept delta, clamped ≥ 0 for these). So over time it only ever *raises* total opinion —
  never lowers it, and it's never innate. Not-caring is just 0.
- **UniversalPositive** — flat `+` for everyone, regardless of stance.
- **Special** — bespoke logic required.
- **NA** — not a belief stance; excluded entirely (buildings, ritual seats, naming conventions).

Only **Moral** needs per-issue order/Don't-care data. On the **structural read path** (`issueStances`),
PositiveOnly + NA both resolve to 0 — no table entry, the resolver's safe default. PositiveOnly still
participates through the separate **acquired** channel (the existing personal per-precept delta), clamped
≥ 0; NA never participates at all.

## Moral (need order + Don't-care rank)

**Don't-care placement rule** — where the virtual "no opinion" rung sits depends on whether the ladder has
a *pro* side:
- **One-sided** (all rungs are degrees of restriction, no "encouraged/required" end, e.g. `KillingInnocentAnimals`
  = disapproved→abhorrent): "don't care" = *no restriction* → **permissive extreme**, below rank 0.
- **Two-sided** (pro ↔ anti, e.g. `Cannibalism` = required…abhorrent): "don't care" is **neutral — the
  middle**, between the least-pro and least-anti rungs. NOT an extreme ("below required" would mean *more
  pro-cannibal than required-ravenous", which is nonsense).
- **Explicit neutral rung** (Execution `don't care`, Corpses `don't care`, NutrientPaste `indifferent`,
  IdeoDiversity `neutral`): just use that rung's rank.

| issue                                                                                                                                                                         | mark | Don't-care sits…                                                                          | note                                                                           |
| ------------------------+------+------------------------------------------------------------+-------------------------------------------------------------------------------- |      |                                                                                           |                                                                                |
| Cannibalism                                                                                                                                                                   | (a)  | neutral / mid (between `acceptable` and `disapproved`)                                    | two-sided; vanilla clean                                                       |
| MeatEating                                                                                                                                                                    | (p)  | permissive extreme                                                                        | ⚑ scrambled by VIE (`vegetarian disliked` at high order) — needs order fix     |
| AnimalSlaughter                                                                                                                                                               | (p)  | permissive extreme                                                                        | ⚑ scrambled by Alpha Memes (`desired` appended above `prohibited`) — order fix |
| KillingInnocentAnimals                                                                                                                                                        | (a)  | permissive extreme                                                                        | vanilla clean                                                                  |
| Slavery                                                                                                                                                                       | (a)  | After Slavery_acceptable                                                                  | vanilla clean                                                                  |
| Execution                                                                                                                                                                     | (a)  | explicit `don't care` rung (mid)                                                          | has a real neutral rung                                                        |
| OrganUse                                                                                                                                                                      | (p)  | ⚑ scrambled by Alpha Memes (`desired` appended above `prohibited`) — order fix            |                                                                                |
| DrugUse                                                                                                                                                                       | (a)  | **mid-ladder** (~between medical-or-social & medical-only)                                | not −1; `essential` is pro-drug                                                |
| ChildLabor                                                                                                                                                                    | (a)  | mid (between `encouraged` / `disapproved`)                                                | two-sided                                                                      |
| Lovin                                                                                                                                                                         | (a)  | n/a, mandatory                                                                            | free↔prohibited spectrum                                                       |
| Nudity (M/F)                                                                                                                                                                  | (a)  | n/a mid (`no rules`)                                                                      | has explicit `no rules`                                                        |
| SpouseCount (M/F)                                                                                                                                                             | (a)  | order wrong (0,4,3,2,1), but manditory anyway                                             | monogamy↔polygamy — moral, or leave as preference?                             |
| Scarification                                                                                                                                                                 | (a)  | n/a, mandatory                                                                            |                                                                                |
| Apostasy                                                                                                                                                                      | (a)  | between accepted and disapproved                                                          |                                                                                |
| IdeoDiversity                                                                                                                                                                 | (a)  | n/a, mandatory                                                                            | tolerance-of-other-beliefs — very on-topic; ties into PreferredXenotypes       |
| Corpses                                                                                                                                                                       | (a)  | n/a, explicit `don't care` rung                                                           |                                                                                |
| FungusEating                                                                                                                                                                  | (a)  | n/a mandatory, mid (`preferred`↔`despised`)                                               |                                                                                |
| InsectMeat                                                                                                                                                                    | (a)  | n/a mandatory, mid                                                                        | has explicit `don't care` in some stacks                                       |
| NutrientPasteEating                                                                                                                                                           | (a)  | explicit `indifferent`/`don't mind`                                                       | mods number it cleanly (negative orders)                                       |
| BodyModification                                                                                                                                                              | (a)  | between approved and disapproved. `Only biological` should be between approved/disaproved |                                                                                |
| Raiding                                                                                                                                                                       | (a)  | between honorable and abhorrent                                                           | required↔abhorrent (VIE)                                                       |
| AutonomousWeapons                                                                                                                                                             | (a)  | between accepted and disapproved                                                          |                                                                                |
| Fishing                                                                                                                                                                       | (a)  | betwene disapproved and sacred                                                            | prohibited↔sacred — two-sided but niche                                        |
| GrowthVat                                                                                                                                                                     | (a)  | between essential and prohibited                                                          | essential↔prohibited                                                           |
| Skullspike                                                                                                                                                                    | (a)  | mid                                                                                       | desired↔disapproved                                                            |
| Bloodfeeders                                                                                                                                                                  | (a)  | mid                                                                                       | revered↔reviled                                                                |
| Biosculpting                                                                                                                                                                  | (a)  | mid                                                                                       | Accelerated↔despised                                                           |
| Bonding                                                                                                                                                                       | (a)  | permissive extreme                                                                        | single `disapproved` rung — thin                                               |
| Trees                                                                                                                                                                         | (m)  | mid                                                                                       | Desired↔despised                                                               |
| RoughLiving                                                                                                                                                                   | (m)  | mid                                                                                       | induced by AlphaMemes                                                          |
| GauranlenConnection                                                                                                                                                           | (m)  | mid                                                                                       | induced by AlphaMemes                                                          |
| Eclipse                                                                                                                                                                       | (m)  | mid                                                                                       | induced by VIE-M&S                                                             |
| Ranching                                                                                                                                                                      | (m)  | between Nomadic and central                                                               | only if VIE-M&S is present                                                     |

## Reorder (semantic order remap)

Issues where `displayOrderInIssue` (after stacking) does **not** track the belief axis, so rank-distance
lies. The resolver overrides the rung order for these to the sequence below (permissive/pro → forbidding/anti).
Keyed by rung defName so it's load-order robust. `— neutral —` marks where Don't-care sits for the optional ones.

**MeatEating** — the two vegetarian groups sit above the meat-disapproval group; reversed pole:
`MeatEating_NonMeat_Abhorrent` → `_NonMeat_Horrible` → `_NonMeat_Disapproved` — neutral — `MeatEating_Disapproved`
→ `_Horrible` → `_Abhorrent` → `VME_MeatEating_Abhorrent_Strict`

**AnimalSlaughter** — Alpha Memes' `desired` (pro) appended above the anti degrees:
`AM_AnimalSlaughter_Desired` — neutral — `AnimalSlaughter_Disapproved` → `_Horrible` → `_Prohibited`

**SpouseCount_Male / SpouseCount_Female** — `unlimited` sits at rank 1 instead of the polygamy extreme:
`…_MaxOne` → `…_MaxTwo` → `…_MaxThree` → `…_MaxFour` → `…_Unlimited`  *(mandatory → no Don't-care)*

**BodyModification** — `only biological` is a mid restriction, ties at order 20 with `abhorrent`:
`BodyMod_Approved` — neutral — `VME_BodyMod_OnlyBiological` → `BodyMod_Disapproved` → `BodyMod_Abhorrent`

**OrganUse** — `respected` (pro) has no order so it sorts last instead of the pro extreme:
`OrganUse_Respected` → `OrganUse_Acceptable` — neutral — `VME_OrganUse_PostMortem` → `OrganUse_HorribleSellOK`
→ `OrganUse_HorribleNoSell` → `OrganUse_Abhorrent` → `AM_OrganUse_Torturous`  *(`OrganUse_Classic` unlabeled — leave at acceptable)*

## PositiveOnly (contribute 0; positive only when held)

✓ (David): AnimalsVenerated, ApparelDesire, BlindPsysense, Indoors, MechanoidLabor, MiningYield,
NeuralSupercharge, Nomadic, SlabBed, SlaveTrading, SleepAccelerator, SpaceHabitat.

(a) also: AnimalConnection, Temperature, WorkDrive, MechanoidLabor, Mining, Tree Cutting
Eclipse,

## PositiveOnly review (encoded)

Context: debates used to pick any non-NA issue as a topic, and PositiveOnly topics moved nothing structural.
Debate topics are now limited to Moral + Special (`PreceptPolicy.IsDebatable`), so every PositiveOnly issue that carries a real value claim moved to Moral first.
Everything below is encoded in `PreceptPolicy` except where marked (p) or deferred.

**Test for Moral:** the rung text makes a value claim, and either another rung opposes it or an ordinary outsider (the silent Don't-care side) would object to it.
Pure stat modifiers stay PositiveOnly.
Caveat for one-sided issues: a silent pawn sits at Don't-care, one full span from the rung, so it reads as mild opposition to every faith that holds the rung.
That is right for "madness is holy" and wrong for "tea is sacred", which is why the outsider-objects test matters.

### Multi-rung

Rungs listed pro → anti. `— neutral —` marks where Don't-care sits.

| issue | mark | category | ladder | note |
| --- | --- | --- | --- | --- |
| AM_Armour | ✓ | Moral, tied ranks | `Forbidden` — neutral — `Blunt` = `Sharp` = `Heat` | the three specialties have identical text and differ only in stat; `PreceptPolicy.TiedRungs` maps Sharp and Heat onto Blunt |
| AM_Barracks | ✓ | Moral, plain | `Preferred` (monastic) → `PreferredTrue` → `Acceptable` — neutral — | one-sided: silence is the vanilla dislike of barracks; monastic is Medium impact, preferred Low |
| Pain | ✓ | Moral, plain | `AM_Pain_Required` → `Pain_Idealized` → `VME_Pain_DontCare` | `VME_Pain_DontCare` is the explicit neutral, but `At` degrades to −1 without VME, so use `After("Pain_Idealized")`; the `RoughLiving` induction should cover `AM_Pain_Required` too |
| EB_Contemplation | ✓ | Moral, plain | `Required` → `Respected` → `Normal` → `Disapproved` → `Forbidden` | our own issue; `Normal` is the explicit neutral |
| Research | ✓ | Moral, plain | `VeryFast` → `Fast` → `Normal` → `Slow` → `VerySlow` → `ExtremelySlow` → `None` | text is moral ("an immoral act" ↔ "among the most meaningful acts"); `Normal` is the explicit neutral |
| Blindness | ✓ | Moral, plain | `Sublime` → `Elevated` → `Respected` — neutral — `Blinding_Horrible` | two-sided |
| DarknessCombat | ✓ | Moral, plain | `Preferred` — neutral — `VME_DarknessCombat_Despised` | reverses the earlier (a); only two-sided with VME |
| Lighting | ✓ | Moral, plain | `Darklight_Preferred` — neutral — `VME_Darklight_NormalPreferred` | reverses the earlier (a); only two-sided with VME |
| Proselytizing | ✓ | Moral, plain | `VME_Forceful` → `Frequently` → `Sometimes` → `Occasionally` — neutral — `VME_Never` | vanilla rungs need the Proselytizer meme; `RelicHuntSuccess` was a scan artefact (it is a `HistoryEventDef`) |
| VME_Power | ✓ | Moral, plain | `Exalted` → `Desired` → `Preferred` — neutral — | one-sided, same shape as Barracks |
| VME_Junk | ✓ | Moral, plain | `Beautiful` → `Preferred` — neutral — | one-sided; weak claim |
| VME_Death | ✓ | Moral, plain | `DontCare` → `Normal` → `Troubling` | axis is how much to mourn; `Normal` is the centre |
| VME_AutomationEfficiency | ✓ | Moral, plain | `Increased` — neutral — `Decreased` | "working by hand is primitive" ↔ "automation is despicable" |
| VME_CraftingQuality | ✓ | Moral, plain | `Increased` — neutral — `Decreased` | "take no shortcuts" ↔ "why create when you can steal" |
| VME_PsychicSensitivity | ✓ | Moral, plain | `Heightened` — neutral — `Lowered` | "open mind" ↔ "mental fortress" |
| AM_PsychicSensitivity | ✓ | Moral, plain | `Heightened` — neutral — `Affinity` | same axis as the VME issue, but a separate IssueDef; ✓ only one of the two may hold an opinion when both mods are loaded (see below) |
| BS_AlienAppearanceTolerance | ✓ | Moral, plain | `FullTolerance` → `SomeTolerance` → `Default` | xenophobia axis; `Default` has `defaultSelectionWeight` 8, so every faith holds a rung and no Don't-care is needed |
| Comfort | ✓ | Moral, plain | — neutral — `Comfort_Ignored` → `AM_Comfort_DiscomfortPreferred` | was ✓ PositiveOnly; one-sided anti-comfort, silence is the vanilla love of comfort |
| VME_CraftingSpeed | ✓ | stay PositiveOnly | — | `FasterForManual` ↔ `SlowerForManual` is an axis, but `Slower` ("never rush") is off-axis |
| AM_CombatProwess | ✓ | stay PositiveOnly | — | ranged / melee / reduced are trade-offs, not values |
| MiningYield, VME_SkilledLabor, VME_PermitCooldown, VME_PermitHonorCost, ApparelDesire | ✓ | stay PositiveOnly | — | stat levels or payload; no opposing claim |

**Display-order check** (active mod list, `ParentName` inheritance resolved):
- Clean, no override: `EB_Contemplation`, `Research`, `Proselytizing`, `DarknessCombat`, `Lighting`, `VME_Death`, `VME_Power`, `VME_Junk`.
  The last three run anti → pro, which is fine because `OpinionOnPrecept` is symmetric under reflection.
- All rungs tied at order 0, fixed with an `OrderOverrides` entry: `AM_Barracks`, `Pain`, `Comfort`, `VME_AutomationEfficiency`, `VME_CraftingQuality`, `VME_PsychicSensitivity`, `AM_PsychicSensitivity`, `AM_Armour`.
- Scrambled: `Blindness` sorts `Blinding_Horrible` (0) ahead of `Sublime` (10), `Elevated` (20), `Respected` (30), so the anti pole sits next to the strongest pro rung. Overridden.
- Partly tied: `BS_AlienAppearanceTolerance` has `FullTolerance` and `SomeTolerance` both at 10. Overridden.

**Don't-care where one pole comes from another mod:** the spec is keyed on the base rung only (`After`/`Before`), not `Between`, so it never collapses onto that rung when the other mod is absent.
`Pain` uses `After("Pain_Idealized")`, `DarknessCombat` `After("DarknessCombat_Preferred")`, `Lighting` `Before("Darklight_Preferred")`, `Proselytizing` `After("Proselytizing_Occasionally")`.
The older `Eclipse` entry (`Between("Eclipse_Beautiful", "VME_Eclipse_Despised")`) has this collapse without VME; not changed here.

**Psychic sensitivity with both mods loaded** ✓: one axis must not count twice. Deferred; tracked in [todo.md](../todo.md).
(p) `VME_PsychicSensitivity` holds the opinion, and `AM_PsychicSensitivity` drops to PositiveOnly.
The AM rungs then induce the VME rungs (`AM_..._Heightened` → `VME_..._Heightened`, `AM_..._Affinity` → `VME_..._Lowered`), so a faith that holds only the AM precept still has a stance on the axis.
An explicit VME precept wins over the induced one, as with every induced stance.
With only Alpha Memes loaded, `AM_PsychicSensitivity` is Moral on its own.

### Single-rung value claims

Candidates for a two-point Moral issue with `DontCareSpec.Before`/`After` (the same shape as `Bonding`).
Each passes the outsider-objects test.

| issue | mark | rung | note |
| --- | --- | --- | --- |
| AM_Madness | (a) | `Exalted` | "a twisted mind is a sign of holiness" |
| AM_Death | (a) | `Desired` | "death should be actively sought" |
| VME_Corruption | (a) | `Essential` | "embrace the gifts of our dark god" |
| AM_DeathrestCaskets | (a) | `Abhorrent` | |
| AM_HarbingerTrees | (a) | `Disgusting` | |
| AM_AnimaScreams | (a) | `Delightful` | delights in uprooting anima trees |
| AM_Reliquaries | (a) | `Forbidden` | candidate coupling with `AM_RelicDestruction_Desired` |
| AM_RelicDestruction | (a) | `Desired` | |
| VFEA_Recruiting | (a) | `Forbidden` | debate-efficiency TODO under Interactions already touches this |
| VFEA_BeingRecruited | (a) | `Forbidden` | |
| VME_SocialInteractions | (a) | `Disallowed` | |
| VME_LeaderDivinity | (a) | `Godlike` | regard for the leader, a separate axis from `VME_Leader` (how the leader is chosen) |
| VME_Anonymity | (a) | `Required` | |
| AgeReversal | (a) | `Demanded` | overrides the earlier (a) PositiveOnly |

Not one-rung Moral after review:
- `VME_SlaveTrading` (`OnlyBuying`) (a) — stay PositiveOnly. An induced `Slavery` stance never fires, because `Slavery` is mandatory and every faith holds an explicit rung. The faith's own Slavery rung already carries the axis.
- `AM_HuntFocus_Sanguophage`, `AM_SanguophageCamps_RaidingDesired` (a) — stay PositiveOnly and induce `Bloodfeeders_Reviled` (`Bloodfeeders` is optional, so the induction applies).
- `AnimalsVenerated`, `AM_AnimalsDespised` (a) — stay PositiveOnly for now. The right model is a Special payload compare like Weapons, which is not worth the work yet.

**Couplings found along the way:**
- `AM_HuntFocus_Sanguophage`, `AM_SanguophageCamps_RaidingDesired` → `Bloodfeeders_Reviled` (a), encoded
- `AM_Pain_Required` → `RoughLiving_Welcomed` (same as `Pain_Idealized`) ✓, encoded
- `AM_Armour_Forbidden`, `AM_NakedCombat_Preferred` → an approving `Nudity_*` rung (p)
- `VME_Fishing_Adept` ("fishing is the right way; ranching and farming is not") → `Ranching` (p)

**Stays PositiveOnly** (preferences, perks, production; no outsider objects): the ✓/(a) vanilla list above, plus
`AM_Art`, `AM_ArtProductionSpeed`, `AM_ArtQuality`, `AM_AnimalAnalysis`, `AM_Baths`, `AM_Cattle`, `AM_CoffeeCultivation`,
`AM_CoffeeDrinking`, `AM_CoffeeYield`, `AM_Dryads`, `AM_Horses`, `AM_Hydroagriculture`, `AM_KitchenProficiency`,
`AM_MaternalMortality`, `AM_Meals`, `AM_Megaliths`, `AM_Mood`, `AM_PlantSowing`, `AM_PrefabAcquisition`, `AM_PrefabBuying`,
`AM_PsyfocusGain`, `AM_Reputation`, `AM_RoomSize`, `AM_TableQuality`, `AM_TeaCultivation`, `AM_TeaDrinking`, `AM_TeaYield`,
`AM_Water`, `VME_BookQuality`, `VME_Defeat`, `VME_FarmingYield`, `VME_FishingYield`, `VME_Hospital`, `VME_Immunity`,
`VME_JunkDeconstructionYield`, `VME_LeaderAbilities`, `VME_Library`, `VME_Meditation`, `VME_PsyfocusGain`, `VME_TendQuality`,
`VME_TitleInheritance`, `VME_Trading`, `VME_TradingPrice`, `VME_WoodcuttingYield`.

## UniversalPositive

- **Charity** ✓ — flat positive for everyone.

## Special

- **PreferredXenotypes** ✓ — *which* xenotype matters, so it compares the two faiths' preferred-xenotype sets
  (`TryPayloadSpecialOpinion`): Sørensen similarity → +strength for identical sets, −strength for disjoint,
  scaled between. A faith with no preference has no stance on this axis. The friction with IdeoDiversity
  ("Diversity of Thought") is a directional coupling ✓ (the appreciative rungs Approved/Respected/Exalted sour
  on any PreferredXenotype holder, via `CouplingPenalties`). STILL TODO: the Kind-meme side of that friction —
  it needs the coupling framework extended past precept→precept to accept a meme as a source.
- **Weapons** ✓ — noble/despised come in pairs (`TryPayloadSpecialOpinion`): revering or despising the same
  weapon class agrees, revering what the other despises clashes, and faiths whose tastes don't intersect
  genuinely don't care (skipped, not counted neutral). A single fully-aligned/opposed pair saturates to
  ±strength. Compares payloads, not ranks, so it routes around the rung-based `TrySpecialOpinion`.

## Interactions

**Induced stances** ✓ (`PreceptPolicy.InducedByPrecept`, fold into the structural mean via `HeldRank`; the
coupled target issue joins the comparison even when neither faith holds it explicitly, and grades by rung
distance regardless of its own category):
- `Trees_Desired` → acts like `TreeCutting_Disapproved`; `AM_Trees_Despised` → one step beyond Don't-care for `TreeCutting`
- `Pain_Idealized` → acts like `RoughLiving_Welcomed`
- `VME_LeatherApparel_Disliked` → `AnimalSlaughter_Disapproved`; `VME_LeatherApparel_Abhorrent` → `AnimalSlaughter_Horrible`

**Directional penalty** ✓ (`PreceptPolicy.CouplingPenalties`; single-rung target has no anti rung to grade
toward, so it's a flat one-way hit scaled by the source's conviction, added after the mean):
- `VME_Mechanoids_Despised` → −opinion of any ideo holding `MechanoidLabor_Enhanced`

**TODO (debate-efficiency, a separate mechanic — not the opinion model):**
- recruiting forbidden decreases effective debate efficiency when succeeding
- being recruited decreases effective debate efficiency when losing

## NA (excluded)

✓ IdeoBuilding, IdeoRelic. (p) IdeoRitualSeat, Ritual, MarriageName. 

## Mod issues

Heuristic: **single-rung → PositiveOnly** (no disagreement axis; the default covers them — no entry).
**Multi-rung → judge per issue.** Final classification (all encoded in `PreceptPolicy.cs`):

**Moral, optional** (have a neighbour-keyed Don't-care in `DontCare`): `VME_Alcohol`, `VME_KillingWithFire`,
`VME_LeatherApparel`, `VME_Scars`, `VME_Elders`, `VME_Royalty`, `VME_Mechanoids`, `VME_Insectoids`,
`VME_Fire`, `VME_Firefighting`, `AM_Religion`, `AM_AnimalRelease`, `VME_Expectations`, `AM_Rain`,
`VME_Aurora`, `VME_BookReading`, `VME_BookReadingSpeed`, `VME_BookWriting`, `VME_Travel`, `VME_PermanentBases`.

**Moral, mandatory** (every ideo takes a stance → no Don't-care needed): `VME_Violence`, `VME_Recreation`,
`VME_TaintedApparel`, `VME_TatteredApparel`, `AM_FertilityIssue`, `AM_LearningRate`, `AM_LovinFrequency`,
`AM_Creep`, `AM_Disfigurement`, `VME_Illness`, `VME_InsectJelly`, `VME_Sweets`, `VME_DumbLabor`,
`AM_OcularTrees`.

**Order-fix candidates (verified — no fix needed).** Pulled every ladder above. Each carries a
`defaultSelectionWeight` rung (silent ideos auto-resolve to it — mostly the centred neutral; `AM_Creep` /
`AM_Disfigurement` default to `disliked`), so the `-1` Don't-care default is never reached and no Don't-care
entry is needed. Every ladder is monotonic on its belief axis, so rank-distance never lies; the couple that
are *reversed* (`VME_Recreation` anti→pro, `VME_Illness` indifferent→exalted) still score correctly because
`OpinionOnPrecept` is symmetric under axis reflection. The flagged neutrals (`AM_FertilityIssue`'s `normal`
etc.) are actually dead-centre, and `VME_Recreation`'s `fishing` sits at the pro extreme either way
(numerically inert). No reorder overrides added.

**PositiveOnly:** `AM_CombatProwess`, `AM_HuntFocus`, and the mechanical perks (`*Speed`/`*Yield`/`*Rate`/`Cooldown`).
The value-claim issues that used to sit here (`VME_Death`, `VME_Power`, `VME_Junk`, `AM_Armour`, `AM_Barracks`,
`VME_AutomationEfficiency`, `VME_CraftingQuality`, the psychic sensitivities) moved to Moral; see "PositiveOnly review".

**Special (bespoke):** `VME_Leader` ✓ — categorical, any difference (incl. one side having no leader
precept) = full −opinion, exact match = +opinion. `VME_Mood` ✓ — hybrid: high/normal/low grade by rung
distance on their own sub-ladder; `shared` and `dictated by stars` are pariahs that clash with everything
(and each other) at −opinion, agree only with their own kind. Both fold into the structural mean via
`PreceptPolicy.TrySpecialOpinion`. Still TODO: `PreferredXenotypes`, `Weapons` (fall through to skip).
**NA:** `AM_Abilities` (no opinions at all).
