# Meme and Precept Compatibility

Enhanced Ideology explicitly integrates with the following optional mods.

## [Alpha Memes](https://steamcommunity.com/sharedfiles/filedetails/?id=2661356814)

Full support.
All Alpha Memes issues are classified and their precept ladders are integrated into the structural opinion model.

## [Big and Small — Genes & More](https://steamcommunity.com/sharedfiles/filedetails/?id=2920751126)

Obligate diet genes affect both ideo opinion and belief seeding.

Pawns with the **Herbivore** gene (`BS_Diet_Herbivore`) gain a +20 structural opinion bonus toward ideos that hold a `Disapproved`, `Horrible`, or `Abhorrent` stance on meat eating.
If the target ideo also carries the **Vegan** meme ([Vanilla Ideology Expanded — Memes and Structures](https://steamcommunity.com/sharedfiles/filedetails/?id=2636329500)), an additional +15 is applied.
On spawn, these pawns are seeded with a strong stance at `MeatEating_Abhorrent` (strength 8–20), applied after normal seeding and heterodoxy, so the gene-driven position is the final word.

Pawns with the **Carnivore** gene (`BS_Diet_Carnivore`) get the mirror treatment: +20 toward ideos that hold a `NonMeat_Disapproved`, `NonMeat_Horrible`, or `NonMeat_Abhorrent` stance, and are seeded at `MeatEating_NonMeat_Abhorrent`.

If Big and Small is not installed, all of the above is safely skipped.

## [Waymakers](https://steamcommunity.com/sharedfiles/filedetails/?id=3761384757)

The `infrastructure` precept is treated as an ideo-specific obligation.
Colonists don't penalise other ideos for not sharing it.

## [Mort's Ideologies: Conservationist & Polluter](https://steamcommunity.com/sharedfiles/filedetails/?id=2936965087)

**Pollution** and **toxic waste dumping** are full moral axes.
A Conservationist ideo and an Industrialist ideo will structurally dislike each other's stance on both issues.

In addition, the Conservationist and Industrialist memes carry a direct **−15 cross-meme opinion penalty** — the fundamental worldview conflict is registered even beyond the precept system.

Power generation and world pollution precepts are treated as ideo-specific obligations and don't generate cross-ideo tension.

## [Mort's Ideologies: Empiricism & Faith](https://steamcommunity.com/sharedfiles/filedetails/?id=2948947009)

All mechanical precepts (conversion power, meditation speed, pruning speed, certainty gain, laboratory requirement) are ideo-specific stat modifiers and don't generate inter-ideo tension on their own.

However, the **Empiricist** and **Faith** memes carry a **−5 cross-meme opinion penalty**, reflecting their fundamental epistemological conflict.

## [Mort's Ideologies: Menagerist](https://steamcommunity.com/sharedfiles/filedetails/?id=3514363516)

Zookeeping and animal variety precepts are treated as ideo-specific obligations.
No cross-ideo tension is generated — an ideo that doesn't require a zoo simply holds no position on the issue.

---

## [Mort's Ideologies: Political Compass](https://steamcommunity.com/sharedfiles/filedetails/?id=2932322843)

**Housing distribution** (`strict equality` through `strict stratification`) is a full moral axis.
Equality-minded and stratification-minded ideos will structurally disagree based on rung distance.

**Government type** (`anarchy` through `dictatorship`) is also a full moral axis, with the ladder running: anarchy → democracy → corporate → monarchy → dictatorship.

The **Government Liberty** and **Government Authority** memes carry a **−10 cross-meme opinion penalty**.
The **Wealth Equality** and **Wealth Stratification** memes carry a **−10 cross-meme opinion penalty**.
(These axes are implemented as memes rather than precepts, so the cross-meme penalty is the only way to capture the tension.)

The `DrugUse` and `BodyModification` ladders — which Political Compass extends with permissive mid-range rungs — are reordered correctly so the new rungs sit at the right point on the strictness axis.

Homelessness precepts are treated as ideo-specific obligations and don't generate cross-ideo tension.

## [Questing Meme](https://steamcommunity.com/sharedfiles/filedetails/?id=2826539854)

The `completing quests` and `failing quests` issues are treated as moral-axis beliefs.
Ideos that treat quest completion as respectful vs. daring, or that hold different views on how to regard failure, will generate genuine structural disagreement with opposing positions.
## [Vanilla Ideology Expanded — Memes and Structures](https://steamcommunity.com/sharedfiles/filedetails/?id=2636329500)

Full support.
All VIE issues are integrated.
Where VIE rungs extend existing base-game issue ladders (e.g., `Apostasy`, `BodyModification`, `Slavery`), the combined ladder is ordered correctly so opinion distance is computed on the merged axis.


# Other Mods

These mods don't add ideology content but have explicit compatibility or are common pain points that have been explicitly checked.

## [Vanilla Books Expanded](https://steamcommunity.com/sharedfiles/filedetails/?id=2193152410)

The ideobook writing recipe is added to VBE's writers table when both mods are active.

## [Peer Pressure](https://steamcommunity.com/sharedfiles/filedetails/?id=3057626086) ([Continued](https://steamcommunity.com/sharedfiles/filedetails/?id=3605155621))

Integrated, but prefers the settings from Peer Pressure if both mods are active.

## [Conversion staff](https://steamcommunity.com/sharedfiles/filedetails/?id=2890481507)
Works because it modifies base stats that we just use
## [Combat Extended](https://steamcommunity.com/sharedfiles/filedetails/?id=2890901044)
seemed fine when I booted it, the systems don't really interact so I don't expect issues.
