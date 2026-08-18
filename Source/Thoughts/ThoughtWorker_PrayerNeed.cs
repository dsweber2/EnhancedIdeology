namespace EnhancedIdeology;

[HotSwappable]
internal sealed class ThoughtWorker_PrayerNeed : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn pawn)
    {
        var need = pawn.needs?.TryGetNeed<Need_Prayer>();
        if (need == null)
            return ThoughtState.Inactive;

        return need.CurCategory switch
        {
            PrayerNeedCategory.Critical => ThoughtState.ActiveAtStage(2),
            PrayerNeedCategory.Low => ThoughtState.ActiveAtStage(1),
            PrayerNeedCategory.Satisfied when need.CurLevelPercentage > 0.75f => ThoughtState.ActiveAtStage(0),
            _ => ThoughtState.Inactive,
        };
    }
}
