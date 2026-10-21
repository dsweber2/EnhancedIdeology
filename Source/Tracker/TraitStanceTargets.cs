namespace EnhancedIdeology;

// The stance a pawn's traits imply on an issue (docs/design.md "Trait stance targets").
// A support target (Rung set) pulls the stance to its rung. An opposition target (Rung null) makes the Opposed
// spots a no-go region: a stance on an opposed spot exits to the nearest allowed spot, and a stance on an allowed
// spot keeps its rank. Votes is the weight of the votes behind the target, negative for an opposition target.
// Spots are the issue's rungs plus an off-ladder Don't-care, in rank order.
internal sealed record TraitStanceTarget(
    float? Rung, float Votes, IReadOnlyDictionary<float, float> Opposed, IReadOnlyList<float> Spots, float DontCare)
{
    // Rank and strength that the traits pull a stance at `rank` toward.
    internal (float rank, float strength) PullFrom(float rank)
    {
        if (Rung is { } rung) return (rung, StrengthOf(Votes));
        var spot = NearestSpot(rank);
        return Opposed.TryGetValue(spot, out var weight)
            ? (Exit(spot, rank), StrengthOf(weight))
            : (rank, StrengthOf(-Votes));
    }

    // Opposed weight on the spot nearest to `rank`; 0 for an allowed spot or a support target.
    internal float OpposedWeight(float rank) =>
        Rung == null && Opposed.TryGetValue(NearestSpot(rank), out var weight) ? weight : 0f;

    private static float StrengthOf(float votes) =>
        Mathf.Min(ConvictionScale.TraitVoteStrength(votes), ConvictionScale.MaxConvictionStrength);

    private float NearestSpot(float rank)
    {
        var nearest = Spots[0];
        foreach (var spot in Spots)
            if (Mathf.Abs(spot - rank) < Mathf.Abs(nearest - rank))
                nearest = spot;
        return nearest;
    }

    // The nearest allowed spot on the side away from the opposed centroid. On the centroid, the side with
    // Don't-care. The other side when that side has no allowed spot.
    private float Exit(float spot, float rank)
    {
        var centroid = Opposed.Sum(kv => kv.Key * kv.Value) / Opposed.Values.Sum();
        var upward = rank != centroid ? rank > centroid : DontCare > spot;
        return NearestAllowed(spot, upward) ?? NearestAllowed(spot, !upward)!.Value;
    }

    private float? NearestAllowed(float spot, bool upward)
    {
        var candidates = Spots.Where(other => !Opposed.ContainsKey(other) && (upward ? other > spot : other < spot));
        return upward ? candidates.Cast<float?>().FirstOrDefault() : candidates.Cast<float?>().LastOrDefault();
    }
}

// Per-issue stance targets from a pawn's traits (docs/trait-meme-affinity.md). Read by seeding, brainwipe, and
// daily decay. Each trait link is a signed vote:
// - a rung link (TraitIssueLinks.rung) votes +1 for its rung;
// - an opposes link (TraitIssueLinks.opposes) votes -1 against the issue's valued rung;
// - an indifferent link (TraitIssueLinks.indifferent) votes +1 for the issue's indifferent rank;
// - a meme that lists the trait in agreeableTraits / disagreeableTraits votes +1 / -DisagreeableMemeWeight on
//   each issue the meme links to, once per listing. A link through associatedMemes counts at AssociatedMemeWeight.
// Each rung nets its support less its opposition. If a rung nets above zero, the target is the best-supported
// rung, the closest one on a tie; its votes are that rung's net. Otherwise the rungs that net below zero are an
// opposition target. No votes, or no allowed spot, give no target. UntargetedIssues never get a target.
// The faith does not matter: the targets come from the pawn's traits only.
internal static class TraitStanceTargets
{
    internal const float AssociatedMemeWeight = 0.5f;
    // Disliking a meme does not oppose every precept the meme requires as firmly as liking it supports them.
    internal const float DisagreeableMemeWeight = 0.5f;

    // An ideo holds many roles at once, so a rank on the role list has no meaning.
    private static readonly HashSet<string> UntargetedIssues = ["IdeoRole"];

    // Meme -> issue -> rung rank -> weight of the strongest route from the meme to that rung.
    private static Dictionary<MemeDef, Dictionary<IssueDef, Dictionary<float, float>>>? _memeRungs;

    // Test hook: tests swap the MemeDef and PreceptDef databases between cases.
    internal static void ClearMemeCache() => _memeRungs = null;

    private static Dictionary<MemeDef, Dictionary<IssueDef, Dictionary<float, float>>> MemeRungs
    {
        get
        {
            if (_memeRungs != null) return _memeRungs;
            _memeRungs = [];
            foreach (var meme in DefDatabase<MemeDef>.AllDefs)
            {
                if (!meme.requireOne.NullOrEmpty())
                    foreach (var group in meme.requireOne)
                        foreach (var precept in group)
                            AddMemeRung(meme, precept, 1f);
                if (meme.selectOneOrNone?.preceptThingPairs != null)
                    foreach (var pair in meme.selectOneOrNone.preceptThingPairs)
                        AddMemeRung(meme, pair?.precept, 1f);
            }
            foreach (var precept in DefDatabase<PreceptDef>.AllDefs)
            {
                foreach (var meme in precept.requiredMemes)
                    AddMemeRung(meme, precept, 1f);
                foreach (var meme in precept.associatedMemes)
                    AddMemeRung(meme, precept, AssociatedMemeWeight);
            }
            return _memeRungs;
        }
    }

    private static void AddMemeRung(MemeDef meme, PreceptDef? precept, float weight)
    {
        if (precept?.issue is not { } issue || PreceptPolicy.CategoryOf(issue) == PreceptCategory.NA) return;
        var rank = PreceptLadder.RankOf(precept);
        if (rank < 0f) return;
        if (!_memeRungs!.TryGetValue(meme, out var byIssue)) _memeRungs[meme] = byIssue = [];
        if (!byIssue.TryGetValue(issue, out var byRank)) byIssue[issue] = byRank = [];
        byRank[rank] = Mathf.Max(byRank.GetValueOrDefault(rank), weight);
    }

    // Targets for every issue in `ranks` (the pawn's current stance ranks) that the pawn's traits vote on.
    // When `sources` is given, it receives one line per vote, for the debug log.
    internal static Dictionary<IssueDef, TraitStanceTarget> For(
        Pawn pawn, IReadOnlyDictionary<IssueDef, float> ranks, Dictionary<IssueDef, List<string>>? sources = null)
    {
        var totalWeight = new Dictionary<IssueDef, float>();
        var net = new Dictionary<IssueDef, Dictionary<float, float>>();

        // One vote of `weight` on `issue`, supporting (weight > 0) or opposing (weight < 0) each rung by its own weight.
        void Vote(IssueDef issue, float weight, IEnumerable<(float rank, float weight)> rungs, string? source)
        {
            if (!ranks.ContainsKey(issue) || UntargetedIssues.Contains(issue.defName)) return;
            totalWeight[issue] = totalWeight.GetValueOrDefault(issue) + weight;
            if (!net.TryGetValue(issue, out var byRank)) net[issue] = byRank = [];
            foreach (var (rank, rungWeight) in rungs)
                byRank[rank] = byRank.GetValueOrDefault(rank) + Mathf.Sign(weight) * rungWeight;
            if (sources == null) return;
            if (!sources.TryGetValue(issue, out var lines)) sources[issue] = lines = [];
            var rungText = string.Join(" ", rungs.Select(rung => $"{RungName(issue, rung.rank)}x{rung.weight:0.##}"));
            lines.Add($"{weight:+0.##;-0.##} {source} -> {rungText}");
        }

        foreach (var (trait, link) in TraitIssueLinks.ActiveFor(pawn))
        {
            var source = sources == null ? null : $"trait {TraitName(trait.def, trait.Degree)}";
            if (link.Rung is { issue: { } issue } rung && PreceptLadder.RankOf(rung) is var rank and >= 0f)
            {
                Vote(issue, 1f, [(rank, 1f)], source);
            }
            else if (link.opposes != null
                && DefDatabase<IssueDef>.GetNamedSilentFail(link.opposes) is { } opposedIssue
                && PreceptPolicy.ValuedRank(opposedIssue) is { } valuedRank)
            {
                Vote(opposedIssue, -1f, [(valuedRank, 1f)], source + " opposes");
            }
            else if (link.indifferent != null
                && DefDatabase<IssueDef>.GetNamedSilentFail(link.indifferent) is { } indifferentIssue)
            {
                Vote(indifferentIssue, 1f, [(PreceptLadder.IndifferentRank(indifferentIssue), 1f)], source + " indifferent");
            }
        }

        foreach (var (meme, byIssue) in MemeRungs)
        {
            var listings = MemeListings(meme, pawn);
            if (listings == 0f) continue;
            var scale = Mathf.Abs(listings);
            var source = sources == null ? null : $"meme {meme.defName} via {MemeListingNames(meme, pawn)}";
            foreach (var (issue, byRank) in byIssue)
                Vote(issue, listings * byRank.Values.Max(), byRank.Select(kv => (kv.Key, scale * kv.Value)), source);
        }

        var targets = new Dictionary<IssueDef, TraitStanceTarget>();
        foreach (var (issue, weight) in totalWeight)
        {
            var current = ranks[issue];
            var dontCare = PreceptLadder.DontCareRank(issue);
            var supported = net[issue].Where(kv => kv.Value > 0f).ToList();
            if (supported.Count > 0)
            {
                var (rung, rungNet) = supported
                    .OrderByDescending(kv => kv.Value).ThenBy(kv => Mathf.Abs(kv.Key - current)).First();
                targets[issue] = new TraitStanceTarget(rung, rungNet, new Dictionary<float, float>(), [], dontCare);
                continue;
            }

            var opposed = net[issue].Where(kv => kv.Value < 0f).ToDictionary(kv => kv.Key, kv => -kv.Value);
            var spots = Spots(issue, dontCare);
            if (opposed.Count == 0 || spots.All(opposed.ContainsKey)) continue;
            targets[issue] = new TraitStanceTarget(null, weight, opposed, spots, dontCare);
        }
        return targets;
    }

    // The issue's rung ranks plus Don't-care when it is off the rungs, in rank order.
    private static List<float> Spots(IssueDef issue, float dontCare)
    {
        var spots = Enumerable.Range(0, PreceptLadder.Rungs(issue).Count).Select(index => (float)index).ToList();
        if (!spots.Contains(dontCare))
        {
            spots.Add(dontCare);
            spots.Sort();
        }
        return spots;
    }

    // Times the pawn's traits appear in the meme's agreeableTraits, less DisagreeableMemeWeight per time in its
    // disagreeableTraits.
    private static float MemeListings(MemeDef meme, Pawn pawn)
    {
        var listings = 0f;
        if (!meme.agreeableTraits.NullOrEmpty())
            foreach (var trait in meme.agreeableTraits)
                if (trait.HasTrait(pawn)) listings += 1f;
        if (!meme.disagreeableTraits.NullOrEmpty())
            foreach (var trait in meme.disagreeableTraits)
                if (trait.HasTrait(pawn)) listings -= DisagreeableMemeWeight;
        return listings;
    }

    // The pawn's traits that the meme lists, "+" for agreeable and "-" for disagreeable.
    private static string MemeListingNames(MemeDef meme, Pawn pawn)
    {
        var agreeable = (meme.agreeableTraits ?? []).Where(trait => trait.HasTrait(pawn))
            .Select(trait => "+" + TraitName(trait.def!, trait.degree));
        var disagreeable = (meme.disagreeableTraits ?? []).Where(trait => trait.HasTrait(pawn))
            .Select(trait => "-" + TraitName(trait.def!, trait.degree));
        return string.Join(" ", agreeable.Concat(disagreeable));
    }

    private static string TraitName(TraitDef def, int? degree) =>
        degree is { } value && value != 0 ? $"{def.defName}({value})" : def.defName;

    // The rung's defName at an integer rank, otherwise the rank as a number (Don't-care or a fractional stance).
    internal static string RungName(IssueDef issue, float rank)
    {
        var rungs = PreceptLadder.Rungs(issue);
        var index = Mathf.RoundToInt(rank);
        return rank == index && index >= 0 && index < rungs.Count
            ? rungs[index].defName
            : rank.ToString("0.##");
    }
}
