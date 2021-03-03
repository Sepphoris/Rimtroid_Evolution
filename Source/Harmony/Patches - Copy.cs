using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RT_Rimtroid
{
    public static class FoodUtilityTest
    {
        public const int FoodPoisoningStageInitial = 2;

        public const int FoodPoisoningStageMajor = 1;

        public const int FoodPoisoningStageRecovering = 0;

        public static float? bestFoodSourceOnMap_minNutrition_NewTemp = null;

        private static HashSet<Thing> filtered = new HashSet<Thing>();

        private static readonly SimpleCurve FoodOptimalityEffectFromMoodCurve = new SimpleCurve
                {
                        new CurvePoint(-100f, -600f),
                        new CurvePoint(-10f, -100f),
                        new CurvePoint(-5f, -70f),
                        new CurvePoint(-1f, -50f),
                        new CurvePoint(0f, 0f),
                        new CurvePoint(100f, 800f)
                };

        private static List<Pawn> tmpPredatorCandidates = new List<Pawn>();

        private static List<ThoughtDef> ingestThoughts = new List<ThoughtDef>();

        public static bool InappropriateForTitle(ThingDef food, Pawn p, bool allowIfStarving)
        {
            Log.Message("FoodUtilityTest - InappropriateForTitle - if ((allowIfStarving && p.needs.food.Starving) || (p.story != null - 19", true);
            if ((allowIfStarving && p.needs.food.Starving) || (p.story != null
                && p.story.traits.HasTrait(TraitDefOf.Ascetic)) ||
                p.IsPrisoner || (food.ingestible.joyKind != null && food.ingestible.joy > 0f))
            {
                Log.Message("FoodUtilityTest - InappropriateForTitle - return false; - 20", true);
                return false;
            }
            RoyalTitle royalTitle = p.royalty?.MostSeniorTitle;
            Log.Message("FoodUtilityTest - InappropriateForTitle - if (royalTitle != null && royalTitle.conceited && royalTitle.def.foodRequirement.Defined) - 22", true);
            if (royalTitle != null && royalTitle.conceited && royalTitle.def.foodRequirement.Defined)
            {
                Log.Message("FoodUtilityTest - InappropriateForTitle - return !royalTitle.def.foodRequirement.Acceptable(food); - 23", true);
                return !royalTitle.def.foodRequirement.Acceptable(food);
            }
            return false;
        }

        public static bool TryFindBestFoodSourceFor(Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool allowForbidden = false, bool allowCorpse = true, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false, bool ignoreReservations = false, FoodPreferability minPrefOverride = FoodPreferability.Undefined)
        {
            Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - bool flag = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation); - 25", true);
            bool flag = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - bool allowDrug = !eater.IsTeetotaler(); - 26", true);
            bool allowDrug = !eater.IsTeetotaler();
            Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - Thing thing = null; - 27", true);
            Thing thing = null;
            Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - if (canUseInventory) - 28", true);
            if (canUseInventory)
            {
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - if (flag) - 29", true);
                if (flag)
                {
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - thing = BestFoodInInventory(getter, eater, (minPrefOverride == FoodPreferability.Undefined) ? FoodPreferability.MealAwful : minPrefOverride); - 30", true);
                    thing = BestFoodInInventory(getter, eater, (minPrefOverride == FoodPreferability.Undefined) ? FoodPreferability.MealAwful : minPrefOverride);
                }
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - if (thing != null) - 31", true);
                if (thing != null)
                {
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - if (getter.Faction != Faction.OfPlayer) - 32", true);
                    if (getter.Faction != Faction.OfPlayer)
                    {
                        Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodSource = thing; - 33", true);
                        foodSource = thing;
                        Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodDef = GetFinalIngestibleDef(foodSource); - 34", true);
                        foodDef = GetFinalIngestibleDef(foodSource);
                        Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - return true; - 35", true);
                        return true;
                    }
                    CompRottable compRottable = thing.TryGetComp<CompRottable>();
                    if (compRottable != null && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
                    {
                        Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodSource = thing; - 38", true);
                        foodSource = thing;
                        Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodDef = GetFinalIngestibleDef(foodSource); - 39", true);
                        foodDef = GetFinalIngestibleDef(foodSource);
                        Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - return true; - 40", true);
                        return true;
                    }
                }
            }
            bool allowPlant = getter == eater;
            Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - bool allowForbidden2 = allowForbidden; - 42", true);
            bool allowForbidden2 = allowForbidden;
            Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - ThingDef foodDef2; - 43", true);
            ThingDef foodDef2;
            Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - Thing thing2 = BestFoodSourceOnMap(getter, eater, desperate, out foodDef2, FoodPreferability.MealLavish, allowPlant, allowDrug, allowCorpse, allowDispenserFull: true, canRefillDispenser, allowForbidden2, allowSociallyImproper, allowHarvest, forceScanWholeMap, ignoreReservations, minPrefOverride); - 44", true);
            Thing thing2 = BestFoodSourceOnMap(getter, eater, desperate, out foodDef2, FoodPreferability.MealLavish, allowPlant, allowDrug, allowCorpse, allowDispenserFull: true, canRefillDispenser, allowForbidden2, allowSociallyImproper, allowHarvest, forceScanWholeMap, ignoreReservations, minPrefOverride);
            Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - if (thing != null || thing2 != null) - 45", true);
            if (thing != null || thing2 != null)
            {
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - if (thing == null && thing2 != null) - 46", true);
                if (thing == null && thing2 != null)
                {
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodSource = thing2; - 47", true);
                    foodSource = thing2;
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodDef = foodDef2; - 48", true);
                    foodDef = foodDef2;
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - return true; - 49", true);
                    return true;
                }
                ThingDef finalIngestibleDef = GetFinalIngestibleDef(thing);
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - if (thing2 == null) - 51", true);
                if (thing2 == null)
                {
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodSource = thing; - 52", true);
                    foodSource = thing;
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodDef = finalIngestibleDef; - 53", true);
                    foodDef = finalIngestibleDef;
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - return true; - 54", true);
                    return true;
                }
                float num = FoodOptimality(eater, thing2, foodDef2, (getter.Position - thing2.Position).LengthManhattan);
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - float num2 = FoodOptimality(eater, thing, finalIngestibleDef, 0f); - 56", true);
                float num2 = FoodOptimality(eater, thing, finalIngestibleDef, 0f);
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - num2 -= 32f; - 57", true);
                num2 -= 32f;
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - if (num > num2) - 58", true);
                if (num > num2)
                {
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodSource = thing2; - 59", true);
                    foodSource = thing2;
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodDef = foodDef2; - 60", true);
                    foodDef = foodDef2;
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - return true; - 61", true);
                    return true;
                }
                foodSource = thing;
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodDef = GetFinalIngestibleDef(foodSource); - 63", true);
                foodDef = GetFinalIngestibleDef(foodSource);
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - return true; - 64", true);
                return true;
            }
            Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - if (canUseInventory && flag) - 65", true);
            if (canUseInventory && flag)
            {
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - thing = BestFoodInInventory(getter, eater, FoodPreferability.DesperateOnly, FoodPreferability.MealLavish, 0f, allowDrug); - 66", true);
                thing = BestFoodInInventory(getter, eater, FoodPreferability.DesperateOnly, FoodPreferability.MealLavish, 0f, allowDrug);
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - if (thing != null) - 67", true);
                if (thing != null)
                {
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodSource = thing; - 68", true);
                    foodSource = thing;
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodDef = GetFinalIngestibleDef(foodSource); - 69", true);
                    foodDef = GetFinalIngestibleDef(foodSource);
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - return true; - 70", true);
                    return true;
                }
            }
            Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - if (thing2 == null && getter == eater && (getter.RaceProps.predator || (getter.IsWildMan() && !getter.IsPrisoner && !getter.WorkTypeIsDisabled(WorkTypeDefOf.Hunting)))) - 71", true);
            if (thing2 == null && getter == eater && (getter.RaceProps.predator || (getter.IsWildMan() && !getter.IsPrisoner && !getter.WorkTypeIsDisabled(WorkTypeDefOf.Hunting))))
            {
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - Pawn pawn = BestPawnToHuntForPredator(getter, forceScanWholeMap); - 72", true);
                Pawn pawn = BestPawnToHuntForPredator(getter, forceScanWholeMap);
                Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - if (pawn != null) - 73", true);
                if (pawn != null)
                {
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodSource = pawn; - 74", true);
                    foodSource = pawn;
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodDef = GetFinalIngestibleDef(foodSource); - 75", true);
                    foodDef = GetFinalIngestibleDef(foodSource);
                    Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - return true; - 76", true);
                    return true;
                }
            }
            foodSource = null;
            Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - foodDef = null; - 78", true);
            foodDef = null;
            Log.Message("FoodUtilityTest - TryFindBestFoodSourceFor - return false; - 79", true);
            return false;
        }

        public static ThingDef GetFinalIngestibleDef(Thing foodSource, bool harvest = false)
        {
            Log.Message("FoodUtilityTest - GetFinalIngestibleDef - Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser; - 80", true);
            Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
            Log.Message("FoodUtilityTest - GetFinalIngestibleDef - if (building_NutrientPasteDispenser != null) - 81", true);
            if (building_NutrientPasteDispenser != null)
            {
                Log.Message("FoodUtilityTest - GetFinalIngestibleDef - return building_NutrientPasteDispenser.DispensableDef; - 82", true);
                return building_NutrientPasteDispenser.DispensableDef;
            }
            Pawn pawn = foodSource as Pawn;
            Log.Message("FoodUtilityTest - GetFinalIngestibleDef - if (pawn != null) - 84", true);
            if (pawn != null)
            {
                Log.Message("FoodUtilityTest - GetFinalIngestibleDef - return pawn.RaceProps.corpseDef; - 85", true);
                return pawn.RaceProps.corpseDef;
            }
            Log.Message("FoodUtilityTest - GetFinalIngestibleDef - if (harvest) - 86", true);
            if (harvest)
            {
                Log.Message("FoodUtilityTest - GetFinalIngestibleDef - Plant plant = foodSource as Plant; - 87", true);
                Plant plant = foodSource as Plant;
                Log.Message("FoodUtilityTest - GetFinalIngestibleDef - if (plant != null && plant.HarvestableNow && plant.def.plant.harvestedThingDef.IsIngestible) - 88", true);
                if (plant != null && plant.HarvestableNow && plant.def.plant.harvestedThingDef.IsIngestible)
                {
                    Log.Message("FoodUtilityTest - GetFinalIngestibleDef - return plant.def.plant.harvestedThingDef; - 89", true);
                    return plant.def.plant.harvestedThingDef;
                }
            }
            return foodSource.def;
        }

        public static Thing BestFoodInInventory(Pawn holder, Pawn eater = null, FoodPreferability minFoodPref = FoodPreferability.NeverForNutrition, FoodPreferability maxFoodPref = FoodPreferability.MealLavish, float minStackNutrition = 0f, bool allowDrug = false)
        {
            Log.Message("FoodUtilityTest - BestFoodInInventory - if (holder.inventory == null) - 91", true);
            if (holder.inventory == null)
            {
                Log.Message("FoodUtilityTest - BestFoodInInventory - return null; - 92", true);
                return null;
            }
            Log.Message("FoodUtilityTest - BestFoodInInventory - if (eater == null) - 93", true);
            if (eater == null)
            {
                Log.Message("FoodUtilityTest - BestFoodInInventory - eater = holder; - 94", true);
                eater = holder;
            }
            ThingOwner<Thing> innerContainer = holder.inventory.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                Log.Message("FoodUtilityTest - BestFoodInInventory - Thing thing = innerContainer[i]; - 96", true);
                Thing thing = innerContainer[i];
                Log.Message("FoodUtilityTest - BestFoodInInventory - if (thing.def.IsNutritionGivingIngestible && thing.IngestibleNow && eater.WillEat(thing, holder) && (int)thing.def.ingestible.preferability >= (int)minFoodPref && (int)thing.def.ingestible.preferability <= (int)maxFoodPref && (allowDrug || !thing.def.IsDrug) && thing.GetStatValue(StatDefOf.Nutrition) * (float)thing.stackCount >= minStackNutrition) - 97", true);
                if (thing.def.IsNutritionGivingIngestible && thing.IngestibleNow && eater.WillEat(thing, holder) && (int)thing.def.ingestible.preferability >= (int)minFoodPref && (int)thing.def.ingestible.preferability <= (int)maxFoodPref && (allowDrug || !thing.def.IsDrug) && thing.GetStatValue(StatDefOf.Nutrition) * (float)thing.stackCount >= minStackNutrition)
                {
                    Log.Message("FoodUtilityTest - BestFoodInInventory - return thing; - 98", true);
                    return thing;
                }
            }
            return null;
        }

        public static int GetMaxAmountToPickup(Thing food, Pawn pawn, int wantedCount)
        {
            Log.Message("FoodUtilityTest - GetMaxAmountToPickup - if (food is Building_NutrientPasteDispenser) - 100", true);
            if (food is Building_NutrientPasteDispenser)
            {
                Log.Message("FoodUtilityTest - GetMaxAmountToPickup - if (!pawn.CanReserve(food)) - 101", true);
                if (!pawn.CanReserve(food))
                {
                    Log.Message("FoodUtilityTest - GetMaxAmountToPickup - return 0; - 102", true);
                    return 0;
                }
                return -1;
            }
            Log.Message("FoodUtilityTest - GetMaxAmountToPickup - if (food is Corpse) - 104", true);
            if (food is Corpse)
            {
                Log.Message("FoodUtilityTest - GetMaxAmountToPickup - if (!pawn.CanReserve(food)) - 105", true);
                if (!pawn.CanReserve(food))
                {
                    Log.Message("FoodUtilityTest - GetMaxAmountToPickup - return 0; - 106", true);
                    return 0;
                }
                return 1;
            }
            int num = Math.Min(wantedCount, food.stackCount);
            Log.Message("FoodUtilityTest - GetMaxAmountToPickup - if (food.Spawned && food.Map != null) - 109", true);
            if (food.Spawned && food.Map != null)
            {
                Log.Message("FoodUtilityTest - GetMaxAmountToPickup - return Math.Min(num, food.Map.reservationManager.CanReserveStack(pawn, food, 10)); - 110", true);
                return Math.Min(num, food.Map.reservationManager.CanReserveStack(pawn, food, 10));
            }
            return num;
        }

        public static Thing BestFoodSourceOnMap(Pawn getter, Pawn eater, bool desperate, out ThingDef foodDef, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false, bool ignoreReservations = false, FoodPreferability minPrefOverride = FoodPreferability.Undefined)
        {
            Log.Message("FoodUtilityTest - BestFoodSourceOnMap - foodDef = null; - 112", true);
            foodDef = null;
            Log.Message("FoodUtilityTest - BestFoodSourceOnMap - bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation); - 113", true);
            bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (!getterCanManipulate && getter != eater) - 114", true);
            if (!getterCanManipulate && getter != eater)
            {
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - Log.Error(string.Concat(getter, \" tried to find food to bring to \", eater, \" but \", getter, \" is incapable of Manipulation.\")); - 115", true);
                Log.Error(string.Concat(getter, " tried to find food to bring to ", eater, " but ", getter, " is incapable of Manipulation."));
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - return null; - 116", true);
                return null;
            }
            FoodPreferability minPref;
            Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (minPrefOverride == FoodPreferability.Undefined) - 118", true);
            if (minPrefOverride == FoodPreferability.Undefined)
            {
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (eater.NonHumanlikeOrWildMan()) - 119", true);
                if (eater.NonHumanlikeOrWildMan())
                {
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - minPref = FoodPreferability.NeverForNutrition; - 120", true);
                    minPref = FoodPreferability.NeverForNutrition;
                }
                else if (desperate)
                {
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - minPref = FoodPreferability.DesperateOnly; - 122", true);
                    minPref = FoodPreferability.DesperateOnly;
                }
                else
                {
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - minPref = (((int)eater.needs.food.CurCategory >= 2) ? FoodPreferability.RawBad : FoodPreferability.MealAwful); - 123", true);
                    minPref = (((int)eater.needs.food.CurCategory >= 2) ? FoodPreferability.RawBad : FoodPreferability.MealAwful);
                }
            }
            else
            {
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - minPref = minPrefOverride; - 124", true);
                minPref = minPrefOverride;
            }
            Predicate<Thing> foodValidator = delegate (Thing t)
            {
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - Building_NutrientPasteDispenser building_NutrientPasteDispenser = t as Building_NutrientPasteDispenser; - 125", true);
                Building_NutrientPasteDispenser building_NutrientPasteDispenser = t as Building_NutrientPasteDispenser;
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (building_NutrientPasteDispenser != null) - 126", true);
                if (building_NutrientPasteDispenser != null)
                {
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (!allowDispenserFull || !getterCanManipulate || (int)ThingDefOf.MealNutrientPaste.ingestible.preferability < (int)minPref || (int)ThingDefOf.MealNutrientPaste.ingestible.preferability > (int)maxPref || !eater.WillEat(ThingDefOf.MealNutrientPaste, getter) || (t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!allowForbidden && t.IsForbidden(getter)) || !building_NutrientPasteDispenser.powerComp.PowerOn || (!allowDispenserEmpty && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers()) || !t.InteractionCell.Standable(t.Map) || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some))) - 127", true);
                    if (!allowDispenserFull || !getterCanManipulate || (int)ThingDefOf.MealNutrientPaste.ingestible.preferability < (int)minPref || (int)ThingDefOf.MealNutrientPaste.ingestible.preferability > (int)maxPref || !eater.WillEat(ThingDefOf.MealNutrientPaste, getter) || (t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!allowForbidden && t.IsForbidden(getter)) || !building_NutrientPasteDispenser.powerComp.PowerOn || (!allowDispenserEmpty && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers()) || !t.InteractionCell.Standable(t.Map) || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some)))
                    {
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - return false; - 128", true);
                        return false;
                    }
                }
                else
                {
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - int stackCount = 1; - 129", true);
                    int stackCount = 1;
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (bestFoodSourceOnMap_minNutrition_NewTemp.HasValue) - 130", true);
                    if (bestFoodSourceOnMap_minNutrition_NewTemp.HasValue)
                    {
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - float statValue = t.GetStatValue(StatDefOf.Nutrition); - 131", true);
                        float statValue = t.GetStatValue(StatDefOf.Nutrition);
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - stackCount = StackCountForNutrition(bestFoodSourceOnMap_minNutrition_NewTemp.Value, statValue); - 132", true);
                        stackCount = StackCountForNutrition(bestFoodSourceOnMap_minNutrition_NewTemp.Value, statValue);
                    }
                    Log.Message("Checking: " + t);
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if ((int)t.def.ingestible.preferability < (int)minPref || (int)t.def.ingestible.preferability > (int)maxPref - 134", true);
                    if ((int)t.def.ingestible.preferability < (int)minPref || (int)t.def.ingestible.preferability > (int)maxPref
                    || !eater.WillEat(t, getter) || !t.def.IsNutritionGivingIngestible || !t.IngestibleNow ||
                    (!allowCorpse && t is Corpse) || (!allowDrug && t.def.IsDrug) || (!allowForbidden && t.IsForbidden(getter))
                    || (!desperate && t.IsNotFresh()) || t.IsDessicated() || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper)
                    || (!getter.AnimalAwareOf(t) && !forceScanWholeMap) || (!ignoreReservations && !getter.CanReserve(t, 10, stackCount)))
                    {
                        Log.Message("Fail: " + t);
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - return false; - 136", true);
                        return false;
                    }
                }
                return true;
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - }; - 138", true);
            };
            Log.Message("FoodUtilityTest - BestFoodSourceOnMap - ThingRequest thingRequest = (!((eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) != 0 && allowPlant)) ? ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree) : ThingRequest.ForGroup(ThingRequestGroup.FoodSource); - 139", true);
            ThingRequest thingRequest = (!((eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) != 0 && allowPlant)) ? ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree) : ThingRequest.ForGroup(ThingRequestGroup.FoodSource);
            Log.Message("FoodUtilityTest - BestFoodSourceOnMap - Thing bestThing; - 140", true);
            Thing bestThing;
            Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (getter.RaceProps.Humanlike) - 141", true);
            if (getter.RaceProps.Humanlike)
            {
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - bestThing = SpawnedFoodSearchInnerScan(eater, getter.Position, getter.Map.listerThings.ThingsMatching(thingRequest), PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, foodValidator); - 142", true);
                bestThing = SpawnedFoodSearchInnerScan(eater, getter.Position, getter.Map.listerThings.ThingsMatching(thingRequest), PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, foodValidator);
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (allowHarvest && getterCanManipulate) - 143", true);
                if (allowHarvest && getterCanManipulate)
                {
                    Thing thing = GenClosest.ClosestThingReachable(searchRegionsMax: (!forceScanWholeMap || bestThing != null) ? 30 : (-1), root: getter.Position, map: getter.Map, thingReq: ThingRequest.ForGroup(ThingRequestGroup.HarvestablePlant), peMode: PathEndMode.Touch, traverseParams: TraverseParms.For(getter), maxDistance: 9999f, validator: delegate (Thing x)
                    {
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - Plant plant = (Plant)x; - 144", true);
                        Plant plant = (Plant)x;
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (!plant.HarvestableNow) - 145", true);
                        if (!plant.HarvestableNow)
                        {
                            Log.Message("FoodUtilityTest - BestFoodSourceOnMap - return false; - 146", true);
                            return false;
                        }
                        ThingDef harvestedThingDef = plant.def.plant.harvestedThingDef;
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (!harvestedThingDef.IsNutritionGivingIngestible) - 148", true);
                        if (!harvestedThingDef.IsNutritionGivingIngestible)
                        {
                            Log.Message("FoodUtilityTest - BestFoodSourceOnMap - return false; - 149", true);
                            return false;
                        }
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (!eater.WillEat(harvestedThingDef, getter)) - 150", true);
                        if (!eater.WillEat(harvestedThingDef, getter))
                        {
                            Log.Message("FoodUtilityTest - BestFoodSourceOnMap - return false; - 151", true);
                            return false;
                        }
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (!getter.CanReserve(plant)) - 152", true);
                        if (!getter.CanReserve(plant))
                        {
                            Log.Message("FoodUtilityTest - BestFoodSourceOnMap - return false; - 153", true);
                            return false;
                        }
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (!allowForbidden && plant.IsForbidden(getter)) - 154", true);
                        if (!allowForbidden && plant.IsForbidden(getter))
                        {
                            Log.Message("FoodUtilityTest - BestFoodSourceOnMap - return false; - 155", true);
                            return false;
                        }
                        return (bestThing == null || (int)GetFinalIngestibleDef(bestThing).ingestible.preferability < (int)harvestedThingDef.ingestible.preferability) ? true : false;
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - }); - 157", true);
                    });
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (thing != null) - 158", true);
                    if (thing != null)
                    {
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - bestThing = thing; - 159", true);
                        bestThing = thing;
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - foodDef = GetFinalIngestibleDef(thing, harvest: true); - 160", true);
                        foodDef = GetFinalIngestibleDef(thing, harvest: true);
                    }
                }
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (foodDef == null && bestThing != null) - 161", true);
                if (foodDef == null && bestThing != null)
                {
                    Log.Message("FINAl: " + foodDef + " - " + bestThing);
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - foodDef = GetFinalIngestibleDef(bestThing); - 162", true);
                    foodDef = GetFinalIngestibleDef(bestThing);
                }
            }
            else
            {
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - int maxRegionsToScan = GetMaxRegionsToScan(getter, forceScanWholeMap); - 163", true);
                int maxRegionsToScan = GetMaxRegionsToScan(getter, forceScanWholeMap);
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - filtered.Clear(); - 164", true);
                filtered.Clear();
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - foreach (Thing item in GenRadial.RadialDistinctThingsAround(getter.Position, getter.Map, 2f, useCenter: true)) - 165", true);
                foreach (Thing item in GenRadial.RadialDistinctThingsAround(getter.Position, getter.Map, 2f, useCenter: true))
                {
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - Pawn pawn = item as Pawn; - 166", true);
                    Pawn pawn = item as Pawn;
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (pawn != null && pawn != getter && pawn.RaceProps.Animal && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Ingest && pawn.CurJob.GetTarget(TargetIndex.A).HasThing) - 167", true);
                    if (pawn != null && pawn != getter && pawn.RaceProps.Animal && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Ingest && pawn.CurJob.GetTarget(TargetIndex.A).HasThing)
                    {
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - filtered.Add(pawn.CurJob.GetTarget(TargetIndex.A).Thing); - 168", true);
                        filtered.Add(pawn.CurJob.GetTarget(TargetIndex.A).Thing);
                    }
                }
                bool ignoreEntirelyForbiddenRegions = !allowForbidden && ForbidUtility.CaresAboutForbidden(getter, cellTarget: true) && getter.playerSettings != null && getter.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap != null;
                Predicate<Thing> validator = delegate (Thing t)
                {
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (!foodValidator(t)) - 170", true);
                    if (!foodValidator(t))
                    {
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - return false; - 171", true);
                        return false;
                    }
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (filtered.Contains(t)) - 172", true);
                    if (filtered.Contains(t))
                    {
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - return false; - 173", true);
                        return false;
                    }
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (!(t is Building_NutrientPasteDispenser) && (int)t.def.ingestible.preferability <= 2) - 174", true);
                    if (!(t is Building_NutrientPasteDispenser) && (int)t.def.ingestible.preferability <= 2)
                    {
                        Log.Message("FoodUtilityTest - BestFoodSourceOnMap - return false; - 175", true);
                        return false;
                    }
                    return (!t.IsNotFresh()) ? true : false;
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - }; - 177", true);
                };
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, validator, null, 0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions); - 178", true);
                bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, validator, null, 0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - filtered.Clear(); - 179", true);
                filtered.Clear();
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (bestThing == null) - 180", true);
                if (bestThing == null)
                {
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - desperate = true; - 181", true);
                    desperate = true;
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, foodValidator, null, 0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions); - 182", true);
                    bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, foodValidator, null, 0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
                }
                Log.Message("FoodUtilityTest - BestFoodSourceOnMap - if (bestThing != null) - 183", true);
                if (bestThing != null)
                {
                    Log.Message("FoodUtilityTest - BestFoodSourceOnMap - foodDef = GetFinalIngestibleDef(bestThing); - 184", true);
                    foodDef = GetFinalIngestibleDef(bestThing);
                }
            }
            Log.Message("Returning: " + bestThing);
            return bestThing;
        }

        private static int GetMaxRegionsToScan(Pawn getter, bool forceScanWholeMap)
        {
            Log.Message("FoodUtilityTest - GetMaxRegionsToScan - if (getter.RaceProps.Humanlike) - 186", true);
            if (getter.RaceProps.Humanlike)
            {
                Log.Message("FoodUtilityTest - GetMaxRegionsToScan - return -1; - 187", true);
                return -1;
            }
            Log.Message("FoodUtilityTest - GetMaxRegionsToScan - if (forceScanWholeMap) - 188", true);
            if (forceScanWholeMap)
            {
                Log.Message("FoodUtilityTest - GetMaxRegionsToScan - return -1; - 189", true);
                return -1;
            }
            Log.Message("FoodUtilityTest - GetMaxRegionsToScan - if (getter.Faction == Faction.OfPlayer) - 190", true);
            if (getter.Faction == Faction.OfPlayer)
            {
                Log.Message("FoodUtilityTest - GetMaxRegionsToScan - return 100; - 191", true);
                return 100;
            }
            return 30;
        }

        private static bool IsFoodSourceOnMapSociallyProper(Thing t, Pawn getter, Pawn eater, bool allowSociallyImproper)
        {
            Log.Message("FoodUtilityTest - IsFoodSourceOnMapSociallyProper - if (!allowSociallyImproper) - 193", true);
            if (!allowSociallyImproper)
            {
                Log.Message("FoodUtilityTest - IsFoodSourceOnMapSociallyProper - bool animalsCare = !getter.RaceProps.Animal; - 194", true);
                bool animalsCare = !getter.RaceProps.Animal;
                Log.Message("FoodUtilityTest - IsFoodSourceOnMapSociallyProper - if (!t.IsSociallyProper(getter) && !t.IsSociallyProper(eater, eater.IsPrisonerOfColony, animalsCare)) - 195", true);
                if (!t.IsSociallyProper(getter) && !t.IsSociallyProper(eater, eater.IsPrisonerOfColony, animalsCare))
                {
                    Log.Message("FoodUtilityTest - IsFoodSourceOnMapSociallyProper - return false; - 196", true);
                    return false;
                }
            }
            return true;
        }

        public static float FoodOptimality(Pawn eater, Thing foodSource, ThingDef foodDef, float dist, bool takingToInventory = false)
        {
            Log.Message("FoodUtilityTest - FoodOptimality - float num = 300f; - 198", true);
            float num = 300f;
            Log.Message("FoodUtilityTest - FoodOptimality - num -= dist; - 199", true);
            num -= dist;
            switch (foodDef.ingestible.preferability)
            {
                case FoodPreferability.NeverForNutrition:
                    return -9999999f;
                case FoodPreferability.DesperateOnly:
                    num -= 150f;
                    Log.Message("FoodUtilityTest - FoodOptimality - break; - 202", true);
                    break;
                case FoodPreferability.DesperateOnlyForHumanlikes:
                    Log.Message("FoodUtilityTest - FoodOptimality - if (eater.RaceProps.Humanlike) - 203", true);
                    if (eater.RaceProps.Humanlike)
                    {
                        Log.Message("FoodUtilityTest - FoodOptimality - num -= 150f; - 204", true);
                        num -= 150f;
                    }
                    break;
            }
            CompRottable compRottable = foodSource.TryGetComp<CompRottable>();
            Log.Message("FoodUtilityTest - FoodOptimality - if (compRottable != null) - 207", true);
            if (compRottable != null)
            {
                Log.Message("FoodUtilityTest - FoodOptimality - if (compRottable.Stage == RotStage.Dessicated) - 208", true);
                if (compRottable.Stage == RotStage.Dessicated)
                {
                    Log.Message("FoodUtilityTest - FoodOptimality - return -9999999f; - 209", true);
                    return -9999999f;
                }
                if (!takingToInventory && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
                {
                    Log.Message("FoodUtilityTest - FoodOptimality - num += 12f; - 211", true);
                    num += 12f;
                }
            }
            Log.Message("FoodUtilityTest - FoodOptimality - if (eater.needs != null && eater.needs.mood != null) - 212", true);
            if (eater.needs != null && eater.needs.mood != null)
            {
                Log.Message("FoodUtilityTest - FoodOptimality - List<ThoughtDef> list = ThoughtsFromIngesting(eater, foodSource, foodDef); - 213", true);
                List<ThoughtDef> list = ThoughtsFromIngesting(eater, foodSource, foodDef);
                for (int i = 0; i < list.Count; i++)
                {
                    Log.Message("FoodUtilityTest - FoodOptimality - num += FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].stages[0].baseMoodEffect); - 214", true);
                    num += FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].stages[0].baseMoodEffect);
                }
            }
            Log.Message("FoodUtilityTest - FoodOptimality - if (foodDef.ingestible != null) - 215", true);
            if (foodDef.ingestible != null)
            {
                Log.Message("FoodUtilityTest - FoodOptimality - if (eater.RaceProps.Humanlike) - 216", true);
                if (eater.RaceProps.Humanlike)
                {
                    Log.Message("FoodUtilityTest - FoodOptimality - num += foodDef.ingestible.optimalityOffsetHumanlikes; - 217", true);
                    num += foodDef.ingestible.optimalityOffsetHumanlikes;
                }
                else if (eater.RaceProps.Animal)
                {
                    Log.Message("FoodUtilityTest - FoodOptimality - num += foodDef.ingestible.optimalityOffsetFeedingAnimals; - 219", true);
                    num += foodDef.ingestible.optimalityOffsetFeedingAnimals;
                }
            }
            return num;
        }

        private static Thing SpawnedFoodSearchInnerScan(Pawn eater, IntVec3 root, List<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null)
        {
            Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - if (searchSet == null) - 221", true);
            if (searchSet == null)
            {
                Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - return null; - 222", true);
                return null;
            }
            Pawn pawn = traverseParams.pawn ?? eater;
            Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - int num = 0; - 224", true);
            int num = 0;
            Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - int num2 = 0; - 225", true);
            int num2 = 0;
            Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - Thing result = null; - 226", true);
            Thing result = null;
            Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - float num3 = 0f; - 227", true);
            float num3 = 0f;
            Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - float num4 = float.MinValue; - 228", true);
            float num4 = float.MinValue;
            for (int i = 0; i < searchSet.Count; i++)
            {
                Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - Thing thing = searchSet[i]; - 229", true);
                Thing thing = searchSet[i];
                Log.Message("Iterate over: " + thing);
                Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - num2++; - 231", true);
                num2++;
                Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - float num5 = (root - thing.Position).LengthManhattan; - 232", true);
                float num5 = (root - thing.Position).LengthManhattan;
                Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - if (!(num5 > maxDistance)) - 233", true);
                if (!(num5 > maxDistance))
                {
                    Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - num3 = FoodOptimality(eater, thing, GetFinalIngestibleDef(thing), num5); - 234", true);
                    num3 = FoodOptimality(eater, thing, GetFinalIngestibleDef(thing), num5);
                    Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - if (!(num3 < num4) && pawn.Map.reachability.CanReach(root, thing, peMode, traverseParams) && thing.Spawned && (validator == null || validator(thing))) - 235", true);
                    if (!(num3 < num4) && pawn.Map.reachability.CanReach(root, thing, peMode, traverseParams) && thing.Spawned && (validator == null || validator(thing)))
                    {
                        Log.Message("Result: " + thing);
                        Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - result = thing; - 237", true);
                        result = thing;
                        Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - num4 = num3; - 238", true);
                        num4 = num3;
                        Log.Message("FoodUtilityTest - SpawnedFoodSearchInnerScan - num++; - 239", true);
                        num++;
                    }
                }
            }
            return result;
        }

        private static Pawn BestPawnToHuntForPredator(Pawn predator, bool forceScanWholeMap)
        {
            Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - if (predator.meleeVerbs.TryGetMeleeVerb(null) == null) - 241", true);
            if (predator.meleeVerbs.TryGetMeleeVerb(null) == null)
            {
                Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - return null; - 242", true);
                return null;
            }
            bool flag = false;
            Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - if (predator.health.summaryHealth.SummaryHealthPercent < 0.25f) - 244", true);
            if (predator.health.summaryHealth.SummaryHealthPercent < 0.25f)
            {
                Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - flag = true; - 245", true);
                flag = true;
            }
            tmpPredatorCandidates.Clear();
            Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - if (GetMaxRegionsToScan(predator, forceScanWholeMap) < 0) - 247", true);
            if (GetMaxRegionsToScan(predator, forceScanWholeMap) < 0)
            {
                Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - tmpPredatorCandidates.AddRange(predator.Map.mapPawns.AllPawnsSpawned); - 248", true);
                tmpPredatorCandidates.AddRange(predator.Map.mapPawns.AllPawnsSpawned);
            }
            else
            {
                Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - TraverseParms traverseParms = TraverseParms.For(predator); - 249", true);
                TraverseParms traverseParms = TraverseParms.For(predator);
                RegionTraverser.BreadthFirstTraverse(predator.Position, predator.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate (Region x)
                {
                    Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn); - 250", true);
                    List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
                    for (int j = 0; j < list.Count; j++)
                    {
                        Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - tmpPredatorCandidates.Add((Pawn)list[j]); - 251", true);
                        tmpPredatorCandidates.Add((Pawn)list[j]);
                    }
                    return false;
                    Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - }); - 253", true);
                });
            }
            Pawn pawn = null;
            Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - float num = 0f; - 255", true);
            float num = 0f;
            Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - bool tutorialMode = TutorSystem.TutorialMode; - 256", true);
            bool tutorialMode = TutorSystem.TutorialMode;
            for (int i = 0; i < tmpPredatorCandidates.Count; i++)
            {
                Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - Pawn pawn2 = tmpPredatorCandidates[i]; - 257", true);
                Pawn pawn2 = tmpPredatorCandidates[i];
                Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - if (predator.GetRoom() == pawn2.GetRoom() && predator != pawn2 && (!flag || pawn2.Downed) && IsAcceptablePreyFor(predator, pawn2) && predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly) && !pawn2.IsForbidden(predator) && (!tutorialMode || pawn2.Faction != Faction.OfPlayer)) - 258", true);
                if (predator.GetRoom() == pawn2.GetRoom() && predator != pawn2 && (!flag || pawn2.Downed) && IsAcceptablePreyFor(predator, pawn2) && predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly) && !pawn2.IsForbidden(predator) && (!tutorialMode || pawn2.Faction != Faction.OfPlayer))
                {
                    Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - float preyScoreFor = GetPreyScoreFor(predator, pawn2); - 259", true);
                    float preyScoreFor = GetPreyScoreFor(predator, pawn2);
                    Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - if (preyScoreFor > num || pawn == null) - 260", true);
                    if (preyScoreFor > num || pawn == null)
                    {
                        Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - num = preyScoreFor; - 261", true);
                        num = preyScoreFor;
                        Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - pawn = pawn2; - 262", true);
                        pawn = pawn2;
                    }
                }
            }
            tmpPredatorCandidates.Clear();
            Log.Message("FoodUtilityTest - BestPawnToHuntForPredator - return pawn; - 264", true);
            return pawn;
        }

        public static bool IsAcceptablePreyFor(Pawn predator, Pawn prey)
        {
            Log.Message("FoodUtilityTest - IsAcceptablePreyFor - if (!prey.RaceProps.canBePredatorPrey) - 265", true);
            if (!prey.RaceProps.canBePredatorPrey)
            {
                Log.Message("FoodUtilityTest - IsAcceptablePreyFor - return false; - 266", true);
                return false;
            }
            Log.Message("FoodUtilityTest - IsAcceptablePreyFor - if (!prey.RaceProps.IsFlesh) - 267", true);
            if (!prey.RaceProps.IsFlesh)
            {
                Log.Message("FoodUtilityTest - IsAcceptablePreyFor - return false; - 268", true);
                return false;
            }
            Log.Message("FoodUtilityTest - IsAcceptablePreyFor - if (!Find.Storyteller.difficultyValues.predatorsHuntHumanlikes && prey.RaceProps.Humanlike) - 269", true);
            if (!Find.Storyteller.difficultyValues.predatorsHuntHumanlikes && prey.RaceProps.Humanlike)
            {
                Log.Message("FoodUtilityTest - IsAcceptablePreyFor - return false; - 270", true);
                return false;
            }
            Log.Message("FoodUtilityTest - IsAcceptablePreyFor - if (prey.BodySize > predator.RaceProps.maxPreyBodySize) - 271", true);
            if (prey.BodySize > predator.RaceProps.maxPreyBodySize)
            {
                Log.Message("FoodUtilityTest - IsAcceptablePreyFor - return false; - 272", true);
                return false;
            }
            Log.Message("FoodUtilityTest - IsAcceptablePreyFor - if (!prey.Downed) - 273", true);
            if (!prey.Downed)
            {
                Log.Message("FoodUtilityTest - IsAcceptablePreyFor - if (prey.kindDef.combatPower > 2f * predator.kindDef.combatPower) - 274", true);
                if (prey.kindDef.combatPower > 2f * predator.kindDef.combatPower)
                {
                    Log.Message("FoodUtilityTest - IsAcceptablePreyFor - return false; - 275", true);
                    return false;
                }
                float num = prey.kindDef.combatPower * prey.health.summaryHealth.SummaryHealthPercent * prey.ageTracker.CurLifeStage.bodySizeFactor;
                Log.Message("FoodUtilityTest - IsAcceptablePreyFor - float num2 = predator.kindDef.combatPower * predator.health.summaryHealth.SummaryHealthPercent * predator.ageTracker.CurLifeStage.bodySizeFactor; - 277", true);
                float num2 = predator.kindDef.combatPower * predator.health.summaryHealth.SummaryHealthPercent * predator.ageTracker.CurLifeStage.bodySizeFactor;
                Log.Message("FoodUtilityTest - IsAcceptablePreyFor - if (num >= num2) - 278", true);
                if (num >= num2)
                {
                    Log.Message("FoodUtilityTest - IsAcceptablePreyFor - return false; - 279", true);
                    return false;
                }
            }
            Log.Message("FoodUtilityTest - IsAcceptablePreyFor - if (predator.Faction != null && prey.Faction != null && !predator.HostileTo(prey)) - 280", true);
            if (predator.Faction != null && prey.Faction != null && !predator.HostileTo(prey))
            {
                Log.Message("FoodUtilityTest - IsAcceptablePreyFor - return false; - 281", true);
                return false;
            }
            Log.Message("FoodUtilityTest - IsAcceptablePreyFor - if (predator.Faction != null && prey.HostFaction != null && !predator.HostileTo(prey)) - 282", true);
            if (predator.Faction != null && prey.HostFaction != null && !predator.HostileTo(prey))
            {
                Log.Message("FoodUtilityTest - IsAcceptablePreyFor - return false; - 283", true);
                return false;
            }
            Log.Message("FoodUtilityTest - IsAcceptablePreyFor - if (predator.Faction == Faction.OfPlayer && prey.Faction == Faction.OfPlayer) - 284", true);
            if (predator.Faction == Faction.OfPlayer && prey.Faction == Faction.OfPlayer)
            {
                Log.Message("FoodUtilityTest - IsAcceptablePreyFor - return false; - 285", true);
                return false;
            }
            Log.Message("FoodUtilityTest - IsAcceptablePreyFor - if (predator.RaceProps.herdAnimal && predator.def == prey.def) - 286", true);
            if (predator.RaceProps.herdAnimal && predator.def == prey.def)
            {
                Log.Message("FoodUtilityTest - IsAcceptablePreyFor - return false; - 287", true);
                return false;
            }
            return true;
        }

        public static float GetPreyScoreFor(Pawn predator, Pawn prey)
        {
            Log.Message("FoodUtilityTest - GetPreyScoreFor - float num = prey.kindDef.combatPower / predator.kindDef.combatPower; - 289", true);
            float num = prey.kindDef.combatPower / predator.kindDef.combatPower;
            Log.Message("FoodUtilityTest - GetPreyScoreFor - float num2 = prey.health.summaryHealth.SummaryHealthPercent; - 290", true);
            float num2 = prey.health.summaryHealth.SummaryHealthPercent;
            Log.Message("FoodUtilityTest - GetPreyScoreFor - float bodySizeFactor = prey.ageTracker.CurLifeStage.bodySizeFactor; - 291", true);
            float bodySizeFactor = prey.ageTracker.CurLifeStage.bodySizeFactor;
            Log.Message("FoodUtilityTest - GetPreyScoreFor - float lengthHorizontal = (predator.Position - prey.Position).LengthHorizontal; - 292", true);
            float lengthHorizontal = (predator.Position - prey.Position).LengthHorizontal;
            Log.Message("FoodUtilityTest - GetPreyScoreFor - if (prey.Downed) - 293", true);
            if (prey.Downed)
            {
                Log.Message("FoodUtilityTest - GetPreyScoreFor - num2 = Mathf.Min(num2, 0.2f); - 294", true);
                num2 = Mathf.Min(num2, 0.2f);
            }
            float num3 = 0f - lengthHorizontal - 56f * num2 * num2 * num * bodySizeFactor;
            Log.Message("FoodUtilityTest - GetPreyScoreFor - if (prey.RaceProps.Humanlike) - 296", true);
            if (prey.RaceProps.Humanlike)
            {
                Log.Message("FoodUtilityTest - GetPreyScoreFor - num3 -= 35f; - 297", true);
                num3 -= 35f;
            }
            return num3;
        }

        public static void DebugDrawPredatorFoodSource()
        {
            Log.Message("FoodUtilityTest - DebugDrawPredatorFoodSource - Pawn pawn = Find.Selector.SingleSelectedThing as Pawn; - 299", true);
            Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
            Log.Message("FoodUtilityTest - DebugDrawPredatorFoodSource - if (pawn == null || !TryFindBestFoodSourceFor(pawn, pawn, desperate: true, out Thing foodSource, out ThingDef _, canRefillDispenser: false, canUseInventory: false)) - 300", true);
            if (pawn == null || !TryFindBestFoodSourceFor(pawn, pawn, desperate: true, out Thing foodSource, out ThingDef _, canRefillDispenser: false, canUseInventory: false))
            {
                Log.Message("FoodUtilityTest - DebugDrawPredatorFoodSource - return; - 301", true);
                return;
            }
            GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), foodSource.Position.ToVector3Shifted());
            Log.Message("FoodUtilityTest - DebugDrawPredatorFoodSource - if (!(foodSource is Pawn)) - 303", true);
            if (!(foodSource is Pawn))
            {
                Log.Message("FoodUtilityTest - DebugDrawPredatorFoodSource - Pawn pawn2 = BestPawnToHuntForPredator(pawn, forceScanWholeMap: true); - 304", true);
                Pawn pawn2 = BestPawnToHuntForPredator(pawn, forceScanWholeMap: true);
                Log.Message("FoodUtilityTest - DebugDrawPredatorFoodSource - if (pawn2 != null) - 305", true);
                if (pawn2 != null)
                {
                    Log.Message("FoodUtilityTest - DebugDrawPredatorFoodSource - GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), pawn2.Position.ToVector3Shifted()); - 306", true);
                    GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), pawn2.Position.ToVector3Shifted());
                }
            }
        }

        public static List<ThoughtDef> ThoughtsFromIngesting(Pawn ingester, Thing foodSource, ThingDef foodDef)
        {
            Log.Message("FoodUtilityTest - ThoughtsFromIngesting - ingestThoughts.Clear(); - 307", true);
            ingestThoughts.Clear();
            Log.Message("FoodUtilityTest - ThoughtsFromIngesting - if (ingester.needs == null || ingester.needs.mood == null) - 308", true);
            if (ingester.needs == null || ingester.needs.mood == null)
            {
                Log.Message("FoodUtilityTest - ThoughtsFromIngesting - return ingestThoughts; - 309", true);
                return ingestThoughts;
            }
            Log.Message("FoodUtilityTest - ThoughtsFromIngesting - if (!ingester.story.traits.HasTrait(TraitDefOf.Ascetic) && foodDef.ingestible.tasteThought != null) - 310", true);
            if (!ingester.story.traits.HasTrait(TraitDefOf.Ascetic) && foodDef.ingestible.tasteThought != null)
            {
                Log.Message("FoodUtilityTest - ThoughtsFromIngesting - ingestThoughts.Add(foodDef.ingestible.tasteThought); - 311", true);
                ingestThoughts.Add(foodDef.ingestible.tasteThought);
            }
            CompIngredients compIngredients = foodSource.TryGetComp<CompIngredients>();
            Log.Message("FoodUtilityTest - ThoughtsFromIngesting - Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser; - 313", true);
            Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
            Log.Message("FoodUtilityTest - ThoughtsFromIngesting - if (IsHumanlikeMeat(foodDef) && ingester.RaceProps.Humanlike) - 314", true);
            if (IsHumanlikeMeat(foodDef) && ingester.RaceProps.Humanlike)
            {
                Log.Message("FoodUtilityTest - ThoughtsFromIngesting - ingestThoughts.Add(ingester.story.traits.HasTrait(TraitDefOf.Cannibal) ? ThoughtDefOf.AteHumanlikeMeatDirectCannibal : ThoughtDefOf.AteHumanlikeMeatDirect); - 315", true);
                ingestThoughts.Add(ingester.story.traits.HasTrait(TraitDefOf.Cannibal) ? ThoughtDefOf.AteHumanlikeMeatDirectCannibal : ThoughtDefOf.AteHumanlikeMeatDirect);
            }
            else if (compIngredients != null)
            {
                for (int i = 0; i < compIngredients.ingredients.Count; i++)
                {
                    Log.Message("FoodUtilityTest - ThoughtsFromIngesting - AddIngestThoughtsFromIngredient(compIngredients.ingredients[i], ingester, ingestThoughts); - 317", true);
                    AddIngestThoughtsFromIngredient(compIngredients.ingredients[i], ingester, ingestThoughts);
                }
            }
            else if (building_NutrientPasteDispenser != null)
            {
                Log.Message("FoodUtilityTest - ThoughtsFromIngesting - Thing thing = building_NutrientPasteDispenser.FindFeedInAnyHopper(); - 319", true);
                Thing thing = building_NutrientPasteDispenser.FindFeedInAnyHopper();
                Log.Message("FoodUtilityTest - ThoughtsFromIngesting - if (thing != null) - 320", true);
                if (thing != null)
                {
                    Log.Message("FoodUtilityTest - ThoughtsFromIngesting - AddIngestThoughtsFromIngredient(thing.def, ingester, ingestThoughts); - 321", true);
                    AddIngestThoughtsFromIngredient(thing.def, ingester, ingestThoughts);
                }
            }
            Log.Message("FoodUtilityTest - ThoughtsFromIngesting - if (foodDef.ingestible.specialThoughtDirect != null) - 322", true);
            if (foodDef.ingestible.specialThoughtDirect != null)
            {
                Log.Message("FoodUtilityTest - ThoughtsFromIngesting - ingestThoughts.Add(foodDef.ingestible.specialThoughtDirect); - 323", true);
                ingestThoughts.Add(foodDef.ingestible.specialThoughtDirect);
            }
            Log.Message("FoodUtilityTest - ThoughtsFromIngesting - if (foodSource.IsNotFresh()) - 324", true);
            if (foodSource.IsNotFresh())
            {
                Log.Message("FoodUtilityTest - ThoughtsFromIngesting - ingestThoughts.Add(ThoughtDefOf.AteRottenFood); - 325", true);
                ingestThoughts.Add(ThoughtDefOf.AteRottenFood);
            }
            Log.Message("FoodUtilityTest - ThoughtsFromIngesting - if (ModsConfig.RoyaltyActive && InappropriateForTitle(foodDef, ingester, allowIfStarving: false)) - 326", true);
            if (ModsConfig.RoyaltyActive && InappropriateForTitle(foodDef, ingester, allowIfStarving: false))
            {
                Log.Message("FoodUtilityTest - ThoughtsFromIngesting - ingestThoughts.Add(ThoughtDefOf.AteFoodInappropriateForTitle); - 327", true);
                ingestThoughts.Add(ThoughtDefOf.AteFoodInappropriateForTitle);
            }
            return ingestThoughts;
        }

        private static void AddIngestThoughtsFromIngredient(ThingDef ingredient, Pawn ingester, List<ThoughtDef> ingestThoughts)
        {
            Log.Message("FoodUtilityTest - AddIngestThoughtsFromIngredient - if (ingredient.ingestible != null) - 329", true);
            if (ingredient.ingestible != null)
            {
                Log.Message("FoodUtilityTest - AddIngestThoughtsFromIngredient - if (ingester.RaceProps.Humanlike && IsHumanlikeMeat(ingredient)) - 330", true);
                if (ingester.RaceProps.Humanlike && IsHumanlikeMeat(ingredient))
                {
                    Log.Message("FoodUtilityTest - AddIngestThoughtsFromIngredient - ingestThoughts.Add(ingester.story.traits.HasTrait(TraitDefOf.Cannibal) ? ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal : ThoughtDefOf.AteHumanlikeMeatAsIngredient); - 331", true);
                    ingestThoughts.Add(ingester.story.traits.HasTrait(TraitDefOf.Cannibal) ? ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal : ThoughtDefOf.AteHumanlikeMeatAsIngredient);
                }
                else if (ingredient.ingestible.specialThoughtAsIngredient != null)
                {
                    Log.Message("FoodUtilityTest - AddIngestThoughtsFromIngredient - ingestThoughts.Add(ingredient.ingestible.specialThoughtAsIngredient); - 333", true);
                    ingestThoughts.Add(ingredient.ingestible.specialThoughtAsIngredient);
                }
            }
        }

        public static bool IsHumanlikeMeat(ThingDef def)
        {
            Log.Message("FoodUtilityTest - IsHumanlikeMeat - if (def.ingestible.sourceDef != null && def.ingestible.sourceDef.race != null && def.ingestible.sourceDef.race.Humanlike) - 334", true);
            if (def.ingestible.sourceDef != null && def.ingestible.sourceDef.race != null && def.ingestible.sourceDef.race.Humanlike)
            {
                Log.Message("FoodUtilityTest - IsHumanlikeMeat - return true; - 335", true);
                return true;
            }
            return false;
        }

        public static bool IsHumanlikeMeatOrHumanlikeCorpse(Thing thing)
        {
            Log.Message("FoodUtilityTest - IsHumanlikeMeatOrHumanlikeCorpse - if (IsHumanlikeMeat(thing.def)) - 337", true);
            if (IsHumanlikeMeat(thing.def))
            {
                Log.Message("FoodUtilityTest - IsHumanlikeMeatOrHumanlikeCorpse - return true; - 338", true);
                return true;
            }
            Corpse corpse = thing as Corpse;
            Log.Message("FoodUtilityTest - IsHumanlikeMeatOrHumanlikeCorpse - if (corpse != null && corpse.InnerPawn.RaceProps.Humanlike) - 340", true);
            if (corpse != null && corpse.InnerPawn.RaceProps.Humanlike)
            {
                Log.Message("FoodUtilityTest - IsHumanlikeMeatOrHumanlikeCorpse - return true; - 341", true);
                return true;
            }
            return false;
        }

        public static int WillIngestStackCountOf(Pawn ingester, ThingDef def, float singleFoodNutrition)
        {
            Log.Message("FoodUtilityTest - WillIngestStackCountOf - int num = Mathf.Min(def.ingestible.maxNumToIngestAtOnce, StackCountForNutrition(ingester.needs.food.NutritionWanted, singleFoodNutrition)); - 343", true);
            int num = Mathf.Min(def.ingestible.maxNumToIngestAtOnce, StackCountForNutrition(ingester.needs.food.NutritionWanted, singleFoodNutrition));
            Log.Message("FoodUtilityTest - WillIngestStackCountOf - if (num < 1) - 344", true);
            if (num < 1)
            {
                Log.Message("FoodUtilityTest - WillIngestStackCountOf - num = 1; - 345", true);
                num = 1;
            }
            return num;
        }

        public static float GetBodyPartNutrition(Corpse corpse, BodyPartRecord part)
        {
            Log.Message("FoodUtilityTest - GetBodyPartNutrition - return GetBodyPartNutrition(corpse.GetStatValue(StatDefOf.Nutrition), corpse.InnerPawn, part); - 347", true);
            return GetBodyPartNutrition(corpse.GetStatValue(StatDefOf.Nutrition), corpse.InnerPawn, part);
        }

        public static float GetBodyPartNutrition(float currentCorpseNutrition, Pawn pawn, BodyPartRecord part)
        {
            Log.Message("FoodUtilityTest - GetBodyPartNutrition - HediffSet hediffSet = pawn.health.hediffSet; - 348", true);
            HediffSet hediffSet = pawn.health.hediffSet;
            Log.Message("FoodUtilityTest - GetBodyPartNutrition - float coverageOfNotMissingNaturalParts = hediffSet.GetCoverageOfNotMissingNaturalParts(pawn.RaceProps.body.corePart); - 349", true);
            float coverageOfNotMissingNaturalParts = hediffSet.GetCoverageOfNotMissingNaturalParts(pawn.RaceProps.body.corePart);
            Log.Message("FoodUtilityTest - GetBodyPartNutrition - if (coverageOfNotMissingNaturalParts <= 0f) - 350", true);
            if (coverageOfNotMissingNaturalParts <= 0f)
            {
                Log.Message("FoodUtilityTest - GetBodyPartNutrition - return 0f; - 351", true);
                return 0f;
            }
            float num = hediffSet.GetCoverageOfNotMissingNaturalParts(part) / coverageOfNotMissingNaturalParts;
            Log.Message("FoodUtilityTest - GetBodyPartNutrition - return currentCorpseNutrition * num; - 353", true);
            return currentCorpseNutrition * num;
        }

        public static int StackCountForNutrition(float wantedNutrition, float singleFoodNutrition)
        {
            Log.Message("FoodUtilityTest - StackCountForNutrition - if (wantedNutrition <= 0.0001f) - 354", true);
            if (wantedNutrition <= 0.0001f)
            {
                Log.Message("FoodUtilityTest - StackCountForNutrition - return 0; - 355", true);
                return 0;
            }
            return Mathf.Max(Mathf.RoundToInt(wantedNutrition / singleFoodNutrition), 1);
        }

        public static bool ShouldBeFedBySomeone(Pawn pawn)
        {
            Log.Message("FoodUtilityTest - ShouldBeFedBySomeone - if (!FeedPatientUtility.ShouldBeFed(pawn)) - 357", true);
            if (!FeedPatientUtility.ShouldBeFed(pawn))
            {
                Log.Message("FoodUtilityTest - ShouldBeFedBySomeone - return WardenFeedUtility.ShouldBeFed(pawn); - 358", true);
                return WardenFeedUtility.ShouldBeFed(pawn);
            }
            return true;
        }

        public static void AddFoodPoisoningHediff(Pawn pawn, Thing ingestible, FoodPoisonCause cause)
        {
            Log.Message("FoodUtilityTest - AddFoodPoisoningHediff - Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FoodPoisoning); - 360", true);
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FoodPoisoning);
            Log.Message("FoodUtilityTest - AddFoodPoisoningHediff - if (firstHediffOfDef != null) - 361", true);
            if (firstHediffOfDef != null)
            {
                Log.Message("FoodUtilityTest - AddFoodPoisoningHediff - if (firstHediffOfDef.CurStageIndex != 2) - 362", true);
                if (firstHediffOfDef.CurStageIndex != 2)
                {
                    Log.Message("FoodUtilityTest - AddFoodPoisoningHediff - firstHediffOfDef.Severity = HediffDefOf.FoodPoisoning.stages[2].minSeverity - 0.001f; - 363", true);
                    firstHediffOfDef.Severity = HediffDefOf.FoodPoisoning.stages[2].minSeverity - 0.001f;
                }
            }
            else
            {
                Log.Message("FoodUtilityTest - AddFoodPoisoningHediff - pawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.FoodPoisoning, pawn)); - 364", true);
                pawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.FoodPoisoning, pawn));
            }
            Log.Message("FoodUtilityTest - AddFoodPoisoningHediff - if (PawnUtility.ShouldSendNotificationAbout(pawn) && MessagesRepeatAvoider.MessageShowAllowed(\"MessageFoodPoisoning-\" + pawn.thingIDNumber, 0.1f)) - 365", true);
            if (PawnUtility.ShouldSendNotificationAbout(pawn) && MessagesRepeatAvoider.MessageShowAllowed("MessageFoodPoisoning-" + pawn.thingIDNumber, 0.1f))
            {
                Log.Message("FoodUtilityTest - AddFoodPoisoningHediff - Messages.Message(\"MessageFoodPoisoning\".Translate(pawn.LabelShort, ingestible.LabelCapNoCount, cause.ToStringHuman().CapitalizeFirst(), pawn.Named(\"PAWN\"), ingestible.Named(\"FOOD\")).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeEvent); - 366", true);
                Messages.Message("MessageFoodPoisoning".Translate(pawn.LabelShort, ingestible.LabelCapNoCount, cause.ToStringHuman().CapitalizeFirst(), pawn.Named("PAWN"), ingestible.Named("FOOD")).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeEvent);
            }
        }

        public static float GetFoodPoisonChanceFactor(Pawn ingester)
        {
            Log.Message("FoodUtilityTest - GetFoodPoisonChanceFactor - float num = Find.Storyteller.difficultyValues.foodPoisonChanceFactor; - 367", true);
            float num = Find.Storyteller.difficultyValues.foodPoisonChanceFactor;
            Log.Message("FoodUtilityTest - GetFoodPoisonChanceFactor - if (ingester.health != null && ingester.health.hediffSet != null) - 368", true);
            if (ingester.health != null && ingester.health.hediffSet != null)
            {
                Log.Message("FoodUtilityTest - GetFoodPoisonChanceFactor - foreach (Hediff hediff in ingester.health.hediffSet.hediffs) - 369", true);
                foreach (Hediff hediff in ingester.health.hediffSet.hediffs)
                {
                    Log.Message("FoodUtilityTest - GetFoodPoisonChanceFactor - HediffStage curStage = hediff.CurStage; - 370", true);
                    HediffStage curStage = hediff.CurStage;
                    Log.Message("FoodUtilityTest - GetFoodPoisonChanceFactor - if (curStage != null) - 371", true);
                    if (curStage != null)
                    {
                        Log.Message("FoodUtilityTest - GetFoodPoisonChanceFactor - num *= curStage.foodPoisoningChanceFactor; - 372", true);
                        num *= curStage.foodPoisoningChanceFactor;
                    }
                }
                return num;
            }
            return num;
        }

        public static bool Starving(this Pawn p)
        {
            Log.Message("FoodUtilityTest - Starving - if (p.needs != null && p.needs.food != null) - 375", true);
            if (p.needs != null && p.needs.food != null)
            {
                Log.Message("FoodUtilityTest - Starving - return p.needs.food.Starving; - 376", true);
                return p.needs.food.Starving;
            }
            return false;
        }

        public static float GetNutrition(Thing foodSource, ThingDef foodDef)
        {
            Log.Message("FoodUtilityTest - GetNutrition - if (foodSource == null || foodDef == null) - 378", true);
            if (foodSource == null || foodDef == null)
            {
                Log.Message("FoodUtilityTest - GetNutrition - return 0f; - 379", true);
                return 0f;
            }
            Log.Message("FoodUtilityTest - GetNutrition - if (foodSource.def == foodDef) - 380", true);
            if (foodSource.def == foodDef)
            {
                Log.Message("FoodUtilityTest - GetNutrition - return foodSource.GetStatValue(StatDefOf.Nutrition); - 381", true);
                return foodSource.GetStatValue(StatDefOf.Nutrition);
            }
            return foodDef.GetStatValueAbstract(StatDefOf.Nutrition);
        }

        public static bool WillIngestFromInventoryNow(Pawn pawn, Thing inv)
        {
            Log.Message("FoodUtilityTest - WillIngestFromInventoryNow - if ((inv.def.IsNutritionGivingIngestible || inv.def.IsNonMedicalDrug) && inv.IngestibleNow) - 383", true);
            if ((inv.def.IsNutritionGivingIngestible || inv.def.IsNonMedicalDrug) && inv.IngestibleNow)
            {
                Log.Message("FoodUtilityTest - WillIngestFromInventoryNow - return pawn.WillEat(inv); - 384", true);
                return pawn.WillEat(inv);
            }
            return false;
        }

        public static void IngestFromInventoryNow(Pawn pawn, Thing inv)
        {
            Log.Message("FoodUtilityTest - IngestFromInventoryNow - Job job = JobMaker.MakeJob(JobDefOf.Ingest, inv); - 386", true);
            Job job = JobMaker.MakeJob(JobDefOf.Ingest, inv);
            Log.Message("FoodUtilityTest - IngestFromInventoryNow - job.count = Mathf.Min(inv.stackCount, WillIngestStackCountOf(pawn, inv.def, inv.GetStatValue(StatDefOf.Nutrition))); - 387", true);
            job.count = Mathf.Min(inv.stackCount, WillIngestStackCountOf(pawn, inv.def, inv.GetStatValue(StatDefOf.Nutrition)));
            Log.Message("FoodUtilityTest - IngestFromInventoryNow - pawn.jobs.TryTakeOrderedJob(job); - 388", true);
            pawn.jobs.TryTakeOrderedJob(job);
        }
    }
}