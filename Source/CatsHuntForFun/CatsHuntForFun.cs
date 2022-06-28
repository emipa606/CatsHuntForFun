using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CatsHuntForFun;

[StaticConstructorOnStartup]
public class CatsHuntForFun
{
    public static List<PawnKindDef> ValidCatRaces;
    public static readonly List<ThingDef> AllAnimals;
    public static readonly JobDef HuntForFun;
    public static readonly Dictionary<PawnKindDef, float> AnimalSizes;
    public static readonly ThingDef Cat;

    static CatsHuntForFun()
    {
        AllAnimals = DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => def.race is { Animal: true })
            .OrderBy(def => def.label).ToList();
        HuntForFun = DefDatabase<JobDef>.GetNamedSilentFail("HuntForFun");
        Cat = DefDatabase<ThingDef>.GetNamedSilentFail("Cat");
        AnimalSizes = new Dictionary<PawnKindDef, float>();
        foreach (var animal in AllAnimals)
        {
            AnimalSizes[animal.race.AnyPawnKind] = animal.race.baseBodySize;
        }

        UpdateAvailableCats();
    }

    public static void UpdateAvailableCats()
    {
        ValidCatRaces = new List<PawnKindDef>();
        if (CatsHuntForFunMod.instance.Settings.ManualCats?.Any() == true)
        {
            foreach (var settingsManualCat in CatsHuntForFunMod.instance.Settings.ManualCats)
            {
                ValidCatRaces.Add(PawnKindDef.Named(settingsManualCat));
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
                CatsHuntForFunMod.instance.Settings.ManualCats?.Add(validRatRace.defName);
            }

            LogMessage($"Found {ValidCatRaces.Count} valid cat-races in game: {string.Join(", ", ValidCatRaces)}",
                true);
        }
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