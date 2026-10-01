namespace HarmonyLib;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HarmonyPatchAttribute : Attribute
{
    public HarmonyPatchAttribute() { }
    public HarmonyPatchAttribute(Type declaringType) { }
    public HarmonyPatchAttribute(Type declaringType, string methodName) { }
    public HarmonyPatchAttribute(Type declaringType, string methodName, MethodType methodType) { }
    public HarmonyPatchAttribute(Type declaringType, Type[] argumentTypes) { }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class HarmonyPrefixAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public sealed class HarmonyPostfixAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public sealed class HarmonyTranspilerAttribute : Attribute { }

public enum MethodType { Normal, Getter, Setter, Constructor, StaticConstructor }
