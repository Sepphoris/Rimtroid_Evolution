using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DD
{
    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    public static class DD_Verb_TryCastNextBurstShot
    {
        public static void Postfix(ref Verb __instance)
        {
            if(__instance != null)
            {
                if(__instance.Caster != null)
                {
                    CompManagedVerbTracker comp = __instance.Caster.TryGetComp<CompManagedVerbTracker>();
                    if (comp != null)
                    {
                        comp.Notify_VerbUsed(__instance);
                    }
                }
            }
        }
    }
}
