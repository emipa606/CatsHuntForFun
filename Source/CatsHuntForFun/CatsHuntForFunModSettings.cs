using System.Collections.Generic;
using Verse;

namespace CatsHuntForFun;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class CatsHuntForFunModSettings : ModSettings
{
    public bool AlsoWild;
    public float ChanceToHunt;
    public float HuntRange;
    public List<string> ManualCats = new List<string>();
    public bool NotColonyPets = true;
    public bool OnlyHomeArea;
    public float RelativeBodySize;
    public bool VerboseLogging;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref VerboseLogging, "VerboseLogging");
        Scribe_Values.Look(ref AlsoWild, "AlsoWild");
        Scribe_Values.Look(ref OnlyHomeArea, "OnlyHomeArea");
        Scribe_Values.Look(ref NotColonyPets, "NotColonyPets", true);
        Scribe_Values.Look(ref RelativeBodySize, "RelativeBodySize", 0.75f);
        Scribe_Values.Look(ref ChanceToHunt, "ChanceToHunt", 0.2f);
        Scribe_Values.Look(ref HuntRange, "HuntRange", 7f);
        Scribe_Collections.Look(ref ManualCats, "ManualCats");
    }
}