using Verse;
using UnityEngine;

namespace RimWorld;

public class TraitRequirement
{
    public TraitDef? def;
    public int? degree;

    public bool HasTrait(Pawn pawn)
    {
        if (def == null) return false;
        var trait = pawn.story.traits.GetTrait(def);
        if (trait == null) return false;
        return degree == null || trait.Degree == degree.Value;
    }
}

public class InteractionDef : Def
{
    public float socialFightBaseChance;
}

public abstract class InteractionWorker
{
    public InteractionDef interaction = new();

    public virtual float RandomSelectionWeight(Pawn initiator, Pawn recipient) => 0f;

    public virtual void Interacted(
        Pawn initiator,
        Pawn recipient,
        List<RulePackDef> extraSentencePacks,
        out string? letterText,
        out string? letterLabel,
        out LetterDef? letterDef,
        out LookTargets? lookTargets)
    {
        letterText = null;
        letterLabel = null;
        letterDef = null;
        lookTargets = null;
    }
}

public abstract class InteractionWorker_ConvertIdeoAttempt : InteractionWorker { }

public static class ConversionUtility
{
    public static float ConversionPowerFactor_MemesVsTraits(Pawn initiator, Pawn recipient, System.Text.StringBuilder? sb = null)
    {
        float offset = OffsetFromIdeo(initiator, recipient, invert: false)
                     + OffsetFromIdeo(recipient, recipient, invert: true);
        return Mathf.Max(1f + offset, -0.4f);
    }

    private static float OffsetFromIdeo(Pawn ideoPawn, Pawn recipient, bool invert)
    {
        if (ideoPawn.Ideo == null) return 0f;
        float offset = 0f;
        foreach (var meme in ideoPawn.Ideo.memes)
        {
            foreach (var req in meme.agreeableTraits)
                if (req.HasTrait(recipient)) offset += invert ? -0.2f : 0.2f;
            foreach (var req in meme.disagreeableTraits)
                if (req.HasTrait(recipient)) offset += invert ? 0.2f : -0.2f;
        }
        return offset;
    }
}

public static class ReliquaryUtility
{
    public static float GetRelicConvertPowerFactorForPawn(Pawn pawn) => 1f;
}
