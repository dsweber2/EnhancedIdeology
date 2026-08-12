namespace UnityEngine;

public static class Mathf
{
    public static float Clamp01(float v) => Math.Clamp(v, 0f, 1f);
    public static float Clamp(float v, float min, float max) => Math.Clamp(v, min, max);
    public static float Abs(float v) => MathF.Abs(v);
    public static float Min(float a, float b) => MathF.Min(a, b);
    public static float Max(float a, float b) => MathF.Max(a, b);
    public static float Sign(float v) => v >= 0 ? 1f : -1f;
    public static float Sqrt(float v) => MathF.Sqrt(v);
    public static float Pow(float b, float p) => MathF.Pow(b, p);
    public static float Exp(float v) => MathF.Exp(v);
    public static float Round(float v) => MathF.Round(v);
    public static int RoundToInt(float v) => (int)MathF.Round(v);
}

public struct Vector3(float x, float y, float z)
{
    public float x = x, y = y, z = z;
    public static readonly Vector3 zero = default;
}

public struct Vector2(float x, float y)
{
    public float x = x, y = y;
}

public struct Rect(float x, float y, float width, float height)
{
    public float x = x, y = y, width = width, height = height;
}

public struct Color(float r, float g, float b, float a = 1f)
{
    public float r = r, g = g, b = b, a = a;
    public static readonly Color white = new(1, 1, 1);
    public static readonly Color clear = new(0, 0, 0, 0);
}
