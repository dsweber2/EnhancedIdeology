namespace Verse;

public abstract class Def
{
    public string defName = string.Empty;
    public string? label;
    public string LabelCap => string.IsNullOrEmpty(label)
        ? defName
        : char.ToUpperInvariant(label[0]) + label[1..];

    private List<DefModExtension>? modExtensions;

    public T? GetModExtension<T>() where T : DefModExtension
        => (T?)modExtensions?.FirstOrDefault(e => e is T);

    public void AddModExtension(DefModExtension ext)
    {
        modExtensions ??= [];
        modExtensions.Add(ext);
    }

    public override string ToString() => defName;
}

public abstract class DefModExtension { }

public static class DefOfHelper
{
    public static void EnsureInitializedInCtor(object _) { }
}

public static class DefDatabase<T> where T : Def
{
    private static readonly List<T> _defs = [];
    public static int DefCount => _defs.Count;
    public static IEnumerable<T> AllDefs => _defs;
    public static void Add(T def) => _defs.Add(def);
    public static void Clear() => _defs.Clear();

    public static T? GetNamedSilentFail(string defName)
    {
        foreach (var def in _defs)
        {
            if (def.defName == defName)
            {
                return def;
            }
        }

        return null;
    }
}

public class IssueDef : Def { }

public class ThingDef : Def { }

public class JobDef : Def { }

public class RecipeDef : Def { }

public class EffecterDef : Def { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class DefOfAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field)]
public sealed class MayRequireIdeologyAttribute : Attribute { }
