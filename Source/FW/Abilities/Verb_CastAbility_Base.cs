using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RT_Rimtroid
{
    public class Verb_CastAbility_Base : Verb_CastAbility
    {
        public override bool Available()
        {
            return ability.CanCast && base.Available();
        }

        public override void Reset()
        {
            base.Reset();
            if (ability is Ability_Base ab)
            {
                ab.Reset();
            }
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            return targ == Caster || base.CanHitTargetFrom(root, targ);
        }

        protected override bool TryCastShot()
        {
            if (ability is Ability_Base ba)
            {
                if (!ba.CanApplyOn(CurrentTarget) && !ba.CanApplyOn(CurrentDestination))
                {
                    //Should be applicable on either the target or the destination.
                    return false;
                }
            }

            if (ability.CanCast)
            {
                return base.TryCastShot();
            }
            else
            {
                return false;
            }
        }
    }
}
