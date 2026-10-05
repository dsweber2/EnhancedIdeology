# Source/Thoughts/

Custom thought classes and workers. Most read certainty or opinion data indirectly via the ThoughtWorker pattern.

- `ThoughtWorker_IdeologyOpinion.cs` / `Thought_IdeologyOpinion.cs` — social thought expressing ideo approval/disapproval; stage index driven by structural opinion score from `OpinionCache`
- `ThoughtWorker_LowCertaintyCoBeliever_Social.cs` — social thought for co-believers with critically low certainty (concern/disappointment stages)
- `Thought_MemeMemory.cs` — memory thought for positive/negative meme experiences; adds a "caused by" meme label to the description
- `Thought_CognitiveDissonance.cs` — subclass of `Thought_MemeMemory`; mood debuff when a pawn's stance has drifted far from ideo orthodoxy; `TryMergeWithExistingMemory` prevents stacking
- `ThoughtWorker_ContemplationNeed.cs` — mood debuff when `Need_Contemplation` is critically low
- `Thought_ReligiousBookDestroyed.cs` — mood debuff on witnessing a sacred book burned; granted by `CompReligiousBook` on destruction
