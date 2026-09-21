// Minimal stubs for EnhancedIdeology types that IdeoTrackerData references but that pull in
// unshimmed Verse.Grammar types (GrammarRequest, GrammarResolver, etc.). The sim never calls
// the full logging logic, so no-op constructors are sufficient.
namespace EnhancedIdeology;

// Stub used by InteractionWorker_IdeologicalDebatePrecept in the simulator. The real class lives
// in Source/Jobs/MentalState_Iconoclast.cs and is not compiled into the sim project; this
// provides the minimal surface the interaction worker needs so tests can exercise the iconoclast
// debate branch by setting Pawn.MentalState to an instance of this stub.
public class MentalState_Iconoclast : Verse.MentalState
{
    public Verse.Pawn? fightTarget;
    public List<Verse.Pawn> debateTargets = [];

    public void ClearDebateTargets() => debateTargets.Clear();

    public void PinDebateTarget(Verse.Pawn p)
    {
        if (!debateTargets.Contains(p)) debateTargets.Add(p);
    }

    public void RemoveDebateTarget(Verse.Pawn p) => debateTargets.Remove(p);
}

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
