using System.Collections.Generic;
using Verse;
using RimWorld;

namespace RT_Rimtroid
{
    public class SedativeProjectile : Bullet
    {
        public HediffDef HediffToAdd = RT_DefOf.RT_SedativeBuildup;

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (hitThing is Pawn hitPawn)
            {
                if (hitPawn.IsAnyMetroid())
                //float rand = Rand.Value; // 0 .. 1
                //if (rand <= Props.addHediffChance) // decimal % chance of occurence
                {
                    if (hitPawn.needs?.food != null) // make sure this pawn needs food
                    {
                        hitPawn.needs.food.CurLevel += hitPawn.needs.food.MaxLevel * Rand.Range(0.8f, 0.15f); // Represent as a percent of the max capacity, you may also want to make sure that curLevel <= MaxLevel
                        Hediff hediff = HediffMaker.MakeHediff(HediffToAdd, hitPawn, null);
                        hitPawn.health.AddHediff(hediff, null, null);
                        hediff.Severity = Rand.Range(0.9f, 1.5f) / (hitPawn.BodySize);
                    }
                }
                else if (hitPawn.RaceProps.IsMechanoid)
                {
                    return;
                }
                else
                {
                    return;
                }
            }
        }
    }
}