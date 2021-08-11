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
    public class RimtroidMod : Mod
    {
        public static RimtroidSettings settings;
        public RimtroidMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<RimtroidSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return "Rimtroid: Evolution";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            Utils.ApplySettings();
        }
    }
}
