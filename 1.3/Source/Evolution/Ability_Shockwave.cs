using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace RT_Rimtroid
{
    public class Ability_Shockwave : RT_Core.Ability_Base
    {
        public Ability_Shockwave(Pawn pawn) : base(pawn)
        {
        }

        public Ability_Shockwave(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            //MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDefOf.Mote_LineEMP, 5);
            MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDef.Named("Mote_PsycastPsychicEffect"), 5);

            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, def.verbProperties.range, true).Where(t => t != pawn && pawn.CanSee(t)))
            {
                thing.TakeDamage(new DamageInfo(def.verbProperties.meleeDamageDef, 6f, instigator: this.pawn));
                if (thing is Pawn pawn && Rand.Chance(0.3f))
                {
                    pawn?.stances?.stunner?.StunFor(150, pawn);
                }
            }

            return base.Activate(target, dest);
        }
    }
}