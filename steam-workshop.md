# Enhanced Ideology — Steam Workshop description
---
Vanilla ideologious certainty is more or less a mood meter. This mod replaces it with a system that derives from each pawns beliefs and practices. Each pawn develops an opinion of every ideoligion they encounter — shaped by their opinions on execution, slavery, drug use, and a dozen other issues. Whether they stay faithful or drift away is determined by reinforcing those opinions, not just by their mood, or whether they've been wolo'd by a moral guide.

The initial implementation is from Smartkar's Enhanced Beliefs, with some updates and rework by Ilyvion to get it into 1.6. This has diverged sufficiently however that I thought it best to consider it a different mod. 

---
## Certainty

![Info_Certainty](Infographics/Info_Certainty.png) (insert image of the social tool-tip here)

Certainty is a (normally) slow moving drifts towards a target set by three bands:

- **Structural**: how well the ideoligion's stances on issues fit this pawn's own beliefs. The main driver. The pawn has preferred precepts for each issue and a strength for each. Debates, books, rituals, and contemplation shift per-issue stances over time.
- **Relational**: how the pawn feels about their co-religionists. If they get along well they'll reinforce each other's beliefs.
- **Practitional**: whether the pawn is actually practicing. Precept moodlets push this up; regularly performing rituals and activities, abstaining from forbidden foods, wearing encouraged clothing, and generally acting in line with the ideoligion increases certainty.
---
## Ideoligion Opinions

![Info_Opinions](Infographics/Info_Opinions.png) (insert image of the opinions tab here)

A pawn's opinion of an ideolgion, or Structural fit comes from three things:

- **Issue stances**: each pawn holds a preferred position on every contested issue (e.g. on execution: "Respected if guilty") with a conviction strength. The pawn has a higher opinion of Ideoligions with matching, or close precepts, with a weight corresponding to their precept strength, with an equal and opposite reaction towards opposing precepts. Pawn traits influence their starting point, but over time their views will evolve through contact with other pawns.
- **Personality fit**: some memes list traits they agree or disagree with. A Nudist pawn in a ideoligion that venerates Flesh Purity structurally fits better; the same pawn in a Transhumanist ideoligion fits worse.
- **Inter-faith relations**: memes that are explicitly about other beliefs (e.g. supremacist, loyalist or guilty memes) apply flat opinion modifiers to all outside ideoligions.

---
## Contemplation
Contemplation is a new type of recreation that deepens a pawns certainty in their precepts and associated issue. During recreation (or when their ideoligion requires it) they'll seek out somewhere to sit and reflect:
(insert image of a couple of praying pawns)
More sanctified places like worship rooms, reliquaries (with or without a relic) or ideoligious statues reinforce beliefs more strongly.

---
## Debates, conversion and Crises of Faith

(image of pawns debating a precept)
There are three ways beliefs change through interaction:

**Social conversion**: any pawn with sufficient social skill can try to convert any other social pawn. They will focus on a single issue where they disagree most sharply. If the initiator succeeds, the listener shifts their stance on that issue and if they have a higher opinion of the speaker's ideoligion they may instantaneously convert. Prisoner conversion uses exactly the same mechanics, just more intentionally applied.

**Ideological debate**: spontaneous arguments over contested issues or memes. A win shifts the loser's stance on the issue or meme's associated issues; a draw can actually entrench both sides. Pawns with the diversity-of-thought precept enjoy a mood bonus just from debating, while apostates hate losing.

**Moral guide Convert ability**: Like a social conversion but more intense. Targets 1–4 of the recipient's most-contested issues, and similarly can lead to spontaneous conversion. See the tooltip for some details.

Beyond these, if they have a higher opinion of any ideoligion other than their certainty in their own, they may spontaneously convert, or if they disbelieve every ideoligion enter a crisis of faith.
The result could be a mental break, a switch of ideoligions, or a long period of constant contemplation to strengthen their beliefs. Either way, certainty lands above the crisis threshold. But if their opinion of the ideoligion isn't strengthened they're guaranteed to enter a crisis again.

---
## Ideoligion books

![Info_Books](Infographics/Info_Books.png) (pawns reading and a pawn writing)

Books are tied to a specific ideoligion and carry per-issue conviction strengths, seeded from the author's own ideosyncratic stances. The more certain the author and the more impressive the materials, the stronger the effect.
### Iconoclasm 

Low certainty pawns who snap into the Iconoclast mental break hunt for ideoligion books anywhere on the map, drag them somewhere clear, place them on the ground, and burn them, debating loudly against other pawns and trying to pick a fight. If there are no books, they'll try to destroy a relic or an altar.

---
## Settings
**Ideoligion**: Settings related to certainty in one's ideoligion, such as drift rate, crisis threshold, or the influence of relationships on certainty.

**Precept**: Settings for issue/precept level mechanics, such as debate and conversion settings or decay rate.

---
## Compatibility

- DLCs: Ideoligion required, Royalty supported, Anomaly implemented but untested (I don't own it).
- Alpha Memes, and Vanilla Ideoligion Expanded - memes and structures supported.
- integrates Peer Pressure (can run both)
- See [here](https://github.com/dsweber2/EnhancedIdeology/blob/main/compatibility.md) for a full list
- If there's one missing see the compatibility discussion
- *If you are loading mid-save, consider adjusting SaveCompatMinCertainty*, as initial precept strengths come from current pawn certainty.

---
### Credits
Original 1.5 version is by SmArtKar, updated and bugfixes by Ilyvion.

Original art is from Elseud for the mod preview as well as the original icons and banners. I did some tracing to get the SVGs included here.

DetVisor made the included book textures, which I also traced into the writing tables.

Thanks to Joseasoler and Densevoid for the Peer Pressure mod, which I have integrated here. Can't believe that's not Vanilla.
#### Translation
At the moment it's all Claude. I will happily accept corrections or new languages; I've compacted the text to translate here: If you give me a form like that translated that would be ideal.
### Legal
Creative Commons 4.0

Creative Commons Attribution 4.0. Use it however you want, credit me.
---
### Code generation
I used Claude Code in the process of making this mod. Coding agents are just tools, use them when they work, don't use them when they don't, and learn to tell the difference. Keep thinking.
---
*Source code: [GitHub](https://github.com/dsweber2/EnhancedIdeoligion)*
