using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CatsHuntForFun;

public class JobDriver_BringGift : JobDriver_AttackMelee
{
    private Thing Prey => job.targetA.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Prey, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.B);

        var reportSuccess = new Toil
        {
            initAction = delegate
            {
                Messages.Message(
                    "CatsHuntForFun.deliveredgift".Translate(pawn.NameFullColored),
                    new LookTargets(new List<Thing> { Prey }),
                    MessageTypeDefOf.NeutralEvent,
                    false);
            }
        };

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.A);

        yield return Toils_Haul.StartCarryThing(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);

        yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, null, false);

        yield return reportSuccess;
    }
}