using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RT_Rimtroid
{
    public class Alpha_Bomb : Building_Trap
    {
		public Pawn parent;
		protected override void SpringSub(Pawn p)
		{
			Log.Message(p + " - SpringSub", true);
			base.GetComp<CompExplosive>().StartWick(null);
		}
		private int tickCount;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
            {
				proximity = true;
				hostileOnly = true;
			}
        }

		private List<Pawn> touchingPawns = new List<Pawn>();

		private bool hostileOnly;
		private bool proximity;
		private bool timeOut;
        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref tickCount, "tickCount");
			Scribe_Values.Look(ref hostileOnly, "hostileOnly");
			Scribe_Values.Look(ref proximity, "proximity");
			Scribe_Values.Look(ref timeOut, "timeOut");
			Scribe_References.Look(ref parent, "parent");
		}
		/* Temporarily Disabling while testing something else
		public override bool ClaimableBy(Faction by)
        {
			return false;
        }
		*/
        public override bool DeconstructibleBy(Faction faction)
        {
			return false;
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
				yield return g;
            }
			if (Faction == Faction.OfPlayer)
            {

                Command_Toggle command_Toggle = new Command_Toggle
                {
                    defaultLabel = "RT_EnableTimedBomb".Translate(),
                    defaultDesc = "RT_EnableTimedBomb".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/BombTimer"),
                    isActive = (() => this.timeOut),
                    toggleAction = delegate
                    {
                        this.timeOut = !this.timeOut;
                    },

                    hotKey = KeyBindingDefOf.Misc3,
                    turnOffSound = null,
                    turnOnSound = null
                };
                yield return command_Toggle;

                Command_Toggle command_Toggle2 = new Command_Toggle
                {
                    defaultLabel = "RT_EnableProximityBomb".Translate(),
                    defaultDesc = "RT_EnableProximityBomb".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/BombProximity"),
                    isActive = (() => this.proximity),
                    toggleAction = delegate
                    {
                        this.proximity = !this.proximity;
                    },

                    hotKey = KeyBindingDefOf.Misc4,
                    turnOffSound = null,
                    turnOnSound = null
                };
                yield return command_Toggle2;

                Command_Toggle command_Toggle3 = new Command_Toggle
                {
                    defaultLabel = "RT_EnableHostileOnlyBomb".Translate(),
                    defaultDesc = "RT_EnableHostileOnlyBomb".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/BombHostile"),
                    isActive = (() => this.hostileOnly),
                    toggleAction = delegate
                    {
                        this.hostileOnly = !this.hostileOnly;
                    },

                    hotKey = KeyBindingDefOf.Misc5,
                    turnOffSound = null,
                    turnOnSound = null
                };
                yield return command_Toggle3;

                Command_Action command_Action = new Command_Action
                {
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Detonate"),
                    defaultDesc = "CommandDetonateDesc".Translate(),
                    action = Command_Detonate
                };
                if (GetComp<CompExplosive>().wickStarted)
				{
					command_Action.Disable();
				}
				command_Action.defaultLabel = "CommandDetonateLabel".Translate();
				yield return command_Action;

                Command_Action command_Action2 = new Command_Action
                {
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/BombDissipate"),
                    defaultDesc = "RT_Dissipate".Translate(),
                    action = Command_Dissipate,
                    defaultLabel = "RT_Dissipate".Translate()
                };
                yield return command_Action2;
			}

		}
		private void Command_Detonate()
		{
			GetComp<CompExplosive>().StartWick();
		}

		private void Command_Dissipate()
		{
			this.Destroy();
		}

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
			var alphaComp = parent?.TryGetComp<CompAlphaBomb>();
			alphaComp?.traps.Remove(this);
            base.Destroy(mode);

        }
        public override void Tick()
		{
			if (base.Spawned)
			{
				var options = this.def.GetModExtension<Alpha_ProximityTrapProperties>();
				if (options != null)
                {
					if (this.timeOut) 
					{
						tickCount++;
						if (tickCount >= options.explosionTimeout)
                        {
							this.SpringSub(null);
							tickCount = 0;
						}
					}

					if (this.proximity)
					{
						foreach (var cell in GenRadial.RadialCellsAround(this.Position, options.proximityRange, true))
						{
							List<Thing> thingList = cell.GetThingList(base.Map);
							for (int i = 0; i < thingList.Count; i++)
							{
                                if (thingList[i] is Pawn pawn && !touchingPawns.Contains(pawn))
                                {
                                    touchingPawns.Add(pawn);
                                    if (hostileOnly && !pawn.HostileTo(Faction.OfPlayer))
                                    {
                                        continue;
                                    }
                                    if ((pawn.IsAlphaMetroid() || pawn.IsBanteeMetroid() || pawn.IsMetroidLarvae()) && !pawn.HostileTo(Faction.OfPlayer))
                                    {
                                        continue;
                                    }
                                    this.SpringSub(null);
                                }
                            }
						}
					}
					else
                    {
						List<Thing> thingList = this.Position.GetThingList(base.Map);
						for (int i = 0; i < thingList.Count; i++)
						{
                            if (thingList[i] is Pawn pawn && !touchingPawns.Contains(pawn))
                            {
                                touchingPawns.Add(pawn);
                                if (hostileOnly && !pawn.HostileTo(Faction.OfPlayer))
                                {
                                    continue;
                                }
                                if ((pawn.IsAlphaMetroid() || pawn.IsBanteeMetroid() || pawn.IsMetroidLarvae()) && !pawn.HostileTo(Faction.OfPlayer))
                                {
                                    continue;
                                }
                                this.SpringSub(null);
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

			if (AllComps != null)
			{
				int i = 0;
				for (int count = AllComps.Count; i < count; i++)
				{
					AllComps[i].CompTick();
				}
			}
		}

        public override string GetInspectString()
        {
			string text = base.GetInspectString();
			if (text.Length > 0)
			{
				text += "\n";
			}
			var options = this.def.GetModExtension<Alpha_ProximityTrapProperties>();
			if (this.timeOut)
			{
				text += "RT_DetonatesIn".Translate((options.explosionTimeout - this.tickCount).ToStringSecondsFromTicks());
			}
			else if (this.tickCount > 0)
            {
				text += "RT_DetonatesInPaused".Translate((options.explosionTimeout - this.tickCount).ToStringSecondsFromTicks());
			}
			return text;
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
		protected override float SpringChance(Pawn p)
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
