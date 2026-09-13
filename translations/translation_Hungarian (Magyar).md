<!-- EnhancedIdeology Translation Template: English → Hungarian (Magyar) -->
<!-- Author: claude -->
<!-- Pipes in text: use \| -->

## Keyed/Books.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.BookReadingBenefit` | {IDEO} - {1} conviction shift/qd | {IDEO} - {1} meggyőződés-változás/negyed |
| `EnhancedIdeology.BookBeliefsHeader` | Beliefs advocated | Hirdetett hittételek |
| `EnhancedIdeology.ShiftRatePerQuadrum` | {0}/qd | {0}/negyed |
| `EnhancedIdeology.LetterReligiousBookDestroyedLabel` | Religious book destroyed | Vallásos könyv megsemmisült |
| `EnhancedIdeology.LetterReligiousBookDestroyedText` | {BOOK}, an important religious book for {IDEO}, has been destroyed. Followers of {IDEO} won't be happy about it. | {BOOK}, a {IDEO} számára fontos vallásos könyv megsemmisült. A {IDEO} követői nem lesznek boldogok emiatt. |
| `EnhancedIdeology.DestroyBookByBurningItUpsetIdeo` | Destroy {BOOK} by burning it. This will upset {IDEO_memberNamePlural} of {IDEO}. | Semmisítsd meg {BOOK}-t elégetéssel. Ez felháborítja a {IDEO} tagjait: {IDEO_memberNamePlural}. |
| `EnhancedIdeology.DestroyBookByBurningIt` | Destroy {BOOK} by burning it. | Semmisítsd meg {BOOK}-t elégetéssel. |
| `EnhancedIdeology.NoColonistCanDestroy` | No colonist can destroy {BOOK}. | Egyetlen gyarmatos sem tudja megsemmisíteni {BOOK}-t. |
| `EnhancedIdeology.ConfirmDestroyBookByBurningItUpsetIdeo` | Are you sure you want to make {PAWN} burn {BOOK}? Doing so will greatly upset all {IDEO_memberNamePlural} of {IDEO}. | Biztosan elégetted {PAWN}-val {BOOK}-t? Ez nagymértékben felháborítja a {IDEO} összes tagját: {IDEO_memberNamePlural}. |
| `EnhancedIdeology.ConfirmDestroyBookByBurningIt` | Are you sure you want to make {PAWN} burn {BOOK}? | Biztosan el akarod égettetni {PAWN}-val {BOOK}-t? |
| `EnhancedIdeology.BookBurningSuccess` | {PAWN} has destroyed {BOOK}. This has greatly upset {IDEO_memberNamePlural} of {IDEO}. | {PAWN} megsemmisítette {BOOK}-t. Ez nagymértékben felháborította a {IDEO} tagjait: {IDEO_memberNamePlural}. |

## Keyed/Contemplation.xml

| key | source | translation |
|-----|--------|-------------|
| `EB_NoPewToPrayIn` | {PAWN_nameDef} couldn't find a ritual seat to contemplate in for {IDEO_name}. | {PAWN_nameDef} nem talált szertartási ülőhelyet a {IDEO_name} elmélkedéséhez. |
| `EB_ContemplationRoomDisrespected` | {PAWN_nameDef}'s {IDEO_name} contemplation room is disrespected. | {PAWN_nameDef} {IDEO_name} elmélkedőszobája nem megfelelő állapotban van. |
| `EB_ContemplationActivityReport` | contemplating (gain: {0}/hr) | elmélkedik (nyereség: {0}/óra) |
| `EB_ContemplationSiteStatLabel` | Contemplation gain | Elmélkedési nyereség |
| `EB_ContemplationSiteStatReport` | Conviction reinforcement rate per hour during contemplation. Shown for the selected pawn, or as a base multiplier when no pawn is selected. Scales with room impressiveness and the pawn's current conviction strength. Additional +10% per fellow believer in the room. | Elmélkedés közbeni meggyőződés-erősítési ráta óránként. A kiválasztott karakter értékét mutatja, vagy alap-szorzóként jelenik meg, ha nincs karakter kiválasztva. A szoba lenyűgözőségével és a karakter meggyőződés-erősségével arányos. Minden további hívő jelenléte +10%-ot ad a szobában. |
| `EB_ContemplationSiteInspect` | Contemplation gain: {0} | Elmélkedési nyereség: {0} |

## Keyed/Convert.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.Convert.CertaintyKnock` | Temporary certainty reduction: {0} | Átmeneti meggyőződés-csökkenés: {0} |
| `EnhancedIdeology.Convert.SuccessChance` | Chance to win the debate: {0} | A vita megnyerésének esélye: {0} |
| `EnhancedIdeology.Convert.ConversionChance` | Chance to convert if you win: {0} | Áttérítés esélye győzelem esetén: {0} |
| `EnhancedIdeology.Convert.TargetBeliefs` | Target beliefs (a random 1-{0} of): | Megcélzott hittételek (véletlenszerűen 1-{0} ezek közül): |
| `EnhancedIdeology.Convert.NoOpposition` | They oppose nothing your faith preaches - there is nothing to convert. | Semmit sem utasítanak el a hitedből – nincs mit áttéríteni. |
| `EnhancedIdeology.Convert.Factors` | Factors (you vs. them): | Tényezők (te vs. ők): |
| `EnhancedIdeology.Convert.Factor.ConversionPower` | Conversion power: {0} vs {1} | Áttérítési erő: {0} vs {1} |
| `EnhancedIdeology.Convert.Factor.Intellectual` | Intellectual debate: {0} vs {1} | Intellektuális vita: {0} vs {1} |
| `EnhancedIdeology.Convert.Factor.Social` | Social impact: {0} vs {1} | Társadalmi hatás: {0} vs {1} |

## Keyed/Interaction_IdeologicalDebate.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.IdeologicalDebateOutcomeSocialFight` | {PAWN1_labelShort} tried to debate ideological views with {PAWN2_labelShort}. This led to a social fight! | {PAWN1_labelShort} ideológiai vitát kezdeményezett {PAWN2_labelShort}-val. Ez verekedésbe torkollott! |
| `EnhancedIdeology.LetterLabelIdeologicalDebateConversion` | Ideological conversion | Ideológiai áttérítés |
| `EnhancedIdeology.LetterIdeologicalDebateConversionText` | After debating {ISSUE_label}, {CONVINCER_labelShort} convinced {CONVINCED_labelShort} to abandon {OLDIDEO_name} and embrace {NEWIDEO_name}. | {ISSUE_label} megvitatása után {CONVINCER_labelShort} meggyőzte {CONVINCED_labelShort}-t, hogy hagyja el a {OLDIDEO_name}-t és fogadja el a {NEWIDEO_name}-t. |
| `EnhancedIdeology.DebateLog.AboutMeme` | [INITIATOR_nameDef] debated [RECIPIENT_nameDef] about the meme [TOPIC_label]. | [INITIATOR_nameDef] vitázott [RECIPIENT_nameDef]-vel a [TOPIC_label] méméről. |
| `EnhancedIdeology.DebateLog.AboutPrecept` | [INITIATOR_nameDef] debated [TOPIC_label] with [RECIPIENT_nameDef]. | [INITIATOR_nameDef] vitázott [TOPIC_label]-ről [RECIPIENT_nameDef]-vel. |
| `EnhancedIdeology.DebateLog.Generic` | [INITIATOR_nameDef] debated with [RECIPIENT_nameDef]. | [INITIATOR_nameDef] vitázott [RECIPIENT_nameDef]-vel. |
| `EnhancedIdeology.DebateSent.InitiatorMoved` | [INITIATOR_nameDef] moved [RECIPIENT_nameDef] towards stance "[WINNING_STANCE_label]". | [INITIATOR_nameDef] közelebb vitte [RECIPIENT_nameDef]-t a(z) „[WINNING_STANCE_label]" állásponthoz. |
| `EnhancedIdeology.DebateSent.RecipientMoved` | [RECIPIENT_nameDef] moved [INITIATOR_nameDef] towards stance "[WINNING_STANCE_label]". | [RECIPIENT_nameDef] közelebb vitte [INITIATOR_nameDef]-t a(z) „[WINNING_STANCE_label]" állásponthoz. |
| `EnhancedIdeology.DebateSent.WinnerPersuasive` | [WINNER_nameDef] proved more persuasive. | [WINNER_nameDef] meggyőzőbbnek bizonyult. |
| `EnhancedIdeology.DebateSent.Draw` | Neither changed their view. | Egyikük sem változtatott véleményén. |
| `EnhancedIdeology.JobReport_Debating` | Debating {0} | Vitázik {0}-val |
| `EnhancedIdeology.CrisisLog.Wander` | [INITIATOR_nameDef] experienced a crisis of faith. | [INITIATOR_nameDef] hiterősist élt át. |
| `EnhancedIdeology.CrisisLog.MoodBreak` | [INITIATOR_nameDef]'s crisis of faith compounded [INITIATOR_possessive] misery. | [INITIATOR_nameDef] hiterősis súlyosbította [INITIATOR_possessive] nyomorát. |

## Keyed/Reassure.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.Reassure.CertaintyGain` | Certainty gain on win: +{0} | Meggyőződés-növekedés győzelem esetén: +{0} |
| `EnhancedIdeology.Reassure.SuccessChance` | Chance to win the debate: {0} | A vita megnyerésének esélye: {0} |
| `EnhancedIdeology.Reassure.SelfTarget` | Self-reassurance: guaranteed success | Önmegerősítés: garantált siker |
| `EnhancedIdeology.Reassure.TargetBeliefs` | Beliefs to reinforce (a random 1-{0} of): | Megerősítendő hittételek (véletlenszerűen 1-{0} ezek közül): |
| `EnhancedIdeology.Reassure.NoHeterodoxy` | Their beliefs are already orthodox - there is nothing to reinforce. | Hitük már ortodox – nincs mit megerősíteni. |
| `EnhancedIdeology.Reassure.FailMessage` | {0} failed to reassure {1}. | {0} nem tudta megerősíteni {1}-t. |

## Keyed/Settings.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.TranslationCredit` | Translation: dsweber2 | Fordítás: claude |
| `EnhancedIdeology.Section.Ideoligion` | Ideology | Ideológia |
| `EnhancedIdeology.Section.Precept` | Precept | Előírás |
| `EnhancedIdeology.Section.Compat` | Compatibility | Kompatibilitás |
| `EnhancedIdeology.Section.Debug` | Debug | Hibakeresés |
| `EnhancedIdeology.SubSection.Belief` | Personal Certainty dynamics | Személyes meggyőződés dinamikája |
| `EnhancedIdeology.SubSection.Conversion` | Conversion effects | Áttérítési hatások |
| `EnhancedIdeology.CertaintyDriftRate` | Certainty drift rate: {0}/day | Meggyőződés-sodródási ráta: {0}/nap |
| `EnhancedIdeology.CertaintyDriftRate.Tip` | How fast a pawn's certainty moves toward its target each day. Higher = faith reacts faster to circumstances. Makes for more dynamic beliefs. | Milyen gyorsan közelít egy karakter meggyőződése a célértékhez naponta. Magasabb = a hit gyorsabban reagál a körülményekre. Dinamikusabb hittételeket eredményez. |
| `EnhancedIdeology.RelationalMaxRange` | Relational influence: ±{0} | Kapcsolati hatás: ±{0} |
| `EnhancedIdeology.RelationalMaxRange.Tip` | The most that a pawn's opinion of their co-religionists can raise or lower their target certainty. | Legfeljebb ennyivel emelheti vagy csökkentheti egy karakter hittestvéreiről alkotott véleménye a célmeggyőződését. |
| `EnhancedIdeology.PracticeMaxRange` | Practice influence: ±{0} | Gyakorlati hatás: ±{0} |
| `EnhancedIdeology.PracticeMaxRange.Tip` | The most that precept moods (from rituals, veneration, taboos, etc) can raise or lower target certainty. | Legfeljebb ennyivel emelheti vagy csökkentheti az előírásokból eredő hangulat (szertartások, tisztelet, tabuk stb.) a célmeggyőződést. |
| `EnhancedIdeology.CrisisThreshold` | Crisis of faith threshold: {0} | Hiterősis-küszöb: {0} |
| `EnhancedIdeology.CrisisThreshold.Tip` | When a pawn's certainty falls below this, they may have a crisis-of-faith breakdown. Higher = more crises. | Ha egy karakter meggyőződése ez alá esik, hiterősis történhet. Magasabb = több krízis. |
| `EnhancedIdeology.ConversionPace` | Conversion pace: {0} | Áttérítési ütem: {0} |
| `EnhancedIdeology.ConversionPace.Tip` | How quickly pawns abandon a poorly-fitting faith on their own, relative to default. Higher = spontaneous conversions are more likely. | Milyen gyorsan hagynak fel a karakterek önként egy nem illő hittel az alapértékhez képest. Magasabb = spontán áttérések valószínűbbek. |
| `EnhancedIdeology.ConversionCertaintyKnock` | Conversion attempt Certainty loss: {0} | Áttérítési kísérlet meggyőződés-vesztesége: {0} |
| `EnhancedIdeology.ConversionCertaintyKnock.Tip` | When a moral guide wins a conversion debate but doesn't immediately flip the pawn, their certainty is multiplied by this. 1x = an 80%-certain pawn stays at 80%; 0.8x = an 80%-certain pawn drops to 64%. Lower = conversion attempts more impactful. | Ha az erkölcsi vezető megnyeri az áttérítési vitát, de a karakter nem tér át azonnal, a meggyőződésük ezzel szorzódik. 1x = egy 80%-os meggyőződésű karakter 80%-on marad; 0,8x = 80%-ról 64%-ra esik. Alacsonyabb = az áttérítési kísérletek hatásosabbak. |
| `EnhancedIdeology.ConvictionDecayRate` | Conviction decay: {0}/quadrum | Meggyőződés-kopás: {0}/negyed |
| `EnhancedIdeology.ConvictionDecayRate.Tip` | How much conviction strength all precepts lose per quadrum. Note that 100% conviction is at a precept average of 20, max is 50. Higher = stronger need for reinforcing beliefs. | Mennyit veszítenek az összes előírás meggyőződés-erősségéből negyed évenként. Megjegyzés: 100%-os meggyőződés egy 20-as előírás-átlagot jelent, a maximum 50. Magasabb = erősebb igény a hittételek megerősítésére. |
| `EnhancedIdeology.PreceptOppositionScale` | Opposite-stance opposition: {0} | Ellentétes álláspont ellenállása: {0} |
| `EnhancedIdeology.PreceptOppositionScale.Tip` | How strongly a pawn opposes the stance at the opposite extreme of an issue, as a fraction of their conviction. At 100%, a pawn with Cannibalism: Disapproved 12 has opinion -12 of Cannibalism: Required (ravenous). 0% = indifferent to opposing stances; 100% = equal and opposite to their support for their own at the furthest extreme precept. | Milyen erősen ellenzi egy karakter a kérdés ellentétes szélső álláspontját, meggyőződésük töredékeként. 100%-nál egy Kannibalizmus: Nem javasolt 12 értékű karakter véleménye -12 a Kannibalizmus: Kötelező (falánk) álláspontra. 0% = közömbös az ellentétes álláspontok iránt; 100% = egyenlő és ellentétes a saját álláspont támogatásával a legszélső előírásnál. |
| `EnhancedIdeology.ConversionStancePull` | Conversion strength: {0} | Áttérítési erősség: {0} |
| `EnhancedIdeology.ConversionStancePull.Tip` | How much harder a successful conversion attempt (a moral guide's directed action) pushes a pawn's beliefs than an ordinary conversion attempt. Higher = faster conversions. | Mennyivel erősebben tolja egy sikeres áttérítési kísérlet (erkölcsi vezető irányított cselekvése) a karakter hitét, mint egy közönséges kísérlet. Magasabb = gyorsabb áttérések. |
| `EnhancedIdeology.DebateConvictionChange` | Debate conviction change: {0} | Vita meggyőződés-változása: {0} |
| `EnhancedIdeology.DebateConvictionChange.Tip` | How far a single won argument moves belief on the issue, relative to default. Shifts both the precept and the conviction in that precept simultaneously. Higher = each won debate persuades more. Leads to faster conversions. | Mennyire mozdítja el a kérdésben a hitet egyetlen megnyert érv az alapértékhez képest. Egyidejűleg változtatja az előírást és az abban lévő meggyőződést. Magasabb = minden megnyert vita jobban győz meg. Gyorsabb áttérésekhez vezet. |
| `EnhancedIdeology.ConversionOpinionMultiplier` | Friendship bonus (conversion): {0} | Barátsági bónusz (áttérítés): {0} |
| `EnhancedIdeology.ConversionOpinionMultiplier.Tip` | How much a pawn's opinion of someone trying to change their precept amplifies the effect. At 1x and opinion +100, both the certainty knock and the belief pull are doubled. At 0x, opinion has no effect. Not used when Peer Pressure is installed — that mod's own multiplier setting takes over instead. | Mennyivel erősíti a hatást egy karakter véleménye arról, aki meg akarja változtatni az előírását. 1x értéknél és +100 véleménynél mind a meggyőződés-csökkenés, mind a hittétel-vonzás megduplázódik. 0x értéknél a véleménynek nincs hatása. Nem használható, ha a Peer Pressure mod telepítve van – helyette az a mod saját szorzó-beállítása lép életbe. |
| `EnhancedIdeology.SaveCompatMinCertainty` | Save-load minimum certainty: {0} | Mentés-betöltés minimális meggyőződése: {0} |
| `EnhancedIdeology.SaveCompatMinCertainty.Tip` | When loading a vanilla or pre-mod save, pawns whose certainty is below this have their beliefs seeded to at least this target instead of zero. Prevents a pawn with 0% certainty from being left with no convictions whatsoever. | Vanilla vagy mod előtti mentés betöltésekor az ez alatti meggyőződésű karakterek hitét legalább erre a célértékre állítja nulla helyett. Megakadályozza, hogy egy 0%-os meggyőződésű karakter teljesen meggyőződés nélkül maradjon. |
| `EnhancedIdeology.DebugInteractionWorkers` | DEV: Debug Interaction Workers | FEJ: Interakciós munkások hibakeresése |
| `EnhancedIdeology.DebugInteractionWorkers.Tip` | Enables debug logging for interaction workers when dev mode is active. I suggest you leave this off unless debugging Enhanced Ideology specifically. | Engedélyezi az interakciós munkások hibakeresési naplózását, ha a fejlesztői mód aktív. Javasolt kikapcsolt állapotban hagyni, kivéve ha kifejezetten az Enhanced Ideology-t hibakeresed. |

## Keyed/TabOpinion.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.TabOpinion` | Ideology | Ideológia |
| `EnhancedIdeology.IdeologyOpinions` | Ideology Opinions | Ideológiai vélemények |
| `EnhancedIdeology.BeliefsHeader` | Issues | Kérdések |
| `EnhancedIdeology.ColIssue` | Issue | Kérdés |
| `EnhancedIdeology.ColStance` | Stance | Álláspont |
| `EnhancedIdeology.ColStrength` | Strength | Erősség |
| `EnhancedIdeology.StanceDontCare` | Don't care | Mindegy |
| `EnhancedIdeology.StanceTooltip` | On the issue of {1}, {PAWN_nameDef} has the stance {2} with conviction {3}. {IDEO_name} preaches {5}; {PAWN_nameDef}'s opinion of that stance: {6}. | A {1} kérdésben {PAWN_nameDef} álláspontja {2}, meggyőződése {3}. A {IDEO_name} {5}-t hirdet; {PAWN_nameDef} véleménye erről az álláspontról: {6}. |
| `EnhancedIdeology.PawnOpinionTooltip` | {PAWN_nameDef}'s opinion of {IDEO_name}: {2} | {PAWN_nameDef} véleménye a {IDEO_name}-ről: {2} |
| `EnhancedIdeology.PawnOptionToolTip.FromMemesAndPrecepts` | From memes and precepts: {0} | Mémekből és előírásokból: {0} |
| `EnhancedIdeology.PawnOptionToolTip.FromPersonalBeliefs` | From personal beliefs: {0} | Személyes meggyőződésből: {0} |
| `EnhancedIdeology.PawnOptionToolTip.FromInterpersonalRelationships` | From interpersonal relationships: {0} | Személyközi kapcsolatokból: {0} |

## Keyed/TabSocial.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.PawnCertaintyTooltip` | {PAWN_nameDef}'s certainty in {IDEO_name}: {2} | {PAWN_nameDef} meggyőződése a {IDEO_name}-ben: {2} |
| `EnhancedIdeology.CertaintyTarget` | Target certainty: {0} | Célmeggyőződés: {0} |
| `EnhancedIdeology.CertainChangePerDay` | Drifting toward it at {0}/day | Feléje sodródik: {0}/nap |
| `EnhancedIdeology.CertaintyBandStructural` | Structural (beliefs & nature): {0} | Strukturális (hittételek és természet): {0} |
| `EnhancedIdeology.CertaintyBandRelational` | Congregation: {0} | Gyülekezet: {0} |
| `EnhancedIdeology.CertaintyBandPractice` | Practice (recent rites): {0} | Gyakorlat (közelmúlt szertartásai): {0} |
| `EnhancedIdeology.CertaintyPreceptDecay` | Precept decay: {0}/quadrum | Előírás-kopás: {0}/negyed |
| `EnhancedIdeology.CouplingPenalty` | Opposed practices | Ellentétes gyakorlatok |
| `EnhancedIdeology.DietGeneAntiMeat` | Obligate herbivore | Kötelező növényevő |
| `EnhancedIdeology.DietGeneProMeat` | Obligate carnivore | Kötelező húsevő |
| `EnhancedIdeology.XenotypePreferred` | Preferred xenotype | Előnyben részesített xenotípus |
| `EnhancedIdeology.XenotypeDisapproved` | Disapproved xenotype | Nem javasolt xenotípus |

## Keyed/Work_CompleteReligiousBook.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.IdeologyMissingRole` | Ideoligion does not have the required '{ROLE}' role. | Az ideológiából hiányzik a szükséges '{ROLE}' szerep. |
| `EnhancedIdeology.PawnMissingRequiredRole` | {PAWN} does not have the required '{ROLE}' role. | {PAWN} nem rendelkezik a szükséges '{ROLE}' szereppel. |
| `EnhancedIdeology.PawnIsNotAuthor` | {PAWN} is not the author of the book. | {PAWN} nem a könyv szerzője. |
| `EnhancedIdeology.BookIsNotForPawnIdeoligion` | The book is not for {PAWN_possessive} ideology. | A könyv nem {PAWN_possessive} ideológiájához tartozik. |
| `EnhancedIdeology.NoFreeValidLecternFound` | No available, accessible lectern found to do work on. | Nem található szabad, elérhető felolvasóállvány a munkához. |
| `EnhancedIdeology.InsufficientCertaintyToWrite` | Certainty must be above 90% to write religious texts. | A vallásos szövegek írásához a meggyőződésnek 90% fölé kell emelkednie. |

## DefInjected/ThingDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Ideobook.label` | religious book | vallásos könyv |
| `EB_Ideobook.description` | A book containing fictional or true stories for the pleasure and edification of the reader. | Kitalált vagy valós történeteket tartalmazó könyv az olvasó örömére és épülésére. |
| `EB_UnfinishedDeskIdeobook.label` | unfinished religious book | befejezetlen vallásos könyv |
| `EB_UnfinishedDeskIdeobook.description` | An unfinished religious book being written at the writing desk. | Az írásasztalnál készülő, befejezetlen vallásos könyv. |
| `EB_WritingDesk.label` | writing desk | írásasztal |
| `EB_WritingDesk.description` | A desk for composing ideological texts. Only colonists with deep conviction in their beliefs can write at it. | Ideológiai szövegek írásához való asztal. Csak mélyen meggyőzött gyarmatosok írhatnak rajta. |

## DefInjected/HistoryEventDef

| key | source | translation |
|-----|--------|-------------|
| `EB_DestroyedReligiousBook.label` | destroyed religious book | vallásos könyv megsemmisült |
| `EB_BookDestroyed.label` | religious book was destroyed | vallásos könyvet megsemmisítettek |
| `EB_Contemplated.label` | contemplated | elmélkedett |

## DefInjected/HediffDef

| key | source | translation |
|-----|--------|-------------|
| `EB_BrainwipeRecovery.label` | belief reformation | hittétel-újjáalakulás |
| `EB_BrainwipeRecovery.description` | This person's beliefs were shattered by a psychic brainwipe. Their convictions are weak and unfocused, leaving them unusually open to ideological influence. | Ennek a személynek a hittételeit pszichikai agymosás zúzta szét. Meggyőződéseik gyengék és diffúzak, szokatlanul fogékonnyá téve őket az ideológiai befolyásra. |

## DefInjected/InspirationDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ReligiousEnlightenment.label` | religious enlightenment | vallási megvilágosodás |
| `EB_ReligiousEnlightenment.beginLetter` | [PAWN_pronoun] feels a moment of divine clarity and is compelled to write a religious text. | [PAWN_pronoun] isteni tisztánlátás pillanatát érzi, és vallásos szöveg írására kényszerül. |
| `EB_ReligiousEnlightenment.baseInspectLine` | Inspired: Religious enlightenment | Ihletett: Vallási megvilágosodás |
| `EB_ReligiousEnlightenment.endMessage` | [PAWN_nameDef]'s moment of religious enlightenment has passed. | [PAWN_nameDef] vallási megvilágosodásának pillanata elmúlt. |

## DefInjected/InteractionDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ConversionLog.label` | conversion | áttérítés |
| `EB_ConversionLog.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] converted to a new faith. | r_logentry->[INITIATOR_nameDef] új hitre tért. |
| `EB_CrisisOfFaithLog.label` | crisis of faith | hiterősis |
| `EB_CrisisOfFaithLog.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] experienced a crisis of faith. | r_logentry->[INITIATOR_nameDef] hiterősist élt át. |
| `EB_IdeologicalDebateMeme.label` | ideological debate over memes | ideológiai vita mémekről |
| `EB_IdeologicalDebateMeme.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] and [RECIPIENT_nameDef] debated memes. | r_logentry->[INITIATOR_nameDef] és [RECIPIENT_nameDef] mémekről vitáztak. |
| `EB_IdeologicalDebatePrecept.label` | ideological debate over precepts | ideológiai vita előírásokról |
| `EB_IdeologicalDebatePrecept.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] and [RECIPIENT_nameDef] debated beliefs. | r_logentry->[INITIATOR_nameDef] és [RECIPIENT_nameDef] hittételekről vitáztak. |

## DefInjected/RulePackDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Sentence_DebateWon.rulePack.rulesStrings.0` | sent->[INITIATOR_nameDef] won the debate. | sent->[INITIATOR_nameDef] megnyerte a vitát. |
| `EB_Sentence_InitiatorWon.rulePack.rulesStrings.0` | sent->[INITIATOR_nameDef] proved more persuasive. | sent->[INITIATOR_nameDef] meggyőzőbbnek bizonyult. |
| `EB_Sentence_RecipientWon.rulePack.rulesStrings.0` | sent->[RECIPIENT_nameDef] proved more persuasive. | sent->[RECIPIENT_nameDef] meggyőzőbbnek bizonyult. |
| `EB_Sentence_DebateDraw.rulePack.rulesStrings.0` | sent->Neither changed their view. | sent->Egyikük sem változtatott véleményén. |

## DefInjected/JobDef

| key | source | translation |
|-----|--------|-------------|
| `EB_IconoclastDebate.reportString` | haranguing TargetA about ideology. |  |
| `EB_PlaceAndBurnUntilDestroyed.reportString` | placing TargetA on the ground to burn. | TargetA-t a földre helyezi, hogy elégesse. |
| `EB_Pray.reportString` | contemplating. | elmélkedik. |

## DefInjected/WorkGiverDef

| key | source | translation |
|-----|--------|-------------|
| `EB_WriteIdeobookAtDesk.label` | write religious books | vallásos könyvek írása |
| `EB_WriteIdeobookAtDesk.verb` | write | ír |
| `EB_WriteIdeobookAtDesk.gerund` | writing religious books | vallásos könyvek írása |

## DefInjected/MentalStateDef

| key | source | translation |
|-----|--------|-------------|
| `EB_CrisisOfFaith.label` | crisis of faith | hiterősis |
| `EB_CrisisOfFaith.beginLetterLabel` | crisis of faith | hiterősis |
| `EB_CrisisOfFaith.beginLetter` | {0} is having a crisis of faith.\n\n[PAWN_pronoun] will spend the next day or two in contemplation, questioning [PAWN_possessive] beliefs. | {0} hiterősisben van.\n\n[PAWN_pronoun] a következő egy-két napot elmélkedéssel tölti, megkérdőjelezve [PAWN_possessive] hittételeit. |
| `EB_CrisisOfFaith.recoveryMessage` | {0}'s crisis of faith has passed. | {0} hiterősise elmúlt. |
| `EB_CrisisOfFaith.baseInspectLine` | Mental state: Crisis of faith | Mentális állapot: Hiterősis |
| `EB_Iconoclast.label` | iconoclast | képromboló |
| `EB_Iconoclast.beginLetter` | {0} had a mental break and is being an iconoclast.\n\n[PAWN_pronoun] is going try and burn religious books. | {0} mentálisan összeomlott és képrombolóként viselkedik.\n\n[PAWN_pronoun] megpróbálja elégetni a vallásos könyveket. |
| `EB_Iconoclast.recoveryMessage` | {0} is no longer being an iconoclast. | {0} már nem viselkedik képrombolóként. |
| `EB_Iconoclast.baseInspectLine` | Mental state: Iconoclast | Mentális állapot: Képromboló |

## DefInjected/IssueDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Contemplation.label` | contemplation | elmélkedés |

## DefInjected/NeedDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Contemplation.label` | contemplation | elmélkedés |
| `EB_Contemplation.description` | This pawn's ideology requires regular contemplation. Neglecting it erodes their sense of devotion. | Ennek a karakternek az ideológiája rendszeres elmélkedést követel meg. Az elhanyagolása alássa odaadásukat. |

## DefInjected/PreceptDef

| key | source | translation |
|-----|--------|-------------|
| `Contemplation_Required.label` | required | kötelező |
| `Contemplation_Required.description` | Contemplation is a sacred obligation. Members must contemplate regularly, and the moral guide is expected to lead by example. | Az elmélkedés szent kötelesség. A tagoknak rendszeresen elmélkedniük kell, és az erkölcsi vezető köteles jó példával elöl járni. |
| `Contemplation_Respected.label` | respected | tisztelt |
| `Contemplation_Respected.description` | Contemplation is a meaningful practice, and those who contemplate regularly are held in higher regard. | Az elmélkedés értelmes gyakorlat, és azokat, akik rendszeresen elmélkednek, nagyobb tisztelet övezi. |
| `Contemplation_Normal.label` | acceptable | elfogadható |
| `Contemplation_Normal.description` | Contemplation is a personal matter — neither encouraged nor discouraged. | Az elmélkedés személyes ügy – sem nem ösztönzik, sem nem tiltják. |
| `Contemplation_Disapproved.label` | disapproved | nem javasolt |
| `Contemplation_Disapproved.description` | Contemplation is seen as weakness or superstition. Members who contemplate are viewed unfavorably. | Az elmélkedést gyengeségnek vagy babonának tekintik. Az elmélkedő tagokat kedvezőtlenül ítélik meg. |
| `Contemplation_Forbidden.label` | forbidden | tiltott |
| `Contemplation_Forbidden.description` | Contemplation is strictly forbidden. Any member caught contemplating will be shunned. | Az elmélkedés szigorúan tilos. Minden elmélkedésen ért tagot kiközösítenek. |

## DefInjected/ThoughtDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ContemplationNeed.stages.0.label` | contemplated regularly | rendszeresen elmélkedett |
| `EB_ContemplationNeed.stages.0.description` | I've been faithful in my contemplations. It brings me peace. | Hűséges voltam elmélkedéseimben. Békét hoz nekem. |
| `EB_ContemplationNeed.stages.1.label` | contemplation neglected | elmélkedés elhanyagolva |
| `EB_ContemplationNeed.stages.1.description` | I haven't found time to contemplate. My devotion is slipping. | Nem találtam időt az elmélkedésre. Odaadásom csúszik. |
| `EB_ContemplationNeed.stages.2.label` | contemplation severely neglected | elmélkedés súlyosan elhanyagolva |
| `EB_ContemplationNeed.stages.2.description` | I've gone far too long without contemplation. I feel spiritually adrift. | Túl sokáig voltam elmélkedés nélkül. Lelkileg elveszettnek érzem magam. |
| `EB_WitnessedContemplation_Approved.stages.0.label` | contemplated devoutly | áhítatosan elmélkedett |
| `EB_WitnessedContemplation_Disapproved.stages.0.label` | engaged in contemplation | elmélkedésbe merült |
| `EB_WitnessedContemplation_Forbidden.stages.0.label` | defied the ban on contemplation | megszegte az elmélkedési tilalmat |
| `EB_ReligiousBookDestroyed.stages.0.label` | religious book destroyed | vallásos könyv megsemmisült |
| `EB_ReligiousBookDestroyed.stages.0.description` | Our religious book was destroyed. That is absolutely inexcusable. | Vallásos könyvünket megsemmisítették. Ez teljesen megbocsáthatatlan. |
| `EB_WroteSacrilegousBinding.stages.0.label` | defiled a sacred text | megszentségtelenített egy szent szöveget |
| `EB_WroteSacrilegousBinding.stages.0.description` | I bound a holy book in animal hide. That was wrong. | Állati bőrbe kötöttem egy szent könyvet. Ez helytelen volt. |
| `EB_ReadingLeatherboundBook.stages.0.label` | reading leather-bound scripture | bőrkötéses szentírást olvas |
| `EB_ReadingLeatherboundBook.stages.0.description` | This holy text is bound in animal hide. Holding it feels wrong. | Ez a szent szöveg állati bőrbe van kötve. Rosszul esik tartani. |
| `EB_GoodDebate.stages.0.label` | good debate | jó vita |
| `EB_GoodDebate.stages.0.description` | We had a stimulating debate about our beliefs. A meeting of open minds. | Serkentő vitát folytattunk hittételeinkről. Nyitott elmék találkozása volt. |
| `EB_BadDebate.stages.0.label` | heretical debate | eretnek vita |
| `EB_BadDebate.stages.0.description` | I had to refute their deviant beliefs. Diversity of thought is a poison. | Cáfolnom kellett az eltévelyedett nézeteiket. A gondolatok sokfélesége méreg. |
| `EB_ProselytizerDebated.stages.0.label` | spread the word | terjesztette az igét |
| `EB_ProselytizerDebated.stages.0.description` | I argued for the truth of my faith. Whatever the outcome, the attempt itself was a righteous act. | Érvekkel képviseltem hitem igazságát. Bármi legyen az eredmény, maga a kísérlet igaz tett volt. |
| `EB_ProselytizerConverted.stages.0.label` | brought a soul to the faith | lelket hozott a hitbe |
| `EB_ProselytizerConverted.stages.0.description` | I brought someone into the light of our faith. There is no greater calling. | Valakit a hitünk fényébe hoztam. Nincs nagyobb hivatás. |
| `EB_ProselytizerFailedConversion.stages.0.label` | failed to convert | sikertelen volt az áttérítés |
| `EB_ProselytizerFailedConversion.stages.0.description` | I tried to bring someone to the faith and failed. They remain in their ignorance. | Megpróbáltam valakit a hithez vezérelni, és kudarcot vallottam. Tudatlanságukban maradnak. |
| `EB_ApostacyDebated.stages.0.label` | faith challenged | hit megkérdőjelezve |
| `EB_ApostacyDebated.stages.0.description` | I had to defend the truth of my faith against a challenger. The very fact they dared question it is an insult. | Meg kellett védeni hitem igazságát egy kihívóval szemben. Már az is sértés, hogy merték megkérdőjelezni. |
| `EB_CognitiveDissonance.stages.0.label` | cognitive dissonance | kognitív disszonancia |
| `EB_CognitiveDissonance.stages.0.description` | I shouldn't be participating in other ideoligion's practices. | Nem kellene más ideológia szertartásaiban részt vennem. |
| `EB_FaithReaffirmed.stages.0.label` | faith reaffirmed | hit megerősítve |
| `EB_FaithReaffirmed.stages.0.description` | Seeing another tradition's way of doing things reminds me why I value my own path. | Egy másik hagyomány módszereit látva emlékeztet, miért értékelem a saját utamat. |
| `EB_LowCertaintyCoBeliever.stages.0.label` | wavering co-believer | ingadozó hittestvér |
| `EB_LowCertaintyCoBeliever.stages.1.label` | faithless co-believer | hitetlen hittestvér |
| `EB_LowCertaintyCoBeliever.stages.2.label` | apostate-hearted co-believer | hitehagyott lelkű hittestvér |

## DefInjected/RecipeDef

| key | source | translation |
|-----|--------|-------------|
| `EB_WriteIdeobook.label` | write religious book | vallásos könyv írása |
| `EB_WriteIdeobook.description` | Write a religious book detailing your ideology's beliefs. Requires high certainty in one's ideoligion (at least 90%). | Írj vallásos könyvet, amely részletezi ideológiád hittételeit. Magas meggyőződést igényel az ideológiában (legalább 90%). |
| `EB_WriteIdeobook.jobString` | writing a religious book. | vallásos könyvet ír. |
| `EB_WriteIllustratedIdeobook.label` | write illustrated religious book | illusztrált vallásos könyv írása |
| `EB_WriteIllustratedIdeobook.description` | Write an illuminated religious book adorned with jems and inks. The quality of materials increases the devotional aspects of reading it. Requires high certainty in one's ideoligion (at least 90%). | Írj illuminált vallásos könyvet, drágakövekkel és tintákkal díszítve. Az anyagok minősége növeli az olvasás áhítatos hatását. Magas meggyőződést igényel az ideológiában (legalább 90%). |
| `EB_WriteIllustratedIdeobook.jobString` | illustrating religious book. | vallásos könyvet illusztrál. |

## DefInjected/RitualAttachableOutcomeEffectDef

| key | source | translation |
|-----|--------|-------------|
| `EB_BeliefReinforcement.label` | belief reinforcement | hittétel-megerősítés |
| `EB_BeliefReinforcement.effectDesc` | participants' convictions will be reinforced toward orthodoxy. | a résztvevők meggyőződései az ortodoxia felé erősödnek. |
| `EB_BeliefReinforcement.letterInfoText` | The ritual deepened the participants' convictions. | A szertartás elmélyítette a résztvevők meggyőződéseit. |
