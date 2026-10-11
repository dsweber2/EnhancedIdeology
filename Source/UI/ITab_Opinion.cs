namespace EnhancedIdeology;

[HotSwappable]
internal sealed class ITab_Opinion : ITab
{
    public ITab_Opinion()
    {
        labelKey = "EnhancedIdeology.TabOpinion";
    }

    protected override void FillTab()
    {
        var card = new OpinionCard(SelPawn);
        size = card.Size;
        card.Draw();
    }

    public override bool Hidden => !IsVisible;
    public override bool IsVisible => OpinionCard.CanShow(SelPawn);
}
