# Enhanced Ideology — design

This document describes how belief works in Enhanced Ideology and why it works that way.
For where the code lives, see the `AGENTS.md` file in each `Source/` folder.
For the per-issue classification data, see [preceptPolicy.md](preceptPolicy.md); for the merged precept ladders of supported mods, see [modCompat.md](modCompat.md).

## Goals

Vanilla certainty is close to a time-averaged mood: happy pawns stay faithful, stressed pawns drift.
This mod replaces that with beliefs that pawns actually hold.

- A pawn's faith depends on how well their personal beliefs fit their ideoligion, not only on how they feel.
- Beliefs change through events the player can see and influence: debates, books, rituals, the moral guide.
- Conversion happens because a pawn has come to prefer another faith, not because a timer and a dice roll lined up.
- The behaviour stays the same regardless of tick rate, colony size, or how many issues a mod list adds.

## Stances

A pawn holds one **stance** per issue (`IssueDef`): a preferred rung on that issue's ladder and a conviction strength.
Opinion of any particular precept is derived from the stance, not stored.

### Ladders

An issue's ladder is its precepts ordered by `displayOrderInIssue`, with classic-mode fallback precepts removed.
Rank 0 is the most permissive rung and the top rank is the most restrictive.
Some mod combinations stack rungs from several sources in an order that does not follow the belief axis; [preceptPolicy.md](preceptPolicy.md) records the per-issue reorders.

An ideoligion that holds no precept for an issue does not have "no opinion" on it.
A vegan faith against a faith that is silent on animal slaughter is a real clash.
So a missing precept is a virtual **Don't-care rung**, placed per issue: at the permissive end for one-sided ladders, between two rungs for two-sided ones.

### Opinion of a rung

For a pawn whose preferred rung is `r` with strength `s`, the opinion of a target rung is

```
t       = |target - r| / maxDist         // maxDist = distance from r to the farther ladder end
opinion = s * (1 - t * (1 + oppositionScale))
```

The pawn fully agrees with its own rung (`+s`).
Agreement falls linearly with distance and reaches `-oppositionScale * s` only at the far end of the ladder.
The near end is opposed less, because it is closer to the pawn's view: a cannibalism-tolerant pawn dislikes "required" less than "abhorrent".
`oppositionScale` is a setting (default 1, full opposition at the far end).

### Issue categories

Not every issue is a moral axis.
Each issue has a category, set in [preceptPolicy.md](preceptPolicy.md):

- **Moral**: graded by rung distance as above.
- **Special**: bespoke comparisons, for example weapon preferences and preferred xenotypes, which compare the precepts' contents rather than ranks.
- **PositiveOnly** and **UniversalPositive**: never a source of disagreement; universal ones (charity) give every faith that holds them a small bonus.
- **NA**: not a belief (buildings, ritual seats, naming), excluded.

Some precepts imply a stance on another issue (venerating trees implies disapproving of tree cutting).
These **couplings** let the implied stance take part in the comparison.

### Strength

Conviction strength lives on a scale where 20 means full certainty; values up to 50 are possible but exceptional.
At pawn generation a stance starts at `U(5, 25)`, shifted by personality (iron-willed and steadfast pawns hold beliefs more firmly; nervous, volatile, pessimistic and neurotic pawns less) and by traits that suit or clash with the ideoligion's memes (see [trait-meme-affinity.md](trait-meme-affinity.md)).
A few of the weakest-held issues may start on a different rung from the faith's, so not every pawn is perfectly orthodox.
Unreinforced conviction decays slowly toward zero.

## Opinion of an ideoligion

A pawn's opinion of a **foreign** ideoligion has three parts:

- **Structural**: `mean(per-issue opinion) * 5` over the issues either faith holds, plus meme and trait terms (loyalty memes, supremacism, elders, diet, xenotype preference).
  The mean (not a sum) keeps the result stable when a mod list adds dozens of issues.
  Only issues one of the two faiths actually holds count: mutual indifference is not agreement.
- **Personal**: accumulated experience with that faith.
- **Relational**: what the pawn thinks of that faith's followers.

The opinion of the pawn's **own** ideoligion is its certainty.
Both are on the same scale, so "do I prefer that faith over mine?" is a direct comparison.

## Certainty

Certainty relaxes toward a setpoint:

```
dc/dt  = k * (target - c)
target = structural + relational + practitional
```

- **Structural**: the pawn's structural opinion of its own faith, that is, mean conviction × 5.
  This is the main term: a pawn that holds its faith's stances firmly is certain.
- **Relational**: the mean opinion of co-religionists on the same map, through a curve, scaled by a setting (default ±25%).
  It uses the mean, not the sum, so that a small colony's congregation matters as much as a large one, and the pawn itself is excluded.
- **Practitional**: the summed mood of current precept thoughts, through a curve, scaled by a setting (default ±25%).
  Enjoying another faith's ritual counts against the pawn's own faith.

`k` is the drift-rate setting (default 10% of the gap per day).
Without a restoring term, the old model drained every pawn's certainty to zero over time; the setpoint gives certainty an equilibrium that events move.

Certainty is not capped at 100%: a pawn with very strong fit can sit above it, and so must lose more before it doubts.
Vanilla's certainty field always holds the value clamped to 0–100%, so vanilla systems and other mods see a normal number.
The mod sets vanilla's own certainty drift to zero and integrates certainty itself.

New pawns start at their setpoint.
Pawns from saves made without the mod keep their existing certainty: their stances are scaled once so the structural band matches it.

## Belief change

Every event that changes belief moves stances; certainty then follows through the setpoint.
No event writes certainty directly, except the short-lived "knock" after a lost conversion argument.

### The conviction valley

When a pawn is persuaded toward another rung, its stance does not jump.
It moves a fixed distance along a curve in (rank, strength) space whose low point sits between the two positions.
Conviction falls as the pawn moves through the uncertain middle and recovers as it settles on the new rung.
So a firmly held stance moves slowly, a weakly held one moves fast, and a full conversion of a belief takes several events.
The derivation and plots are in `analysis/conviction_valley.py`.

### Sources of change

- **Debates** between pawns pick an issue they disagree on, from the issues the initiator's ideo takes a position on.
  Wider disagreements come up more often: each issue is weighted by its rung gap (as a fraction of the ladder, scaled to 20) plus its conviction gap.
  Both pawns argue from their personal stance, not their ideo's position, so a decisive win pulls the loser toward the winner's own belief.
  Pawns that can see and hear a decisive debate are pulled toward the winner too, at a quarter of the normal pull.
  A winner from a Proselytizer faith doubles that onlooker pull, and onlookers with a high certainty loss factor move further.
  Pawns whose faith approves of diversity can choose debating as recreation; while they do, most of their social interactions become debates.
  A draw can make either pawn dig in (more likely for intelligent pawns and those with shakier faith) or start a social fight.
  The diversity precept decides whether each pawn enjoys or resents the argument.
  Caravans debate too, although vanilla runs no social interactions there.
  Each pawn has the same chance of a random interaction as at home, with the same interaction weights, but only debates are resolved.
  Prisoners take part like everyone else.
  A caravan rests whenever it stops, so a pawn counts as awake when its rest need is at least 28% (vanilla "Tired" begins below that).
  Every such pawn can debate, and every such pawn in the caravan is an onlooker.
  A fight is not simulated off-map: both pawns get the memory of a social fight instead.
- **Conversion attempts** (socializing and prisoner conversion) argue the single issue the target most opposes, at twice the normal pull.
  If the preacher wins, the target's certainty is knocked down temporarily and conversion is checked at once.
  If the preacher loses, the preacher's own stance moves toward the target's.
- **The moral guide's Convert ability** argues one to four of the target's most-opposed issues on a single roll at normal strength.
  It is wide and shallow where the conversion attempt is narrow and deep.
- **The moral guide's Reassure ability** is the same-faith mirror: it pulls the target's most heterodox stances back toward the faith and raises certainty.
  A guide can reassure themselves.
- **Rituals** with the belief-reinforcement outcome move every participant toward orthodoxy and full conviction on a good result, and away on a bad one.
  The conversion ritual pulls the convertee's stances strongly toward the ritual's faith and checks conversion.
- **Books** carry conviction like a pawn, taken from their author.
  Reading a book of one's own faith hardens conviction; reading a rival's pulls stances toward it.
- **Contemplation** at a shrine, pew or reliquary reinforces the pawn's own stances.
- **Precept moods** shift conviction on the issue they come from, a little every few hours.
  A mood is a vote on the faith's rung, not on the pawn's own: a good mood pulls toward the faith's rung, a bad mood pulls the stance away from the faith's rung toward a firm dissent (strength 15): an orthodox stance toward a random end of the ladder, a heterodox one further out on its own side.
  Loyalty memes turn the good mood from another faith's practices into cognitive dissonance.
- **Relics** strengthen every follower on all of the faith's moral issues when found; relic moods act like precept moods.

## Conversion

Conversion is a hazard rate, checked on the slow pawn tick.
For each ideoligion the pawn prefers to its own (opinion `O` greater than certainty `C`):

```
p          = (O - C) / O                       // chance per reference window W
p_interval = 1 - (1 - p) ^ (Δt / W)            // chance for this tick's Δt
```

- A pawn never converts to a faith it does not prefer.
- The same edge converts a doubtful pawn much more readily than a confident one, because `p` is a ratio.
- Because survival multiplies over time, the result does not depend on how often the check runs; `W` (the conversion-pace setting) is the only rate knob.
- Competing candidates combine as survival `Π(1 - p_interval)`; the winner is drawn weighted by how much the pawn prefers it.
- The overall chance is scaled by `1 - certainty` (no spontaneous conversion at 100% or above), by the pawn's certainty-loss factor, and down by how harshly the pawn's faith condemns apostasy.

Events do not roll for conversion themselves; they move stances and certainty, and the hazard integrates the result.
The exceptions are the deliberate attempts (conversion interaction, Convert ability, conversion ritual), which check at once after a won argument, because the player or warden chose to make them happen.

### Crisis of faith

When certainty falls below the crisis threshold (default 25%, higher for volatile pawns), a crisis competes in the same draw as the other faiths.
A pawn already near a mental break has an ordinary mood break instead.
Otherwise the pawn switches to the faith it prefers most (if it prefers one), certainty resets to 1.5× the threshold, and it wanders in doubt, seeking contemplation or a religious book.

## Children

Babies have no ideoligion.
When a child comes of age, its stances are the exposure-weighted average of the faiths it grew up around, and it joins the faith that best fits those stances.
Beliefs come first; the ideoligion follows.

## Design rules

- Certainty is a consequence of stances. To change certainty, change stances.
- Anything that rolls dice over time is expressed as a rate, so tick frequency, camera position and game speed do not change outcomes.
- Means, not sums, wherever the number of terms depends on the mod list or the colony size.
- Vanilla state is kept valid (certainty clamped to 0–100%) so other mods that read it keep working.
