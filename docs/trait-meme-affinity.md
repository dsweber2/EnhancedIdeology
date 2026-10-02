# Trait–Meme–Precept Affinity

Vanilla `MemeDef` XML declares `agreeableTraits` and `disagreeableTraits`.
The mod uses these in two ways:

1. **Structural ideo opinion** (`StructuralOpinionOf`): ±10 per matching trait, regardless of which ideo the meme belongs to.
2. **Conviction seeding** (`TraitMemeConvictionOffsets`): ±`TraitMemeConvictionBonus` (10) to the initial conviction strength on each issue covered by the meme's precepts, applied at pawn generation.
Only memes whose precepts carry a `requiredMemes` back-reference produce a conviction effect; memes with no precepts (e.g. gender supremacy) only affect structural opinion.

---

## By trait

| Trait          | Degree | Meme               | Direction    | Conviction-affected issues                                                                   |
|----------------+--------+--------------------+--------------+----------------------------------------------------------------------------------------------|
| Transhumanist  | any    | Transhumanist      | agreeable    | SleepAccelerator, NeuralSupercharge, Biosculpting, AgeReversal, NutrientPasteEating, BodyMod |
| BodyPurist     | any    | Transhumanist      | disagreeable | same as above                                                                                |
| BodyPurist     | any    | FleshPurity        | agreeable    | DrugUse, BodyMod                                                                             |
| Nudist         | any    | FleshPurity        | agreeable    | DrugUse, BodyMod                                                                             |
| Transhumanist  | any    | FleshPurity        | disagreeable | DrugUse, BodyMod                                                                             |
| Ascetic        | any    | PainIsVirtue       | agreeable    | Pain, Comfort, SlabBed, RoughLiving, Temperature, Scarification                              |
| TorturedArtist | any    | PainIsVirtue       | agreeable    | same                                                                                         |
| Masochist      | any    | PainIsVirtue       | agreeable    | same                                                                                         |
| Wimp           | any    | PainIsVirtue       | disagreeable | same                                                                                         |
| Gourmand       | any    | PainIsVirtue       | disagreeable | same                                                                                         |
| Cannibal       | any    | Cannibal           | agreeable    | Cannibalism, Execution, OrganUse                                                             |
| DrugDesire     | 1 or 2 | HighLife           | agreeable    | DrugUse                                                                                      |
| Nudist         | any    | Nudism             | agreeable    | Nudity_Male, Nudity_Female                                                                   |
| Undergrounder  | any    | Tunneler           | agreeable    | FungusEating, InsectMeatEating, Indoors, MiningYield                                         |
| Undergrounder  | any    | Shipborn (Odyssey) | disagreeable | Indoors, NutrientPasteEating, Temperature, SpaceHabitat                                      |
| DislikesWomen  | any    | MaleSupremacy      | agreeable    | *(opinion only — no precepts)*                                                               |
| DislikesMen    | any    | FemaleSupremacy    | agreeable    | *(opinion only — no precepts)*                                                               |

---

## Notable interactions

- **Nudist** matches both FleshPurity and Nudism → double conviction boost on BodyMod/DrugUse (via FleshPurity) and nudity issues.
- **Transhumanist** and **BodyPurist** are each other's mirror: agreeable for one meme, disagreeable for the other. A Transhumanist pawn who converts to FleshPurity starts with low conviction on those issues → easy debate target.
- **DrugDesire** is degree-gated (only 1 and 2 match, not 0 or −1), so a casual drug user gets the HighLife boost but a teetotaller does not.
- Gender supremacy traits (**DislikesWomen**, **DislikesMen**) only affect structural ideo opinion; they produce no per-issue conviction shift because those memes define no precepts via `requireOne`.
