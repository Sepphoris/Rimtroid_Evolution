using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System.Text;
using System.Threading.Tasks;

namespace RT_Rimtroid
{
	public static class ElectricGenExplosion
	{
		private static readonly int PawnNotifyCellCount = GenRadial.NumCellsInRadius(4.5f);

		public static void DoExplosion(IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, int damAmount = -1, float armorPenetration = -1f, SoundDef explosionSound = null, ThingDef weapon = null, ThingDef projectile = null, Thing intendedTarget = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1, float chanceToStartFire = 0f, bool damageFalloff = false)
		{
			Log.Message("ElectricGenExplosion (DoExplosion) has started");
			if (map == null)
			{
				Log.Warning("Tried to do an electric explosion in a null map (Rimtroid: Evolution).", false);
				return;
			}
			if (damAmount < 0)
			{
				damAmount = damType.defaultDamage;
				armorPenetration = damType.defaultArmorPenetration;
				if (damAmount < 0)
				{
					Log.ErrorOnce("Attempted to trigger an electric explosion without defined damage (Rimtroid: Evolution)", 91094882, false);
					damAmount = 1;
				}
			}
			if (armorPenetration < 0f)
			{
				armorPenetration = (float)damAmount * 0.015f;
			}
			ElectricExplosion explosion = (ElectricExplosion)GenSpawn.Spawn(RT_DefOf.RT_ExplosionTest, center, map, WipeMode.Vanish);
			Log.Message("Spawned RT_ExplosionTest item");
			explosion.radius = radius;
			explosion.damType = damType;
			explosion.instigator = instigator;
			explosion.damAmount = damAmount;
			explosion.armorPenetration = armorPenetration;
			explosion.weapon = weapon;
			explosion.projectile = projectile;
			explosion.intendedTarget = intendedTarget;
			explosion.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
			explosion.preExplosionSpawnChance = preExplosionSpawnChance;
			explosion.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
			explosion.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
			explosion.postExplosionSpawnChance = postExplosionSpawnChance;
			explosion.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
			explosion.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
			explosion.chanceToStartFire = chanceToStartFire;
			explosion.damageFalloff = damageFalloff;
			explosion.StartExplosion(explosionSound);
		}

		public static void RenderPredictedAreaOfEffect(IntVec3 loc, float radius)
		{
			Log.Message("DrawFieldEdges for ElectricGenExplosion has started.");
			GenDraw.DrawFieldEdges(DamageDefOf.Bomb.Worker.ExplosionCellsToHit(loc, Find.CurrentMap, radius).ToList<IntVec3>());
		}

		public static void NotifyNearbyPawnsOfDangerousExplosive(Thing exploder, DamageDef damage, Faction onlyFaction = null)
		{
			Log.Message("Notifying nearby pawns of dangerous explosive for ElectricGenExplosion.");
			Room room = exploder.GetRoom(RegionType.Set_Passable);
			for (int i = 0; i < ElectricGenExplosion.PawnNotifyCellCount; i++)
			{
				IntVec3 c = exploder.Position + GenRadial.RadialPattern[i];
				if (c.InBounds(exploder.Map))
				{
					List<Thing> thingList = c.GetThingList(exploder.Map);
					for (int j = 0; j < thingList.Count; j++)
					{
						Pawn pawn = thingList[j] as Pawn;
						if (pawn != null && pawn.RaceProps.intelligence >= Intelligence.Humanlike && (onlyFaction == null || pawn.Faction == onlyFaction) && damage.ExternalViolenceFor(pawn))
						{
							Room room2 = pawn.GetRoom(RegionType.Set_Passable);
							if (room2 == null || room2.CellCount == 1 || (room2 == room && GenSight.LineOfSight(exploder.Position, pawn.Position, exploder.Map, true, null, 0, 0)))
							{
								return;
							}
						}
					}
				}
			}
		}
	}
}