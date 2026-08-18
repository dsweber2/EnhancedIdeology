namespace EnhancedIdeology;

internal sealed class CompProperties_PrayerSite : CompProperties
{
    public string prayerGainLabel = "1×";
    public string prayerGainReport = "EB_PrayerSiteStatReport";

    public CompProperties_PrayerSite() => compClass = typeof(Comp_PrayerSite);
}

internal sealed class Comp_PrayerSite : ThingComp
{
    private CompProperties_PrayerSite Props => (CompProperties_PrayerSite)props;

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
        yield return new StatDrawEntry(
            StatCategoryDefOf.Building,
            "EB_PrayerSiteStatLabel".Translate(),
            Props.prayerGainLabel,
            Props.prayerGainReport.Translate(),
            970);
    }

    public override string? CompInspectStringExtra() =>
        "EB_PrayerSiteInspect".Translate(Props.prayerGainLabel);
}
