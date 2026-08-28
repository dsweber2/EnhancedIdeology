[![RimWorld 1.6](https://img.shields.io/badge/RimWorld-1.6-brightgreen.svg)](http://rimworldgame.com/) [![Build](https://github.com/ilyvion/EnhancedBeliefs-Updated/actions/workflows/ci.yml/badge.svg)](https://github.com/ilyvion/EnhancedBeliefs-Updated/actions/workflows/ci.yml)

**Enhanced Beliefs** is a RimWorld mod that replaces the ideology certainty system with a deep, opinion-driven belief model. Pawns develop nuanced views of every ideology they encounter, and those views — not mood — are what determine whether they hold firm or eventually convert.

## How it works

### Certainty

Certainty drifts slowly toward a target set by three bands:

- **Structural** — how well the ideology's stances on issues fit the pawn's own beliefs. The main driver. Each pawn holds a preferred stance per issue with a conviction strength; ideologies that agree score positive, those that oppose score negative.
- **Relational** — how the pawn feels about their co-ideologues. Getting along well reinforces belief; friction erodes it.
- **Practitional** — whether the pawn is actually practicing. Precept moodlets (rituals, veneration, taboos, encouraged behaviours) push this up; neglecting them drags it down.

### Ideoligion opinions

A pawn's structural opinion of an ideology comes from three sources:

- **Issue stances** — each pawn holds a preferred position on every contested issue (e.g. on execution: "Respected if guilty") with a conviction strength. Traits influence starting conviction — iron-willed and steadfast pawns begin firmer; volatile or neurotic pawns start shakier. Stances evolve over time through contact with other pawns.
- **Personality fit** — some memes list traits they agree or disagree with. A Nudist pawn fits better in an ideology that venerates Flesh Purity; the same pawn fits worse in a Transhumanist one.
- **Inter-faith relations** — memes explicitly about other beliefs (supremacist, loyalist, guilty, etc.) apply flat opinion modifiers to all outside ideologies.

### Contemplation

Contemplation is a new recreation type that deepens a pawn's certainty in their precepts and associated issues. During free time (or when their ideology requires it) they'll seek somewhere to sit and reflect. More sanctified places — worship rooms, reliquaries, ideological statues — reinforce beliefs more strongly.

### Debates, conversion, and crises of faith

There are three ways beliefs change through interaction:

- **Social conversion** — any pawn with sufficient social skill can try to convert another. They focus on the single issue where they disagree most sharply. A win shifts the listener's stance; a high enough opinion gap may trigger an immediate conversion. Prisoner conversion uses exactly the same mechanics, just applied more deliberately.
- **Ideological debate** — spontaneous arguments over contested issues. A win shifts the loser's stance; a draw can entrench both sides further. Pawns with the diversity-of-thought precept enjoy a mood bonus from debating; apostates hate losing.
- **Moral guide Convert ability** — like social conversion but more intense. Targets 1–4 of the recipient's most-contested issues simultaneously. The cursor tooltip shows estimated success chance and target issues.

Beyond direct interaction: if a pawn has a higher opinion of another ideology than certainty in their own, they may spontaneously convert. If they disbelieve every ideology, they enter a crisis of faith — a mental break, an ideology switch, or a long period of contemplation to rebuild conviction. Either way, certainty lands above the crisis threshold, but if their structural opinion hasn't improved, a new crisis is guaranteed.

### Ideology books

Books are bound to a specific ideology and carry per-issue conviction strengths, seeded from the author's own stances and fervor (or random for trader finds).

- Reading your own faith's book **hardens conviction** on its issues
- Reading a rival's book **tugs your stances** toward that ideology's positions
- The more fervently written the book, the stronger the effect

### Iconoclast mental break

Pawns who snap into the Iconoclast state actively seek out ideology books across the map, place them on the ground, and burn them — a dramatic multi-step process rather than instant destruction.

## New UI

- **Opinion tab** — a new inspector tab on every pawn with an ideology; shows all world ideologies sorted by opinion, with a breakdown tooltip (structural / personal / relationship) and a "Convictions" column listing the pawn's per-issue stances and strengths
- **Enriched certainty bar** — the social card's certainty display now shows the daily change rate and, if active, the inactivity penalty for neglecting precept rituals and activities

## Settings

All sliders snap to a marked default when dragged close, so individual values are easy to reset without wiping everything.

**Ideoligion**
- *Belief:* certainty drift rate (how fast certainty closes on its target); relational band range (max contribution from co-ideologue opinion); practice band range (max contribution from precept moodlets); crisis-of-faith threshold (certainty floor below which a crisis can trigger).
- *Conversion:* spontaneous conversion pace; certainty multiplier applied after a directed conversion attempt (lower = conversion attempts destabilise faith more).

**Precept**
- *Belief:* conviction decay per quadrum (natural fade without reinforcement); opposite-stance opposition scale (how steeply disagreeing precepts penalise structural fit).
- *Conversion:* conversion strength multiplier (how hard a directed attempt pulls stances vs. a plain debate win); debate conviction change (how far a won argument moves the loser's stance); friendship bonus on conversion (how much liking the converter amplifies the effect).

A **Compatibility** section holds the save-load minimum certainty floor, used when seeding pawns from a vanilla or pre-Enhanced Beliefs save.

## Compatibility

Tested on RimWorld 1.6. Ideology DLC required. Royalty supported. Anomaly probably supported (untested).

Alpha Memes and Vanilla Ideology Expanded (memes and structures) are supported. Peer Pressure integrates directly — when both mods are active, Peer Pressure's opinion multiplier takes over from the friendship bonus slider.

Primarily needs patches for mods that add precepts; meme-only mods are generally fine. See the compat discussion thread for a full list.

*If loading mid-save, consider raising the SaveCompatMinCertainty setting — initial precept strengths are seeded from current pawn certainty, so very low-certainty pawns may end up underseeded.*

**T's Conversion Staff** ([Steam](https://steamcommunity.com/sharedfiles/filedetails/?id=2890481507)) — compatible and synergistic. The staff's `ConversionPower` bonus applies to the convert ability as normal. Its `SocialIdeoSpreadFrequencyFactor` offset also increases the frequency of Enhanced Beliefs' ideological debates, so a moral guide with the staff will proselytize more aggressively.

## License

Licensed under Creative Commons Attribution 4.0, ([LICENSE](LICENSE))

`SPDX-License-Identifier: CC-BY-4.0`

### Contribution

Unless you explicitly state otherwise, any contribution intentionally submitted
for inclusion in the work by you, shall be licensed as above, without any additional
terms or conditions.
