using RimWorld;
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
    public class HediffGiver_GrantAbility : HediffGiver
    {
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            //Irrelevant pawn, not going to learn anything.
            if (!pawn.kindDef.HasModExtension<GainAbilityExtension>())
            {
                return;
            }

            //Can't gain abilities.
            if (pawn.abilities == null)
            {
                return;
            }

            //Can't have hediffs or already has the hediff.
            if (pawn.health == null || pawn.health.hediffSet.HasHediff(hediff))
            {
                return;
            }

            foreach (GainAbilityExtension ext in pawn.kindDef.modExtensions.OfType<GainAbilityExtension>())
            {
                if(pawn.abilities.GetAbility(ext.abilityDef) != null)
                {
                    //Already has this ability (skip it)
                    continue;
                }

                //Can ever gain the ability?
                if (ext.CanGainAbility(pawn))
                {
                    //Is in the lifestage or above?
                    if (ext.ShouldGainAbility(pawn))
                    {
                        if (ext.ShouldHaveAbility(pawn))
                        {
                            //Should've already gone through growing pains.
                            GainAbility(pawn, ext.abilityDef);
                        }
                        else
                        {
                            //Too young, should get growing pains.
                            ApplyHediff(pawn, ext.abilityDef);
                        }
                    }
                }
            }
        }

        private void ApplyHediff(Pawn pawn, AbilityDef def)
        {
            List<Hediff> hediffs = new List<Hediff>();
            HediffGiverUtility.TryApply(pawn, hediff, partsToAffect, outAddedHediffs: hediffs);
            foreach (HediffComp_GrowthSeverityScaling comp in hediffs.Select(h => h.TryGetComp<HediffComp_GrowthSeverityScaling>()).OfType<HediffComp_GrowthSeverityScaling>())
            {
                comp.Props.AbilityOnCompletion = def;
            }
        }

        private void GainAbility(Pawn pawn, AbilityDef def)
        {
            pawn.abilities.GainAbility(def);
        }
    }
}
