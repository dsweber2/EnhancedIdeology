using Verse.AI;

namespace EnhancedIdeology;

[HotSwappable]
internal sealed class JobGiver_PrayFromNeed : ThinkNode_JobGiver
{
    protected override Job? TryGiveJob(Pawn pawn)
    {
        if (pawn.Ideo == null || pawn.Map == null)
            return null;
        var need = pawn.needs?.TryGetNeed<Need_Prayer>();
        if (need?.CurCategory != PrayerNeedCategory.Critical)
            return null;
        if (!MeditationUtility.CanMeditateNow(pawn))
            return null;
        return JoyGiver_Prayer.TryBuildPrayJob(pawn);
    }
}
