using System;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;
using RimWorld.QuestGen;
using System.Collections.Generic;
using RimWorld.BaseGen;
using RT_Core;

namespace RT_Rimtroid
{
	public class GenStep_QueenLair : GenStep
	{
		public override int SeedPart => 398638181;

		public override void Generate(Map map, GenStepParams parms)
		{
			if (!MapGenerator.TryGetVar("RectOfInterest", out CellRect var))
			{
				var = CellRect.SingleCell(map.Center);
			}
			if (!MapGenerator.TryGetVar("UsedRects", out List<CellRect> var2))
			{
				var2 = new List<CellRect>();
				MapGenerator.SetVar("UsedRects", var2);
			}
			Faction faction = (map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.RandomEnemyFaction();

			var center = CellFinder.RandomSpawnCellForPawnNear(map.Center, map);
			GenSpawn.Spawn(faction.leader, center, map);
			List<Pawn> guards = new List<Pawn>();
			var raidOptions = this.def.GetModExtension<RaidOptions>();

            PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Combat,
                tile = map.Tile,
                faction = faction,
                points = raidOptions.raidPoints.RandomInRange
            };
            if (parms.sitePart != null)
			{
				pawnGroupMakerParms.seed = SleepingMechanoidsSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms);
			}
			GeneratePawns(pawnGroupMakerParms, raidOptions.pawnGroup, guards);
			foreach (var pawn in guards)
            {
				var cell = CellFinder.RandomSpawnCellForPawnNear(center, map);
				GenSpawn.Spawn(pawn, cell, map);
			}
		}

		public bool CanGenerateFrom(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
		{
			if (!PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, groupMaker.options, parms).Any())
			{
				return false;
			}
			return true;
		}
		protected void GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns)
		{
			if (CanGenerateFrom(parms, groupMaker))
			{
				bool allowFood = parms.raidStrategy == null || parms.raidStrategy.pawnsCanBringFood || (parms.faction != null && !parms.faction.HostileTo(Faction.OfPlayer));
				Predicate<Pawn> validatorPostGear = (parms.raidStrategy != null) ? ((Predicate<Pawn>)((Pawn p) => parms.raidStrategy.Worker.CanUsePawn(parms.points, p, outPawns))) : null;
				bool flag = false;
				foreach (PawnGenOptionWithXenotype item in PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, groupMaker.options, parms))
				{
					Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                        item.Option.kind,
                        parms.faction,
                        PawnGenerationContext.NonPlayer,
                        parms.tile,
                        forceGenerateNewPawn: false,
                        newborn: false,
                        allowDead: false,
                        allowDowned: false,
                        canGeneratePawnRelations: true,
                        mustBeCapableOfViolence: true,
                        1f,
                        forceAddFreeWarmLayerIfNeeded: false,
                        allowGay: true,
                        allowFood,
                        allowAddictions: true,
                        parms.inhabitants,
                        certainlyBeenInCryptosleep: false,
                        forceRedressWorldPawnIfFormerColonist: false,
                        worldPawnFactionDoesntMatter: false,
                        0f,
                        validatorPostGear: validatorPostGear));
					if (parms.forceOneDowned && !flag)
					{
						pawn.health.forceDowned = true;
						pawn.mindState.canFleeIndividual = false;
						flag = true;
					}
					outPawns.Add(pawn);
				}
			}
		}

	}
}