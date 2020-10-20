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
    public class GainAbilityExtension : DefModExtension
    {
        public AbilityDef abilityDef;
        public float minAge;

        public bool CanGainAbility(Pawn pawn)
        {
            if (abilityDef == null)
            {
                Log.Error("GainAbilityExtension: AbilityDef not set");
                return false;
            }

            //Pawn not destoyed or null, can gain abilities, has a ThingDef, has RaceProps. (Skips over irrelevant/misconfigured pawns)
            return !pawn.DestroyedOrNull() && pawn.abilities != null && pawn.def != null && pawn.def.race != null && pawn.ageTracker != null;
        }

        public bool ShouldGainAbility(Pawn pawn)
        {
            //Can gain abilities and if the minLifeStage is what the pawn is currently at, or the minLifeStage was passed. (Get growing pains)
            return CanGainAbility(pawn) && pawn.ageTracker.AgeBiologicalYears >= minAge;
        }

        public bool ShouldHaveAbility(Pawn pawn)
        {
            //Should gain the ability and is older than the 'growth pains' range. (Instantly gain the ability)
            return ShouldGainAbility(pawn) && Mathf.Floor(pawn.ageTracker.AgeBiologicalYearsFloat) > minAge;
        }
    }
}
