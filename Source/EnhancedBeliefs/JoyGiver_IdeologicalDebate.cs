namespace EnhancedBeliefs;

internal sealed class JoyGiver_IdeologicalDebate : JoyGiver_SocialRelax
{
    public override bool CanBeGivenTo(Pawn pawn)
    {
        if (!base.CanBeGivenTo(pawn) || pawn.Ideo == null)
            return false;

        return pawn.Ideo.HasPrecept(EnhancedBeliefsDefOf.IdeoDiversity_Approved)
            || pawn.Ideo.HasPrecept(EnhancedBeliefsDefOf.IdeoDiversity_Respected)
            || pawn.Ideo.HasPrecept(EnhancedBeliefsDefOf.IdeoDiversity_Exalted);
    }
}
