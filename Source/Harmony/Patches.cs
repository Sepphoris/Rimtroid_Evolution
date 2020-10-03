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


    [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
    public static class RT_TryStartMentalState_Patch
    {
        public static void Postfix(Pawn ___pawn, bool __result, MentalStateDef stateDef, string reason = null, bool forceWake = false, bool causedByMood = false, Pawn otherPawn = null, bool transitionSilently = false)
        {
            if (__result && ___pawn is Queen queen && stateDef.IsAggro)
            {
                foreach (var pawn in queen.Map.mapPawns.AllPawns)
                {
                    if (pawn.Faction != Faction.OfPlayer && !pawn.InAggroMentalState && pawn.IsMetroid())
                    {
                        pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, otherPawn: otherPawn, forceWake: true);
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(Need_Food), "NeedInterval")]
    public static class RT_NeedInterval_Patch
    {
        public static void Postfix(Need_Food __instance, Pawn ___pawn)
        {
            var options = ___pawn.kindDef.GetModExtension<HungerBerserkOptions>();
            if (options != null)
            {
                var key = options.hungerBerserkChanges.Keys.MaxBy(x => x >= __instance.CurLevelPercentage);
                var berserkChance = options.hungerBerserkChanges[key];
                if (berserkChance > 0)
                {
                    Log.Message(___pawn + " has " + berserkChance + " berserk chance, cur food level: " + __instance.CurLevelPercentage, true);
                    if (!___pawn.InMentalState && Rand.Chance(berserkChance))
                    {
                        Log.Message(___pawn + " gets berserk state", true);
                        if (___pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, forceWake: true))
                        {
                            if (___pawn.Faction == Faction.OfPlayer && Rand.Chance(options.chanceToBecomeWildIfBerserkAndTamed))
                            {
                                ___pawn.SetFaction(null);
                            }
                        }
                    }
                }
                else if (___pawn.mindState.mentalStateHandler.CurStateDef == MentalStateDefOf.Berserk)
                {
                    Log.Message(___pawn + " recovers from berserk state", true);
                    ___pawn.MentalState.RecoverFromState();
                }
                Log.Message(___pawn + " - " + __instance.CurLevelPercentage + " - " + key, true);
            }
        }
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