namespace EnhancedIdeology;

[HotSwappable]
internal sealed class Need_Prayer : Need
{
    // Drains to empty in one game day; moral guide drains twice as fast.
    private const float BaseFallPerInterval = 1f / 400f;

    private const float ThreshLow = 0.25f;
    private const float ThreshCritical = 0.1f;

    public PrayerNeedCategory CurCategory
    {
        get
        {
            if (CurLevelPercentage <= ThreshCritical)
                return PrayerNeedCategory.Critical;
            if (CurLevelPercentage <= ThreshLow)
                return PrayerNeedCategory.Low;
            return PrayerNeedCategory.Satisfied;
        }
    }

    public Need_Prayer(Pawn pawn) : base(pawn)
    {
        threshPercents = [ThreshCritical, ThreshLow, 0.75f];
    }

    public override int GUIChangeArrow
    {
        get
        {
            if (IsFrozen) return 0;
            return pawn.jobs?.curDriver is JobDriver_Pray ? 1 : -1;
        }
    }

    public override void NeedInterval()
    {
        if (IsFrozen)
            return;
        var rate = IsMoralGuide() ? BaseFallPerInterval * 2f : BaseFallPerInterval;
        CurLevel = Mathf.Max(0f, CurLevel - rate);
    }

    public void Satisfy(float amount = 1f)
    {
        CurLevel = Mathf.Min(MaxLevel, CurLevel + amount);
    }

    private bool IsMoralGuide() =>
        pawn.Ideo?.GetRole(pawn)?.def == PreceptDefOf.IdeoRole_Moralist;
}

internal enum PrayerNeedCategory
{
    Satisfied,
    Low,
    Critical,
}
