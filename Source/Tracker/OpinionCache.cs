namespace EnhancedIdeology;

// Per-pawn opinion storage: ideo base/personal opinions, meme opinions, and relationship caches.
// Also owns the dirty flag that signals when structural opinions must be recomputed. Methods that
// call back into IdeoTrackerData (e.g. StructuralIdeoOpinion) stay there and access dicts through
// the internal properties below.
internal sealed class OpinionCache
{
    private Pawn _pawn;

    // Separate because recalculating base from memes in case player's ideo is fluid cuts down on
    // overall performance cost. Breaks if you multiply opinion but you really shouldn't do that.
    private Dictionary<Ideo, float> _baseIdeoOpinions = [];
    private Dictionary<Ideo, float> _personalIdeoOpinions = [];
    private Dictionary<MemeDef, float> _memeOpinions = [];
    private readonly Dictionary<Ideo, float> _cachedRelationshipIdeoOpinions = [];
    private readonly Dictionary<Pawn, float> _cachedRelationships = [];

    internal Dictionary<Ideo, float> BaseIdeoOpinions => _baseIdeoOpinions;
    internal Dictionary<Ideo, float> PersonalIdeoOpinions => _personalIdeoOpinions;
    internal Dictionary<MemeDef, float> MemeOpinions => _memeOpinions;
    internal Dictionary<Ideo, float> CachedRelationshipIdeoOpinions => _cachedRelationshipIdeoOpinions;
    internal Dictionary<Pawn, float> CachedRelationships => _cachedRelationships;

    // Set when a stance shift invalidates the cached structural (base) opinions. Read paths refresh
    // lazily so a batch of stance shifts pays only one recompute on the next read.
    private bool _dirty;
    internal bool IsDirty => _dirty;
    internal void MarkDirty() { _dirty = true; }
    internal void ClearDirty() { _dirty = false; }

    // Scribe_Collections working lists.
    private List<Ideo>? _baseKeys;
    private List<Ideo>? _personalKeys;
    private List<MemeDef>? _memeKeys;
    private List<float>? _baseValues;
    private List<float>? _personalValues;
    private List<float>? _memeValues;

    internal OpinionCache(Pawn pawn) { _pawn = pawn; }
    internal void SetPawn(Pawn pawn) { _pawn = pawn; }

    public void SetIdeoBaseOpinion(Ideo ideo, float opinion)
    {
        _baseIdeoOpinions[ideo] = opinion;
        _ = _personalIdeoOpinions.TryAdd(ideo, 0f);
    }

    public void AdjustMemeOpinion(MemeDef meme, float power)
    {
        _memeOpinions ??= [];
        if (!_memeOpinions.ContainsKey(meme))
            _memeOpinions[meme] = 0;
        _memeOpinions[meme] += power * 100f;
    }

    public float TrueMemeOpinion(MemeDef meme)
    {
        if (!_memeOpinions.TryGetValue(meme, out var opinion))
        {
            opinion = 0;
            _memeOpinions[meme] = opinion;
        }

        if (!meme.agreeableTraits.NullOrEmpty())
            foreach (var trait in meme.agreeableTraits)
                if (trait.HasTrait(_pawn))
                    opinion += 10;

        if (!meme.disagreeableTraits.NullOrEmpty())
            foreach (var trait in meme.disagreeableTraits)
                if (trait.HasTrait(_pawn))
                    opinion -= 10;

        return opinion;
    }

    // Recalculate and store the relationship-weighted ideo opinion for `ideo`.
    public void CacheRelationshipIdeoOpinion(Ideo ideo, GameComponent_EnhancedIdeology? comp = null)
    {
        float opinion = 0;
        comp ??= Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        var pawns = comp.GetIdeoPawns(ideo);

        foreach (var otherPawn in pawns)
        {
            if (!SameLocalGroup(otherPawn)) continue;
            float pawnOpinion = _pawn.relations.OpinionOf(otherPawn);
            opinion += pawnOpinion * IdeoTrackerData.PawnOpinionFactor;
            _cachedRelationships[otherPawn] = pawnOpinion;
        }

        _cachedRelationshipIdeoOpinions[ideo] = opinion;
    }

    public void RecalculateRelationshipIdeoOpinions()
    {
        foreach (var ideo in _baseIdeoOpinions.Keys)
            CacheRelationshipIdeoOpinion(ideo);
    }

    public IEnumerable<(Pawn pawn, float opinion)> GetOwnIdeoRelationships()
    {
        var ideo = _pawn.Ideo!;
        foreach (var kvp in _cachedRelationships)
            if (kvp.Key != _pawn && kvp.Key.Ideo == ideo)
                yield return (kvp.Key, kvp.Value);
    }

    // Called flat from IdeoTrackerData.ExposeData (no Scribe_Deep wrapper) to preserve XML structure.
    internal void ExposeData()
    {
        Scribe_Collections.Look(ref _baseIdeoOpinions, "baseIdeoOpinions", LookMode.Reference, LookMode.Value, ref _baseKeys, ref _baseValues);
        Scribe_Collections.Look(ref _personalIdeoOpinions, "personalIdeoOpinions", LookMode.Reference, LookMode.Value, ref _personalKeys, ref _personalValues);
        Scribe_Collections.Look(ref _memeOpinions, "memeOpinions", LookMode.Def, LookMode.Value, ref _memeKeys, ref _memeValues);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            _baseIdeoOpinions ??= [];
            _personalIdeoOpinions ??= [];
            _memeOpinions ??= [];
        }
    }

    private bool SameLocalGroup(Pawn other)
    {
        if (_pawn.MapHeld != null)
            return other.MapHeld == _pawn.MapHeld;
        var caravan = _pawn.GetCaravan();
        return caravan != null && other.GetCaravan() == caravan;
    }
}
