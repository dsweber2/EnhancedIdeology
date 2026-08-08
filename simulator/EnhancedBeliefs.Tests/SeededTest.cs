namespace EnhancedBeliefs.Tests;

// Base for all test classes. xUnit constructs a fresh instance per test method, so this reseeds the shared
// global Rand stream before every test - making outcomes order-independent and stopping one test's RNG
// consumption from leaking into another. Tests that need a specific stream may reseed themselves.
public abstract class SeededTest
{
    protected SeededTest()
    {
        Rand.SetSeed(1);
        // Precept ladders resolve through the global DefDatabase; clear it so one test's registered
        // issues/rungs never leak into another's ladder ordering.
        DefDatabase<PreceptDef>.Clear();
        DefDatabase<IssueDef>.Clear();
        PreceptPolicy.ClearOverrides();
    }
}
