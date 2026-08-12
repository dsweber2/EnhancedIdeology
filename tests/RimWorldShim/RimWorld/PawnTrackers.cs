using Verse;

namespace RimWorld;

public class SkillDef : Def { }

public class SkillRecord
{
    public int Level = 10;
    public bool TotallyDisabled = false;
}

public class Pawn_SkillTracker
{
    private readonly Dictionary<SkillDef, SkillRecord> _skills = [];

    public SkillRecord GetSkill(SkillDef def)
    {
        if (!_skills.TryGetValue(def, out var record))
        {
            record = new SkillRecord();
            _skills[def] = record;
        }
        return record;
    }

    public void SetSkillLevel(SkillDef def, int level)
        => GetSkill(def).Level = level;
}

public class Pawn_IdeoTracker(Pawn pawn)
{
    public readonly Pawn pawn = pawn;
    public Ideo? ideo;
    public float Certainty;
    public readonly List<Ideo> PreviousIdeos = [];

    public void SetIdeo(Ideo newIdeo)
    {
        if (ideo != null && !PreviousIdeos.Contains(ideo))
            PreviousIdeos.Add(ideo);
        ideo = newIdeo;
    }

    public float ApplyCertaintyChangeFactor(float delta)
        => delta * pawn.GetStatValue(StatDefOf.CertaintyLossFactor);

    public bool IdeoConversionAttempt(float certaintyReduction, Ideo initiatorIdeo, bool applyCertaintyFactor = true)
        => false;
}

public class Pawn_RelationTracker(Pawn pawn)
{
    private readonly Pawn _pawn = pawn;
    private readonly Dictionary<Pawn, float> _opinions = [];

    public float OpinionOf(Pawn other)
        => _opinions.GetValueOrDefault(other, 0f);

    public void SetOpinion(Pawn other, float value)
        => _opinions[other] = value;

    public float CompatibilityWith(Pawn other)
        => 0f;
}

public class Pawn_InteractionsTracker
{
    public float SocialFightChance(InteractionDef interaction, Pawn other) => 0f;
    public bool SocialFightPossible(Pawn other) => false;
    public void StartSocialFight(Pawn other, string message) { }
}

public class MentalStateDef : Def { }

public class MentalStateHandler
{
    public bool TryStartMentalState(MentalStateDef? def, string? reason = null, bool forceWake = false)
        => false;
}
