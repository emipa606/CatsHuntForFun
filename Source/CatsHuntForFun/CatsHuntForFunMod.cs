using System.Collections.Generic;
using System.Linq;
using Mlie;
using UnityEngine;
using Verse;

namespace CatsHuntForFun;

[StaticConstructorOnStartup]
internal class CatsHuntForFunMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static CatsHuntForFunMod instance;

    private static string currentVersion;
    private static readonly Vector2 searchSize = new Vector2(200f, 25f);
    private static readonly Vector2 buttonSize = new Vector2(120f, 25f);
    private static readonly Vector2 iconSize = new Vector2(58f, 58f);
    private static string searchText;
    private static Vector2 scrollPosition;
    private static readonly Color alternateBackground = new Color(0.2f, 0.2f, 0.2f, 0.5f);


    /// <summary>
    ///     The private settings
    /// </summary>
    private CatsHuntForFunModSettings settings;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public CatsHuntForFunMod(ModContentPack content)
        : base(content)
    {
        instance = this;
        if (instance.Settings.ManualCats == null)
        {
            instance.Settings.ManualCats = new List<string>();
        }

        searchText = string.Empty;
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(
                ModLister.GetActiveModWithIdentifier("Mlie.CatsHuntForFun"));
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal CatsHuntForFunModSettings Settings
    {
        get
        {
            if (settings == null)
            {
                settings = GetSettings<CatsHuntForFunModSettings>();
            }

            return settings;
        }

        set => settings = value;
    }

    public override string SettingsCategory()
    {
        return "Cats Hunt For Fun";
    }

    /// <summary>
    ///     The settings-window
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        base.DoSettingsWindowContents(rect);

        var listing_Standard = new Listing_Standard
        {
            ColumnWidth = rect.width * 0.49f
        };
        listing_Standard.Begin(rect);
        Text.Font = GameFont.Medium;
        listing_Standard.Label("CatsHuntForFun.options".Translate());
        Text.Font = GameFont.Small;
        Settings.HuntRange = Widgets.HorizontalSlider(listing_Standard.GetRect(50f), Settings.HuntRange,
            1f, 20f, false,
            "CatsHuntForFun.huntrange".Translate(Settings.HuntRange), null, null, 1f);
        Settings.ChanceToHunt = Widgets.HorizontalSlider(listing_Standard.GetRect(50f), Settings.ChanceToHunt,
            0.01f, 1f, false,
            "CatsHuntForFun.chancetohunt".Translate(Settings.ChanceToHunt.ToStringPercent()));
        Settings.RelativeBodySize = Widgets.HorizontalSlider(listing_Standard.GetRect(50f), Settings.RelativeBodySize,
            0.01f, 1f, false,
            "CatsHuntForFun.relativebodysize".Translate(Settings.RelativeBodySize.ToStringPercent()));
        listing_Standard.Label("CatsHuntForFun.relativebodysize.example".Translate(string.Join(", ",
            CatsHuntForFun.ValidPrey(CatsHuntForFun.Cat).Select(def => def.label))));

        listing_Standard.Gap();
        listing_Standard.CheckboxLabeled("CatsHuntForFun.notcolonypets.label".Translate(), ref Settings.NotColonyPets,
            "CatsHuntForFun.notcolonypets.tooltip".Translate());
        listing_Standard.CheckboxLabeled("CatsHuntForFun.onlyhomearea.label".Translate(), ref Settings.OnlyHomeArea,
            "CatsHuntForFun.onlyhomearea.tooltip".Translate());
        listing_Standard.Gap();
        listing_Standard.CheckboxLabeled("CatsHuntForFun.alsowild.label".Translate(), ref Settings.AlsoWild,
            "CatsHuntForFun.alsowild.tooltip".Translate());
        if (Settings.AlsoWild)
        {
            listing_Standard.Label("CatsHuntForFun.alsowild.explanation".Translate());
        }

        listing_Standard.Gap();
        listing_Standard.CheckboxLabeled("CatsHuntForFun.logging.label".Translate(), ref Settings.VerboseLogging,
            "CatsHuntForFun.logging.tooltip".Translate());

        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("CatsHuntForFun.version.label".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.NewColumn();
        Text.Font = GameFont.Medium;
        var titleRect = listing_Standard.Label("CatsHuntForFun.whatisacat.label".Translate());
        Text.Font = GameFont.Small;
        if (Widgets.ButtonText(
                new Rect(titleRect.position + new Vector2(titleRect.width - buttonSize.x, 0), buttonSize),
                "CatsHuntForFun.reset".Translate()))
        {
            instance.Settings.ManualCats = new List<string>();
            CatsHuntForFun.UpdateAvailableCats();
        }

        var searchRect = listing_Standard.GetRect(searchSize.x);
        searchText =
            Widgets.TextField(
                new Rect(
                    searchRect.position +
                    new Vector2(searchRect.width - searchSize.x, 5),
                    searchSize),
                searchText);
        Widgets.Label(searchRect, "CatsHuntForFun.search".Translate());


        var allAnimals = CatsHuntForFun.AllAnimals;
        if (!string.IsNullOrEmpty(searchText))
        {
            allAnimals = CatsHuntForFun.AllAnimals.Where(def =>
                    def.label.ToLower().Contains(searchText.ToLower()) || def.modContentPack?.Name.ToLower()
                        .Contains(searchText.ToLower()) == true)
                .ToList();
        }

        listing_Standard.End();

        var borderRect = rect;
        borderRect.width *= 0.49f;
        borderRect.x += rect.width / 2;
        borderRect.height -= searchSize.y + 40f;
        borderRect.y += searchSize.y + 40f;
        var scrollContentRect = borderRect;
        scrollContentRect.height = allAnimals.Count * 61f;
        scrollContentRect.width -= 20;
        scrollContentRect.x = 0;
        scrollContentRect.y = 0;

        var scrollListing = new Listing_Standard();
        Widgets.BeginScrollView(borderRect, ref scrollPosition, scrollContentRect);
        scrollListing.Begin(scrollContentRect);
        var alternate = false;
        foreach (var animal in allAnimals)
        {
            var modInfo = animal.modContentPack?.Name;
            var rowRect = scrollListing.GetRect(60);
            alternate = !alternate;
            if (alternate)
            {
                Widgets.DrawBoxSolid(rowRect.ExpandedBy(10, 0), alternateBackground);
            }

            var raceLabel = $"{animal.label.CapitalizeFirst()} ({animal.defName}) - {modInfo}";
            var isCat = instance.Settings.ManualCats.Contains(animal.defName);
            var wasCat = isCat;
            DrawIcon(animal,
                new Rect(rowRect.position, iconSize));
            var nameRect = new Rect(rowRect.position + new Vector2(iconSize.x, 0),
                rowRect.size - new Vector2(iconSize.x, 0));
            Widgets.CheckboxLabeled(nameRect, raceLabel, ref isCat);
            if (isCat == wasCat)
            {
                continue;
            }

            if (isCat)
            {
                instance.Settings.ManualCats.Add(animal.defName);
            }
            else
            {
                instance.Settings.ManualCats.Remove(animal.defName);
            }
        }

        scrollListing.End();
        Widgets.EndScrollView();
    }

    private void DrawIcon(ThingDef animal, Rect rect)
    {
        var pawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(animal.defName);

        var texture2D = pawnKind?.lifeStages?.Last()?.bodyGraphicData?.Graphic?.MatSingle?.mainTexture;

        if (texture2D == null)
        {
            return;
        }

        var toolTip = $"{pawnKind.LabelCap}\n{pawnKind.race?.description}";
        if (texture2D.width != texture2D.height)
        {
            var ratio = (float)texture2D.width / texture2D.height;

            if (ratio < 1)
            {
                rect.x += (rect.width - (rect.width * ratio)) / 2;
                rect.width *= ratio;
            }
            else
            {
                rect.y += (rect.height - (rect.height / ratio)) / 2;
                rect.height /= ratio;
            }
        }

        GUI.DrawTexture(rect, texture2D);
        TooltipHandler.TipRegion(rect, toolTip);
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        CatsHuntForFun.UpdateAvailableCats();
    }
}