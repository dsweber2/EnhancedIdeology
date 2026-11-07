namespace EnhancedIdeology;

public enum IdeoChangeNotificationMode
{
    Letter,
    Message,
    Off,
}

// Tells the player that a pawn changed ideoligion. The mod setting selects a letter, a message, or nothing.
// Only a change that involves a colonist, a prisoner, or a slave of the colony is reported.
internal static class IdeoChangeNotifier
{
    internal static void Notify(string label, string text, LookTargets lookTargets, params Pawn[] involved)
    {
        if (!involved.Any(pawn => pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony))
            return;

        switch (EnhancedIdeologyMod.Settings.IdeoChangeNotification)
        {
            case IdeoChangeNotificationMode.Letter:
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, lookTargets);
                break;
            case IdeoChangeNotificationMode.Message:
                Messages.Message(text, lookTargets, MessageTypeDefOf.NeutralEvent);
                break;
        }
    }

    // Notifies a conversion that has no other source of text. textKey takes PAWN, OLDIDEO and NEWIDEO.
    // Get oldRole before the conversion: the change of ideoligion unassigns it.
    internal static void NotifyConversion(Pawn pawn, Ideo oldIdeo, Precept_Role? oldRole, string textKey, params NamedArgument[] extraArgs)
    {
        var args = new List<NamedArgument> { pawn.Named("PAWN"), oldIdeo.Named("OLDIDEO"), pawn.Ideo!.Named("NEWIDEO") };
        args.AddRange(extraArgs);
        Notify("EnhancedIdeology.LetterLabelIdeoChange".Translate(),
            textKey.Translate(args.ToArray()).Resolve() + RoleLostPostfix(pawn, oldIdeo, oldRole),
            pawn,
            pawn);
    }

    internal static string RoleLostPostfix(Pawn pawn, Ideo oldIdeo, Precept_Role? oldRole) =>
        oldRole == null
            ? string.Empty
            : "\n\n" + "LetterRoleLostLetterIdeoChangedPostfix".Translate(
                pawn.Named("PAWN"), oldRole.Named("ROLE"), oldIdeo.Named("OLDIDEO")).Resolve();
}
