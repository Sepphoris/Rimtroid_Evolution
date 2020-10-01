using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;

namespace RT_Rimtroid
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.symphoniess.rimworld.mod.RimtroidEvolution");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    public class RT_DesiccatorExt : DefModExtension
    {

        public ThingDef RT_DesiccatedDef;
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class RT_Desiccator_Pawn_Kill_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
        {
            if (dinfo.HasValue)
            {
                if (dinfo.Value.Instigator != null)
                {
                    Thing inst = dinfo.Value.Instigator;
                    RT_DesiccatorExt desiccator = inst.def.GetModExtension<RT_DesiccatorExt>();
                    if (desiccator != null)
                    {
                        if (desiccator.RT_DesiccatedDef != null)
                        {
                            FieldInfo corpse = typeof(Pawn).GetField("Corpse", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
                            Traverse.Create(__instance);
                            corpse.SetValue(__instance, ThingMaker.MakeThing(desiccator.RT_DesiccatedDef));
                        }
                        else
                        {
                            CompRottable compRottable = __instance.Corpse.TryGetComp<CompRottable>();
                            compRottable.RotImmediately();
                        }
                    }
                }
            }
            HediffDef def = DefDatabase<HediffDef>.GetNamed("RT_LifeDrainSickness");
            if (__instance.health.hediffSet.HasHediff(def))
            {
                CompRottable compRottable = __instance.Corpse.TryGetComp<CompRottable>();
                compRottable.RotImmediately();
            }
            if (__instance.Corpse.GetRotStage() == RotStage.Fresh)
            {
                Log.Message(__instance + " failed rot");
            }
            /*
            else
            {
                Log.Message(__instance + " rotted by");
            }
            */
        }
    }
}