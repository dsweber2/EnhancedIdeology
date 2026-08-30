namespace EnhancedIdeology;

internal sealed class InspirationWorker_ReligiousEnlightenment : InspirationWorker
{
    public override bool InspirationCanOccur(Pawn pawn)
    {
        if (!base.InspirationCanOccur(pawn))
            return false;

        if (pawn.Ideo == null || pawn.Map == null || !pawn.Position.IsValid)
            return false;

        return MeetsThresholds(pawn);
    }

    public override float CommonalityFor(Pawn pawn)
    {
        if (!MeetsThresholds(pawn))
            return 0f;

        var tracker = Current.Game.GetComponent<GameComponent_EnhancedIdeology>()
            .PawnTracker.TryGetIdeoTracker(pawn);
        var excessCertainty = (tracker?.ExtendedCertainty ?? 0f) - 1f;
        var avgSkill = AverageSkill(pawn) - 5f;
        return excessCertainty * avgSkill;
    }

    private static bool MeetsThresholds(Pawn pawn)
    {
        var tracker = Current.Game.GetComponent<GameComponent_EnhancedIdeology>()
            .PawnTracker.TryGetIdeoTracker(pawn);
        if (tracker == null || tracker.ExtendedCertainty <= 1f)
            return false;

        return AverageSkill(pawn) > 5f;
    }

    private static float AverageSkill(Pawn pawn)
    {
        if (pawn.skills == null)
            return 0f;
        var social = pawn.skills.GetSkill(SkillDefOf.Social).Level;
        var intellectual = pawn.skills.GetSkill(SkillDefOf.Intellectual).Level;
        return (social + intellectual) / 2f;
    }
}
