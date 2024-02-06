using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using RT_Rimtroid;

namespace RT_Core
{
	public class JobGiver_GetLiveFood : ThinkNode_JobGiver
	{
		protected HungerCategory minCategory;

		private float maxLevelPercentage = 1f;

		public bool forceScanWholeMap;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GetLiveFood obj = (JobGiver_GetLiveFood)base.DeepCopy(resolve);
			obj.minCategory = minCategory;
			obj.maxLevelPercentage = maxLevelPercentage;
			obj.forceScanWholeMap = forceScanWholeMap;

			return obj;
		}

		public override float GetPriority(Pawn pawn)
		{
			Need_Food food = pawn.needs.food;
			if (food == null)
			{
				return 0f;
			}
			if ((int)pawn.needs.food.CurCategory < 3 && FoodUtility.ShouldBeFedBySomeone(pawn))
			{
				return 0f;
			}
			if ((int)food.CurCategory < (int)minCategory)
			{
				return 0f;
			}
			if (food.CurLevelPercentage > maxLevelPercentage)
			{
				return 0f;
			}
			if (food.CurLevelPercentage < pawn.RaceProps.FoodLevelPercentageWantEat)
			{
				return 9.5f;
			}
			return 0f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{

			Need_Food food = pawn.needs.food;
			if (food == null || (int)food.CurCategory < (int)minCategory || food.CurLevelPercentage > maxLevelPercentage)
			{
				return null;
			}
			Predicate<Thing> stationValidator = delegate (Thing x)
			{
				if (!RT_Utils.feedingStations.Contains(x.def))
                {
					return false;
                }
				var powerComp = x.TryGetComp<CompPowerTrader>();
				if (powerComp != null && !powerComp.PowerOn)
                {
					return false;
                }
				var flickableComp = x.TryGetComp<CompFlickable>();
				if (flickableComp != null && !flickableComp.SwitchIsOn)
                {
					return false;
                }
				var fuelComp = x.TryGetComp<CompRefuelable>();
				if (fuelComp != null && !fuelComp.HasFuel)
                {
					return false;
                }
				if (!x.def.GetModExtension<MetroidFeedingStationOptions>().options.Any(y => y.defName == pawn.def.defName))
                {
					return false;
                }
				return true;
			};
			var feedingStations = pawn.Map.listerThings.AllThings.Where(x => stationValidator(x) && pawn.CanReserveAndReach(x, PathEndMode.InteractionCell, Danger.Deadly))
					.OrderBy(x => x.Position.DistanceTo(pawn.Position));
			if (feedingStations.Any())
			{
				Job job = JobMaker.MakeJob(RT_RimtroidDefOf.RT_EatFromStation, feedingStations.First());
				Log.Message(pawn + " gets " + job);
				return job;
			}

			var options = pawn.def.GetModExtension<RT_EnergyDrain>();
			if (options != null)
            {
				var freshCorpse = FoodMethod.FindTarget(pawn, 250f, (Thing x) => x is Corpse corpse && !RT_Utils.blackListRaces.Contains(corpse.InnerPawn.def) 
					&& corpse.GetRotStage() == RotStage.Fresh && corpse.Age < GenDate.TicksPerDay * 3 && pawn.CanReserve(x), ThingRequestGroup.Corpse); 
				if (freshCorpse != null)
                {
					Job job = JobMaker.MakeJob(RT_RimtroidDefOf.RT_AbsorbingEnergy, freshCorpse);
					var hediff = HediffMaker.MakeHediff(RT_DefOf.RT_MetroidHunting, pawn);
					pawn.health.AddHediff(hediff);
					return job;
                }
				var prisoner = FoodMethod.FindTarget(pawn, 250f, (Thing x) => x is Pawn victim && victim.IsPrisoner && !victim.Downed && victim.GetComp<CompPrisonerFeed>().canBeEaten && pawn.CanReserve(x), 
					ThingRequestGroup.Pawn);
				if (prisoner != null)
                {
					Job job = JobMaker.MakeJob(RT_RimtroidDefOf.RT_AbsorbingEnergy, prisoner);
					var hediff = HediffMaker.MakeHediff(RT_DefOf.RT_MetroidHunting, pawn);
					pawn.health.AddHediff(hediff);
					return job;
				}
				var wildAnimal = FoodMethod.FindTarget(pawn, 250f, (Thing x) => x is Pawn victim && (pawn.Faction is null && !victim.IsAnyMetroid() || pawn.Faction == Faction.OfPlayer) && !RT_Utils.blackListRaces.Contains(victim.def) && victim.RaceProps.Animal && victim.Faction != pawn.Faction && victim.BodySize <= 4f 
				&& pawn.CanReserve(x), ThingRequestGroup.Pawn);
				if (wildAnimal != null)
                {
					Job job = JobMaker.MakeJob(RT_RimtroidDefOf.RT_AbsorbingEnergy, wildAnimal);
					var hediff = HediffMaker.MakeHediff(RT_DefOf.RT_MetroidHunting, pawn);
					pawn.health.AddHediff(hediff);
					return job;
				}
			}
			Thing foodSource = FoodMethod.FindPawnTarget(pawn, 250f);
			Pawn pawn2 = foodSource as Pawn;
			if (pawn2 != null)
			{
				Log.Message("Metroid is hunting.");
				Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, pawn2);
				job.killIncappedTarget = false;
				return job;
			}
			return null;
		}
	}
}