namespace EnhancedIdeology;

// Reads of CertaintyLossFactor (docs/design.md "Certainty loss factor"). Vanilla does not clamp the stat, and mods
// that stack negative offsets push it to zero or below. Zero or less means the pawn is immune to persuasion.
internal static class CertaintyLoss
{
    // Lowest factor that still speeds up a gain. Below it, resistance gives no more speed-up (4x cap).
    internal const float ResistanceFloor = 0.25f;

    // The stat, clamped at zero. Use this for every multiplicative read.
    internal static float Factor(Pawn pawn) => Mathf.Max(0f, pawn.GetStatValue(StatDefOf.CertaintyLossFactor));

    // Multiplier for the entrench gain after a drawn debate. A resistant pawn hardens more, up to 1 / ResistanceFloor.
    internal static float Resistance(float factor) => 1f / Mathf.Max(factor, ResistanceFloor);

    // The part of the stat that comes from traits and genes, in the same order as vanilla's StatWorker. Race base
    // values, conditional stat affecters and all other sources (ranks, hediffs, precepts, gear) are not included.
    internal static float InnateFactor(Pawn pawn)
    {
        var stat = StatDefOf.CertaintyLossFactor;
        var traits = pawn.story?.traits.allTraits;
        var genes = pawn.genes?.GenesListForReading;

        var value = stat.defaultBaseValue;
        if (traits != null)
        {
            foreach (var trait in traits)
            {
                if (!trait.Suppressed) value += trait.OffsetOfStat(stat);
            }
        }
        if (genes != null)
        {
            foreach (var gene in genes)
            {
                if (gene.Active) value += gene.def.statOffsets.GetStatOffsetFromList(stat);
            }
        }
        if (traits != null)
        {
            foreach (var trait in traits)
            {
                if (!trait.Suppressed) value *= trait.MultiplierOfStat(stat);
            }
        }
        if (genes != null)
        {
            foreach (var gene in genes)
            {
                if (gene.Active) value *= gene.def.statFactors.GetStatFactorFromList(stat);
            }
        }

        return Mathf.Max(0f, value);
    }

    // Multiplier for the daily walk toward the pawn's trait stance targets. Innate resistance speeds the walk up
    // (1 / innate, capped by ResistanceFloor). Resistance from other sources slows it down by factor / innate. The
    // product is factor / innate². A pawn with factor 0 does not move toward its trait targets.
    internal static float InnatePull(float factor, float innate) =>
        factor / Mathf.Pow(Mathf.Max(innate, ResistanceFloor), 2f);

    // Chance that a debate this pawn loses becomes a draw instead. It rises from 0 at ResistanceFloor to 1 at
    // factor 0, because a pawn that cannot be moved does not lose. The draw then gives a fight or entrenchment.
    internal static float UnmovedChance(float factor) => Mathf.Clamp01(1f - (factor / ResistanceFloor));
}
