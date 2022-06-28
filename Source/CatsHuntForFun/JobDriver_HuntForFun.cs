using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace CatsHuntForFun;

public class JobDriver_HuntForFun : JobDriver_AttackMelee
{
    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
        yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, TargetIndex.B, delegate
        {
            var thing = job.GetTarget(TargetIndex.A).Thing;
            if (!pawn.meleeVerbs.TryMeleeAttack(thing, job.verbToUse, true))
            {
                return;
            }

            if (pawn.CurJob == null || pawn.jobs.curDriver != this)
            {
                return;
            }

            EndJobWith(JobCondition.Succeeded);
        }).FailOnDespawnedOrNull(TargetIndex.A);
    }
}