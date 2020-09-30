using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RT_Rimtroid
{
	public class CompProperties_SpawnerAdv : CompProperties
	{
		public ThingDef thingToSpawn;

		public PawnKindDef pawnKindToSpawn;

		public int maxPawnCount;

		public int maxThingCount;

		public int spawnCount = 1;

		public IntRange spawnIntervalRange = new IntRange(100, 100);

		public int spawnMaxAdjacent = -1;

		public bool spawnForbidden;

		public bool requiresPower;

		public bool writeTimeLeftToSpawn;

		public bool showMessageIfOwned;

		public string saveKeysPrefix;

		public bool inheritFaction;

		public CompProperties_SpawnerAdv()
		{
			compClass = typeof(CompSpawnerAdv);
		}
	}
	public class CompSpawnerAdv : ThingComp
	{
		private int ticksUntilSpawn;

		private List<Pawn> spawnedPawns;

		private List<Thing> spawnedThings;
		public CompProperties_SpawnerAdv PropsSpawner => (CompProperties_SpawnerAdv)props;

		private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				ResetCountdown();
			}
		}

		public override void CompTick()
		{
			TickInterval(1);
		}

		public override void CompTickRare()
		{
			TickInterval(250);
		}

		private void TickInterval(int interval)
		{
			if (!parent.Spawned)
			{
				return;
			}
			CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
			if (comp != null)
			{
				if (!comp.Awake)
				{
					return;
				}
			}
			else if (parent.Position.Fogged(parent.Map))
			{
				return;
			}
			if (!PropsSpawner.requiresPower || PowerOn)
			{
				ticksUntilSpawn -= interval;
				CheckShouldSpawn();
			}
		}

		private void CheckShouldSpawn()
		{
			Log.Message("ticksUntilSpawn: " + ticksUntilSpawn, true);
			if (ticksUntilSpawn <= 0)
			{
				TryDoSpawn();
				ResetCountdown();
			}
		}

		public int SpawnedPawns
        {
			get
            {
				if (spawnedPawns == null) return 0;
				int num = 0;
				for (int i = spawnedPawns.Count - 1; i >= 0; i--)
                {
					if (!spawnedPawns[i].Dead && !spawnedPawns[i].Destroyed)
                    {
						num++;
                    }
					else
                    {
						spawnedPawns.RemoveAt(i);
					}
                }
				return num;
            }
        }

		public int SpawnedThings
		{
			get
			{
				if (spawnedThings == null) return 0;
				int num = 0;
				for (int i = spawnedThings.Count - 1; i >= 0; i--)
				{
					if (!spawnedThings[i].Destroyed)
					{
						num++;
					}
					else
					{
						spawnedThings.RemoveAt(i);
					}
				}
				return num;
			}
		}
		public bool TryDoSpawn()
		{
			if (!parent.Spawned)
			{
				return false;
			}
			Log.Message(this.parent + " - " + PropsSpawner.pawnKindToSpawn, true);
			if (PropsSpawner.pawnKindToSpawn != null)
            {
				for (int i = 0; i < PropsSpawner.spawnCount; i++)
                {
					if (PropsSpawner.maxPawnCount > 0 && !(this.SpawnedPawns >= PropsSpawner.maxPawnCount) || PropsSpawner.maxPawnCount == 0)
                    {
						Faction faction = null;
						if (PropsSpawner.inheritFaction)
						{
							faction = this.parent.Faction;
						}
						Pawn pawn = PawnGenerator.GeneratePawn(PropsSpawner.pawnKindToSpawn, faction);
						GenSpawn.Spawn(pawn, this.parent.Position, this.parent.Map);
						if (PropsSpawner.maxPawnCount > 0)
                        {
							if (this.spawnedPawns == null) 
								this.spawnedPawns = new List<Pawn>();
							this.spawnedPawns.Add(pawn);
						}
					}
					else
                    {
						return false;
                    }
					return true;
				}
			}
			else if (PropsSpawner.thingToSpawn != null && TryFindSpawnCell(parent, PropsSpawner.thingToSpawn, PropsSpawner.spawnCount, out IntVec3 result))
            {
				if (PropsSpawner.maxThingCount > 0 && !(this.SpawnedThings >= PropsSpawner.maxThingCount) || PropsSpawner.maxThingCount == 0)
				{
					Thing thing = ThingMaker.MakeThing(PropsSpawner.thingToSpawn);
					thing.stackCount = PropsSpawner.spawnCount;
					if (thing == null)
					{
						Log.Error("Could not spawn anything for " + parent);
					}
					if (PropsSpawner.inheritFaction && thing.Faction != parent.Faction)
					{
						thing.SetFaction(parent.Faction);
					}
					GenPlace.TryPlaceThing(thing, result, parent.Map, ThingPlaceMode.Direct, out Thing lastResultingThing);
					if (PropsSpawner.spawnForbidden)
					{
						lastResultingThing.SetForbidden(value: true);
					}
					if (PropsSpawner.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
					{
						Messages.Message("MessageCompSpawnerSpawnedItem".Translate(PropsSpawner.thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent);
					}
					if (PropsSpawner.maxThingCount > 0)
					{
						if (this.spawnedThings == null)
							this.spawnedThings = new List<Thing>();
						this.spawnedThings.Add(thing);
					}
					return true;
				}


			}
			return false;
		}

		public static bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
		{
			foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(parent).InRandomOrder())
			{
				if (item.Walkable(parent.Map))
				{
					Building edifice = item.GetEdifice(parent.Map);
					if (edifice == null || !thingToSpawn.IsEdifice())
					{
						Building_Door building_Door = edifice as Building_Door;
						if ((building_Door == null || building_Door.FreePassage) && (parent.def.passability == Traversability.Impassable || GenSight.LineOfSight(parent.Position, item, parent.Map)))
						{
							bool flag = false;
							List<Thing> thingList = item.GetThingList(parent.Map);
							for (int i = 0; i < thingList.Count; i++)
							{
								Thing thing = thingList[i];
								if (thing.def.category == ThingCategory.Item && (thing.def != thingToSpawn || thing.stackCount > thingToSpawn.stackLimit - spawnCount))
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								result = item;
								return true;
							}
						}
					}
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		private void ResetCountdown()
		{
			ticksUntilSpawn = PropsSpawner.spawnIntervalRange.RandomInRange;
		}

		public override void PostExposeData()
		{
			string str = PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (PropsSpawner.saveKeysPrefix + "_");
			Scribe_Values.Look(ref ticksUntilSpawn, str + "ticksUntilSpawn", 0);
			Scribe_Collections.Look<Pawn>(ref spawnedPawns, "spawnedPawns", LookMode.Reference);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEBUG: Spawn " + PropsSpawner.thingToSpawn.label;
				command_Action.icon = TexCommand.DesirePower;
				command_Action.action = delegate
				{
					TryDoSpawn();
					ResetCountdown();
				};
				yield return command_Action;
			}
		}

		public override string CompInspectStringExtra()
		{
			if (PropsSpawner.writeTimeLeftToSpawn && (!PropsSpawner.requiresPower || PowerOn))
			{
				return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(PropsSpawner.thingToSpawn, null, PropsSpawner.spawnCount)) + ": " + ticksUntilSpawn.ToStringTicksToPeriod();
			}
			return null;
		}
	}
}
