using System;

using UnityEngine;
using RimWorld;
using Verse;
using System.Collections.Generic;
using Verse.Sound;
using System.Linq;

namespace RT_Rimtroid
{
    public class LaserBeam : Projectile
    {
        new LaserBeamDef def => base.def as LaserBeamDef;

        public override void Draw()
        {

        }

        void TriggerEffect(EffecterDef effect, Vector3 position)
        {
            TriggerEffect(effect, IntVec3.FromVector3(position));
        }

        void TriggerEffect(EffecterDef effect, IntVec3 dest)
        {
            if (effect == null) return;

            var targetInfo = new TargetInfo(dest, Map, false);

            Effecter effecter = effect.Spawn();
            effecter.Trigger(targetInfo, targetInfo);
            effecter.Cleanup();
        }

        void SpawnBeam(Vector3 a, Vector3 b)
        {
            LaserBeamGraphic graphic = ThingMaker.MakeThing(def.beamGraphic, null) as LaserBeamGraphic;
            if (graphic == null) return;

            graphic.projDef = def;
            graphic.Setup(launcher, a, b);
            GenSpawn.Spawn(graphic, origin.ToIntVec3(), Map, WipeMode.Vanish);
        }

        void SpawnBeamReflections(Vector3 a, Vector3 b, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = (b - a).normalized;
                Vector3 c = b - dir.RotatedBy(Rand.Range(-22.5f, 22.5f)) * Rand.Range(1f, 4f);

                SpawnBeam(b, c);
            }
        }

        public HashSet<IntVec3> MakeLine(IntVec3 start, IntVec3 end, Map map)
        {
            var resultingLine = new ShootLine(start, end);
            HashSet<IntVec3> Positions = new HashSet<IntVec3>();
            var points = resultingLine.Points().ToList();
            foreach (var current in points)
            {
                Positions.Add(current);
                var adjs = GenAdj.CellsAdjacent8Way(new TargetInfo(current, map));
                foreach (var c in adjs)
                {
                    if (c.DistanceTo(current) <= def.fireWidth && c.DistanceTo(end) < points[0].DistanceTo(end))
                    {
                        Positions.Add(c);
                    }
                }
            }

            return Positions.Where(x => x.DistanceTo(start) > def.fireDistanceFromCaster).ToHashSet();
        }

        protected override void Impact(Thing hitThing)
        {

            LaserGunDef defWeapon = equipmentDef as LaserGunDef;
            Vector3 dir = (destination - origin).normalized;
            dir.y = 0;

            Vector3 a = origin + dir * (defWeapon == null ? 0.9f : defWeapon.barrelLength);
            Vector3 b;
            if (hitThing == null)
            {
                b = destination;
            }
            else if ((destination - hitThing.TrueCenter()).magnitude < 1)
            {
                b = destination;
            }
            else
            {
                b = hitThing.TrueCenter();
                b.x += Rand.Range(-0.5f, 0.5f);
                b.z += Rand.Range(-0.5f, 0.5f);
            }

            a.y = b.y = def.Altitude;

            SpawnBeam(a, b);

            Pawn pawn = launcher as Pawn;
            if (def.spawnFire)
            {
                foreach (var c in MakeLine(origin.ToIntVec3(), destination.ToIntVec3(), pawn.Map))
                {
                    if (c == pawn.Position && def.dontSpawnFireOnCaster)
                    {
                        continue;
                    }
                    Fire obj = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire);
                    obj.fireSize = 1f;
                    var grass = ThingMaker.MakeThing(ThingDef.Named("RT_Dummy_Grass"));
                    GenSpawn.Spawn(grass, c, pawn.Map, Rot4.North);
                    GenSpawn.Spawn(obj, c, pawn.Map, Rot4.North);
                }
            }
            IDrawnWeaponWithRotation weapon = null;
            if (pawn != null && pawn.equipment != null) weapon = pawn.equipment.Primary as IDrawnWeaponWithRotation;
            if (weapon != null)
            {
                float angle = (b - a).AngleFlat() - (intendedTarget.CenterVector3 - a).AngleFlat();
                weapon.RotationOffset = (angle + 180) % 360 - 180;
            }

            TriggerEffect(def.explosionEffect, b);

            base.Impact(hitThing);

            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget.Thing, ThingDef.Named("Gun_Autopistol"), this.def, this.targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null)
            {
                DamageDef damageDef = this.def.projectile.damageDef;
                float amount = (float)base.DamageAmount;
                float armorPenetration = base.ArmorPenetration;
                float y = this.ExactRotation.eulerAngles.y;
                Thing launcher = this.launcher;
                ThingDef equipmentDef = this.equipmentDef;
                DamageInfo dinfo = new DamageInfo(damageDef, amount, armorPenetration, y, launcher, null, null, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);

                Pawn hitPawn = hitThing as Pawn;
                if (hitPawn != null && hitPawn.stances != null && hitPawn.BodySize <= this.def.projectile.StoppingPower + 0.001f)
                {
                    hitPawn.stances.StaggerFor(95);
                }
            }
        }
    }
}
