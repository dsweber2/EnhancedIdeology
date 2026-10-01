using System.Runtime.CompilerServices;

namespace EnhancedIdeology.HarmonyPatches;

// Scribing data in a postfix to ensure that no junk data is saved
[HarmonyPatch(typeof(Pawn), nameof(Pawn.ExposeData))]
internal static class Pawn_ExposeData
{
    // Pawns deep-saved inside another mod's GameComponent load before this mod's component exists.
    // Their data waits here until PostLoadInit, when the component is present.
    private static readonly ConditionalWeakTable<Pawn, IdeoTrackerData> pendingData = new();

    private static void Postfix(Pawn __instance)
    {
        if (__instance.ideo == null)
        {
            return;
        }

        var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
        if (comp == null)
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                IdeoTrackerData? loaded = null;
                Scribe_Deep.Look(ref loaded, "EB_IdeoTrackerData", __instance);
                if (loaded != null)
                {
                    pendingData.AddOrUpdate(__instance, loaded);
                }
            }
            return;
        }
        var pawnTracker = comp.PawnTracker;
        if (pawnTracker == null)
        {
            EnhancedIdeologyMod.ErrorOnce($"Pawn_ExposeData: PawnTracker is null. "
                + "This should not happen. Please report this issue and any related logs.",
                typeof(Pawn_ExposeData).GetHashCode() + typeof(GameComponent_EnhancedIdeology.PawnIdeoTracker).GetHashCode());
            return;
        }
        var data = pawnTracker.TryGetIdeoTracker(__instance);
        if (data == null && Scribe.mode == LoadSaveMode.PostLoadInit && pendingData.TryGetValue(__instance, out var pending))
        {
            _ = pendingData.Remove(__instance);
            data = pending;
        }

        Scribe_Deep.Look(ref data, "EB_IdeoTrackerData", __instance);

        if (Scribe.mode != LoadSaveMode.Saving)
        {
            if (data != null)
            {
                // The pawn reference only resolves after cross-refs, so check it once loading is finished.
                if (Scribe.mode == LoadSaveMode.PostLoadInit
                    && (data.Pawn is not Pawn pawn || (pawn != __instance && !pawn.Dead)))
                {
                    EnhancedIdeologyMod.Warning($"Tried to scribe IdeoTrackerData for pawn {__instance} but "
                        + $"the data is for pawn {data.Pawn?.ToString() ?? "[null]"}. "
                        + $"This should not happen. Overriding data pawn to match the current pawn.");
                    data.ForceNewPawn(__instance);
                }
                comp.PawnTracker.SetIdeoTracker(__instance, data);
            }
            else if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // No EB data in save (vanilla or completely pre-EB save): create a tracker now that the pawn is
                // fully loaded, preserving their existing certainty via calibration on the first recache.
                data = new IdeoTrackerData(__instance);
                data.MarkAsLoadedWithoutData(__instance.ideo.Certainty);
                comp.PawnTracker.SetIdeoTracker(__instance, data);
                comp.SetIdeo(__instance, __instance.Ideo);
            }
        }
    }
}
