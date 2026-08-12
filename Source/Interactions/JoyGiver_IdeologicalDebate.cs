namespace EnhancedIdeology;

internal sealed class JoyGiver_IdeologicalDebate : JoyGiver_SocialRelax
{
    public override bool CanBeGivenTo(Pawn pawn)
    {
        if (!base.CanBeGivenTo(pawn) || pawn.Ideo == null)
            return false;

        return pawn.Ideo.HasPrecept(EnhancedIdeologyDefOf.IdeoDiversity_Approved)
            || pawn.Ideo.HasPrecept(EnhancedIdeologyDefOf.IdeoDiversity_Respected)
            || pawn.Ideo.HasPrecept(EnhancedIdeologyDefOf.IdeoDiversity_Exalted);
    }
}
