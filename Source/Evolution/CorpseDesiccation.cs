using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using System.Reflection;

namespace RT_Rimtroid
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Desiccator_Pawn_Kill_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
        {
            if (dinfo.HasValue)
            {
                Log.Message("dinfo.HasValue (fifth check)");
                if (dinfo.Value.Instigator != null)
                {
                    Log.Message("dinfo.Value.Instigator (sixth check)");
                    Thing inst = dinfo.Value.Instigator;
                    RT_DesiccatorExt desiccator = inst.def.GetModExtension<RT_DesiccatorExt>();
                    if (desiccator != null)
                    {
                        Log.Message("desiccator = null (seventh check)");
                        if (desiccator.RT_DesiccatedDef != null)
                        {
                            FieldInfo corpse = typeof(Pawn).GetField("Corpse", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
                            Traverse.Create(__instance);
                            corpse.SetValue(__instance, ThingMaker.MakeThing(desiccator.RT_DesiccatedDef));
                            Log.Message("corpse.setvalue (eighth check)");
                        }
                        else
                        {
                            CompRottable compRottable = __instance.Corpse.TryGetComp<CompRottable>();
                            compRottable.RotImmediately();
                            Log.Message("Rotted (Third statement)");
                        }
                    }
                }
            }
        }
    }
}
