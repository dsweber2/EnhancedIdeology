namespace EnhancedIdeology.Tests;

// Covers carrying stored stance ranks from the ladder a game was saved against to the live ladder.
public class LadderMigrationTests : SeededTest
{
    private static SavedLadder Saved(float dontCareRank, params string[] rungs) => new([.. rungs], dontCareRank);

    [Fact]
    public void Remap_UnchangedLadder_IsIdentity()
    {
        var (issue, _) = SimIssues.Ladder("TestIssue", "A", "B", "C");
        var saved = LadderMigration.Capture(issue);

        Assert.Equal(1.37f, LadderMigration.Remap(issue, saved, 1.37f));
        Assert.Equal(-1f, LadderMigration.Remap(issue, saved, -1f));
    }

    // The v1.1.2 Blindness order override: vanilla displayOrderInIssue put Horrible first, the override puts it last
    // and the new Don't-care spec sits between Respected and Horrible.
    [Fact]
    public void Remap_ReorderedBlindness_KeepsEachPawnOnTheirRung()
    {
        var (issue, _) = SimIssues.Ladder("Blindness",
            "Blinding_Horrible", "Blindness_Sublime", "Blindness_Elevated", "Blindness_Respected");
        var saved = Saved(-1f, "Blinding_Horrible", "Blindness_Sublime", "Blindness_Elevated", "Blindness_Respected");

        Assert.Equal(3f, PreceptLadder.RankOfName(issue, "Blinding_Horrible"));
        Assert.Equal(3f, LadderMigration.Remap(issue, saved, 0f));
        Assert.Equal(0f, LadderMigration.Remap(issue, saved, 1f));
        Assert.Equal(2f, LadderMigration.Remap(issue, saved, 3f));
        Assert.Equal(2.5f, LadderMigration.Remap(issue, saved, -1f));
    }

    [Fact]
    public void Remap_RungRemoved_InterpolatesAcrossTheGap()
    {
        var (issue, _) = SimIssues.Ladder("TestIssue", "A", "B");
        var saved = Saved(-1f, "A", "Gone", "B");

        Assert.Equal(1f, LadderMigration.Remap(issue, saved, 2f));
        Assert.Equal(0.5f, LadderMigration.Remap(issue, saved, 1f));
    }

    [Fact]
    public void Remap_RungAdded_ShiftsLaterRungsAndKeepsOffsetPastTheEnd()
    {
        var (issue, _) = SimIssues.Ladder("TestIssue", "A", "New", "B");
        var saved = Saved(-1f, "A", "B");

        Assert.Equal(0f, LadderMigration.Remap(issue, saved, 0f));
        Assert.Equal(2f, LadderMigration.Remap(issue, saved, 1f));
        Assert.Equal(1f, LadderMigration.Remap(issue, saved, 0.5f));
        Assert.Equal(2.25f, LadderMigration.Remap(issue, saved, 1.25f));
        Assert.Equal(-1f, LadderMigration.Remap(issue, saved, -1f));
    }

    private static (IdeoTrackerData tracker, IssueDef issue) TrackerHolding(string heldRung, params string[] rungNames)
    {
        var world = new SimWorld();
        world.Initialize();
        var (issue, rungs) = SimIssues.Ladder("TestIssue", rungNames);
        var ideo = new IdeoBuilder().WithName("TestIdeo").AddPrecept(rungs.First(rung => rung.defName == heldRung)).Build();
        world.AddIdeo(ideo);
        var pawn = new PawnBuilder().WithIdeo(ideo).Build(world);
        return (world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn), issue);
    }

    private static (float rank, float strength) Stance(IdeoTrackerData tracker, IssueDef issue)
    {
        var stance = tracker.IssueStances().First(stance => stance.issue == issue);
        return (stance.rank, stance.strength);
    }

    [Fact]
    public void RemapToLiveLadders_MovesRankAndKeepsStrength()
    {
        var (tracker, issue) = TrackerHolding("A", "A", "New", "B");
        tracker.SetIssueStance(issue, 1f, 12f);
        var saved = Saved(-1f, "A", "B");

        var changed = tracker.Stances.RemapToLiveLadders(target => target == issue ? saved : null);

        Assert.True(changed);
        Assert.Equal((2f, 12f), Stance(tracker, issue));
    }

    [Fact]
    public void RemapToLiveLadders_NoSavedLadder_LeavesRank()
    {
        var (tracker, issue) = TrackerHolding("A", "A", "B");
        tracker.SetIssueStance(issue, 1f, 12f);

        Assert.False(tracker.Stances.RemapToLiveLadders(_ => null));
        Assert.Equal((1f, 12f), Stance(tracker, issue));
    }

    [Fact]
    public void ResetStancesToOrthodox_MovesToHeldRungAndKeepsStrength()
    {
        var (tracker, issue) = TrackerHolding("C", "A", "B", "C");
        tracker.SetIssueStance(issue, 0.4f, 17f);

        tracker.ResetStancesToOrthodox();

        Assert.Equal((2f, 17f), Stance(tracker, issue));
    }
}
