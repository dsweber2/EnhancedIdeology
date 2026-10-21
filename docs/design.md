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
An issue the pawn's traits vote on instead decays toward the traits' target (see "Trait stance targets").
So a neglected pawn drifts back toward what their personality implies, not toward believing nothing.

### Trait stance targets

Each link in [trait-meme-affinity.md](trait-meme-affinity.md) "Additional meme affinity", and each trait already listed in a meme def, is a vote on an issue (`Source/Tracker/TraitStanceTargets.cs`), whether or not the pawn's faith holds the meme:

- a rung link votes +1 for its rung, an indifferent link votes +1 for the issue's indifferent rank (`PreceptLadder.IndifferentRank`, which can sit between rungs), and an opposes link votes −1 against the issue's most-valued rung (`Charity_Essential`);
- a meme votes +1 per `agreeableTraits` listing and −½ per `disagreeableTraits` listing, on every rung it links to through `requiredMemes`, `requireOne` or `selectOneOrNone`; a link through `associatedMemes` counts half.
  Disliking a meme counts half because a meme bundles precepts: a trait that rejects pacifism does not, by that alone, reject abolition.

`IdeoRole` gets no votes: an ideo holds many roles at once, so a rank on the role list means nothing.

Each rung nets its support less its opposition.
If any rung nets above zero, the issue has a **support target**: the best-supported rung, the one closest to the pawn's stance on a tie.
Its votes are that rung's net.
Opposition to other rungs does not add to it: "not `Execution_Abhorrent`" is not "`Execution_Required`", and on a list like the roles it says nothing about which rung is left.
A support target pulls straight to its rung, even past an opposed rung: explicit support beats avoidance.

A count of votes gives strength `8 · votes^(2/3)` (`ConvictionScale.TraitVoteStrength`): ½ → 5, 1 → 8, 2 → 12.7, 5 → 23.4.
The curve grows slower than the votes because several mods often add memes for one idea (`AM_NonViolence`, `VME_Pacifist`), and each one is a vote.

Otherwise the rungs that net below zero are a no-go region, an **opposition target**.
Opposition says where the pawn is not, not where they are, so it never pulls a stance toward an opposed rung:

- a stance on an allowed spot (a rung or an off-ladder Don't-care) keeps its rank, and its strength moves toward 10;
- a stance on an opposed spot exits to the nearest allowed spot on the side away from the opposed rungs' weighted centre, at the strength of the opposing votes on its spot, up to 20.
  On the centre it exits toward Don't-care, and it takes the other side when its own side has no allowed spot.

Mirroring the opposed range was rejected: it aims at the opposed rung when the range is centred, and it sends a pawn who rejects one extreme to the other.
If every spot is opposed, the issue has no target.
The target is used in three places:

- **Spawn**, after the rest of seeding: if the faith is silent on the issue, the pawn takes a support target at the vote strength, or the exit from an opposed Don't-care.
  Toward a rung the faith holds, the pawn gains the vote strength as conviction; toward another rung, the pawn starts heterodox on the target at the vote strength.
  A stance on an opposed rung the faith holds loses the strength of the opposing votes; a stance on an allowed spot does not change.
- **Brainwipe**: the pawn returns to the target, or exits an opposed spot.
- **Daily decay**: the stance moves along the conviction valley toward the pull, instead of draining toward zero. A support target pulls at the vote strength, up to 20.

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

The setpoint is recomputed at each long tick (2000 ticks), just before certainty advances by it.
The social and opinion tabs also recompute it for the pawn they show, when it is more than a rare tick (250 ticks) old or an input has changed.
Two bands reuse cached inputs to keep that cheap.
The structural band is kept until a stance write, trait change, faith change or reform invalidates it.
Inputs that change without a notification (age, genes, settings) are picked up within four long ticks (8000 ticks, about three game hours).
The relational band reads opinions of co-religionists refreshed at that same interval, because vanilla's opinion calculation is costly.
Co-religionists who died or left the pawn's map or caravan drop out at that refresh.
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
  Reading walks each issue along the conviction valley toward the book's rung, by an arc per tick of the reading gain times the book's conviction on that issue.
  A book of one's own faith pulls a drifted stance back to orthodoxy, then hardens it toward full conviction, as contemplation does.
  A rival's book pulls toward its own position and conviction, like a debate, so a firmly held stance gives up conviction before it moves in rank.
  The reading report and the book's tooltip show the arc on the issue the book argues most strongly.
  Prisoners have no recreation need in vanilla, so they read the closest religious book they can reach in their cell, of any faith.
  The less certain a prisoner is, the more they read: the share of the day is 1 − certainty, clamped to 25–90%.
  Prisoners that have a recreation need from another mod read through that instead.
  Slaves do not get this, since their books would be whatever is near their work.
- **Contemplation** at a shrine, pew or reliquary reinforces the pawn's own stances.
- **Precept moods** shift conviction on the issue they come from, a little every few hours.
  A mood is a vote on the faith's rung, not on the pawn's own: a good mood pulls toward the faith's rung, a bad mood pulls the stance away from the faith's rung toward a firm dissent (strength 15): an orthodox stance toward a random end of the ladder, a heterodox one further out on its own side.
  Loyalty memes turn the good mood from another faith's practices into cognitive dissonance.
  These pulls are small and land on every pawn, so the pawn's opinion of other faiths catches up with them at most a quarter day late.
  The certainty setpoint follows them at once, and every other stance change updates both at once.
- **Relics** strengthen every follower on all of the faith's moral issues when found; relic moods act like precept moods.
- **Reforming** a fluid ideoligion moves the faith's rungs, not the believers.
  A believer who was orthodox on a changed issue jumps straight to the new rung at half their conviction, rather than walking the conviction valley.
  Orthodoxy is judged per issue, so a pawn who dissents on one issue still follows the reform on the others.
  Heterodox stances stay where they are: a reform toward a dissenter brings them into line for free, and one away from them pushes them further out.
- **Schisms** (Vanilla Ideology Expanded - Splits and Schisms) follow the same rule for the colonists who split off.
  The old ideoligion's rungs count as the "before" and the new ideoligion's rungs as the "after".

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
