using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace RT_Rimtroid
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.drazzii.rimworld.mod.RimtroidEvolution");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            DefDatabase<ThingDef>.GetNamed("RT_BanteeMetroid").race.predator = true;
            DefDatabase<ThingDef>.GetNamed("RT_MetroidLarvae").race.predator = true;
            DefDatabase<ThingDef>.GetNamed("RT_AlphaMetroid").race.predator = true;
            DefDatabase<ThingDef>.GetNamed("RT_GammaMetroid").race.predator = true;
            DefDatabase<ThingDef>.GetNamed("RT_ZetaMetroid").race.predator = true;
            DefDatabase<ThingDef>.GetNamed("RT_OmegaMetroid").race.predator = true;
            DefDatabase<ThingDef>.GetNamed("RT_QueenMetroid").race.predator = true;
        }
    }

    public class RT_DesiccatorExt : DefModExtension
    {

        public ThingDef RT_DesiccatedDef;
    }


    [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
    public static class RT_TryStartMentalState_Patch
    {
        public static void Postfix(Pawn ___pawn, bool __result, MentalStateDef stateDef, string reason = null, bool forceWake = false, bool causedByMood = false, Pawn otherPawn = null, bool transitionSilently = false)
        {
            if (__result && ___pawn is Queen queen && stateDef.IsAggro)
            {
                for (int num = queen.Map.mapPawns.AllPawns.Count - 1; num >= 0; num--)
                {
                    if (queen.Map.mapPawns.AllPawns[num].Faction != Faction.OfPlayer && !queen.Map.mapPawns.AllPawns[num].InAggroMentalState && queen.Map.mapPawns.AllPawns[num].IsAnyMetroid())
                    {
                        queen.Map.mapPawns.AllPawns[num].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, otherPawn: otherPawn, forceWake: true);
                    }
                }
            }
        }
    }

    //[HarmonyPatch(typeof(Pawn), "Kill")]
    //public static class RT_Desiccator_Pawn_Kill_Patch
    //{
        //[HarmonyPostfix]
        //public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
        //{
            //if (dinfo.HasValue)
            //{
                //if (dinfo.Value.Instigator != null)
                //{
                    //Thing inst = dinfo.Value.Instigator;
                    //RT_DesiccatorExt desiccator = inst.def.GetModExtension<RT_DesiccatorExt>();
                    //if (desiccator != null)
                    //{
                        //if (desiccator.RT_DesiccatedDef != null)
                        //{
                            //FieldInfo corpse = typeof(Pawn).GetField("Corpse", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
                            //Traverse.Create(__instance);
                            //corpse.SetValue(__instance, ThingMaker.MakeThing(desiccator.RT_DesiccatedDef));
                        //}
                        //else
                        //{
                            //CompRottable compRottable = __instance.Corpse.TryGetComp<CompRottable>();
                            //compRottable.RotImmediately();
                       // }
                    //}
                //}
            //}
            //HediffDef def = DefDatabase<HediffDef>.GetNamed("RT_LifeDrainSickness");
            //if (__instance.health.hediffSet.HasHediff(def))
            //{
                //CompRottable compRottable = __instance.Corpse.TryGetComp<CompRottable>();
                //compRottable.RotImmediately();
            //}
            //if (__instance.Corpse.GetRotStage() == RotStage.Fresh)
            //{
                //Log.Message(__instance + " failed rot");
            //}
            /*
            else
            {
                Log.Message(__instance + " rotted by");
            }
            */
        //}
    //}
}