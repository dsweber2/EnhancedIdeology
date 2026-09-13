<!-- EnhancedIdeology Translation Template: English → Korean (한국어) -->
<!-- Author: claude -->
<!-- Pipes in text: use \| -->

## Keyed/Books.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.BookReadingBenefit` | {IDEO} - {1} conviction shift/qd | {IDEO} - {1} 확신 변화/분기 |
| `EnhancedIdeology.BookBeliefsHeader` | Beliefs advocated | 지지하는 신념 |
| `EnhancedIdeology.ShiftRatePerQuadrum` | {0}/qd | {0}/분기 |
| `EnhancedIdeology.LetterReligiousBookDestroyedLabel` | Religious book destroyed | 종교 서적 파괴됨 |
| `EnhancedIdeology.LetterReligiousBookDestroyedText` | {BOOK}, an important religious book for {IDEO}, has been destroyed. Followers of {IDEO} won't be happy about it. | {IDEO}의 중요한 종교 서적인 {BOOK}이(가) 파괴되었습니다. {IDEO}의 신도들은 이를 달가워하지 않을 것입니다. |
| `EnhancedIdeology.DestroyBookByBurningItUpsetIdeo` | Destroy {BOOK} by burning it. This will upset {IDEO_memberNamePlural} of {IDEO}. | {BOOK}을(를) 불태워 파괴합니다. 이는 {IDEO}의 {IDEO_memberNamePlural}을(를) 화나게 할 것입니다. |
| `EnhancedIdeology.DestroyBookByBurningIt` | Destroy {BOOK} by burning it. | {BOOK}을(를) 불태워 파괴합니다. |
| `EnhancedIdeology.NoColonistCanDestroy` | No colonist can destroy {BOOK}. | 어떤 정착민도 {BOOK}을(를) 파괴할 수 없습니다. |
| `EnhancedIdeology.ConfirmDestroyBookByBurningItUpsetIdeo` | Are you sure you want to make {PAWN} burn {BOOK}? Doing so will greatly upset all {IDEO_memberNamePlural} of {IDEO}. | {PAWN}에게 {BOOK}을(를) 불태우게 하시겠습니까? 그렇게 하면 {IDEO}의 모든 {IDEO_memberNamePlural}이(가) 크게 화를 낼 것입니다. |
| `EnhancedIdeology.ConfirmDestroyBookByBurningIt` | Are you sure you want to make {PAWN} burn {BOOK}? | {PAWN}에게 {BOOK}을(를) 불태우게 하시겠습니까? |
| `EnhancedIdeology.BookBurningSuccess` | {PAWN} has destroyed {BOOK}. This has greatly upset {IDEO_memberNamePlural} of {IDEO}. | {PAWN}이(가) {BOOK}을(를) 파괴했습니다. 이로 인해 {IDEO}의 {IDEO_memberNamePlural}이(가) 크게 화를 냈습니다. |

## Keyed/Contemplation.xml

| key | source | translation |
|-----|--------|-------------|
| `EB_NoPewToPrayIn` | {PAWN_nameDef} couldn't find a ritual seat to contemplate in for {IDEO_name}. | {PAWN_nameDef}이(가) {IDEO_name}을(를) 위해 명상할 행사 좌석을 찾을 수 없었습니다. |
| `EB_ContemplationRoomDisrespected` | {PAWN_nameDef}'s {IDEO_name} contemplation room is disrespected. | {PAWN_nameDef}의 {IDEO_name} 명상 공간이 존중받지 못하고 있습니다. |
| `EB_ContemplationActivityReport` | contemplating (gain: {0}/hr) | 명상 중 (획득: {0}/시) |
| `EB_ContemplationSiteStatLabel` | Contemplation gain | 명상 획득량 |
| `EB_ContemplationSiteStatReport` | Conviction reinforcement rate per hour during contemplation. Shown for the selected pawn, or as a base multiplier when no pawn is selected. Scales with room impressiveness and the pawn's current conviction strength. Additional +10% per fellow believer in the room. | 명상 중 시간당 확신 강화율. 선택된 폰에 대해 표시되며, 폰이 선택되지 않은 경우 기본 배수로 표시됩니다. 방의 인상도와 폰의 현재 확신 강도에 따라 달라집니다. 같은 공간에 있는 동료 신도 1명당 추가 +10%. |
| `EB_ContemplationSiteInspect` | Contemplation gain: {0} | 명상 획득량: {0} |

## Keyed/Convert.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.Convert.CertaintyKnock` | Temporary certainty reduction: {0} | 일시적 믿음 감소: {0} |
| `EnhancedIdeology.Convert.SuccessChance` | Chance to win the debate: {0} | 토론 승리 확률: {0} |
| `EnhancedIdeology.Convert.ConversionChance` | Chance to convert if you win: {0} | 승리 시 교화 확률: {0} |
| `EnhancedIdeology.Convert.TargetBeliefs` | Target beliefs (a random 1-{0} of): | 교화 대상 신념 (무작위 1~{0}개): |
| `EnhancedIdeology.Convert.NoOpposition` | They oppose nothing your faith preaches - there is nothing to convert. | 상대방은 당신의 사상이 가르치는 어떤 것도 반대하지 않습니다 - 교화할 것이 없습니다. |
| `EnhancedIdeology.Convert.Factors` | Factors (you vs. them): | 요인 (나 vs. 상대방): |
| `EnhancedIdeology.Convert.Factor.ConversionPower` | Conversion power: {0} vs {1} | 교화력: {0} vs {1} |
| `EnhancedIdeology.Convert.Factor.Intellectual` | Intellectual debate: {0} vs {1} | 지적 토론: {0} vs {1} |
| `EnhancedIdeology.Convert.Factor.Social` | Social impact: {0} vs {1} | 사회적 영향: {0} vs {1} |

## Keyed/Interaction_IdeologicalDebate.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.IdeologicalDebateOutcomeSocialFight` | {PAWN1_labelShort} tried to debate ideological views with {PAWN2_labelShort}. This led to a social fight! | {PAWN1_labelShort}이(가) {PAWN2_labelShort}와(과) 사상적 관점에 대해 토론을 시도했습니다. 이것이 싸움으로 이어졌습니다! |
| `EnhancedIdeology.LetterLabelIdeologicalDebateConversion` | Ideological conversion | 사상적 교화 |
| `EnhancedIdeology.LetterIdeologicalDebateConversionText` | After debating {ISSUE_label}, {CONVINCER_labelShort} convinced {CONVINCED_labelShort} to abandon {OLDIDEO_name} and embrace {NEWIDEO_name}. | {ISSUE_label}에 대한 토론 끝에, {CONVINCER_labelShort}이(가) {CONVINCED_labelShort}을(를) 설득하여 {OLDIDEO_name}을(를) 버리고 {NEWIDEO_name}을(를) 받아들이게 했습니다. |
| `EnhancedIdeology.DebateLog.AboutMeme` | [INITIATOR_nameDef] debated [RECIPIENT_nameDef] about the meme [TOPIC_label]. | [INITIATOR_nameDef]이(가) [RECIPIENT_nameDef]와(과) 가르침 [TOPIC_label]에 대해 토론했습니다. |
| `EnhancedIdeology.DebateLog.AboutPrecept` | [INITIATOR_nameDef] debated [TOPIC_label] with [RECIPIENT_nameDef]. | [INITIATOR_nameDef]이(가) [RECIPIENT_nameDef]와(과) [TOPIC_label]에 대해 토론했습니다. |
| `EnhancedIdeology.DebateLog.Generic` | [INITIATOR_nameDef] debated with [RECIPIENT_nameDef]. | [INITIATOR_nameDef]이(가) [RECIPIENT_nameDef]와(과) 토론했습니다. |
| `EnhancedIdeology.DebateSent.InitiatorMoved` | [INITIATOR_nameDef] moved [RECIPIENT_nameDef] towards stance "[WINNING_STANCE_label]". | [INITIATOR_nameDef]이(가) [RECIPIENT_nameDef]를 "[WINNING_STANCE_label]" 입장으로 이끌었습니다. |
| `EnhancedIdeology.DebateSent.RecipientMoved` | [RECIPIENT_nameDef] moved [INITIATOR_nameDef] towards stance "[WINNING_STANCE_label]". | [RECIPIENT_nameDef]이(가) [INITIATOR_nameDef]를 "[WINNING_STANCE_label]" 입장으로 이끌었습니다. |
| `EnhancedIdeology.DebateSent.WinnerPersuasive` | [WINNER_nameDef] proved more persuasive. | [WINNER_nameDef]이(가) 더 설득력 있음을 증명했습니다. |
| `EnhancedIdeology.DebateSent.Draw` | Neither changed their view. | 아무도 견해를 바꾸지 않았습니다. |
| `EnhancedIdeology.JobReport_Debating` | Debating {0} | {0}와(과) 토론 중 |
| `EnhancedIdeology.CrisisLog.Wander` | [INITIATOR_nameDef] experienced a crisis of faith. | [INITIATOR_nameDef]이(가) 신앙의 위기를 경험했습니다. |
| `EnhancedIdeology.CrisisLog.MoodBreak` | [INITIATOR_nameDef]'s crisis of faith compounded [INITIATOR_possessive] misery. | [INITIATOR_nameDef]의 신앙의 위기가 [INITIATOR_possessive] 고통을 가중시켰습니다. |

## Keyed/Reassure.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.Reassure.CertaintyGain` | Certainty gain on win: +{0} | 승리 시 믿음 획득: +{0} |
| `EnhancedIdeology.Reassure.SuccessChance` | Chance to win the debate: {0} | 토론 승리 확률: {0} |
| `EnhancedIdeology.Reassure.SelfTarget` | Self-reassurance: guaranteed success | 자기 확신: 항상 성공 |
| `EnhancedIdeology.Reassure.TargetBeliefs` | Beliefs to reinforce (a random 1-{0} of): | 강화할 신념 (무작위 1~{0}개): |
| `EnhancedIdeology.Reassure.NoHeterodoxy` | Their beliefs are already orthodox - there is nothing to reinforce. | 상대방의 신념은 이미 정통입니다 - 강화할 것이 없습니다. |
| `EnhancedIdeology.Reassure.FailMessage` | {0} failed to reassure {1}. | {0}이(가) {1}을(를) 설득하는 데 실패했습니다. |

## Keyed/Settings.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.TranslationCredit` | Translation: dsweber2 | 번역: claude |
| `EnhancedIdeology.Section.Ideoligion` | Ideology | 사상 |
| `EnhancedIdeology.Section.Precept` | Precept | 규율 |
| `EnhancedIdeology.Section.Compat` | Compatibility | 호환성 |
| `EnhancedIdeology.Section.Debug` | Debug | 디버그 |
| `EnhancedIdeology.SubSection.Belief` | Personal Certainty dynamics | 개인 믿음 역학 |
| `EnhancedIdeology.SubSection.Conversion` | Conversion effects | 교화 효과 |
| `EnhancedIdeology.CertaintyDriftRate` | Certainty drift rate: {0}/day | 믿음 변화율: {0}/일 |
| `EnhancedIdeology.CertaintyDriftRate.Tip` | How fast a pawn's certainty moves toward its target each day. Higher = faith reacts faster to circumstances. Makes for more dynamic beliefs. | 폰의 믿음이 매일 목표를 향해 얼마나 빠르게 이동하는지. 높을수록 = 믿음이 상황에 더 빠르게 반응합니다. 더 역동적인 신념을 만들어냅니다. |
| `EnhancedIdeology.RelationalMaxRange` | Relational influence: ±{0} | 관계적 영향: ±{0} |
| `EnhancedIdeology.RelationalMaxRange.Tip` | The most that a pawn's opinion of their co-religionists can raise or lower their target certainty. | 같은 사상을 가진 동료들에 대한 폰의 의견이 목표 믿음을 높이거나 낮출 수 있는 최대 범위. |
| `EnhancedIdeology.PracticeMaxRange` | Practice influence: ±{0} | 실천 영향: ±{0} |
| `EnhancedIdeology.PracticeMaxRange.Tip` | The most that precept moods (from rituals, veneration, taboos, etc) can raise or lower target certainty. | 규율 감정(행사, 숭배, 금기 등에서)이 목표 믿음을 높이거나 낮출 수 있는 최대 범위. |
| `EnhancedIdeology.CrisisThreshold` | Crisis of faith threshold: {0} | 신앙의 위기 임계값: {0} |
| `EnhancedIdeology.CrisisThreshold.Tip` | When a pawn's certainty falls below this, they may have a crisis-of-faith breakdown. Higher = more crises. | 폰의 믿음이 이 값 아래로 떨어지면 신앙의 위기가 발생할 수 있습니다. 높을수록 = 위기가 더 자주 발생합니다. |
| `EnhancedIdeology.ConversionPace` | Conversion pace: {0} | 교화 속도: {0} |
| `EnhancedIdeology.ConversionPace.Tip` | How quickly pawns abandon a poorly-fitting faith on their own, relative to default. Higher = spontaneous conversions are more likely. | 폰이 잘 맞지 않는 사상을 스스로 얼마나 빠르게 떠나는지, 기본값 대비. 높을수록 = 자발적 교화가 더 일어나기 쉽습니다. |
| `EnhancedIdeology.ConversionCertaintyKnock` | Conversion attempt Certainty loss: {0} | 교화 시도 믿음 손실: {0} |
| `EnhancedIdeology.ConversionCertaintyKnock.Tip` | When a moral guide wins a conversion debate but doesn't immediately flip the pawn, their certainty is multiplied by this. 1x = an 80%-certain pawn stays at 80%; 0.8x = an 80%-certain pawn drops to 64%. Lower = conversion attempts more impactful. | 도덕적 지도자가 교화 토론에서 이겼지만 폰이 즉시 전향하지 않을 때, 믿음에 이 값을 곱합니다. 1x = 80% 믿음의 폰은 80%를 유지; 0.8x = 80% 믿음의 폰은 64%로 하락. 낮을수록 = 교화 시도의 영향이 더 큽니다. |
| `EnhancedIdeology.ConvictionDecayRate` | Conviction decay: {0}/quadrum | 확신 쇠퇴: {0}/분기 |
| `EnhancedIdeology.ConvictionDecayRate.Tip` | How much conviction strength all precepts lose per quadrum. Note that 100% conviction is at a precept average of 20, max is 50. Higher = stronger need for reinforcing beliefs. | 모든 규율이 분기당 잃는 확신 강도. 100% 확신은 규율 평균 20이며, 최대는 50입니다. 높을수록 = 신념을 강화해야 할 필요성이 더 강합니다. |
| `EnhancedIdeology.PreceptOppositionScale` | Opposite-stance opposition: {0} | 반대 입장 반감: {0} |
| `EnhancedIdeology.PreceptOppositionScale.Tip` | How strongly a pawn opposes the stance at the opposite extreme of an issue, as a fraction of their conviction. At 100%, a pawn with Cannibalism: Disapproved 12 has opinion -12 of Cannibalism: Required (ravenous). 0% = indifferent to opposing stances; 100% = equal and opposite to their support for their own at the furthest extreme precept. | 폰이 어떤 문제의 정반대 극단 입장에 얼마나 강하게 반대하는지, 확신의 비율로. 100%에서, 식인: 불승인 12를 가진 폰은 식인: 필수(탐식)에 대해 -12 의견을 가집니다. 0% = 반대 입장에 무관심; 100% = 가장 극단적인 규율에서 자신의 지지와 동등하고 반대. |
| `EnhancedIdeology.ConversionStancePull` | Conversion strength: {0} | 교화 강도: {0} |
| `EnhancedIdeology.ConversionStancePull.Tip` | How much harder a successful conversion attempt (a moral guide's directed action) pushes a pawn's beliefs than an ordinary conversion attempt. Higher = faster conversions. | 성공적인 교화 시도(도덕적 지도자의 직접 행동)가 일반 교화 시도보다 폰의 신념을 얼마나 더 강하게 밀어붙이는지. 높을수록 = 더 빠른 교화. |
| `EnhancedIdeology.DebateConvictionChange` | Debate conviction change: {0} | 토론 확신 변화: {0} |
| `EnhancedIdeology.DebateConvictionChange.Tip` | How far a single won argument moves belief on the issue, relative to default. Shifts both the precept and the conviction in that precept simultaneously. Higher = each won debate persuades more. Leads to faster conversions. | 하나의 토론 승리가 해당 문제에 대한 신념을 기본값 대비 얼마나 움직이는지. 규율과 해당 규율의 확신을 동시에 이동시킵니다. 높을수록 = 각 토론 승리의 설득력이 더 강합니다. 더 빠른 교화로 이어집니다. |
| `EnhancedIdeology.ConversionOpinionMultiplier` | Friendship bonus (conversion): {0} | 우정 보너스 (교화): {0} |
| `EnhancedIdeology.ConversionOpinionMultiplier.Tip` | How much a pawn's opinion of someone trying to change their precept amplifies the effect. At 1x and opinion +100, both the certainty knock and the belief pull are doubled. At 0x, opinion has no effect. Not used when Peer Pressure is installed — that mod's own multiplier setting takes over instead. | 규율을 바꾸려는 사람에 대한 폰의 의견이 효과를 얼마나 증폭시키는지. 1x이고 의견 +100일 때, 믿음 손실과 신념 당김 모두 두 배가 됩니다. 0x에서는 의견이 효과가 없습니다. Peer Pressure 모드가 설치되어 있을 때는 사용되지 않습니다 — 해당 모드의 고유 배수 설정이 대신 적용됩니다. |
| `EnhancedIdeology.SaveCompatMinCertainty` | Save-load minimum certainty: {0} | 저장-불러오기 최소 믿음: {0} |
| `EnhancedIdeology.SaveCompatMinCertainty.Tip` | When loading a vanilla or pre-mod save, pawns whose certainty is below this have their beliefs seeded to at least this target instead of zero. Prevents a pawn with 0% certainty from being left with no convictions whatsoever. | 바닐라 또는 모드 이전 저장 파일을 불러올 때, 믿음이 이 값보다 낮은 폰의 신념이 0 대신 최소한 이 목표값으로 설정됩니다. 0% 믿음의 폰이 아무런 확신 없이 남겨지는 것을 방지합니다. |
| `EnhancedIdeology.DebugInteractionWorkers` | DEV: Debug Interaction Workers | 개발: 상호작용 워커 디버그 |
| `EnhancedIdeology.DebugInteractionWorkers.Tip` | Enables debug logging for interaction workers when dev mode is active. I suggest you leave this off unless debugging Enhanced Ideology specifically. | 개발자 모드가 활성화되어 있을 때 상호작용 워커의 디버그 로깅을 활성화합니다. Enhanced Ideology를 직접 디버그하지 않는 한 끄는 것을 권장합니다. |

## Keyed/TabOpinion.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.TabOpinion` | Ideology | 사상 |
| `EnhancedIdeology.IdeologyOpinions` | Ideology Opinions | 사상 의견 |
| `EnhancedIdeology.BeliefsHeader` | Issues | 쟁점 |
| `EnhancedIdeology.ColIssue` | Issue | 쟁점 |
| `EnhancedIdeology.ColStance` | Stance | 입장 |
| `EnhancedIdeology.ColStrength` | Strength | 강도 |
| `EnhancedIdeology.StanceDontCare` | Don't care | 무관심 |
| `EnhancedIdeology.StanceTooltip` | On the issue of {1}, {PAWN_nameDef} has the stance {2} with conviction {3}. {IDEO_name} preaches {5}; {PAWN_nameDef}'s opinion of that stance: {6}. | {1} 쟁점에 대해, {PAWN_nameDef}은(는) 확신 {3}으로 {2} 입장을 가지고 있습니다. {IDEO_name}은(는) {5}을(를) 가르칩니다; 그 입장에 대한 {PAWN_nameDef}의 의견: {6}. |
| `EnhancedIdeology.PawnOpinionTooltip` | {PAWN_nameDef}'s opinion of {IDEO_name}: {2} | {PAWN_nameDef}의 {IDEO_name}에 대한 의견: {2} |
| `EnhancedIdeology.PawnOptionToolTip.FromMemesAndPrecepts` | From memes and precepts: {0} | 가르침과 규율에서: {0} |
| `EnhancedIdeology.PawnOptionToolTip.FromPersonalBeliefs` | From personal beliefs: {0} | 개인 신념에서: {0} |
| `EnhancedIdeology.PawnOptionToolTip.FromInterpersonalRelationships` | From interpersonal relationships: {0} | 대인 관계에서: {0} |

## Keyed/TabSocial.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.PawnCertaintyTooltip` | {PAWN_nameDef}'s certainty in {IDEO_name}: {2} | {PAWN_nameDef}의 {IDEO_name}에 대한 믿음: {2} |
| `EnhancedIdeology.CertaintyTarget` | Target certainty: {0} | 목표 믿음: {0} |
| `EnhancedIdeology.CertainChangePerDay` | Drifting toward it at {0}/day | {0}/일 속도로 목표를 향해 변화 중 |
| `EnhancedIdeology.CertaintyBandStructural` | Structural (beliefs & nature): {0} | 구조적 (신념 및 본성): {0} |
| `EnhancedIdeology.CertaintyBandRelational` | Congregation: {0} | 교인 공동체: {0} |
| `EnhancedIdeology.CertaintyBandPractice` | Practice (recent rites): {0} | 실천 (최근 행사): {0} |
| `EnhancedIdeology.CertaintyPreceptDecay` | Precept decay: {0}/quadrum | 규율 쇠퇴: {0}/분기 |
| `EnhancedIdeology.CouplingPenalty` | Opposed practices | 상충하는 실천 |
| `EnhancedIdeology.DietGeneAntiMeat` | Obligate herbivore | 의무적 초식 |
| `EnhancedIdeology.DietGeneProMeat` | Obligate carnivore | 의무적 육식 |
| `EnhancedIdeology.XenotypePreferred` | Preferred xenotype | 선호 이종 유형 |
| `EnhancedIdeology.XenotypeDisapproved` | Disapproved xenotype | 불승인 이종 유형 |

## Keyed/Work_CompleteReligiousBook.xml

| key | source | translation |
|-----|--------|-------------|
| `EnhancedIdeology.IdeologyMissingRole` | Ideoligion does not have the required '{ROLE}' role. | 사상에 필요한 '{ROLE}' 역할이 없습니다. |
| `EnhancedIdeology.PawnMissingRequiredRole` | {PAWN} does not have the required '{ROLE}' role. | {PAWN}에게 필요한 '{ROLE}' 역할이 없습니다. |
| `EnhancedIdeology.PawnIsNotAuthor` | {PAWN} is not the author of the book. | {PAWN}은(는) 이 책의 저자가 아닙니다. |
| `EnhancedIdeology.BookIsNotForPawnIdeoligion` | The book is not for {PAWN_possessive} ideology. | 이 책은 {PAWN_possessive} 사상을 위한 것이 아닙니다. |
| `EnhancedIdeology.NoFreeValidLecternFound` | No available, accessible lectern found to do work on. | 사용 가능한 강독대를 찾을 수 없습니다. |
| `EnhancedIdeology.InsufficientCertaintyToWrite` | Certainty must be above 90% to write religious texts. | 종교 서적을 작성하려면 믿음이 90% 이상이어야 합니다. |

## DefInjected/ThingDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Ideobook.label` | religious book | 종교 서적 |
| `EB_Ideobook.description` | A book containing fictional or true stories for the pleasure and edification of the reader. | 독자의 즐거움과 교화를 위한 허구 또는 실제 이야기가 담긴 책. |
| `EB_UnfinishedDeskIdeobook.label` | unfinished religious book | 미완성 종교 서적 |
| `EB_UnfinishedDeskIdeobook.description` | An unfinished religious book being written at the writing desk. | 글쓰기 책상에서 작성 중인 미완성 종교 서적. |
| `EB_WritingDesk.label` | writing desk | 글쓰기 책상 |
| `EB_WritingDesk.description` | A desk for composing ideological texts. Only colonists with deep conviction in their beliefs can write at it. | 사상적 텍스트를 작성하기 위한 책상. 신념에 깊은 확신을 가진 정착민만 여기서 글을 쓸 수 있습니다. |

## DefInjected/HistoryEventDef

| key | source | translation |
|-----|--------|-------------|
| `EB_DestroyedReligiousBook.label` | destroyed religious book | 종교 서적 파괴 |
| `EB_BookDestroyed.label` | religious book was destroyed | 종교 서적이 파괴되었음 |
| `EB_Contemplated.label` | contemplated | 명상함 |

## DefInjected/HediffDef

| key | source | translation |
|-----|--------|-------------|
| `EB_BrainwipeRecovery.label` | belief reformation | 신념 재형성 |
| `EB_BrainwipeRecovery.description` | This person's beliefs were shattered by a psychic brainwipe. Their convictions are weak and unfocused, leaving them unusually open to ideological influence. | 이 사람의 신념은 정신적 세뇌에 의해 산산조각났습니다. 확신이 약하고 집중되지 않아 사상적 영향에 비정상적으로 개방되어 있습니다. |

## DefInjected/InspirationDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ReligiousEnlightenment.label` | religious enlightenment | 종교적 깨달음 |
| `EB_ReligiousEnlightenment.beginLetter` | [PAWN_pronoun] feels a moment of divine clarity and is compelled to write a religious text. | [PAWN_pronoun]은(는) 신성한 명료함의 순간을 느끼고 종교적 텍스트를 써야 한다는 충동을 느낍니다. |
| `EB_ReligiousEnlightenment.baseInspectLine` | Inspired: Religious enlightenment | 영감: 종교적 깨달음 |
| `EB_ReligiousEnlightenment.endMessage` | [PAWN_nameDef]'s moment of religious enlightenment has passed. | [PAWN_nameDef]의 종교적 깨달음의 순간이 지나갔습니다. |

## DefInjected/InteractionDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ConversionLog.label` | conversion | 교화 |
| `EB_ConversionLog.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] converted to a new faith. | r_logentry->[INITIATOR_nameDef]이(가) 새로운 사상으로 교화되었습니다. |
| `EB_CrisisOfFaithLog.label` | crisis of faith | 신앙의 위기 |
| `EB_CrisisOfFaithLog.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] experienced a crisis of faith. | r_logentry->[INITIATOR_nameDef]이(가) 신앙의 위기를 경험했습니다. |
| `EB_IdeologicalDebateMeme.label` | ideological debate over memes | 가르침에 관한 사상적 토론 |
| `EB_IdeologicalDebateMeme.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] and [RECIPIENT_nameDef] debated memes. | r_logentry->[INITIATOR_nameDef]와(과) [RECIPIENT_nameDef]이(가) 가르침에 대해 토론했습니다. |
| `EB_IdeologicalDebatePrecept.label` | ideological debate over precepts | 규율에 관한 사상적 토론 |
| `EB_IdeologicalDebatePrecept.logRulesInitiator.rulesStrings.0` | r_logentry->[INITIATOR_nameDef] and [RECIPIENT_nameDef] debated beliefs. | r_logentry->[INITIATOR_nameDef]와(과) [RECIPIENT_nameDef]이(가) 신념에 대해 토론했습니다. |

## DefInjected/RulePackDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Sentence_DebateWon.rulePack.rulesStrings.0` | sent->[INITIATOR_nameDef] won the debate. | sent->[INITIATOR_nameDef]이(가) 토론에서 이겼습니다. |
| `EB_Sentence_InitiatorWon.rulePack.rulesStrings.0` | sent->[INITIATOR_nameDef] proved more persuasive. | sent->[INITIATOR_nameDef]이(가) 더 설득력 있음을 증명했습니다. |
| `EB_Sentence_RecipientWon.rulePack.rulesStrings.0` | sent->[RECIPIENT_nameDef] proved more persuasive. | sent->[RECIPIENT_nameDef]이(가) 더 설득력 있음을 증명했습니다. |
| `EB_Sentence_DebateDraw.rulePack.rulesStrings.0` | sent->Neither changed their view. | sent->아무도 견해를 바꾸지 않았습니다. |

## DefInjected/JobDef

| key | source | translation |
|-----|--------|-------------|
| `EB_IconoclastDebate.reportString` | haranguing TargetA about ideology. |  |
| `EB_PlaceAndBurnUntilDestroyed.reportString` | placing TargetA on the ground to burn. | TargetA를 불태우기 위해 바닥에 놓는 중. |
| `EB_Pray.reportString` | contemplating. | 명상 중. |

## DefInjected/WorkGiverDef

| key | source | translation |
|-----|--------|-------------|
| `EB_WriteIdeobookAtDesk.label` | write religious books | 종교 서적 작성 |
| `EB_WriteIdeobookAtDesk.verb` | write | 작성 |
| `EB_WriteIdeobookAtDesk.gerund` | writing religious books | 종교 서적 작성 중 |

## DefInjected/MentalStateDef

| key | source | translation |
|-----|--------|-------------|
| `EB_CrisisOfFaith.label` | crisis of faith | 신앙의 위기 |
| `EB_CrisisOfFaith.beginLetterLabel` | crisis of faith | 신앙의 위기 |
| `EB_CrisisOfFaith.beginLetter` | {0} is having a crisis of faith.\n\n[PAWN_pronoun] will spend the next day or two in contemplation, questioning [PAWN_possessive] beliefs. | {0}이(가) 신앙의 위기를 겪고 있습니다.\n\n[PAWN_pronoun]은(는) 앞으로 하루 이틀간 명상하며 [PAWN_possessive] 신념에 의문을 품을 것입니다. |
| `EB_CrisisOfFaith.recoveryMessage` | {0}'s crisis of faith has passed. | {0}의 신앙의 위기가 지나갔습니다. |
| `EB_CrisisOfFaith.baseInspectLine` | Mental state: Crisis of faith | 정신 상태: 신앙의 위기 |
| `EB_Iconoclast.label` | iconoclast | 성상파괴자 |
| `EB_Iconoclast.beginLetter` | {0} had a mental break and is being an iconoclast.\n\n[PAWN_pronoun] is going try and burn religious books. | {0}이(가) 정신적 붕괴를 겪고 성상파괴 행동을 보이고 있습니다.\n\n[PAWN_pronoun]은(는) 종교 서적을 불태우려 할 것입니다. |
| `EB_Iconoclast.recoveryMessage` | {0} is no longer being an iconoclast. | {0}은(는) 더 이상 성상파괴 행동을 하지 않습니다. |
| `EB_Iconoclast.baseInspectLine` | Mental state: Iconoclast | 정신 상태: 성상파괴자 |

## DefInjected/IssueDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Contemplation.label` | contemplation | 명상 |

## DefInjected/NeedDef

| key | source | translation |
|-----|--------|-------------|
| `EB_Contemplation.label` | contemplation | 명상 |
| `EB_Contemplation.description` | This pawn's ideology requires regular contemplation. Neglecting it erodes their sense of devotion. | 이 폰의 사상은 정기적인 명상을 요구합니다. 이를 게을리하면 헌신감이 약해집니다. |

## DefInjected/PreceptDef

| key | source | translation |
|-----|--------|-------------|
| `Contemplation_Required.label` | required | 필수 |
| `Contemplation_Required.description` | Contemplation is a sacred obligation. Members must contemplate regularly, and the moral guide is expected to lead by example. | 명상은 신성한 의무입니다. 구성원은 정기적으로 명상해야 하며, 도덕적 지도자는 솔선수범해야 합니다. |
| `Contemplation_Respected.label` | respected | 존중됨 |
| `Contemplation_Respected.description` | Contemplation is a meaningful practice, and those who contemplate regularly are held in higher regard. | 명상은 의미 있는 수행이며, 정기적으로 명상하는 사람들은 더 높이 평가받습니다. |
| `Contemplation_Normal.label` | acceptable | 허용됨 |
| `Contemplation_Normal.description` | Contemplation is a personal matter — neither encouraged nor discouraged. | 명상은 개인적인 문제입니다 — 권장되지도 만류되지도 않습니다. |
| `Contemplation_Disapproved.label` | disapproved | 불승인 |
| `Contemplation_Disapproved.description` | Contemplation is seen as weakness or superstition. Members who contemplate are viewed unfavorably. | 명상은 나약함 또는 미신으로 여겨집니다. 명상하는 구성원은 부정적으로 평가됩니다. |
| `Contemplation_Forbidden.label` | forbidden | 금지됨 |
| `Contemplation_Forbidden.description` | Contemplation is strictly forbidden. Any member caught contemplating will be shunned. | 명상은 엄격히 금지됩니다. 명상을 하다 발각된 구성원은 배척당할 것입니다. |

## DefInjected/ThoughtDef

| key | source | translation |
|-----|--------|-------------|
| `EB_ContemplationNeed.stages.0.label` | contemplated regularly | 정기적으로 명상함 |
| `EB_ContemplationNeed.stages.0.description` | I've been faithful in my contemplations. It brings me peace. | 명상에 충실해 왔습니다. 평화를 가져다줍니다. |
| `EB_ContemplationNeed.stages.1.label` | contemplation neglected | 명상 소홀 |
| `EB_ContemplationNeed.stages.1.description` | I haven't found time to contemplate. My devotion is slipping. | 명상할 시간을 찾지 못했습니다. 헌신이 흔들리고 있습니다. |
| `EB_ContemplationNeed.stages.2.label` | contemplation severely neglected | 명상 심각하게 소홀 |
| `EB_ContemplationNeed.stages.2.description` | I've gone far too long without contemplation. I feel spiritually adrift. | 너무 오랫동안 명상을 하지 않았습니다. 영적으로 표류하는 느낌입니다. |
| `EB_WitnessedContemplation_Approved.stages.0.label` | contemplated devoutly | 독실하게 명상함 |
| `EB_WitnessedContemplation_Disapproved.stages.0.label` | engaged in contemplation | 명상에 임함 |
| `EB_WitnessedContemplation_Forbidden.stages.0.label` | defied the ban on contemplation | 명상 금지를 어김 |
| `EB_ReligiousBookDestroyed.stages.0.label` | religious book destroyed | 종교 서적 파괴됨 |
| `EB_ReligiousBookDestroyed.stages.0.description` | Our religious book was destroyed. That is absolutely inexcusable. | 우리의 종교 서적이 파괴되었습니다. 절대 용납할 수 없는 일입니다. |
| `EB_WroteSacrilegousBinding.stages.0.label` | defiled a sacred text | 성스러운 텍스트를 더럽힘 |
| `EB_WroteSacrilegousBinding.stages.0.description` | I bound a holy book in animal hide. That was wrong. | 성서를 동물 가죽으로 제본했습니다. 그것은 잘못된 일이었습니다. |
| `EB_ReadingLeatherboundBook.stages.0.label` | reading leather-bound scripture | 가죽 제본 경전 읽기 |
| `EB_ReadingLeatherboundBook.stages.0.description` | This holy text is bound in animal hide. Holding it feels wrong. | 이 성서는 동물 가죽으로 제본되어 있습니다. 들고 있으니 거북합니다. |
| `EB_GoodDebate.stages.0.label` | good debate | 좋은 토론 |
| `EB_GoodDebate.stages.0.description` | We had a stimulating debate about our beliefs. A meeting of open minds. | 우리의 신념에 대해 자극적인 토론을 나눴습니다. 열린 마음의 만남이었습니다. |
| `EB_BadDebate.stages.0.label` | heretical debate | 이단적 토론 |
| `EB_BadDebate.stages.0.description` | I had to refute their deviant beliefs. Diversity of thought is a poison. | 그들의 이단적 신념을 반박해야 했습니다. 사상의 다양성은 독입니다. |
| `EB_ProselytizerDebated.stages.0.label` | spread the word | 말씀 전파 |
| `EB_ProselytizerDebated.stages.0.description` | I argued for the truth of my faith. Whatever the outcome, the attempt itself was a righteous act. | 내 사상의 진리를 위해 논쟁했습니다. 결과가 어떻든, 시도 자체가 의로운 행동이었습니다. |
| `EB_ProselytizerConverted.stages.0.label` | brought a soul to the faith | 영혼을 사상으로 인도함 |
| `EB_ProselytizerConverted.stages.0.description` | I brought someone into the light of our faith. There is no greater calling. | 누군가를 우리 사상의 빛으로 인도했습니다. 이보다 더 위대한 소명은 없습니다. |
| `EB_ProselytizerFailedConversion.stages.0.label` | failed to convert | 교화 실패 |
| `EB_ProselytizerFailedConversion.stages.0.description` | I tried to bring someone to the faith and failed. They remain in their ignorance. | 누군가를 사상으로 인도하려 했지만 실패했습니다. 그들은 무지 속에 남아 있습니다. |
| `EB_ApostacyDebated.stages.0.label` | faith challenged | 믿음에 도전받음 |
| `EB_ApostacyDebated.stages.0.description` | I had to defend the truth of my faith against a challenger. The very fact they dared question it is an insult. | 도전자에 맞서 내 사상의 진리를 지켜야 했습니다. 감히 의심했다는 사실 자체가 모욕입니다. |
| `EB_CognitiveDissonance.stages.0.label` | cognitive dissonance | 인지 부조화 |
| `EB_CognitiveDissonance.stages.0.description` | I shouldn't be participating in other ideoligion's practices. | 다른 사상의 행사에 참여해서는 안 됩니다. |
| `EB_FaithReaffirmed.stages.0.label` | faith reaffirmed | 믿음 재확인 |
| `EB_FaithReaffirmed.stages.0.description` | Seeing another tradition's way of doing things reminds me why I value my own path. | 다른 전통의 방식을 보면서 내 자신의 길을 소중히 여기는 이유를 상기시켜줍니다. |
| `EB_LowCertaintyCoBeliever.stages.0.label` | wavering co-believer | 흔들리는 동료 신도 |
| `EB_LowCertaintyCoBeliever.stages.1.label` | faithless co-believer | 믿음 없는 동료 신도 |
| `EB_LowCertaintyCoBeliever.stages.2.label` | apostate-hearted co-believer | 배교 심성의 동료 신도 |

## DefInjected/RecipeDef

| key | source | translation |
|-----|--------|-------------|
| `EB_WriteIdeobook.label` | write religious book | 종교 서적 작성 |
| `EB_WriteIdeobook.description` | Write a religious book detailing your ideology's beliefs. Requires high certainty in one's ideoligion (at least 90%). | 사상의 신념을 상세히 기술하는 종교 서적을 작성합니다. 자신의 사상에 대한 높은 믿음(최소 90%)이 필요합니다. |
| `EB_WriteIdeobook.jobString` | writing a religious book. | 종교 서적 작성 중. |
| `EB_WriteIllustratedIdeobook.label` | write illustrated religious book | 삽화 종교 서적 작성 |
| `EB_WriteIllustratedIdeobook.description` | Write an illuminated religious book adorned with jems and inks. The quality of materials increases the devotional aspects of reading it. Requires high certainty in one's ideoligion (at least 90%). | 보석과 잉크로 장식된 채식 종교 서적을 작성합니다. 재료의 품질이 읽기의 헌신적 측면을 높입니다. 자신의 사상에 대한 높은 믿음(최소 90%)이 필요합니다. |
| `EB_WriteIllustratedIdeobook.jobString` | illustrating religious book. | 종교 서적 삽화 작업 중. |

## DefInjected/RitualAttachableOutcomeEffectDef

| key | source | translation |
|-----|--------|-------------|
| `EB_BeliefReinforcement.label` | belief reinforcement | 신념 강화 |
| `EB_BeliefReinforcement.effectDesc` | participants' convictions will be reinforced toward orthodoxy. | 참가자들의 확신이 정통을 향해 강화될 것입니다. |
| `EB_BeliefReinforcement.letterInfoText` | The ritual deepened the participants' convictions. | 행사가 참가자들의 확신을 깊어지게 했습니다. |
