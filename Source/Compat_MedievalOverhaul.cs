namespace EnhancedIdeology;

// With Medieval Overhaul, Patches.xml makes the pages of religious books from its paper only.
// The translated page summary still lists the vanilla materials, because DefInjected sets it after
// the patches. Clear it, so that the filter shows the label of the paper instead.
[StaticConstructorOnStartup]
internal static class Compat_MedievalOverhaul
{
    static Compat_MedievalOverhaul()
    {
        if (DefDatabase<ThingDef>.GetNamedSilentFail("DankPyon_Paper") == null) return;
        EnhancedIdeologyDefOf.EB_WriteIdeobook.ingredients[0].filter.customSummary = null;
        EnhancedIdeologyDefOf.EB_WriteIllustratedIdeobook.ingredients[0].filter.customSummary = null;
    }
}
