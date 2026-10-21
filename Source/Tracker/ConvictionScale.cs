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
    // Strength of one trait vote (TraitStanceTargets). See TraitVoteStrength.
    internal const float TraitStrengthPerVote = 8f;
    // Strength that daily decay pulls a stance toward when the traits only oppose rungs and the stance is on none
    // of them. The rank does not move.
    internal const float TraitAllowedStrength = 10f;

    // Diminishing returns of a reinforcing pull as strength approaches the absolute ceiling.
    internal static float StrengthFactor(float strength) =>
        1f - (strength / AbsoluteMaxConvictionStrength);

    // Strength that `votes` trait votes give: 0.5 → 5, 1 → 8, 2 → 12.7, 5 → 23.4. The 2/3 power stops memes from
    // several mods that encode one idea from stacking linearly.
    internal static float TraitVoteStrength(float votes) =>
        TraitStrengthPerVote * Mathf.Pow(votes, 2f / 3f);
}
