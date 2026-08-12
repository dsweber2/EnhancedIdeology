namespace EnhancedIdeology.HarmonyPatches;

[HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.GetBackCompatibleType))]
internal static class BackCompatibility_TypeMigration
{
    private static void Postfix(string typeName, ref Type? __result)
    {
        if (__result is not null || !typeName.StartsWith("EnhancedBeliefs.", StringComparison.Ordinal))
            return;

        var newName = "EnhancedIdeology." + typeName["EnhancedBeliefs.".Length..];
        __result = typeof(EnhancedIdeologyMod).Assembly.GetType(newName);
    }
}
