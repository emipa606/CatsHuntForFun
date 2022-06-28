using RimWorld;
using Verse;
using Verse.AI;

namespace CatsHuntForFun;

public class JobGiver_HuntForFun : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!CatsHuntForFun.IsACat(pawn))
        {
            return null;
        }

        if (pawn.CurJobDef == JobDefOf.AttackMelee)
        {
            return null;
        }

        var onlyInHome = CatsHuntForFunMod.instance.Settings.OnlyHomeArea;
        var notFactionPets = CatsHuntForFunMod.instance.Settings.NotColonyPets;

        if (pawn.Faction != Faction.OfPlayer)
        {
            if (CatsHuntForFunMod.instance.Settings.AlsoWild)
            {
                onlyInHome = notFactionPets = false;
            }
            else
            {
                return null;
            }
        }

        if (pawn.Downed || pawn.health.HasHediffsNeedingTend() || pawn.health.hediffSet.BleedRateTotal > 0.001f)
        {
            return null;
        }

        if (PawnUtility.PlayerForcedJobNowOrSoon(pawn))
        {
            return null;
        }

        foreach (var cell in GenRadial.RadialCellsAround(pawn.Position, CatsHuntForFunMod.instance.Settings.HuntRange,
                     true))
        {
            if (!cell.InBounds(pawn.Map))
            {
                continue;
            }

            var prey = GetPreyFromCell(cell, pawn, onlyInHome, notFactionPets);
            if (prey == null)
            {
                continue;
            }

            if (Rand.Value > CatsHuntForFunMod.instance.Settings.ChanceToHunt)
            {
                return null;
            }

            var job = JobMaker.MakeJob(CatsHuntForFun.HuntForFun, prey);
            job.expiryInterval = 200;
            return job;
        }

        return null;
    }

    private static Pawn GetPreyFromCell(IntVec3 possiblePreyCell, Pawn cat, bool onlyInHome, bool notFactionPets)
    {
        var prey = possiblePreyCell.GetFirstPawn(cat.Map);

        if (prey is null)
        {
            return null;
        }

        if (prey == cat)
        {
            return null;
        }

        if (notFactionPets && prey.Faction == cat.Faction)
        {
            return null;
        }

        if (onlyInHome && !prey.Map.areaManager.Home[prey.Position])
        {
            return null;
        }

        if (prey.IsInvisible())
        {
            return null;
        }

        if (prey.health.Downed)
        {
            return null;
        }

        if (!CatsHuntForFun.ValidPrey(cat).Contains(prey.RaceProps?.AnyPawnKind))
        {
            return null;
        }

        if (!cat.CanSee(prey))
        {
            return null;
        }

        return prey;
    }
}