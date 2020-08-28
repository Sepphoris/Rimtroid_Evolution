using Verse;
using RimWorld;
using System.Collections.Generic;

namespace RT_Rimtroid
{
    public class HediffGiver_AfterPeriod : HediffGiver
    {
        private FloatRange targetAgeRange;
        private HediffDef stuntedHediffDef; //Hediff which if it exists, the pawn shouldn't transform.

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            //If the pawn has the 'stunted' hediff.
            if (pawn.health.hediffSet.GetFirstHediffOfDef(this.stuntedHediffDef) != null)
            {
                //Don't continue executing this hediffgiver, just skip this pawn for now.
                return;
            }

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(this.hediff);
            if (hediff == null)
            {
                if (targetAgeRange.Includes(pawn.ageTracker.AgeBiologicalYearsFloat))
                {
                    TryApply(pawn);
                }
            }
            else
            {
                if (pawn.ageTracker.AgeBiologicalYearsFloat > targetAgeRange.max)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }
    }
}