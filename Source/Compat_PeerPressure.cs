namespace EnhancedIdeology;

// Peer Pressure (joseasoler.peerpressure) multiplies certainty reduction and conversion-selection chance
// by a factor derived from the recipient's opinion of the initiator. Our conversion flow never calls
// InteractionWorker_ConvertIdeoAttempt.CertaintyReduction, so Peer Pressure's Postfix on that method
// is silently skipped. We replicate the effect by adjusting our certainty knock directly.
//
// ConversionSelectionFactor is NOT overridden in InteractionWorker_AdvancedConversionAttempt, so
// Peer Pressure's Postfix on the base class already fires correctly for that part.
internal static class Compat_PeerPressure
{
    public const string PackageId = "joseasoler.peerpressure";

    private static Func<int, float>? _certaintyReductionOpinion;

    internal static bool Active => _certaintyReductionOpinion != null;

    // Test-only injection point — lets unit tests exercise the math without a real PP assembly.
    internal static void SetForTest(Func<int, float>? fn) => _certaintyReductionOpinion = fn;

#if TESTS
    internal static void Initialize() { }

    internal static string? OpinionTooltipLine(Pawn initiator, Pawn recipient) => null;
#else
    internal static void Initialize()
    {
        if (ModLister.GetActiveModWithIdentifier(PackageId) == null)
            return;

        var type = AccessTools.TypeByName("PeerPressure.Multiplier");
        var method = type != null ? AccessTools.Method(type, "CertaintyReductionOpinion") : null;
        if (method == null)
        {
            EnhancedIdeologyMod.Warning("Peer Pressure detected but CertaintyReductionOpinion method not found — skipping compatibility.");
            return;
        }

        _certaintyReductionOpinion = (Func<int, float>)Delegate.CreateDelegate(typeof(Func<int, float>), method);
        EnhancedIdeologyMod.Message("Peer Pressure compatibility active.");
    }

    // Returns the opinion amplification label for tooltips, or null if the factor is ≤1.
    internal static string? OpinionTooltipLine(Pawn initiator, Pawn recipient)
    {
        var opinion = recipient.relations.OpinionOf(initiator);
        var factor = OpinionFactor(opinion);
        if (factor <= 1f)
            return null;
        return " -  " + "SP_OpinionOf".Translate(
            recipient.Named("PAWN1"),
            initiator.Named("PAWN2"),
            GenText.ToStringPercent(factor).Named("FACTOR"));
    }
#endif

    // Returns the opinion amplification factor: PP's function when active, otherwise our native
    // formula (1 + 0.01 * opinion * multiplier, clamped to ≥1). Mirrors PP's default formula
    // exactly at the default multiplier of 1.0.
    private static float OpinionFactor(int opinion)
        => _certaintyReductionOpinion != null
            ? _certaintyReductionOpinion(opinion)
            : NativeFactor(opinion);

    private static float NativeFactor(int opinion)
    {
        var multiplier = EnhancedIdeologyMod.Settings.ConversionOpinionMultiplier;
        return opinion > 0 ? 1f + 0.01f * opinion * multiplier : 1f;
    }

    // Adjusts our certainty-knock multiplier to incorporate the opinion factor.
    // PP multiplies the certainty REDUCTION by factor; we multiply certainty ITSELF
    // by knock, so: adjustedKnock = 1 - (1 - knock) * factor
    internal static float AdjustCertaintyKnock(float knock, int opinion)
    {
        var factor = OpinionFactor(opinion);
        return 1f - (1f - knock) * factor;
    }

    // Scales the stance pull by the opinion factor: liking the converter shifts beliefs harder.
    internal static float AdjustStancePull(float pull, int opinion)
        => pull * OpinionFactor(opinion);
}
