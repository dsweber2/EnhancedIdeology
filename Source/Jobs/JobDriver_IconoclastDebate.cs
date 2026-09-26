using Verse.AI;

namespace EnhancedIdeology;

internal sealed class JobDriver_IconoclastDebate : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

    public override string GetReport()
    {
        var target = job.GetTarget(TargetIndex.A).Pawn;
        return target != null
            ? "EnhancedIdeology.JobReport_Debating".Translate(target.LabelShort)
            : base.GetReport();
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        var standToil = ToilMaker.MakeToil("IconoclastDebateStand");
        standToil.defaultDuration = 240;
        standToil.defaultCompleteMode = ToilCompleteMode.Delay;
        standToil.handlingFacing = true;
        standToil.tickAction = () => pawn.rotationTracker.FaceTarget(job.GetTarget(TargetIndex.A));
        standToil.AddFailCondition(DebateTargetGone);
        yield return standToil;

        var debateToil = ToilMaker.MakeToil("IconoclastDebate");
        debateToil.initAction = delegate
        {
            var actor = debateToil.actor;
            if (actor.MentalState is not MentalState_Iconoclast ms) return;
            foreach (var dt in ms.debateTargets.ToList())
                actor.interactions.TryInteractWith(dt, EnhancedIdeologyDefOf.EB_IdeologicalDebatePrecept);
        };
        debateToil.AddFailCondition(DebateTargetGone);
        yield return debateToil;

        yield return Toils_Jump.Jump(standToil);
    }

    private bool DebateTargetGone()
    {
        var target = (Pawn)job.GetTarget(TargetIndex.A).Thing;
        return pawn.MentalState is not MentalState_Iconoclast ms || !ms.debateTargets.Contains(target);
    }
}
