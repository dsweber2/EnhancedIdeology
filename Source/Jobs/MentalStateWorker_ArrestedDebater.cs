using Verse.AI;

namespace EnhancedIdeology;

internal sealed class MentalStateWorker_ArrestedDebater : MentalStateWorker
{
    // Only started programmatically from MentalState_Iconoclast. Bypass base checks —
    // the pawn may be unspawned (mid-carry) and is by definition a prisoner.
    public override bool StateCanOccur(Pawn pawn) => pawn.IsPrisonerOfColony;
}
