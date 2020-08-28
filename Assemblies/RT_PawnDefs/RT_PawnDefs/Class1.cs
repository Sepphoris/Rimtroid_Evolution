using Verse;
using RimWorld;
using System.Collections.Generic;

namespace RT_StartingHediffs
{
    public class CompProperties_InitialHediff : CompProperties
    {
        public string hediffname;
        public float hediffseverity;
        public int numberOfHediffs;

        public CompProperties_InitialHediff()
        {
            base.pawn();
            this.compClass = (__Null)typeof(CompInitialHediff);
        }
    }
}
