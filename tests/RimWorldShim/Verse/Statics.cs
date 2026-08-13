namespace Verse;
using UnityEngine;

public static class Rand
{
    private static System.Random _rng = new(42);

    public static void SetSeed(int seed) => _rng = new System.Random(seed);

    public static float Value => (float)_rng.NextDouble();

    public static float Range(float min, float max) => min + Value * (max - min);

    public static int RangeInclusive(int min, int max) => _rng.Next(min, max + 1);

    public static bool Chance(float probability) => Value < probability;

    public static float Gaussian(float centerX = 0f, float widthFactor = 1f)
    {
        float value = Value;
        float value2 = Value;
        return (float)System.Math.Sqrt(-2f * (float)System.Math.Log(value)) * (float)System.Math.Sin((float)System.Math.PI * 2f * value2) * widthFactor + centerX;
    }
}

public static class GenDate
{
    public const int TicksPerDay = 60000;
    public const int DaysPerYear = 60;
    public const int TicksPerYear = TicksPerDay * DaysPerYear;
}

public static class GenTicks
{
    public const int TickRareInterval = 250;
    public const int TickLongInterval = 2000;
    public const int TicksPerRealSecond = 60;
}

public static class Prefs
{
    public static bool DevMode => false;
}

public static class Log
{
    public static void Message(string msg) => Console.Error.WriteLine($"[LOG] {msg}");
    public static void Warning(string msg) => Console.Error.WriteLine($"[WARN] {msg}");
    public static void Error(string msg) => Console.Error.WriteLine($"[ERROR] {msg}");
    public static void ResetMessageCount() { }
}

public static class ModsConfig
{
    public static bool IdeologyActive => true;
    public static bool RoyaltyActive => false;
    public static bool BiotechActive => false;
    public static bool AnomalyActive => false;
}

public static class ModLister
{
    public static bool CheckIdeology(string reason) => true;
}

public static class GenCollection
{
    public static T? MaxByWithFallback<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
        where TKey : IComparable<TKey>
    {
        T? best = default;
        TKey? bestKey = default;
        var first = true;
        foreach (var item in source)
        {
            var key = selector(item);
            if (first || key.CompareTo(bestKey!) > 0)
            {
                best = item;
                bestKey = key;
                first = false;
            }
        }

        return best;
    }
}

public static class PawnsFinder
{
    private static List<Pawn> _allPawns = [];

    public static IEnumerable<Pawn> All_AliveOrDead => _allPawns;

    public static void SetPawns(IEnumerable<Pawn> pawns) => _allPawns = pawns.ToList();
}
