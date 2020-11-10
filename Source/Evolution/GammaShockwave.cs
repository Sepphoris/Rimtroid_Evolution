using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace RT_Rimtroid
{
    public class GammaShockwave : Verb
    {
        protected override bool TryCastShot()
        {
            MoteMaker.MakeStaticMote(Caster.Position, Caster.Map, RT_DefOf.RT_GammaShockwaveMote, 5);
            foreach (IntVec3 pos in GenRadial.RadialCellsAround(Caster.Position, 5, true).Where(p => p.InBounds(Caster.Map) && GenSight.LineOfSight(Caster.Position, p, Caster.Map)))
            {
                List<Thing> thingsInRange = pos.GetThingList(Caster.Map);
                if (!thingsInRange.NullOrEmpty())
                {
                    foreach (Thing thing in thingsInRange.Where(thing => thing != Caster))
                    {
                        thing.TakeDamage(new DamageInfo(DamageDefOf.Burn, 5f));
                        if (thing is Pawn pawn && Rand.Chance(0.4f))
                        {
                            pawn.stances.stunner.StunFor(3, Caster);
                        }
                    }
                }
            }
            return true;
        }
    }
}