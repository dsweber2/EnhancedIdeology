namespace EnhancedIdeology;

[DefOf]
internal static class EnhancedIdeologyDefOf
{
#pragma warning disable CA2211, CS0649 // Ensured by DefOfAttribute
    public static MemeDef Supremacist;
    public static MemeDef Collectivist;
    public static MemeDef Loyalist;
    public static MemeDef Individualist;
    public static MemeDef Proselytizer;
    public static MemeDef Guilty;
    public static MentalStateDef EB_CrisisOfFaith;
    public static MentalStateDef EB_Iconoclast;
    public static MentalStateDef EB_ArrestedDebater;
    public static ThingDef EB_Ideobook;
    public static InspirationDef EB_ReligiousEnlightenment;
    public static JobDef EB_Pray;
    public static JobDef EB_DebateRelax;
    public static ThingDef EB_Mote_ContemplationIcon;
    public static RecipeDef EB_WriteIdeobook;
    public static RecipeDef EB_WriteIllustratedIdeobook;
    public static JobDef EB_PlaceAndBurnUntilDestroyed;
    public static JobDef EB_IconoclastDebate;
    public static JobDef EB_IconoclastAgitate;
    public static ThoughtDef EB_ReligiousBookDestroyed;
    public static ThoughtDef EB_WroteSacrilegousBinding;
    public static ThoughtDef EB_ReadingLeatherboundBook;
    public static ThoughtDef EB_CognitiveDissonance;
    public static ThoughtDef EB_FaithReaffirmed;
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
    public static HistoryEventDef EB_Contemplated;
    [MayRequireIdeology]
    public static NeedDef EB_Contemplation;
    public static EffecterDef EB_CompleteBook;
    public static InteractionDef EB_ConversionLog;
    public static InteractionDef EB_CrisisOfFaithLog;
    public static InteractionDef EB_IdeologicalDebatePrecept;
    public static InteractionDef EB_IdeologicalDebateMeme;
    public static RulePackDef EB_Sentence_DebateWon;
    public static RulePackDef EB_Sentence_InitiatorWon;
    public static RulePackDef EB_Sentence_RecipientWon;
    public static RulePackDef EB_Sentence_DebateDraw;
    // Vanilla Ideology Expanded - Memes and Structures (VanillaExpanded.VMemesE)
    [MayRequire("VanillaExpanded.VMemesE")]
    public static MemeDef VME_Elders;
    [MayRequire("VanillaExpanded.VMemesE")]
    public static MemeDef VME_Gestalt;
    [MayRequire("VanillaExpanded.VMemesE")]
    public static MemeDef VME_Nationalist;
    [MayRequire("VanillaExpanded.VMemesE")]
    public static MemeDef VME_Egalitarian;
    [MayRequire("VanillaExpanded.VMemesE")]
    public static MemeDef VME_Emancipation;
    [MayRequire("VanillaExpanded.VMemesE")]
    public static MemeDef VFEA_Isolationist;
    [MayRequire("VanillaExpanded.VMemesE")]
    public static MemeDef VME_ViolentConversion;
    // Mort's Ideologies: Empiricism and Faith (MortStrudel.MortIdeologySciFai)
    [MayRequire("MortStrudel.MortIdeologySciFai")]
    public static MemeDef MI_Empiricist;
    [MayRequire("MortStrudel.MortIdeologySciFai")]
    public static MemeDef MI_Faith;
    // Mort's Ideologies: Conservationist and Polluter (MortStrudel.MortIdeologyEnv)
    [MayRequire("MortStrudel.MortIdeologyEnv")]
    public static MemeDef MI_Environmentalist;
    [MayRequire("MortStrudel.MortIdeologyEnv")]
    public static MemeDef MI_Industrialist;
    // Mort's Ideologies: Political Compass (MortStrudel.MortIdeology)
    [MayRequire("MortStrudel.MortIdeology")]
    public static MemeDef MI_GovernmentLiberty;
    [MayRequire("MortStrudel.MortIdeology")]
    public static MemeDef MI_GovernmentAuthority;
    [MayRequire("MortStrudel.MortIdeology")]
    public static MemeDef MI_WealthEquality;
    [MayRequire("MortStrudel.MortIdeology")]
    public static MemeDef MI_WealthStratification;
    // Big and Small - Genes & More (RedMattis.BigSmall.Core)
    [MayRequire("RedMattis.BigSmall.Core")]
    public static GeneDef BS_Diet_Herbivore;
    [MayRequire("RedMattis.BigSmall.Core")]
    public static GeneDef BS_Diet_Carnivore;
    // Vanilla Ideology Expanded - Memes and Structures (VanillaExpanded.VMemesE) - Vegan meme
    [MayRequire("VanillaExpanded.VMemesE")]
    public static MemeDef VME_Vegan;
    [MayRequireIdeology]
    public static HediffDef EB_BrainwipeRecovery;
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
