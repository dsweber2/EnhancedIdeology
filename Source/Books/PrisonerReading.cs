namespace EnhancedIdeology;

// Pacing for prisoners without a recreation need, who read religious books from JobGiver_PrisonerReadReligiousText.
// A less certain prisoner spends more of the day reading.
internal static class PrisonerReading
{
    public const float MinShare = 0.25f;
    public const float MaxShare = 0.9f;

    public static float ShareOfDay(float certainty) => Mathf.Clamp(1f - certainty, MinShare, MaxShare);

    // Ticks from the start of one reading session to the earliest start of the next, so that a session of
    // readingTicks fills the share of the time.
    public static int CycleTicks(float certainty, int readingTicks) =>
        Mathf.CeilToInt(readingTicks / ShareOfDay(certainty));
}
