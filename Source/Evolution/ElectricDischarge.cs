using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using RT_Core;

namespace RT_Rimtroid
{
	class ElectricDischarge : AttachableThing, ISizeReporter
	{
		private int ticksSinceSpawn;

		public float ElectricSize = 0.1f;

		private Sustainer sustainer;

		private static int ElectricCount;

		private static int lastElectricCountUpdateTick;

		private const int ComplexCalcsInterval = 150;

		public override string Label
		{
			get
			{
				if (this.parent != null)
				{
					return "Electric".Translate(this.parent.LabelCap, this.parent);
				}
				return this.def.label;
			}
		}

		public float CurrentSize()
		{
			return this.ElectricSize;
		}

		public override string InspectStringAddon
		{
			get
			{
				return "Burning".Translate() + " (" + "ElectricSizeLower".Translate((this.ElectricSize * 100f).ToString("F0")) + ")";
			}
		}

		private float SpreadInterval
		{
			get
			{
				float num = 150f - (this.ElectricSize - 1f) * 40f;
				if (num < 75f)
				{
					num = 75f;
				}
				return num;
			}
		}



		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			if (this.sustainer != null)
			{
				if (this.sustainer.externalParams.sizeAggregator == null)
				{
					this.sustainer.externalParams.sizeAggregator = new SoundSizeAggregator();
				}
				this.sustainer.externalParams.sizeAggregator.RemoveReporter(this);
			}
			Map map = base.Map;
			base.DeSpawn(mode);
			RecalcPathsOnAndAroundMe(map);
			//Log.Message("base.DeSpawn (ElectricDischarge)", true);
		}

		private void RecalcPathsOnAndAroundMe(Map map)
		{
			IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
			for (int i = 0; i < adjacentCellsAndInside.Length; i++)
			{
				IntVec3 c = base.Position + adjacentCellsAndInside[i];
				if (c.InBounds(map))
				{
					map.pathGrid.RecalculatePerceivedPathCostAt(c);
				}
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			RecalcPathsOnAndAroundMe(map);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.HomeArea, this, OpportunityType.Important);
		}

        public override void Draw()
        {
			//RimtroidMoteMaker.MakeElectricFloor(this.DrawPos, this.Map);
		}

        public override void Tick()
		{
				if (Rand.Chance(1f) && Find.TickManager.TicksGame
				% Rand.RangeInclusive(60, 100) == 0)
				{ 
				RimtroidMoteMaker.MakeElectricFloor(this.Position.ToVector3Shifted(), this.Map);
				}

			if (Find.TickManager.TicksGame % 30 == 0)
			{
				//Log.Message(" Beginning public override void Tick ()", true);
				this.ticksSinceSpawn++;
				if (ElectricDischarge.lastElectricCountUpdateTick != Find.TickManager.TicksGame)
				{
					ElectricDischarge.ElectricCount = base.Map.listerThings.ThingsOfDef(this.def).Count;
					ElectricDischarge.lastElectricCountUpdateTick = Find.TickManager.TicksGame;
					//Log.Message("Find.TickManager.TicksGame completed", true);
					if (this.sustainer != null)
					{
						if (!sustainer.Ended)
						{
							sustainer.Maintain();
							Log.Message("sustainer.maintain", true);
						}
					}
					//Log.Message(" Attempting to play sound", true);
					SoundInfo info = SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.PerTick);
					this.sustainer = SustainerAggregatorUtility.AggregateOrSpawnSustainerFor(this, RT_DefOf.RT_ElectricBurning, info);
				}
				if (ElectricDischarge.ElectricCount < 15 && this.ElectricSize > 0.7f && Rand.Value < this.ElectricSize * 0.01f)
				{
					MoteMaker.ThrowMicroSparks(this.DrawPos, base.Map);
				}
				{
					List<Pawn> thingsInRange = Position.GetThingList(Map).Where(t => t != this && !t.IsMetroid() && t is Pawn pawn).Cast<Pawn>().ToList();
					for (int i = thingsInRange.Count - 1; i >= 0; i--)
					{
						Pawn pawn = thingsInRange[i];
						Rand.PushState();
						pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, (float)Rand.RangeInclusive(1, 4)));
						bool randomChance = Rand.Chance(0.1f);
						Rand.PopState();
						if (randomChance)
						{
							pawn.stances.stunner.StunFor_NewTmp(120, this);
						}
					}

				}
			}
		}
	}
}
