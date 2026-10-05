# Source/Rituals/

Ritual outcome effect workers for belief-reinforcement.

- `RitualAttachableOutcomeEffectWorker_BeliefReinforcement.cs` — attachable outcome; pulls all participants' Moral stances via `ConvictionMath.ApplyRitualPull`: toward the faith's rung on a good outcome, and toward `AwayFromFaithRank` on a bad one (orthodox stances leave the rung toward a random ladder end, heterodox ones move out on their own side)
