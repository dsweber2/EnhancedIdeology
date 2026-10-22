namespace EnhancedIdeology;

// Whether a drawn debate boils over into a social fight, and starting that fight (docs/design.md "Sources of change").
internal static class DebateFights
{
    // Iconoclast draw fight chance: bypasses SocialFightPossible (which gates on WorkTags.Violent) because
    // a mental break overrides the pawn's normal pacifism. ~75% fight chance per draw.
    internal const float IconoclastDrawFightChance = 0.75f;

    // Base chance per draw, fed to vanilla SocialFightChance. With about one debate in five drawn, an evenly
    // matched full-ladder debate between 0-social pawns fights about as often as a vanilla Slight (0.005).
    internal const float DrawFightBaseChance = 0.026f;

    // Social skill of the more adept debater defuses the fight: 1 / (1 + falloff * skill).
    internal const float SocialFalloff = 0.2f;

    // Topic weight of a full-ladder rung gap (InteractionWorker_IdeologicalDebatePrecept.TopicWeight). The fight
    // chance scales with the topic weight relative to it, so petty disagreements rarely come to blows.
    internal const float ReferenceTopicWeight = 20f;

    // Per-pawn factors at the ends of the IdeoDiversity ladder. The tolerant end applies to every debate; the
    // bigoted end only across faiths.
    internal const float TolerantFightFactor = 0.2f;
    internal const float BigotedFightFactor = 2f;

    // Added to the cross-faith fight multiplier per faith, scaled by how strict its apostasy precept is.
    internal const float ApostacyFightFactorPerFaith = 0.4f;

    internal static float Chance(InteractionDef interaction, Pawn initiator, Pawn recipient, float topicWeight)
    {
        if (initiator.MentalState is MentalState_Iconoclast || recipient.MentalState is MentalState_Iconoclast)
        {
            // Mental break overrides normal violence restrictions — use a flat chance instead of SocialFightChance.
            // If either pawn can't fight (pacifist ideology, trait, etc.) skip the fight entirely; repeated debates
            // will naturally occur because the interloper is still present when the iconoclast's next job fires.
            return initiator.interactions?.SocialFightPossible(recipient) == true
                && recipient.interactions?.SocialFightPossible(initiator) == true
                ? IconoclastDrawFightChance
                : 0f;
        }

        var crossFaith = initiator.Ideo != recipient.Ideo;
        interaction.socialFightBaseChance = DrawFightBaseChance;
        // A despawned pawn (in a caravan) has no interactions tracker. SocialFightChance reads only the pawn, so a
        // temporary tracker gives the same result.
        var initiatorChance = (initiator.interactions ?? new Pawn_InteractionsTracker(initiator)).SocialFightChance(interaction, recipient)
            * DiversityFightFactor(initiator.Ideo!, crossFaith);
        var recipientChance = (recipient.interactions ?? new Pawn_InteractionsTracker(recipient)).SocialFightChance(interaction, initiator)
            * DiversityFightFactor(recipient.Ideo!, crossFaith);
        interaction.socialFightBaseChance = 0f;

        var maxSocial = Mathf.Max(initiator.skills.GetSkill(SkillDefOf.Social).Level, recipient.skills.GetSkill(SkillDefOf.Social).Level);
        var chance = (initiatorChance + recipientChance) / 2f
            / (1f + (SocialFalloff * maxSocial))
            * (topicWeight / ReferenceTopicWeight);

        // Pawns from strict-apostacy faiths have less tolerance for being stalemated by a heretic.
        if (crossFaith)
        {
            chance *= 1f
                + (EnhancedIdeologyUtilities.ApostacyStrictness(initiator.Ideo) * ApostacyFightFactorPerFaith)
                + (EnhancedIdeologyUtilities.ApostacyStrictness(recipient.Ideo) * ApostacyFightFactorPerFaith);
        }

        EnhancedIdeologyMod.DebugIf(EnhancedIdeologyMod.Settings.DebugInteractionWorkers,
            $"DebateFights.Chance: initiatorChance={initiatorChance}, recipientChance={recipientChance}, maxSocial={maxSocial}, topicWeight={topicWeight}, chance={chance}");
        return chance;
    }

    // Lerps from 1 at Standard to TolerantFightFactor at Exalted. On the bigoted side it lerps to
    // BigotedFightFactor at Abhorrent, but only across faiths.
    internal static float DiversityFightFactor(Ideo ideo, bool crossFaith)
    {
        var axis = DiversityAxis.Of(ideo);
        if (axis == null)
            return 1f;

        var held = axis.Value.HeldDelta;
        if (held > 0f && axis.Value.DeltaOf("IdeoDiversity_Exalted") is { } exalted && exalted > 0f)
            return Mathf.Lerp(1f, TolerantFightFactor, held / exalted);
        if (held < 0f && crossFaith && axis.Value.DeltaOf("IdeoDiversity_Abhorrent") is { } abhorrent && abhorrent < 0f)
            return Mathf.Lerp(1f, BigotedFightFactor, held / abhorrent);
        return 1f;
    }

    // When an iconoclast is involved, StartSocialFight fails silently — TryStartMentalState(SocialFighting) can't
    // replace an already-active mental state. Issue paired AttackMelee jobs directly instead and track the fight
    // in MentalState_Iconoclast so the job givers don't interrupt it.
    internal static void Start(Pawn initiator, Pawn recipient)
    {
        if (initiator.GetCaravan() != null)
        {
            CaravanDebates.SettleFight(initiator, recipient);
            return;
        }

        var iconoclast = initiator.MentalState as MentalState_Iconoclast ?? recipient.MentalState as MentalState_Iconoclast;
        if (iconoclast != null)
        {
            var other = iconoclast.pawn == initiator ? recipient : initiator;
            iconoclast.fightTarget = other;
            iconoclast.ClearDebateTargets();

            // Give the other pawn SocialFighting. IsOtherPawnSocialFightingWithMe is patched to return
            // true when the opponent is an iconoclast with matching fightTarget, so the state persists.
            other.mindState.mentalStateHandler.TryStartMentalState(
                MentalStateDefOf.SocialFighting, null, forced: false, forceWake: false,
                causedByMood: false, iconoclast.pawn);

            if (PawnUtility.ShouldSendNotificationAbout(iconoclast.pawn) || PawnUtility.ShouldSendNotificationAbout(other))
                Messages.Message("MessageSocialFight".Translate(iconoclast.pawn.LabelShort, other.LabelShort, iconoclast.pawn.Named("PAWN1"), other.Named("PAWN2")), iconoclast.pawn, MessageTypeDefOf.ThreatSmall);
            TaleRecorder.RecordTale(TaleDefOf.SocialFight, iconoclast.pawn, other);
        }
        else
        {
            recipient.interactions.StartSocialFight(initiator, "EnhancedIdeology.IdeologicalDebateOutcomeSocialFight");
        }
    }
}

// A faith's position on the IdeoDiversity ladder, as signed rung distances from Standard (positive toward
// Approved). Direction-agnostic, so it holds however the rungs are ordered.
internal readonly struct DiversityAxis
{
    private readonly IssueDef issue;
    private readonly float standard;
    private readonly float direction;

    private DiversityAxis(IssueDef issue, float standard, float direction, float heldDelta)
    {
        this.issue = issue;
        this.standard = standard;
        this.direction = direction;
        HeldDelta = heldDelta;
    }

    public float HeldDelta { get; }

    // Null when the faith holds no diversity precept or the ladder lacks Standard or Approved.
    public static DiversityAxis? Of(Ideo ideo)
    {
        var precept = ideo.precepts.FirstOrDefault(p => p.def.issue?.defName == "IdeoDiversity");
        if (precept == null)
            return null;

        var issue = precept.def.issue!;
        var standard = PreceptLadder.RankOfName(issue, "IdeoDiversity_Standard");
        var approved = PreceptLadder.RankOfName(issue, "IdeoDiversity_Approved");
        if (standard < 0f || approved < 0f)
            return null;

        var direction = Math.Sign(approved - standard);
        return new DiversityAxis(issue, standard, direction, (PreceptLadder.RankOf(precept.def) - standard) * direction);
    }

    // Null when the rung is absent from the live ladder.
    public float? DeltaOf(string defName)
    {
        var rank = PreceptLadder.RankOfName(issue, defName);
        return rank < 0f ? null : (rank - standard) * direction;
    }
}
