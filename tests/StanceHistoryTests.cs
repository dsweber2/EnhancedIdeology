namespace EnhancedIdeology.Tests;

public class StanceHistoryTests : SeededTest
{
    private static (IssueDef first, IssueDef second) Issues()
    {
        var world = new SimWorld();
        world.Initialize();
        var (first, _) = SimIssues.Ladder("History_First", "First0", "First1", "First2");
        var (second, _) = SimIssues.Ladder("History_Second", "Second0", "Second1");
        return (first, second);
    }

    [Fact]
    public void FillMissing_RecordsTheCurrentStanceWhenThereIsNoHistory()
    {
        var (first, _) = Issues();
        var history = new StanceHistory();

        history.FillMissing(new() { [first] = 1f }, new() { [first] = 12f });

        Assert.Equal((1f, 12f), history.Before(first));
    }

    [Fact]
    public void FillMissing_KeepsExistingRecords()
    {
        var (first, second) = Issues();
        var history = new StanceHistory();
        history.RecordDayStart(new() { [first] = 0f }, new() { [first] = 5f });

        history.FillMissing(new() { [first] = 2f, [second] = 1f }, new() { [first] = 15f, [second] = 8f });

        Assert.Equal((0f, 5f), history.Before(first));
        Assert.Equal((1f, 8f), history.Before(second));
    }

    [Fact]
    public void RemapRanks_MovesBothDayStartRecords()
    {
        var (first, second) = Issues();
        var history = new StanceHistory();
        history.RecordDayStart(new() { [first] = 0f, [second] = 1f }, new() { [first] = 5f, [second] = 6f });
        history.RecordDayStart(new() { [first] = 1f, [second] = 1f }, new() { [first] = 7f, [second] = 8f });

        history.RemapRanks((issue, rank) => issue == first ? rank + 1f : null);

        Assert.Equal((1f, 5f), history.Before(first));
        Assert.Equal((1f, 6f), history.Before(second));
        history.RecordDayStart(new() { [first] = 0f, [second] = 0f }, new() { [first] = 0f, [second] = 0f });
        Assert.Equal((2f, 7f), history.Before(first));
    }
}
