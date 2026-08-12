namespace Verse;

public struct CurvePoint(float x, float y)
{
    public float x = x;
    public float y = y;
}

public class SimpleCurve : IList<CurvePoint>
{
    private readonly List<CurvePoint> _pts = [];

    public SimpleCurve() { }
    public SimpleCurve(IEnumerable<CurvePoint> pts) { _pts.AddRange(pts); }

    public float Evaluate(float x)
    {
        if (_pts.Count == 0) return 0f;
        if (x <= _pts[0].x) return _pts[0].y;
        if (x >= _pts[^1].x) return _pts[^1].y;
        for (int ii = 0; ii < _pts.Count - 1; ii++)
        {
            if (x <= _pts[ii + 1].x)
            {
                float t = (x - _pts[ii].x) / (_pts[ii + 1].x - _pts[ii].x);
                return _pts[ii].y + t * (_pts[ii + 1].y - _pts[ii].y);
            }
        }
        return _pts[^1].y;
    }

    public CurvePoint this[int index] { get => _pts[index]; set => _pts[index] = value; }
    public int Count => _pts.Count;
    public bool IsReadOnly => false;
    public void Add(CurvePoint item) => _pts.Add(item);
    public void Clear() => _pts.Clear();
    public bool Contains(CurvePoint item) => _pts.Contains(item);
    public void CopyTo(CurvePoint[] array, int arrayIndex) => _pts.CopyTo(array, arrayIndex);
    public IEnumerator<CurvePoint> GetEnumerator() => _pts.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _pts.GetEnumerator();
    public int IndexOf(CurvePoint item) => _pts.IndexOf(item);
    public void Insert(int index, CurvePoint item) => _pts.Insert(index, item);
    public bool Remove(CurvePoint item) => _pts.Remove(item);
    public void RemoveAt(int index) => _pts.RemoveAt(index);
}

public readonly struct TaggedString(string? value)
{
    private readonly string _value = value ?? string.Empty;
    public string Resolve() => _value;
    public static implicit operator string(TaggedString ts) => ts._value;
    public static implicit operator TaggedString(string? s) => new(s);
    public static TaggedString operator +(TaggedString a, string b) => new(a._value + b);
    public static TaggedString operator +(string a, TaggedString b) => new(a + b._value);
    public override string ToString() => _value;
}

public readonly struct NamedArgument(object? value, string tag)
{
    public readonly object? Value = value;
    public readonly string Tag = tag;
    public static implicit operator NamedArgument(string s) => new(s, string.Empty);
    public static implicit operator NamedArgument(TaggedString ts) => new((string)ts, string.Empty);
}
