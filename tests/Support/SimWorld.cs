namespace EnhancedIdeology.Tests.Support;

internal sealed class SimWorld
{
    public readonly GameComponent_EnhancedIdeology Comp;
    public readonly List<SimPawn> Pawns = [];
    public readonly List<Ideo> Ideos = [];
    public readonly Game Game;

    public SimWorld()
    {
        Game = new Game();
        Comp = new GameComponent_EnhancedIdeology(Game);
        Game.SetComponent(Comp);
    }

    public void Initialize()
    {
        Find.IdeoManager = new ShimIdeoManager
        {
            classicMode = false,
            IdeosListForReading = Ideos
        };
        Find.TickManager = new ShimTickManager { TicksGame = 0 };
        Find.Storyteller = new ShimStoryteller();
        Find.HistoryEventsManager = new HistoryEventsManager();
        Current.Game = Game;
        PawnsFinder.SetPawns(Pawns.Cast<Pawn>());
    }

    public void AddIdeo(Ideo ideo)
    {
        Ideos.Add(ideo);
        Find.IdeoManager.IdeosListForReading = Ideos;
        // Mirrors the Ideo constructor patch, which registers every ideo with the component.
        Comp.IdeoTracker.EnsureIdeoHasPawnTracker(ideo);
    }

    public void AddPawn(SimPawn pawn)
    {
        Pawns.Add(pawn);
        PawnsFinder.SetPawns(Pawns.Cast<Pawn>());
        Comp.SetIdeo(pawn, pawn.Ideo!);
    }
}
