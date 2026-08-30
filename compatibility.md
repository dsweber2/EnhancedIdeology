# Mod Compatibility

Enhanced Beliefs explicitly integrates with the following optional mods.
When a supported mod isn't installed, all its content is safely skipped — no crashes, no issues.

## Big and Small — Genes & More

Obligate diet genes affect both ideo opinion and belief seeding.

Pawns with the **Herbivore** gene (`BS_Diet_Herbivore`) gain a +20 structural opinion bonus toward ideos that hold a `Disapproved`, `Horrible`, or `Abhorrent` stance on meat eating.
If the target ideo also carries the **Vegan** meme (VIE — Memes & Structures), an additional +15 is applied.
On spawn, these pawns are seeded with a strong stance at `MeatEating_Abhorrent` (strength 8–20), applied after normal seeding and heterodoxy, so the gene-driven position is the final word.

Pawns with the **Carnivore** gene (`BS_Diet_Carnivore`) get the mirror treatment: +20 toward ideos that hold a `NonMeat_Disapproved`, `NonMeat_Horrible`, or `NonMeat_Abhorrent` stance, and are seeded at `MeatEating_NonMeat_Abhorrent`.

If Big and Small is not installed, all of the above is safely skipped.

## Alpha Memes

Full support.
All Alpha Memes issues are classified and their precept ladders are integrated into the structural opinion model.

## VIE — Memes & Structures

Full support.
All VIE issues are integrated.
Where VIE rungs extend existing base-game issue ladders (e.g., `Apostasy`, `BodyModification`, `Slavery`), the combined ladder is ordered correctly so opinion distance is computed on the merged axis.

## Questing Meme

The `completing quests` and `failing quests` issues are treated as moral-axis beliefs.
Ideos that treat quest completion as respectful vs. daring, or that hold different views on how to regard failure, will generate genuine structural disagreement with opposing positions.

## Waymakers

The `infrastructure` precept is treated as an ideo-specific obligation.
Colonists don't penalise other ideos for not sharing it.

## Mort's Ideologies: Conservationist & Polluter

**Pollution** and **toxic waste dumping** are full moral axes.
A Conservationist ideo and an Industrialist ideo will structurally dislike each other's stance on both issues.

In addition, the Conservationist and Industrialist memes carry a direct **−15 cross-meme opinion penalty** — the fundamental worldview conflict is registered even beyond the precept system.

Power generation and world pollution precepts are treated as ideo-specific obligations and don't generate cross-ideo tension.

## Mort's Ideologies: Political Compass

**Housing distribution** (`strict equality` through `strict stratification`) is a full moral axis.
Equality-minded and stratification-minded ideos will structurally disagree based on rung distance.

**Government type** (`anarchy` through `dictatorship`) is also a full moral axis, with the ladder running: anarchy → democracy → corporate → monarchy → dictatorship.

The **Government Liberty** and **Government Authority** memes carry a **−10 cross-meme opinion penalty**.
The **Wealth Equality** and **Wealth Stratification** memes carry a **−10 cross-meme opinion penalty**.
(These axes are implemented as memes rather than precepts, so the cross-meme penalty is the only way to capture the tension.)

The `DrugUse` and `BodyModification` ladders — which Political Compass extends with permissive mid-range rungs — are reordered correctly so the new rungs sit at the right point on the strictness axis.

Homelessness precepts are treated as ideo-specific obligations and don't generate cross-ideo tension.

## Mort's Ideologies: Empiricism & Faith

All mechanical precepts (conversion power, meditation speed, pruning speed, certainty gain, laboratory requirement) are ideo-specific stat modifiers and don't generate inter-ideo tension on their own.

However, the **Empiricist** and **Faith** memes carry a **−5 cross-meme opinion penalty**, reflecting their fundamental epistemological conflict.

## Mort's Ideologies: Menagerist

Zookeeping and animal variety precepts are treated as ideo-specific obligations.
No cross-ideo tension is generated — an ideo that doesn't require a zoo simply holds no position on the issue.
