using System.Reflection;
using System.Reflection.Emit;

namespace EnhancedIdeology.HarmonyPatches;

// Replace vanilla's certainty-offset conversion logic with our stance-based system.
// The transpiler surgically removes the ideoCertaintyOffset if/else block (SetIdeo / OffsetCertainty)
// and replaces it with a call to ApplyConversionRitual, which does the 4x stance pull + CheckConversion.
// Everything else in Apply — quality calculation, the letter, development points, participant memories — is
// untouched.
[HarmonyPatch(typeof(RitualOutcomeEffectWorker_Conversion), "Apply")]
internal static class RitualOutcomeEffectWorker_Conversion_Reroute
{
    private static readonly MethodInfo HandlerMethod =
        AccessTools.Method(typeof(RitualOutcomeEffectWorker_Conversion_Reroute), nameof(ApplyConversionRitual));

    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();

        var ideoCertaintyOffsetField = AccessTools.Field(
            typeof(RitualOutcomePossibility), nameof(RitualOutcomePossibility.ideoCertaintyOffset));
        var offsetCertainty = AccessTools.Method(
            typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.OffsetCertainty));

        if (ideoCertaintyOffsetField == null || offsetCertainty == null)
        {
            Log.Error("[EnhancedIdeology] RitualOutcomeEffectWorker_Conversion transpiler: could not resolve vanilla members (ideoCertaintyOffset or OffsetCertainty missing — game update?). Patch skipped.");
            return codes;
        }

        int startIdx = -1, endIdx = -1;
        for (var ii = 0; ii < codes.Count; ii++)
        {
            if (startIdx == -1 && codes[ii].LoadsField(ideoCertaintyOffsetField))
                startIdx = ii - 1; // step back to the ldloc outcome preceding the callvirt
            if (codes[ii].Calls(offsetCertainty))
            {
                endIdx = ii;
                break;
            }
        }

        if (startIdx == -1 || endIdx == -1)
        {
            Log.Error("[EnhancedIdeology] RitualOutcomeEffectWorker_Conversion transpiler: could not find target range — vanilla Apply layout may have changed.");
            return codes;
        }

        // Any labels on instructions inside the removed range that are referenced by branches we're
        // also removing can be dropped. Transfer labels from [startIdx+1, endIdx] to keep IL valid
        // in case the JIT happens to care; attach them to our inserted call.
        var orphanedLabels = codes
            .Skip(startIdx + 1)
            .Take(endIdx - startIdx)
            .SelectMany(c => c.labels)
            .ToList();

        // Replacement: ldarg.3 (jobRitual), [ldloc outcome — reuse startIdx instruction], ldarg.2 (totalPresence), call handler.
        var outcomeLoad = codes[startIdx].Clone();
        var callInstruction = new CodeInstruction(OpCodes.Call, HandlerMethod);
        callInstruction.labels.AddRange(orphanedLabels);

        codes.RemoveRange(startIdx, endIdx - startIdx + 1);
        codes.InsertRange(startIdx, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_3),  // LordJob_Ritual jobRitual
            outcomeLoad,                            // RitualOutcomePossibility outcome
            new CodeInstruction(OpCodes.Ldarg_2),  // Dictionary<Pawn, int> totalPresence
            callInstruction,
        });

        return codes;
    }

    // Called in place of the vanilla SetIdeo / OffsetCertainty block.
    // Positive outcome: pulls the convertee 4x toward the moralist's ideo orthodoxy on every Moral issue,
    // then fires a conversion attempt with the moralist's ideo as the priority candidate.
    // Negative outcome: standard 1x erosion on the convertee (moves away from orthodoxy, strength → 0).
    private static void ApplyConversionRitual(
        LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, Dictionary<Pawn, int> totalPresence)
    {
        if (outcome.positivityIndex == 0)
        {
            return;
        }

        var moralist = jobRitual.PawnWithRole("moralist");
        var convertee = jobRitual.PawnWithRole("convertee");
        if (convertee == null || !convertee.RaceProps.Humanlike)
        {
            return;
        }

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var ritualIdeo = moralist?.Ideo ?? jobRitual.Ritual?.ideo;
        if (ritualIdeo == null)
        {
            return;
        }

        var baseStep = ConvictionMath.RitualBaseArc
            * Math.Abs(outcome.positivityIndex)
            * convertee.GetStatValue(StatDefOf.CertaintyLossFactor);

        var preceptSource = outcome.positivityIndex > 0 ? ritualIdeo.precepts : convertee.Ideo?.precepts;
        if (preceptSource == null)
        {
            return;
        }

        foreach (var precept in preceptSource)
        {
            var issue = precept.def.issue;
            if (issue == null || PreceptPolicy.CategoryOf(issue) != PreceptCategory.Moral)
            {
                continue;
            }

            float targetRank, targetStrength;
            if (outcome.positivityIndex > 0)
            {
                targetRank = IdeoTrackerData.HeldRank(ritualIdeo, issue);
                targetStrength = IdeoTrackerData.AbsoluteMaxConvictionStrength;
            }
            else
            {
                targetRank = ConvictionMath.LadderExtremeAwayFrom(issue, IdeoTrackerData.HeldRank(convertee.Ideo!, issue));
                targetStrength = 0f;
            }

            ConvictionMath.ApplyRitualPull(comp, convertee, issue, targetRank, targetStrength, baseStep * ConversionRitualMultiplier);
        }

        if (outcome.positivityIndex > 0)
        {
            var converteeTracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(convertee);
            converteeTracker.CheckConversion(ritualIdeo, noBreakdown: true);
        }
    }

    private const float ConversionRitualMultiplier = 4f;
}
