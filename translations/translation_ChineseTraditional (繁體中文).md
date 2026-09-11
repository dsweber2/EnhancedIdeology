<!-- EnhancedIdeology Translation Template: English → ChineseTraditional (繁體中文) -->
<!-- Author: claude -->
<!-- Pipes in text: use \| -->

## Keyed/Books.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.BookReadingBenefit` | {IDEO} - {1} conviction shift/qd | {IDEO} - {1} 信念變化/月 |
| `EnhancedIdeology.BookBeliefsHeader` | Beliefs advocated | 宣揚的信念 |
| `EnhancedIdeology.ShiftRatePerQuadrum` | {0}/qd | {0}/月 |
| `EnhancedIdeology.LetterReligiousBookDestroyedLabel` | Religious book destroyed | 宗教書籍已被摧毀 |
| `EnhancedIdeology.LetterReligiousBookDestroyedText` | {BOOK}, an important religious book for {IDEO}, has been destroyed. Followers of {IDEO} won't be happy about it. | {BOOK}，{IDEO}的重要宗教書籍，已遭摧毀。{IDEO}的信徒對此將深感不滿。 |
| `EnhancedIdeology.DestroyBookByBurningItUpsetIdeo` | Destroy {BOOK} by burning it. This will upset {IDEO_memberNamePlural} of {IDEO}. | 焚燒並摧毀{BOOK}。此舉將激怒{IDEO}的{IDEO_memberNamePlural}。 |
| `EnhancedIdeology.DestroyBookByBurningIt` | Destroy {BOOK} by burning it. | 焚燒並摧毀{BOOK}。 |
| `EnhancedIdeology.NoColonistCanDestroy` | No colonist can destroy {BOOK}. | 沒有殖民者能夠摧毀{BOOK}。 |
| `EnhancedIdeology.ConfirmDestroyBookByBurningItUpsetIdeo` | Are you sure you want to make {PAWN} burn {BOOK}? Doing so will greatly upset all {IDEO_memberNamePlural} of {IDEO}. | 確定要讓{PAWN}焚燒{BOOK}嗎？此舉將嚴重激怒{IDEO}所有的{IDEO_memberNamePlural}。 |
| `EnhancedIdeology.ConfirmDestroyBookByBurningIt` | Are you sure you want to make {PAWN} burn {BOOK}? | 確定要讓{PAWN}焚燒{BOOK}嗎？ |
| `EnhancedIdeology.BookBurningSuccess` | {PAWN} has destroyed {BOOK}. This has greatly upset {IDEO_memberNamePlural} of {IDEO}. | {PAWN}已摧毀{BOOK}。此舉嚴重激怒了{IDEO}的{IDEO_memberNamePlural}。 |

## Keyed/Contemplation.xml

| key | source | translation |
|-----|--------|-------------|
| `EB_NoPewToPrayIn` | {PAWN_nameDef} couldn't find a ritual seat to contemplate in for {IDEO_name}. | {PAWN_nameDef}找不到可供進行{IDEO_name}冥想的儀式座位。 |
| `EB_ContemplationRoomDisrespected` | {PAWN_nameDef}'s {IDEO_name} contemplation room is disrespected. | {PAWN_nameDef}的{IDEO_name}冥想室遭到褻瀆。 |
| `EB_ContemplationActivityReport` | contemplating (gain: {0}/hr) | 冥想中（獲益：{0}/小時） |
| `EB_ContemplationSiteStatLabel` | Contemplation gain | 冥想獲益 |
| `EB_ContemplationSiteStatReport` | Conviction reinforcement rate per hour during contemplation. Shown for the selected pawn, or as a base multiplier when no pawn is selected. Scales with room impressiveness and the pawn's current conviction strength. Additional +10% per fellow believer in the room. | 冥想期間每小時的信念強化速率。顯示所選棋子的數值，若未選擇棋子則顯示基礎倍率。依房間壯觀程度及棋子當前信念強度縮放。房間內每位同信徒額外加成+10%。 |
| `EB_ContemplationSiteInspect` | Contemplation gain: {0} | 冥想獲益：{0} |

## Keyed/Convert.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.Convert.CertaintyKnock` | Temporary certainty reduction: {0} | 暫時感悟降低：{0} |
| `EnhancedIdeology.Convert.SuccessChance` | Chance to win the debate: {0} | 贏得辯論的機率：{0} |
| `EnhancedIdeology.Convert.ConversionChance` | Chance to convert if you win: {0} | 贏得辯論後皈依的機率：{0} |
| `EnhancedIdeology.Convert.TargetBeliefs` | Target beliefs (a random 1-{0} of): | 目標信念（隨機1至{0}項）： |
| `EnhancedIdeology.Convert.NoOpposition` | They oppose nothing your faith preaches - there is nothing to convert. | 他們對你信仰所宣揚的一切均無異議——無可轉化之處。 |
| `EnhancedIdeology.Convert.Factors` | Factors (you vs. them): | 影響因素（你 vs. 他們）： |
| `EnhancedIdeology.Convert.Factor.ConversionPower` | Conversion power: {0} vs {1} | 皈依力量：{0} vs {1} |
| `EnhancedIdeology.Convert.Factor.Intellectual` | Intellectual debate: {0} vs {1} | 智識辯論：{0} vs {1} |
| `EnhancedIdeology.Convert.Factor.Social` | Social impact: {0} vs {1} | 社交影響：{0} vs {1} |

## Keyed/Interaction_IdeologicalDebate.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.IdeologicalDebateOutcomeSocialFight` | {PAWN1_labelShort} tried to debate ideological views with {PAWN2_labelShort}. This led to a social fight! | {PAWN1_labelShort}試圖與{PAWN2_labelShort}辯論理念觀點，此舉引發了社交衝突！ |
| `EnhancedIdeology.LetterLabelIdeologicalDebateConversion` | Ideological conversion | 理念皈依 |
| `EnhancedIdeology.LetterIdeologicalDebateConversionText` | After debating {ISSUE_label}, {CONVINCER_labelShort} convinced {CONVINCED_labelShort} to abandon {OLDIDEO_name} and embrace {NEWIDEO_name}. | 在辯論{ISSUE_label}後，{CONVINCER_labelShort}說服{CONVINCED_labelShort}放棄{OLDIDEO_name}，轉而信奉{NEWIDEO_name}。 |

## Keyed/Reassure.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.Reassure.CertaintyGain` | Certainty gain on win: +{0} | 勝利後感悟提升：+{0} |
| `EnhancedIdeology.Reassure.SuccessChance` | Chance to win the debate: {0} | 贏得辯論的機率：{0} |
| `EnhancedIdeology.Reassure.SelfTarget` | Self-reassurance: guaranteed success | 自我安撫：必定成功 |
| `EnhancedIdeology.Reassure.TargetBeliefs` | Beliefs to reinforce (a random 1-{0} of): | 待強化的信念（隨機1至{0}項）： |
| `EnhancedIdeology.Reassure.NoHeterodoxy` | Their beliefs are already orthodox - there is nothing to reinforce. | 其信念已為正統——無需強化。 |
| `EnhancedIdeology.Reassure.FailMessage` | {0} failed to reassure {1}. | {0}未能安撫{1}。 |

## Keyed/Settings.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.TranslationCredit` | Translation: dsweber2 | 翻譯：claude |
| `EnhancedIdeology.Section.Ideoligion` | Ideology | 理念 |
| `EnhancedIdeology.Section.Precept` | Precept | 戒律 |
| `EnhancedIdeology.Section.Compat` | Compatibility | 相容性 |
| `EnhancedIdeology.Section.Debug` | Debug | 除錯 |
| `EnhancedIdeology.SubSection.Belief` | Personal Certainty dynamics | 個人感悟動態 |
| `EnhancedIdeology.SubSection.Conversion` | Conversion effects | 皈依效果 |
| `EnhancedIdeology.CertaintyDriftRate` | Certainty drift rate: {0}/day | 感悟漂移速率：{0}/天 |
| `EnhancedIdeology.CertaintyDriftRate.Tip` | How fast a pawn's certainty moves toward its target each day. Higher = faith reacts faster to circumstances. Makes for more dynamic beliefs. | 棋子感悟每天朝目標移動的速度。數值越高，信仰對環境反應越快，信念也越動態。 |
| `EnhancedIdeology.RelationalMaxRange` | Relational influence: ±{0} | 人際影響：±{0} |
| `EnhancedIdeology.RelationalMaxRange.Tip` | The most that a pawn's opinion of their co-religionists can raise or lower their target certainty. | 棋子對同信徒的看法能夠提升或降低目標感悟的最大幅度。 |
| `EnhancedIdeology.PracticeMaxRange` | Practice influence: ±{0} | 實踐影響：±{0} |
| `EnhancedIdeology.PracticeMaxRange.Tip` | The most that precept moods (from rituals, veneration, taboos, etc) can raise or lower target certainty. | 戒律情緒（源自儀式、崇敬、禁忌等）能夠提升或降低目標感悟的最大幅度。 |
| `EnhancedIdeology.CrisisThreshold` | Crisis of faith threshold: {0} | 信仰危機閾值：{0} |
| `EnhancedIdeology.CrisisThreshold.Tip` | When a pawn's certainty falls below this, they may have a crisis-of-faith breakdown. Higher = more crises. | 當棋子感悟低於此值時，可能爆發信仰危機崩潰。數值越高，危機越頻繁。 |
| `EnhancedIdeology.ConversionPace` | Conversion pace: {0} | 皈依速度：{0} |
| `EnhancedIdeology.ConversionPace.Tip` | How quickly pawns abandon a poorly-fitting faith on their own, relative to default. Higher = spontaneous conversions are more likely. | 棋子自發放棄不合適信仰的速度，相對於預設值。數值越高，自發皈依越可能發生。 |
| `EnhancedIdeology.ConversionCertaintyKnock` | Conversion attempt Certainty loss: {0} | 皈依嘗試感悟損失：{0} |
| `EnhancedIdeology.ConversionCertaintyKnock.Tip` | When a moral guide wins a conversion debate but doesn't immediately flip the pawn, their certainty is multiplied by this. 1x = an 80%-certain pawn stays at 80%; 0.8x = an 80%-certain pawn drops to 64%. Lower = conversion attempts more impactful. | 當道德引導者贏得皈依辯論但棋子未立即轉變時，其感悟乘以此係數。1倍＝感悟80%的棋子維持80%；0.8倍＝感悟80%的棋子降至64%。數值越低，皈依嘗試影響越大。 |
| `EnhancedIdeology.ConvictionDecayRate` | Conviction decay: {0}/quadrum | 信念衰退：{0}/月 |
| `EnhancedIdeology.ConvictionDecayRate.Tip` | How much conviction strength all precepts lose per quadrum. Note that 100% conviction is at a precept average of 20, max is 50. Higher = stronger need for reinforcing beliefs. | 所有戒律每月損失的信念強度。注意：平均戒律值20對應100%信念，最大值為50。數值越高，強化信念的需求越迫切。 |
| `EnhancedIdeology.PreceptOppositionScale` | Opposite-stance opposition: {0} | 對立立場反對度：{0} |
| `EnhancedIdeology.PreceptOppositionScale.Tip` | How strongly a pawn opposes the stance at the opposite extreme of an issue, as a fraction of their conviction. At 100%, a pawn with Cannibalism: Disapproved 12 has opinion -12 of Cannibalism: Required (ravenous). 0% = indifferent to opposing stances; 100% = equal and opposite to their support for their own at the furthest extreme precept. | 棋子反對某議題極端對立立場的強度，以其信念的分數表示。100%時，持「食人：不贊同12」的棋子對「食人：必須（飢渴）」的看法為-12。0%＝對對立立場漠然；100%＝與自身在最極端戒律的支持等量反對。 |
| `EnhancedIdeology.ConversionStancePull` | Conversion strength: {0} | 皈依強度：{0} |
| `EnhancedIdeology.ConversionStancePull.Tip` | How much harder a successful conversion attempt (a moral guide's directed action) pushes a pawn's beliefs than an ordinary conversion attempt. Higher = faster conversions. | 成功的皈依嘗試（道德引導者的定向行動）相較於普通皈依嘗試，更有力地推動棋子信念的程度。數值越高，皈依越迅速。 |
| `EnhancedIdeology.DebateConvictionChange` | Debate conviction change: {0} | 辯論信念變化：{0} |
| `EnhancedIdeology.DebateConvictionChange.Tip` | How far a single won argument moves belief on the issue, relative to default. Shifts both the precept and the conviction in that precept simultaneously. Higher = each won debate persuades more. Leads to faster conversions. | 單次勝利的辯論在議題上推動信念的幅度，相對於預設值。同時改變戒律及對該戒律的信念。數值越高，每次勝利辯論的說服力越強，皈依也越迅速。 |
| `EnhancedIdeology.ConversionOpinionMultiplier` | Friendship bonus (conversion): {0} | 友誼加成（皈依）：{0} |
| `EnhancedIdeology.ConversionOpinionMultiplier.Tip` | How much a pawn's opinion of someone trying to change their precept amplifies the effect. At 1x and opinion +100, both the certainty knock and the belief pull are doubled. At 0x, opinion has no effect. Not used when Peer Pressure is installed — that mod's own multiplier setting takes over instead. | 棋子對試圖改變其戒律者的看法放大效果的程度。1倍且看法+100時，感悟衝擊與信念牽引均翻倍。0倍時，看法無任何效果。安裝「同伴壓力」模組時此設定無效——改由該模組自身的倍率設定接管。 |
| `EnhancedIdeology.SaveCompatMinCertainty` | Save-load minimum certainty: {0} | 存檔載入最低感悟：{0} |
| `EnhancedIdeology.SaveCompatMinCertainty.Tip` | When loading a vanilla or pre-mod save, pawns whose certainty is below this have their beliefs seeded to at least this target instead of zero. Prevents a pawn with 0% certainty from being left with no convictions whatsoever. | 載入原版或模組前的存檔時，感悟低於此值的棋子，其信念目標至少設為此值而非零。防止感悟0%的棋子完全失去所有信念。 |
| `EnhancedIdeology.DebugInteractionWorkers` | DEV: Debug Interaction Workers | 開發：除錯互動工作器 |
| `EnhancedIdeology.DebugInteractionWorkers.Tip` | Enables debug logging for interaction workers when dev mode is active. I suggest you leave this off unless debugging Enhanced Ideology specifically. | 在開發者模式啟用時，為互動工作器啟用除錯紀錄。建議除非專門除錯「Enhanced Ideology」，否則保持關閉。 |

## Keyed/TabOpinion.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.TabOpinion` | Ideology | 理念 |
| `EnhancedIdeology.IdeologyOpinions` | Ideology Opinions | 理念看法 |
| `EnhancedIdeology.BeliefsHeader` | Issues | 議題 |
| `EnhancedIdeology.ColIssue` | Issue | 議題 |
| `EnhancedIdeology.ColStance` | Stance | 立場 |
| `EnhancedIdeology.ColStrength` | Strength | 強度 |
| `EnhancedIdeology.StanceDontCare` | Don't care | 無所謂 |
| `EnhancedIdeology.StanceTooltip` | On the issue of {1}, {PAWN_nameDef} has the stance {2} with conviction {3}. {IDEO_name} preaches {5}; {PAWN_nameDef}'s opinion of that stance: {6}. | 在{1}的議題上，{PAWN_nameDef}持有立場{2}，信念強度為{3}。{IDEO_name}宣揚{5}；{PAWN_nameDef}對該立場的看法：{6}。 |
| `EnhancedIdeology.PawnOpinionTooltip` | {PAWN_nameDef}'s opinion of {IDEO_name}: {2} | {PAWN_nameDef}對{IDEO_name}的看法：{2} |
| `EnhancedIdeology.PawnOptionToolTip.FromMemesAndPrecepts` | From memes and precepts: {0} | 源自模因與戒律：{0} |
| `EnhancedIdeology.PawnOptionToolTip.FromPersonalBeliefs` | From personal beliefs: {0} | 源自個人信念：{0} |
| `EnhancedIdeology.PawnOptionToolTip.FromInterpersonalRelationships` | From interpersonal relationships: {0} | 源自人際關係：{0} |

## Keyed/TabSocial.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.PawnCertaintyTooltip` | {PAWN_nameDef}'s certainty in {IDEO_name}: {2} | {PAWN_nameDef}對{IDEO_name}的感悟：{2} |
| `EnhancedIdeology.CertaintyTarget` | Target certainty: {0} | 目標感悟：{0} |
| `EnhancedIdeology.CertainChangePerDay` | Drifting toward it at {0}/day | 每天以{0}的速率漂移趨近 |
| `EnhancedIdeology.CertaintyBandStructural` | Structural (beliefs & nature): {0} | 結構性（信念與天性）：{0} |
| `EnhancedIdeology.CertaintyBandRelational` | Congregation: {0} | 會眾：{0} |
| `EnhancedIdeology.CertaintyBandPractice` | Practice (recent rites): {0} | 實踐（近期禮儀）：{0} |
| `EnhancedIdeology.CertaintyPreceptDecay` | Precept decay: {0}/quadrum | 戒律衰退：{0}/月 |
| `EnhancedIdeology.CouplingPenalty` | Opposed practices | 對立行為 |
| `EnhancedIdeology.DietGeneAntiMeat` | Obligate herbivore | 強制草食者 |
| `EnhancedIdeology.DietGeneProMeat` | Obligate carnivore | 強制肉食者 |
| `EnhancedIdeology.XenotypePreferred` | Preferred xenotype | 偏好外基因型 |
| `EnhancedIdeology.XenotypeDisapproved` | Disapproved xenotype | 不贊同外基因型 |

## Keyed/Work_CompleteReligiousBook.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.IdeologyMissingRole` | Ideoligion does not have the required '{ROLE}' role. | 理念缺少所需的「{ROLE}」角色。 |
| `EnhancedIdeology.PawnMissingRequiredRole` | {PAWN} does not have the required '{ROLE}' role. | {PAWN}不具備所需的「{ROLE}」角色。 |
| `EnhancedIdeology.PawnIsNotAuthor` | {PAWN} is not the author of the book. | {PAWN}並非此書的作者。 |
| `EnhancedIdeology.BookIsNotForPawnIdeoligion` | The book is not for {PAWN_possessive} ideology. | 此書並非為{PAWN_possessive}的理念所寫。 |
| `EnhancedIdeology.NoFreeValidLecternFound` | No available, accessible lectern found to do work on. | 找不到可用且可進入的講台來進行工作。 |
| `EnhancedIdeology.InsufficientCertaintyToWrite` | Certainty must be above 90% to write religious texts. | 撰寫宗教文本需要感悟高於90%。 |

## DefInjected/ThingDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Ideobook.label` | religious book | 宗教書籍 |
| `EB_Ideobook.description` | A book containing fictional or true stories for the pleasure and edification of the reader. | 一本包含虛構或真實故事的書籍，供讀者愉悅身心並獲得啟迪。 |
| `EB_UnfinishedDeskIdeobook.label` | unfinished religious book | 未完成的宗教書籍 |
| `EB_UnfinishedDeskIdeobook.description` | An unfinished religious book being written at the writing desk. | 一本正在書桌上撰寫中的未完成宗教書籍。 |
| `EB_WritingDesk.label` | writing desk | 書桌 |
| `EB_WritingDesk.description` | A desk for composing ideological texts. Only colonists with deep conviction in their beliefs can write at it. | 用於撰寫理念文本的書桌。只有對信仰有深厚信念的殖民者才能在此寫作。 |

## DefInjected/HistoryEventDef

| key | source | translation |
|-----|--------|-------------|
| `EB_DestroyedReligiousBook.label` | destroyed religious book | 摧毀宗教書籍 |
| `EB_BookDestroyed.label` | religious book was destroyed | 宗教書籍遭到摧毀 |
| `EB_Contemplated.label` | contemplated | 進行冥想 |

## DefInjected/HediffDef

| key | source | translation |
|-----|--------|-------------|
| `EB_BrainwipeRecovery.label` | belief reformation | 信念重塑 |
| `EB_BrainwipeRecovery.description` | This person's beliefs were shattered by a psychic brainwipe. Their convictions are weak and unfocused, leaving them unusually open to ideological influence. | 此人的信念被心靈洗腦術摧毀。其信念薄弱且渙散，使其對理念影響異常敏感。 |

## DefInjected/InspirationDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ReligiousEnlightenment.label` | religious enlightenment | 宗教啟示 |
| `EB_ReligiousEnlightenment.beginLetter` | [PAWN_pronoun] feels a moment of divine clarity and is compelled to write a religious text. | [PAWN_pronoun]感受到一瞬神聖的清明，深受驅使要撰寫一篇宗教文本。 |
| `EB_ReligiousEnlightenment.baseInspectLine` | Inspired: Religious enlightenment | 受到啟發：宗教啟示 |
| `EB_ReligiousEnlightenment.endMessage` | [PAWN_nameDef]'s moment of religious enlightenment has passed. | [PAWN_nameDef]的宗教啟示時刻已然消逝。 |

## DefInjected/InteractionDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ConversionLog.label` | conversion | 皈依 |
| `EB_ConversionLog.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] converted to a new faith. | r_logentry->[INITIATOR_nameDef]皈依了新的信仰。 |
| `EB_CrisisOfFaithLog.label` | crisis of faith | 信仰危機 |
| `EB_CrisisOfFaithLog.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] experienced a crisis of faith. | r_logentry->[INITIATOR_nameDef]經歷了信仰危機。 |
| `EB_IdeologicalDebateMeme.label` | ideological debate over memes | 關於模因的理念辯論 |
| `EB_IdeologicalDebateMeme.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] and [RECIPIENT_nameDef] debated memes. | r_logentry->[INITIATOR_nameDef]與[RECIPIENT_nameDef]就模因展開辯論。 |
| `EB_IdeologicalDebatePrecept.label` | ideological debate over precepts | 關於戒律的理念辯論 |
| `EB_IdeologicalDebatePrecept.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] and [RECIPIENT_nameDef] debated beliefs. | r_logentry->[INITIATOR_nameDef]與[RECIPIENT_nameDef]就信念展開辯論。 |

## DefInjected/RulePackDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Sentence_DebateWon.rulePack.rulesStrings.0` | sent->[INITIATOR_nameDef] won the debate. | sent->[INITIATOR_nameDef]贏得了辯論。 |
| `EB_Sentence_InitiatorWon.rulePack.rulesStrings.0` | sent->[INITIATOR_nameDef] proved more persuasive. | sent->[INITIATOR_nameDef]更具說服力。 |
| `EB_Sentence_RecipientWon.rulePack.rulesStrings.0` | sent->[RECIPIENT_nameDef] proved more persuasive. | sent->[RECIPIENT_nameDef]更具說服力。 |
| `EB_Sentence_DebateDraw.rulePack.rulesStrings.0` | sent->Neither changed their view. | sent->雙方均未改變立場。 |

## DefInjected/JobDef

| key | source | translation |
|-----|--------|-------------|
| `EB_PlaceAndBurnUntilDestroyed.reportString` | placing TargetA on the ground to burn. | 將TargetA放置於地面焚燒。 |
| `EB_Pray.reportString` | contemplating. | 冥想中。 |

## DefInjected/WorkGiverDef

| key | source | translation |
|-----|--------|-------------|
| `EB_WriteIdeobookAtDesk.label` | write religious books | 撰寫宗教書籍 |
| `EB_WriteIdeobookAtDesk.verb` | write | 撰寫 |
| `EB_WriteIdeobookAtDesk.gerund` | writing religious books | 撰寫宗教書籍 |

## DefInjected/MentalStateDef

| key | source | translation |
|-----|--------|-------------|
| `EB_CrisisOfFaith.label` | crisis of faith | 信仰危機 |
| `EB_CrisisOfFaith.beginLetterLabel` | crisis of faith | 信仰危機 |
| `EB_CrisisOfFaith.beginLetter` | {0} is having a crisis of faith.\n\n[PAWN_pronoun] will spend the next day or two in contemplation, questioning [PAWN_possessive] beliefs. | {0}正在經歷信仰危機。\n\n[PAWN_pronoun]將在接下來一兩天內沉浸於冥想，質疑[PAWN_possessive]的信念。 |
| `EB_CrisisOfFaith.recoveryMessage` | {0}'s crisis of faith has passed. | {0}的信仰危機已然平息。 |
| `EB_CrisisOfFaith.baseInspectLine` | Mental state: Crisis of faith | 精神狀態：信仰危機 |
| `EB_Iconoclast.label` | iconoclast | 毀像者 |
| `EB_Iconoclast.beginLetter` | {0} had a mental break and is being an iconoclast.\n\n[PAWN_pronoun] is going try and burn religious books. | {0}精神崩潰，化身毀像者。\n\n[PAWN_pronoun]將嘗試焚燒宗教書籍。 |
| `EB_Iconoclast.recoveryMessage` | {0} is no longer being an iconoclast. | {0}不再是毀像者了。 |
| `EB_Iconoclast.baseInspectLine` | Mental state: Iconoclast | 精神狀態：毀像者 |

## DefInjected/IssueDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Contemplation.label` | contemplation | 冥想 |

## DefInjected/NeedDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Contemplation.label` | contemplation | 冥想 |
| `EB_Contemplation.description` | This pawn's ideology requires regular contemplation. Neglecting it erodes their sense of devotion. | 此棋子的理念要求定期進行冥想。忽視冥想將逐漸侵蝕其虔誠感。 |

## DefInjected/PreceptDef

| key | source | translation |
|-----|--------|-------------|
| `Contemplation_Required.label` | required | 必須 |
| `Contemplation_Required.description` | Contemplation is a sacred obligation. Members must contemplate regularly, and the moral guide is expected to lead by example. | 冥想是神聖的義務。成員必須定期冥想，而道德引導者更應以身作則。 |
| `Contemplation_Respected.label` | respected | 受尊重 |
| `Contemplation_Respected.description` | Contemplation is a meaningful practice, and those who contemplate regularly are held in higher regard. | 冥想是一種有意義的修行，定期冥想者備受推崇。 |
| `Contemplation_Normal.label` | acceptable | 可接受 |
| `Contemplation_Normal.description` | Contemplation is a personal matter — neither encouraged nor discouraged. | 冥想是個人事務——不加鼓勵，也不加阻止。 |
| `Contemplation_Disapproved.label` | disapproved | 不贊同 |
| `Contemplation_Disapproved.description` | Contemplation is seen as weakness or superstition. Members who contemplate are viewed unfavorably. | 冥想被視為軟弱或迷信。進行冥想的成員將受到負面看待。 |
| `Contemplation_Forbidden.label` | forbidden | 禁止 |
| `Contemplation_Forbidden.description` | Contemplation is strictly forbidden. Any member caught contemplating will be shunned. | 冥想被嚴格禁止。任何被發現冥想的成員將遭到排斥。 |

## DefInjected/ThoughtDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ContemplationNeed.stages.0.label` | contemplated regularly | 定期進行冥想 |
| `EB_ContemplationNeed.stages.0.description` | I've been faithful in my contemplations. It brings me peace. | 我一直忠實地進行冥想，這給我帶來了平靜。 |
| `EB_ContemplationNeed.stages.1.label` | contemplation neglected | 冥想被忽視 |
| `EB_ContemplationNeed.stages.1.description` | I haven't found time to contemplate. My devotion is slipping. | 我一直找不到時間冥想，我的虔誠正在消退。 |
| `EB_ContemplationNeed.stages.2.label` | contemplation severely neglected | 冥想嚴重被忽視 |
| `EB_ContemplationNeed.stages.2.description` | I've gone far too long without contemplation. I feel spiritually adrift. | 我已太久沒有冥想了，我感到精神上漂泊無依。 |
| `EB_WitnessedContemplation_Approved.stages.0.label` | contemplated devoutly | 虔誠地冥想 |
| `EB_WitnessedContemplation_Disapproved.stages.0.label` | engaged in contemplation | 進行了冥想 |
| `EB_WitnessedContemplation_Forbidden.stages.0.label` | defied the ban on contemplation | 無視冥想禁令 |
| `EB_ReligiousBookDestroyed.stages.0.label` | religious book destroyed | 宗教書籍遭到摧毀 |
| `EB_ReligiousBookDestroyed.stages.0.description` | Our religious book was destroyed. That is absolutely inexcusable. | 我們的宗教書籍遭到摧毀，這絕對不可原諒。 |
| `EB_WroteSacrilegousBinding.stages.0.label` | defiled a sacred text | 褻瀆了神聖文本 |
| `EB_WroteSacrilegousBinding.stages.0.description` | I bound a holy book in animal hide. That was wrong. | 我用獸皮裝訂了一本聖書，那是錯誤的。 |
| `EB_ReadingLeatherboundBook.stages.0.label` | reading leather-bound scripture | 閱讀皮革裝訂的經文 |
| `EB_ReadingLeatherboundBook.stages.0.description` | This holy text is bound in animal hide. Holding it feels wrong. | 這部聖典以獸皮裝訂，拿在手中感覺有違道義。 |
| `EB_GoodDebate.stages.0.label` | good debate | 良好的辯論 |
| `EB_GoodDebate.stages.0.description` | We had a stimulating debate about our beliefs. A meeting of open minds. | 我們就信念進行了一場精彩刺激的辯論，是開放心靈的相遇。 |
| `EB_BadDebate.stages.0.label` | heretical debate | 異端辯論 |
| `EB_BadDebate.stages.0.description` | I had to refute their deviant beliefs. Diversity of thought is a poison. | 我不得不駁斥他們偏離正道的信念。思想多樣性是一種毒素。 |
| `EB_ProselytizerDebated.stages.0.label` | spread the word | 傳播信仰 |
| `EB_ProselytizerDebated.stages.0.description` | I argued for the truth of my faith. Whatever the outcome, the attempt itself was a righteous act. | 我為信仰的真理而辯，無論結果如何，這份嘗試本身就是正義之舉。 |
| `EB_ProselytizerConverted.stages.0.label` | brought a soul to the faith | 引領靈魂皈依 |
| `EB_ProselytizerConverted.stages.0.description` | I brought someone into the light of our faith. There is no greater calling. | 我將某人帶入我們信仰的光明之中，沒有比這更崇高的召喚了。 |
| `EB_ProselytizerFailedConversion.stages.0.label` | failed to convert | 皈依失敗 |
| `EB_ProselytizerFailedConversion.stages.0.description` | I tried to bring someone to the faith and failed. They remain in their ignorance. | 我嘗試引領某人皈依卻失敗了，他們仍沉浸在無知之中。 |
| `EB_ApostacyDebated.stages.0.label` | faith challenged | 信仰受到挑戰 |
| `EB_ApostacyDebated.stages.0.description` | I had to defend the truth of my faith against a challenger. The very fact they dared question it is an insult. | 我不得不抵禦挑戰者，捍衛信仰的真理。他們竟敢質疑，這本身就是一種侮辱。 |
| `EB_CognitiveDissonance.stages.0.label` | cognitive dissonance | 認知失調 |
| `EB_CognitiveDissonance.stages.0.description` | I shouldn't be participating in other ideoligion's practices. | 我不應該參與其他理念的活動。 |
| `EB_FaithReaffirmed.stages.0.label` | faith reaffirmed | 信仰得到肯定 |
| `EB_FaithReaffirmed.stages.0.description` | Seeing another tradition's way of doing things reminds me why I value my own path. | 看到另一傳統的做事方式，提醒了我為何珍視自己的道路。 |
| `EB_LowCertaintyCoBeliever.stages.0.label` | wavering co-believer | 動搖的同信徒 |
| `EB_LowCertaintyCoBeliever.stages.1.label` | faithless co-believer | 失去信仰的同信徒 |
| `EB_LowCertaintyCoBeliever.stages.2.label` | apostate-hearted co-believer | 心懷叛道的同信徒 |

## DefInjected/RecipeDef

| key | source | translation |
|-----|--------|-------------|
| `EB_WriteIdeobook.label` | write religious book | 撰寫宗教書籍 |
| `EB_WriteIdeobook.description` | Write a religious book detailing your ideology's beliefs. Requires high certainty in one's ideoligion (at least 90%). | 撰寫一本詳述理念信仰的宗教書籍。需要對自身理念有高度感悟（至少90%）。 |
| `EB_WriteIdeobook.jobString` | writing a religious book. | 正在撰寫宗教書籍。 |
| `EB_WriteIllustratedIdeobook.label` | write illustrated religious book | 撰寫插圖宗教書籍 |
| `EB_WriteIllustratedIdeobook.description` | Write an illuminated religious book adorned with jems and inks. The quality of materials increases the devotional aspects of reading it. Requires high certainty in one's ideoligion (at least 90%). | 撰寫一本以寶石和墨水裝飾的彩飾宗教書籍。材料品質越高，閱讀時的虔誠效果越好。需要對自身理念有高度感悟（至少90%）。 |
| `EB_WriteIllustratedIdeobook.jobString` | illustrating religious book. | 正在為宗教書籍繪製插圖。 |

## DefInjected/RitualAttachableOutcomeEffectDef

| key | source | translation |
|-----|--------|-------------|
| `EB_BeliefReinforcement.label` | belief reinforcement | 信念強化 |
| `EB_BeliefReinforcement.effectDesc` | participants' convictions will be reinforced toward orthodoxy. | 參與者的信念將朝正統方向強化。 |
| `EB_BeliefReinforcement.letterInfoText` | The ritual deepened the participants' convictions. | 儀式加深了參與者的信念。 |
