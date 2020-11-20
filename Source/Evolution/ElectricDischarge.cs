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
		}

		public override void Tick()
		{
			this.ticksSinceSpawn++;
			if (ElectricDischarge.lastElectricCountUpdateTick != Find.TickManager.TicksGame)
			{
				ElectricDischarge.ElectricCount = base.Map.listerThings.ThingsOfDef(this.def).Count;
				ElectricDischarge.lastElectricCountUpdateTick = Find.TickManager.TicksGame;
			}
			if (this.sustainer != null)
			{
				this.sustainer.Maintain();
			}
			else if (!base.Position.Fogged(base.Map))
			{
				SoundInfo info = SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.PerTick);
				this.sustainer = SustainerAggregatorUtility.AggregateOrSpawnSustainerFor(this, SoundDefOf.FireBurning, info);
			}
			if (ElectricDischarge.ElectricCount < 15 && this.ElectricSize > 0.7f && Rand.Value < this.ElectricSize * 0.01f)
			{
				MoteMaker.ThrowMicroSparks(this.DrawPos, base.Map);
			}
		}


		private void DoFireDamage(Thing targ)
		{
			float num = 0.0125f + 0.0036f * this.ElectricSize;
			num = Mathf.Clamp(num, 0.0125f, 0.05f);
			int num2 = GenMath.RoundRandom(num * 150f);
			if (num2 < 1)
			{
				num2 = 1;
			}
			Pawn pawn = targ as Pawn;
			if (pawn != null)
			{
				BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire, null);
				Find.BattleLog.Add(battleLogEntry_DamageTaken);
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Flame, (float)num2, 0f, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
				dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
				targ.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_DamageTaken);
				Apparel apparel;
				if (pawn.apparel != null && pawn.apparel.WornApparel.TryRandomElement(out apparel))
				{
					apparel.TakeDamage(new DamageInfo(DamageDefOf.Flame, (float)num2, 0f, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
				}
			}
			else
			{
				targ.TakeDamage(new DamageInfo(DamageDefOf.Flame, (float)num2, 0f, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
			}
		}
	}
}
