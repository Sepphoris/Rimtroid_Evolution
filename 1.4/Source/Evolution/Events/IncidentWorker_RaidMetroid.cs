using RimWorld;
using RT_Rimtroid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RT_Core
{
	public class IncidentWorker_RaidMetroid : IncidentWorker_RaidEnemy
	{
		protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			if (base.FactionCanBeGroupSource(f, map, desperate) && f.HostileTo(Faction.OfPlayer))
			{
				if (!desperate)
				{
					return (float)GenDate.DaysPassed >= f.def.earliestRaidDays;
				}
				return true;
			}
			return false;
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
                        allowFood,
                        parms.inhabitants,
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
		public List<Pawn> SpawnThreats(IncidentParms parms, RaidOptions options)
		{
			List<Pawn> list = new List<Pawn>();
			int num = 0;
			int attempts = 0;
			while (num < options.minimumPawnCount && attempts < options.minimumPawnCount + 100)
			{
				if (options.pawnGroup.options.TryRandomElementByWeight(x => x.selectionWeight, out var result))
                {
                    PawnGenerationRequest request = new PawnGenerationRequest(
                        result.kind,
                        parms.faction,
                        PawnGenerationContext.NonPlayer,
                        biocodeWeaponChance: parms.biocodeWeaponsChance)
                    {
                        BiocodeApparelChance = 1f
                    };
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
					if (pawn != null)
					{
						list.Add(pawn);
						num++;
					}
				}
				attempts++;
			}
			if (list.Any())
			{
				return list;
			}
			return null;
		}

		public List<Pawn> SpawnThreats2(IncidentParms parms, RaidOptions options)
		{
			List<Pawn> list = new List<Pawn>();
			foreach (var pawnCount in options.minimumPawnCountPerKind.options)
            {
				int num = 0;
				int attempts = 0;
				int minimumPawnCount = (int)pawnCount.selectionWeight;
				while (num < minimumPawnCount && attempts < minimumPawnCount + 100)
				{
                    PawnGenerationRequest request = new PawnGenerationRequest(
                        pawnCount.kind,
                        parms.faction,
                        PawnGenerationContext.NonPlayer,
                        canGeneratePawnRelations: true,
                        mustBeCapableOfViolence: true,
                        biocodeWeaponChance: parms.biocodeWeaponsChance)
                    {
                        BiocodeApparelChance = 1f
                    };
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
					if (pawn != null)
					{
						list.Add(pawn);
						num++;
					}
					attempts++;
				}
			}

			if (list.Any())
			{
				return list;
			}
			return null;
		}

        protected override bool CanFireNowSub(IncidentParms parms)
        {
			var comp = Current.Game.GetComponent<RimtroidEvolutionTracker>();
			if (comp.metroidRaidGracePeriod > Find.TickManager.TicksGame)
            {
				return Rand.Chance(0.4f);
            }
			return base.CanFireNowSub(parms);
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
		{
			ResolveRaidPoints(parms);
			var options = this.def.GetModExtension<RaidOptions>();
			if (options.minimumPlayerTechLevel.HasValue && Faction.OfPlayer.def.techLevel < options.minimumPlayerTechLevel.Value)
            {
				return false;
            }
			Map map = (Map)parms.target;
			if (options.minimumPlayerWealth.HasValue && options.minimumPlayerWealth.Value > map.wealthWatcher.WealthTotal)
            {
				return false;
            }
			if (options.requiredResearchProjectsUnlocked != null && options.requiredResearchProjectsUnlocked.Any(x => !x.IsFinished))
            {
				return false;
            }
			if (options.raidFaction is null)
            {
				if (!TryResolveRaidFaction(parms))
				{
					return false;
				}
			}
			else
            {
				parms.faction = Find.FactionManager.FirstFactionOfDef(options.raidFaction);
            }
			if (parms.faction.leader is null || parms.faction.leader.Dead)
            {
				return false;
            }
			PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
			if (options.raidStrategy is null)
			{
				ResolveRaidStrategy(parms, combat);
			}
			else
			{
				parms.raidStrategy = options.raidStrategy;
			}
			if (options.raidArrival is null)
			{
				ResolveRaidArriveMode(parms);
			}
			else
			{
				parms.raidArrivalMode = options.raidArrival;
			}

			parms.raidStrategy.Worker.TryGenerateThreats(parms);
			if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
			{
				return false;
			}
			float points = parms.points;
			parms.points = AdjustedRaidPoints(parms.points, parms.raidArrivalMode, parms.raidStrategy, parms.faction, combat);
			
			if (options.fixedRaidPoints != -1)
            {
				parms.points = options.fixedRaidPoints;
			}
			if (options.raidPointsMultiplier != -1)
			{
				parms.points *= options.raidPointsMultiplier;
			}

			var list = new List<Pawn>(); 
			if (options.minimumPawnCount != -1)
            {
				list = SpawnThreats(parms, options);
			}
			else if (options.minimumPawnCountPerKind != null)
            {
				list = SpawnThreats2(parms, options);
			}
			if (options.pawnGroup != null)
            {
				GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms), options.pawnGroup, list);
            }

			if (list.Count == 0)
			{
				Log.Error("Got no pawns spawning raid from parms " + parms);
				return false;
			}
			parms.raidArrivalMode.Worker.Arrive(list, parms);
			GenerateRaidLoot(parms, points, list);
			TaggedString letterLabel = GetLetterLabel(parms, options);
			TaggedString letterText = GetLetterText(parms, list, options);
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref letterLabel, ref letterText, GetRelatedPawnsInfoLetterText(parms), informEvenIfSeenBefore: true);
			List<TargetInfo> list2 = new List<TargetInfo>();
			if (parms.pawnGroups != null)
			{
				List<List<Pawn>> list3 = IncidentParmsUtility.SplitIntoGroups(list, parms.pawnGroups);
				List<Pawn> list4 = list3.MaxBy((List<Pawn> x) => x.Count);
				if (list4.Any())
				{
					list2.Add(list4[0]);
				}
				for (int i = 0; i < list3.Count; i++)
				{
					if (list3[i] != list4 && list3[i].Any())
					{
						list2.Add(list3[i][0]);
					}
				}
			}
			else if (list.Any())
			{
				foreach (Pawn item in list)
				{
					list2.Add(item);
				}
			}
			SendStandardLetter(letterLabel, letterText, GetLetterDef(options), parms, list2);
			parms.raidStrategy.Worker.MakeLords(parms, list);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);/*
			if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts))
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].apparel.WornApparel.Any((Apparel ap) => ap is ShieldBelt))
					{
						LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);
						break;
					}
				}
			} Temporarily Disabled while working on other bugs */
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			Find.StoryWatcher.statsRecord.numRaidsEnemy++;
			return true;
		}

		protected override bool TryResolveRaidFaction(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (parms.faction != null)
			{
				return true;
			}
			float num = parms.points;
			if (num <= 0f)
			{
				num = 999999f;
			}
			if (PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(num, out parms.faction, (Faction f) => FactionCanBeGroupSource(f, map), allowNonHostileToPlayer: true, allowHidden: true, allowDefeated: true))
			{
				return true;
			}
			if (PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(num, out parms.faction, (Faction f) => FactionCanBeGroupSource(f, map, desperate: true), allowNonHostileToPlayer: true, allowHidden: true, allowDefeated: true))
			{
				return true;
			}
			return false;
		}

		protected override void ResolveRaidPoints(IncidentParms parms)
		{
			if (parms.points <= 0f)
			{
				Log.Error("RaidEnemy is resolving raid points. They should always be set before initiating the incident.");
				parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
			}
		}

		public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
		{
			if (parms.raidStrategy == null)
			{
				Map map = (Map)parms.target;
				DefDatabase<RaidStrategyDef>.AllDefs.Where((RaidStrategyDef d) => d.Worker.CanUseWith(parms, groupKind) && (parms.raidArrivalMode != null || (d.arriveModes != null && d.arriveModes.Any((PawnsArrivalModeDef x) => x.Worker.CanUseWith(parms))))).TryRandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionWeight(map, parms.points), out RaidStrategyDef result);
				parms.raidStrategy = result;
				if (parms.raidStrategy == null)
				{
					Log.Error(string.Concat("No raid stategy found, defaulting to ImmediateAttack. Faction=", parms.faction.def.defName, ", points=", parms.points, ", groupKind=", groupKind, ", parms=", parms));
					parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
				}
			}
		}

		protected string GetLetterLabel(IncidentParms parms, RaidOptions options)
		{
			if (!options.letterTitle.NullOrEmpty())
            {
				return options.letterTitle;
			}
			return parms.raidStrategy.letterLabelEnemy + ": " + parms.faction.Name;
		}

		protected string GetLetterText(IncidentParms parms, List<Pawn> pawns, RaidOptions options)
		{
			if (!options.letterText.NullOrEmpty())
			{
				return options.letterText;
			}
			string str = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name.ApplyTag(parms.faction)).CapitalizeFirst();
			str += "\n\n";
			str += parms.raidStrategy.arrivalTextEnemy;
			Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
			if (pawn != null)
			{
				str += "\n\n";
				str += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
			}
			return str;
		}

		protected LetterDef GetLetterDef(RaidOptions options)
		{
			if (options.letterDef != null)
            {
				return options.letterDef;
            }
			return LetterDefOf.ThreatBig;
		}

		protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
		{
			return "LetterRelatedPawnsRaidEnemy".Translate(Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
		}
	}
}
