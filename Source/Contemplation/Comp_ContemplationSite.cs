namespace EnhancedIdeology;

internal sealed class CompProperties_ContemplationSite : CompProperties
{
    public string contemplationGainLabel = "1×";
    public string contemplationGainReport = "EB_ContemplationSiteStatReport";

    public CompProperties_ContemplationSite() => compClass = typeof(Comp_ContemplationSite);
}

internal sealed class Comp_ContemplationSite : ThingComp
{
    private CompProperties_ContemplationSite Props => (CompProperties_ContemplationSite)props;

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
        yield return new StatDrawEntry(
            StatCategoryDefOf.Building,
            "EB_ContemplationSiteStatLabel".Translate(),
            Props.contemplationGainLabel,
            Props.contemplationGainReport.Translate(),
            970);
    }

    public override string? CompInspectStringExtra() =>
        "EB_ContemplationSiteInspect".Translate(Props.contemplationGainLabel);
}
