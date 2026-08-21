using Verse.Grammar;

namespace EnhancedIdeology;

internal sealed class PlayLogEntry_Conversion : PlayLogEntry_InteractionSinglePawn
{
    private Ideo? newIdeo;

    public PlayLogEntry_Conversion() { }

    public PlayLogEntry_Conversion(Pawn pawn, Ideo newIdeo)
        : base(EnhancedIdeologyDefOf.EB_ConversionLog, pawn, null)
    {
        this.newIdeo = newIdeo;
    }

    public override Color? IconColorFromPOV(Thing pov) => initiatorIdeo?.Color;

    protected override string ToGameStringFromPOV_Worker(Thing pov, bool forceLog)
    {
        if (initiator == null)
            return "[conversion log error: null pawn]";

        Rand.PushState();
        Rand.Seed = logID;
        GrammarRequest request = GenerateGrammarRequest();
        request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiator, request.Constants));

        string template;
        if (newIdeo != null)
        {
            request.Rules.Add(new Rule_String("IDEO_name", newIdeo.name));
            template = "[INITIATOR_nameDef] converted to [IDEO_name].";
        }
        else
        {
            template = "[INITIATOR_nameDef] converted to a new faith.";
        }

        request.Rules.Add(new Rule_String("r_logentry", template));
        var text = GrammarResolver.Resolve("r_logentry", request, "conversion", forceLog);
        Rand.PopState();
        return text;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref newIdeo, "newIdeo");
    }
}
