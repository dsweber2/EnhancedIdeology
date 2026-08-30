namespace EnhancedIdeology;

[HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory),
    typeof(Thought_Memory), typeof(Pawn))]
static class MemoryThoughts_TryGainMemory
{
    static void Postfix(MemoryThoughtHandler __instance, Thought_Memory newThought)
    {
        var pawn = newThought.pawn;
        if (pawn?.Ideo == null) return;
        if (newThought is Thought_CognitiveDissonance) return;
        if (newThought.sourcePrecept?.ideo == null) return;
        if (newThought.sourcePrecept.ideo == pawn.Ideo) return;

        var offset = newThought.MoodOffset();

        float counterMultiplier = 0f;

        var gestalt = EnhancedIdeologyDefOf.VME_Gestalt;
        var nationalist = EnhancedIdeologyDefOf.VME_Nationalist;
        var isolationist = EnhancedIdeologyDefOf.VFEA_Isolationist;
        var violentConversion = EnhancedIdeologyDefOf.VME_ViolentConversion;
        var egalitarian = EnhancedIdeologyDefOf.VME_Egalitarian;
        var emancipation = EnhancedIdeologyDefOf.VME_Emancipation;
        if (gestalt != null && pawn.Ideo.HasMeme(gestalt))
            counterMultiplier = -2f;
        else if ((isolationist != null && pawn.Ideo.HasMeme(isolationist))
            || (violentConversion != null && pawn.Ideo.HasMeme(violentConversion)))
            counterMultiplier = -1.5f;
        else if (pawn.Ideo.HasMeme(EnhancedIdeologyDefOf.Supremacist)
            || pawn.Ideo.HasMeme(EnhancedIdeologyDefOf.Collectivist)
            || (nationalist != null && pawn.Ideo.HasMeme(nationalist)))
            counterMultiplier = -1f;
        else if (pawn.Ideo.HasMeme(EnhancedIdeologyDefOf.Loyalist))
            counterMultiplier = -0.5f;
        else if (pawn.Ideo.HasMeme(EnhancedIdeologyDefOf.Guilty)
            || (egalitarian != null && pawn.Ideo.HasMeme(egalitarian))
            || (emancipation != null && pawn.Ideo.HasMeme(emancipation)))
            counterMultiplier = 0.5f;
        else if (pawn.Ideo.HasMeme(EnhancedIdeologyDefOf.Individualist))
            counterMultiplier = 0.5f;

        if (counterMultiplier == 0f) return;

        var counterOffset = counterMultiplier * Mathf.Abs(offset);
        if (Mathf.Abs(counterOffset) < 0.01f) return;

        var counterDef = counterOffset < 0
            ? EnhancedIdeologyDefOf.EB_CognitiveDissonance
            : EnhancedIdeologyDefOf.EB_FaithReaffirmed;

        var counterThought = (Thought_CognitiveDissonance)ThoughtMaker.MakeThought(counterDef);
        counterThought.StoredMoodOffset = counterOffset;
        counterThought.sourcePrecept = newThought.sourcePrecept;
        __instance.TryGainMemory(counterThought);
    }
}
