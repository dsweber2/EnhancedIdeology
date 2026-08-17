namespace RimWorld;

public enum QualityCategory
{
    Awful = 0, Poor = 1, Normal = 2, Good = 3, Excellent = 4, Masterwork = 5, Legendary = 6
}

public static class Dialog_InfoCard
{
    public readonly struct Hyperlink
    {
        public Hyperlink(object? def) { }
    }
}

public abstract class BookOutcomeProperties
{
    public virtual Type DoerClass => typeof(BookOutcomeDoer);
}

public abstract class BookOutcomeDoer
{
    protected internal BookOutcomeProperties props = null!;
    public BookOutcomeProperties Props => props;
    public QualityCategory Quality;

    public virtual void OnBookGenerated(Verse.Pawn? author = null) { }
    public virtual void Reset() { }
    public virtual void PostExposeData() { }
    public virtual void OnReadingTick(Verse.Pawn reader, float factor) { }
    public virtual IEnumerable<Dialog_InfoCard.Hyperlink> GetHyperlinks() => [];
    public virtual string GetBenefitsString(Verse.Pawn? reader = null) => string.Empty;
    public virtual bool DoesProvidesOutcome(Verse.Pawn reader) => false;
}
