using System.Text;
using Verse.Grammar;

using static RimWorld.IdeoFoundation_Deity;

#if v1_5
using PlanetTile = int;
#else
using RimWorld.Planet;
#endif

namespace EnhancedIdeology;

internal sealed class BookIdeo : Book
{
    private ReadingOutcomeDoer_CertaintyChange? doer;
    public ReadingOutcomeDoer_CertaintyChange Doer
    {
        get
        {
            if (doer == null)
            {
                var comp = GetComp<CompBook>();

                foreach (var doer in comp.doers)
                {
                    if (doer is ReadingOutcomeDoer_CertaintyChange change)
                    {
                        this.doer = change;
                        break;
                    }
                }
            }

            return (doer) ?? throw new InvalidOperationException(
                "Tried to get Doer on a EnhancedIdeology.BookIdeo without a ReadingOutcomeDoer_CertaintyChange. This should not happen.");
        }
    }

    public Ideo? Ideo
    {
        get => Doer?.ideo;
        set
        {
            if (Doer == null)
            {
                EnhancedIdeologyMod.Error("Tried to set Ideo on a book without a ReadingOutcomeDoer_CertaintyChange. This should not happen.");
                return;
            }
            Doer.ideo = value;
        }
    }

    // Set during recipe completion to override quality with (Intellectual+Social)/2 average
    private Pawn? _recipeWorker;
    private bool _applyingCustomQuality;

    public float MaterialBonus => Doer.MaterialBonus;

    // Curve input: sum of base market values of unique ingredient ThingDefs used.
    // Plain (cloth+wood ~2.6) → 1.0x; illustrated jade (~43) → ~1.17x;
    // illustrated silver (~78) → ~1.3x; illustrated gold (~303) → 2.0x.
    // Sum of base market values of unique ingredient ThingDefs used in crafting.
    // cloth+steel ≈ 2.0 → 1.05×; cloth+plasteel ≈ 8.0 → 1.1×;
    // illustrated jade ≈ 42.5 → 1.15×; silver ≈ 77.5 → 1.3×; gold ≈ 302.5 → 2.0×.
    private static readonly SimpleCurve materialBonusFromIngredientValue =
    [
        new CurvePoint(0f, 1.0f),
        new CurvePoint(2f, 1.05f),
        new CurvePoint(9f, 1.1f),
        new CurvePoint(42f, 1.15f),
        new CurvePoint(78f, 1.3f),
        new CurvePoint(300f, 2.0f),
    ];

    public override void Notify_RecipeProduced(Pawn worker)
    {
        base.Notify_RecipeProduced(worker);
        _recipeWorker = worker;
    }

    public override void PostQualitySet()
    {
        if (_applyingCustomQuality) return;

        if (_recipeWorker != null)
        {
            var worker = _recipeWorker;
            _recipeWorker = null;

            var intellectLevel = worker.skills?.GetSkill(SkillDefOf.Intellectual)?.Level ?? 0;
            var socialLevel = worker.skills?.GetSkill(SkillDefOf.Social)?.Level ?? 0;

            _applyingCustomQuality = true;
            this.TryGetComp<CompQuality>()?.SetQuality(
                QualityUtility.GenerateQualityCreatedByPawn((intellectLevel + socialLevel) / 2, false),
                ArtGenerationContext.Colony);
            _applyingCustomQuality = false;

            var ingredients = this.TryGetComp<CompIngredients>();
            if (ingredients?.ingredients is { Count: > 0 } list)
            {
                var totalValue = list.Sum(t => t.GetStatValueAbstract(StatDefOf.MarketValue));
                Doer.SetMaterialBonus(materialBonusFromIngredientValue.Evaluate(totalValue));
            }

            if (VegetarianUtils.IsVegetarian(worker) && VegetarianUtils.HasLeatherIngredient(this))
                worker.needs?.mood?.thoughts.memories.TryGainMemory(EnhancedIdeologyDefOf.EB_WroteSacrilegousBinding);

            Ideo = worker.Ideo;
            GenerateBook(worker, GenTicks.TicksAbs);
            return;
        }

        base.PostQualitySet();
        GenerateSpawnedIngredients();
    }

    private void GenerateSpawnedIngredients()
    {
        var comp = this.TryGetComp<CompIngredients>();
        if (comp == null) return;

        static ThingDef? GetDef(string name) => DefDatabase<ThingDef>.GetNamedSilentFail(name);

        var quality = (int)(this.TryGetComp<CompQuality>()?.Quality ?? QualityCategory.Normal);

        ThingDef pages = quality >= 5 ? (GetDef("Synthread") ?? GetDef("Cloth") ?? ThingDefOf.WoodLog)
                       : quality >= 2 ? (GetDef("Cloth") ?? ThingDefOf.WoodLog)
                       :                ThingDefOf.WoodLog;
        comp.ingredients.Add(pages);

        ThingDef cover = quality >= 5 ? (GetDef("Plasteel") ?? ThingDefOf.Steel)
                       : quality <= 1 ? (GetDef("Leather_Plain") ?? ThingDefOf.Steel)
                       :                ThingDefOf.Steel;
        comp.ingredients.Add(cover);

        if (quality >= 3 && Rand.Value < (quality - 2) * 0.2f)
        {
            ThingDef? precious = quality >= 6 ? (GetDef("Gold") ?? GetDef("Silver") ?? GetDef("Jade"))
                               : quality >= 5 ? (GetDef("Silver") ?? GetDef("Jade"))
                               :                GetDef("Jade");
            if (precious != null)
                comp.ingredients.Add(precious);
        }

        var totalValue = comp.ingredients.Sum(t => t.GetStatValueAbstract(StatDefOf.MarketValue));
        Doer.SetMaterialBonus(materialBonusFromIngredientValue.Evaluate(totalValue));
    }

    public override string DescriptionDetailed => base.DescriptionDetailed + BuildStanceText();

    public override void GenerateBook(Pawn? author = null, long? fixedDate = null)
    {
        base.GenerateBook(author, fixedDate);

        if (Ideo != null)
        {
            RegenerateName(Ideo);
        }
    }

    // Ensure that traders get their book ideo
    public override void PostGeneratedForTrader(TraderKindDef trader, PlanetTile forTile, Faction forFaction)
    {
        base.PostGeneratedForTrader(trader, forTile, forFaction);

        Ideo ??= forFaction == null || forFaction.ideos == null
                ? Find.IdeoManager.IdeosListForReading.RandomElement()
                : forFaction.ideos.PrimaryIdeo;

        RegenerateName(Ideo);
    }

    // Checks for null ideos in case something goes wrong
    public override void TickRare()
    {
        base.TickRare();

        if (Ideo == null)
        {
            Ideo = Find.IdeoManager.IdeosListForReading.RandomElement();
            RegenerateName(Ideo);
        }
    }

    private string BuildStanceText()
    {
        var doer = GetComp<CompBook>().Doers.OfType<ReadingOutcomeDoer_CertaintyChange>().FirstOrDefault();
        if (doer?.ideo == null) return string.Empty;

        var stances = doer.IdeoStances().ToList();
        if (stances.Count == 0) return string.Empty;

        var gainPerQuadrum = doer.CertaintyGain() * ReadingOutcomeDoer_CertaintyChange.TypicalReadingTicksPerQuadrum;
        var sb = new StringBuilder("\n\n");
        sb.AppendLine("EnhancedIdeology.BookBeliefsHeader".Translate());
        foreach (var (issue, stance, strength) in stances)
        {
            var shiftPerQuadrum = (gainPerQuadrum * strength / IdeoTrackerData.MaxConvictionStrength).ToString("0.##", CultureInfo.InvariantCulture) + "/qd";
            sb.AppendLine($"  - {issue.LabelCap}: {stance.LabelCap} ({shiftPerQuadrum})");
        }
        return sb.ToString().TrimEndNewlines();
    }

    //Completely copied over from ideo generation code, also generates description
    // TODO: Consider using a reverse transpiler to avoid code duplication
    private void RegenerateName(Ideo ideo)
    {
        var request = default(GrammarRequest);
        request.Includes.Add(ideo.culture.ideoNameMaker);
        var foundation = ideo.foundation;
        var foundationDeity = foundation as IdeoFoundation_Deity;
        foundation.AddPlaceRules(ref request);
        foundationDeity?.AddDeityRules(ref request);
        List<SymbolSource> list = [];
        if (ideo.memes.Any(m => !m.symbolPacks.NullOrEmpty()))
        {
            list.Add(SymbolSource.Pack);
        }
        if (foundationDeity != null && foundationDeity.deities.Count >= 1 && !ideo.memes.Any(m => !m.allowSymbolsFromDeity))
        {
            list.Add(SymbolSource.Deity);
        }
        if (list.Count == 0)
        {
            return;
        }
        switch (list.RandomElementByWeight(s => s == SymbolSource.Pack ? 1f : 0.5f))
        {
            case SymbolSource.Pack:
                SetupFromSymbolPack(ideo);
                break;
            case SymbolSource.Deity:
                SetupFromDeity(ideo);
                break;
            default:
                break;
        }
        title = GenText.CapitalizeAsTitle(GrammarResolver.Resolve("r_ideoName", request, null, false, null, null, null, true));

        var patterns = (from entry in ideo.memes.Where(meme => meme.descriptionMaker?.patterns != null).SelectMany(meme => meme.descriptionMaker.patterns)
                        group entry by entry.def into grp
                        select grp.MaxBy(entry => entry.weight)).ToList();
        if (!list.Any())
        {
            return;
        }

        var def = patterns.RandomElementByWeight(entry => entry.weight).def;
        descriptionFlavor = IdeoDescriptionUtility.ResolveDescription(Ideo, def, true).text;

        var topStances = Doer.IdeoStances().Take(3).ToList();
        if (topStances.Count > 0)
        {
            var stanceList = topStances.Select(s => s.stance.LabelCap.ToString()).ToCommaList(useAnd: true);
            descriptionFlavor += "\n\n" + "EnhancedIdeology.BookFocalStances".Translate(stanceList);
        }

        description = GenerateFullDescription();

        void AddMemeContent(Ideo ideo)
        {
            foreach (var item in ideo.memes)
            {
                if (item.generalRules != null)
                {
                    request.IncludesBare.Add(item.generalRules);
                }
            }
        }

        void AddSymbolPack(IdeoSymbolPack pack, MemeCategory memeCategory)
        {
            request.Constants.SetOrAdd("forcePrefix", pack.prefix.ToString());
            var text = pack.prefix ? (GrammarResolver.Resolve("hyphenPrefix", request) + "-") : string.Empty;
            if (pack.ideoName != null)
            {
                if (memeCategory == MemeCategory.Structure)
                {
                    request.Rules.Add(new Rule_String("packIdeoNameStructure", text + pack.ideoName));
                }
                else
                {
                    request.Rules.Add(new Rule_String("packIdeoName", text + pack.ideoName));
                }
            }
            if (pack.theme != null)
            {
                request.Rules.Add(new Rule_String("packTheme", pack.theme));
            }
            if (pack.adjective != null)
            {
                request.Rules.Add(new Rule_String("packAdjective", text + pack.adjective));
            }
            if (pack.member != null)
            {
                request.Rules.Add(new Rule_String("packMember", text + pack.member));
            }
        }

        void SetupFromDeity(Ideo ideo)
        {
            request.Rules.Add(new Rule_String("keyDeity", ideo.KeyDeityName));
            AddMemeContent(ideo);
        }

        void SetupFromSymbolPack(Ideo ideo)
        {
            MemeDef result;
            if (ideo.StructureMeme.symbolPackOverride)
            {
                result = ideo.StructureMeme;
            }
            else if (!ideo.memes.Where(m => m.symbolPacks.HasData() && m.symbolPacks.Any()).TryRandomElement(out result))
            {
                result = ideo.memes.Where(m => m.symbolPacks.HasData()).RandomElement();
            }
            AddMemeContent(ideo);
            if (result.symbolPacks.TryRandomElement(out var result2))
            {
                AddSymbolPack(result2, result.category);
            }
            else
            {
                AddSymbolPack(result.symbolPacks.RandomElement(), result.category);
            }
        }
    }
}
