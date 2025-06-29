using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace CatsHuntForFun;

[StaticConstructorOnStartup]
public class CatsHuntForFun
{
    private static List<PawnKindDef> ValidCatRaces;
    public static readonly List<ThingDef> AllAnimals;
    public static readonly JobDef HuntForFun = DefDatabase<JobDef>.GetNamedSilentFail("CatsHuntForFun_Hunt");
    public static readonly JobDef BringGift = DefDatabase<JobDef>.GetNamedSilentFail("CatsHuntForFun_BringGift");
    private static readonly Dictionary<PawnKindDef, float> AnimalSizes = new();
    public static readonly ThingDef Cat = DefDatabase<ThingDef>.GetNamedSilentFail("Cat");

    static CatsHuntForFun()
    {
        AllAnimals = DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => def.race is
            {
                Animal: true, AnyPawnKind: not null, DeathActionWorker: not
                {
                    DangerousInMelee: true
                }
            })
            .OrderBy(def => def.label).ToList();
        logMessage("Saving all animal-sizes");
        foreach (var animal in AllAnimals)
        {
            logMessage($"Checking size of {animal}");
            AnimalSizes[animal.race.AnyPawnKind] = animal.race.baseBodySize;
            logMessage($"Size: {animal.race.baseBodySize}");
        }

        UpdateAvailableCats();
    }

    public static void UpdateAvailableCats()
    {
        ValidCatRaces = [];
        if (CatsHuntForFunMod.Instance.Settings.ManualCats?.Any() == true)
        {
            logMessage("Found manually defined cat-races, iterating");
            foreach (var settingsManualCat in CatsHuntForFunMod.Instance.Settings.ManualCats)
            {
                var catToAdd = DefDatabase<PawnKindDef>.GetNamedSilentFail(settingsManualCat);
                if (catToAdd == null)
                {
                    logMessage($"{settingsManualCat} not found, skipping");
                    continue;
                }

                logMessage($"Adding {settingsManualCat}");
                ValidCatRaces.Add(catToAdd);
            }

            if (ValidCatRaces.Count == 0)
            {
                logMessage("Could not find any valid cat-races in game", false, true);
            }
            else
            {
                logMessage($"Found {ValidCatRaces.Count} valid cat-races in game: {string.Join(", ", ValidCatRaces)}",
                    true);
                logMessage(string.Join(", ", ValidCatRaces));
            }

            return;
        }

        ValidCatRaces.AddRange(from race in DefDatabase<PawnKindDef>.AllDefsListForReading
            where race.HasModExtension<CatExtension>() &&
                  race.GetModExtension<CatExtension>().IsCat
            select race);
        if (ValidCatRaces.Count == 0)
        {
            logMessage("Could not find any valid cat-races in game", false, true);
        }
        else
        {
            CatsHuntForFunMod.Instance.Settings.ManualCats ??= [];

            foreach (var validRatRace in ValidCatRaces)
            {
                logMessage($"Adding hardcoded {validRatRace.defName}");
                CatsHuntForFunMod.Instance.Settings.ManualCats?.Add(validRatRace.defName);
            }

            logMessage($"Found {ValidCatRaces.Count} valid cat-races in game: {string.Join(", ", ValidCatRaces)}",
                true);
        }
    }

    public static IntVec3 GetGiftLocation(Pawn cat)
    {
        if (Rand.Value >= CatsHuntForFunMod.Instance.Settings.ChanceForGifts)
        {
            return IntVec3.Invalid;
        }

        var firstDirectRelationPawn = cat.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, x => !x.Dead);

        if (firstDirectRelationPawn?.ownership.OwnedBed == null ||
            firstDirectRelationPawn.ownership.OwnedBed.Map != cat.Map)
        {
            return IntVec3.Invalid;
        }

        return !cat.CanReach(firstDirectRelationPawn.ownership.OwnedBed.Position, PathEndMode.ClosestTouch, Danger.Some)
            ? IntVec3.Invalid
            : firstDirectRelationPawn.ownership.OwnedBed.Position;
    }

    public static bool CanStartJobNow(Pawn pawn)
    {
        if (!isAnAdultCat(pawn))
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
            logMessage($"{cat} will ignore {prey}: same faction");
            return null;
        }

        if (notFactionPets && prey.Faction != null && prey.Faction != cat.Faction)
        {
            logMessage($"{cat} will ignore {prey}: belongs to another faction");
            return null;
        }

        if (onlyInHome && !thing.Map.areaManager.Home[thing.Position])
        {
            logMessage($"{cat} will ignore {prey}: not in home area");
            return null;
        }

        if (prey.IsHiddenFromPlayer())
        {
            logMessage($"{cat} will ignore {prey}: is invisible");
            return null;
        }

        if (!isGift && prey.health?.Downed == true)
        {
            logMessage($"{cat} will ignore {prey}: is downed");
            return null;
        }

        if (!validPrey(cat).Contains(prey.RaceProps?.AnyPawnKind))
        {
            logMessage($"{cat} will ignore {prey}: not a valid prey-race");
            return null;
        }

        if (cat.CanSee(thing))
        {
            return thing;
        }

        logMessage($"{cat} will ignore {prey}: cant see it");
        return null;
    }

    private static bool isAnAdultCat(Pawn pawn)
    {
        if (pawn.ageTracker?.Adult == false)
        {
            return false;
        }

        return CatsHuntForFunMod.Instance.Settings.ManualCats?.Contains(pawn.RaceProps.AnyPawnKind?.defName) == true;
    }

    public static List<PawnKindDef> ValidPrey(ThingDef raceDef)
    {
        return AnimalSizes.Where(pair =>
                pair.Value < raceDef.race.baseBodySize * CatsHuntForFunMod.Instance.Settings.RelativeBodySize)
            .Select(pair => pair.Key).ToList();
    }

    private static List<PawnKindDef> validPrey(Pawn pawn)
    {
        return AnimalSizes.Where(pair =>
                pair.Value < pawn.RaceProps.baseBodySize * CatsHuntForFunMod.Instance.Settings.RelativeBodySize)
            .Select(pair => pair.Key).ToList();
    }

    private static void logMessage(string message, bool forced = false, bool warning = false)
    {
        if (warning)
        {
            Log.Warning($"[CatsHuntForFun]: {message}");
            return;
        }

        if (!forced && !CatsHuntForFunMod.Instance.Settings.VerboseLogging)
        {
            return;
        }

        Log.Message($"[CatsHuntForFun!]: {message}");
    }
}