# Trait–Meme–Precept Affinity

Vanilla `MemeDef` XML declares `agreeableTraits` and `disagreeableTraits`.
The mod uses these in two ways:

1. **Structural ideo opinion** (`StructuralOpinionOf`): ±10 per matching trait, regardless of which ideo the meme belongs to.
2. **Conviction seeding** (`TraitMemeConvictionOffsets`): ±`TraitMemeConvictionBonus` (10) to the initial conviction strength on each issue covered by the meme's precepts, applied at pawn generation.
A meme links to an issue in three ways: a held precept lists it in `requiredMemes`; a held precept lists it in `associatedMemes` and the faith holds the meme; or the meme's `requireOne`/`selectOneOrNone` covers the issue and the faith takes a stance on it.
Each meme–issue link counts once, however many routes find it.
Spawn seeding and brainwipe use the same rule.
A meme with no link only affects structural opinion.

For the full list of memes and precept issues per mod, with the issues each meme links to, see [meme-precept-catalog.md](meme-precept-catalog.md).

---

## By trait

| Trait          | Degree | Meme               | Direction    | Conviction-affected issues                                                                                                   |
|----------------+--------+--------------------+--------------+------------------------------------------------------------------------------------------------------------------------------|
| Transhumanist  | any    | Transhumanist      | agreeable    | AgeReversal, Biosculpting, BodyModification, GrowthVat, NeuralSupercharge, NutrientPasteEating, SleepAccelerator             |
| BodyPurist     | any    | Transhumanist      | disagreeable | same as above                                                                                                                |
| BodyPurist     | any    | FleshPurity        | agreeable    | BodyModification, DrugUse, GrowthVat, OrganUse                                                                               |
| Nudist         | any    | FleshPurity        | agreeable    | same                                                                                                                         |
| Transhumanist  | any    | FleshPurity        | disagreeable | same                                                                                                                         |
| Ascetic        | any    | PainIsVirtue       | agreeable    | Cannibalism, Comfort, Corpses, Execution, Lovin, Pain, RoughLiving, Scarification, Skullspike, SlabBed, Slavery, Temperature |
| TorturedArtist | any    | PainIsVirtue       | agreeable    | same                                                                                                                         |
| Masochist      | any    | PainIsVirtue       | agreeable    | same                                                                                                                         |
| Wimp           | any    | PainIsVirtue       | disagreeable | same                                                                                                                         |
| Gourmand       | any    | PainIsVirtue       | disagreeable | same                                                                                                                         |
| Cannibal       | any    | Cannibal           | agreeable    | Cannibalism, Corpses, Execution, OrganUse                                                                                    |
| DrugDesire     | 1 or 2 | HighLife           | agreeable    | DrugUse                                                                                                                      |
| Nudist         | any    | Nudism             | agreeable    | Nudity_Male, Nudity_Female                                                                                                   |
| Undergrounder  | any    | Tunneler           | agreeable    | FungusEating, Indoors, InsectMeat, MiningYield                                                                               |
| Undergrounder  | any    | Shipborn (Odyssey) | disagreeable | Indoors, NutrientPasteEating, Temperature, SpaceHabitat                                                                      |
| DislikesWomen  | any    | MaleSupremacy      | agreeable    | MarriageName, Nudity_Female, SpouseCount_Male                                                                                |
| DislikesMen    | any    | FemaleSupremacy    | agreeable    | MarriageName, Nudity_Male, SpouseCount_Female                                                                                |

---

## Notable interactions

- **Nudist** matches both FleshPurity and Nudism → double conviction boost on BodyMod/DrugUse (via FleshPurity) and nudity issues.
- **Transhumanist** and **BodyPurist** are each other's mirror: agreeable for one meme, disagreeable for the other. A Transhumanist pawn who converts to FleshPurity starts with low conviction on those issues → easy debate target.
- **DrugDesire** is degree-gated (only 1 and 2 match, not 0 or −1), so a casual drug user gets the HighLife boost but a teetotaller does not.
- Gender supremacy traits (**DislikesWomen**, **DislikesMen**) reach conviction only through `associatedMemes`, because those memes have no `requireOne`.
  The shift applies only to the marriage-name, nudity and spouse-count rungs the faith actually picked from the meme's associated set.
- **PainIsVirtue** has the widest reach of any vanilla meme with traits.
  Its `associatedMemes` add Cannibalism, Execution, Lovin, Skullspike and Slavery, so an Ascetic or Masochist in such a faith starts firmer on all five.

---

## Mod-added affinities

Trait mods add their own traits to memes' `agreeableTraits` and `disagreeableTraits` through XML patches.
The mod reads these lists at runtime, so these traits already have the opinion and conviction effects above, with no code on our side.
Of the trait mods that RimPsyche supports (see [personality_integration.md](personality_integration.md)), only Hauts' Added Traits and The Sims Traits do this.
Vanilla Traits Expanded, SYR Individuality, Bad People and Big and Small add no meme affinities.
Consolidated Traits was checked against the old [KV] source only; the [LC] continuation that RimPsyche targets has no public repository.

Nikolai's Ideology: Gender Works lists one Hauts trait in its own meme defs: HVT_Aestheticist is agreeable for Aphroditic and disagreeable for Chastity.

The tables below were generated from the trait mods' 1.6 patches.
"Debatable issues" lists only the Moral and Special issues (see [preceptPolicy.md](preceptPolicy.md)) that the meme's precepts cover.
The trait also shifts conviction on the meme's other issues, but nothing reads those stances, so they are left out.
"*(opinion only)*" means the meme covers no debatable issue, so the trait only changes structural opinion.
Issues for meme mods that are not installed locally were not computed.

### Hauts' Added Traits

| Meme                        | Meme from                        | Agreeable                                                                                                                                                                                                                                                                                     | Disagreeable                                                                           | Debatable issues                                                                               |
|-----------------------------+----------------------------------+-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+----------------------------------------------------------------------------------------+------------------------------------------------------------------------------------------------|
| Inhuman                     | Anomaly                          | HVT_Corruptible, HVT_MonsterLover, HVT_Twisted                                                                                                                                                                                                                                                | HVT_MonsterHunter                                                                      | Corpses, NutrientPasteEating                                                                   |
| AnimalPersonhood            | Ideology                         | —                                                                                                                                                                                                                                                                                             | HVT_Agrizoophobe, HVT_Angler, HVT_Pescatarian                                          | AnimalSlaughter, Fishing, KillingInnocentAnimals, MeatEating                                   |
| Blindsight                  | Ideology                         | HVT_Daydreamer, HVT_LatentPsychic, HVT_AwakenedAugur, HVT_AwakenedChanshi, HVT_AwakenedDeluge, HVT_AwakenedErudite, HVT_AwakenedIncarnate, HVT_AwakenedLuminary, HVT_AwakenedMantraist, HVT_AwakenedPerennial, HVT_AwakenedSage, HVT_AwakenedSiphoner, HVT_AwakenedTitan, HVT_AwakenedUndying | —                                                                                      | Blindness                                                                                      |
| Collectivist                | Ideology                         | HVT_Allegiant, HVT_Champion, HVT_Conformist                                                                                                                                                                                                                                                   | HVT_Vagabond                                                                           | ChildLabor, DrugUse, Execution, Lovin, OrganUse, Slavery                                       |
| Darkness                    | Ideology                         | HVT_Skulker                                                                                                                                                                                                                                                                                   | HVT_Sunbather                                                                          | DarknessCombat, Eclipse, FungusEating, Lighting, VME_Aurora                                    |
| Guilty                      | Ideology                         | HVT_Humble, HVT_PersecutionComplex                                                                                                                                                                                                                                                            | —                                                                                      | Pain, Slavery                                                                                  |
| HighLife                    | Ideology                         | HVT_Daydreamer, HVT_Hedonist                                                                                                                                                                                                                                                                  | —                                                                                      | DrugUse                                                                                        |
| HumanPrimacy                | Ideology                         | HVT_Agrizoophobe                                                                                                                                                                                                                                                                              | —                                                                                      | Bonding                                                                                        |
| Individualist               | Ideology                         | HVT_Asocial, HVT_Vagabond, HVT_Liberator                                                                                                                                                                                                                                                      | HVT_Allegiant, HVT_Subjugator, HVT_Servile                                             | ChildLabor, DrugUse, Execution, IdeoDiversity, Lovin, Nudity_Female, Nudity_Male, Slavery      |
| Loyalist                    | Ideology                         | HVT_Allegiant, HVT_Champion, HVT_Servile                                                                                                                                                                                                                                                      | HVT_Vagabond                                                                           | Apostasy, LanguageLearning                                                                     |
| NaturePrimacy               | Ideology                         | HVT_Outdoorsy, HVT_Environmentalist                                                                                                                                                                                                                                                           | HVT_Agrizoophobe                                                                       | AnimalSlaughter, AutonomousWeapons, KillingInnocentAnimals, Mining, TreeCutting                |
| Nudism                      | Ideology                         | —                                                                                                                                                                                                                                                                                             | HVT_Textile                                                                            | Nudity_Female, Nudity_Male                                                                     |
| Proselytizer                | Ideology                         | HVT_Proclaimer                                                                                                                                                                                                                                                                                | —                                                                                      | Apostasy, Proselytizing                                                                        |
| Raider                      | Ideology                         | HVT_Sadist                                                                                                                                                                                                                                                                                    | —                                                                                      | Corpses, Execution, LanguageLearning, OrganUse, Raiding, Skullspike                            |
| Supremacist                 | Ideology                         | HVT_Sadist, HVT_Prideful, HVT_Intolerant, HVT_Subjugator                                                                                                                                                                                                                                      | HVT_Tolerant, HVT_Liberator, HVT_Anarchist                                             | Corpses, Execution, LanguageLearning, Skullspike, Slavery                                      |
| TreeConnection              | Ideology                         | HVT_Environmentalist                                                                                                                                                                                                                                                                          | —                                                                                      | GauranlenConnection, RoughLiving, Trees                                                        |
| Tunneler                    | Ideology                         | HVT_Earthborne                                                                                                                                                                                                                                                                                | HVT_Sunbather, HVT_Tempestophile, HVT_Outdoorsy, HVT_Skybound                          | FungusEating, Indoors, InsectMeat                                                              |
| Shipborn                    | Odyssey                          | HVT_Skybound                                                                                                                                                                                                                                                                                  | HVT_Earthborne                                                                         | Indoors, NutrientPasteEating                                                                   |
| AM_Artist                   | Alpha Memes                      | HVT_Aestheticist, HVT_Daydreamer, HVT_Hedonist                                                                                                                                                                                                                                                | HVT_Staid                                                                              | *(opinion only)*                                                                               |
| AM_BiologicalReconstructors | Alpha Memes                      | HVT_Environmentalist                                                                                                                                                                                                                                                                          | —                                                                                      | AM_AnimalRelease                                                                               |
| AM_Cowboys                  | Alpha Memes                      | —                                                                                                                                                                                                                                                                                             | HVT_Angler                                                                             | Ranching                                                                                       |
| AM_DeepDevotion             | Alpha Memes                      | —                                                                                                                                                                                                                                                                                             | HVT_MetabolicFreak, HVT_Sunbather, HVT_Tempestophile, HVT_Outdoorsy                    | FungusEating, Indoors, InsectMeat                                                              |
| AM_Deforestation            | Alpha Memes                      | —                                                                                                                                                                                                                                                                                             | HVT_Environmentalist                                                                   | AM_AnimaScreams, AM_HarbingerTrees, GauranlenConnection, RoughLiving, Trees                    |
| AM_Epicurean                | Alpha Memes                      | HVT_Aestheticist                                                                                                                                                                                                                                                                              | HVT_MetabolicFreak                                                                     | NutrientPasteEating                                                                            |
| AM_Fertility                | Alpha Memes                      | HVT_Caretaker                                                                                                                                                                                                                                                                                 | HVT_Misopedist                                                                         | AM_FertilityIssue, AM_LovinFrequency                                                           |
| AM_GauranlenSupremacy       | Alpha Memes                      | HVT_Environmentalist                                                                                                                                                                                                                                                                          | —                                                                                      | *(opinion only)*                                                                               |
| AM_Iconoclast               | Alpha Memes                      | HVT_Anarchist                                                                                                                                                                                                                                                                                 | HVT_Aestheticist                                                                       | AM_RelicDestruction, AM_Reliquaries                                                            |
| AM_Madness                  | Alpha Memes                      | HVT_Aestheticist, HVT_Daydreamer                                                                                                                                                                                                                                                              | —                                                                                      | AM_Madness, AM_PsychicSensitivity                                                              |
| AM_Monastic                 | Alpha Memes                      | HVT_Humble, HVT_Staid                                                                                                                                                                                                                                                                         | HVT_Prideful                                                                           | AM_Barracks, DrugUse, Lovin, RoughLiving                                                       |
| AM_NakedTruth               | Alpha Memes                      | —                                                                                                                                                                                                                                                                                             | HVT_Textile                                                                            | AM_Armour, AM_Disfigurement, Nudity_Female, Nudity_Male                                        |
| AM_NonViolence              | Alpha Memes                      | HVT_Tranquil, HVT_Liberator                                                                                                                                                                                                                                                                   | HVT_Sadist, HVT_Vicious, HVT_WeaponArtist, HVT_Subjugator, HVT_Angler, HVT_Pescatarian | AnimalSlaughter, Execution, KillingInnocentAnimals, MeatEating, Raiding, Slavery, VME_Violence |
| AM_PainIsDivine             | Alpha Memes                      | —                                                                                                                                                                                                                                                                                             | HVT_Aestheticist, HVT_Perceptive (1), HVT_Perceptive (2)                               | AM_Disfigurement, Comfort, Pain, RoughLiving, Scarification                                    |
| AM_PsychicVampirism         | Alpha Memes                      | HVT_LatentPsychic, HVT_AwakenedAugur, HVT_AwakenedChanshi, HVT_AwakenedDeluge, HVT_AwakenedErudite, HVT_AwakenedIncarnate, HVT_AwakenedLuminary, HVT_AwakenedMantraist, HVT_AwakenedPerennial, HVT_AwakenedSage, HVT_AwakenedSiphoner, HVT_AwakenedTitan, HVT_AwakenedUndying                 | HVT_PsychicallyBlank                                                                   | AM_PsychicSensitivity                                                                          |
| AM_Sadist                   | Alpha Memes                      | HVT_Sadist                                                                                                                                                                                                                                                                                    | —                                                                                      | AM_Death, Corpses, OrganUse                                                                    |
| AM_Sharpshooter             | Alpha Memes                      | HVT_Sniper, HVT_Vicious                                                                                                                                                                                                                                                                       | —                                                                                      | *(opinion only)*                                                                               |
| AM_VampireHunting           | Alpha Memes                      | HVT_GenePurist, HVT_MonsterHunter                                                                                                                                                                                                                                                             | —                                                                                      | AM_DeathrestCaskets, Bloodfeeders                                                              |
| AM_WaterPrimacy             | Alpha Memes                      | HVT_Mariner                                                                                                                                                                                                                                                                                   | —                                                                                      | AM_Rain                                                                                        |
| AM_Youth                    | Alpha Memes                      | HVT_Caretaker                                                                                                                                                                                                                                                                                 | HVT_Misopedist                                                                         | VME_Elders                                                                                     |
| BMT_CavernDweller           | Biomes! Caverns                  | —                                                                                                                                                                                                                                                                                             | HVT_Tempestophile, HVT_Outdoorsy                                                       | FungusEating, Indoors                                                                          |
| HAR_Xenophilia              | Humanoid Alien Races             | HVT_MonsterLover                                                                                                                                                                                                                                                                              | HVT_GenePurist                                                                         | LanguageLearning                                                                               |
| HAR_Xenophobia              | Humanoid Alien Races             | HVT_GenePurist                                                                                                                                                                                                                                                                                | HVT_MonsterLover                                                                       | LanguageLearning                                                                               |
| LFM_Mechanization           | Lost Factory Mechanoids          | HVT_Mechaphile                                                                                                                                                                                                                                                                                | HVT_Mechaphobe                                                                         | *(not installed locally)*                                                                      |
| Seti_Militarism             | Millitarism Meme                 | —                                                                                                                                                                                                                                                                                             | HVT_Tranquil                                                                           | *(not installed locally)*                                                                      |
| MI_Environmentalist         | Mort's Ideologies                | HVT_Outdoorsy, HVT_Environmentalist                                                                                                                                                                                                                                                           | —                                                                                      | MI_Pollution, MI_ToxicWasteDumping, Trees                                                      |
| MI_Faith                    | Mort's Ideologies                | HVT_Faithful, HVT_PersecutionComplex, HVT_Proclaimer, HVT_Superstitious                                                                                                                                                                                                                       | HVT_Doubtful                                                                           | Research                                                                                       |
| MI_GovernmentAuthority      | Mort's Ideologies                | HVT_Subjugator, HVT_Servile                                                                                                                                                                                                                                                                   | HVT_Vagabond, HVT_Anarchist                                                            | Execution, MI_Leader, Slavery                                                                  |
| MI_GovernmentLiberty        | Mort's Ideologies                | HVT_Vagabond, HVT_Liberator, HVT_Anarchist                                                                                                                                                                                                                                                    | HVT_Subjugator, HVT_Servile                                                            | DrugUse, Execution, MI_Leader, Slavery                                                         |
| MI_Industrialist            | Mort's Ideologies                | —                                                                                                                                                                                                                                                                                             | HVT_Environmentalist                                                                   | MI_Pollution, MI_ToxicWasteDumping                                                             |
| MI_Menagerist               | Mort's Ideologies                | —                                                                                                                                                                                                                                                                                             | HVT_Agrizoophobe                                                                       | *(opinion only)*                                                                               |
| NikolaisIdeology_Aphroditic | Nikolai's Ideology: Gender Works | HVT_Hedonist, HVT_Lech, HVT_Lovesick, HVT_Vain                                                                                                                                                                                                                                                | HVT_Asocial, HVT_Bookworm, HVT_Twisted                                                 | AM_FertilityIssue, AM_LovinFrequency, Lovin, VME_Recreation                                    |
| QuesterMeme_QuesterMeme     | Questing Meme                    | HVT_Outdoorsy                                                                                                                                                                                                                                                                                 | —                                                                                      | QuesterMeme_QuestComplete, QuesterMeme_QuestFail                                               |
| SM_slaughtering             | Slaughtering Meme                | HVT_Graver, HVT_Vicious                                                                                                                                                                                                                                                                       | HVT_Tranquil                                                                           | *(not installed locally)*                                                                      |
| MP3_Survivalist             | Survivalist Meme                 | HVT_Outdoorsy                                                                                                                                                                                                                                                                                 | —                                                                                      | *(not installed locally)*                                                                      |
| VFEA_Isolationist           | VFE – Ancients                   | —                                                                                                                                                                                                                                                                                             | HVT_Champion, HVT_Conversationalist                                                    | VFEA_BeingRecruited, VFEA_Recruiting                                                           |
| VFEP_PirateMeme             | VFE – Pirates                    | HVT_Champion, HVT_Conversationalist, HVT_Vicious                                                                                                                                                                                                                                              | HVT_Tranquil                                                                           | *(not installed locally)*                                                                      |
| VME_Angler                  | VIE – Memes and Structures       | HVT_Outdoorsy, HVT_Angler, HVT_Pescatarian                                                                                                                                                                                                                                                    | HVT_Agrizoophobe                                                                       | Ranching, VME_Recreation                                                                       |
| VME_Anonymity               | VIE – Memes and Structures       | HVT_Asocial, HVT_Curmudgeon, HVT_Skulker                                                                                                                                                                                                                                                      | HVT_PeoplePleaser, HVT_Prideful                                                        | VME_Anonymity, VME_SocialInteractions                                                          |
| VME_Aristocratic            | VIE – Memes and Structures       | HVT_Prideful, HVT_Subjugator, HVT_Servile                                                                                                                                                                                                                                                     | HVT_Anarchist                                                                          | VME_DumbLabor, VME_Power                                                                       |
| VME_Astrology               | VIE – Memes and Structures       | HVT_Daydreamer, HVT_Tempestophile                                                                                                                                                                                                                                                             | —                                                                                      | VME_Mood                                                                                       |
| VME_BloodCourt              | VIE – Memes and Structures       | HVT_Graver, HVT_Sadist, HVT_Vicious, HVT_WeaponArtist                                                                                                                                                                                                                                         | —                                                                                      | Apostasy, Corpses, VME_Death, VME_Leader, VME_Scars                                            |
| VME_Bushido                 | VIE – Memes and Structures       | HVT_Subjugator                                                                                                                                                                                                                                                                                | HVT_Skulker, HVT_Tranquil, HVT_Liberator, HVT_Anarchist                                | Pain, Raiding, Slavery                                                                         |
| VME_CityBuilders            | VIE – Memes and Structures       | —                                                                                                                                                                                                                                                                                             | HVT_Outdoorsy, HVT_Vagabond, HVT_Globetrotter, HVT_Environmentalist                    | VME_PermanentBases, VME_Travel                                                                 |
| VME_CraftCulture            | VIE – Memes and Structures       | HVT_Aestheticist, HVT_Vain                                                                                                                                                                                                                                                                    | —                                                                                      | VME_CraftingQuality, VME_Leader                                                                |
| VME_Cultured                | VIE – Memes and Structures       | HVT_Aestheticist, HVT_Bookworm, HVT_Daydreamer, HVT_Hedonist                                                                                                                                                                                                                                  | —                                                                                      | VME_BookReading, VME_BookReadingSpeed, VME_BookWriting                                         |
| VME_Egalitarian             | VIE – Memes and Structures       | HVT_Champion, HVT_Liberator, HVT_Tolerant, HVT_Anarchist                                                                                                                                                                                                                                      | HVT_Sadist, HVT_Intolerant, HVT_Subjugator, HVT_Servile                                | Apostasy, IdeoDiversity, Proselytizing, Slavery                                                |
| VME_Emancipation            | VIE – Memes and Structures       | HVT_Champion, HVT_Humble, HVT_Liberator, HVT_Anarchist                                                                                                                                                                                                                                        | HVT_Sadist, HVT_Subjugator                                                             | Slavery                                                                                        |
| VME_ExaltedPriesthood       | VIE – Memes and Structures       | HVT_Servile                                                                                                                                                                                                                                                                                   | HVT_Anarchist                                                                          | *(opinion only)*                                                                               |
| VME_Fleshcrafters           | VIE – Memes and Structures       | HVT_Graver                                                                                                                                                                                                                                                                                    | —                                                                                      | BodyModification, Corpses, OrganUse, Scarification, VME_Death                                  |
| VME_Gestalt                 | VIE – Memes and Structures       | HVT_Champion, HVT_Allegiant, HVT_Humble, HVT_Conformist                                                                                                                                                                                                                                       | HVT_Vagabond                                                                           | VME_Mood                                                                                       |
| VME_GodEmperor              | VIE – Memes and Structures       | HVT_Allegiant, HVT_Subjugator, HVT_Servile                                                                                                                                                                                                                                                    | HVT_Anarchist                                                                          | VME_LeaderDivinity, VME_Power                                                                  |
| VME_HardcoreIndustrialism   | VIE – Memes and Structures       | HVT_Staid                                                                                                                                                                                                                                                                                     | —                                                                                      | Apostasy, VME_DumbLabor                                                                        |
| VME_InsectoidSupremacy      | VIE – Memes and Structures       | —                                                                                                                                                                                                                                                                                             | HVT_Agrizoophobe                                                                       | InsectMeat, VME_InsectJelly, VME_Insectoids                                                    |
| VME_Light                   | VIE – Memes and Structures       | HVT_Sunbather                                                                                                                                                                                                                                                                                 | HVT_Skulker                                                                            | DarknessCombat, Eclipse, Lighting, VME_Aurora                                                  |
| VME_MechanoidSupremacy      | VIE – Memes and Structures       | HVT_Mechaphile                                                                                                                                                                                                                                                                                | HVT_Mechaphobe                                                                         | VME_Mechanoids                                                                                 |
| VME_Nationalist             | VIE – Memes and Structures       | HVT_Champion, HVT_Allegiant, HVT_Prideful, HVT_Conformist, HVT_Intolerant                                                                                                                                                                                                                     | HVT_Tranquil, HVT_Vagabond, HVT_Tolerant                                               | Execution, IdeoDiversity, VME_Death                                                            |
| VME_Nomad                   | VIE – Memes and Structures       | HVT_Outdoorsy, HVT_Globetrotter                                                                                                                                                                                                                                                               | —                                                                                      | Ranching, RoughLiving, VME_PermanentBases, VME_Travel                                          |
| VME_Pacifist                | VIE – Memes and Structures       | HVT_Tranquil, HVT_Liberator                                                                                                                                                                                                                                                                   | HVT_Sniper, HVT_Sadist, HVT_Vicious, HVT_WeaponArtist, HVT_Subjugator                  | Execution, Raiding, Slavery, VME_Violence                                                      |
| VME_PartyLife               | VIE – Memes and Structures       | HVT_Conversationalist, HVT_Hedonist, HVT_Reveller                                                                                                                                                                                                                                             | —                                                                                      | VME_Alcohol, VME_Recreation                                                                    |
| VME_Progressive             | VIE – Memes and Structures       | HVT_Mechaphile                                                                                                                                                                                                                                                                                | —                                                                                      | VME_AutomationEfficiency                                                                       |
| VME_PsychicFocus            | VIE – Memes and Structures       | HVT_LatentPsychic, HVT_AwakenedAugur, HVT_AwakenedChanshi, HVT_AwakenedDeluge, HVT_AwakenedErudite, HVT_AwakenedIncarnate, HVT_AwakenedLuminary, HVT_AwakenedMantraist, HVT_AwakenedPerennial, HVT_AwakenedSage, HVT_AwakenedSiphoner, HVT_AwakenedTitan, HVT_AwakenedUndying                 | HVT_PsychicallyBlank                                                                   | VME_Leader, VME_PsychicSensitivity                                                             |
| VME_Republic                | VIE – Memes and Structures       | HVT_Subjugator, HVT_Servile                                                                                                                                                                                                                                                                   | —                                                                                      | VME_Leader                                                                                     |
| VME_Royal                   | VIE – Memes and Structures       | HVT_Servile                                                                                                                                                                                                                                                                                   | HVT_Anarchist                                                                          | VME_Leader, VME_Royalty                                                                        |
| VME_Scrapper                | VIE – Memes and Structures       | HVT_Environmentalist, HVT_Scavenger                                                                                                                                                                                                                                                           | HVT_Textile                                                                            | VME_Junk, VME_TaintedApparel, VME_TatteredApparel                                              |
| VME_SweetTeeth              | VIE – Memes and Structures       | HVT_Hedonist                                                                                                                                                                                                                                                                                  | —                                                                                      | VME_Sweets                                                                                     |
| VME_Trader                  | VIE – Memes and Structures       | —                                                                                                                                                                                                                                                                                             | HVT_Asocial                                                                            | VME_Expectations                                                                               |
| VME_Vegan                   | VIE – Memes and Structures       | —                                                                                                                                                                                                                                                                                             | HVT_Pescatarian                                                                        | AnimalSlaughter, MeatEating, Ranching, VME_LeatherApparel                                      |
| VME_ViolentConversion       | VIE – Memes and Structures       | HVT_Allegiant, HVT_Sadist, HVT_Intolerant, HVT_Subjugator, HVT_Proclaimer                                                                                                                                                                                                                     | HVT_Tranquil, HVT_Liberator, HVT_Anarchist                                             | IdeoDiversity, Proselytizing, Slavery                                                          |
| GR_MadScientists            | Vanilla Genetics Expanded        | HVT_Visionary, HVT_MonsterLover                                                                                                                                                                                                                                                               | HVT_GenePurist, HVT_MonsterHunter                                                      | AutonomousWeapons                                                                              |
| VVE_Skyseekers              | Vanilla Vehicles Expanded        | HVT_Skybound                                                                                                                                                                                                                                                                                  | HVT_Earthborne                                                                         | Indoors, Research, VVE_Flying                                                                  |
| VVE_Travelers               | Vanilla Vehicles Expanded        | HVT_Vagabond, HVT_Globetrotter                                                                                                                                                                                                                                                                | —                                                                                      | RoughLiving, VVE_Driving, VVE_Flying, VVE_Sailing                                              |
| APMimeMeme                  | [AP] Mime Meme                   | HVT_Daydreamer, HVT_Skulker                                                                                                                                                                                                                                                                   | HVT_Staid                                                                              | *(not installed locally)*                                                                      |
| AP_Slaveholding_Meme        | [AP] Slaveholding                | HVT_Subjugator                                                                                                                                                                                                                                                                                | HVT_Liberator                                                                          | *(not installed locally)*                                                                      |
| LYN_MilitaryMeme            | [LYN] Military Ideology          | —                                                                                                                                                                                                                                                                                             | HVT_Tranquil                                                                           | *(not installed locally)*                                                                      |

### The Sims Traits

| Meme             | Meme from                  | Agreeable                     | Disagreeable   | Debatable issues                                                                                       |
|------------------+----------------------------+-------------------------------+----------------+--------------------------------------------------------------------------------------------------------|
| Bloodfeeding     | Biotech                    | —                             | ST_Vegan       | Bloodfeeders, Cannibalism, Execution, OrganUse                                                         |
| AnimalPersonhood | Ideology                   | ST_Vegan                      | —              | AnimalSlaughter, Fishing, KillingInnocentAnimals, MeatEating                                           |
| Blindsight       | Ideology                   | —                             | ST_HealthNut   | Blindness                                                                                              |
| FleshPurity      | Ideology                   | ST_HealthNut                  | —              | BodyModification, DrugUse, GrowthVat, OrganUse                                                         |
| Nudism           | Ideology                   | —                             | ST_Proper      | Nudity_Female, Nudity_Male                                                                             |
| PainIsVirtue     | Ideology                   | —                             | ST_PartyAnimal | Cannibalism, Comfort, Corpses, Execution, Lovin, Pain, RoughLiving, Scarification, Skullspike, Slavery |
| Proselytizer     | Ideology                   | ST_Devout                     | —              | Apostasy, Proselytizing                                                                                |
| Rancher          | Ideology                   | —                             | ST_Vegan       | Ranching                                                                                               |
| VME_Angler       | VIE – Memes and Structures | —                             | ST_Vegan       | Ranching, VME_Recreation                                                                               |
| VME_Aristocratic | VIE – Memes and Structures | ST_Proper, ST_HighMaintenance | —              | VME_DumbLabor, VME_Power                                                                               |
| VME_Cultured     | VIE – Memes and Structures | ST_Bookworm                   | —              | VME_BookReading, VME_BookReadingSpeed, VME_BookWriting                                                 |
| VME_Healthcare   | VIE – Memes and Structures | ST_HealthNut                  | —              | *(opinion only)*                                                                                       |
| VME_HolyDiseases | VIE – Memes and Structures | —                             | ST_HealthNut   | Pain, VME_Illness                                                                                      |
| VME_PartyLife    | VIE – Memes and Structures | ST_PartyAnimal                | —              | VME_Alcohol, VME_Recreation                                                                            |
| VME_Trader       | VIE – Memes and Structures | ST_Materialistic              | —              | VME_Expectations                                                                                       |

### Notable interactions

- **HVT_Allegiant** is agreeable for Collectivist and Loyalist and disagreeable for Individualist, so it acts as a general "loyal follower" trait.
  Through `associatedMemes`, Collectivist reaches six debatable issues (ChildLabor, DrugUse, Execution, Lovin, OrganUse, Slavery) and Loyalist reaches Apostasy.
- **HVT_Sadist** is agreeable for Raider, Supremacist and several modded cruelty memes, so it stacks conviction on Execution and Corpses.
- **Blindsight** lists every Hauts psychic trait (LatentPsychic and the Awakened traits) as agreeable.
  A colony with many Hauts psychics leans toward Blindsight faiths.
- **ST_Devout** and **HVT_Proclaimer** are both agreeable for Proselytizer.
  Under RimPsyche, both traits also raise Passion, which the planned K3 debate knob reads.

---

## Traits with no meme affinity

These traits appear in no meme's `agreeableTraits` or `disagreeableTraits`, so they have no effect on ideo opinion or conviction today.
The list is meant for deciding which of them should get a link to precepts by some other route.

How the list was built:

- Traits come from the base game with all DLCs, and from the 1.6 defs of each trait mod that RimPsyche supports.
  Consolidated Traits is the old [KV] 1.3 source, because the [LC] continuation has no public repository.
- Traits that a mod redefines from vanilla are listed under Vanilla only.
- A trait counts as having an affinity if any of its degrees appears in any meme.
  That includes vanilla memes, the meme mods installed locally, and the Hauts' and Sims patches above.
  So some vanilla traits (Kind, Bloodlust, Brawler, Pyromaniac, Asexual, Industriousness, SpeedOffset) are absent here only because a locally installed meme mod lists them.
- The RimPsyche columns show which personality nodes each trait scopes and which facets it gates (see [personality_integration.md](personality_integration.md)).
  A trait with no meme affinity but with RimPsyche hooks already affects belief through the planned knobs when RimPsyche is active.

### Vanilla

| Trait              | meme/precepts | Label                                                                                                   | RimPsyche scopes                            | RimPsyche gates                                                      |
|--------------------+---------------+---------------------------------------------------------------------------------------------------------+---------------------------------------------+----------------------------------------------------------------------|
| Psychopath         | cannibal (+), animal personhood (-), charity (-), loyalist (-), individualist (+), inhuman (+)              | psychopath                                                                                              | Compassion, Emotionality, Morality, Tension | Compassion, Humbleness, Insecurity, Integrity, Pessimism, Volatility |
| Abrasive           |               | abrasive                                                                                               | Tact                                        | —                                                                    |
| TooSmart           |               | too smart                                                                                               | Reflectiveness, Tension                     | Intellect                                                            |
| NightOwl           |               | night owl                                                                                               | —                                           | —                                                                    |
| Greedy             |               | greedy                                                                                                  | Expectation, SelfInterest                   | —                                                                    |
| Jealous            | individualism               | jealous                                                                                                 | Competitiveness                             | Humbleness                                                           |
| Gay                |               | gay                                                                                                     | —                                           | —                                                                    |
| Bisexual           |               | bisexual                                                                                                | —                                           | —                                                                    |
| AnnoyingVoice      |               | annoying voice                                                                                          | —                                           | —                                                                    |
| CreepyBreathing    |               | creepy breathing                                                                                        | —                                           | —                                                                    |
| Nimble             |               | nimble                                                                                                  | —                                           | —                                                                    |
| FastLearner        |               | fast learner                                                                                            | —                                           | —                                                                    |
| SlowLearner        |               | slow learner                                                                                            | —                                           | —                                                                    |
| GreatMemory        |               | great memory                                                                                            | —                                           | —                                                                    |
| Tough              |               | tough                                                                                                   | —                                           | —                                                                    |
| QuickSleeper       |               | quick sleeper                                                                                           | —                                           | —                                                                    |
| NaturalMood        |               | sanguine (2), optimist (1), pessimist (-1), depressive (-2)                                             | Optimism                                    | Pessimism                                                            |
| Nerves             |               | iron-willed (2), steadfast (1), nervous (-1), volatile (-2)                                             | Tenacity, Tension                           | Volatility                                                           |
| Neurotic           |               | neurotic (1), very neurotic (2)                                                                         | Organization, Tension                       | Orderliness                                                          |
| PsychicSensitivity |               | psychically hypersensitive (2), psychically sensitive (1), psychically dull (-1), psychically deaf (-2) | —                                           | —                                                                    |
| ShootingAccuracy   |               | careful shooter (1), trigger-happy (-1)                                                                 | Deliberation                                | —                                                                    |
| Beauty             |               | beautiful (2), pretty (1), ugly (-1), staggeringly ugly (-2)                                            | —                                           | —                                                                    |
| Immunity           |               | super-immune (1), sickly (-1)                                                                           | —                                           | —                                                                    |
| Delicate           |               | delicate                                                                                                | —                                           | —                                                                    |
| Recluse            |               | recluse                                                                                                 | Sociability                                 | Sociability                                                          |

### Vanilla Traits Expanded

| Trait               | Label          | RimPsyche scopes            | RimPsyche gates |
|---------------------+----------------+-----------------------------+-----------------|
| VTE_AbsentMinded    | absent-minded  | Focus, Imagination          | —               |
| VTE_Eccentric       | eccentric      | Reflectiveness, Sociability | Intellect       |
| VTE_Perfectionist   | perfectionist  | Deliberation, Tension       | —               |
| VTE_Coward          | coward         | Bravery                     | —               |
| VTE_Brave           | brave          | Bravery                     | —               |
| VTE_Clumsy          | clumsy         | —                           | —               |
| VTE_HeatInclined    | heat inclined  | —                           | —               |
| VTE_ColdInclined    | cold inclined  | —                           | —               |
| VTE_HeavySleeper    | heavy sleeper  | —                           | —               |
| VTE_Neat            | neat           | Organization                | —               |
| VTE_Slob            | slob           | Organization                | —               |
| VTE_Workaholic      | workaholic     | Diligence, Focus            | —               |
| VTE_IronStomach     | iron stomach   | —                           | —               |
| VTE_AnimalLover     | animal lover   | —                           | —               |
| VTE_AnimalHater     | animal hater   | —                           | —               |
| VTE_BigBoned        | big-boned      | —                           | —               |
| VTE_Rebel           | rebel          | Loyalty, Morality           | —               |
| VTE_Submissive      | submissive     | —                           | Assertiveness   |
| VTE_Vengeful        | vengeful       | Compassion                  | —               |
| VTE_Technophobe     | technophobe    | —                           | —               |
| VTE_FunLoving       | fun-loving     | Playfulness                 | —               |
| VTE_Dunce           | dunce          | Reflectiveness              | Intellect       |
| VTE_MartialArtist   | martial artist | —                           | —               |
| VTE_Snob            | snob           | Expectation                 | Humbleness      |
| VTE_Squeamish       | squeamish      | —                           | —               |
| VTE_Anxious         | anxious        | Sociability, Tension        | Insecurity      |
| VTE_Insatiable      | insatiable     | —                           | —               |
| VTE_Prude           | prude          | Propriety                   | —               |
| VTE_CouchPotato     | couch potato   | —                           | —               |
| VTE_CatPerson       | cat person     | —                           | —               |
| VTE_DogPerson       | dog person     | —                           | —               |
| VTE_Prodigy         | prodigy        | Reflectiveness, Tension     | Intellect       |
| VTE_Menagerist      | menagerist     | —                           | —               |
| VTE_ChildOfSea      | Ocean lover    | —                           | —               |
| VTE_ChildOfMountain | Mountain lover | —                           | —               |
| VTE_MadSurgeon      | mad surgeon    | Compassion, Morality        | Compassion      |
| VTE_DrunkenMaster   | drunken master | —                           | —               |
| VTE_Kleptomaniac    | kleptomaniac   | Discipline, SelfInterest    | —               |
| VTE_Stoner          | stoner         | —                           | —               |
| VTE_Lush            | lush           | —                           | —               |
| VTE_Wanderlust      | wanderlust     | —                           | —               |
| VTE_Insomniac       | insomniac      | —                           | —               |
| VTE_WorldWeary      | world weary    | Optimism, Sociability       | Pessimism       |
| VTE_Desensitized    | desensitized   | Compassion, Emotionality    | —               |
| VTE_RefinedPalate   | refined palate | —                           | —               |
| VTE_Academian       | academian      | Reflectiveness              | Intellect       |
| VTE_Groundbreaker   | groundbreaker  | —                           | —               |
| VTE_Gastronomist    | gastronomist   | —                           | —               |
| VTE_Ecologist       | ecologist      | —                           | —               |
| VTE_Tycoon          | tycoon         | —                           | —               |
| VTE_ThickSkinned    | thick skinned  | —                           | —               |
| VTE_ThinSkinned     | thin skinned   | —                           | —               |
| VTE_Schizoid        | schizoid       | —                           | —               |

### Hauts' Added Traits

| Trait | Label | RimPsyche scopes | RimPsyche gates |
|-------+-------+------------------+-----------------|
| HVT_Drudge | drudge | Deliberation, Reflectiveness | Intellect |
| HVT_Forgettable | forgettable | — | — |
| HVT_Hale | hale | — | — |
| HVT_Hotfoot | hotfoot | — | — |
| HVT_Hysteric | hysteric | Tenacity | — |
| HVT_Judgemental | judgemental | Openness, Trust | — |
| HVT_Rambunctious | rambunctious | — | — |
| HVT_RepressedRage | repressed rage | Aggressiveness | — |
| HVT_Tranquil0 | tranquil | — | — |
| HVT_Winsome | winsome | — | — |
| HVT_ChronicNightmares | chronic nightmares | — | — |
| HVT_VividDreams | vivid dreams | — | — |
| HVT_Imperceptive | imperceptive | — | — |
| HVT_Strong | mighty (1), herculean (2) | — | — |
| HVT_Guru | guru | Tact | — |
| HVT_RadicalThinker | radical thinker | Passion, Reflectiveness | Intellect |
| HVT_Mentor | mentor | Competitiveness | — |
| HVT_Catastrophist | catastrophist | Compassion | — |
| HVT_Haunted | haunted | — | — |
| HVT_CelestialCelerity | celestial celerity | — | — |
| HVT_Navigator | navigator | — | — |
| HVT_Test | game developer | — | — |
| HVT_Everliving | everliving | — | — |
| HVT_Vanquisher | vanquisher | — | — |
| HVT_Compromised | compromised | — | — |
| HVT_Pathogenic | pathogenic | — | — |
| HVT_ChanshiGene | chanshi gene | — | — |
| HVT_TTrait\* (122 traits) | psychic albatross … psychic ziz, plus locustspawn and doppelganger | — | — |

### The Sims Traits

| Trait             | Label           | RimPsyche scopes        | RimPsyche gates |
|-------------------+-----------------+-------------------------+-----------------|
| ST_Manipulative   | manipulative    | —                       | —               |
| ST_Naive          | naive           | Trust                   | —               |
| ST_Narcissist     | vain            | SelfInterest            | —               |
| ST_Emotional      | emotional       | Emotionality            | —               |
| ST_Paranoid       | paranoid        | Optimism                | —               |
| ST_Goofball       | goofball        | Playfulness             | —               |
| ST_Loyal          | loyal           | Loyalty                 | —               |
| ST_Insane         | insane          | Stability               | —               |
| ST_Grumpy         | grumpy          | Aggressiveness          | —               |
| ST_Shy            | shy             | —                       | Assertiveness   |
| ST_Procrastinator | procrastinator  | Diligence, Organization | —               |
| ST_NonCommital    | non-committal   | Loyalty                 | —               |
| ST_FamilyOriented | family oriented | Loyalty, Sociability    | —               |
| ST_SteadyHands    | steady hands    | Deliberation            | —               |
| ST_Daredevil      | daredevil       | Bravery                 | —               |
| ST_Chatterbox     | chatterbox      | Talkativeness           | —               |
| ST_Handy          | handy           | —                       | —               |
| ST_Virtuoso       | virtuoso        | —                       | Imagination     |
| ST_TechWhiz       | tech whiz       | —                       | —               |
| ST_Gambler        | gambler         | Discipline              | —               |
| ST_Childish       | childish        | Playfulness             | —               |
| ST_Nosy           | nosy            | Inquisitiveness         | —               |
| ST_HugePower      | huge power      | —                       | —               |
| ST_Observant      | observant       | —                       | —               |
| ST_Zen            | zen             | Aggressiveness          | Imagination     |
| ST_Submissive     | submissive      | —                       | Assertiveness   |
| ST_DrunkenMaster  | drunken master  | —                       | —               |
| ST_Insomniac      | insomniac       | —                       | —               |

### SYR Individuality

| Trait                   | Label                 | RimPsyche scopes      | RimPsyche gates |
|-------------------------+-----------------------+-----------------------+-----------------|
| SYR_Agile               | stealthy              | —                     | —               |
| SYR_AnimalAffinity      | animal friend         | —                     | —               |
| SYR_Architect           | architect             | —                     | —               |
| SYR_CreativeThinker     | creative thinker      | Imagination           | —               |
| SYR_GoodFortune         | fortunate             | —                     | —               |
| SYR_GreenThumb          | green thumb           | —                     | —               |
| SYR_GunNut              | gun nut               | —                     | —               |
| SYR_HandEyeCoordination | hand-eye coordination | —                     | —               |
| SYR_KeenEye             | keen eye              | —                     | —               |
| SYR_MechanoidExpert     | mechanoid expert      | —                     | —               |
| SYR_Perfectionist       | perfectionist         | Deliberation, Tension | —               |
| SYR_Haggler             | silver tongue         | Tact                  | —               |
| SYR_SteadyHands         | steady hands          | —                     | —               |
| SYR_StrongBack          | strong back           | —                     | —               |

### Bad People

| Trait               | Label     | RimPsyche scopes     | RimPsyche gates       |
|---------------------+-----------+----------------------+-----------------------|
| BadPeople_Evil      | Depraved  | Compassion, Morality | Compassion, Integrity |
| BadPeople_Kinslayer | Kinslayer | Compassion, Morality | Compassion, Integrity |

### Big and Small

| Trait                   | Label                 | RimPsyche scopes | RimPsyche gates |
|-------------------------+-----------------------+------------------+-----------------|
| Passioned_Learning      | diligent student      | —                | —               |
| Very_Passioned_Learning | very diligent student | —                | —               |
| BS_AnimalFriend         | Animal Friend         | —                | —               |
| BS_AlcoholAddict        | alcohol lover         | Discipline       | —               |
| BS_Gentle               | gentle                | Aggressiveness   | Compassion      |
| BS_InsultProof          | uninsultable          | Stability        | —               |
| BS_Unpretentious        | Unpretentious         | Expectation      | —               |
| Gigantism               | gigantism             | —                | —               |
| Large                   | large                 | —                | —               |
| Small                   | small                 | —                | —               |
| Dwarfism                | dwarfism              | —                | —               |

### Consolidated Traits

| Trait              | Label                                                 | RimPsyche scopes                       | RimPsyche gates |
|--------------------+-------------------------------------------------------+----------------------------------------+-----------------|
| RCT_Aesthete       | aesthete                                              | —                                      | Imagination     |
| RCT_AnimalLover    | animal lover                                          | —                                      | —               |
| RCT_AnimalHater    | animal hater                                          | —                                      | —               |
| RCT_Builder        | builder                                               | —                                      | —               |
| RCT_Butcher        | butcher                                               | —                                      | —               |
| RCT_Claustrophobic | claustrophobic                                        | —                                      | —               |
| RCT_ColdLover      | cold lover                                            | —                                      | —               |
| RCT_DeepSleeper    | deep sleeper                                          | —                                      | —               |
| RCT_Dunce          | dunce                                                 | Reflectiveness                         | Intellect       |
| RCT_Gourmet        | gourmet                                               | —                                      | —               |
| RCT_HeatLover      | heat lover                                            | —                                      | —               |
| RCT_Aptitude       | inept (-1), coordinated (1)                           | —                                      | —               |
| RCT_PainThreshold  | unstoppable (2), ironman (1), low pain tolerance (-1) | —                                      | —               |
| RCT_Inventor       | inventor                                              | Focus, Imagination                     | —               |
| RCT_Eyesight       | cranial nerve palsy (-1), eagle-eyed (1)              | —                                      | —               |
| RCT_NeatFreak      | neat freak                                            | Organization                           | —               |
| RCT_EatingSpeed    | nibbler (-1), glutton (1)                             | —                                      | —               |
| RCT_Nyctophobe     | nyctophobe (-1), nyctophile (1)                       | —                                      | —               |
| RCT_Perfectionist  | perfectionist                                         | Deliberation, Tension                  | —               |
| RCT_Medic          | poor medic (-1), skilled medic (1), master medic (2)  | —                                      | —               |
| RCT_Rockhound      | rockhound                                             | —                                      | —               |
| RCT_Savant         | savant                                                | Diligence, Imagination, Reflectiveness | Intellect       |
| RCT_decentlearner  | decent learner                                        | —                                      | —               |
| RCT_Trader         | sucker (-1), haggler (1), master trader (2)           | —                                      | —               |
| RCT_Diplomat       | uncouth (-1), diplomat (1), master diplomat (2)       | Tact                                   | —               |
| RCT_Constitution   | weak constitution (-1), strong constitution (1)       | —                                      | —               |
| RCT_BrownThumb     | brown thumb                                           | —                                      | —               |
| RCT_GreenThumb     | Green thumb                                           | —                                      | —               |
