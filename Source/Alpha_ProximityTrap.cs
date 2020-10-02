using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RT_Rimtroid
{
    public class Alpha_ProximityTrap : Building
    {
		protected void SpringSub(Pawn p)
		{
			Log.Message(p + " - SpringSub", true);
			base.GetComp<CompExplosive>().StartWick(null);
		}
		private int tickCount;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

		private List<Pawn> touchingPawns = new List<Pawn>();

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref tickCount, "tickCount");
        }

        public override bool ClaimableBy(Faction by)
        {
			return false;
        }

        public override bool DeconstructibleBy(Faction faction)
        {
			return false;
        }
        public override void Tick()
		{
			tickCount++;
			if (base.Spawned)
			{
				var options = this.def.GetModExtension<Alpha_ProximityTrapProperties>();
				if (options != null)
                {
					if (tickCount >= options.explosionTimeout)
                    {
						this.SpringSub(null);
						tickCount = 0;
					}
					else
                    {
						foreach (var cell in GenRadial.RadialCellsAround(this.Position, options.proximityRange, true))
						{
							List<Thing> thingList = cell.GetThingList(base.Map);
							for (int i = 0; i < thingList.Count; i++)
							{
								Pawn pawn = thingList[i] as Pawn;
								if (pawn != null && pawn.def != RT_DefOf.RT_AlphaMetroid && !touchingPawns.Contains(pawn))
								{
									touchingPawns.Add(pawn);
									CheckSpring(pawn);
								}
							}
						}

						for (int j = 0; j < this.touchingPawns.Count; j++)
						{
							Pawn pawn = this.touchingPawns[j];
							if (!pawn.Spawned || Find.TickManager.TicksGame % 60 == 0)
							{
								this.touchingPawns.Remove(pawn);
							}
						}
					}
				}
			}
			base.Tick();
		}

		private void CheckSpring(Pawn p)
		{
			if (Rand.Chance(SpringChance(p)))
			{
				Map map = base.Map;
				Spring(p);
				if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
				{
					Find.LetterStack.ReceiveLetter("LetterFriendlyTrapSprungLabel".Translate(p.LabelShort, p).CapitalizeFirst(), "LetterFriendlyTrapSprung".Translate(p.LabelShort, p).CapitalizeFirst(), LetterDefOf.NegativeEvent, new TargetInfo(base.Position, map));
				}
			}
		}

		public void Spring(Pawn p)
		{
			bool spawned = base.Spawned;
			Map map = base.Map;
			SpringSub(p);
			if (def.building.trapDestroyOnSpring)
			{
				if (!base.Destroyed)
				{
					Destroy();
				}
			}
		}

		protected float SpringChance(Pawn p)
		{
			float num = 1f;
			if (KnowsOfTrap(p))
			{
				if (p.RaceProps.Animal)
				{
					num = 0.2f;
					num *= def.building.trapPeacefulWildAnimalsSpringChanceFactor;
				}
				else
				{
					num = 0.3f;
				}
			}
			Log.Message(p + " - chance: " + Mathf.Clamp01(num) + " - " + p.def, true);
			return Mathf.Clamp01(num);
		}

		public new bool KnowsOfTrap(Pawn p)
		{
			if (p.Faction != null && !p.Faction.HostileTo(base.Faction))
			{
				return true;
			}
			if (p.Faction == null && p.RaceProps.Animal && !p.InAggroMentalState)
			{
				return true;
			}
			if (p.guest != null && p.guest.Released)
			{
				return true;
			}
			if (!p.IsPrisoner && base.Faction != null && p.HostFaction == base.Faction)
			{
				return true;
			}
			if (p.RaceProps.Humanlike && p.IsFormingCaravan())
			{
				return true;
			}
			if (p.IsPrisoner && p.guest.ShouldWaitInsteadOfEscaping && base.Faction == p.HostFaction)
			{
				return true;
			}
			if (p.Faction == null && p.RaceProps.Humanlike)
			{
				return true;
			}
			return false;
		}

		public override ushort PathFindCostFor(Pawn p)
		{
			if (!KnowsOfTrap(p))
			{
				return 0;
			}
			return 800;
		}

		public override ushort PathWalkCostFor(Pawn p)
		{
			if (!KnowsOfTrap(p))
			{
				return 0;
			}
			return 40;
		}

		public override bool IsDangerousFor(Pawn p)
		{
			return KnowsOfTrap(p);
		}
	}
}
