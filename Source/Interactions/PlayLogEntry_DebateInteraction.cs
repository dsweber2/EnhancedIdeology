using Verse.Grammar;

namespace EnhancedIdeology;

internal sealed class PlayLogEntry_DebateInteraction : PlayLogEntry_Interaction
{
    private IssueDef? debateTopic;
    private MemeDef? debateMemeTopic;
    private Pawn? debateWinner;
    private string? winnerPreceptLabel;

    public PlayLogEntry_DebateInteraction() { }

    public PlayLogEntry_DebateInteraction(
        InteractionDef intDef, Pawn initiator, Pawn recipient,
        List<RulePackDef> extraSentencePacks, Def? topic, Pawn? winner, string? winnerLabel)
        : base(intDef, initiator, recipient, extraSentencePacks)
    {
        debateTopic = topic as IssueDef;
        debateMemeTopic = topic as MemeDef;
        debateWinner = winner;
        winnerPreceptLabel = winnerLabel;
    }

    // The entry for a debate the worker just resolved, or null when intDef is not a debate or no topic was found.
    // Consumes the worker's logTopic so that the next interaction cannot log it again.
    internal static PlayLogEntry_DebateInteraction? FromLastDebate(
        InteractionDef intDef, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
    {
        switch (intDef.Worker)
        {
            case InteractionWorker_IdeologicalDebatePrecept { logTopic: { } topic } worker:
                worker.logTopic = null;
                return new(intDef, initiator, recipient, extraSentencePacks, topic, worker.lastWinner, worker.lastWinnerPrecept?.label);
            case InteractionWorker_IdeologicalDebateMeme { logTopic: { } topic } worker:
                worker.logTopic = null;
                return new(intDef, initiator, recipient, extraSentencePacks, topic, worker.lastWinner, topic.label);
            default:
                return null;
        }
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
            // Inject r_logentry directly so logRulesInitiator (used by vanilla/Interaction Bubbles
            // without our custom symbols) doesn't compete with the rich topic-aware version.
            InjectLogentryRules(ref request);
            AddPawnRules(ref request);
            text = GrammarResolver.Resolve("r_logentry", request, "interaction from initiator", forceLog, "r_logentry_en");
        }
        else if (pov == recipient)
        {
            InjectLogentryRules(ref request);
            AddPawnRules(ref request);
            text = GrammarResolver.Resolve("r_logentry", request, "interaction from recipient", forceLog, "r_logentry_en");
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
                // Re-inject after Clear() — wipes Constants too.
                InjectDebateSymbols(ref request);
                AddPawnRules(ref request);
                // Inject rich sentence rule directly; XML packs have simplified grammar for
                // the vanilla/Interaction Bubbles path which lacks our custom symbols.
                InjectSentenceRules(ref request, pack);
                text += " " + GrammarResolver.Resolve(pack.FirstRuleKeyword, request, "extraSentencePack", forceLog,
                    SentKeyForPack(pack) != null ? "sent_en" : pack.FirstUntranslatedRuleKeyword);
            }
        }

        Rand.PopState();
        return text;
    }

    private void InjectLogentryRules(ref GrammarRequest request)
    {
        string key = RlogentryKey();
        request.Rules.Add(new Rule_String("r_logentry", key.Translate()));
        // English fallback: Translate() returns obfuscated text in dev mode for untranslated keys,
        // which garbles the [bracket] tokens. The default-language lookup always returns clean text.
        if (LanguageDatabase.defaultLanguage.TryGetTextFromKey(key, out var english))
            request.Rules.Add(new Rule_String("r_logentry_en", english));
    }

    private static void InjectSentenceRules(ref GrammarRequest request, RulePackDef pack)
    {
        string? key = SentKeyForPack(pack);
        if (key == null)
        {
            request.Rules.Add(new Rule_String(pack.FirstRuleKeyword,
                pack.RulesImmediate?.FirstOrDefault()?.Generate() ?? string.Empty));
            return;
        }
        request.Rules.Add(new Rule_String(pack.FirstRuleKeyword, key.Translate()));
        if (LanguageDatabase.defaultLanguage.TryGetTextFromKey(key, out var english))
            request.Rules.Add(new Rule_String("sent_en", english));
    }

    private string RlogentryKey()
    {
        if (debateMemeTopic != null) return "EnhancedIdeology.DebateLog.AboutMeme";
        if (debateTopic != null) return "EnhancedIdeology.DebateLog.AboutPrecept";
        return "EnhancedIdeology.DebateLog.Generic";
    }

    private static string? SentKeyForPack(RulePackDef pack)
    {
        if (pack == EnhancedIdeologyDefOf.EB_Sentence_InitiatorWon)
            return "EnhancedIdeology.DebateSent.InitiatorMoved";
        if (pack == EnhancedIdeologyDefOf.EB_Sentence_RecipientWon)
            return "EnhancedIdeology.DebateSent.RecipientMoved";
        if (pack == EnhancedIdeologyDefOf.EB_Sentence_DebateWon)
            return "EnhancedIdeology.DebateSent.WinnerPersuasive";
        if (pack == EnhancedIdeologyDefOf.EB_Sentence_DebateDraw)
            return "EnhancedIdeology.DebateSent.Draw";
        return null;
    }

    private GrammarRequest BuildRequest()
    {
        GrammarRequest request = default;
        InjectDebateSymbols(ref request);
        return request;
    }

    public override Texture2D IconFromPOV(Thing pov) =>
        debateTopic?.Icon ?? debateMemeTopic?.Icon ?? base.IconFromPOV(pov);

    public override Color? IconColorFromPOV(Thing pov) =>
        debateMemeTopic != null ? initiatorIdeo?.Color : null;

    private void InjectDebateSymbols(ref GrammarRequest request)
    {
        var topicLabel = debateTopic?.label ?? debateMemeTopic?.label ?? "a precept";
        request.Rules.Add(new Rule_String("TOPIC_label", topicLabel));
        if (debateWinner != null)
            request.Rules.AddRange(GrammarUtility.RulesForPawn("WINNER", debateWinner, request.Constants));
        request.Rules.Add(new Rule_String("WINNING_STANCE_label", winnerPreceptLabel ?? "the issue"));
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
        Scribe_Defs.Look(ref debateMemeTopic, "debateMemeTopic");
        Scribe_References.Look(ref debateWinner, "debateWinner", saveDestroyedThings: true);
        Scribe_Values.Look(ref winnerPreceptLabel, "winnerPreceptLabel");
    }
}
