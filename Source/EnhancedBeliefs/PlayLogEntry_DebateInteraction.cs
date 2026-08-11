using Verse.Grammar;

namespace EnhancedBeliefs;

internal sealed class PlayLogEntry_DebateInteraction : PlayLogEntry_Interaction
{
    private IssueDef? debateTopic;
    private Pawn? debateWinner;
    private string? winnerPreceptLabel;

    public PlayLogEntry_DebateInteraction() { }

    public PlayLogEntry_DebateInteraction(
        InteractionDef intDef, Pawn initiator, Pawn recipient,
        List<RulePackDef> extraSentencePacks, IssueDef? topic, Pawn? winner, string? winnerPrecept)
        : base(intDef, initiator, recipient, extraSentencePacks)
    {
        debateTopic = topic;
        debateWinner = winner;
        winnerPreceptLabel = winnerPrecept;
    }

    protected override string ToGameStringFromPOV_Worker(Thing pov, bool forceLog)
    {
        if (initiator == null || recipient == null)
        {
            Log.ErrorOnce("PlayLogEntry_DebateInteraction has a null pawn reference.", 34423);
            return "[" + intDef.label + " error: null pawn reference]";
        }

        Rand.PushState();
        Rand.Seed = logID;
        var request = BuildRequest();
        string text;

        if (pov == initiator)
        {
            request.IncludesBare.Add(intDef.logRulesInitiator);
            AddPawnRules(ref request);
            text = GrammarResolver.Resolve("r_logentry", request, "interaction from initiator", forceLog);
        }
        else if (pov == recipient)
        {
            request.IncludesBare.Add(intDef.logRulesRecipient ?? intDef.logRulesInitiator);
            AddPawnRules(ref request);
            text = GrammarResolver.Resolve("r_logentry", request, "interaction from recipient", forceLog);
        }
        else
        {
            Log.ErrorOnce("Cannot display PlayLogEntry_DebateInteraction from POV who isn't initiator or recipient.", 51253);
            Rand.PopState();
            return ToString();
        }

        if (extraSentencePacks != null)
        {
            foreach (var pack in extraSentencePacks)
            {
                request.Clear();
                request.Includes.Add(pack);
                // Re-inject on each sentence pack — request.Clear() wipes Constants too.
                InjectDebateSymbols(ref request);
                AddPawnRules(ref request);
                text += " " + GrammarResolver.Resolve(pack.FirstRuleKeyword, request, "extraSentencePack", forceLog, pack.FirstUntranslatedRuleKeyword);
            }
        }

        Rand.PopState();
        return text;
    }

    private GrammarRequest BuildRequest()
    {
        GrammarRequest request = default;
        InjectDebateSymbols(ref request);
        return request;
    }

    public override Color? IconColorFromPOV(Thing pov) => initiatorIdeo?.Color;

    private void InjectDebateSymbols(ref GrammarRequest request)
    {
        if (debateTopic != null)
            request.Rules.Add(new Rule_String("TOPIC_label", debateTopic.label));
        if (debateWinner != null)
            request.Rules.AddRange(GrammarUtility.RulesForPawn("WINNER", debateWinner, request.Constants));
    }

    private void AddPawnRules(ref GrammarRequest request)
    {
        request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiator, request.Constants));
        request.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", recipient, request.Constants));
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref debateTopic, "debateTopic");
        Scribe_References.Look(ref debateWinner, "debateWinner", saveDestroyedThings: true);
    }
}
