<!-- EnhancedIdeology Translation Template: English → Ukrainian (Українська) -->
<!-- Author: claude -->
<!-- Pipes in text: use \| -->

## Keyed/Books.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.BookReadingBenefit` | {IDEO} - {1} conviction shift/qd | {IDEO} — {1} зміна переконань/кварт. |
| `EnhancedIdeology.BookBeliefsHeader` | Beliefs advocated | Відстоювані переконання |
| `EnhancedIdeology.ShiftRatePerQuadrum` | {0}/qd | {0}/кварт. |
| `EnhancedIdeology.LetterReligiousBookDestroyedLabel` | Religious book destroyed | Релігійну книгу знищено |
| `EnhancedIdeology.LetterReligiousBookDestroyedText` | {BOOK}, an important religious book for {IDEO}, has been destroyed. Followers of {IDEO} won't be happy about it. | {BOOK}, важливу релігійну книгу для {IDEO}, знищено. Послідовники {IDEO} будуть незадоволені. |
| `EnhancedIdeology.DestroyBookByBurningItUpsetIdeo` | Destroy {BOOK} by burning it. This will upset {IDEO_memberNamePlural} of {IDEO}. | Знищити {BOOK} спаленням. Це засмутить {IDEO_memberNamePlural} з {IDEO}. |
| `EnhancedIdeology.DestroyBookByBurningIt` | Destroy {BOOK} by burning it. | Знищити {BOOK} спаленням. |
| `EnhancedIdeology.NoColonistCanDestroy` | No colonist can destroy {BOOK}. | Жоден колоніст не може знищити {BOOK}. |
| `EnhancedIdeology.ConfirmDestroyBookByBurningItUpsetIdeo` | Are you sure you want to make {PAWN} burn {BOOK}? Doing so will greatly upset all {IDEO_memberNamePlural} of {IDEO}. | Ви впевнені, що хочете змусити {PAWN} спалити {BOOK}? Це дуже засмутить усіх {IDEO_memberNamePlural} з {IDEO}. |
| `EnhancedIdeology.ConfirmDestroyBookByBurningIt` | Are you sure you want to make {PAWN} burn {BOOK}? | Ви впевнені, що хочете змусити {PAWN} спалити {BOOK}? |
| `EnhancedIdeology.BookBurningSuccess` | {PAWN} has destroyed {BOOK}. This has greatly upset {IDEO_memberNamePlural} of {IDEO}. | {PAWN} знищив {BOOK}. Це дуже засмутило {IDEO_memberNamePlural} з {IDEO}. |

## Keyed/Contemplation.xml

| key | source | translation |
|-----|--------|-------------|
| `EB_NoPewToPrayIn` | {PAWN_nameDef} couldn't find a ritual seat to contemplate in for {IDEO_name}. | {PAWN_nameDef} не зміг знайти ритуального місця для споглядання заради {IDEO_name}. |
| `EB_ContemplationRoomDisrespected` | {PAWN_nameDef}'s {IDEO_name} contemplation room is disrespected. | Кімнату для споглядання {PAWN_nameDef} ({IDEO_name}) не поважають. |
| `EB_ContemplationActivityReport` | contemplating (gain: {0}/hr) | споглядає (приріст: {0}/год) |
| `EB_ContemplationSiteStatLabel` | Contemplation gain | Приріст від споглядання |
| `EB_ContemplationSiteStatReport` | Conviction reinforcement rate per hour during contemplation. Shown for the selected pawn, or as a base multiplier when no pawn is selected. Scales with room impressiveness and the pawn's current conviction strength. Additional +10% per fellow believer in the room. | Швидкість підсилення переконань за годину під час споглядання. Відображається для вибраного колоніста або як базовий множник, якщо нікого не вибрано. Залежить від вражаючості кімнати та поточної сили переконань колоніста. Додатково +10% за кожного вірянина у кімнаті. |
| `EB_ContemplationSiteInspect` | Contemplation gain: {0} | Приріст від споглядання: {0} |

## Keyed/Convert.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.Convert.CertaintyKnock` | Temporary certainty reduction: {0} | Тимчасове зниження впевненості: {0} |
| `EnhancedIdeology.Convert.SuccessChance` | Chance to win the debate: {0} | Шанс виграти дебати: {0} |
| `EnhancedIdeology.Convert.ConversionChance` | Chance to convert if you win: {0} | Шанс навернення при перемозі: {0} |
| `EnhancedIdeology.Convert.TargetBeliefs` | Target beliefs (a random 1-{0} of): | Цільові переконання (випадкові 1–{0} з): |
| `EnhancedIdeology.Convert.NoOpposition` | They oppose nothing your faith preaches - there is nothing to convert. | Вони не заперечують жодного з постулатів вашої віри — навертати нікого. |
| `EnhancedIdeology.Convert.Factors` | Factors (you vs. them): | Чинники (ви проти них): |
| `EnhancedIdeology.Convert.Factor.ConversionPower` | Conversion power: {0} vs {1} | Сила навернення: {0} проти {1} |
| `EnhancedIdeology.Convert.Factor.Intellectual` | Intellectual debate: {0} vs {1} | Інтелектуальна суперечка: {0} проти {1} |
| `EnhancedIdeology.Convert.Factor.Social` | Social impact: {0} vs {1} | Соціальний вплив: {0} проти {1} |

## Keyed/Interaction_IdeologicalDebate.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.IdeologicalDebateOutcomeSocialFight` | {PAWN1_labelShort} tried to debate ideological views with {PAWN2_labelShort}. This led to a social fight! | {PAWN1_labelShort} спробував посперечатися про ідеологічні погляди з {PAWN2_labelShort}. Це призвело до соціальної сутички! |
| `EnhancedIdeology.LetterLabelIdeologicalDebateConversion` | Ideological conversion | Ідеологічне навернення |
| `EnhancedIdeology.LetterIdeologicalDebateConversionText` | After debating {ISSUE_label}, {CONVINCER_labelShort} convinced {CONVINCED_labelShort} to abandon {OLDIDEO_name} and embrace {NEWIDEO_name}. | Після дебатів про {ISSUE_label}, {CONVINCER_labelShort} переконав {CONVINCED_labelShort} залишити {OLDIDEO_name} та прийняти {NEWIDEO_name}. |

## Keyed/Reassure.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.Reassure.CertaintyGain` | Certainty gain on win: +{0} | Приріст впевненості при перемозі: +{0} |
| `EnhancedIdeology.Reassure.SuccessChance` | Chance to win the debate: {0} | Шанс виграти дебати: {0} |
| `EnhancedIdeology.Reassure.SelfTarget` | Self-reassurance: guaranteed success | Самозаспокоєння: гарантований успіх |
| `EnhancedIdeology.Reassure.TargetBeliefs` | Beliefs to reinforce (a random 1-{0} of): | Переконання для підсилення (випадкові 1–{0} з): |
| `EnhancedIdeology.Reassure.NoHeterodoxy` | Their beliefs are already orthodox - there is nothing to reinforce. | Їхні переконання вже ортодоксальні — підсилювати нічого. |
| `EnhancedIdeology.Reassure.FailMessage` | {0} failed to reassure {1}. | {0} не зміг заспокоїти {1}. |

## Keyed/Settings.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.TranslationCredit` | Translation: dsweber2 | Переклад: claude |
| `EnhancedIdeology.Section.Ideoligion` | Ideology | Ідеологія |
| `EnhancedIdeology.Section.Precept` | Precept | Правило |
| `EnhancedIdeology.Section.Compat` | Compatibility | Сумісність |
| `EnhancedIdeology.Section.Debug` | Debug | Налагодження |
| `EnhancedIdeology.SubSection.Belief` | Personal Certainty dynamics | Динаміка особистої впевненості |
| `EnhancedIdeology.SubSection.Conversion` | Conversion effects | Ефекти навернення |
| `EnhancedIdeology.CertaintyDriftRate` | Certainty drift rate: {0}/day | Швидкість дрейфу впевненості: {0}/день |
| `EnhancedIdeology.CertaintyDriftRate.Tip` | How fast a pawn's certainty moves toward its target each day. Higher = faith reacts faster to circumstances. Makes for more dynamic beliefs. | Наскільки швидко впевненість колоніста рухається до цілі кожного дня. Вище = віра реагує на обставини швидше. Робить переконання більш динамічними. |
| `EnhancedIdeology.RelationalMaxRange` | Relational influence: ±{0} | Реляційний вплив: ±{0} |
| `EnhancedIdeology.RelationalMaxRange.Tip` | The most that a pawn's opinion of their co-religionists can raise or lower their target certainty. | Максимальний вплив, який думка колоніста про одновірців може мати на цільову впевненість. |
| `EnhancedIdeology.PracticeMaxRange` | Practice influence: ±{0} | Вплив практики: ±{0} |
| `EnhancedIdeology.PracticeMaxRange.Tip` | The most that precept moods (from rituals, veneration, taboos, etc) can raise or lower target certainty. | Максимальний вплив настрою від правил (ритуали, вшанування, табу тощо) на цільову впевненість. |
| `EnhancedIdeology.CrisisThreshold` | Crisis of faith threshold: {0} | Поріг кризи віри: {0} |
| `EnhancedIdeology.CrisisThreshold.Tip` | When a pawn's certainty falls below this, they may have a crisis-of-faith breakdown. Higher = more crises. | Коли впевненість колоніста падає нижче цього значення, може статися криза віри. Вище = більше криз. |
| `EnhancedIdeology.ConversionPace` | Conversion pace: {0} | Темп навернення: {0} |
| `EnhancedIdeology.ConversionPace.Tip` | How quickly pawns abandon a poorly-fitting faith on their own, relative to default. Higher = spontaneous conversions are more likely. | Наскільки швидко колоністи самостійно покидають погано відповідну їм віру відносно стандарту. Вище = спонтанні навернення частіші. |
| `EnhancedIdeology.ConversionCertaintyKnock` | Conversion attempt Certainty loss: {0} | Втрата впевненості при спробі навернення: {0} |
| `EnhancedIdeology.ConversionCertaintyKnock.Tip` | When a moral guide wins a conversion debate but doesn't immediately flip the pawn, their certainty is multiplied by this. 1x = an 80%-certain pawn stays at 80%; 0.8x = an 80%-certain pawn drops to 64%. Lower = conversion attempts more impactful. | Коли моральний провідник виграє дебати про навернення, але не переконує одразу, впевненість помножується на це значення. 1x = колоніст з 80% впевненості залишається на 80%; 0.8x = падає до 64%. Нижче = спроби навернення ефективніші. |
| `EnhancedIdeology.ConvictionDecayRate` | Conviction decay: {0}/quadrum | Спад переконань: {0}/квартал |
| `EnhancedIdeology.ConvictionDecayRate.Tip` | How much conviction strength all precepts lose per quadrum. Note that 100% conviction is at a precept average of 20, max is 50. Higher = stronger need for reinforcing beliefs. | Наскільки сила переконань усіх правил знижується за квартал. 100% переконаність відповідає середньому 20, максимум — 50. Вище = більша потреба в підкріпленні переконань. |
| `EnhancedIdeology.PreceptOppositionScale` | Opposite-stance opposition: {0} | Протилежна позиція — опозиція: {0} |
| `EnhancedIdeology.PreceptOppositionScale.Tip` | How strongly a pawn opposes the stance at the opposite extreme of an issue, as a fraction of their conviction. At 100%, a pawn with Cannibalism: Disapproved 12 has opinion -12 of Cannibalism: Required (ravenous). 0% = indifferent to opposing stances; 100% = equal and opposite to their support for their own at the furthest extreme precept. | Наскільки сильно колоніст заперечує протилежну крайню позицію, як частку від переконань. При 100%, колоніст із «Канібалізм: Засуджено 12» матиме думку -12 щодо «Канібалізм: Обов'язково». 0% = байдужість до протилежних позицій; 100% = рівна й протилежна підтримці власної крайньої позиції. |
| `EnhancedIdeology.ConversionStancePull` | Conversion strength: {0} | Сила навернення: {0} |
| `EnhancedIdeology.ConversionStancePull.Tip` | How much harder a successful conversion attempt (a moral guide's directed action) pushes a pawn's beliefs than an ordinary conversion attempt. Higher = faster conversions. | Наскільки сильніше успішна спроба навернення (цілеспрямована дія морального провідника) змінює переконання порівняно зі звичайною спробою. Вище = швидше навернення. |
| `EnhancedIdeology.DebateConvictionChange` | Debate conviction change: {0} | Зміна переконань у дебатах: {0} |
| `EnhancedIdeology.DebateConvictionChange.Tip` | How far a single won argument moves belief on the issue, relative to default. Shifts both the precept and the conviction in that precept simultaneously. Higher = each won debate persuades more. Leads to faster conversions. | Наскільки далеко одна виграна суперечка змінює переконання з питання відносно стандарту. Одночасно зміщує і правило, і переконання в ньому. Вище = кожні виграні дебати переконують більше. Прискорює навернення. |
| `EnhancedIdeology.ConversionOpinionMultiplier` | Friendship bonus (conversion): {0} | Бонус дружби (навернення): {0} |
| `EnhancedIdeology.ConversionOpinionMultiplier.Tip` | How much a pawn's opinion of someone trying to change their precept amplifies the effect. At 1x and opinion +100, both the certainty knock and the belief pull are doubled. At 0x, opinion has no effect. Not used when Peer Pressure is installed — that mod's own multiplier setting takes over instead. | Наскільки думка колоніста про того, хто намагається змінити його правило, підсилює ефект. При 1x і думці +100 і зниження впевненості, і притягування переконань подвоюються. При 0x думка не має ефекту. Не використовується, якщо встановлено Peer Pressure — замість цього застосовується власний множник того мода. |
| `EnhancedIdeology.SaveCompatMinCertainty` | Save-load minimum certainty: {0} | Мінімальна впевненість при завантаженні: {0} |
| `EnhancedIdeology.SaveCompatMinCertainty.Tip` | When loading a vanilla or pre-mod save, pawns whose certainty is below this have their beliefs seeded to at least this target instead of zero. Prevents a pawn with 0% certainty from being left with no convictions whatsoever. | При завантаженні ванільного або доісторичного збереження колоністи з впевненістю нижче цього значення отримують переконання мінімум на цій позначці замість нуля. Запобігає тому, щоб колоніст з 0% впевненості залишився без жодних переконань. |
| `EnhancedIdeology.DebugInteractionWorkers` | DEV: Debug Interaction Workers | DEV: Налагодження обробників взаємодій |
| `EnhancedIdeology.DebugInteractionWorkers.Tip` | Enables debug logging for interaction workers when dev mode is active. I suggest you leave this off unless debugging Enhanced Ideology specifically. | Вмикає журнал налагодження для обробників взаємодій у режимі розробника. Рекомендується вимкнути, якщо ви не налагоджуєте Enhanced Ideology. |

## Keyed/TabOpinion.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.TabOpinion` | Ideology | Ідеологія |
| `EnhancedIdeology.IdeologyOpinions` | Ideology Opinions | Думки про ідеологію |
| `EnhancedIdeology.BeliefsHeader` | Issues | Питання |
| `EnhancedIdeology.ColIssue` | Issue | Питання |
| `EnhancedIdeology.ColStance` | Stance | Позиція |
| `EnhancedIdeology.ColStrength` | Strength | Сила |
| `EnhancedIdeology.StanceDontCare` | Don't care | Байдуже |
| `EnhancedIdeology.StanceTooltip` | On the issue of {1}, {PAWN_nameDef} has the stance {2} with conviction {3}. {IDEO_name} preaches {5}; {PAWN_nameDef}'s opinion of that stance: {6}. | З питання {1}, {PAWN_nameDef} займає позицію {2} з переконанням {3}. {IDEO_name} проповідує {5}; думка {PAWN_nameDef} щодо цієї позиції: {6}. |
| `EnhancedIdeology.PawnOpinionTooltip` | {PAWN_nameDef}'s opinion of {IDEO_name}: {2} | Думка {PAWN_nameDef} про {IDEO_name}: {2} |
| `EnhancedIdeology.PawnOptionToolTip.FromMemesAndPrecepts` | From memes and precepts: {0} | Від рис та правил: {0} |
| `EnhancedIdeology.PawnOptionToolTip.FromPersonalBeliefs` | From personal beliefs: {0} | Від особистих переконань: {0} |
| `EnhancedIdeology.PawnOptionToolTip.FromInterpersonalRelationships` | From interpersonal relationships: {0} | Від міжособистісних стосунків: {0} |

## Keyed/TabSocial.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.PawnCertaintyTooltip` | {PAWN_nameDef}'s certainty in {IDEO_name}: {2} | Впевненість {PAWN_nameDef} у {IDEO_name}: {2} |
| `EnhancedIdeology.CertaintyTarget` | Target certainty: {0} | Цільова впевненість: {0} |
| `EnhancedIdeology.CertainChangePerDay` | Drifting toward it at {0}/day | Дрейфує до цілі: {0}/день |
| `EnhancedIdeology.CertaintyBandStructural` | Structural (beliefs & nature): {0} | Структурна (переконання та природа): {0} |
| `EnhancedIdeology.CertaintyBandRelational` | Congregation: {0} | Громада: {0} |
| `EnhancedIdeology.CertaintyBandPractice` | Practice (recent rites): {0} | Практика (недавні обряди): {0} |
| `EnhancedIdeology.CertaintyPreceptDecay` | Precept decay: {0}/quadrum | Спад правил: {0}/квартал |
| `EnhancedIdeology.CouplingPenalty` | Opposed practices | Протилежні практики |
| `EnhancedIdeology.DietGeneAntiMeat` | Obligate herbivore | Обов'язковий травоїдний |
| `EnhancedIdeology.DietGeneProMeat` | Obligate carnivore | Обов'язковий м'ясоїд |
| `EnhancedIdeology.XenotypePreferred` | Preferred xenotype | Бажаний ксенотип |
| `EnhancedIdeology.XenotypeDisapproved` | Disapproved xenotype | Засуджений ксенотип |

## Keyed/Work_CompleteReligiousBook.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.IdeologyMissingRole` | Ideoligion does not have the required '{ROLE}' role. | В ідеології немає необхідної ролі «{ROLE}». |
| `EnhancedIdeology.PawnMissingRequiredRole` | {PAWN} does not have the required '{ROLE}' role. | {PAWN} не має необхідної ролі «{ROLE}». |
| `EnhancedIdeology.PawnIsNotAuthor` | {PAWN} is not the author of the book. | {PAWN} не є автором книги. |
| `EnhancedIdeology.BookIsNotForPawnIdeoligion` | The book is not for {PAWN_possessive} ideology. | Книга не призначена для ідеології {PAWN_possessive}. |
| `EnhancedIdeology.NoFreeValidLecternFound` | No available, accessible lectern found to do work on. | Не знайдено вільної доступної кафедри для роботи. |
| `EnhancedIdeology.InsufficientCertaintyToWrite` | Certainty must be above 90% to write religious texts. | Для написання релігійних текстів впевненість повинна бути вище 90%. |

## DefInjected/ThingDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Ideobook.label` | religious book | релігійна книга |
| `EB_Ideobook.description` | A book containing fictional or true stories for the pleasure and edification of the reader. | Книга, що містить вигадані або правдиві оповіді для задоволення та просвіти читача. |
| `EB_UnfinishedDeskIdeobook.label` | unfinished religious book | незакінчена релігійна книга |
| `EB_UnfinishedDeskIdeobook.description` | An unfinished religious book being written at the writing desk. | Незакінчена релігійна книга, яку пишуть за письмовим столом. |
| `EB_WritingDesk.label` | writing desk | письмовий стіл |
| `EB_WritingDesk.description` | A desk for composing ideological texts. Only colonists with deep conviction in their beliefs can write at it. | Стіл для складання ідеологічних текстів. Писати за ним можуть лише колоністи з глибокою впевненістю у своїх переконаннях. |

## DefInjected/HistoryEventDef

| key | source | translation |
|-----|--------|-------------|
| `EB_DestroyedReligiousBook.label` | destroyed religious book | знищила релігійну книгу |
| `EB_BookDestroyed.label` | religious book was destroyed | релігійну книгу знищено |
| `EB_Contemplated.label` | contemplated | споглядав |

## DefInjected/HediffDef

| key | source | translation |
|-----|--------|-------------|
| `EB_BrainwipeRecovery.label` | belief reformation | переформування переконань |
| `EB_BrainwipeRecovery.description` | This person's beliefs were shattered by a psychic brainwipe. Their convictions are weak and unfocused, leaving them unusually open to ideological influence. | Переконання цієї людини були зруйновані психічним промиванням мозку. Їхні переконання слабкі та розпорошені, що робить їх незвично відкритими до ідеологічного впливу. |

## DefInjected/InspirationDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ReligiousEnlightenment.label` | religious enlightenment | релігійне просвітлення |
| `EB_ReligiousEnlightenment.beginLetter` | [PAWN_pronoun] feels a moment of divine clarity and is compelled to write a religious text. | [PAWN_pronoun] відчуває мить божественної ясності і відчуває потяг написати релігійний текст. |
| `EB_ReligiousEnlightenment.baseInspectLine` | Inspired: Religious enlightenment | Натхнення: Релігійне просвітлення |
| `EB_ReligiousEnlightenment.endMessage` | [PAWN_nameDef]'s moment of religious enlightenment has passed. | Мить релігійного просвітлення [PAWN_nameDef] минула. |

## DefInjected/InteractionDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ConversionLog.label` | conversion | навернення |
| `EB_ConversionLog.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] converted to a new faith. | r_logentry->[INITIATOR_nameDef] перейшов до нової віри. |
| `EB_CrisisOfFaithLog.label` | crisis of faith | криза віри |
| `EB_CrisisOfFaithLog.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] experienced a crisis of faith. | r_logentry->[INITIATOR_nameDef] пережив кризу віри. |
| `EB_IdeologicalDebateMeme.label` | ideological debate over memes | ідеологічна дискусія про риси |
| `EB_IdeologicalDebateMeme.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] and [RECIPIENT_nameDef] debated memes. | r_logentry->[INITIATOR_nameDef] та [RECIPIENT_nameDef] дискутували про риси. |
| `EB_IdeologicalDebatePrecept.label` | ideological debate over precepts | ідеологічна дискусія про правила |
| `EB_IdeologicalDebatePrecept.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] and [RECIPIENT_nameDef] debated beliefs. | r_logentry->[INITIATOR_nameDef] та [RECIPIENT_nameDef] дискутували про переконання. |

## DefInjected/RulePackDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Sentence_DebateWon.rulePack.rulesStrings.0` | sent->[INITIATOR_nameDef] won the debate. | sent->[INITIATOR_nameDef] виграв дебати. |
| `EB_Sentence_InitiatorWon.rulePack.rulesStrings.0` | sent->[INITIATOR_nameDef] proved more persuasive. | sent->[INITIATOR_nameDef] виявився переконливішим. |
| `EB_Sentence_RecipientWon.rulePack.rulesStrings.0` | sent->[RECIPIENT_nameDef] proved more persuasive. | sent->[RECIPIENT_nameDef] виявився переконливішим. |
| `EB_Sentence_DebateDraw.rulePack.rulesStrings.0` | sent->Neither changed their view. | sent->Жоден не змінив своєї думки. |

## DefInjected/JobDef

| key | source | translation |
|-----|--------|-------------|
| `EB_PlaceAndBurnUntilDestroyed.reportString` | placing TargetA on the ground to burn. | кладе ціль на землю для спалення. |
| `EB_Pray.reportString` | contemplating. | споглядає. |

## DefInjected/WorkGiverDef

| key | source | translation |
|-----|--------|-------------|
| `EB_WriteIdeobookAtDesk.label` | write religious books | писати релігійні книги |
| `EB_WriteIdeobookAtDesk.verb` | write | писати |
| `EB_WriteIdeobookAtDesk.gerund` | writing religious books | написання релігійних книг |

## DefInjected/MentalStateDef

| key | source | translation |
|-----|--------|-------------|
| `EB_CrisisOfFaith.label` | crisis of faith | криза віри |
| `EB_CrisisOfFaith.beginLetterLabel` | crisis of faith | криза віри |
| `EB_CrisisOfFaith.beginLetter` | {0} is having a crisis of faith.\n\n[PAWN_pronoun] will spend the next day or two in contemplation, questioning [PAWN_possessive] beliefs. | {0} переживає кризу віри.\n\n[PAWN_pronoun] проведе наступний день-два у спогляданні, ставлячи під сумнів [PAWN_possessive] переконання. |
| `EB_CrisisOfFaith.recoveryMessage` | {0}'s crisis of faith has passed. | Криза віри {0} минула. |
| `EB_CrisisOfFaith.baseInspectLine` | Mental state: Crisis of faith | Психічний стан: Криза віри |
| `EB_Iconoclast.label` | iconoclast | іконоборець |
| `EB_Iconoclast.beginLetter` | {0} had a mental break and is being an iconoclast.\n\n[PAWN_pronoun] is going try and burn religious books. | {0} зазнав психічного зриву та стає іконоборцем.\n\n[PAWN_pronoun] спробує спалити релігійні книги. |
| `EB_Iconoclast.recoveryMessage` | {0} is no longer being an iconoclast. | {0} більше не іконоборець. |
| `EB_Iconoclast.baseInspectLine` | Mental state: Iconoclast | Психічний стан: Іконоборець |

## DefInjected/IssueDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Contemplation.label` | contemplation | споглядання |

## DefInjected/NeedDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Contemplation.label` | contemplation | споглядання |
| `EB_Contemplation.description` | This pawn's ideology requires regular contemplation. Neglecting it erodes their sense of devotion. | Ідеологія цього колоніста вимагає регулярного споглядання. Нехтування ним підриває відчуття відданості. |

## DefInjected/PreceptDef

| key | source | translation |
|-----|--------|-------------|
| `Contemplation_Required.label` | required | обов'язково |
| `Contemplation_Required.description` | Contemplation is a sacred obligation. Members must contemplate regularly, and the moral guide is expected to lead by example. | Споглядання є священним обов'язком. Члени повинні споглядати регулярно, а моральний провідник зобов'язаний подавати приклад. |
| `Contemplation_Respected.label` | respected | шанована |
| `Contemplation_Respected.description` | Contemplation is a meaningful practice, and those who contemplate regularly are held in higher regard. | Споглядання є значущою практикою, і тих, хто споглядає регулярно, поважають більше. |
| `Contemplation_Normal.label` | acceptable | прийнятна |
| `Contemplation_Normal.description` | Contemplation is a personal matter — neither encouraged nor discouraged. | Споглядання — особиста справа, яку ні заохочують, ні забороняють. |
| `Contemplation_Disapproved.label` | disapproved | засуджена |
| `Contemplation_Disapproved.description` | Contemplation is seen as weakness or superstition. Members who contemplate are viewed unfavorably. | Споглядання вважається слабкістю або забобоном. Членів, які споглядають, сприймають негативно. |
| `Contemplation_Forbidden.label` | forbidden | заборонена |
| `Contemplation_Forbidden.description` | Contemplation is strictly forbidden. Any member caught contemplating will be shunned. | Споглядання суворо заборонене. Будь-якого члена, якого спіймають на спогляданні, уникатимуть. |

## DefInjected/ThoughtDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ContemplationNeed.stages.0.label` | contemplated regularly | споглядав регулярно |
| `EB_ContemplationNeed.stages.0.description` | I've been faithful in my contemplations. It brings me peace. | Я був вірний своїм спогляданням. Це приносить мені спокій. |
| `EB_ContemplationNeed.stages.1.label` | contemplation neglected | споглядання занедбане |
| `EB_ContemplationNeed.stages.1.description` | I haven't found time to contemplate. My devotion is slipping. | Я не знаходив часу для споглядання. Моя відданість слабшає. |
| `EB_ContemplationNeed.stages.2.label` | contemplation severely neglected | споглядання дуже занедбане |
| `EB_ContemplationNeed.stages.2.description` | I've gone far too long without contemplation. I feel spiritually adrift. | Я надто довго обходився без споглядання. Я відчуваю духовну втрату. |
| `EB_WitnessedContemplation_Approved.stages.0.label` | contemplated devoutly | споглядав побожно |
| `EB_WitnessedContemplation_Disapproved.stages.0.label` | engaged in contemplation | займався спогляданням |
| `EB_WitnessedContemplation_Forbidden.stages.0.label` | defied the ban on contemplation | порушив заборону на споглядання |
| `EB_ReligiousBookDestroyed.stages.0.label` | religious book destroyed | релігійну книгу знищено |
| `EB_ReligiousBookDestroyed.stages.0.description` | Our religious book was destroyed. That is absolutely inexcusable. | Нашу релігійну книгу знищено. Це абсолютно неприпустимо. |
| `EB_WroteSacrilegousBinding.stages.0.label` | defiled a sacred text | осквернив священний текст |
| `EB_WroteSacrilegousBinding.stages.0.description` | I bound a holy book in animal hide. That was wrong. | Я оправив священну книгу в шкіру тварини. Це було неправильно. |
| `EB_ReadingLeatherboundBook.stages.0.label` | reading leather-bound scripture | читає священне писання в шкіряній оправі |
| `EB_ReadingLeatherboundBook.stages.0.description` | This holy text is bound in animal hide. Holding it feels wrong. | Цей священний текст оправлений у шкіру тварини. Тримати його в руках — неправильно. |
| `EB_GoodDebate.stages.0.label` | good debate | хороші дебати |
| `EB_GoodDebate.stages.0.description` | We had a stimulating debate about our beliefs. A meeting of open minds. | Ми провели жваві дебати про наші переконання. Зустріч відкритих розумів. |
| `EB_BadDebate.stages.0.label` | heretical debate | єретичні дебати |
| `EB_BadDebate.stages.0.description` | I had to refute their deviant beliefs. Diversity of thought is a poison. | Мені довелося спростовувати їхні єретичні переконання. Різноманітність думок — це отрута. |
| `EB_ProselytizerDebated.stages.0.label` | spread the word | поширив слово |
| `EB_ProselytizerDebated.stages.0.description` | I argued for the truth of my faith. Whatever the outcome, the attempt itself was a righteous act. | Я відстоював істину своєї віри. Яким би не був результат, сама спроба була праведним вчинком. |
| `EB_ProselytizerConverted.stages.0.label` | brought a soul to the faith | привів душу до віри |
| `EB_ProselytizerConverted.stages.0.description` | I brought someone into the light of our faith. There is no greater calling. | Я привів когось у світло нашої віри. Немає вищого покликання. |
| `EB_ProselytizerFailedConversion.stages.0.label` | failed to convert | не зміг навернути |
| `EB_ProselytizerFailedConversion.stages.0.description` | I tried to bring someone to the faith and failed. They remain in their ignorance. | Я намагався привести когось до віри і зазнав невдачі. Вони залишаються у своєму невіданні. |
| `EB_ApostacyDebated.stages.0.label` | faith challenged | віру оскаржили |
| `EB_ApostacyDebated.stages.0.description` | I had to defend the truth of my faith against a challenger. The very fact they dared question it is an insult. | Мені довелося захищати істину своєї віри від опонента. Сам факт того, що вони насмілилися поставити її під сумнів, — образа. |
| `EB_CognitiveDissonance.stages.0.label` | cognitive dissonance | когнітивний дисонанс |
| `EB_CognitiveDissonance.stages.0.description` | I shouldn't be participating in other ideoligion's practices. | Я не повинен брати участь у практиках інших ідеологій. |
| `EB_FaithReaffirmed.stages.0.label` | faith reaffirmed | віру підтверджено |
| `EB_FaithReaffirmed.stages.0.description` | Seeing another tradition's way of doing things reminds me why I value my own path. | Спостерігаючи за традиціями інших, я нагадую собі, чому ціную власний шлях. |
| `EB_LowCertaintyCoBeliever.stages.0.label` | wavering co-believer | нестійкий одновірець |
| `EB_LowCertaintyCoBeliever.stages.1.label` | faithless co-believer | невірний одновірець |
| `EB_LowCertaintyCoBeliever.stages.2.label` | apostate-hearted co-believer | відступник серцем одновірець |

## DefInjected/RecipeDef

| key | source | translation |
|-----|--------|-------------|
| `EB_WriteIdeobook.label` | write religious book | написати релігійну книгу |
| `EB_WriteIdeobook.description` | Write a religious book detailing your ideology's beliefs. Requires high certainty in one's ideoligion (at least 90%). | Написати релігійну книгу, що детально описує переконання вашої ідеології. Потрібна висока впевненість в ідеології (не менше 90%). |
| `EB_WriteIdeobook.jobString` | writing a religious book. | пише релігійну книгу. |
| `EB_WriteIllustratedIdeobook.label` | write illustrated religious book | написати ілюстровану релігійну книгу |
| `EB_WriteIllustratedIdeobook.description` | Write an illuminated religious book adorned with jems and inks. The quality of materials increases the devotional aspects of reading it. Requires high certainty in one's ideoligion (at least 90%). | Написати ілюміновану релігійну книгу, прикрашену коштовним камінням та чорнилом. Якість матеріалів підвищує духовний ефект читання. Потрібна висока впевненість в ідеології (не менше 90%). |
| `EB_WriteIllustratedIdeobook.jobString` | illustrating religious book. | ілюструє релігійну книгу. |

## DefInjected/RitualAttachableOutcomeEffectDef

| key | source | translation |
|-----|--------|-------------|
| `EB_BeliefReinforcement.label` | belief reinforcement | підкріплення переконань |
| `EB_BeliefReinforcement.effectDesc` | participants' convictions will be reinforced toward orthodoxy. | переконання учасників будуть підкріплені у напрямку ортодоксії. |
| `EB_BeliefReinforcement.letterInfoText` | The ritual deepened the participants' convictions. | Ритуал поглибив переконання учасників. |
