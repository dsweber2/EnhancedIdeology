# Ideas

Design ideas, not commitments.
Committed work lives in [todo.md](todo.md); how the current system works is in [docs/design.md](docs/design.md).

## Open

### Belief mechanics

- **Debate-obsessed break.**
  A mental break where the pawn goes around starting debates with everyone (the iconoclast agitation covers part of this).
- **Hated faiths colour pawn opinions.**
  A pawn dislikes confident believers of a faith it hates.
  This should affect the opinion of the pawn, not (directly) the opinion of the faith.
- **Self-doubt.**
  Pawns whose traits clash with their own faith get negative thoughts about it ("I hate myself").
  Check overlap with the cognitive-dissonance thoughts before building.
- **Moodlets move the rung too.**
  Precept moods currently shift only conviction strength.
  Ideally they also nudge the preferred rung, but the valley step (`PullStance`) is expensive to run that often.
- **A lecture ritual.**
  A short, cheap ritual so belief can be reinforced more often (the "DIY rituals" mod is a reference).
- **Conversion ritual outcome tiers.**
  Distinct results per quality: for example, a terrible outcome makes the target more certain of their top two stances and gives every attendee a bad thought about the preaching faith.

### Memes and precepts

- **Gestalt:** more durable penalties from interacting with non-Gestalt pawns.
- **Illness (VME):** immunity-boosting genes favour ideoligions that exalt illness.
- **Kind meme vs preferred xenotypes:** the xenotype/diversity friction exists as a precept coupling; the Kind-meme half needs the coupling framework to accept a meme as a source.
- **Recruiting couplings:** precepts about recruiting should change how effective debates are, not opinion.

### UI

- **Why does this pawn believe this?**
  More clarity on where each opinion comes from.

### Other mods

- **Flexible ideoligions:** faith precepts drift toward what its present followers believe, or a splinter faith forms.
  Excluding off-map pawns makes faction faiths more flexible than wanted.
  Alternative: fluid-ideoligion development costs come from the precepts its followers hold.
- **[Ideo roles hate work](https://steamcommunity.com/sharedfiles/filedetails/?id=3284846696):** instead of forbidding the work, the role holder and onlookers get a bad thought and opinion when it is done.
- **[Allied ideologies](https://steamcommunity.com/sharedfiles/filedetails/?id=3768592866)**
- **[Simple Hive Mind](https://steamcommunity.com/sharedfiles/filedetails/?id=3547837397)**
- **[Ideological Virtues](https://steamcommunity.com/sharedfiles/filedetails/?id=3459205505):** probably similar to the xenotype match.
- **[Ideology: More Precepts](https://steamcommunity.com/sharedfiles/filedetails/?id=2559533848)**
- **[Mort's Ideologies](https://steamcommunity.com/workshop/filedetails/?id=2935990253):** Conservationist/Polluter and Empiricism/Faith have structural terms; check the rest.
- **[Gender Works](https://steamcommunity.com/sharedfiles/filedetails/?id=3573504386)**
- rimtalk integration (or something like it) to generate book text descriptions and/or debates

## Implemented

- **Mod settings** for the main rates (certainty drift, band ranges, conversion pace, conviction decay, and others).
- **Certainty setpoint**: certainty drifts toward a fit-based equilibrium instead of decaying, so idle pawns keep a sensible certainty.
- **Stance model**: per-issue stances with distance-based opinion replace flat per-precept opinions; new pawns start with 5–25 conviction per issue.
- **Mod precepts** (Alpha Memes, VIE, optional precepts): opinions are means over held issues, so faiths with more precepts get no free boost.
- **Conversion targets stances**: debates and conversion attempts argue the issue that most drives disagreement; a won attempt knocks certainty down temporarily.
- **Crisis of faith**: the pawn may switch to its preferred faith, then wanders seeking contemplation or a book.
- **Loyalist, nationalist, violent conversion**: brittle certainty; another faith's good moods become cognitive dissonance.
- **Pluralist, egalitarian, diversity of thought**: exposure to other faiths reinforces belief.
- **Apostasy**: stances move less in debates, low-certainty co-believers are disliked, fights over debates are more likely.
- **Gestalt**: a stronger supremacist-style penalty.
- **Elders (VME)**: old pawns favour faiths that venerate the old.
- **Vegan (VME)**: herbivorous pawns favour vegan faiths.
- **Charity and recreation**: a small bonus for every faith that holds them.
- **Xenotype preferences**: favoured xenotypes like the faith, disfavoured ones dislike it.
- **VIE Royal meme**: only pawns on the same map count toward relational opinion, so a large off-map hierarchy no longer maxes it out.
- **Books**: describe their topics and carry their author's conviction; written at a writing desk like any other bill.
- **Debate**: thoughts depend on how far apart the two stances are.
- **Debate as recreation**: pawns that relax by debating mostly debate, and onlookers are swayed toward the winner.
- **Babies and children**: babies have no faith; a child's stances come from the faiths around it, and it joins the best fit.
- **Contemplation precept and need**, with lecterns for moral guides and reliquaries as stronger sites.
- **Conviction decay**: unreinforced conviction slowly fades.
- **Iconoclasm**: targets precept-related structures.
- **Explicit "don't care" precepts**: lower strength, with the difference given to another precept.
- **Anomaly compatibility** and **compatibility with mods that add precepts**.
