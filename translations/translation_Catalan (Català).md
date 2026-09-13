<!-- EnhancedIdeology Translation Template: English → Catalan (Català) -->
<!-- Author: claude -->
<!-- Pipes in text: use \| -->

## Keyed/Books.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.BookReadingBenefit` | {IDEO} - {1} conviction shift/qd | {IDEO} - canvi de convicció {1}/qd |
| `EnhancedIdeology.BookBeliefsHeader` | Beliefs advocated | Creences defensades |
| `EnhancedIdeology.ShiftRatePerQuadrum` | {0}/qd | {0}/qd |
| `EnhancedIdeology.LetterReligiousBookDestroyedLabel` | Religious book destroyed | Llibre religiós destruït |
| `EnhancedIdeology.LetterReligiousBookDestroyedText` | {BOOK}, an important religious book for {IDEO}, has been destroyed. Followers of {IDEO} won't be happy about it. | {BOOK}, un important llibre religiós per a {IDEO}, ha estat destruït. Els seguidors de {IDEO} no estaran contents. |
| `EnhancedIdeology.DestroyBookByBurningItUpsetIdeo` | Destroy {BOOK} by burning it. This will upset {IDEO_memberNamePlural} of {IDEO}. | Destruir {BOOK} cremant-lo. Això molestarà els {IDEO_memberNamePlural} de {IDEO}. |
| `EnhancedIdeology.DestroyBookByBurningIt` | Destroy {BOOK} by burning it. | Destruir {BOOK} cremant-lo. |
| `EnhancedIdeology.NoColonistCanDestroy` | No colonist can destroy {BOOK}. | Cap colon pot destruir {BOOK}. |
| `EnhancedIdeology.ConfirmDestroyBookByBurningItUpsetIdeo` | Are you sure you want to make {PAWN} burn {BOOK}? Doing so will greatly upset all {IDEO_memberNamePlural} of {IDEO}. | Esteu segurs que voleu que {PAWN} cremi {BOOK}? Fer-ho molestarà molt tots els {IDEO_memberNamePlural} de {IDEO}. |
| `EnhancedIdeology.ConfirmDestroyBookByBurningIt` | Are you sure you want to make {PAWN} burn {BOOK}? | Esteu segurs que voleu que {PAWN} cremi {BOOK}? |
| `EnhancedIdeology.BookBurningSuccess` | {PAWN} has destroyed {BOOK}. This has greatly upset {IDEO_memberNamePlural} of {IDEO}. | {PAWN} ha destruït {BOOK}. Això ha molestat molt els {IDEO_memberNamePlural} de {IDEO}. |

## Keyed/Contemplation.xml

| key | source | translation |
|-----|--------|-------------|
| `EB_NoPewToPrayIn` | {PAWN_nameDef} couldn't find a ritual seat to contemplate in for {IDEO_name}. | {PAWN_nameDef} no ha pogut trobar un seient ritual on contemplar per a {IDEO_name}. |
| `EB_ContemplationRoomDisrespected` | {PAWN_nameDef}'s {IDEO_name} contemplation room is disrespected. | La sala de contemplació de {IDEO_name} de {PAWN_nameDef} és desrespectada. |
| `EB_ContemplationActivityReport` | contemplating (gain: {0}/hr) | contemplant (guany: {0}/h) |
| `EB_ContemplationSiteStatLabel` | Contemplation gain | Guany de contemplació |
| `EB_ContemplationSiteStatReport` | Conviction reinforcement rate per hour during contemplation. Shown for the selected pawn, or as a base multiplier when no pawn is selected. Scales with room impressiveness and the pawn's current conviction strength. Additional +10% per fellow believer in the room. | Taxa de reforç de la convicció per hora durant la contemplació. Es mostra per al peó seleccionat, o com a multiplicador base quan no hi ha cap peó seleccionat. Escala amb la impressivitat de la sala i la força de convicció actual del peó. +10% addicional per cada cocrient a la sala. |
| `EB_ContemplationSiteInspect` | Contemplation gain: {0} | Guany de contemplació: {0} |

## Keyed/Convert.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.Convert.CertaintyKnock` | Temporary certainty reduction: {0} | Reducció temporal de la certesa: {0} |
| `EnhancedIdeology.Convert.SuccessChance` | Chance to win the debate: {0} | Probabilitat de guanyar el debat: {0} |
| `EnhancedIdeology.Convert.ConversionChance` | Chance to convert if you win: {0} | Probabilitat de convertir si guanyeu: {0} |
| `EnhancedIdeology.Convert.TargetBeliefs` | Target beliefs (a random 1-{0} of): | Creences objectiu (1-{0} aleatòries de): |
| `EnhancedIdeology.Convert.NoOpposition` | They oppose nothing your faith preaches - there is nothing to convert. | No s'oposen a res del que predica la vostra fe; no hi ha res per convertir. |
| `EnhancedIdeology.Convert.Factors` | Factors (you vs. them): | Factors (vós vs. ells): |
| `EnhancedIdeology.Convert.Factor.ConversionPower` | Conversion power: {0} vs {1} | Poder de conversió: {0} vs {1} |
| `EnhancedIdeology.Convert.Factor.Intellectual` | Intellectual debate: {0} vs {1} | Debat intel·lectual: {0} vs {1} |
| `EnhancedIdeology.Convert.Factor.Social` | Social impact: {0} vs {1} | Impacte social: {0} vs {1} |

## Keyed/Interaction_IdeologicalDebate.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.IdeologicalDebateOutcomeSocialFight` | {PAWN1_labelShort} tried to debate ideological views with {PAWN2_labelShort}. This led to a social fight! | {PAWN1_labelShort} ha intentat debatre opinions ideològiques amb {PAWN2_labelShort}. Això ha acabat en baralla! |
| `EnhancedIdeology.LetterLabelIdeologicalDebateConversion` | Ideological conversion | Conversió ideològica |
| `EnhancedIdeology.LetterIdeologicalDebateConversionText` | After debating {ISSUE_label}, {CONVINCER_labelShort} convinced {CONVINCED_labelShort} to abandon {OLDIDEO_name} and embrace {NEWIDEO_name}. | Després de debatre sobre {ISSUE_label}, {CONVINCER_labelShort} ha convençut {CONVINCED_labelShort} d'abandonar {OLDIDEO_name} i abraçar {NEWIDEO_name}. |
| `EnhancedIdeology.DebateLog.AboutMeme` | [INITIATOR_nameDef] debated [RECIPIENT_nameDef] about the meme [TOPIC_label]. | [INITIATOR_nameDef] ha debatut amb [RECIPIENT_nameDef] sobre el mem [TOPIC_label]. |
| `EnhancedIdeology.DebateLog.AboutPrecept` | [INITIATOR_nameDef] debated [TOPIC_label] with [RECIPIENT_nameDef]. | [INITIATOR_nameDef] ha debatut [TOPIC_label] amb [RECIPIENT_nameDef]. |
| `EnhancedIdeology.DebateLog.Generic` | [INITIATOR_nameDef] debated with [RECIPIENT_nameDef]. | [INITIATOR_nameDef] ha debatut amb [RECIPIENT_nameDef]. |
| `EnhancedIdeology.DebateSent.InitiatorMoved` | [INITIATOR_nameDef] moved [RECIPIENT_nameDef] towards stance "[WINNING_STANCE_label]". | [INITIATOR_nameDef] ha apropat [RECIPIENT_nameDef] a la postura "[WINNING_STANCE_label]". |
| `EnhancedIdeology.DebateSent.RecipientMoved` | [RECIPIENT_nameDef] moved [INITIATOR_nameDef] towards stance "[WINNING_STANCE_label]". | [RECIPIENT_nameDef] ha apropat [INITIATOR_nameDef] a la postura "[WINNING_STANCE_label]". |
| `EnhancedIdeology.DebateSent.WinnerPersuasive` | [WINNER_nameDef] proved more persuasive. | [WINNER_nameDef] ha resultat més persuasiu. |
| `EnhancedIdeology.DebateSent.Draw` | Neither changed their view. | Cap dels dos ha canviat de parer. |
| `EnhancedIdeology.JobReport_Debating` | Debating {0} | Debatent amb {0} |
| `EnhancedIdeology.CrisisLog.Wander` | [INITIATOR_nameDef] experienced a crisis of faith. | [INITIATOR_nameDef] ha experimentat una crisi de fe. |
| `EnhancedIdeology.CrisisLog.MoodBreak` | [INITIATOR_nameDef]'s crisis of faith compounded [INITIATOR_possessive] misery. | La crisi de fe de [INITIATOR_nameDef] ha agreujat la misèria de [INITIATOR_possessive]. |

## Keyed/Reassure.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.Reassure.CertaintyGain` | Certainty gain on win: +{0} | Guany de certesa en guanyar: +{0} |
| `EnhancedIdeology.Reassure.SuccessChance` | Chance to win the debate: {0} | Probabilitat de guanyar el debat: {0} |
| `EnhancedIdeology.Reassure.SelfTarget` | Self-reassurance: guaranteed success | Autoreafirmació: èxit garantit |
| `EnhancedIdeology.Reassure.TargetBeliefs` | Beliefs to reinforce (a random 1-{0} of): | Creences a reforçar (1-{0} aleatòries de): |
| `EnhancedIdeology.Reassure.NoHeterodoxy` | Their beliefs are already orthodox - there is nothing to reinforce. | Les seves creences ja són ortodoxes; no hi ha res per reforçar. |
| `EnhancedIdeology.Reassure.FailMessage` | {0} failed to reassure {1}. | {0} no ha pogut reafirmar {1}. |

## Keyed/Settings.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.TranslationCredit` | Translation: dsweber2 | Traducció: claude |
| `EnhancedIdeology.Section.Ideoligion` | Ideology | Ideoligió |
| `EnhancedIdeology.Section.Precept` | Precept | Precepte |
| `EnhancedIdeology.Section.Compat` | Compatibility | Compatibilitat |
| `EnhancedIdeology.Section.Debug` | Debug | Depuració |
| `EnhancedIdeology.SubSection.Belief` | Personal Certainty dynamics | Dinàmica de certesa personal |
| `EnhancedIdeology.SubSection.Conversion` | Conversion effects | Efectes de la conversió |
| `EnhancedIdeology.CertaintyDriftRate` | Certainty drift rate: {0}/day | Taxa de deriva de la certesa: {0}/dia |
| `EnhancedIdeology.CertaintyDriftRate.Tip` | How fast a pawn's certainty moves toward its target each day. Higher = faith reacts faster to circumstances. Makes for more dynamic beliefs. | Velocitat a la qual la certesa d'un peó s'acosta al seu objectiu cada dia. Més alt = la fe reacciona més ràpidament a les circumstàncies. Genera creences més dinàmiques. |
| `EnhancedIdeology.RelationalMaxRange` | Relational influence: ±{0} | Influència relacional: ±{0} |
| `EnhancedIdeology.RelationalMaxRange.Tip` | The most that a pawn's opinion of their co-religionists can raise or lower their target certainty. | El màxim que l'opinió d'un peó sobre els seus correligionaris pot augmentar o disminuir la seva certesa objectiu. |
| `EnhancedIdeology.PracticeMaxRange` | Practice influence: ±{0} | Influència de la pràctica: ±{0} |
| `EnhancedIdeology.PracticeMaxRange.Tip` | The most that precept moods (from rituals, veneration, taboos, etc) can raise or lower target certainty. | El màxim que els estats d'ànim dels preceptes (de rituals, veneració, tabús, etc.) poden augmentar o disminuir la certesa objectiu. |
| `EnhancedIdeology.CrisisThreshold` | Crisis of faith threshold: {0} | Llindar de crisi de fe: {0} |
| `EnhancedIdeology.CrisisThreshold.Tip` | When a pawn's certainty falls below this, they may have a crisis-of-faith breakdown. Higher = more crises. | Quan la certesa d'un peó cau per sota d'aquest valor, pot tenir una crisi de fe. Més alt = més crisis. |
| `EnhancedIdeology.ConversionPace` | Conversion pace: {0} | Ritme de conversió: {0} |
| `EnhancedIdeology.ConversionPace.Tip` | How quickly pawns abandon a poorly-fitting faith on their own, relative to default. Higher = spontaneous conversions are more likely. | Velocitat a la qual els peons abandonen espontàniament una fe poc adequada, en relació al valor predeterminat. Més alt = conversions espontànies més probables. |
| `EnhancedIdeology.ConversionCertaintyKnock` | Conversion attempt Certainty loss: {0} | Pèrdua de certesa per intent de conversió: {0} |
| `EnhancedIdeology.ConversionCertaintyKnock.Tip` | When a moral guide wins a conversion debate but doesn't immediately flip the pawn, their certainty is multiplied by this. 1x = an 80%-certain pawn stays at 80%; 0.8x = an 80%-certain pawn drops to 64%. Lower = conversion attempts more impactful. | Quan un guia moral guanya un debat de conversió però no canvia el peó immediatament, la seva certesa es multiplica per aquest valor. 1x = un peó amb 80% de certesa es queda al 80%; 0,8x = cau al 64%. Més baix = intents de conversió més impactants. |
| `EnhancedIdeology.ConvictionDecayRate` | Conviction decay: {0}/quadrum | Decaïment de la convicció: {0}/quàdrum |
| `EnhancedIdeology.ConvictionDecayRate.Tip` | How much conviction strength all precepts lose per quadrum. Note that 100% conviction is at a precept average of 20, max is 50. Higher = stronger need for reinforcing beliefs. | Quanta força de convicció perden tots els preceptes per quàdrum. Nota: el 100% de convicció correspon a una mitjana de preceptes de 20; el màxim és 50. Més alt = necessitat més forta de reforçar les creences. |
| `EnhancedIdeology.PreceptOppositionScale` | Opposite-stance opposition: {0} | Oposició a la postura contrària: {0} |
| `EnhancedIdeology.PreceptOppositionScale.Tip` | How strongly a pawn opposes the stance at the opposite extreme of an issue, as a fraction of their conviction. At 100%, a pawn with Cannibalism: Disapproved 12 has opinion -12 of Cannibalism: Required (ravenous). 0% = indifferent to opposing stances; 100% = equal and opposite to their support for their own at the furthest extreme precept. | Amb quina força s'oposa un peó a la postura de l'extrem oposat d'un tema, com a fracció de la seva convicció. Al 100%, un peó amb Canibalisme: Desaprovat 12 té opinió -12 de Canibalisme: Obligatori (voraç). 0% = indiferent a les postures oposades; 100% = igual i oposat al suport de la pròpia al precepte extrem més allunyat. |
| `EnhancedIdeology.ConversionStancePull` | Conversion strength: {0} | Força de conversió: {0} |
| `EnhancedIdeology.ConversionStancePull.Tip` | How much harder a successful conversion attempt (a moral guide's directed action) pushes a pawn's beliefs than an ordinary conversion attempt. Higher = faster conversions. | Quant més fort empeny les creences d'un peó un intent de conversió exitós (acció dirigida d'un guia moral) que un intent ordinari. Més alt = conversions més ràpides. |
| `EnhancedIdeology.DebateConvictionChange` | Debate conviction change: {0} | Canvi de convicció en debat: {0} |
| `EnhancedIdeology.DebateConvictionChange.Tip` | How far a single won argument moves belief on the issue, relative to default. Shifts both the precept and the conviction in that precept simultaneously. Higher = each won debate persuades more. Leads to faster conversions. | Fins a on mou la creença en el tema un sol argument guanyat, en relació al valor predeterminat. Desplaça alhora el precepte i la convicció en aquell precepte. Més alt = cada debat guanyat persuadeix més. Condueix a conversions més ràpides. |
| `EnhancedIdeology.ConversionOpinionMultiplier` | Friendship bonus (conversion): {0} | Bonus d'amistat (conversió): {0} |
| `EnhancedIdeology.ConversionOpinionMultiplier.Tip` | How much a pawn's opinion of someone trying to change their precept amplifies the effect. At 1x and opinion +100, both the certainty knock and the belief pull are doubled. At 0x, opinion has no effect. Not used when Peer Pressure is installed — that mod's own multiplier setting takes over instead. | Fins a quin punt l'opinió d'un peó sobre algú que intenta canviar el seu precepte amplifica l'efecte. A 1x i opinió +100, tant la reducció de certesa com l'atracció de creença es doblen. A 0x, l'opinió no té cap efecte. No s'utilitza quan Peer Pressure està instal·lat: en aquell cas s'utilitza el multiplicador d'aquell mod. |
| `EnhancedIdeology.SaveCompatMinCertainty` | Save-load minimum certainty: {0} | Certesa mínima en carregar: {0} |
| `EnhancedIdeology.SaveCompatMinCertainty.Tip` | When loading a vanilla or pre-mod save, pawns whose certainty is below this have their beliefs seeded to at least this target instead of zero. Prevents a pawn with 0% certainty from being left with no convictions whatsoever. | En carregar una partida estàndard o anterior al mod, els peons amb certesa per sota d'aquest valor reben com a mínim aquest objectiu en lloc de zero. Evita que un peó amb 0% de certesa quedi sense cap convicció. |
| `EnhancedIdeology.DebugInteractionWorkers` | DEV: Debug Interaction Workers | DEV: Depura Interaction Workers |
| `EnhancedIdeology.DebugInteractionWorkers.Tip` | Enables debug logging for interaction workers when dev mode is active. I suggest you leave this off unless debugging Enhanced Ideology specifically. | Activa el registre de depuració per als interaction workers quan el mode de desenvolupament és actiu. Es recomana deixar-ho desactivat tret que s'estigui depurant Enhanced Ideology específicament. |

## Keyed/TabOpinion.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.TabOpinion` | Ideology | Ideoligió |
| `EnhancedIdeology.IdeologyOpinions` | Ideology Opinions | Opinions sobre la ideoligió |
| `EnhancedIdeology.BeliefsHeader` | Issues | Temes |
| `EnhancedIdeology.ColIssue` | Issue | Tema |
| `EnhancedIdeology.ColStance` | Stance | Postura |
| `EnhancedIdeology.ColStrength` | Strength | Força |
| `EnhancedIdeology.StanceDontCare` | Don't care | Indiferent |
| `EnhancedIdeology.StanceTooltip` | On the issue of {1}, {PAWN_nameDef} has the stance {2} with conviction {3}. {IDEO_name} preaches {5}; {PAWN_nameDef}'s opinion of that stance: {6}. | Sobre el tema de {1}, {PAWN_nameDef} té la postura {2} amb convicció {3}. {IDEO_name} predica {5}; opinió de {PAWN_nameDef} sobre aquesta postura: {6}. |
| `EnhancedIdeology.PawnOpinionTooltip` | {PAWN_nameDef}'s opinion of {IDEO_name}: {2} | Opinió de {PAWN_nameDef} sobre {IDEO_name}: {2} |
| `EnhancedIdeology.PawnOptionToolTip.FromMemesAndPrecepts` | From memes and precepts: {0} | De mems i preceptes: {0} |
| `EnhancedIdeology.PawnOptionToolTip.FromPersonalBeliefs` | From personal beliefs: {0} | De creences personals: {0} |
| `EnhancedIdeology.PawnOptionToolTip.FromInterpersonalRelationships` | From interpersonal relationships: {0} | De relacions interpersonals: {0} |

## Keyed/TabSocial.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.PawnCertaintyTooltip` | {PAWN_nameDef}'s certainty in {IDEO_name}: {2} | Convicció de {PAWN_nameDef} sobre {IDEO_name}: {2} |
| `EnhancedIdeology.CertaintyTarget` | Target certainty: {0} | Certesa objectiu: {0} |
| `EnhancedIdeology.CertainChangePerDay` | Drifting toward it at {0}/day | Derivant-hi a {0}/dia |
| `EnhancedIdeology.CertaintyBandStructural` | Structural (beliefs & nature): {0} | Estructural (creences i naturalesa): {0} |
| `EnhancedIdeology.CertaintyBandRelational` | Congregation: {0} | Congregació: {0} |
| `EnhancedIdeology.CertaintyBandPractice` | Practice (recent rites): {0} | Pràctica (ritus recents): {0} |
| `EnhancedIdeology.CertaintyPreceptDecay` | Precept decay: {0}/quadrum | Decaïment del precepte: {0}/quàdrum |
| `EnhancedIdeology.CouplingPenalty` | Opposed practices | Pràctiques oposades |
| `EnhancedIdeology.DietGeneAntiMeat` | Obligate herbivore | Herbívor obligat |
| `EnhancedIdeology.DietGeneProMeat` | Obligate carnivore | Carnívor obligat |
| `EnhancedIdeology.XenotypePreferred` | Preferred xenotype | Xenotip preferit |
| `EnhancedIdeology.XenotypeDisapproved` | Disapproved xenotype | Xenotip desaprovat |

## Keyed/Work_CompleteReligiousBook.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.IdeologyMissingRole` | Ideoligion does not have the required '{ROLE}' role. | La ideoligió no té el rol '{ROLE}' requerit. |
| `EnhancedIdeology.PawnMissingRequiredRole` | {PAWN} does not have the required '{ROLE}' role. | {PAWN} no té el rol '{ROLE}' requerit. |
| `EnhancedIdeology.PawnIsNotAuthor` | {PAWN} is not the author of the book. | {PAWN} no és l'autor del llibre. |
| `EnhancedIdeology.BookIsNotForPawnIdeoligion` | The book is not for {PAWN_possessive} ideology. | El llibre no és per a la ideoligió de {PAWN_possessive}. |
| `EnhancedIdeology.NoFreeValidLecternFound` | No available, accessible lectern found to do work on. | No s'ha trobat cap faristol disponible i accessible per treballar. |
| `EnhancedIdeology.InsufficientCertaintyToWrite` | Certainty must be above 90% to write religious texts. | La certesa ha de ser superior al 90% per escriure textos religiosos. |

## DefInjected/ThingDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Ideobook.label` | religious book | llibre religiós |
| `EB_Ideobook.description` | A book containing fictional or true stories for the pleasure and edification of the reader. | Un llibre que conté històries fictícies o reals per al plaer i l'edificació del lector. |
| `EB_UnfinishedDeskIdeobook.label` | unfinished religious book | llibre religiós inacabat |
| `EB_UnfinishedDeskIdeobook.description` | An unfinished religious book being written at the writing desk. | Un llibre religiós inacabat que s'està escrivint a l'escriptori. |
| `EB_WritingDesk.label` | writing desk | escriptori d'escriptura |
| `EB_WritingDesk.description` | A desk for composing ideological texts. Only colonists with deep conviction in their beliefs can write at it. | Un escriptori per compondre textos ideològics. Només els colons amb una convicció profunda en les seves creences poden escriure-hi. |

## DefInjected/HistoryEventDef

| key | source | translation |
|-----|--------|-------------|
| `EB_DestroyedReligiousBook.label` | destroyed religious book | llibre religiós destruït |
| `EB_BookDestroyed.label` | religious book was destroyed | el llibre religiós va ser destruït |
| `EB_Contemplated.label` | contemplated | contemplat |

## DefInjected/HediffDef

| key | source | translation |
|-----|--------|-------------|
| `EB_BrainwipeRecovery.label` | belief reformation | reformació de creences |
| `EB_BrainwipeRecovery.description` | This person's beliefs were shattered by a psychic brainwipe. Their convictions are weak and unfocused, leaving them unusually open to ideological influence. | Les creences d'aquesta persona van ser destruïdes per un esborrat psíquic. Les seves conviccions són febles i difuses, la qual cosa les deixa inusualment obertes a la influència ideològica. |

## DefInjected/InspirationDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ReligiousEnlightenment.label` | religious enlightenment | il·luminació religiosa |
| `EB_ReligiousEnlightenment.beginLetter` | [PAWN_pronoun] feels a moment of divine clarity and is compelled to write a religious text. | [PAWN_pronoun] sent un moment de claredat divina i se sent impulsat a escriure un text religiós. |
| `EB_ReligiousEnlightenment.baseInspectLine` | Inspired: Religious enlightenment | Inspirat: Il·luminació religiosa |
| `EB_ReligiousEnlightenment.endMessage` | [PAWN_nameDef]'s moment of religious enlightenment has passed. | El moment d'il·luminació religiosa de [PAWN_nameDef] ha passat. |

## DefInjected/InteractionDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ConversionLog.label` | conversion | conversió |
| `EB_ConversionLog.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] converted to a new faith. | r_logentry->[INITIATOR_nameDef] s'ha convertit a una nova fe. |
| `EB_CrisisOfFaithLog.label` | crisis of faith | crisi de fe |
| `EB_CrisisOfFaithLog.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] experienced a crisis of faith. | r_logentry->[INITIATOR_nameDef] ha experimentat una crisi de fe. |
| `EB_IdeologicalDebateMeme.label` | ideological debate over memes | debat ideològic sobre mems |
| `EB_IdeologicalDebateMeme.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] and [RECIPIENT_nameDef] debated memes. | r_logentry->[INITIATOR_nameDef] i [RECIPIENT_nameDef] han debatut sobre mems. |
| `EB_IdeologicalDebatePrecept.label` | ideological debate over precepts | debat ideològic sobre preceptes |
| `EB_IdeologicalDebatePrecept.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] and [RECIPIENT_nameDef] debated beliefs. | r_logentry->[INITIATOR_nameDef] i [RECIPIENT_nameDef] han debatut sobre creences. |

## DefInjected/RulePackDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Sentence_DebateWon.rulePack.rulesStrings.0` | sent->[INITIATOR_nameDef] won the debate. | sent->[INITIATOR_nameDef] ha guanyat el debat. |
| `EB_Sentence_InitiatorWon.rulePack.rulesStrings.0` | sent->[INITIATOR_nameDef] proved more persuasive. | sent->[INITIATOR_nameDef] ha resultat més persuasiu. |
| `EB_Sentence_RecipientWon.rulePack.rulesStrings.0` | sent->[RECIPIENT_nameDef] proved more persuasive. | sent->[RECIPIENT_nameDef] ha resultat més persuasiu. |
| `EB_Sentence_DebateDraw.rulePack.rulesStrings.0` | sent->Neither changed their view. | sent->Cap dels dos ha canviat d'opinió. |

## DefInjected/JobDef

| key | source | translation |
|-----|--------|-------------|
| `EB_IconoclastDebate.reportString` | haranguing TargetA about ideology. |  |
| `EB_PlaceAndBurnUntilDestroyed.reportString` | placing TargetA on the ground to burn. | col·locant TargetA a terra per cremar. |
| `EB_Pray.reportString` | contemplating. | contemplant. |

## DefInjected/WorkGiverDef

| key | source | translation |
|-----|--------|-------------|
| `EB_WriteIdeobookAtDesk.label` | write religious books | escriure llibres religiosos |
| `EB_WriteIdeobookAtDesk.verb` | write | escriure |
| `EB_WriteIdeobookAtDesk.gerund` | writing religious books | escrivint llibres religiosos |

## DefInjected/MentalStateDef

| key | source | translation |
|-----|--------|-------------|
| `EB_CrisisOfFaith.label` | crisis of faith | crisi de fe |
| `EB_CrisisOfFaith.beginLetterLabel` | crisis of faith | crisi de fe |
| `EB_CrisisOfFaith.beginLetter` | {0} is having a crisis of faith.\n\n[PAWN_pronoun] will spend the next day or two in contemplation, questioning [PAWN_possessive] beliefs. | {0} pateix una crisi de fe.\n\n[PAWN_pronoun] passarà el proper dia o dos en contemplació, qüestionant les creences de [PAWN_possessive]. |
| `EB_CrisisOfFaith.recoveryMessage` | {0}'s crisis of faith has passed. | La crisi de fe de {0} ha passat. |
| `EB_CrisisOfFaith.baseInspectLine` | Mental state: Crisis of faith | Estat mental: Crisi de fe |
| `EB_Iconoclast.label` | iconoclast | iconoclasta |
| `EB_Iconoclast.beginLetter` | {0} had a mental break and is being an iconoclast.\n\n[PAWN_pronoun] is going try and burn religious books. | {0} ha tingut una crisi mental i s'ha tornat iconoclasta.\n\n[PAWN_pronoun] intentarà cremar llibres religiosos. |
| `EB_Iconoclast.recoveryMessage` | {0} is no longer being an iconoclast. | {0} ja no és un iconoclasta. |
| `EB_Iconoclast.baseInspectLine` | Mental state: Iconoclast | Estat mental: Iconoclasta |

## DefInjected/IssueDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Contemplation.label` | contemplation | contemplació |

## DefInjected/NeedDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Contemplation.label` | contemplation | contemplació |
| `EB_Contemplation.description` | This pawn's ideology requires regular contemplation. Neglecting it erodes their sense of devotion. | La ideoligió d'aquest peó requereix contemplació regular. Descuidar-la erosiona el seu sentit de la devoció. |

## DefInjected/PreceptDef

| key | source | translation |
|-----|--------|-------------|
| `Contemplation_Required.label` | required | obligatòria |
| `Contemplation_Required.description` | Contemplation is a sacred obligation. Members must contemplate regularly, and the moral guide is expected to lead by example. | La contemplació és una obligació sagrada. Els membres han de contemplar regularment, i s'espera que el guia moral doni exemple. |
| `Contemplation_Respected.label` | respected | respectada |
| `Contemplation_Respected.description` | Contemplation is a meaningful practice, and those who contemplate regularly are held in higher regard. | La contemplació és una pràctica significativa, i aquells que contemplen regularment gaudeixen d'un major respecte. |
| `Contemplation_Normal.label` | acceptable | acceptable |
| `Contemplation_Normal.description` | Contemplation is a personal matter — neither encouraged nor discouraged. | La contemplació és un assumpte personal: ni s'anima ni es desaconsella. |
| `Contemplation_Disapproved.label` | disapproved | desaprovada |
| `Contemplation_Disapproved.description` | Contemplation is seen as weakness or superstition. Members who contemplate are viewed unfavorably. | La contemplació es considera una debilitat o superstició. Els membres que contemplen són vistos desfavorablement. |
| `Contemplation_Forbidden.label` | forbidden | prohibida |
| `Contemplation_Forbidden.description` | Contemplation is strictly forbidden. Any member caught contemplating will be shunned. | La contemplació està estrictament prohibida. Qualsevol membre sorprès contemplant serà rebutjat. |

## DefInjected/ThoughtDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ContemplationNeed.stages.0.label` | contemplated regularly | contemplat regularment |
| `EB_ContemplationNeed.stages.0.description` | I've been faithful in my contemplations. It brings me peace. | He estat fidel en les meves contemplacions. Em porta pau. |
| `EB_ContemplationNeed.stages.1.label` | contemplation neglected | contemplació descuidada |
| `EB_ContemplationNeed.stages.1.description` | I haven't found time to contemplate. My devotion is slipping. | No he trobat temps per contemplar. La meva devoció s'esmuny. |
| `EB_ContemplationNeed.stages.2.label` | contemplation severely neglected | contemplació greument descuidada |
| `EB_ContemplationNeed.stages.2.description` | I've gone far too long without contemplation. I feel spiritually adrift. | He passat massa temps sense contemplació. Em sento a la deriva espiritualment. |
| `EB_WitnessedContemplation_Approved.stages.0.label` | contemplated devoutly | contemplat devotament |
| `EB_WitnessedContemplation_Disapproved.stages.0.label` | engaged in contemplation | s'ha dedicat a la contemplació |
| `EB_WitnessedContemplation_Forbidden.stages.0.label` | defied the ban on contemplation | ha desafiat la prohibició de la contemplació |
| `EB_ReligiousBookDestroyed.stages.0.label` | religious book destroyed | llibre religiós destruït |
| `EB_ReligiousBookDestroyed.stages.0.description` | Our religious book was destroyed. That is absolutely inexcusable. | El nostre llibre religiós ha estat destruït. Això és absolutament inexcusable. |
| `EB_WroteSacrilegousBinding.stages.0.label` | defiled a sacred text | ha profanat un text sagrat |
| `EB_WroteSacrilegousBinding.stages.0.description` | I bound a holy book in animal hide. That was wrong. | He enquadernat un llibre sagrat amb pell animal. Això era incorrecte. |
| `EB_ReadingLeatherboundBook.stages.0.label` | reading leather-bound scripture | llegint les escriptures en cuir |
| `EB_ReadingLeatherboundBook.stages.0.description` | This holy text is bound in animal hide. Holding it feels wrong. | Aquest text sagrat està enquadernat en pell animal. Tenir-lo a les mans em sembla incorrecte. |
| `EB_GoodDebate.stages.0.label` | good debate | bon debat |
| `EB_GoodDebate.stages.0.description` | We had a stimulating debate about our beliefs. A meeting of open minds. | Hem tingut un debat estimulant sobre les nostres creences. Una trobada de ments obertes. |
| `EB_BadDebate.stages.0.label` | heretical debate | debat heretge |
| `EB_BadDebate.stages.0.description` | I had to refute their deviant beliefs. Diversity of thought is a poison. | He hagut de refutar les seves creences desviades. La diversitat de pensament és un verí. |
| `EB_ProselytizerDebated.stages.0.label` | spread the word | ha difós la paraula |
| `EB_ProselytizerDebated.stages.0.description` | I argued for the truth of my faith. Whatever the outcome, the attempt itself was a righteous act. | He defensat la veritat de la meva fe. Sigui quin sigui el resultat, l'intent en si mateix va ser un acte just. |
| `EB_ProselytizerConverted.stages.0.label` | brought a soul to the faith | ha portat una ànima a la fe |
| `EB_ProselytizerConverted.stages.0.description` | I brought someone into the light of our faith. There is no greater calling. | He portat algú a la llum de la nostra fe. No hi ha vocació més gran. |
| `EB_ProselytizerFailedConversion.stages.0.label` | failed to convert | no ha pogut convertir |
| `EB_ProselytizerFailedConversion.stages.0.description` | I tried to bring someone to the faith and failed. They remain in their ignorance. | He intentat portar algú a la fe i he fracassat. Romanen en la seva ignorància. |
| `EB_ApostacyDebated.stages.0.label` | faith challenged | fe desafiada |
| `EB_ApostacyDebated.stages.0.description` | I had to defend the truth of my faith against a challenger. The very fact they dared question it is an insult. | He hagut de defensar la veritat de la meva fe davant un desafiador. El simple fet que s'atrevissin a qüestionar-la és un insult. |
| `EB_CognitiveDissonance.stages.0.label` | cognitive dissonance | dissonància cognitiva |
| `EB_CognitiveDissonance.stages.0.description` | I shouldn't be participating in other ideoligion's practices. | No hauria de participar en les pràctiques d'altres ideoligions. |
| `EB_FaithReaffirmed.stages.0.label` | faith reaffirmed | fe reafirmada |
| `EB_FaithReaffirmed.stages.0.description` | Seeing another tradition's way of doing things reminds me why I value my own path. | Veure la manera de fer les coses d'una altra tradició em recorda per què valoro el meu propi camí. |
| `EB_LowCertaintyCoBeliever.stages.0.label` | wavering co-believer | correligionari vacil·lant |
| `EB_LowCertaintyCoBeliever.stages.1.label` | faithless co-believer | correligionari sense fe |
| `EB_LowCertaintyCoBeliever.stages.2.label` | apostate-hearted co-believer | correligionari d'esperit apòstata |

## DefInjected/RecipeDef

| key | source | translation |
|-----|--------|-------------|
| `EB_WriteIdeobook.label` | write religious book | escriure llibre religiós |
| `EB_WriteIdeobook.description` | Write a religious book detailing your ideology's beliefs. Requires high certainty in one's ideoligion (at least 90%). | Escriu un llibre religiós que detalli les creences de la teva ideoligió. Requereix una alta certesa en la pròpia ideoligió (almenys el 90%). |
| `EB_WriteIdeobook.jobString` | writing a religious book. | escrivint un llibre religiós. |
| `EB_WriteIllustratedIdeobook.label` | write illustrated religious book | escriure llibre religiós il·lustrat |
| `EB_WriteIllustratedIdeobook.description` | Write an illuminated religious book adorned with jems and inks. The quality of materials increases the devotional aspects of reading it. Requires high certainty in one's ideoligion (at least 90%). | Escriu un llibre religiós il·luminat adornat amb gemmes i tintes. La qualitat dels materials augmenta els aspectes devocionals de llegir-lo. Requereix una alta certesa en la pròpia ideoligió (almenys el 90%). |
| `EB_WriteIllustratedIdeobook.jobString` | illustrating religious book. | il·lustrant un llibre religiós. |

## DefInjected/RitualAttachableOutcomeEffectDef

| key | source | translation |
|-----|--------|-------------|
| `EB_BeliefReinforcement.label` | belief reinforcement | reforç de creences |
| `EB_BeliefReinforcement.effectDesc` | participants' convictions will be reinforced toward orthodoxy. | les conviccions dels participants es reforçaran cap a l'ortodòxia. |
| `EB_BeliefReinforcement.letterInfoText` | The ritual deepened the participants' convictions. | El ritual ha aprofundit les conviccions dels participants. |
