namespace EnhancedIdeology.Tests.Support;

internal sealed class IdeoBuilder
{
    private readonly Ideo _ideo = new();

    public IdeoBuilder WithName(string name)
    {
        _ideo.name = name;
        return this;
    }

    public IdeoBuilder AddMeme(MemeDef meme)
    {
        _ideo.memes.Add(meme);
        return this;
    }

    // Give the ideo a stance on an issue. The precept is registered as a rung of that issue's ladder (so
    // PreceptLadder can rank it) and held by the ideo. displayOrderInIssue sets its ladder position; when
    // several rungs share an issue, register them all (each via AddPrecept on some ideo, or SimIssues).
    public IdeoBuilder AddPrecept(PreceptDef def, IssueDef? issue = null, int displayOrderInIssue = 0)
    {
        if (issue != null)
        {
            def.issue = issue;
            def.displayOrderInIssue = displayOrderInIssue;
            SimIssues.Register(def);
        }

        _ideo.precepts.Add(new Precept { def = def, ideo = _ideo });
        return this;
    }

    // Hold a Weapons precept revering `noble` and despising `despised` (a Special payload issue, no ladder).
    public IdeoBuilder AddWeaponPrecept(IssueDef issue, WeaponClassDef? noble, WeaponClassDef? despised)
    {
        _ideo.precepts.Add(new Precept_Weapon
        {
            def = new PreceptDef { defName = $"Weapon_{_ideo.name}_{_ideo.precepts.Count}", issue = issue },
            ideo = _ideo,
            noble = noble,
            despised = despised,
        });
        return this;
    }

    // Hold a PreferredXenotypes precept favouring `xenotype` (a Special payload issue, no ladder).
    public IdeoBuilder AddXenotypePrecept(IssueDef issue, XenotypeDef xenotype)
    {
        // Every PreferredXenotype instance shares the one PreferredXenotype PreceptDef in-game; mirror that so
        // couplings keyed on the precept defName resolve.
        _ideo.precepts.Add(new Precept_Xenotype
        {
            def = new PreceptDef { defName = "PreferredXenotype", issue = issue },
            ideo = _ideo,
            xenotype = xenotype,
        });
        return this;
    }

    public Ideo Build() => _ideo;
}

// Registers issues and their precept rungs into the global DefDatabase so PreceptLadder resolves them.
internal static class SimIssues
{
    public static void Register(PreceptDef rung)
    {
        if (rung.issue != null && !DefDatabase<IssueDef>.AllDefs.Contains(rung.issue))
        {
            DefDatabase<IssueDef>.Add(rung.issue);
            // Test-built issues carry a rung ladder to exercise the rung-distance model, so treat them as
            // Moral unless a test says otherwise; real issues resolve through PreceptPolicy's own tables.
            PreceptPolicy.RegisterCategory(rung.issue.defName, PreceptCategory.Moral);
        }

        if (!DefDatabase<PreceptDef>.AllDefs.Contains(rung))
        {
            DefDatabase<PreceptDef>.Add(rung);
        }
    }

    // A Special payload issue (Weapons, PreferredXenotypes): registered so stances are seeded and it enters the
    // relevant-issues set, but classified Special rather than the default Moral so it routes to the payload
    // resolver instead of the rung model. Returns the existing def if already registered.
    public static IssueDef Special(string issueName)
    {
        var existing = DefDatabase<IssueDef>.GetNamedSilentFail(issueName);
        if (existing != null)
        {
            return existing;
        }

        var issue = new IssueDef { defName = issueName };
        DefDatabase<IssueDef>.Add(issue);
        PreceptPolicy.RegisterCategory(issueName, PreceptCategory.Special);
        return issue;
    }

    // Define a full ladder for an issue: rungs given permissive-to-forbidding, spaced by 10. Registers all
    // rungs (none held) and returns them so a caller can make an ideo hold one.
    public static (IssueDef issue, PreceptDef[] rungs) Ladder(string issueName, params string[] rungNames)
    {
        var issue = new IssueDef { defName = issueName };
        var rungs = new PreceptDef[rungNames.Length];
        for (var ii = 0; ii < rungNames.Length; ii++)
        {
            rungs[ii] = new PreceptDef { defName = rungNames[ii], issue = issue, displayOrderInIssue = ii * 10 };
            Register(rungs[ii]);
        }

        return (issue, rungs);
    }
}

internal sealed class MemeBuilder
{
    private readonly MemeDef _meme = new();

    public MemeBuilder WithName(string name)
    {
        _meme.defName = name;
        return this;
    }

    public MemeBuilder WithExclusionTag(string tag)
    {
        _meme.exclusionTags.Add(tag);
        return this;
    }

    public MemeBuilder WithAgreeableTrait(TraitDef trait, int? degree = null)
    {
        _meme.agreeableTraits.Add(new TraitRequirement { def = trait, degree = degree });
        return this;
    }

    public MemeBuilder WithDisagreeableTrait(TraitDef trait, int? degree = null)
    {
        _meme.disagreeableTraits.Add(new TraitRequirement { def = trait, degree = degree });
        return this;
    }

    public MemeDef Build() => _meme;
}
