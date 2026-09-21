using Verse.AI;

namespace EnhancedIdeology;

internal sealed class MentalStateWorker_Iconoclast : MentalStateWorker
{
    public override bool StateCanOccur(Pawn pawn)
    {
        if (!base.StateCanOccur(pawn)) return false;
        return MentalState_Iconoclast.FindClosestReachable(pawn, t => t is BookIdeo b && b.Ideo == pawn.Ideo) != null
            || MentalState_Iconoclast.FindClosestReachable(pawn, t => t.IsRelic() || t.TryGetComp<CompRelicContainer>() != null) != null
            || MentalState_Iconoclast.FindClosestReachable(pawn, t => MentalState_Iconoclast.IsIdeoligicalStructure(t)) != null;
    }
}
