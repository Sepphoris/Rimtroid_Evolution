using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RT_Rimtroid
{
	public class Command_MetroidBomb : Command_Ability
	{
		public Command_MetroidBomb(Ability ability)
			: base(ability)
		{
		}
		public static TargetingParameters ForLoc(Pawn pawn = null, float? radius = null)
		{
			TargetingParameters targetingParameters = new TargetingParameters();
			targetingParameters.canTargetLocations = true;
			if (radius != null)
				targetingParameters.validator = (TargetInfo target) => pawn.CanReserveAndReach(target.Cell, PathEndMode.OnCell, Danger.Deadly);
			return targetingParameters;
		}
		public override void ProcessInput(Event ev)
		{
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			if (ability.def.targetRequired)
			{
				if (!ability.def.targetWorldCell)
				{
					Find.Targeter.BeginTargeting(ForLoc(ability.pawn), delegate (LocalTargetInfo x)
					{
						var list = GetFloatMenuOptions(ability.pawn, x.Cell).ToList();
						FloatMenu floatMenu = new FloatMenu(list);
						floatMenu.vanishIfMouseDistant = true;
						Find.WindowStack.Add(floatMenu);
					}, null, null);
					return;
				}
			}
			else
			{
				ability.QueueCastingJob(ability.pawn, LocalTargetInfo.Invalid);
			}
		}

		public IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn, IntVec3 cell)
		{
			Action placeProximityBomb = delegate
			{
				pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RT_DefOf.RT_PlaceProximityBomb, cell));
			};
			yield return new FloatMenuOption("RT_PlaceProximityBomb".Translate(), placeProximityBomb);
			Action placeTimedBomb = delegate
			{
				pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RT_DefOf.RT_PlaceTimedBomb, cell));
			};
			yield return new FloatMenuOption("RT_PlaceTimedBomb".Translate(), placeTimedBomb);

			Action placeHostileOnlyBomb = delegate
			{
				pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RT_DefOf.RT_PlaceHostileOnlyBomb, cell));
			};
			yield return new FloatMenuOption("RT_PlaceHostileOnlyBomb".Translate(), placeHostileOnlyBomb);
		}
	}
}
