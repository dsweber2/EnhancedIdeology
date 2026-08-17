namespace EnhancedIdeology;

[DefOf]
internal static class EnhancedIdeologyDefOf
{
#pragma warning disable CA2211, CS0649 // Ensured by DefOfAttribute
    public static MemeDef Supremacist;
    public static MemeDef Loyalist;
    public static MemeDef Proselytizer;
    public static MemeDef Guilty;
    public static MentalStateDef EB_CrisisOfFaith;
    public static ThingDef EB_UnfinishedIdeobook;
    public static ThingDef EB_Ideobook;
    public static JobDef EB_Pray;
    public static ThingDef EB_Mote_PrayerIcon;
    public static JobDef EB_CompleteReligiousBook;
    public static RecipeDef EB_WriteIdeobook;
    public static RecipeDef EB_WriteIllustratedIdeobook;
    public static JobDef EB_PlaceAndBurnUntilDestroyed;
    public static ThoughtDef EB_ReligiousBookDestroyed;
    public static ThoughtDef EB_WroteSacrilegousBinding;
    public static ThoughtDef EB_ReadingLeatherboundBook;
    public static ThoughtDef EB_GoodDebate;
    public static ThoughtDef EB_BadDebate;
    [MayRequireIdeology]
    public static ThoughtDef EB_ApostacyDebated;
    [MayRequireIdeology]
    public static ThoughtDef EB_LowCertaintyCoBeliever;
    [MayRequireIdeology]
    public static ThoughtDef EB_ProselytizerDebated;
    [MayRequireIdeology]
    public static ThoughtDef EB_ProselytizerConverted;
    [MayRequireIdeology]
    public static ThoughtDef EB_ProselytizerFailedConversion;
    public static HistoryEventDef EB_DestroyedReligiousBook;
    public static HistoryEventDef EB_BookDestroyed;
    public static EffecterDef EB_CompleteBook;
    public static InteractionDef EB_IdeologicalDebatePrecept;
    public static InteractionDef EB_IdeologicalDebateMeme;
    public static RulePackDef EB_Sentence_DebateWon;
    public static RulePackDef EB_Sentence_InitiatorWon;
    public static RulePackDef EB_Sentence_RecipientWon;
    public static RulePackDef EB_Sentence_DebateDraw;
    [MayRequireIdeology]
    public static PreceptDef IdeoDiversity_Approved;
    [MayRequireIdeology]
    public static PreceptDef IdeoDiversity_Respected;
    [MayRequireIdeology]
    public static PreceptDef IdeoDiversity_Exalted;
#pragma warning restore CA2211, CS0649

#pragma warning disable CS8618 // Set by RimWorld
    static EnhancedIdeologyDefOf()
#pragma warning restore CS8618
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(EnhancedIdeologyDefOf));
    }
}
