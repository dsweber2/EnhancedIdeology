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

#if SIM
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

    // Returns the Peer Pressure opinion label for tooltips, or null if PP is not active.
    internal static string? OpinionTooltipLine(Pawn initiator, Pawn recipient)
    {
        if (_certaintyReductionOpinion == null)
            return null;
        var opinion = recipient.relations.OpinionOf(initiator);
        var factor = _certaintyReductionOpinion(opinion);
        if (factor <= 1f)
            return null;
        return " -  " + "SP_OpinionOf".Translate(
            recipient.Named("PAWN1"),
            initiator.Named("PAWN2"),
            GenText.ToStringPercent(factor).Named("FACTOR"));
    }
#endif

    // Adjusts our certainty-knock multiplier to incorporate Peer Pressure's opinion factor.
    // Peer Pressure multiplies the certainty REDUCTION by ppFactor; we multiply certainty ITSELF
    // by knock, so: adjustedKnock = 1 - (1 - knock) * ppFactor
    internal static float AdjustCertaintyKnock(float knock, int opinion)
    {
        if (_certaintyReductionOpinion == null)
            return knock;
        var ppFactor = _certaintyReductionOpinion(opinion);
        return 1f - (1f - knock) * ppFactor;
    }

    // Scales the stance pull multiplier by PP's opinion factor: liking the preacher makes the belief
    // shift stronger, not just the certainty drop.
    internal static float AdjustStancePull(float pull, int opinion)
    {
        if (_certaintyReductionOpinion == null)
            return pull;
        return pull * _certaintyReductionOpinion(opinion);
    }
}
