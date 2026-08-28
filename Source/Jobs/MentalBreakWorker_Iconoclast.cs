using RimWorld;
using Verse;
using Verse.AI;

namespace EnhancedIdeology;

internal sealed class MentalBreakWorker_Iconoclast : MentalBreakWorker
{
    public override float CommonalityFor(Pawn pawn, bool moodCaused = false)
    {
        var certainty = GetCertainty(pawn);
        var factor = Mathf.Max(0f, 1f - certainty);
        return base.CommonalityFor(pawn, moodCaused) * factor / 0.55f;
    }

    private static float GetCertainty(Pawn pawn)
    {
        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var tracker = comp?.PawnTracker?.EnsurePawnHasIdeoTracker(pawn);
        return tracker?.ExtendedCertainty ?? 1f;
    }
}
