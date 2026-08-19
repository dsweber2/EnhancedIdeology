using Verse.AI;

namespace EnhancedIdeology;

[HotSwappable]
internal sealed class JobGiver_PrayFromNeed : ThinkNode_JobGiver
{
    protected override Job? TryGiveJob(Pawn pawn)
    {
        if (pawn.Ideo == null || pawn.Map == null)
            return null;
        var need = pawn.needs?.TryGetNeed<Need_Contemplation>();
        if (need?.CurCategory != ContemplationNeedCategory.Critical)
            return null;
        if (!MeditationUtility.CanMeditateNow(pawn))
            return null;
        return JoyGiver_Contemplation.TryBuildPrayJob(pawn);
    }
}
