using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace DD
{
    [HarmonyPatch(typeof(Pawn), "TryGetAttackVerb")]
    public static class DD_Pawn_TryGetAttackVerb
    {
        public static void Postfix(Pawn __instance, ref Verb __result, Thing target, bool allowManualCastWeapons)
        {
            if (__instance.kindDef.HasModExtension<VerbSettingExtension>() && __instance.kindDef.GetModExtension<VerbSettingExtension>().verbControl)
            {
                IEnumerable<Verb> verbs = VerbUtils.GetPossibleVerbs(__instance).Filter_KeepOffensive();

                if (target != null)
                {
                    //If given a target, keep only the verbs in range.
                    verbs = verbs.Filter_KeepInRange(target);
                }

                if (verbs.Any())
                {
                    //Still has verbs.
                    __result = verbs.Get_MostPreferred();
                }
            }
        }
    }
}
