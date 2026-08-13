using Verse;

namespace RimWorld;

public class MemeDef : Def
{
    public List<string> exclusionTags = [];
    public List<TraitRequirement> agreeableTraits = [];
    public List<TraitRequirement> disagreeableTraits = [];
    public string? category;
}

public class PreceptDef : Def
{
    public IssueDef? issue;
    public int displayOrderInIssue;
    public bool classic;
    public List<PreceptComp> comps = [];
    public List<MemeDef> requiredMemes = [];
    public List<MemeDef> associatedMemes = [];

    public void AddComp(PreceptComp comp) => comps.Add(comp);
}

public abstract class PreceptComp { }

public class Precept
{
    public PreceptDef def = new();
    // No TryGetComps instance method — EnhancedIdeologyUtilities provides the extension
}

public class IdeoRoleDef : Def
{
    public float certaintyLossFactor = 1f;
}

public class IdeoRole
{
    public IdeoRoleDef def = new();
}

public class ThoughtWorker_Precept : Verse.ThoughtWorker { }

// Weapons issue payload: each precept reveres one weapon class and despises another. Two ideos clash when one
// reveres what the other despises (see PreceptPolicy weapons handling).
public class WeaponClassDef : Def { }

public class Precept_Weapon : Precept
{
    public WeaponClassDef? noble;
    public WeaponClassDef? despised;
}

// PreferredXenotypes payload: each precept names a preferred xenotype (a def, or a custom one carrying a name).
public class XenotypeDef : Def { }

public class CustomXenotype
{
    public string name = string.Empty;
}

public class Precept_Xenotype : Precept
{
    public XenotypeDef? xenotype;
    public CustomXenotype? customXenotype;
}

public class Ideo
{
    public string name = string.Empty;
    public List<MemeDef> memes = [];
    public List<Precept> precepts = [];

    public bool HasMeme(MemeDef? meme) => meme != null && memes.Contains(meme);

    public IdeoRole? GetRole(Pawn pawn) => null;

    public void Notify_MemberGainedByConversion() { }

    public override string ToString() => name;
}
