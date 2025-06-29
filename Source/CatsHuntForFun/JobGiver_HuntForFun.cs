using RimWorld;
using Verse;
using Verse.AI;

namespace CatsHuntForFun;

public class JobGiver_HuntForFun : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!CatsHuntForFun.CanStartJobNow(pawn))
        {
            return null;
        }

        if (Rand.Value > CatsHuntForFunMod.Instance.Settings.ChanceToHunt)
        {
            return null;
        }

        var onlyInHome = CatsHuntForFunMod.Instance.Settings.OnlyHomeArea;
        var notColonyPets = CatsHuntForFunMod.Instance.Settings.NotColonyPets;
        var notFactionPets = CatsHuntForFunMod.Instance.Settings.NotFactionPets;

        if (pawn.Faction != Faction.OfPlayer)
        {
            if (CatsHuntForFunMod.Instance.Settings.AlsoWild)
            {
                onlyInHome = notColonyPets = false;
            }
            else
            {
                return null;
            }
        }

        foreach (var cell in GenRadial.RadialCellsAround(pawn.Position, CatsHuntForFunMod.Instance.Settings.HuntRange,
                     true))
        {
            if (!cell.InBounds(pawn.Map))
            {
                continue;
            }

            var prey = CatsHuntForFun.GetPreyFromCell(cell, pawn, onlyInHome, notColonyPets, notFactionPets);
            if (prey == null)
            {
                continue;
            }

            var job = JobMaker.MakeJob(CatsHuntForFun.HuntForFun, prey);

            job.expiryInterval = 200;
            return job;
        }

        return null;
    }
}