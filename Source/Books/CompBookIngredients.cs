using System.Text;

namespace EnhancedIdeology;

internal sealed class CompBookIngredients : CompIngredients
{
    public override string CompInspectStringExtra()
    {
        if (ingredients.Count == 0) return string.Empty;
        var sb = new StringBuilder("Ingredients".Translate() + ": ");
        for (int ii = 0; ii < ingredients.Count; ii++)
        {
            sb.Append(ii == 0 ? ingredients[ii].LabelCap.Resolve() : ingredients[ii].label);
            if (ii < ingredients.Count - 1) sb.Append(", ");
        }
        return sb.ToString();
    }
}

internal sealed class CompProperties_BookIngredients : CompProperties_Ingredients
{
    public CompProperties_BookIngredients() => compClass = typeof(CompBookIngredients);
}
