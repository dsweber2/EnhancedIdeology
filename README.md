[![RimWorld 1.6](https://img.shields.io/badge/RimWorld-1.6-brightgreen.svg)](http://rimworldgame.com/) [![Build](https://github.com/ilyvion/EnhancedBeliefs-Updated/actions/workflows/ci.yml/badge.svg)](https://github.com/ilyvion/EnhancedBeliefs-Updated/actions/workflows/ci.yml)

**Enhanced Beliefs** is a RimWorld mod that replaces the ideology certainty system with a deep, opinion-driven belief model. Pawns develop nuanced views of every ideology they encounter, and those views — not mood — are what determine whether they hold firm or eventually convert.

## How it works

### Opinion model

Each pawn tracks an opinion (0–100) of every ideology in the world, composed of three parts:

- **Structural opinion** — how well the ideology's stances on issues (execution, slavery, drug use, etc.) match the pawn's own preferred stances, weighted by how strongly they hold each view
- **Personal opinion** — accumulated through debates, book reading, conversion attempts, and social interactions
- **Relationship opinion** — a weighted average of how the pawn feels about their co-ideologues

Certainty is now *derived* from ideology opinion. A pawn who genuinely agrees with their faith's stances stays certain; one who has been argued into doubting them slowly drifts away.

### Precept stances

Rather than flat opinion offsets per precept, each pawn holds a **preferred stance per issue** (e.g., on execution: "Respected if guilty") with a conviction strength. Opinion of any ideology is computed from how far its stances are from the pawn's preferred stances — close agreement earns positive opinion, strong disagreement earns negative.

Traits influence starting conviction strength: iron-willed and steadfast pawns begin with firmer beliefs; volatile, nervous, or neurotic pawns start shakier.

### Debates and conversion

There are three distinct conversion surfaces:

- **Social conversion** (`ConvertIdeoAttempt`) — a directed debate over the single most-opposed issue between the two pawns; a preacher win shifts the listener's stance and may trigger conversion
- **Ideological debate** (`IdeologicalDebatePrecept`) — a spontaneous match over a contested issue; a win shifts the loser's stance; a tie can entrench both pawns further; pawns with diversity-of-thought precepts enjoy a mood bonus from debating
- **Moral guide Convert ability** — targets 1–4 of the recipient's most-opposed issues simultaneously; a single debate roll determines outcome; the cursor tooltip shows estimated success chance and target issues

In all cases, events knock certainty and shift stances — the background tick then integrates whether conversion fires, rather than converting immediately on a roll.

### Ideology books

Books are bound to a specific ideology and carry per-issue conviction strengths (seeded from the author's own fervor, or random for trader finds).

- Reading your own faith's book **hardens conviction** on its issues
- Reading a rival's book **tugs your stances** toward that ideology's positions
- The more fervently written the book, the stronger the effect

### Iconoclast mental break

Pawns who snap into the Iconoclast state will actively seek out ideology books across the map, place them on the ground, and burn them — a dramatic multi-step process rather than instant destruction.

## New UI

- **Opinion tab** — a new inspector tab on every pawn with an ideology; shows all world ideologies sorted by opinion, with a breakdown tooltip (structural / personal / relationship) and a "Convictions" column listing the pawn's per-issue stances and strengths
- **Enriched certainty bar** — the social card's certainty display now shows the daily change rate and, if active, the inactivity penalty for neglecting precept rituals and activities

## Compatibility

Tested on RimWorld 1.6. Ideology DLC required. Royalty supported.

## License

Licensed under Creative Commons Attribution 4.0, ([LICENSE](LICENSE))

`SPDX-License-Identifier: CC-BY-4.0`

### Contribution

Unless you explicitly state otherwise, any contribution intentionally submitted
for inclusion in the work by you, shall be licensed as above, without any additional
terms or conditions.
