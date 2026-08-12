using UnityEngine;

namespace Verse;

public struct LookTargets
{
    public LookTargets(params Pawn[] pawns) { }
}

public class RulePackDef : Def { }

public static class RulePackDefOf
{
    public static RulePackDef Sentence_ConvertIdeoAttemptSuccess = new() { defName = "Sentence_ConvertIdeoAttemptSuccess" };
    public static RulePackDef Sentence_ConvertIdeoAttemptFail = new() { defName = "Sentence_ConvertIdeoAttemptFail" };
    public static RulePackDef Sentence_ConvertIdeoAttemptFailResentment = new() { defName = "Sentence_ConvertIdeoAttemptFailResentment" };
    public static RulePackDef Sentence_ConvertIdeoAttemptFailSocialFight = new() { defName = "Sentence_ConvertIdeoAttemptFailSocialFight" };
}

public readonly struct HistoryEvent(RimWorld.HistoryEventDef? def, params NamedArgument[] args) { }

public static class MoteMaker
{
    public static void ThrowText(Vector3 pos, Map? map, string text, float duration = 3f) { }
}

public static class Extensions
{
    public static bool NullOrEmpty<T>(this IEnumerable<T>? source)
        => source == null || !source.Any();

    public static bool NullOrEmpty(this string? s) => string.IsNullOrEmpty(s);

    public static void SortBy<T, K>(this List<T> list, Func<T, K> keySelector)
        where K : IComparable<K>
        => list.Sort((a, b) => keySelector(a).CompareTo(keySelector(b)));

    public static T RandomElement<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        if (list.Count == 0) throw new InvalidOperationException("Empty sequence");
        return list[Rand.RangeInclusive(0, list.Count - 1)];
    }

    public static IEnumerable<T> InRandomOrder<T>(this IEnumerable<T> source)
        => source.OrderBy(_ => Rand.Value);

    public static T RandomElementByWeight<T>(this IEnumerable<T> source, Func<T, float> weightSelector)
    {
        var list = source.ToList();
        if (list.Count == 0) throw new InvalidOperationException("Empty sequence");
        var total = list.Sum(weightSelector);
        var roll = Rand.Value * total;
        foreach (var item in list)
        {
            roll -= weightSelector(item);
            if (roll <= 0f) return item;
        }
        return list[^1];
    }

    public static TaggedString Translate(this string key) => new(key);

    public static TaggedString Translate(this string key, params NamedArgument[] args) => new(key);

    public static NamedArgument Named(this object? obj, string tag) => new(obj, tag);

    public static string ToStringPercent(this float value) => $"{value:P0}";
}
