namespace EnhancedIdeology;

internal sealed class ThoughtWorker_IdeologyOpinion : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
    {
        return !Find.IdeoManager.classicMode
            && p.Ideo != null
            && otherPawn.Ideo != null
            && Find.World != null;
    }
}
