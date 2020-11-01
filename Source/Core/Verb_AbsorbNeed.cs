using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace RT_Rimtroid
{
    class VerbProperties_AbsorbNeed : VerbProperties
    {
        public int ageDaysAmount = 5;
        public float absorbAmount = 0.02f;
        public float stopFeedingAt = 0.8f;
    }

    class Verb_AbsorbNeed : Verb_MeleeAttackDamage
    {
        public VerbProperties_AbsorbNeed AbsorbProps => (VerbProperties_AbsorbNeed)verbProps;

        protected override bool TryCastShot()
        {
            Pawn casterPawn = CasterPawn;
            Pawn targetPawn = currentTarget.Thing as Pawn;

            if (!casterPawn.Spawned || !targetPawn.Spawned)
            {
                return false;
            }

            // does it have multiple attacks? With this code it will stop attacking when hunger is satisfied, which may not be what you want.
            if (casterPawn.needs.food.CurLevel >= AbsorbProps.stopFeedingAt)
            {
                return false;
            }

            // why is it stealing hunger satisfaction? also, again, if the target has 100% hunger, this verb stops working.
            if (casterPawn.needs.food.CurLevel >= 1 || targetPawn.needs.food.CurLevel <= 0)
            {
                return false;
            }

            // this can never miss?
            AbsorbNeed(casterPawn.needs.food, targetPawn.needs.food, AbsorbProps.absorbAmount);
            GainAge(casterPawn, AbsorbProps.ageDaysAmount);
            return base.TryCastShot();
        }

        // this is redundant, and doesn't match the corresponding conditions in TryCastShot, why?
        //public bool CanAbsorbFrom(Pawn target)
       // {
           // if (target.needs.TryGetNeed(NeedDefOf.Food).CurLevel > 0)
          //  {
          //     return true;
          //  }
          //
          //  return false;
        //}

        public void GainAge(Pawn caster, int ageDays)
        {
            caster.ageTracker.AgeBiologicalTicks += ageDays * GenDate.TicksPerDay; // age 5 days, check GenDate for other TicksPerX options
        }

        public void AbsorbNeed(Need casterFood, Need targetFood, float absorbtionRate = 0.01f)
        {
            float amountAbsorbed = Mathf.Min(targetFood.MaxLevel * absorbtionRate, targetFood.CurLevel);
            targetFood.CurLevel -= amountAbsorbed;
            casterFood.CurLevel += amountAbsorbed;

            // used Mathf.Min(x,y) to simplify this.
            // if (targetFood.CurLevel - amountAbsorbed >= 0)
            // {
            //     targetFood.CurLevel -= amountAbsorbed;
            //     casterFood.CurLevel += amountAbsorbed;
            // }
            // else
            // {
            //     casterFood.CurLevel += targetFood.CurLevel;
            //     targetFood.CurLevel = 0;
            // }
        }
    }
}

//When you define it like this, your ManeuverDef will have a different <verb> tag:
//<verb>
//<verbClass>RT_Rimtroid.Verb_AbsorbNeed</verbClass>
//<meleeDamageDef>RT_Lifedrain</meleeDamageDef>
//<ageDaysAmount>5</ageDaysAmount>
//<absorbAmount>0.02</absorbAmount>
//<stopFeedingAt>0.8</stopFeedingAt>
//</verb>