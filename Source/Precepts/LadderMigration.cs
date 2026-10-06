namespace EnhancedIdeology;

// One issue ladder as it was when the game was saved: rung defNames in rank order, and the Don't-care rank.
internal sealed class SavedLadder : IExposable
{
    internal List<string> rungs = [];
    internal float dontCareRank;

    public SavedLadder() { }

    internal SavedLadder(List<string> rungs, float dontCareRank)
    {
        this.rungs = rungs;
        this.dontCareRank = dontCareRank;
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref rungs, "rungs", LookMode.Value);
        Scribe_Values.Look(ref dontCareRank, "dontCareRank");
    }
}

// Stances store a positional rank on the issue ladder. When the ladder changes between a save and a load
// (rungs reordered, a mod added or removed, Don't-care moved), the same rank points at a different rung.
// The save keeps a copy of every ladder, and on load each stored rank is carried to the live ladder by rung name.
internal static class LadderMigration
{
    internal static SavedLadder Capture(IssueDef issue) =>
        new([.. PreceptLadder.Rungs(issue).Select(precept => precept.defName)], PreceptLadder.DontCareRank(issue));

    internal static Dictionary<string, SavedLadder> CaptureAll() =>
        DefDatabase<IssueDef>.AllDefs
            .Where(issue => PreceptPolicy.CategoryOf(issue) != PreceptCategory.NA)
            .ToDictionary(issue => issue.defName, Capture);

    // Rank on the live ladder for a rank stored against `saved`.
    // Each saved rung that still exists, and the Don't-care rung, is an anchor from its saved rank to its live rank.
    // A rank between two anchors interpolates between their live ranks; a rank outside all anchors keeps its
    // offset from the nearest one.
    internal static float Remap(IssueDef issue, SavedLadder saved, float rank)
    {
        var liveDontCare = PreceptLadder.DontCareRank(issue);
        if (saved.dontCareRank == liveDontCare
            && saved.rungs.SequenceEqual(PreceptLadder.Rungs(issue).Select(precept => precept.defName)))
        {
            return rank;
        }

        var anchors = new List<(float saved, float live)> { (saved.dontCareRank, liveDontCare) };
        for (var ii = 0; ii < saved.rungs.Count; ii++)
        {
            var live = PreceptLadder.RankOfName(issue, saved.rungs[ii]);
            if (live >= 0f)
            {
                anchors.Add((ii, live));
            }
        }
        anchors.Sort((left, right) => left.saved.CompareTo(right.saved));

        if (rank <= anchors[0].saved)
        {
            return anchors[0].live + (rank - anchors[0].saved);
        }
        for (var ii = 1; ii < anchors.Count; ii++)
        {
            if (rank > anchors[ii].saved)
            {
                continue;
            }
            var (low, high) = (anchors[ii - 1], anchors[ii]);
            var fraction = (rank - low.saved) / (high.saved - low.saved);
            return low.live + ((high.live - low.live) * fraction);
        }
        var last = anchors[^1];
        return last.live + (rank - last.saved);
    }
}
