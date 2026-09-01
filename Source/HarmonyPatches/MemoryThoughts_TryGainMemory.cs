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

        var offset = newThought.MoodOffset();

        if (newThought.sourcePrecept.ideo == pawn.Ideo)
        {
            var issue = newThought.sourcePrecept.def.issue;
            if (issue != null && Mathf.Abs(offset) > 0.01f)
            {
                var comp = Current.Game.GetComponent<GameComponent_EnhancedIdeology>();
                var tracker = comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);
                tracker.ShiftIssueStance(issue, 0f, 0f, offset * 0.01f);
            }
            return;
        }

        float counterMultiplier = 0f;
        MemeDef? triggeringMeme = null;

        var gestalt = EnhancedIdeologyDefOf.VME_Gestalt;
        var nationalist = EnhancedIdeologyDefOf.VME_Nationalist;
        var isolationist = EnhancedIdeologyDefOf.VFEA_Isolationist;
        var violentConversion = EnhancedIdeologyDefOf.VME_ViolentConversion;
        var egalitarian = EnhancedIdeologyDefOf.VME_Egalitarian;
        var emancipation = EnhancedIdeologyDefOf.VME_Emancipation;
        if (gestalt != null && pawn.Ideo.HasMeme(gestalt))
            (counterMultiplier, triggeringMeme) = (-2f, gestalt);
        else if (isolationist != null && pawn.Ideo.HasMeme(isolationist))
            (counterMultiplier, triggeringMeme) = (-1.5f, isolationist);
        else if (violentConversion != null && pawn.Ideo.HasMeme(violentConversion))
            (counterMultiplier, triggeringMeme) = (-1.5f, violentConversion);
        else if (pawn.Ideo.HasMeme(EnhancedIdeologyDefOf.Supremacist))
            (counterMultiplier, triggeringMeme) = (-1f, EnhancedIdeologyDefOf.Supremacist);
        else if (pawn.Ideo.HasMeme(EnhancedIdeologyDefOf.Collectivist))
            (counterMultiplier, triggeringMeme) = (-1f, EnhancedIdeologyDefOf.Collectivist);
        else if (nationalist != null && pawn.Ideo.HasMeme(nationalist))
            (counterMultiplier, triggeringMeme) = (-1f, nationalist);
        else if (pawn.Ideo.HasMeme(EnhancedIdeologyDefOf.Loyalist))
            (counterMultiplier, triggeringMeme) = (-0.5f, EnhancedIdeologyDefOf.Loyalist);
        else if (pawn.Ideo.HasMeme(EnhancedIdeologyDefOf.Guilty))
            (counterMultiplier, triggeringMeme) = (0.5f, EnhancedIdeologyDefOf.Guilty);
        else if (egalitarian != null && pawn.Ideo.HasMeme(egalitarian))
            (counterMultiplier, triggeringMeme) = (0.5f, egalitarian);
        else if (emancipation != null && pawn.Ideo.HasMeme(emancipation))
            (counterMultiplier, triggeringMeme) = (0.5f, emancipation);
        else if (pawn.Ideo.HasMeme(EnhancedIdeologyDefOf.Individualist))
            (counterMultiplier, triggeringMeme) = (0.5f, EnhancedIdeologyDefOf.Individualist);

        if (counterMultiplier == 0f) return;

        var counterOffset = counterMultiplier * Mathf.Abs(offset);
        if (Mathf.Abs(counterOffset) < 0.01f) return;

        var counterDef = counterOffset < 0
            ? EnhancedIdeologyDefOf.EB_CognitiveDissonance
            : EnhancedIdeologyDefOf.EB_FaithReaffirmed;

        var sourcePrecept = triggeringMeme != null
            ? pawn.Ideo.precepts.FirstOrDefault(pp => pp.def.associatedMemes?.Contains(triggeringMeme) == true)
            : null;

        var counterThought = (Thought_CognitiveDissonance)ThoughtMaker.MakeThought(counterDef);
        counterThought.StoredMoodOffset = counterOffset;
        counterThought.sourcePrecept = sourcePrecept;
        counterThought.SourceMemeLabel = sourcePrecept == null ? triggeringMeme?.LabelCap : null;
        __instance.TryGainMemory(counterThought);
    }
}
