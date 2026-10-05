namespace EnhancedIdeology;

// Domain-wide conviction strength constants shared across Books, UI, Rituals, Contemplation, and
// HarmonyPatches. Centralised here so callers do not need to import IssueStanceTracker.
internal static class ConvictionScale
{
    internal const float BaseConvictionMin = 5f;
    internal const float BaseConvictionMax = 25f;
    // Lower bound used in Mathf.Clamp when writing stance strength.
    internal const float MinConvictionStrength = 0f;
    // Normal play ceiling; strengths above this are possible but exceptional.
    public const float MaxConvictionStrength = 20f;
    // Hard ceiling enforced by all stance-write paths.
    public const float AbsoluteMaxConvictionStrength = 50f;
    // Target strength of a pull away from the faith's rung (bad moods, bad rituals): a firm, not a fanatical, dissent.
    internal const float AwayFromFaithStrength = 15f;
    internal const float ConvictionPerTraitDegree = 3f;
    internal const float TraitMemeConvictionBonus = 10f;
}
