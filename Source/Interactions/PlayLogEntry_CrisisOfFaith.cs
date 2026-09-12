using Verse.Grammar;

namespace EnhancedIdeology;

internal enum CrisisOutcome { Wander, MoodBreak }

internal sealed class PlayLogEntry_CrisisOfFaith : PlayLogEntry_InteractionSinglePawn
{
    private CrisisOutcome outcome;

    public PlayLogEntry_CrisisOfFaith() { }

    public PlayLogEntry_CrisisOfFaith(Pawn pawn, CrisisOutcome outcome)
        : base(EnhancedIdeologyDefOf.EB_CrisisOfFaithLog, pawn, null)
    {
        this.outcome = outcome;
    }

    public override Color? IconColorFromPOV(Thing pov) => initiatorIdeo?.Color;

    protected override string ToGameStringFromPOV_Worker(Thing pov, bool forceLog)
    {
        if (initiator == null)
            return "[crisis of faith log error: null pawn]";

        Rand.PushState();
        Rand.Seed = logID;
        GrammarRequest request = GenerateGrammarRequest();
        request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiator, request.Constants));

        string key = outcome == CrisisOutcome.MoodBreak
            ? "EnhancedIdeology.CrisisLog.MoodBreak"
            : "EnhancedIdeology.CrisisLog.Wander";

        request.Rules.Add(new Rule_String("r_logentry", key.Translate()));
        if (LanguageDatabase.defaultLanguage.TryGetTextFromKey(key, out var english))
            request.Rules.Add(new Rule_String("r_logentry_en", english));

        var text = GrammarResolver.Resolve("r_logentry", request, "crisis of faith", forceLog, "r_logentry_en");
        Rand.PopState();
        return text;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref outcome, "outcome");
    }
}
