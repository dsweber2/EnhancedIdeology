using Verse.AI;

namespace EnhancedIdeology;

// Iconoclast agitation: walk up to one pawn, debate them once, then linger. Does not pin the recipient.
internal sealed class JobDriver_IconoclastAgitate : JobDriver
{
    private const int HarangueTicks = 240;

    private Pawn Recipient => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

    public override string GetReport() =>
        "EnhancedIdeology.JobReport_Debating".Translate(Recipient.LabelShort);

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOn(() => pawn.MentalState is not MentalState_Iconoclast || Recipient.Downed);

        yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
        yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);

        var harangue = ToilMaker.MakeToil("IconoclastHarangue");
        harangue.initAction = () =>
        {
            var debated = pawn.interactions.TryInteractWith(Recipient, EnhancedIdeologyDefOf.EB_IdeologicalDebatePrecept);
            Log.Message($"[EB] Iconoclast {pawn.LabelShort} agitating {Recipient.LabelShort}: debated={debated}");
        };
        harangue.defaultDuration = HarangueTicks;
        harangue.defaultCompleteMode = ToilCompleteMode.Delay;
        harangue.handlingFacing = true;
        harangue.tickAction = () => pawn.rotationTracker.FaceTarget(Recipient);
        yield return harangue;
    }
}
