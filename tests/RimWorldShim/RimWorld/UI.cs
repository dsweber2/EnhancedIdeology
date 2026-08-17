using Verse;

namespace RimWorld;

public class LetterDef : Def { }

public static class LetterDefOf
{
    public static readonly LetterDef PositiveEvent = new() { defName = "PositiveEvent" };
    public static readonly LetterDef NeutralEvent = new() { defName = "NeutralEvent" };
    public static readonly LetterDef NegativeEvent = new() { defName = "NegativeEvent" };
}

public class MessageTypeDef : Def { }

public static class MessageTypeDefOf
{
    public static readonly MessageTypeDef NeutralEvent = new() { defName = "NeutralEvent" };
    public static readonly MessageTypeDef PositiveEvent = new() { defName = "PositiveEvent" };
    public static readonly MessageTypeDef NegativeEvent = new() { defName = "NegativeEvent" };
    public static readonly MessageTypeDef RejectInput = new() { defName = "RejectInput" };
}

public static class Messages
{
    public static void Message(TaggedString text, Pawn? target, MessageTypeDef? type, bool historical = true) { }
    public static void Message(string text, Pawn? target, MessageTypeDef? type, bool historical = true) { }
}

public static class PawnUtility
{
    public static bool ShouldSendNotificationAbout(Pawn pawn) => false;
}

public static class ThoughtDefOf
{
    public static readonly Verse.ThoughtDef FailedConvertIdeoAttemptResentment = new() { defName = "FailedConvertIdeoAttemptResentment" };
}

public class HistoryEventDef : Def { }

public static class HistoryEventDefOf
{
    public static readonly HistoryEventDef ConvertedNewMember = new() { defName = "ConvertedNewMember" };
}

public static class HistoryEventArgsNames
{
    public const string Doer = "DOER";
    public const string Ideo = "IDEO";
}

public class HistoryEventsManager
{
    public void RecordEvent(HistoryEvent ev) { }
}

public class ShimIdeoManager
{
    public bool classicMode = false;
    public List<Ideo> IdeosListForReading = [];
}

public class DifficultyDef : Def
{
    public float CertaintyReductionFactor(Pawn initiator, Pawn recipient) => 1f;
}

public class ShimStoryteller
{
    public DifficultyDef difficulty = new();
}

public class ShimTickManager
{
    public int TicksGame;
}

public static class Find
{
    public static ShimIdeoManager IdeoManager { get; set; } = new();
    public static ShimTickManager TickManager { get; set; } = new();
    public static ShimStoryteller Storyteller { get; set; } = new();
    public static HistoryEventsManager HistoryEventsManager { get; set; } = new();
}

public static class Current
{
    public static Game Game { get; set; } = null!;
}
