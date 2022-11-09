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

        if (Rand.Value > CatsHuntForFunMod.instance.Settings.ChanceToHunt)
        {
            return null;
        }

        var onlyInHome = CatsHuntForFunMod.instance.Settings.OnlyHomeArea;
        var notColonyPets = CatsHuntForFunMod.instance.Settings.NotColonyPets;
        var notFactionPets = CatsHuntForFunMod.instance.Settings.NotFactionPets;

        if (pawn.Faction != Faction.OfPlayer)
        {
            if (CatsHuntForFunMod.instance.Settings.AlsoWild)
            {
                onlyInHome = notColonyPets = false;
            }
            else
            {
                return null;
            }
        }

        foreach (var cell in GenRadial.RadialCellsAround(pawn.Position, CatsHuntForFunMod.instance.Settings.HuntRange,
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