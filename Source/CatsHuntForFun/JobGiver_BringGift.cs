using Verse;
using Verse.AI;

namespace CatsHuntForFun;

public class JobGiver_BringGift : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!CatsHuntForFun.CanStartJobNow(pawn))
        {
            return null;
        }

        if (Rand.Value > CatsHuntForFunMod.Instance.Settings.ChanceForGifts)
        {
            return null;
        }

        var giftLocation = CatsHuntForFun.GetGiftLocation(pawn);

        if (giftLocation == IntVec3.Invalid)
        {
            return null;
        }

        foreach (var cell in GenRadial.RadialCellsAround(pawn.Position, CatsHuntForFunMod.Instance.Settings.HuntRange,
                     true))
        {
            if (!cell.InBounds(pawn.Map))
            {
                continue;
            }

            if (giftLocation == cell)
            {
                continue;
            }

            var prey = CatsHuntForFun.GetPreyFromCell(cell, pawn, false,
                CatsHuntForFunMod.Instance.Settings.NotColonyPets, true);
            if (prey == null)
            {
                continue;
            }

            var job = JobMaker.MakeJob(CatsHuntForFun.BringGift, prey, giftLocation);

            job.count = 1;
            job.haulMode = HaulMode.ToCellNonStorage;
            job.expiryInterval = 3000;
            return job;
        }

        return null;
    }
}