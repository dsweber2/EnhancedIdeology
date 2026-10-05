using RimWorld;
using Verse;
using Verse.AI;

namespace EnhancedIdeology;

internal sealed class MentalBreakWorker_Iconoclast : MentalBreakWorker
{
    public override float CommonalityFor(Pawn pawn, bool moodCaused = false)
    {
        if (Find.IdeoManager.classicMode) return 0f;
        // A small congregation gives the pawn nothing to rebel against.
        if (pawn.Ideo is not { } ideo || ideo.ColonistBelieverCountCached < Ideo.MinBelieversToEnableObligations) return 0f;
        var certaintyFactor = Mathf.Max(0f, 1f - GetCertainty(pawn));
        return base.CommonalityFor(pawn, moodCaused) * certaintyFactor;
    }

    private static float GetCertainty(Pawn pawn)
    {
        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var tracker = comp?.PawnTracker?.EnsurePawnHasIdeoTracker(pawn);
        return tracker?.ExtendedCertainty ?? 1f;
    }
}
