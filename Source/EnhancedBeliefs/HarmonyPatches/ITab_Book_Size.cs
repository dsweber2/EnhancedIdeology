using System.Reflection;

namespace EnhancedBeliefs.HarmonyPatches;

[HarmonyPatch(typeof(ITab_Book), MethodType.Constructor)]
internal static class ITab_Book_Size
{
    private const float Height = 520f;

    private static readonly FieldInfo SizeField =
        typeof(InspectTabBase).GetField("size", BindingFlags.NonPublic | BindingFlags.Instance)!;

    static void Postfix(ITab_Book __instance)
    {
        var current = (Vector2)SizeField.GetValue(__instance);
        SizeField.SetValue(__instance, new Vector2(current.x, Height));
    }
}
