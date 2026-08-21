// Minimal stubs for EnhancedIdeology types that IdeoTrackerData references but that pull in
// unshimmed Verse.Grammar types (GrammarRequest, GrammarResolver, etc.). The sim never calls
// the full logging logic, so no-op constructors are sufficient.
namespace EnhancedIdeology;

public enum CrisisOutcome { Wander, MoodBreak }

public sealed class PlayLogEntry_CrisisOfFaith
{
    public PlayLogEntry_CrisisOfFaith() { }
    public PlayLogEntry_CrisisOfFaith(Verse.Pawn pawn, CrisisOutcome outcome) { }
}

public sealed class PlayLogEntry_Conversion
{
    public PlayLogEntry_Conversion() { }
    public PlayLogEntry_Conversion(Verse.Pawn pawn, RimWorld.Ideo newIdeo) { }
}
