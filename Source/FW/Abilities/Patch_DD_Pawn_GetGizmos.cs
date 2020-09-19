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
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class DD_Pawn_GetGizmos
    {
        public static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!__instance.IsColonistPlayerControlled && __instance.Faction != null && __instance.Faction.IsPlayer && __instance.abilities != null && __instance.kindDef.HasModExtension<VerbSettingExtension>() && __instance.kindDef.GetModExtension<VerbSettingExtension>().showAbilityGizmos)
            {
                __result = __result.Concat(__instance.abilities.GetGizmos());
            }
        }
    }
}
