# Enhanced Ideology — Steam Workshop description
---
Vanilla ideologious certainty is more or less a mood meter. This mod replaces it with a system that derives from each pawns beliefs and practices. Each pawn develops an opinion of every ideoligion they encounter — shaped by their opinions on execution, slavery, drug use, and a dozen other issues. Whether they stay faithful or drift away is determined by reinforcing those opinions, not just by their mood.

The initial implementation is from Smartkar's Enhanced Beliefs, with some updates and rework by Ilyvion to get it into 1.6. This has diverged sufficiently however that I thought it best to consider it a different mod. 

---
## Certainty

![Info_Certainty](https://images.steamusercontent.com/ugc/16395313985291272682/A3EFBBC3E8F819594B6B86D943DE611ACB2483D6/?imw=5000&imh=5000&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=false)

Certainty is a slow moving drift towards a target set by three bands:

- **Structural**: how well the ideoligion's stances on issues fit this pawn's own beliefs, it's their opinion on the ideoligion (see below).
- **Relational**: how the pawn feels about their co-religionists. If they get along well they'll reinforce each other's beliefs.
- **Practitional**: whether the pawn is actually practicing. Precept moodlets push this up; regularly performing rituals and activities, abstaining from forbidden foods, wearing encouraged clothing, and generally acting in line with the ideoligion increases certainty.
---
## Ideoligion Opinions

![Info_Opinions](https://images.steamusercontent.com/ugc/11981801783309351549/759C3B3B5AA76F75001B3814B1E6C8F73C3131D6/?imw=5000&imh=5000&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=false)

A pawn's opinion of an ideolgion, or Structural fit comes from three things:

- **Issue stances**: each pawn holds a preferred position on each issue (e.g. "execution: Respected if guilty") with a conviction strength. They favour ideoligions whose precepts match and oppose those that don't, weighted by conviction. Traits set the starting point; interactions and moodlets shape them over time. Concretely, this contributes `mean(strength/20) * 100`.
- **Personality fit**: some memes list traits they agree or disagree with. A Nudist pawn in a ideoligion that venerates Flesh Purity structurally fits better; the same pawn in a Transhumanist ideoligion fits worse.
- **Inter-faith relations**: memes that are explicitly about other beliefs (e.g. supremacist, loyalist or guilty memes) apply flat opinion modifiers to all outside ideoligions.

---
## Contemplation
![Info_Contemplation](https://images.steamusercontent.com/ugc/15011342002304492908/B29C6E525BCD7C212E1ECBEE56DABE34F59215C1/?imw=5000&imh=5000&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=false)
Contemplation is a new type of recreation that deepens a pawns certainty in their precepts. During recreation (or more frequently if their ideoligion has it as a precept making it a need) they'll seek out somewhere to sit and reflect. More sanctified places like worship rooms, reliquaries (with or without a relic) or ideoligious statues reinforce beliefs more strongly, with the impressiveness of the room increasing the strength gain.

---
## Debates, conversion and Crises of Faith

![Info_Debates](https://images.steamusercontent.com/ugc/15846418937295764847/13A7CD1D36F7BED3E6769789C81684FC83AA1E7F/?imw=5000&imh=5000&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=false)
There are three ways beliefs change through interaction:

**Social conversion**: any pawn that can do social work can try to convert any other pawn. They will focus on a single issue where they disagree most sharply. If the initiator succeeds, the listener shifts their stance on that issue and if their opinion of the speaker's ideoligion exceeds their certainty they may instantaneously convert. Prisoner conversion just spams this with more regularity.

**Ideological debate**: spontaneous arguments over contested issues or memes. A win shifts the loser's stance on the issue or meme's associated issues; a draw can actually entrench both sides. Pawns with the diversity-of-thought precept enjoy a mood bonus just from debating, while apostates hate losing.

**Moral guide Convert ability**: Like a social conversion but more intense. Targets 1–4 of the recipient's most-contested issues, and similarly can lead to spontaneous conversion. See the tooltip for some details.

Beyond these, a pawn who holds a higher opinion of another ideoligion than their certainty in their own may spontaneously convert, or below a threshold enter a crisis of faith, which could cause a mental break, an ideo switch, or a long contemplation period. Their certainty lands above the crisis threshold afterward, but if their ideoligion opinion hasn't improved, another crisis is guaranteed.

---
## Ideoligion books

![Info_Books](https://images.steamusercontent.com/ugc/16283911562514376382/2DA48114CBC430029CA40933F554A655A4FD30C9/?imw=5000&imh=5000&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=false)

Books are tied to a specific ideoligion and carry per-issue conviction strengths, seeded from the author's own ideosyncratic stances. The more certain the author and the more impressive the materials, the stronger the effect.
### Iconoclasm 

Low certainty pawns who snap into the Iconoclast mental break hunt for ideoligion books anywhere on the map, drag them somewhere clear, place them on the ground, and burn them, debating loudly against other pawns and trying to pick a fight. If there are no books, they'll try to destroy a relic or an altar.

---
## Settings
**Ideoligion**: Settings related to certainty in one's ideoligion, such as drift rate, crisis threshold, or the influence of relationships on certainty.

**Precept**: Settings for issue/precept level mechanics, such as debate and conversion settings or decay rate.

---
## Compatibility

- DLCs: Ideoligion required, Royalty supported, Anomaly implemented but mostly untested (I don't own it).
- Alpha Memes, and Vanilla Ideoligion Expanded - memes and structures supported.
- integrates Peer Pressure (can run both)
- See [here](https://github.com/dsweber2/EnhancedIdeology/blob/main/compatibility.md) for a full list
- If there's one missing see the compatibility discussion
- *If you are loading mid-save, consider adjusting SaveCompatMinCertainty*, as initial precept strengths come from current pawn certainty.

---
### Credits
Original 1.5 version is by SmArtKar, updated and bugfixes by Ilyvion.

Original art by Elseud (preview, icons, banners). Book textures by DetVisor. Both traced into SVG and expanded by me.

Peer Pressure by Joseasoler and Densevoid, integrated here. Can't believe that's not vanilla.

Translations are Claude-generated; corrections and new languages welcome, see the pinned discussion.
### Legal
CC BY 4.0 — use it however, credit me.

### Code generation

I used Claude Code in the process of making this mod. Coding agents are just tools, use them when they work, don't use them when they don't, and learn to tell the difference. Keep thinking.

---
*Source code: [GitHub](https://github.com/dsweber2/EnhancedIdeoligion)*
