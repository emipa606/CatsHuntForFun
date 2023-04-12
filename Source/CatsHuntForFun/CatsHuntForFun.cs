using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace CatsHuntForFun;

[StaticConstructorOnStartup]
public class CatsHuntForFun
{
    public static List<PawnKindDef> ValidCatRaces;
    public static readonly List<ThingDef> AllAnimals;
    public static readonly JobDef HuntForFun;
    public static readonly JobDef BringGift;
    public static readonly Dictionary<PawnKindDef, float> AnimalSizes;
    public static readonly ThingDef Cat;

    static CatsHuntForFun()
    {
        AllAnimals = DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => def.race is { Animal: true } && (def.race.DeathActionWorker == null ||
                                                           def.race.DeathActionWorker.DangerousInMelee == false))
            .OrderBy(def => def.label).ToList();
        HuntForFun = DefDatabase<JobDef>.GetNamedSilentFail("CatsHuntForFun_Hunt");
        BringGift = DefDatabase<JobDef>.GetNamedSilentFail("CatsHuntForFun_BringGift");
        Cat = DefDatabase<ThingDef>.GetNamedSilentFail("Cat");
        AnimalSizes = new Dictionary<PawnKindDef, float>();
        LogMessage("Saving all animal-sizes");
        foreach (var animal in AllAnimals)
        {
            LogMessage($"Checking size of {animal}");
            AnimalSizes[animal.race.AnyPawnKind] = animal.race.baseBodySize;
            LogMessage($"Size: {animal.race.baseBodySize}");
        }

        UpdateAvailableCats();
    }

    public static void UpdateAvailableCats()
    {
        ValidCatRaces = new List<PawnKindDef>();
        if (CatsHuntForFunMod.instance.Settings.ManualCats?.Any() == true)
        {
            LogMessage("Found manually defined cat-races, iterating");
            foreach (var settingsManualCat in CatsHuntForFunMod.instance.Settings.ManualCats)
            {
                var catToAdd = DefDatabase<PawnKindDef>.GetNamedSilentFail(settingsManualCat);
                if (catToAdd == null)
                {
                    LogMessage($"{settingsManualCat} not found, skipping");
                    continue;
                }

                LogMessage($"Adding {settingsManualCat}");
                ValidCatRaces.Add(catToAdd);
            }

            if (ValidCatRaces.Count == 0)
            {
                LogMessage("Could not find any valid cat-races in game", false, true);
            }
            else
            {
                LogMessage($"Found {ValidCatRaces.Count} valid cat-races in game: {string.Join(", ", ValidCatRaces)}",
                    true);
                LogMessage(string.Join(", ", ValidCatRaces));
            }

            return;
        }

        ValidCatRaces.AddRange(from race in DefDatabase<PawnKindDef>.AllDefsListForReading
            where race.HasModExtension<CatExtension>() &&
                  race.GetModExtension<CatExtension>().IsCat
            select race);
        if (ValidCatRaces.Count == 0)
        {
            LogMessage("Could not find any valid cat-races in game", false, true);
        }
        else
        {
            if (CatsHuntForFunMod.instance.Settings.ManualCats == null)
            {
                CatsHuntForFunMod.instance.Settings.ManualCats = new List<string>();
            }

            foreach (var validRatRace in ValidCatRaces)
            {
                LogMessage($"Adding hardcoded {validRatRace.defName}");
                CatsHuntForFunMod.instance.Settings.ManualCats?.Add(validRatRace.defName);
            }

            LogMessage($"Found {ValidCatRaces.Count} valid cat-races in game: {string.Join(", ", ValidCatRaces)}",
                true);
        }
    }

    public static IntVec3 GetGiftLocation(Pawn cat)
    {
        if (Rand.Value >= CatsHuntForFunMod.instance.Settings.ChanceForGifts)
        {
            return IntVec3.Invalid;
        }

        var firstDirectRelationPawn = cat.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, x => !x.Dead);

        if (firstDirectRelationPawn?.ownership.OwnedBed == null)
        {
            return IntVec3.Invalid;
        }

        if (firstDirectRelationPawn.ownership.OwnedBed.Map != cat.Map)
        {
            return IntVec3.Invalid;
        }

        return !cat.CanReach(firstDirectRelationPawn.ownership.OwnedBed.Position, PathEndMode.ClosestTouch, Danger.Some)
            ? IntVec3.Invalid
            : firstDirectRelationPawn.ownership.OwnedBed.Position;
    }

    public static bool CanStartJobNow(Pawn pawn)
    {
        if (!IsACat(pawn))
        {
            return false;
        }

        if (pawn.jobs.startingNewJob)
        {
            return false;
        }

        if (pawn.Downed || pawn.health.HasHediffsNeedingTend() || pawn.health.hediffSet.BleedRateTotal > 0.001f)
        {
            return false;
        }

        return !PawnUtility.PlayerForcedJobNowOrSoon(pawn);
    }

    public static Thing GetPreyFromCell(IntVec3 possiblePreyCell, Pawn cat, bool onlyInHome, bool notColonyPets,
        bool notFactionPets,
        bool isGift = false)
    {
        Thing thing;
        var prey = possiblePreyCell.GetFirstPawn(cat.Map);

        if (prey is null)
        {
            if (isGift)
            {
                var corpse = possiblePreyCell.GetFirstThing<Corpse>(cat.Map);
                if (corpse == null)
                {
                    return null;
                }

                thing = corpse;
                prey = corpse.InnerPawn;
                if (prey == null)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        else
        {
            thing = prey;
        }

        if (prey == cat)
        {
            return null;
        }

        if (notColonyPets && prey.Faction == cat.Faction)
        {
            LogMessage($"{cat} will ignore {prey}: same faction");
            return null;
        }

        if (notFactionPets && prey.Faction != null && prey.Faction != cat.Faction)
        {
            LogMessage($"{cat} will ignore {prey}: belongs to another faction");
            return null;
        }

        if (onlyInHome && !thing.Map.areaManager.Home[thing.Position])
        {
            LogMessage($"{cat} will ignore {prey}: not in home area");
            return null;
        }

        if (prey.IsInvisible())
        {
            LogMessage($"{cat} will ignore {prey}: is invisible");
            return null;
        }

        if (!isGift && prey.health?.Downed == true)
        {
            LogMessage($"{cat} will ignore {prey}: is downed");
            return null;
        }

        if (!ValidPrey(cat).Contains(prey.RaceProps?.AnyPawnKind))
        {
            LogMessage($"{cat} will ignore {prey}: not a valid prey-race");
            return null;
        }

        if (cat.CanSee(thing))
        {
            return thing;
        }

        LogMessage($"{cat} will ignore {prey}: cant see it");
        return null;
    }

    public static bool IsACat(Pawn pawn)
    {
        return CatsHuntForFunMod.instance.Settings.ManualCats?.Contains(pawn.RaceProps.AnyPawnKind?.defName) == true;
    }

    public static List<PawnKindDef> ValidPrey(ThingDef raceDef)
    {
        return AnimalSizes.Where(pair =>
                pair.Value < raceDef.race.baseBodySize * CatsHuntForFunMod.instance.Settings.RelativeBodySize)
            .Select(pair => pair.Key).ToList();
    }

    public static List<PawnKindDef> ValidPrey(Pawn pawn)
    {
        return AnimalSizes.Where(pair =>
                pair.Value < pawn.RaceProps.baseBodySize * CatsHuntForFunMod.instance.Settings.RelativeBodySize)
            .Select(pair => pair.Key).ToList();
    }

    public static void LogMessage(string message, bool forced = false, bool warning = false)
    {
        if (warning)
        {
            Log.Warning($"[CatsHuntForFun]: {message}");
            return;
        }

        if (!forced && !CatsHuntForFunMod.instance.Settings.VerboseLogging)
        {
            return;
        }

        Log.Message($"[CatsHuntForFun!]: {message}");
    }
}