<!-- EnhancedIdeology Translation Template: English → Japanese (日本語) -->
<!-- Author: claude -->
<!-- Pipes in text: use \| -->

## Keyed/Books.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.BookReadingBenefit` | {IDEO} - {1} conviction shift/qd | {IDEO} - 確信変化 {1}/節 |
| `EnhancedIdeology.BookBeliefsHeader` | Beliefs advocated | 提唱する信条 |
| `EnhancedIdeology.ShiftRatePerQuadrum` | {0}/qd | {0}/節 |
| `EnhancedIdeology.LetterReligiousBookDestroyedLabel` | Religious book destroyed | 聖典が破壊された |
| `EnhancedIdeology.LetterReligiousBookDestroyedText` | {BOOK}, an important religious book for {IDEO}, has been destroyed. Followers of {IDEO} won't be happy about it. | {IDEO}にとって重要な聖典{BOOK}が破壊された。{IDEO}の信者はこれを喜ばないだろう。 |
| `EnhancedIdeology.DestroyBookByBurningItUpsetIdeo` | Destroy {BOOK} by burning it. This will upset {IDEO_memberNamePlural} of {IDEO}. | {BOOK}を焼いて破壊する。{IDEO}の{IDEO_memberNamePlural}を怒らせることになる。 |
| `EnhancedIdeology.DestroyBookByBurningIt` | Destroy {BOOK} by burning it. | {BOOK}を焼いて破壊する。 |
| `EnhancedIdeology.NoColonistCanDestroy` | No colonist can destroy {BOOK}. | {BOOK}を破壊できる入植者がいない。 |
| `EnhancedIdeology.ConfirmDestroyBookByBurningItUpsetIdeo` | Are you sure you want to make {PAWN} burn {BOOK}? Doing so will greatly upset all {IDEO_memberNamePlural} of {IDEO}. | {PAWN}に{BOOK}を焼かせますか？これにより{IDEO}の{IDEO_memberNamePlural}全員が大きく傷つくことになります。 |
| `EnhancedIdeology.ConfirmDestroyBookByBurningIt` | Are you sure you want to make {PAWN} burn {BOOK}? | {PAWN}に{BOOK}を焼かせますか？ |
| `EnhancedIdeology.BookBurningSuccess` | {PAWN} has destroyed {BOOK}. This has greatly upset {IDEO_memberNamePlural} of {IDEO}. | {PAWN}は{BOOK}を破壊した。{IDEO}の{IDEO_memberNamePlural}は大きく傷ついた。 |

## Keyed/Contemplation.xml

| key | source | translation |
|-----|--------|-------------|
| `EB_NoPewToPrayIn` | {PAWN_nameDef} couldn't find a ritual seat to contemplate in for {IDEO_name}. | {PAWN_nameDef}は{IDEO_name}のための瞑想用の儀式席を見つけられなかった。 |
| `EB_ContemplationRoomDisrespected` | {PAWN_nameDef}'s {IDEO_name} contemplation room is disrespected. | {PAWN_nameDef}の{IDEO_name}瞑想室が冒涜されている。 |
| `EB_ContemplationActivityReport` | contemplating (gain: {0}/hr) | 瞑想中（獲得: {0}/時） |
| `EB_ContemplationSiteStatLabel` | Contemplation gain | 瞑想の獲得量 |
| `EB_ContemplationSiteStatReport` | Conviction reinforcement rate per hour during contemplation. Shown for the selected pawn, or as a base multiplier when no pawn is selected. Scales with room impressiveness and the pawn's current conviction strength. Additional +10% per fellow believer in the room. | 瞑想中の確信強化率（毎時）。選択中のポーンに対して表示、ポーン未選択時は基本倍率として表示。部屋の印象と現在の確信強度に応じてスケーリング。同じ部屋にいる同信者1人につき追加+10%。 |
| `EB_ContemplationSiteInspect` | Contemplation gain: {0} | 瞑想獲得量: {0} |

## Keyed/Convert.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.Convert.CertaintyKnock` | Temporary certainty reduction: {0} | 一時的な信仰度の低下: {0} |
| `EnhancedIdeology.Convert.SuccessChance` | Chance to win the debate: {0} | 議論に勝つ確率: {0} |
| `EnhancedIdeology.Convert.ConversionChance` | Chance to convert if you win: {0} | 勝った場合の改宗確率: {0} |
| `EnhancedIdeology.Convert.TargetBeliefs` | Target beliefs (a random 1-{0} of): | 対象の信条（無作為に1〜{0}件）: |
| `EnhancedIdeology.Convert.NoOpposition` | They oppose nothing your faith preaches - there is nothing to convert. | 彼らはあなたの信仰が説くものに反対していない——改宗すべきものがない。 |
| `EnhancedIdeology.Convert.Factors` | Factors (you vs. them): | 要因（あなた対相手）: |
| `EnhancedIdeology.Convert.Factor.ConversionPower` | Conversion power: {0} vs {1} | 改宗力: {0} 対 {1} |
| `EnhancedIdeology.Convert.Factor.Intellectual` | Intellectual debate: {0} vs {1} | 知的議論: {0} 対 {1} |
| `EnhancedIdeology.Convert.Factor.Social` | Social impact: {0} vs {1} | 社会的影響: {0} 対 {1} |

## Keyed/Interaction_IdeologicalDebate.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.IdeologicalDebateOutcomeSocialFight` | {PAWN1_labelShort} tried to debate ideological views with {PAWN2_labelShort}. This led to a social fight! | {PAWN1_labelShort}は{PAWN2_labelShort}と思想的な議論をしようとした。これにより口論になった！ |
| `EnhancedIdeology.LetterLabelIdeologicalDebateConversion` | Ideological conversion | 思想的改宗 |
| `EnhancedIdeology.LetterIdeologicalDebateConversionText` | After debating {ISSUE_label}, {CONVINCER_labelShort} convinced {CONVINCED_labelShort} to abandon {OLDIDEO_name} and embrace {NEWIDEO_name}. | {ISSUE_label}について議論した末、{CONVINCER_labelShort}は{CONVINCED_labelShort}を説得し、{OLDIDEO_name}を捨てて{NEWIDEO_name}を受け入れさせた。 |
| `EnhancedIdeology.DebateLog.AboutMeme` | [INITIATOR_nameDef] debated [RECIPIENT_nameDef] about the meme [TOPIC_label]. | [INITIATOR_nameDef]はミーム[TOPIC_label]について[RECIPIENT_nameDef]と議論した。 |
| `EnhancedIdeology.DebateLog.AboutPrecept` | [INITIATOR_nameDef] debated [TOPIC_label] with [RECIPIENT_nameDef]. | [INITIATOR_nameDef]は[TOPIC_label]について[RECIPIENT_nameDef]と議論した。 |
| `EnhancedIdeology.DebateLog.Generic` | [INITIATOR_nameDef] debated with [RECIPIENT_nameDef]. | [INITIATOR_nameDef]は[RECIPIENT_nameDef]と議論した。 |
| `EnhancedIdeology.DebateSent.InitiatorMoved` | [INITIATOR_nameDef] moved [RECIPIENT_nameDef] towards stance "[WINNING_STANCE_label]". | [INITIATOR_nameDef]は[RECIPIENT_nameDef]を「[WINNING_STANCE_label]」の立場へと近づけた。 |
| `EnhancedIdeology.DebateSent.RecipientMoved` | [RECIPIENT_nameDef] moved [INITIATOR_nameDef] towards stance "[WINNING_STANCE_label]". | [RECIPIENT_nameDef]は[INITIATOR_nameDef]を「[WINNING_STANCE_label]」の立場へと近づけた。 |
| `EnhancedIdeology.DebateSent.WinnerPersuasive` | [WINNER_nameDef] proved more persuasive. | [WINNER_nameDef]の方がより説得力があった。 |
| `EnhancedIdeology.DebateSent.Draw` | Neither changed their view. | どちらも考えを変えなかった。 |
| `EnhancedIdeology.JobReport_Debating` | Debating {0} | {0}と議論中 |
| `EnhancedIdeology.CrisisLog.Wander` | [INITIATOR_nameDef] experienced a crisis of faith. | [INITIATOR_nameDef]は信仰の危機を経験した。 |
| `EnhancedIdeology.CrisisLog.MoodBreak` | [INITIATOR_nameDef]'s crisis of faith compounded [INITIATOR_possessive] misery. | [INITIATOR_nameDef]の信仰の危機が[INITIATOR_possessive]苦悩をさらに深めた。 |

## Keyed/Reassure.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.Reassure.CertaintyGain` | Certainty gain on win: +{0} | 勝利時の信仰度上昇: +{0} |
| `EnhancedIdeology.Reassure.SuccessChance` | Chance to win the debate: {0} | 議論に勝つ確率: {0} |
| `EnhancedIdeology.Reassure.SelfTarget` | Self-reassurance: guaranteed success | 自己再確認: 必ず成功 |
| `EnhancedIdeology.Reassure.TargetBeliefs` | Beliefs to reinforce (a random 1-{0} of): | 強化する信条（無作為に1〜{0}件）: |
| `EnhancedIdeology.Reassure.NoHeterodoxy` | Their beliefs are already orthodox - there is nothing to reinforce. | 彼らの信条はすでに正統——強化するものがない。 |
| `EnhancedIdeology.Reassure.FailMessage` | {0} failed to reassure {1}. | {0}は{1}を安心させることができなかった。 |

## Keyed/Settings.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.TranslationCredit` | Translation: dsweber2 | 翻訳: claude |
| `EnhancedIdeology.Section.Ideoligion` | Ideology | 思想 |
| `EnhancedIdeology.Section.Precept` | Precept | 戒律 |
| `EnhancedIdeology.Section.Compat` | Compatibility | 互換性 |
| `EnhancedIdeology.Section.Debug` | Debug | デバッグ |
| `EnhancedIdeology.SubSection.Belief` | Personal Certainty dynamics | 個人の信仰度の動態 |
| `EnhancedIdeology.SubSection.Conversion` | Conversion effects | 改宗の効果 |
| `EnhancedIdeology.CertaintyDriftRate` | Certainty drift rate: {0}/day | 信仰度の変動率: {0}/日 |
| `EnhancedIdeology.CertaintyDriftRate.Tip` | How fast a pawn's certainty moves toward its target each day. Higher = faith reacts faster to circumstances. Makes for more dynamic beliefs. | ポーンの信仰度が毎日目標値に向かって動く速さ。高いほど信仰が状況に素早く反応する。より動的な信条をもたらす。 |
| `EnhancedIdeology.RelationalMaxRange` | Relational influence: ±{0} | 関係的影響: ±{0} |
| `EnhancedIdeology.RelationalMaxRange.Tip` | The most that a pawn's opinion of their co-religionists can raise or lower their target certainty. | 同信者への評価が目標信仰度を上下させる最大量。 |
| `EnhancedIdeology.PracticeMaxRange` | Practice influence: ±{0} | 実践的影響: ±{0} |
| `EnhancedIdeology.PracticeMaxRange.Tip` | The most that precept moods (from rituals, veneration, taboos, etc) can raise or lower target certainty. | 戒律のムード（儀式・崇拝・タブーなど）が目標信仰度を上下させる最大量。 |
| `EnhancedIdeology.CrisisThreshold` | Crisis of faith threshold: {0} | 信仰の危機の閾値: {0} |
| `EnhancedIdeology.CrisisThreshold.Tip` | When a pawn's certainty falls below this, they may have a crisis-of-faith breakdown. Higher = more crises. | ポーンの信仰度がこれを下回ると、信仰の危機による精神崩壊が起きる可能性がある。高いほど危機が多くなる。 |
| `EnhancedIdeology.ConversionPace` | Conversion pace: {0} | 改宗ペース: {0} |
| `EnhancedIdeology.ConversionPace.Tip` | How quickly pawns abandon a poorly-fitting faith on their own, relative to default. Higher = spontaneous conversions are more likely. | ポーンが自発的に合わない信仰を捨てる速さ（デフォルト比）。高いほど自発的な改宗が起こりやすい。 |
| `EnhancedIdeology.ConversionCertaintyKnock` | Conversion attempt Certainty loss: {0} | 改宗試みによる信仰度低下: {0} |
| `EnhancedIdeology.ConversionCertaintyKnock.Tip` | When a moral guide wins a conversion debate but doesn't immediately flip the pawn, their certainty is multiplied by this. 1x = an 80%-certain pawn stays at 80%; 0.8x = an 80%-certain pawn drops to 64%. Lower = conversion attempts more impactful. | 道徳的指導者が改宗議論に勝ってもポーンがすぐに改宗しない場合、信仰度がこの値で乗算される。1x=80%信仰度のポーンが80%のまま。0.8x=80%が64%に低下。低いほど改宗試みの影響が大きい。 |
| `EnhancedIdeology.ConvictionDecayRate` | Conviction decay: {0}/quadrum | 確信の減衰: {0}/節 |
| `EnhancedIdeology.ConvictionDecayRate.Tip` | How much conviction strength all precepts lose per quadrum. Note that 100% conviction is at a precept average of 20, max is 50. Higher = stronger need for reinforcing beliefs. | 全戒律が節ごとに失う確信強度。100%確信は戒律平均20、最大50。高いほど信条強化の必要性が高まる。 |
| `EnhancedIdeology.PreceptOppositionScale` | Opposite-stance opposition: {0} | 対立姿勢への反発: {0} |
| `EnhancedIdeology.PreceptOppositionScale.Tip` | How strongly a pawn opposes the stance at the opposite extreme of an issue, as a fraction of their conviction. At 100%, a pawn with Cannibalism: Disapproved 12 has opinion -12 of Cannibalism: Required (ravenous). 0% = indifferent to opposing stances; 100% = equal and opposite to their support for their own at the furthest extreme precept. | ポーンが問題の反対極端の姿勢にどれだけ強く反発するか（確信の割合）。100%では、食人：不承認12のポーンは食人：必須（貪欲）に対して-12の評価を持つ。0%=対立姿勢に無関心、100%=自分の最遠極端戒律への支持と等しく反対。 |
| `EnhancedIdeology.ConversionStancePull` | Conversion strength: {0} | 改宗強度: {0} |
| `EnhancedIdeology.ConversionStancePull.Tip` | How much harder a successful conversion attempt (a moral guide's directed action) pushes a pawn's beliefs than an ordinary conversion attempt. Higher = faster conversions. | 成功した改宗試み（道徳的指導者の指示行動）が通常の改宗試みよりもポーンの信条をどれだけ強く押し進めるか。高いほど改宗が速い。 |
| `EnhancedIdeology.DebateConvictionChange` | Debate conviction change: {0} | 議論による確信の変化: {0} |
| `EnhancedIdeology.DebateConvictionChange.Tip` | How far a single won argument moves belief on the issue, relative to default. Shifts both the precept and the conviction in that precept simultaneously. Higher = each won debate persuades more. Leads to faster conversions. | 1度の勝利した議論が問題に対する信条をどれだけ動かすか（デフォルト比）。戒律とその確信の両方を同時に変化させる。高いほど各議論の説得力が高まる。改宗が速くなる。 |
| `EnhancedIdeology.ConversionOpinionMultiplier` | Friendship bonus (conversion): {0} | 友好ボーナス（改宗）: {0} |
| `EnhancedIdeology.ConversionOpinionMultiplier.Tip` | How much a pawn's opinion of someone trying to change their precept amplifies the effect. At 1x and opinion +100, both the certainty knock and the belief pull are doubled. At 0x, opinion has no effect. Not used when Peer Pressure is installed — that mod's own multiplier setting takes over instead. | 戒律を変えようとしている相手への評価が効果をどれだけ増幅するか。1xかつ評価+100では、信仰度低下と信条引力の両方が2倍になる。0xでは評価は効果なし。「仲間の圧力」MODがインストールされている場合は使用されず、そのMODの倍率設定が代わりに適用される。 |
| `EnhancedIdeology.SaveCompatMinCertainty` | Save-load minimum certainty: {0} | セーブ読込時の最低信仰度: {0} |
| `EnhancedIdeology.SaveCompatMinCertainty.Tip` | When loading a vanilla or pre-mod save, pawns whose certainty is below this have their beliefs seeded to at least this target instead of zero. Prevents a pawn with 0% certainty from being left with no convictions whatsoever. | バニラまたはMOD導入前のセーブを読み込む際、信仰度がこれを下回るポーンの信条はゼロの代わりに最低限この目標値に設定される。信仰度0%のポーンが確信を一切持たない状態になるのを防ぐ。 |
| `EnhancedIdeology.DebugInteractionWorkers` | DEV: Debug Interaction Workers | 開発: インタラクションワーカーのデバッグ |
| `EnhancedIdeology.DebugInteractionWorkers.Tip` | Enables debug logging for interaction workers when dev mode is active. I suggest you leave this off unless debugging Enhanced Ideology specifically. | 開発モード有効時にインタラクションワーカーのデバッグログを有効にする。Enhanced Ideologyのデバッグ以外ではオフにすることを推奨する。 |

## Keyed/TabOpinion.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.TabOpinion` | Ideology | 思想 |
| `EnhancedIdeology.IdeologyOpinions` | Ideology Opinions | 思想への評価 |
| `EnhancedIdeology.BeliefsHeader` | Issues | 問題 |
| `EnhancedIdeology.ColIssue` | Issue | 問題 |
| `EnhancedIdeology.ColStance` | Stance | 姿勢 |
| `EnhancedIdeology.ColStrength` | Strength | 強度 |
| `EnhancedIdeology.StanceDontCare` | Don't care | 無関心 |
| `EnhancedIdeology.StanceTooltip` | On the issue of {1}, {PAWN_nameDef} has the stance {2} with conviction {3}. {IDEO_name} preaches {5}; {PAWN_nameDef}'s opinion of that stance: {6}. | {1}の問題について、{PAWN_nameDef}は確信{3}で姿勢{2}をとっている。{IDEO_name}は{5}を説く。{PAWN_nameDef}のその姿勢への評価: {6}。 |
| `EnhancedIdeology.PawnOpinionTooltip` | {PAWN_nameDef}'s opinion of {IDEO_name}: {2} | {PAWN_nameDef}の{IDEO_name}への評価: {2} |
| `EnhancedIdeology.PawnOptionToolTip.FromMemesAndPrecepts` | From memes and precepts: {0} | ミームと戒律から: {0} |
| `EnhancedIdeology.PawnOptionToolTip.FromPersonalBeliefs` | From personal beliefs: {0} | 個人の信条から: {0} |
| `EnhancedIdeology.PawnOptionToolTip.FromInterpersonalRelationships` | From interpersonal relationships: {0} | 対人関係から: {0} |

## Keyed/TabSocial.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.PawnCertaintyTooltip` | {PAWN_nameDef}'s certainty in {IDEO_name}: {2} | {PAWN_nameDef}の{IDEO_name}に対する信仰度: {2} |
| `EnhancedIdeology.CertaintyTarget` | Target certainty: {0} | 目標信仰度: {0} |
| `EnhancedIdeology.CertainChangePerDay` | Drifting toward it at {0}/day | {0}/日の速さで変動中 |
| `EnhancedIdeology.CertaintyBandStructural` | Structural (beliefs & nature): {0} | 構造的（信条と性質）: {0} |
| `EnhancedIdeology.CertaintyBandRelational` | Congregation: {0} | 会衆: {0} |
| `EnhancedIdeology.CertaintyBandPractice` | Practice (recent rites): {0} | 実践（最近の儀式）: {0} |
| `EnhancedIdeology.CertaintyPreceptDecay` | Precept decay: {0}/quadrum | 戒律の減衰: {0}/節 |
| `EnhancedIdeology.CouplingPenalty` | Opposed practices | 相反する慣行 |
| `EnhancedIdeology.DietGeneAntiMeat` | Obligate herbivore | 絶対草食 |
| `EnhancedIdeology.DietGeneProMeat` | Obligate carnivore | 絶対肉食 |
| `EnhancedIdeology.XenotypePreferred` | Preferred xenotype | 好まれる異種型 |
| `EnhancedIdeology.XenotypeDisapproved` | Disapproved xenotype | 忌避される異種型 |

## Keyed/Work_CompleteReligiousBook.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.IdeologyMissingRole` | Ideoligion does not have the required '{ROLE}' role. | 思想に必要な「{ROLE}」の役職がない。 |
| `EnhancedIdeology.PawnMissingRequiredRole` | {PAWN} does not have the required '{ROLE}' role. | {PAWN}は必要な「{ROLE}」の役職を持っていない。 |
| `EnhancedIdeology.PawnIsNotAuthor` | {PAWN} is not the author of the book. | {PAWN}はこの本の著者ではない。 |
| `EnhancedIdeology.BookIsNotForPawnIdeoligion` | The book is not for {PAWN_possessive} ideology. | この本は{PAWN_possessive}の思想のものではない。 |
| `EnhancedIdeology.NoFreeValidLecternFound` | No available, accessible lectern found to do work on. | 作業できる利用可能な演台が見つからない。 |
| `EnhancedIdeology.InsufficientCertaintyToWrite` | Certainty must be above 90% to write religious texts. | 聖典を書くには信仰度が90%以上必要。 |

## DefInjected/ThingDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Ideobook.label` | religious book | 聖典 |
| `EB_Ideobook.description` | A book containing fictional or true stories for the pleasure and edification of the reader. | 読者の喜びと教化のために、架空または実際の物語を収めた書物。 |
| `EB_UnfinishedDeskIdeobook.label` | unfinished religious book | 未完成の聖典 |
| `EB_UnfinishedDeskIdeobook.description` | An unfinished religious book being written at the writing desk. | 執筆台で書かれている途中の聖典。 |
| `EB_WritingDesk.label` | writing desk | 執筆台 |
| `EB_WritingDesk.description` | A desk for composing ideological texts. Only colonists with deep conviction in their beliefs can write at it. | 思想的な文章を書くための机。信条に深い確信を持つ入植者のみが執筆できる。 |

## DefInjected/HistoryEventDef

| key | source | translation |
|-----|--------|-------------|
| `EB_DestroyedReligiousBook.label` | destroyed religious book | 聖典を破壊した |
| `EB_BookDestroyed.label` | religious book was destroyed | 聖典が破壊された |
| `EB_Contemplated.label` | contemplated | 瞑想した |

## DefInjected/HediffDef

| key | source | translation |
|-----|--------|-------------|
| `EB_BrainwipeRecovery.label` | belief reformation | 信条の再形成 |
| `EB_BrainwipeRecovery.description` | This person's beliefs were shattered by a psychic brainwipe. Their convictions are weak and unfocused, leaving them unusually open to ideological influence. | この人物の信条は精神洗浄によって粉砕された。確信が弱く散漫になっており、思想的影響を受けやすい状態にある。 |

## DefInjected/InspirationDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ReligiousEnlightenment.label` | religious enlightenment | 宗教的啓示 |
| `EB_ReligiousEnlightenment.beginLetter` | [PAWN_pronoun] feels a moment of divine clarity and is compelled to write a religious text. | [PAWN_pronoun]は神聖な明晰さの瞬間を感じ、聖典を書かずにはいられなくなった。 |
| `EB_ReligiousEnlightenment.baseInspectLine` | Inspired: Religious enlightenment | 鼓舞: 宗教的啓示 |
| `EB_ReligiousEnlightenment.endMessage` | [PAWN_nameDef]'s moment of religious enlightenment has passed. | [PAWN_nameDef]の宗教的啓示の瞬間が過ぎ去った。 |

## DefInjected/InteractionDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ConversionLog.label` | conversion | 改宗 |
| `EB_ConversionLog.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] converted to a new faith. | r_logentry->[INITIATOR_nameDef]は新たな信仰に改宗した。 |
| `EB_CrisisOfFaithLog.label` | crisis of faith | 信仰の危機 |
| `EB_CrisisOfFaithLog.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] experienced a crisis of faith. | r_logentry->[INITIATOR_nameDef]は信仰の危機を経験した。 |
| `EB_IdeologicalDebateMeme.label` | ideological debate over memes | ミームをめぐる思想的議論 |
| `EB_IdeologicalDebateMeme.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] and [RECIPIENT_nameDef] debated memes. | r_logentry->[INITIATOR_nameDef]と[RECIPIENT_nameDef]はミームについて議論した。 |
| `EB_IdeologicalDebatePrecept.label` | ideological debate over precepts | 戒律をめぐる思想的議論 |
| `EB_IdeologicalDebatePrecept.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] and [RECIPIENT_nameDef] debated beliefs. | r_logentry->[INITIATOR_nameDef]と[RECIPIENT_nameDef]は信条について議論した。 |

## DefInjected/RulePackDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Sentence_DebateWon.rulePack.rulesStrings.0` | sent->[INITIATOR_nameDef] won the debate. | sent->[INITIATOR_nameDef]は議論に勝った。 |
| `EB_Sentence_InitiatorWon.rulePack.rulesStrings.0` | sent->[INITIATOR_nameDef] proved more persuasive. | sent->[INITIATOR_nameDef]はより説得力があることを示した。 |
| `EB_Sentence_RecipientWon.rulePack.rulesStrings.0` | sent->[RECIPIENT_nameDef] proved more persuasive. | sent->[RECIPIENT_nameDef]はより説得力があることを示した。 |
| `EB_Sentence_DebateDraw.rulePack.rulesStrings.0` | sent->Neither changed their view. | sent->どちらも考えを変えなかった。 |

## DefInjected/JobDef

| key | source | translation |
|-----|--------|-------------|
| `EB_IconoclastDebate.reportString` | haranguing TargetA about ideology. |  |
| `EB_PlaceAndBurnUntilDestroyed.reportString` | placing TargetA on the ground to burn. | TargetAを地面に置いて燃やしている。 |
| `EB_Pray.reportString` | contemplating. | 瞑想中。 |

## DefInjected/WorkGiverDef

| key | source | translation |
|-----|--------|-------------|
| `EB_WriteIdeobookAtDesk.label` | write religious books | 聖典を書く |
| `EB_WriteIdeobookAtDesk.verb` | write | 書く |
| `EB_WriteIdeobookAtDesk.gerund` | writing religious books | 聖典を執筆中 |

## DefInjected/MentalStateDef

| key | source | translation |
|-----|--------|-------------|
| `EB_CrisisOfFaith.label` | crisis of faith | 信仰の危機 |
| `EB_CrisisOfFaith.beginLetterLabel` | crisis of faith | 信仰の危機 |
| `EB_CrisisOfFaith.beginLetter` | {0} is having a crisis of faith.\n\n[PAWN_pronoun] will spend the next day or two in contemplation, questioning [PAWN_possessive] beliefs. | {0}は信仰の危機を迎えている。\n\n[PAWN_pronoun]は次の1〜2日間を瞑想に費やし、[PAWN_possessive]信条に疑問を抱くだろう。 |
| `EB_CrisisOfFaith.recoveryMessage` | {0}'s crisis of faith has passed. | {0}の信仰の危機は過ぎ去った。 |
| `EB_CrisisOfFaith.baseInspectLine` | Mental state: Crisis of faith | 精神状態: 信仰の危機 |
| `EB_Iconoclast.label` | iconoclast | 偶像破壊者 |
| `EB_Iconoclast.beginLetter` | {0} had a mental break and is being an iconoclast.\n\n[PAWN_pronoun] is going try and burn religious books. | {0}は精神崩壊を起こし、偶像破壊者と化している。\n\n[PAWN_pronoun]は聖典を燃やそうとするだろう。 |
| `EB_Iconoclast.recoveryMessage` | {0} is no longer being an iconoclast. | {0}はもはや偶像破壊者ではなくなった。 |
| `EB_Iconoclast.baseInspectLine` | Mental state: Iconoclast | 精神状態: 偶像破壊者 |

## DefInjected/IssueDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Contemplation.label` | contemplation | 瞑想 |

## DefInjected/NeedDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Contemplation.label` | contemplation | 瞑想 |
| `EB_Contemplation.description` | This pawn's ideology requires regular contemplation. Neglecting it erodes their sense of devotion. | このポーンの思想は定期的な瞑想を要求する。怠ると信仰心が損なわれる。 |

## DefInjected/PreceptDef

| key | source | translation |
|-----|--------|-------------|
| `Contemplation_Required.label` | required | 必須 |
| `Contemplation_Required.description` | Contemplation is a sacred obligation. Members must contemplate regularly, and the moral guide is expected to lead by example. | 瞑想は神聖な義務である。信者は定期的に瞑想しなければならず、道徳的指導者は率先垂範が求められる。 |
| `Contemplation_Respected.label` | respected | 尊重 |
| `Contemplation_Respected.description` | Contemplation is a meaningful practice, and those who contemplate regularly are held in higher regard. | 瞑想は意義深い実践であり、定期的に瞑想する者はより高く評価される。 |
| `Contemplation_Normal.label` | acceptable | 許容 |
| `Contemplation_Normal.description` | Contemplation is a personal matter — neither encouraged nor discouraged. | 瞑想は個人的な問題であり——推奨も否定もされない。 |
| `Contemplation_Disapproved.label` | disapproved | 不承認 |
| `Contemplation_Disapproved.description` | Contemplation is seen as weakness or superstition. Members who contemplate are viewed unfavorably. | 瞑想は弱さや迷信と見なされる。瞑想する信者は不利な目で見られる。 |
| `Contemplation_Forbidden.label` | forbidden | 禁止 |
| `Contemplation_Forbidden.description` | Contemplation is strictly forbidden. Any member caught contemplating will be shunned. | 瞑想は厳しく禁じられている。瞑想しているところを見つかった信者は追放される。 |

## DefInjected/ThoughtDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ContemplationNeed.stages.0.label` | contemplated regularly | 定期的に瞑想した |
| `EB_ContemplationNeed.stages.0.description` | I've been faithful in my contemplations. It brings me peace. | 瞑想に励んできた。心に平和をもたらしてくれる。 |
| `EB_ContemplationNeed.stages.1.label` | contemplation neglected | 瞑想を怠っている |
| `EB_ContemplationNeed.stages.1.description` | I haven't found time to contemplate. My devotion is slipping. | 瞑想する時間が取れていない。信仰心が薄れていく。 |
| `EB_ContemplationNeed.stages.2.label` | contemplation severely neglected | 瞑想を著しく怠っている |
| `EB_ContemplationNeed.stages.2.description` | I've gone far too long without contemplation. I feel spiritually adrift. | 瞑想せずにあまりにも長い時間が過ぎた。精神的に漂流しているようだ。 |
| `EB_WitnessedContemplation_Approved.stages.0.label` | contemplated devoutly | 敬虔に瞑想した |
| `EB_WitnessedContemplation_Disapproved.stages.0.label` | engaged in contemplation | 瞑想に従事した |
| `EB_WitnessedContemplation_Forbidden.stages.0.label` | defied the ban on contemplation | 瞑想禁止に背いた |
| `EB_ReligiousBookDestroyed.stages.0.label` | religious book destroyed | 聖典が破壊された |
| `EB_ReligiousBookDestroyed.stages.0.description` | Our religious book was destroyed. That is absolutely inexcusable. | 我々の聖典が破壊された。まったく許しがたいことだ。 |
| `EB_WroteSacrilegousBinding.stages.0.label` | defiled a sacred text | 聖典を汚した |
| `EB_WroteSacrilegousBinding.stages.0.description` | I bound a holy book in animal hide. That was wrong. | 聖典を動物の皮で装丁した。それは間違いだった。 |
| `EB_ReadingLeatherboundBook.stages.0.label` | reading leather-bound scripture | 革装丁の聖典を読んでいる |
| `EB_ReadingLeatherboundBook.stages.0.description` | This holy text is bound in animal hide. Holding it feels wrong. | この聖典は動物の皮で装丁されている。手に持つだけで不快だ。 |
| `EB_GoodDebate.stages.0.label` | good debate | 良い議論 |
| `EB_GoodDebate.stages.0.description` | We had a stimulating debate about our beliefs. A meeting of open minds. | 信条について刺激的な議論をした。開かれた心の出会いだった。 |
| `EB_BadDebate.stages.0.label` | heretical debate | 異端的な議論 |
| `EB_BadDebate.stages.0.description` | I had to refute their deviant beliefs. Diversity of thought is a poison. | 彼らの逸脱した信条を論駁しなければならなかった。思想の多様性は毒だ。 |
| `EB_ProselytizerDebated.stages.0.label` | spread the word | 教えを広めた |
| `EB_ProselytizerDebated.stages.0.description` | I argued for the truth of my faith. Whatever the outcome, the attempt itself was a righteous act. | 信仰の真実のために議論した。結果がどうあれ、その試み自体が義なる行為だった。 |
| `EB_ProselytizerConverted.stages.0.label` | brought a soul to the faith | 魂を信仰に導いた |
| `EB_ProselytizerConverted.stages.0.description` | I brought someone into the light of our faith. There is no greater calling. | 誰かを我々の信仰の光へと導いた。これ以上の使命はない。 |
| `EB_ProselytizerFailedConversion.stages.0.label` | failed to convert | 改宗に失敗した |
| `EB_ProselytizerFailedConversion.stages.0.description` | I tried to bring someone to the faith and failed. They remain in their ignorance. | 誰かを信仰に導こうとして失敗した。彼らは無知のままだ。 |
| `EB_ApostacyDebated.stages.0.label` | faith challenged | 信仰に挑まれた |
| `EB_ApostacyDebated.stages.0.description` | I had to defend the truth of my faith against a challenger. The very fact they dared question it is an insult. | 挑戦者に対して信仰の真実を守らなければならなかった。疑問を呈した事実自体が侮辱だ。 |
| `EB_CognitiveDissonance.stages.0.label` | cognitive dissonance | 認知的不協和 |
| `EB_CognitiveDissonance.stages.0.description` | I shouldn't be participating in other ideoligion's practices. | 他の思想の慣行に参加すべきではなかった。 |
| `EB_FaithReaffirmed.stages.0.label` | faith reaffirmed | 信仰が再確認された |
| `EB_FaithReaffirmed.stages.0.description` | Seeing another tradition's way of doing things reminds me why I value my own path. | 別の伝統のやり方を見て、自分の道を大切にする理由を思い出した。 |
| `EB_LowCertaintyCoBeliever.stages.0.label` | wavering co-believer | 揺らぐ同信者 |
| `EB_LowCertaintyCoBeliever.stages.1.label` | faithless co-believer | 信仰のない同信者 |
| `EB_LowCertaintyCoBeliever.stages.2.label` | apostate-hearted co-believer | 背教心を持つ同信者 |

## DefInjected/RecipeDef

| key | source | translation |
|-----|--------|-------------|
| `EB_WriteIdeobook.label` | write religious book | 聖典を書く |
| `EB_WriteIdeobook.description` | Write a religious book detailing your ideology's beliefs. Requires high certainty in one's ideoligion (at least 90%). | 自身の思想の信条を詳述した聖典を書く。思想への高い信仰度（90%以上）が必要。 |
| `EB_WriteIdeobook.jobString` | writing a religious book. | 聖典を執筆中。 |
| `EB_WriteIllustratedIdeobook.label` | write illustrated religious book | 彩飾聖典を書く |
| `EB_WriteIllustratedIdeobook.description` | Write an illuminated religious book adorned with jems and inks. The quality of materials increases the devotional aspects of reading it. Requires high certainty in one's ideoligion (at least 90%). | 宝石とインクで飾られた彩飾聖典を書く。素材の品質が読書時の信仰的側面を高める。思想への高い信仰度（90%以上）が必要。 |
| `EB_WriteIllustratedIdeobook.jobString` | illustrating religious book. | 聖典を彩飾中。 |

## DefInjected/RitualAttachableOutcomeEffectDef

| key | source | translation |
|-----|--------|-------------|
| `EB_BeliefReinforcement.label` | belief reinforcement | 信条強化 |
| `EB_BeliefReinforcement.effectDesc` | participants' convictions will be reinforced toward orthodoxy. | 参加者の確信が正統へと強化される。 |
| `EB_BeliefReinforcement.letterInfoText` | The ritual deepened the participants' convictions. | 儀式により参加者の確信が深まった。 |
