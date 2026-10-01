using System.Reflection;
using System.Reflection.Emit;

namespace EnhancedIdeology.HarmonyPatches;

// Vanilla gives the relic lost/destroyed thought to every pawn of each faction that saw the relic, whatever
// their ideo. This transpiler replaces the pawn list with only the followers of the relic's ideo, so that
// both the thoughts and the letter's list of affected colonists are filtered.
[HarmonyPatch(typeof(Precept_Relic), nameof(Precept_Relic.Notify_ThingLost))]
internal static class Precept_Relic_NotifyThingLost
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var allPawns = AccessTools.PropertyGetter(typeof(PawnsFinder), nameof(PawnsFinder.AllMapsAndWorld_Alive));
        var followers = AccessTools.Method(typeof(Precept_Relic_NotifyThingLost), nameof(FollowersOf));

        var codes = instructions.ToList();
        var index = codes.FindIndex(code => code.Calls(allPawns));
        if (index == -1)
        {
            Log.Error("[EnhancedIdeology] Precept_Relic.Notify_ThingLost transpiler: could not find PawnsFinder.AllMapsAndWorld_Alive — vanilla layout may have changed. Relic loss thoughts are not filtered by ideo.");
            return codes;
        }

        var original = codes[index];
        codes[index] = new CodeInstruction(OpCodes.Call, followers);
        codes.Insert(index, new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(original));
        return codes;
    }

    static List<Pawn> FollowersOf(Precept_Relic relic) =>
        PawnsFinder.AllMapsAndWorld_Alive.Where(pawn => pawn.Ideo == relic.ideo).ToList();
}
