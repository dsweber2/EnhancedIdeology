namespace EnhancedIdeology;

[HotSwappable]
internal sealed class ThoughtWorker_ContemplationNeed : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn pawn)
    {
        var need = pawn.needs?.TryGetNeed<Need_Contemplation>();
        if (need == null)
            return ThoughtState.Inactive;

        return need.CurCategory switch
        {
            ContemplationNeedCategory.Critical => ThoughtState.ActiveAtStage(2),
            ContemplationNeedCategory.Low => ThoughtState.ActiveAtStage(1),
            ContemplationNeedCategory.Satisfied when need.CurLevelPercentage > 0.75f => ThoughtState.ActiveAtStage(0),
            _ => ThoughtState.Inactive,
        };
    }
}
