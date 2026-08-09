namespace EnhancedBeliefs.Tests;

// Covers how the certainty setpoint (target) is composed from its bands, how the user settings
// steer it, and the per-band contributor breakdown shown in the social-card tooltip.
public class CertaintySetpointTests : SeededTest
{
    // SimSettings is a process-wide singleton; snapshot and restore around any mutation so tests don't leak.
    private static void WithSettings(Action<SimSettings> mutate, Action body)
    {
        var settings = EnhancedBeliefsMod.Settings;
        var drift = settings.CertaintyDriftRate;
        var difficulty = settings.DifficultyOffset;
        var relational = settings.RelationalMaxRange;
        var practice = settings.PracticeMaxRange;
        try
        {
            mutate(settings);
            body();
        }
        finally
        {
            settings.CertaintyDriftRate = drift;
            settings.DifficultyOffset = difficulty;
            settings.RelationalMaxRange = relational;
            settings.PracticeMaxRange = practice;
        }
    }

    private static (SimWorld world, IdeoTrackerData tracker, SimPawn pawn) BuildCongregation(
        int coReligionists, float opinionEach, float certainty = 0.5f)
    {
        var world = new SimWorld();
        world.Initialize();

        var ideo = new IdeoBuilder().WithName("I").AddPrecept(new PreceptDef { defName = "P" }).Build();
        world.AddIdeo(ideo);

        var others = new List<SimPawn>();
        for (var ii = 0; ii < coReligionists; ii++)
            others.Add(new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel($"O{ii}").Build(world));

        var builder = new PawnBuilder().WithIdeo(ideo).WithCertainty(certainty).WithLabel("P");
        foreach (var other in others)
            builder.WithOpinionOf(other, opinionEach);
        var pawn = builder.Build(world);

        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        pawn.UpdateThoughts();
        tracker.CertaintyChangeRecache(world.Comp);
        return (world, tracker, pawn);
    }

    [Fact]
    public void Relational_IsMeanBased_SizeIndependent()
    {
        // The core design decision: 1 friend at +80 pulls as hard as 4 friends at +80.
        var (_, lone, _) = BuildCongregation(coReligionists: 1, opinionEach: 80f);
        var (_, many, _) = BuildCongregation(coReligionists: 4, opinionEach: 80f);

        Assert.Equal(lone.CachedRelational, many.CachedRelational, precision: 5);
        Assert.True(lone.CachedRelational > 0f);
    }

    [Fact]
    public void Relational_MaxRange_CapsBand()
    {
        WithSettings(s => s.RelationalMaxRange = 0.12f, () =>
        {
            var (_, tracker, _) = BuildCongregation(coReligionists: 2, opinionEach: 100f);
            Assert.Equal(0.12f, tracker.CachedRelational, precision: 5);
        });
    }

    [Fact]
    public void Relational_MaxRangeSetting_ScalesBand()
    {
        float wide = 0f;
        float narrow = 0f;
        WithSettings(s => s.RelationalMaxRange = 0.20f, () =>
            wide = BuildCongregation(coReligionists: 1, opinionEach: 60f).tracker.CachedRelational);
        WithSettings(s => s.RelationalMaxRange = 0.05f, () =>
            narrow = BuildCongregation(coReligionists: 1, opinionEach: 60f).tracker.CachedRelational);

        Assert.True(wide > narrow);
        Assert.Equal(0.20f / 0.05f, wide / narrow, precision: 3);
    }

    [Fact]
    public void Difficulty_ShiftsTargetByOffset()
    {
        // Pick a pawn whose bands leave the target well inside (0, 1) so the offset is not clamped.
        float baseTarget = 0f;
        float shifted = 0f;
        WithSettings(s => s.DifficultyOffset = 0f, () =>
            baseTarget = BuildCongregation(coReligionists: 0, opinionEach: 0f).tracker.CachedTargetCertainty);
        WithSettings(s => s.DifficultyOffset = 0.15f, () =>
            shifted = BuildCongregation(coReligionists: 0, opinionEach: 0f).tracker.CachedTargetCertainty);

        Assert.Equal(baseTarget + 0.15f, shifted, precision: 5);
    }

    [Fact]
    public void DriftRateSetting_ScalesRate()
    {
        // A fresh pawn seeds to its setpoint (zero drift), so push certainty off-target first, then measure.
        float DriftAt(float rate)
        {
            var result = 0f;
            WithSettings(s => s.CertaintyDriftRate = rate, () =>
            {
                var (world, tracker, pawn) = BuildCongregation(coReligionists: 0, opinionEach: 0f);
                pawn.ideo.Certainty = 0.9f;
                pawn.UpdateThoughts();
                tracker.CertaintyChangeRecache(world.Comp);
                result = tracker.CachedCertaintyChange;
            });
            return result;
        }

        // Same gap, 4x the rate -> 4x the (negative) change.
        Assert.Equal(4f, DriftAt(0.20f) / DriftAt(0.05f), precision: 3);
    }

    [Fact]
    public void Target_IsSumOfBandsPlusDifficulty()
    {
        var (_, tracker, _) = BuildCongregation(coReligionists: 2, opinionEach: 50f);
        var expected = Mathf.Clamp01(
            tracker.CachedStructural + tracker.CachedRelational + tracker.CachedPractitional + tracker.CachedDifficulty);

        Assert.Equal(expected, tracker.CachedTargetCertainty, precision: 5);
    }

    [Fact]
    public void StructuralContributors_ComeFromPerIssuePreceptStances()
    {
        // The flat +30 base faith and the per-precept external offset are gone. The structural band is now
        // built from per-issue precept agreement: the pawn's own ideo holds a rung, so on that issue they
        // agree with themselves (+strength) and it contributes a positive term that sums to the band.
        var world = new SimWorld();
        world.Initialize();

        var (issue, rungs) = SimIssues.Ladder("Generosity", "Selfish", "Generous");
        var ideo = new IdeoBuilder().WithName("I").AddPrecept(rungs[1], issue, displayOrderInIssue: 10).Build();
        world.AddIdeo(ideo);

        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        pawn.UpdateThoughts();
        tracker.CertaintyChangeRecache(world.Comp);

        // The structural band is positive (agreement with own stance) and non-trivial.
        Assert.True(tracker.CachedStructural > 0f, $"expected positive structural band, got {tracker.CachedStructural}");
        // There is a per-issue contributor for the registered ladder.
        Assert.Contains(tracker.StructuralContributors, c => c.label == issue.LabelCap && c.pct > 0f);
        // Contributors sum to the structural band.
        Assert.Equal(tracker.CachedStructural, tracker.StructuralContributors.Sum(c => c.pct), precision: 5);
    }

    [Fact]
    public void RelationalContributors_OnePerCoReligionist_SignedByOpinion()
    {
        var world = new SimWorld();
        world.Initialize();

        var ideo = new IdeoBuilder().WithName("I").AddPrecept(new PreceptDef { defName = "P" }).Build();
        world.AddIdeo(ideo);

        var friend = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("Friend").Build(world);
        var rival = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("Rival").Build(world);
        var pawn = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithLabel("P")
            .WithOpinionOf(friend, 60f)
            .WithOpinionOf(rival, -40f)
            .Build(world);

        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
        pawn.UpdateThoughts();
        tracker.CertaintyChangeRecache(world.Comp);

        Assert.Equal(2, tracker.RelationalContributors.Count);
        Assert.Contains(tracker.RelationalContributors, c => c.label == "Friend" && c.pct > 0f);
        Assert.Contains(tracker.RelationalContributors, c => c.label == "Rival" && c.pct < 0f);
    }

    [Fact]
    public void PractitionalContributors_PresentWhenPreceptMood_EmptyOtherwise()
    {
        var world = new SimWorld();
        world.Initialize();
        var ideo = new IdeoBuilder().WithName("I").AddPrecept(new PreceptDef { defName = "P" }).Build();
        world.AddIdeo(ideo);

        var practising = new PawnBuilder().WithIdeo(ideo).WithCertainty(0.5f).WithColonyMoodOffset(20f).WithLabel("A").Build(world);
        var practisingTracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(practising);
        practising.UpdateThoughts();
        practisingTracker.CertaintyChangeRecache(world.Comp);

        Assert.NotEmpty(practisingTracker.PractitionalContributors);

        // No precept in the ideo -> no precept-sourced thought -> no practitional contributors.
        var flatWorld = new SimWorld();
        flatWorld.Initialize();
        var flatIdeo = new IdeoBuilder().WithName("Flat").Build();
        flatWorld.AddIdeo(flatIdeo);
        var idle = new PawnBuilder().WithIdeo(flatIdeo).WithCertainty(0.5f).WithColonyMoodOffset(20f).WithLabel("B").Build(flatWorld);
        var idleTracker = flatWorld.Comp.PawnTracker.EnsurePawnHasIdeoTracker(idle);
        idle.UpdateThoughts();
        idleTracker.CertaintyChangeRecache(flatWorld.Comp);

        Assert.Empty(idleTracker.PractitionalContributors);
    }
}
