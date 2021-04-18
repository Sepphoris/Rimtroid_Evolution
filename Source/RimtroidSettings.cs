using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RT_Rimtroid
{
    public class RimtroidSettings : ModSettings
    {
        // Evolution
        public static bool allowBerserkChanceMetroidHunger = true;
        public static bool allowWildChanceMetroidBerserk = true;
        public static bool allowBiggerMetroidsToBeTamed;
        public static int metroidAgeGainRate = 3;
        public static Dictionary<string, bool> metroidsToDisable = new Dictionary<string, bool>
        {
            {"RT_BanteeMetroid", false},
            {"RT_MetroidLarvae", false},
            {"RT_AlphaMetroid", false},
            {"RT_GammaMetroid", false},
            {"RT_ZetaMetroid", false},
            {"RT_OmegaMetroid", false},
            {"RT_QueenMetroid", false}
        };
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allowBerserkChanceMetroidHunger, "allowBerserkChanceMetroidHunger", true);
            Scribe_Values.Look(ref allowBerserkChanceMetroidHunger, "allowBerserkChanceMetroidHunger", true);
            Scribe_Values.Look(ref allowBiggerMetroidsToBeTamed, "allowBiggerMetroidsToBeTamed");
            Scribe_Values.Look(ref metroidAgeGainRate, "metroidAgeGainRate", 1);
            Scribe_Collections.Look(ref metroidsToDisable, "metroidsToDisable", LookMode.Value, LookMode.Value, ref stringKeys, ref boolValues);
            if (metroidsToDisable is null)
            {
                metroidsToDisable = new Dictionary<string, bool>
                {
                    {"RT_BanteeMetroid", false},
                    {"RT_MetroidLarvae", false},
                    {"RT_AlphaMetroid", false},
                    {"RT_GammaMetroid", false},
                    {"RT_ZetaMetroid", false},
                    {"RT_OmegaMetroid", false},
                    {"RT_QueenMetroid", false}
                };
            }
        }
        private List<string> stringKeys;
        private List<bool> boolValues;

        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Listing_Standard ls_Evolution = new Listing_Standard();
            ls_Evolution.Begin(rect);
            ls_Evolution.CheckboxLabeled("RT.allowBerserkChanceMetroidHunger".Translate(), ref allowBerserkChanceMetroidHunger, "RT.allowBerserkChanceMetroidHungerTooltip".Translate());
            ls_Evolution.CheckboxLabeled("RT.allowWildChanceMetroidBerserk".Translate(), ref allowWildChanceMetroidBerserk, "RT.allowWildChanceMetroidBerserkTooltip".Translate());
            ls_Evolution.CheckboxLabeled("RT.allowBiggerMetroidsToBeTamed".Translate(), ref allowBiggerMetroidsToBeTamed, "RT.allowBiggerMetroidsToBeTamedTooltip".Translate());
            ls_Evolution.Label("RT.MetroidAgeGainRate".Translate() + ": " + metroidAgeGainRate * 100 + "%");
            metroidAgeGainRate = (int)ls_Evolution.Slider(metroidAgeGainRate, 1, 100);
            ls_Evolution.Label("RT.DisableEnableSpawns".Translate());
            var keys = metroidsToDisable.Keys.ToList();
            for (int num = keys.Count - 1; num >= 0; num--)
            {
                var checkbox = metroidsToDisable[keys[num]];
                var metroid = DefDatabase<PawnKindDef>.GetNamed(keys[num]);
                ls_Evolution.CheckboxLabeled(metroid.label, ref checkbox);
                metroidsToDisable[keys[num]] = checkbox;
            }
            ls_Evolution.End();
            base.Write();
        }

        private Vector2 scrollPosition = Vector2.zero;
    }
}
