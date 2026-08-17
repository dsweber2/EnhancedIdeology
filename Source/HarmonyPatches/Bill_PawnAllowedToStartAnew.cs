using Verse.AI;

namespace EnhancedIdeology;

[HarmonyPatch(typeof(Bill), nameof(Bill.PawnAllowedToStartAnew))]
internal static class Bill_PawnAllowedToStartAnew_CertaintyGate
{
    private const float MinCertainty = 0.9f;

    public static void Postfix(Bill __instance, Pawn p, ref bool __result)
    {
        if (!__result) return;
        if (__instance is not Bill_Production bill) return;
        if (bill.recipe != EnhancedIdeologyDefOf.EB_WriteIdeobook
            && bill.recipe != EnhancedIdeologyDefOf.EB_WriteIllustratedIdeobook) return;

        if (p.Ideo == null || p.ideo.Certainty < MinCertainty)
        {
            JobFailReason.Is("EnhancedIdeology.InsufficientCertaintyToWrite".Translate(), bill.Label);
            __result = false;
        }
    }
}
