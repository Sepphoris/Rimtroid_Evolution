using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RT_Rimtroid
{
    public static class RimtroidMoteMaker
    {
        public static void MakeElectricFloor(Vector3 loc, Map map)
        {
            if (!loc.ShouldSpawnMotesAt(map))
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_BlastEMP"), null);
            moteThrown.Scale = Rand.Range(0.5f, 1f);
            moteThrown.rotationRate = Rand.Range(-12f, 12f);
            moteThrown.exactPosition = loc;
            GenSpawn.Spawn(moteThrown, IntVec3Utility.ToIntVec3(loc), map, 0);
        }
    }
}
