using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using RT_Core;

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
        private static void Prefix(ref bool __result, Pawn p, Thing food, Pawn getter = null, bool careIfNotAcceptableForTitle = true)
        {
            if (food?.def == RT_DefOf.RT_ProtusSphere && !p.IsAnyMetroid())
            {
                __result = false;
            }
            else if (p.IsAnyMetroid() && food.def != RT_DefOf.RT_ProtusSphere)
            {
                __result = false;
            }
            else if (p.IsAnyMetroid() && food.def == RT_DefOf.RT_ProtusSphere)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "WillEat", new Type[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool) })]
    public static class WillEat_Patch2
    {
        private static void Postfix(ref bool __result, Pawn p, ThingDef food, Pawn getter = null, bool careIfNotAcceptableForTitle = true)
        {
            if (food == RT_DefOf.RT_ProtusSphere && !p.IsAnyMetroid())
            {
                __result = false;
            }
            else if (p.IsAnyMetroid() && food != RT_DefOf.RT_ProtusSphere)
            {
                __result = false;
            }
            else if (p.IsAnyMetroid() && food == RT_DefOf.RT_ProtusSphere)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", new Type[]
    {
            typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult)
    })]
    public static class AddHediff_Patch
    {
        private static HashSet<HediffDef> hediffDefs = new HashSet<HediffDef>
        {
            HediffDef.Named("SandInEyes"),
            HediffDef.Named("DirtInEyes"),
            HediffDef.Named("MudInEyes"),
            HediffDef.Named("GravelInEyes"),
            HediffDef.Named("WaterInEyes")
        };
        private static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
        {
            if (hediffDefs.Contains(hediff.def) && ___pawn.IsAnyMetroid())
            {
                return false;
            }
            var options = hediff.def.GetModExtension<HediffDefections>();
            if (options != null)
            {
                foreach (var hediffData in options.hediffDefections)
                {
                    if (Rand.Chance(hediffData.Value))
                    {
                        var newHediff = HediffMaker.MakeHediff(hediffData.Key, ___pawn);
                        ___pawn.health.AddHediff(newHediff);
                        Messages.Message("RT_DefectionHediff".Translate(hediffData.Key.label, ___pawn.Named("PAWN")), ___pawn, MessageTypeDefOf.NegativeHealthEvent);
                    }
                }
            }
            return true;
        }
    }

    //[HarmonyPatch(typeof(RaceProperties), "CanEverEat", new Type[] { typeof(ThingDef)})]
    //public static class CanEverEat_Patch3
    //{
    //    private static void Postfix(ref bool __result, RaceProperties __instance, ThingDef t)
    //    {
    //        if (t == RT_DefOf.RT_ProtusSphere && !__instance.AnyPawnKind.IsAnyMetroid())
    //        {
    //            __result = false;
    //        }
    //        else if (__instance.AnyPawnKind.IsAnyMetroid() && t != RT_DefOf.RT_ProtusSphere)
    //        {
    //            __result = false;
    //        }
    //        else if (__instance.AnyPawnKind.IsAnyMetroid() && t == RT_DefOf.RT_ProtusSphere)
    //        {
    //            __result = true;
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public static class AddHumanlikeOrders_Patch
    {
        public static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            List<Thing> thingList = c.GetThingList(pawn.Map);

            for (int i = 0; i < thingList.Count; i++)
            {
                var t = thingList[i];
                if (t.def == RT_DefOf.RT_ProtusSphere && !pawn.IsAnyMetroid())
                {
                    string text = (!t.def.ingestible.ingestCommandString.NullOrEmpty()) ? string.Format(t.def.ingestible.ingestCommandString, t.LabelShort) : ((string)"ConsumeThing".Translate(t.LabelShort, t));
                    FloatMenuOption floatMenuOption = opts.FirstOrDefault((FloatMenuOption x) => x.Label.Contains(text));
                    if (floatMenuOption != null)
                    {
                        floatMenuOption.Label += " (" + "RT_Inedible".Translate() + ")";
                        floatMenuOption.action = null;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Faction), "CommFloatMenuOption")]
    public static class CommFloatMenuOption_Patch
    {
        public static bool Prefix(ref FloatMenuOption __result, Faction __instance, Building_CommsConsole console, Pawn negotiator)
        {
            if (__instance.def == RT_DefOf.RT_Metroids)
            {
                __result = null;
                return false;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(Faction), "TryGenerateNewLeader")]
    public static class TryGenerateNewLeader_Patch
    {
        public static bool Prefix(ref bool __result)
        {
            if (Queen.preventFactionLeaderSpawn)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
    public static class CanTakeOrder_Patch
    {
        public static void Postfix(ref bool __result, Pawn pawn)
        {
            if (!__result && (pawn.IsMetroidLarvae() || pawn.IsBanteeMetroid()))
            {
                __result = true;
            }
        }
    }
    [HarmonyPatch(typeof(FloatMenuMakerMap), "ChoicesAtFor")]
    public static class ChoicesAtFor_Patch
    {
        public static void Postfix(ref List<FloatMenuOption> __result, Vector3 clickPos, Pawn pawn)
        {

            if (pawn.Faction == Faction.OfPlayer)
            {
                if (pawn.IsMetroidLarvae() || pawn.IsBanteeMetroid())
                {
                    IntVec3 c = IntVec3.FromVector3(clickPos);
                    foreach (Thing thing in c.GetThingList(pawn.Map))
                    {
                        if (thing is Queen queen && queen.Faction == Faction.OfPlayer)
                        {
                            var comp = pawn.TryGetComp<QueenDroneComp>();
                            if (comp.queen == queen)
                            {
                                Action action = delegate
                                {
                                    comp.RemoveFromQueen();
                                };
                                __result.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RT_LeaveQueen".Translate(), action, MenuOptionPriority.High, null, queen), pawn, queen));
                            }
                            else if (queen.spawnPool.TotalPawns.Count() < SpawnPool.maxPawnCount)
                            {
                                if (pawn.ageTracker.AgeBiologicalYearsFloat >= 75)
                                {
                                    __result.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RT_JoinQueenTooOld".Translate(), null, MenuOptionPriority.High, null, queen), pawn, queen));
                                }
                                else
                                {
                                    Action action = delegate
                                    {
                                        queen.spawnPool.spawnedPawns.Add(pawn);
                                        comp.AssignToQueen(queen);
                                    };
                                    __result.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RT_JoinQueen".Translate(), action, MenuOptionPriority.High, null, queen), pawn, queen));
                                }
                            }
                            else
                            {
                                __result.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RT_JoinQueenLimit".Translate(), null, MenuOptionPriority.High, null, queen), pawn, queen));
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Need_Food), "NeedInterval")]
    public static class RT_NeedInterval_Patch
    {
        public static float GetBerserkChance(float curFoodLevel, Dictionary<float, float> hungerValues)
        {
            var keys = hungerValues.Keys.OrderByDescending(x => x);
            float result = 0;
            foreach (var key in keys)
            {
                if (key >= curFoodLevel)
                {
                    result = hungerValues[key];
                }
            }
            return result;
        }

        public static bool Prefix(Need_Food __instance, Pawn ___pawn)
        {
            if (___pawn.IsAnyMetroid() && ___pawn.CurJobDef == JobDefOf.LayDown)
            {
                return false;
            }
            var comp = ___pawn.TryGetComp<QueenDroneComp>();
            if (comp?.queen != null && __instance.CurLevelPercentage <= 0.3f)
            {
                return false;
            }
            return true;
        }
        public static void Postfix(Need_Food __instance, Pawn ___pawn)
        {
            if (RimtroidSettings.allowBerserkChanceMetroidHunger)
            {
                var options = ___pawn.kindDef.GetModExtension<HungerBerserkOptions>();
                if (options != null)
                {
                    var comp = ___pawn.TryGetComp<QueenDroneComp>();
                    if (comp?.queen != null)
                    {
                        return;
                    }
                    var berserkChance = GetBerserkChance(__instance.CurLevelPercentage, options.hungerBerserkChanges);
                    //Log.Message(___pawn + " has " + berserkChance + " berserk chance, cur food level: " + __instance.CurLevelPercentage, true);
                    if (berserkChance > 0)
                    {
                        if (!___pawn.InMentalState && Rand.Chance(berserkChance))
                        {
                            if (___pawn.CurJobDef != JobDefOf.LayDown && ___pawn.CurJobDef != RT_DefOf.RT_EatFromStation && ___pawn.CurJobDef != RT_DefOf.RT_AbsorbingEnergy && !InCombat(___pawn))
                            {
                                //Log.Message(___pawn + " gets berserk state", true);
                                if (___pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, forceWake: true))
                                {
                                    if (RimtroidSettings.allowWildChanceMetroidBerserk && ___pawn.Faction == Faction.OfPlayer && Rand.Chance(options.chanceToBecomeWildIfBerserkAndTamed))
                                    {
                                        ___pawn.SetFaction(null);
                                    }
                                }
                            }
                        }
                    }
                    else if (___pawn.mindState.mentalStateHandler.CurStateDef == MentalStateDefOf.Berserk)
                    {
                        //Log.Message(___pawn + " recovers from berserk state", true);
                        ___pawn.MentalState.RecoverFromState();
                    }
                }
            }
        }

        public static HashSet<JobDef> combatJobs = new HashSet<JobDef>
                                                    {
                                                        JobDefOf.AttackMelee,
                                                        JobDefOf.AttackStatic,
                                                        JobDefOf.FleeAndCower,
                                                        JobDefOf.ManTurret,
                                                        JobDefOf.Wait_Combat,
                                                        JobDefOf.Flee
                                                    };
        private static bool InCombat(Pawn pawn)
        {
            if (combatJobs.Contains(pawn.CurJobDef))
            {
                return true;
            }
            else if (pawn.mindState.duty?.def.alwaysShowWeapon ?? false)
            {
                return true;
            }
            else if (pawn.CurJobDef?.alwaysShowWeapon ?? false)
            {
                return true;
            }
            else if (pawn.mindState.lastEngageTargetTick > Find.TickManager.TicksGame - 1000)
            {
                return true;
            }
            else if (pawn.mindState.lastAttackTargetTick > Find.TickManager.TicksGame - 1000)
            {
                return true;
            }
            return false;
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