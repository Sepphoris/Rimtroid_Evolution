using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;

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

    [HarmonyPatch(typeof(FoodUtility), "WillEat", new Type[] { typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool) })]
    public static class WillEat_Patch1
    {
        private static bool Prefix(Pawn p, Thing food, Pawn getter = null, bool careIfNotAcceptableForTitle = true)
        {
            if (food?.def == RT_DefOf.RT_ProtusSphere && !p.IsAnyMetroid())
            {
                return false;
            }
            else if (p.IsAnyMetroid() && food.def != RT_DefOf.RT_ProtusSphere)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "WillEat", new Type[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool) })]
    public static class WillEat_Patch2
    {
        private static bool Prefix(Pawn p, ThingDef food, Pawn getter = null, bool careIfNotAcceptableForTitle = true)
        {
            if (food == RT_DefOf.RT_ProtusSphere && !p.IsAnyMetroid())
            {
                return false;
            }
            else if (p.IsAnyMetroid() && food != RT_DefOf.RT_ProtusSphere)
            {
                return false;
            }
            return true;
        }
    }

    //[HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    //public static class SpawnSetup_Patch
    // {
    //public static bool Prefix(Pawn __instance, Map map, bool respawningAfterLoad)
    //{
    //if (!respawningAfterLoad && GenDate.DaysPassed < 3)
    //{
    //if (__instance.def == RT_DefOf.RT_GammaMetroid || __instance.def == RT_DefOf.RT_ZetaMetroid || __instance.def == RT_DefOf.RT_OmegaMetroid)
    //{
    //return false;
    //}
    //}
    //return true;
    //}
    //}

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