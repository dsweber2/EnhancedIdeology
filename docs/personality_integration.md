# RimPsyche integration

This document plans compatibility with [RimPsyche](https://github.com/jagerguy36/Rimpsyche) (local clone: `../otherMods/Rimpsyche`).
It records how RimPsyche models personality, which parts of it map onto Enhanced Ideology's knobs, and how its religion and beliefs conversations work today.
Nothing here is implemented yet.
For the belief model the knobs refer to, see [design.md](design.md).

## RimPsyche's personality model

### Facets

Each pawn has 15 **facets**: the Big Five, each split into three (`Source/1.6/CompPsyche/Facet.cs`).

| Domain            | Facets (high / low pole)                                                                                  |
|-------------------+-----------------------------------------------------------------------------------------------------------|
| Openness          | Imagination (imaginative / realistic), Intellect (philosophical / unreflective), Curiosity (explorative / conventional) |
| Conscientiousness | Industriousness (persistent / unmotivated), Orderliness (organized / disorganized), Integrity (reliable / inconsistent) |
| Extraversion      | Sociability (friendly / aloof), Assertiveness (assertive / timid), Enthusiasm (cheerful / stoic)          |
| Agreeableness     | Compassion (compassionate / cold), Cooperation (accommodating / stubborn), Humbleness (humble / arrogant)  |
| Neuroticism       | Volatility (volatile / stable), Pessimism (pessimistic / optimistic), Insecurity (insecure / confident)   |

- Raw values are floats in [-50, 50].
- At generation, each domain rolls one base value from `U(-35, 35)`, and each of its three facets is `Gaussian(base, 10)`.
  The three facets of a domain are therefore strongly correlated.
- **Gates**: traits and genes remap a facet's full range linearly onto a sub-range (`RimpsycheDatabase.RegisterTraitGate`, `Rimpsyche_Utility.ApplyGate`).
  `GetFacetValueRaw` returns the ungated value; `GetFacetValue` returns the gated value.
- Facets drift during play.
  A successful conversation calls `AffectFacetValue`, which moves the facets behind the topic's nodes, with a 10% spill-over onto the other facets of the same domain.
  Anything we derive from facets or nodes must be read live or invalidated, not cached for the pawn's lifetime.

### Personality nodes

The player sees 34 **personality nodes** (`1.6/Defs/PersonalityDefs/Personalities.xml`).
Each node is a weighted sum of facets, where the absolute weights sum to about 1:

```
node = clamp(0.02 * Σ facet_i * weight_i, -1, 1)
```

After that, traits **scope** a node: they remap its value onto a sub-range, in the same way gates remap facets.
`GetPersonality` returns the scoped, cached value.

### Access

```csharp
pawn.compPsyche().Personality.GetPersonality("Rimpsyche_Openness")  // -1..1, gated + scoped, cached
pawn.compPsyche().Personality.GetFacetValue(Facet.Curiosity)        // -50..50, gated
pawn.compPsyche().Personality.GetFacetValueRaw(Facet.Curiosity)     // -50..50, no trait influence
pawn.compPsyche().Evaluate(RimpsycheDatabase.ReceiveBase)           // their named formulas, cached
```

`compPsyche()` is an extension method that Prepatcher injects.
`compPsyche()?.Enabled` is false for pawns without a psyche, for example inhumanized pawns.
We need either a soft reference to their assembly (a separate compat assembly, loaded only when RimPsyche is active) or reflection.

## Traits already accounted for

RimPsyche already folds many vanilla traits into facets (gates) and nodes (scopes).
If we read a node and also apply our own term for the same trait, that trait counts twice.
Rule: **when RimPsyche is active, drop our trait term wherever a node we read already scopes or gates that trait.**

A trait reaches nodes in two ways:

- A **scope** remaps one node onto a sub-range.
  In the table, ↑ means the node is remapped to [0, 1] and ⇑ to [0.5, 1]; ↓ and ⇓ are the mirror images.
- A **gate** remaps one facet onto half its range (or less).
  Every node that reads that facet moves, so a gate reaches many more nodes than a scope.
  The gate column lists the nodes where the facet has a weight of at least 0.1, largest first.

Scopes are in `Personalities.xml`; gates for vanilla traits are in `RimpsycheDatabase.RegisterBaseGates` (`RimpsycheDatabase.cs:401`).

| Trait                     | Scopes (node)                                                                                     | Gate (facet → range): nodes reached                                                                                                  | Where we use it                              | Under RimPsyche                                                                                     |
|---------------------------+---------------------------------------------------------------------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------+----------------------------------------------+-----------------------------------------------------------------------------------------------------|
| Nerves                    | Tension: iron-willed ⇓, steadfast ↓, nervous ↑, volatile ⇑. Tenacity: the mirror image.            | Volatility → [−50, 0] for steadfast and iron-willed, [0, 50] for nervous and volatile: Stability, Emotionality, Tenacity, Discipline, Tension, Spontaneity, Tact, Aggressiveness, Organization, Deliberation | `ConvictionOffsetFromTraits`                 | Drop our term; K1 reads Tension.                                                                    |
| NaturalMood               | Optimism: sanguine ⇑, optimist ↑, pessimist ↓, depressive ⇓.                                      | Pessimism → [−50, 0] for optimist and sanguine, [0, 50] for pessimist and depressive: Optimism, Tension, Playfulness, Diligence, Trust, Bravery, Confidence, Aggressiveness, Passion, Stability, Compassion, Deliberation | `ConvictionOffsetFromTraits` (downside only) | Drop our term; K1 reads Optimism.                                                                   |
| Neurotic                  | Organization: neurotic ↑, very neurotic ⇑. Tension ↑ for both.                                    | Orderliness → [0, 50]: Organization, Spontaneity, Deliberation, Propriety, Experimentation, Openness, Tension, Imagination, Diligence, Focus, Discipline, Expectation | `ConvictionOffsetFromTraits`                 | Drop our term; K1 reads Tension.                                                                    |
| Transhumanist, BodyPurist | Openness: transhumanist ↑, body purist ↓.                                                         | —                                                                                                                                    | meme affinity (Transhumanist, FleshPurity)   | Keep meme affinity: it is meme-specific. Do not give Openness an issue bias on transhumanist issues. |
| Nudist                    | Propriety ↓.                                                                                      | —                                                                                                                                    | meme affinity (Nudism, FleshPurity)          | Conflict: the Propriety issue bias on nudity and the Nudism affinity both act. Pick one.             |
| DrugDesire                | Experimentation: chemical interest ↑, chemical fascination ⇑, teetotaler ↓. Discipline: the mirror image. | —                                                                                                                              | meme affinity (HighLife)                     | Conflict: the Experimentation issue bias on drug use and the HighLife affinity both act. Pick one.   |
| Ascetic                   | Expectation ↓.                                                                                    | —                                                                                                                                    | meme affinity (PainIsVirtue)                 | Conflict if Expectation gets a comfort issue bias. Pick one.                                        |
| Wimp                      | Bravery ↓.                                                                                        | —                                                                                                                                    | meme affinity (PainIsVirtue, disagreeable)   | No conflict while Bravery stays unused.                                                             |
| TorturedArtist            | Imagination ↑.                                                                                    | Imagination → [0, 50]: Imagination, Emotionality, Reflectiveness, Experimentation, Passion, Stability, Compassion, Playfulness       | meme affinity (PainIsVirtue)                 | Conflict through the gate: Stability (K7) moves.                                                    |
| Psychopath                | Emotionality → [−1, −0.8]. Tension, Morality, Compassion ↓.                                       | Compassion, Integrity, Volatility → [−50, −40]; Humbleness, Pessimism, Insecurity → [−50, 0]: nearly every node                      | not used                                     | Free: it comes in through the nodes.                                                                |
| TooSmart                  | Tension ↑. Reflectiveness ⇑.                                                                      | Intellect → [0, 50]: Reflectiveness, Inquisitiveness, Imagination, Openness, Experimentation, Trust                                  | not used                                     | Free.                                                                                               |
| Kind, Bloodlust           | Aggressiveness: kind ↓, bloodlust ↑.                                                              | Compassion → [0, 50] for kind, [−50, 0] for bloodlust: Compassion, SelfInterest, Tact, Aggressiveness, Trust, Morality, Loyalty     | not used                                     | Free.                                                                                               |
| Jealous                   | Competitiveness ↑.                                                                                | Humbleness → [−50, 0]: Authenticity, Expectation, Competitiveness, SelfInterest, Tact, Propriety, Loyalty, Morality, Ambition        | not used                                     | Free.                                                                                               |
| Industriousness           | Diligence: industrious ⇑, hard worker ↑, lazy ↓, slothful ⇓.                                      | Industriousness → [0, 50] for hard worker and industrious, [−50, 0] for lazy and slothful: Diligence, Ambition, Passion, Tenacity, Focus, Organization, Deliberation | not used                                     | Free.                                                                                               |
| Recluse (Biotech)         | Sociability → [−1, −0.5].                                                                         | Sociability → [−50, 0]: Sociability, Talkativeness, Playfulness, Inquisitiveness, Compassion, Trust                                  | not used                                     | Free.                                                                                               |
| Greedy                    | Expectation ↑. SelfInterest ↑.                                                                    | —                                                                                                                                    | not used                                     | Free.                                                                                               |
| Abrasive                  | Tact ↓.                                                                                           | —                                                                                                                                    | not used                                     | Free.                                                                                               |
| Joyous                    | Playfulness ⇑.                                                                                    | —                                                                                                                                    | not used                                     | Free.                                                                                               |
| ShootingAccuracy          | Deliberation: careful shooter ↑, trigger-happy ↓.                                                 | —                                                                                                                                    | not used                                     | Free.                                                                                               |

Modded traits (VTE, HVT, RCT, ST, BadPeople, BS prefixes) are gated in `RimpsycheDatabase.cs` around lines 540–770.
We do not read any of them today.

The alternative is to rebuild nodes from `GetFacetValueRaw`, so that no trait influence enters and all our trait terms stay unchanged.
That is clean, but the numbers we use would no longer match what the player sees in RimPsyche's UI (a psychopath's displayed Morality would not be the one we act on), so this plan does not use it.

## Knobs

To keep the work linear rather than nodes × issues, nodes feed a small set of **global knobs**.
Only a short, hand-picked list of nodes gets an **issue bias**: a nudge on the preferred rung for specific issues at generation.

Nodes that share a domain are correlated.
Summing several of them in one knob counts that domain several times, so each knob reads one or two nodes that load on different facets.

| Knob | What it scales                                                             | Nodes (sign)                                                                                                                                                                    | Replaces / notes                                                                                   |
|------+----------------------------------------------------------------------------+---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+----------------------------------------------------------------------------------------------------|
| K1   | Initial conviction strength offset                                         | Tension (−), Optimism (+), Openness (-), Experimentation (-), Discipline (+),  Morality (+), Loyalty (+), SelfInterest (-)                                                      | Replaces `ConvictionOffsetFromTraits`. Optimism stays one-sided like the current NaturalMood term. |
| K2   | Susceptibility: pull multiplier when the pawn loses a debate or conversion | Openness (+), Trust (+), Reflectiveness (-), Confidence (-), Inquisitiveness (+), Passion (-), Discipline (-), Deliberation (-), Tenacity (-), Loyalty (-), Competitiveness (-) | RimPsyche's `ReceiveBase = openness * (trust + 1) / 2` already combines these.                     |
| K3   | Debate roll mean (persuasiveness)                                          | Passion (+), Aggressiveness (−), Sociability (+), Talkativeness (+), Tact (+)                                                                                                   | RimPsyche's `Fervor = 0.1 * (passion - aggressiveness)`. A small term next to ConversionPower.     |
| K4   | Debate initiation weight                                                   | Talkativeness (+), Competitiveness (+), authentic (+), tact (-), Confidence (+), Propriety (-, flipped for proselytizers), Passion (+), Spontaneity (+), Tenacity (+)           |                                                                                                    |
| K5   | Dig-in chance on a draw                                                    | Tenacity (+), Competitiveness (+), Confidence (+), Passion (+), Openness (-), Confidence (+), Aggressiveness (+), Trust (-), Loyalty (+)                                        | Today: Intellectual skill and certainty (should be in addition).                                   |
| K6   | Social fight chance on a draw                                              | Aggressiveness (+), Tact (−),  Emotionality (+), Compassion (-), Stability (-), playfulness (-), Tension (+)                                                                    | RimPsyche's `SocialFightChanceMultiplier` also reads Emotionality and Compassion.                  |
| K7   | Crisis threshold                                                           | Stability (−), Discipline (+), Confidence (-),  Passion (-), Tension (-), Emotionality (-), Spontaneity (+), Deliberation (-)                                                   | Today: `CertaintyLossFactor` only.                                                                 |
| K8   | Onlooker pull taken from a decisive debate                                 | Confidence (−), Trust (+), Openness (+), Loyalty (-), Sociability (+), Deliberation (-)                                                                                         |                                                                                                    |
| K9   | Precept-mood conviction shift magnitude                                    | Emotionality (+), Confidence (-), Stability (-), Discipline (-), Optimism (-), Deliberation (+)                                                                                 |                                                                                                    |
| K10  | Conviction decay rate                                                      | Discipline (−), Confidence (-), Focus (-), Emotionality (+), Morality (-), Trust (-), Loyalty (-)                                                                               |                                                                                                    |
| K11  | Conversion hazard (apostasy resistance)                                    | Discipline (−), Confidence (-), Focus (-), Emotionality (+), Morality (-), Trust (-), Loyalty (-)                                                                               |                                                                                                    |
| K12  | Weight of the relational term in the certainty setpoint                    | Sociability (+), Discipline (-), Focus (-), Talkativeness (+), Competitiveness (+), Reflectiveness (-), Authenticity (-)                                                        |                                                                                                    |
| K13  | Chance to start heterodox on weakly held issues; pull from rival books     | Inquisitiveness (+), Deliberation (+), Authenticity (+), Reflectiveness (+)                                                                                                     |                                                                                                    |
| K14  | Contemplation reinforcement                                                | Reflectiveness (+), Deliberation (+), Authenticity (+), Spontaneity (-)                                                                                                         |                                                                                                    |
| IB   | Issue bias at generation                                                   | see the node table                                                                                                                                                              | Rung direction per issue comes from [preceptPolicy.md](preceptPolicy.md).                          |

## All nodes

Facet weights are listed largest first; small weights (|w| < 0.1) are left out.

Precept Affinity is in addition to precepts implied by meme affinity

| Node            | High / low                  | Proposed use    | Trait scopes                           | Meme affinity | Precept affinity | Main facets                                                                                                               |
|-----------------+-----------------------------+-----------------+----------------------------------------+---------------+------------------+---------------------------------------------------------------------------------------------------------------------------|
| Talkativeness   | outspoken / taciturn        | fill from above | —                                      |               |                  | Assertiveness +.40, Sociability +.25, Enthusiasm +.15, Insecurity −.15                                                    |
| Sociability     | friendly / reserved         | fill from above | Recluse                                |               |                  | Sociability +.50, Cooperation +.20, Enthusiasm +.15, Insecurity −.15                                                      |
| Tact            | diplomatic / brash          | fill from above | Abrasive                               |               |                  | Cooperation +.35, Compassion +.20, Humbleness +.20, Volatility −.15                                                       |
| Openness        | open-minded / traditional   | fill from above | BodyPurist, Transhumanist              | transhumanist              |                  | Curiosity +.35, Orderliness −.20, Intellect +.15, Integrity −.15, Cooperation +.10                                        |
| Confidence      | confident / insecure        | fill from above | —                                      |               |                  | Insecurity −.50, Assertiveness +.40, Pessimism −.10                                                                       |
| Inquisitiveness | inquisitive / indifferent   | fill from above | —                                      |               |                  | Curiosity +.50, Intellect +.20, Enthusiasm +.15, Sociability +.10                                                         |
| Propriety       | modest / uninhibited        | fill from above | Nudist                                 |               |                  | Orderliness +.25, Curiosity −.20, Humbleness +.20, Integrity +.15, Insecurity +.15                                        |
| Aggressiveness  | aggressive / gentle         | fill from above | Bloodlust, Kind                        |               |                  | Assertiveness +.25, Cooperation −.25, Compassion −.20, Volatility +.10, Pessimism +.10                                    |
| Experimentation | experimental / conventional | fill from above | DrugDesire                             |               |                  | Curiosity +.35, Orderliness −.25, Intellect +.15, Imagination +.10, Insecurity −.10                                       |
| Imagination     | imaginative / grounded      | fill from above | TorturedArtist                         |               |                  | Imagination +.50, Intellect +.20, Curiosity +.10, Orderliness −.15                                                        |
| Diligence       | diligent / lazy             | fill from above | Industriousness                        |               |                  | Industriousness +.50, Orderliness +.15, Integrity +.15, Pessimism −.15                                                    |
| Organization    | organized / haphazard       | fill from above | Neurotic                               |               |                  | Orderliness +.50, Industriousness +.20, Curiosity −.15, Volatility −.10                                                   |
| Discipline      | disciplined / undisciplined | fill from above | DrugDesire                             |               |                  | Integrity +.50, Volatility −.25, Orderliness +.10                                                                         |
| Focus           | focused / eclectic          | fill from above | —                                      |               |                  | Curiosity −.50, Industriousness +.25, Orderliness +.15, Enthusiasm −.10                                                   |
| Passion         | passionate / apathetic      | fill from above | —                                      |               |                  | Enthusiasm +.50, Industriousness +.30, Imagination +.10, Pessimism −.10                                                   |
| Tension         | high-strung / laid-back     | fill from above | Psychopath, TooSmart, Nerves, Neurotic |               |                  | Pessimism +.30, Insecurity +.25, Volatility +.20, Orderliness +.15                                                        |
| Emotionality    | emotional / phlegmatic      | fill from above | Psychopath                             |               |                  | Imagination +.45, Volatility +.45                                                                                         |
| Stability       | stable / unstable           | fill from above | —                                      |               |                  | Volatility −.50, Imagination −.10, Integrity +.10, Enthusiasm −.10, Pessimism −.10, Insecurity −.10                       |
| Morality        | principled / amoral         | fill from above | Psychopath                             |               |                  | Integrity +.50, Compassion +.10, Cooperation +.10, Humbleness +.10, Insecurity +.10                                       |
| Optimism        | optimistic / pessimistic    | fill from above | NaturalMood                            |               |                  | Pessimism −.50, Enthusiasm +.40, Insecurity −.10                                                                          |
| Ambition        | ambitious / content         | fill from above | —                                      |               |                  | Industriousness +.40, Assertiveness +.25, Curiosity +.10, Humbleness −.10, Insecurity −.10                                |
| Compassion      | compassionate / coldhearted | fill from above | Psychopath                             |               |                  | Compassion +.50, Cooperation +.15, Imagination +.10, Sociability +.10, Pessimism −.10                                     |
| Spontaneity     | impulsive / consistent      | fill from above | —                                      |               |                  | Orderliness −.40, Enthusiasm +.20, Volatility +.20, Curiosity +.10, Insecurity −.10                                       |
| Trust           | trusting / skeptical        | fill from above | —                                      |               |                  | Cooperation +.30, Compassion +.15, Pessimism −.15, Insecurity −.15, Intellect −.10, Sociability +.10                      |
| Deliberation    | deliberate / hasty          | fill from above | ShootingAccuracy                       |               |                  | Orderliness +.35, Integrity +.15, Industriousness +.10, Enthusiasm −.10, Volatility −.10, Pessimism +.10, Insecurity +.10 |
| Playfulness     | playful / serious           | fill from above | Joyous                                 |               |                  | Enthusiasm +.40, Sociability +.25, Pessimism −.20, Imagination +.10                                                       |
| Bravery         | courageous / fearful        | fill from above | Wimp                                   |               |                  | Assertiveness +.40, Insecurity −.25, Pessimism −.15, Integrity +.10                                                       |
| Tenacity        | tenacious / fragile         | fill from above | Nerves                                 |               |                  | Volatility −.40, Industriousness +.30, Integrity +.10, Enthusiasm −.10, Insecurity −.10                                   |
| Loyalty         | loyal / disloyal            | fill from above | —                                      |               |                  | Integrity +.35, Cooperation +.35, Humbleness +.15, Compassion +.10                                                        |
| Expectation     | demanding / spartan         | fill from above | Ascetic, Greedy                        |               |                  | Humbleness −.40, Assertiveness +.25, Cooperation −.15, Orderliness +.10                                                   |
| Competitiveness | competitive / cooperative   | fill from above | Jealous                                |               |                  | Cooperation −.35, Assertiveness +.30, Humbleness −.30                                                                     |
| SelfInterest    | egocentric / altruistic     | fill from above | Greedy                                 |               |                  | Compassion −.35, Humbleness −.30, Integrity −.15                                                                          |
| Reflectiveness  | contemplative / reactive    | fill from above | TooSmart                               |               |                  | Intellect +.50, Imagination +.25, Enthusiasm −.15                                                                         |
| Authenticity    | authentic / superficial     | fill from above | —                                      |               |                  | Humbleness +.45, Integrity +.30, Insecurity −.25                                                                          |

Overlaps to watch:

- Tenacity carries the Nerves scope, and Tension carries Nerves too.
  K1 uses Tension and K5 uses Tenacity, so Nerves affects both knobs, but no knob counts it twice.
- Compassion and SelfInterest both load on the Compassion facet.
  If both bias charity, the Compassion facet counts twice on that issue.

## RimPsyche's religion and beliefs conversations

### How a conversation runs

RimPsyche reassigns `InteractionDefOf.DeepTalk` to `Rimpsyche_StartConversation` and `InteractionDefOf.Chitchat` to `Rimpsyche_Smalltalk` (`RimpsycheDatabase.cs:66`).
Every vanilla code path that asks for a deep talk now gets a RimPsyche conversation.

`InteractionWorker_StartConversation.Interacted` (`Source/1.6/Conversation/`):

1. The initiator picks an **interest**, weighted by their interest score (30–70).
   Religion and beliefs are two of the 12 topics in the `InterestIdentity` interest (`InterestGeneral.xml:1701`).
   The worker then picks a topic uniformly from that interest.
2. If the recipient's opinion of the initiator is below −25, the recipient may brush the conversation off.
3. `Topic.GetScore` computes each pawn's **attitude** to the topic: the weighted sum of node values, clamped and then boosted (`Boost2`).
   It caches the result per pawn in `TopicOpinionCache`, which is cleared whenever personality changes.
   The **alignment** is `SaddleShapeFunction(initAttitude, reciAttitude, controversiality)`, in [-1, 1].
4. Positive alignment means a pleasant talk, scaled by opinion, Tact and Fervor.
   Negative alignment means each side's receptiveness (`AssertBase` of the other pawn + own `ReceiveBase` + opinion) decides between a "good argument" (score boosted ×4) and a bad talk, which can start a social fight.
5. Each pawn with a non-zero result gets a conversation memory (opinion offset).
   A pawn with a **positive** result also gets `CompPsyche.AffectPawn`: with a probability that rises with result and Trust and falls with age, its facets move so that its attitude on this topic moves toward the other pawn.
6. Two extension points then run: the static no-op `InteractionHook(initiator, recipient, topic, alignment, initOffset, reciOffset)`, meant as a Harmony postfix target, and `topic.result?.ApplyEffect(...)`, a per-topic `ConvoResultBase` that can be set in XML.

### The two topics

```
religion   (+ religion matters to identity | − religion should be secular)
           Morality +.35, Loyalty +.25, Reflectiveness +.20, Openness −.10, Experimentation −.10
beliefs    (+ conviction-driven / ideological | − evidence-driven / pragmatic)
           Openness −.30, Trust +.25, Morality +.20, Passion +.15, Reflectiveness +.10
```

Both have `controversiality` 0.
With C = 0 the saddle function reduces to `x * y` before boosting, so alignment depends only on whether both attitudes have the same sign and how strong they are.

### Problems for us

- **Ideology is ignored.** Two principled, loyal pawns of opposing faiths "agree" on religion.
  A devout pawn and a casual co-religionist can disagree, even though they share every precept.
- **The facet drift feeds back into our knobs.** A religion talk moves Integrity, Cooperation and Intellect (through Morality, Loyalty and Reflectiveness), and the beliefs talk moves Curiosity (through Openness).
  These facets drive K2, K5, K10 and K11.
  A lot of religion chat would quietly make pawns more or less persuadable, with no visible ideological event.
- **Two parallel channels.** Our debates and their religion talks both run as social interactions about faith, and they do not know about each other.

### Hook points

| Hook                                              | Use                                                                                                                         |
|---------------------------------------------------+-----------------------------------------------------------------------------------------------------------------------------|
| Prefix on `Topic.GetScore` for `religion` / `beliefs` | Replace the personality attitude with an ideological one, for example certainty or stance distance between the two pawns. Alignment then reflects real agreement. Remember `TopicOpinionCache`: an ideology-derived value must bypass it or be invalidated when stances change. |
| XML patch on the topic's `weights` or `controversiality` | Cheap retuning without code.                                                                                                |
| XML patch to add a `result` (`ConvoResultBase`) subclass | Turn a religion talk into a small belief event: move stances or certainty from the alignment and opinion offsets. No Harmony needed. |
| Postfix on `InteractionHook`                     | Same data as `result`, but for all topics. Useful to react to non-religion topics too, for example a "beliefs" talk that touches an issue. |
| Prefix on `CompPsyche.AffectPawn`                | Suppress or redirect facet drift for religion topics, if we decide that ideology talk should move stances instead of personality. |

## Open questions

- Should a RimPsyche religion talk become a lightweight debate (it picks an issue and moves stances), or only feed relational certainty?
- Should their religion talks replace our debate's random selection weight, or should both stay, with ours limited to explicit arguments?
- Optimism: keep the current one-sided NaturalMood behaviour in K1, or let optimists gain conviction?
- For each conflict in the trait table, does the node or the meme affinity win?
