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

        var template = outcome == CrisisOutcome.MoodBreak
            ? "[INITIATOR_nameDef]'s crisis of faith compounded [INITIATOR_possessive] misery."
            : "[INITIATOR_nameDef] experienced a crisis of faith.";

        request.Rules.Add(new Rule_String("r_logentry", template));
        var text = GrammarResolver.Resolve("r_logentry", request, "crisis of faith", forceLog);
        Rand.PopState();
        return text;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref outcome, "outcome");
    }
}
