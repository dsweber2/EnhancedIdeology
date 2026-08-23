using System.Reflection.Emit;

namespace EnhancedIdeology.HarmonyPatches;

// Vanilla's inspirational speech outcome directly calls SetIdeo on listeners who roll the 2% conversion
// chance, bypassing our conviction system. The converted pawn keeps stances aligned to their old ideo,
// so certainty immediately drifts away from the new faith — they join a religion they actively dislike.
//
// Fix: replace SetIdeo with our conviction machinery. Pull all Moral stances toward the speaker's ideo
// at conversion-ritual strength (4x), scale step by the listener's opinion of the speaker (Peer Pressure
// compat), apply an opinion-scaled certainty knock, then CheckConversion. The 2% gate is unchanged.
[HarmonyPatch(typeof(RitualOutcomeEffectWorker_Speech), "Apply")]
internal static class RitualOutcomeEffectWorker_Speech_Reroute
{
    private static Pawn? s_organizer;

    [HarmonyPrefix]
    private static void Prefix(LordJob_Ritual jobRitual) => s_organizer = jobRitual.Organizer;

    [HarmonyFinalizer]
    private static void Finalizer() => s_organizer = null;

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator gen)
    {
        var codes = instructions.ToList();
        var setIdeo = AccessTools.Method(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.SetIdeo));
        var handler = AccessTools.Method(
            typeof(RitualOutcomeEffectWorker_Speech_Reroute), nameof(TryConvertWithConviction));

        var setIdeoIdx = codes.FindIndex(c => c.Calls(setIdeo));
        if (setIdeoIdx < 0)
        {
            Log.Error("[EnhancedIdeology] RitualOutcomeEffectWorker_Speech transpiler: SetIdeo call not found — game update?");
            return codes;
        }

        // First stloc after SetIdeo is the text2 store at the end of the "converted listeners" concat.
        var text2StlocIdx = codes.FindIndex(setIdeoIdx + 1, c => c.IsStloc());
        if (text2StlocIdx < 0 || text2StlocIdx + 1 >= codes.Count)
        {
            Log.Error("[EnhancedIdeology] RitualOutcomeEffectWorker_Speech transpiler: text2 stloc not found — game update?");
            return codes;
        }

        var skipLabel = gen.DefineLabel();
        codes[text2StlocIdx + 1].labels.Add(skipLabel);

        // Replace void SetIdeo(Ideo) → bool TryConvertWithConviction(Pawn_IdeoTracker, Ideo).
        // Stack shape is identical (2 args consumed from callsite); handler is a static call.
        codes[setIdeoIdx] = new CodeInstruction(OpCodes.Call, handler);
        codes.Insert(setIdeoIdx + 1, new CodeInstruction(OpCodes.Brfalse, skipLabel));

        return codes;
    }

    // Debug/test entry point: sets organizer context, runs the conviction path, clears context.
    internal static bool ForceApply(Pawn speaker, Pawn listener)
    {
        if (listener.Ideo == null || speaker.Ideo == null || listener.Ideo == speaker.Ideo)
            return false;
        s_organizer = speaker;
        try { return TryConvertWithConviction(listener.ideo, speaker.Ideo); }
        finally { s_organizer = null; }
    }

    private static bool TryConvertWithConviction(Pawn_IdeoTracker ideoTracker, Ideo targetIdeo)
    {
        var listener = ideoTracker.pawn;
        var speaker = s_organizer;
        if (listener == null || speaker == null || listener.Ideo == targetIdeo)
            return false;

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var listenerTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(listener);

        var opinion = listener.relations.OpinionOf(speaker);
        var stepLength = ConvictionMath.RitualBaseArc
            * listener.GetStatValue(StatDefOf.CertaintyLossFactor)
            * Compat_PeerPressure.AdjustStancePull(1f, opinion)
            * ConversionMultiplier;

        foreach (var precept in targetIdeo.precepts)
        {
            var issue = precept.def.issue;
            if (issue == null || PreceptPolicy.CategoryOf(issue) != PreceptCategory.Moral)
                continue;
            var targetRank = IdeoTrackerData.HeldRank(targetIdeo, issue);
            ConvictionMath.ApplyRitualPull(
                comp, listener, issue, targetRank, IdeoTrackerData.AbsoluteMaxConvictionStrength, stepLength);
        }

        var knock = Compat_PeerPressure.AdjustCertaintyKnock(
            EnhancedIdeologyMod.Settings.ConversionCertaintyKnock, opinion);
        listenerTracker.SetExtendedCertainty(listenerTracker.ExtendedCertainty * knock);

        var result = listenerTracker.CheckConversion(targetIdeo, noBreakdown: true) == ConversionOutcome.Success;
        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers,
            $"ThroneSpeech: {listener} (opinion of {speaker}: {opinion}) step={stepLength:F2} knock={knock:F2} → {(result ? "converted" : "influenced, no conversion")}");
        return result;
    }

    private const float ConversionMultiplier = 4f;
}
