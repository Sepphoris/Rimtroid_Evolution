using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DD
{
    public class HostilityStatisticRecord : IExposable
    {
        private IAttackTarget target;
        private int lastTickAttacked;
        private int sightings, intendedHits, unintendedHits;
        private float damageTotal;

        public IAttackTarget Target => target;
        public int SightingCount => sightings;
        public int IntendedHitCount => intendedHits;
        public int UnintendedHitCount => unintendedHits;
        public float DamageTotal => damageTotal;
        public int TicksSinceAttack => GenTicks.TicksGame - lastTickAttacked;
        public bool IsRecent => TicksSinceAttack < GenTicks.TickRareInterval;
        public bool IsOld => TicksSinceAttack > GenTicks.TickLongInterval;

        public HostilityStatisticRecord(IAttackTarget target)
        {
            this.target = target;
            lastTickAttacked = 0;
            sightings = 0;
            intendedHits = 0;
            unintendedHits = 0;
            damageTotal = 0;
        }

        public void ProcessAttack(float damage, bool isIntended)
        {
            lastTickAttacked = GenTicks.TicksGame;
            damageTotal += damage;

            if (isIntended)
            {
                intendedHits++;
            }
            else
            {
                unintendedHits++;
            }
        }

        public void ProcessSighting()
        {
            sightings++;
        }

        public float DamagePoints
        {
            get
            {
                //Points = Damage + (Damage * Log10(7*AttackedTick / CurrentTick))
                float points = DamageTotal + DamageTotal * Mathf.Log10((lastTickAttacked * 7) / GenTicks.TicksGame);
                return float.IsNaN(points) ? 0 : points;
            }
        }

        public float CalculatePoints(HostilityResponseType type)
        {
            float points = DamagePoints;

            switch (type)
            {
                case HostilityResponseType.Aggressive:
                    points *= IntendedHitCount + UnintendedHitCount + SightingCount;
                    break;
                case HostilityResponseType.Defensive:
                    points *= IntendedHitCount + (UnintendedHitCount / 2);
                    break;
                case HostilityResponseType.Passive:
                    points *= IntendedHitCount;
                    break;
            }

            points *= GenTicks.TicksGame;

            return points;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref target, "target");
            Scribe_Values.Look(ref sightings, "sightingCount");
            Scribe_Values.Look(ref lastTickAttacked, "lastTickAttacked");
            Scribe_Values.Look(ref damageTotal, "damageTaken");
            Scribe_Values.Look(ref intendedHits, "intendedHits");
            Scribe_Values.Look(ref unintendedHits, "unintendedHits");
        }
    }

}
