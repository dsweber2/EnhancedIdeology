namespace EnhancedIdeology.HarmonyPatches;

// Vanilla runs no social interactions in a caravan, because TryInteractWith needs a spawned recipient. This gives
// caravan pawns the random interactions they would have at home, at the same rate and with the same weights, but
// resolves only the debates. The other interactions do not occur, as in vanilla.
[HarmonyPatch(typeof(Caravan), "TickInterval")]
internal static class Caravan_TickInterval_Debates
{
    // Vanilla Pawn_InteractionsTracker: RandomInteractCheckInterval and RandomInteractMTBTicks_Normal.
    private const int CheckIntervalTicks = 60;
    private const float InteractMtbTicks = 6600f;

    static void Postfix(Caravan __instance, int delta)
    {
        if (!ModsConfig.IdeologyActive || Find.IdeoManager.classicMode)
            return;

        var pawns = __instance.PawnsListForReading;
        for (var ii = 0; ii < pawns.Count; ii++)
        {
            var initiator = pawns[ii];
            if (!initiator.IsHashIntervalTick(CheckIntervalTicks, delta)
                || !Rand.MTBEventOccurs(InteractMtbTicks, 1f, CheckIntervalTicks)
                || !CanInitiate(initiator))
                continue;

            if (pawns.Where(pawn => pawn != initiator && CanReceive(pawn)).TryRandomElement(out var recipient))
                TryInteract(initiator, recipient);
        }
    }

    // SocialInteractionUtility.CanInitiateRandomInteraction without its interactions-tracker check: vanilla removes
    // that tracker from a despawned pawn, so the check always fails in a caravan.
    internal static bool CanInitiate(Pawn pawn) =>
        CaravanDebates.IsAlert(pawn)
        && pawn.RaceProps.Humanlike
        && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking)
        && pawn.Awake()
        && !pawn.Downed
        && !pawn.InAggroMentalState
        && !(pawn.IsMutant && pawn.mutant.Def.incapableOfSocialInteractions)
        && !pawn.IsInteractionBlocked(null, isInitiator: true, isRandom: false)
        && !pawn.IsInteractionBlocked(null, isInitiator: true, isRandom: true)
        && pawn.Faction != null
        && pawn.ageTracker.CurLifeStage.canInitiateSocialInteraction
        && !pawn.Inhumanized();

    internal static bool CanReceive(Pawn pawn) =>
        CaravanDebates.IsAlert(pawn) && SocialInteractionUtility.CanReceiveRandomInteraction(pawn);

    private static void TryInteract(Pawn initiator, Pawn recipient)
    {
        if (!DefDatabase<InteractionDef>.AllDefsListForReading
                .TryRandomElementByWeight(def => SelectionWeight(def, initiator, recipient), out var chosen)
            || (chosen != EnhancedIdeologyDefOf.EB_IdeologicalDebatePrecept && chosen != EnhancedIdeologyDefOf.EB_IdeologicalDebateMeme))
            return;

        Resolve(chosen, initiator, recipient);
    }

    // The parts of Pawn_InteractionsTracker.TryInteractWith that apply off-map: resolve, log, and send the letter.
    // Returns the letter def, which is set when the debate converted someone.
    internal static LetterDef? Resolve(InteractionDef chosen, Pawn initiator, Pawn recipient)
    {
        var sentencePacks = new List<RulePackDef>();
        chosen.Worker.Interacted(initiator, recipient, sentencePacks, out var letterText, out var letterLabel, out var letterDef, out var lookTargets);
        var entry = (PlayLogEntry_Interaction?)PlayLogEntry_DebateInteraction.FromLastDebate(chosen, initiator, recipient, sentencePacks)
            ?? new PlayLogEntry_Interaction(chosen, initiator, recipient, sentencePacks);
        Find.PlayLog.Add(entry);

        if (letterDef != null)
        {
            var text = entry.ToGameStringFromPOV(initiator);
            if (!letterText.NullOrEmpty())
                text += "\n\n" + letterText;
            Find.LetterStack.ReceiveLetter(letterLabel, text, letterDef, lookTargets ?? initiator);
        }
        return letterDef;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Third-party interaction workers")]
    private static float SelectionWeight(InteractionDef def, Pawn initiator, Pawn recipient)
    {
        // CanInitiate already holds the rest of SocialInteractionUtility.CanInitiateInteraction.
        if (initiator.IsInteractionBlocked(def, isInitiator: true, isRandom: false) || !SocialInteractionUtility.CanReceiveInteraction(recipient, def))
            return 0f;

        // Vanilla weights do not use the map, but interactions from other mods can expect a spawned pawn.
        try
        {
            return def.Worker.RandomSelectionWeight(initiator, recipient);
        }
        catch (Exception ex)
        {
            Log.WarningOnce($"[EnhancedIdeology] {def.defName} cannot weigh a caravan interaction, so it is skipped in caravans: {ex}", def.shortHash);
            return 0f;
        }
    }
}
