using LudeonTK;

namespace EnhancedIdeology;

internal static partial class DebugActions
{
    // Sizes of every collection the tick paths walk. Run it twice, some game days apart, to find what grows.
    [DebugAction("Ideoligion", "Log tracker sizes", actionType = DebugActionType.Action,
        allowedGameStates = AllowedGameStates.Playing, requiresIdeology = true)]
    private static void LogTrackerSizes()
    {
        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();

        var trackers = comp.PawnTracker.Select(kvp => kvp.Key).ToList();
        var local = trackers.Count(pawn => !pawn.Dead && (pawn.MapHeld != null || pawn.IsCaravanMember()));
        var listEntries = comp.IdeoTracker.SelectMany(kvp => kvp.Value).ToList();
        var localTrackers = comp.PawnTracker
            .Where(kvp => !kvp.Key.Dead && kvp.Key.MapHeld != null)
            .Select(kvp => kvp.Value)
            .ToList();

        EnhancedIdeologyMod.Message(
            $"[tracker sizes] tick={Find.TickManager.TicksGame}"
            + $"\n  trackers: total={trackers.Count} local={local} dead={trackers.Count(pawn => pawn.Dead)} discarded={trackers.Count(pawn => pawn.Discarded)}"
            + $"\n  ideo lists: ideos={comp.IdeoTracker.Count()} entries={listEntries.Count} dead={listEntries.Count(pawn => pawn.Dead)} discarded={listEntries.Count(pawn => pawn.Discarded)}"
            + $"\n  world pawns: {Find.WorldPawns.AllPawnsAliveOrDead.Count}"
            + $"\n  per local tracker (max): baseOpinions={localTrackers.Select(data => data.Opinions.BaseIdeoOpinions.Count).DefaultIfEmpty().Max()}"
            + $" relationships={localTrackers.Select(data => data.Opinions.CachedRelationships.Count).DefaultIfEmpty().Max()}"
            + $" memories={localTrackers.Select(data => data.Pawn.needs?.mood?.thoughts.memories.Memories.Count ?? 0).DefaultIfEmpty().Max()}"
            + $"\n  held stances per local tracker (off Don't-care, graded by Compute): mean={localTrackers.Select(HeldStanceCount).DefaultIfEmpty().Average():F1}"
            + $" max={localTrackers.Select(HeldStanceCount).DefaultIfEmpty().Max()}");
    }

    // Every issue's live ladder after all patches, with the source of each rung. Runs from the main menu too, so a
    // player can send it without loading a save. Each AllowedGameStates flag is a requirement, so Invalid (no
    // flags) is the only value that allows both the main menu and a game.
    [DebugAction("Ideoligion", "Log issue ladders", actionType = DebugActionType.Action,
        allowedGameStates = AllowedGameStates.Invalid)]
    private static void LogIssueLadders()
    {
        var issues = DefDatabase<IssueDef>.AllDefs
            .Where(issue => PreceptPolicy.CategoryOf(issue) != PreceptCategory.NA)
            .OrderBy(issue => issue.defName)
            .Select(issue =>
                $"{issue.defName} ({PreceptPolicy.CategoryOf(issue)}, from {issue.modContentPack?.Name ?? "no mod"}), "
                + $"neutral rank {PreceptLadder.DontCareRank(issue):0.##}\n{PreceptPolicy.DescribeRungs(issue)}");
        EnhancedIdeologyMod.Message("[issue ladders]\n" + string.Join("\n", issues));
    }

    private static int HeldStanceCount(IdeoTrackerData data) =>
        data.IssueStances().Count(stance => Mathf.Abs(stance.rank - PreceptLadder.DontCareRank(stance.issue))
            > InteractionWorker_IdeologicalDebatePrecept.DebateRankEpsilon);
}
