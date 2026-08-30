using HarmonyLib;

namespace EnhancedIdeology;

[HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
internal static class GenRecipe_MakeRecipeProducts_QualityBoost
{
    [HarmonyPostfix]
    static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker)
    {
        bool isIdeobookRecipe = recipeDef == EnhancedIdeologyDefOf.EB_WriteIdeobook
                             || recipeDef == EnhancedIdeologyDefOf.EB_WriteIllustratedIdeobook;
        bool hasEnlightenment = worker?.InspirationDef == EnhancedIdeologyDefOf.EB_ReligiousEnlightenment;

        foreach (var thing in __result)
        {
            if (isIdeobookRecipe && hasEnlightenment)
            {
                var quality = thing.TryGetComp<CompQuality>();
                if (quality != null)
                {
                    var boosted = (QualityCategory)Math.Min((int)quality.Quality + 2, (int)QualityCategory.Legendary);
                    quality.SetQuality(boosted, ArtGenerationContext.Colony);
                }
            }
            yield return thing;
        }

        if (isIdeobookRecipe && hasEnlightenment)
            worker!.mindState.inspirationHandler.EndInspiration(EnhancedIdeologyDefOf.EB_ReligiousEnlightenment);
    }
}
