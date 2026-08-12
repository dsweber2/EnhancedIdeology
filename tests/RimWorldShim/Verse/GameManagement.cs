namespace Verse;

public interface IExposable
{
    void ExposeData();
}

public enum LoadSaveMode { Inactive, Saving, LoadingVars, ResolvingCrossRefs, PostLoadInit }
public enum LookMode { Undefined, Value, Reference, Def, Deep, GlobalDef }

public static class Scribe
{
    public static LoadSaveMode mode = LoadSaveMode.Inactive;
}

public static class Scribe_References
{
    public static void Look<T>(ref T? value, string label) where T : class { }
}

public static class Scribe_Values
{
    public static void Look<T>(ref T value, string label, T defaultValue = default!) { }
}

public static class Scribe_Collections
{
    public static void Look<K, V>(
        ref Dictionary<K, V>? dict, string label,
        LookMode keyLookMode, LookMode valueLookMode,
        ref List<K>? keysWorkingList, ref List<V>? valuesWorkingList)
        where K : notnull { }

    public static void Look<K, V>(
        ref Dictionary<K, V> dict, string label,
        LookMode keyLookMode = LookMode.Undefined, LookMode valueLookMode = LookMode.Undefined)
        where K : notnull { }
}

public abstract class GameComponent
{
    protected GameComponent() { }
    protected GameComponent(Game game) { }
    public virtual void GameComponentTick() { }
    public virtual void ExposeData() { }
    public virtual void FinalizeInit() { }
}

public class Game
{
    private readonly Dictionary<Type, GameComponent> _components = [];

    public T GetComponent<T>() where T : GameComponent
    {
        if (_components.TryGetValue(typeof(T), out var comp)) return (T)comp;
        throw new InvalidOperationException($"Game component {typeof(T).Name} not set. Call SetComponent in SimWorld.Initialize().");
    }

    public void SetComponent<T>(T component) where T : GameComponent
        => _components[typeof(T)] = component;
}

public class Map { }

public enum DevelopmentalStage { Newborn = 0, Baby = 1, Child = 2, Adult = 3 }

public static class DevelopmentalStageExtensions
{
    public static bool Baby(this DevelopmentalStage stage) => stage <= DevelopmentalStage.Baby;
    public static bool Newborn(this DevelopmentalStage stage) => stage == DevelopmentalStage.Newborn;
    public static bool Child(this DevelopmentalStage stage) => stage <= DevelopmentalStage.Child;
    public static bool Adult(this DevelopmentalStage stage) => stage >= DevelopmentalStage.Adult;
}
