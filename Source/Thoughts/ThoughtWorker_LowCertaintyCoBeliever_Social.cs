namespace EnhancedIdeology;

// A pawn from a strict-apostacy faith looks down on co-believers who hold their shared faith with less
// certainty than they do. The opinion penalty scales with the certainty gap.
internal sealed class ThoughtWorker_LowCertaintyCoBeliever_Social : ThoughtWorker_Precept_Social
{
    private const float MinGap = 0.10f;
    private const float MidGap = 0.25f;
    private const float HighGap = 0.40f;

    protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        if (p.Ideo == null || otherPawn.Ideo == null || p.Ideo != otherPawn.Ideo)
            return ThoughtState.Inactive;
        if (p.ideo == null || otherPawn.ideo == null)
            return ThoughtState.Inactive;

        var gap = p.ideo.Certainty - otherPawn.ideo.Certainty;
        if (gap < MinGap) return ThoughtState.Inactive;
        if (gap < MidGap) return ThoughtState.ActiveAtStage(0);
        if (gap < HighGap) return ThoughtState.ActiveAtStage(1);
        return ThoughtState.ActiveAtStage(2);
    }
}
