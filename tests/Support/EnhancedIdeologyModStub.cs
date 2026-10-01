namespace EnhancedIdeology;

internal static class EnhancedIdeologyMod
{
    private static readonly SimSettings _settings = new();
    public static SimSettings Settings => _settings;

    [System.Diagnostics.Conditional("DEBUG")]
    public static void DebugIf(bool condition, string message) { }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Debug(string message) { }

    public static void Warning(string msg) => Console.Error.WriteLine($"[EB WARN] {msg}");
    public static void Error(string msg) => Console.Error.WriteLine($"[EB ERROR] {msg}");
    public static void ErrorOnce(string msg, int key) => Error(msg);
    public static void WarningOnce(string msg, int key) { }
    public static void Message(string msg) { }
    public static void DevMessage(string msg) { }
    public static void Exception(string msg, Exception? ex = null)
        => Error(ex != null ? $"{msg}: {ex.Message}" : msg);
}

internal sealed class SimSettings
{
    public bool DebugInteractionWorkers { get; set; } = false;
    public float CertaintyDriftRate { get; set; } = 0.10f;
    public float ConvictionDecayRate { get; set; } = 0.1f;
    public float RelationalMaxRange { get; set; } = 0.25f;
    public float PracticeMaxRange { get; set; } = 0.25f;
    public float ConversionPace { get; set; } = 1f;
    public float ConversionInterval => 3f / ConversionPace;
    public float DebateConvictionChange { get; set; } = 1f;
    public float ConversionStancePull { get; set; } = 2f;
    public float ConversionCertaintyKnock { get; set; } = 0.8f;
    public float CrisisThreshold { get; set; } = 0.25f;
    public float PreceptOppositionScale { get; set; } = 1f;
    public float SaveCompatMinCertainty { get; set; } = 0.125f;
    public float ConversionOpinionMultiplier { get; set; } = 1f;
}
