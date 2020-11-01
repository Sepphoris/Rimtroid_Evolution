using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace RT_Rimtroid
{


    public class RT_FloatingComp : CompProperties
    {
        public bool isFloater = false;
        public bool canCrossWater = false;

        public RT_FloatingComp()
        {
            this.compClass = typeof(RT_Flight);
        }

    }
}