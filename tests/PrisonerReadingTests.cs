namespace EnhancedIdeology.Tests;

public class PrisonerReadingTests
{
    [Theory]
    [InlineData(0.5f, 0.5f)]
    [InlineData(0.3f, 0.7f)]
    [InlineData(0f, PrisonerReading.MaxShare)]
    [InlineData(0.05f, PrisonerReading.MaxShare)]
    [InlineData(0.9f, PrisonerReading.MinShare)]
    [InlineData(1f, PrisonerReading.MinShare)]
    public void ShareOfDay_IsOneMinusCertainty_Clamped(float certainty, float expected)
    {
        Assert.Equal(expected, PrisonerReading.ShareOfDay(certainty), 4);
    }

    [Fact]
    public void ShareOfDay_FallsAsCertaintyRises()
    {
        Assert.True(PrisonerReading.ShareOfDay(0.4f) > PrisonerReading.ShareOfDay(0.6f));
    }

    [Theory]
    [InlineData(1f, 32000)]
    [InlineData(0.5f, 16000)]
    [InlineData(0f, 8889)]
    public void CycleTicks_GivesReadingTheShareOfTheCycle(float certainty, int expected)
    {
        Assert.Equal(expected, PrisonerReading.CycleTicks(certainty, 8000));
    }

    [Fact]
    public void CycleTicks_NeverShorterThanTheSession()
    {
        for (var certainty = 0f; certainty <= 1f; certainty += 0.05f)
            Assert.True(PrisonerReading.CycleTicks(certainty, 8000) >= 8000);
    }
}
