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
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allowBerserkChanceMetroidHunger, "allowBerserkChanceMetroidHunger", true);
            Scribe_Values.Look(ref allowBerserkChanceMetroidHunger, "allowBerserkChanceMetroidHunger", true);
            Scribe_Values.Look(ref allowBiggerMetroidsToBeTamed, "allowBiggerMetroidsToBeTamed");
            Scribe_Values.Look(ref metroidAgeGainRate, "metroidAgeGainRate", 1);
        }

        [TweakValue("0RT", 0, 500)] public static float evolutionBoxHeight = 510;
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Vector2 pos = new Vector2(rect.x, rect.y);
            var evolutionBox = new Rect(pos.x, pos.y, rect.width, evolutionBoxHeight);
            Listing_Standard ls_Evolution = new Listing_Standard();
            ls_Evolution.Begin(evolutionBox);
            ls_Evolution.CheckboxLabeled("RT.allowBerserkChanceMetroidHunger".Translate(), ref allowBerserkChanceMetroidHunger, "RT.allowBerserkChanceMetroidHungerTooltip".Translate());
            ls_Evolution.CheckboxLabeled("RT.allowWildChanceMetroidBerserk".Translate(), ref allowWildChanceMetroidBerserk, "RT.allowWildChanceMetroidBerserkTooltip".Translate());
            ls_Evolution.CheckboxLabeled("RT.allowBiggerMetroidsToBeTamed".Translate(), ref allowBiggerMetroidsToBeTamed, "RT.allowBiggerMetroidsToBeTamedTooltip".Translate());
            ls_Evolution.Label("RT.MetroidAgeGainRate".Translate() + ": " + metroidAgeGainRate * 100 + "%");
            metroidAgeGainRate = (int)ls_Evolution.Slider(metroidAgeGainRate, 1, 100);
            ls_Evolution.End();
            base.Write();
        }

        private Vector2 scrollPosition = Vector2.zero;
    }
}
