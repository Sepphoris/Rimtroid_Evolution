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
			//Log.Message("base.DeSpawn (ElectricDischarge)", true);
		}

		public override void Tick()
		{
			if (Find.TickManager.TicksGame % 30 == 0)
			{
				//Log.Message(" Beginning public override void Tick ()", true);
				this.ticksSinceSpawn++;
				if (ElectricDischarge.lastElectricCountUpdateTick != Find.TickManager.TicksGame)
				{
					ElectricDischarge.ElectricCount = base.Map.listerThings.ThingsOfDef(this.def).Count;
					ElectricDischarge.lastElectricCountUpdateTick = Find.TickManager.TicksGame;
					//Log.Message("Find.TickManager.TicksGame completed", true);
				}
				//if (this.sustainer != null)
				//{
					//sustainer.Maintain();
					//Log.Message("sustainer.maintain", true);
				//}
				else if (!base.Position.Fogged(base.Map))
				{
					//Log.Message(" Attempting to play sound", true);
					SoundInfo info = SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.PerTick);
					this.sustainer = SustainerAggregatorUtility.AggregateOrSpawnSustainerFor(this, RT_DefOf.RT_ElectricBurning, info);
				}
				if (ElectricDischarge.ElectricCount < 15 && this.ElectricSize > 0.7f && Rand.Value < this.ElectricSize * 0.01f)
				{
					MoteMaker.ThrowMicroSparks(this.DrawPos, base.Map);
				}
				{
					List<Thing> thingsInRange = this.Position.GetThingList(this.Map);
					if (!thingsInRange.NullOrEmpty())
						//Log.Message(" List<Thing> thingsInRange = this.Position.GetThingList", true);
					{
						//Log.Message(" Attempting to apply damage", true);
						foreach (Thing thing in thingsInRange.Where(thing => thing != this).ToList())
						{
							thing.TakeDamage(new DamageInfo(DamageDefOf.Burn, 3f));
							if (thing is Pawn pawn && Rand.Chance(0.08f))
							{
								pawn.stances.stunner.StunFor(120, this);
								//Log.Message(" Applied damage", true);
							}
						}
					}

				}
			}
		}
	}
}
