using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CatsHuntForFun;

public class JobDriver_HuntForFun : JobDriver_AttackMelee
{
    protected Pawn Prey => (Pawn)job.targetA.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Prey, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

        yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
        yield return Toils_Combat
            .FollowAndMeleeAttack(TargetIndex.A, TargetIndex.None,
                delegate { pawn.meleeVerbs.TryMeleeAttack(Prey, job.verbToUse, true); })
            .FailOnDespawnedOrNull(TargetIndex.A);
    }
}