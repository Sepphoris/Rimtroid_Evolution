using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DD
{
    public class AbilityNode : SequenceNode
    {
        public AbilityDef abilityDef;

        public override Verb GetVerb(Pawn pawn)
        {
            Ability ability = pawn.abilities.GetAbility(abilityDef);
            if (ability != null && !ability.VerbTracker.AllVerbs.NullOrEmpty())
            {
                return ability.VerbTracker.AllVerbs.RandomElementByWeightWithFallback(verb => verb.verbProps.commonality);
            }
            else
            {
                return null;
            }
        }

        public override bool IsUsable(Pawn pawn)
        {
            if (abilityDef == null)
            {
                //Ability not set.
                return false;
            }

            if (pawn.abilities == null)
            {
                //Can't gain abilities.
                return false;
            }

            Ability ability = pawn.abilities.GetAbility(abilityDef);

            if (ability == null)
            {
                //Doesn't have ability.
                return false;
            }

            if(ability.HasCooldown && !ability.CanCast)
            {
                //Ability cooling down.
                return false;
            }

            return true;
        }
    }
}
