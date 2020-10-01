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
        public LifeStageDef minLifeStage;
        public float minAge;

        public bool CanGainAbility(Pawn pawn)
        {
            //Not destoyed or null, can gain abilities, has a ThingDef, has RaceProps, has LifeStageAges(ThingDef), has the minLifeStage in its LifeStageAges list. (Blocks access to irrelevant/misconfigured pawns)
            return !pawn.DestroyedOrNull() && pawn.abilities != null && pawn.def != null && pawn.def.race != null && !pawn.def.race.lifeStageAges.NullOrEmpty() && pawn.def.race.lifeStageAges.Any(lifestage => lifestage.def == minLifeStage);
        }

        public bool ShouldGainAbility(Pawn pawn)
        {
            //Can gain abilities and if the minLifeStage is what the pawn is currently at, or the minLifeStage was passed. (Get growing pains)
            return CanGainAbility(pawn) && pawn.def.race.lifeStageAges.FirstIndexOf(lifestage => lifestage.def == minLifeStage) <= pawn.ageTracker.CurLifeStageIndex;
        }

        public bool ShouldHaveAbility(Pawn pawn)
        {
            //Should gain the ability and is older than the 'growth pains' range. (Instantly gain the ability)
            return ShouldGainAbility(pawn) && pawn.ageTracker.AgeBiologicalYearsFloat >= minAge;
        }
    }
}
