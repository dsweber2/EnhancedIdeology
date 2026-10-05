using System.Reflection;

namespace EnhancedIdeology.HarmonyPatches;

// Vanilla Ideology Expanded - Splits and Schisms moves the split-off colonists to the new ideo when its
// dialog closes. The colonists still hold the old ideo in the prefix, so its held ranks are recorded there.
// The postfix carries their orthodox stances to the new ideo, as a reform does.
[HarmonyPatch]
internal static class VIESAS_SchismStances
{
    private static readonly Dictionary<Pawn, Dictionary<IssueDef, float>> OldHeldRanks = [];

    private static bool Prepare() => TargetMethod() != null;

    private static MethodBase? TargetMethod() =>
        AccessTools.TypeByName("VIESAS.Window_ConfigureIdeo") is { } type
            ? AccessTools.Method(type, "Close")
            : null;

    private static void Prefix(List<Pawn> ___colonistsToConvert)
    {
        OldHeldRanks.Clear();
        foreach (var pawn in ___colonistsToConvert)
        {
            if (pawn.Ideo != null)
            {
                OldHeldRanks[pawn] = IdeoReform.HeldRanks(pawn.Ideo);
            }
        }
    }

    private static void Postfix(Ideo ___newIdeo)
    {
        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        foreach (var (pawn, oldHeldRanks) in OldHeldRanks)
        {
            if (pawn.Ideo == ___newIdeo)
            {
                IdeoReform.CarryOrthodoxStances(comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn), ___newIdeo, oldHeldRanks);
            }
        }
        OldHeldRanks.Clear();
    }
}
