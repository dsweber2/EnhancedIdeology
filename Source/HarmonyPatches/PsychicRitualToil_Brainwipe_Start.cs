using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace EnhancedIdeology;

[HarmonyPatch(typeof(PsychicRitualToil_Brainwipe), nameof(PsychicRitualToil_Brainwipe.Start))]
static class PsychicRitualToil_Brainwipe_Start
{
    static void Postfix(PsychicRitual psychicRitual, PsychicRitualToil_Brainwipe __instance)
    {
        if (!ModsConfig.IdeologyActive) return;

        var target = psychicRitual.assignments.FirstAssignedPawn(__instance.targetRole);
        if (target == null) return;

        Current.Game.GetComponent<GameComponent_EnhancedIdeology>()
            .PawnTracker?.TryGetIdeoTracker(target)
            ?.ApplyBrainwipe();

        target.health.AddHediff(HediffMaker.MakeHediff(EnhancedIdeologyDefOf.EB_BrainwipeRecovery, target));
    }
}
