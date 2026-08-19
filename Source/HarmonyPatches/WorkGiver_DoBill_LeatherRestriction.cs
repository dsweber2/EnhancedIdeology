namespace EnhancedIdeology;

internal static class VegetarianUtils
{
    public static bool IsVegetarian(Pawn pawn) =>
        pawn.Ideo?.precepts.Any(p => p.def.defName.StartsWith("MeatEating_NonMeat", StringComparison.Ordinal)) ?? false;

    public static bool IsLeather(ThingDef def) =>
        def.stuffProps?.categories?.Any(c => c.defName == "Leathery") ?? false;

    public static bool HasLeatherIngredient(Thing thing) =>
        thing.TryGetComp<CompIngredients>()?.ingredients.Any(IsLeather) ?? false;
}

[HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
internal static class WorkGiver_DoBill_LeatherRestriction
{
    static void Prefix(Bill bill, Pawn pawn, out List<ThingDef> __state)
    {
        __state = [];

        if (bill.recipe != EnhancedIdeologyDefOf.EB_WriteIdeobook
            && bill.recipe != EnhancedIdeologyDefOf.EB_WriteIllustratedIdeobook) return;

        if (!VegetarianUtils.IsVegetarian(pawn)) return;

        foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
        {
            if (!VegetarianUtils.IsLeather(def)) continue;
            if (!bill.ingredientFilter.Allows(def)) continue;
            bill.ingredientFilter.SetAllow(def, false);
            __state.Add(def);
        }
    }

    static void Postfix(Bill bill, List<ThingDef> __state)
    {
        foreach (var def in __state)
            bill.ingredientFilter.SetAllow(def, true);
    }
}
