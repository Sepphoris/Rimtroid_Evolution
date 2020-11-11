using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace RT_Rimtroid
{
    public class Ability_Shockwave : Ability_Base
    {
        public Ability_Shockwave(Pawn pawn) : base(pawn)
        {
        }

        public Ability_Shockwave(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDefOf.Mote_LineEMP, 5);

            foreach (IntVec3 pos in GenRadial.RadialCellsAround(pawn.Position, def.verbProperties.range, true).Where(p => p.InBounds(pawn.Map) && GenSight.LineOfSight(pawn.Position, p, pawn.Map)))
            {
                List<Thing> thingsInRange = pos.GetThingList(pawn.Map).ToList();
                if (!thingsInRange.NullOrEmpty())
                {
                    foreach (Thing thing in thingsInRange.Where(thing => thing != pawn))
                    {
                        thing.TakeDamage(new DamageInfo(def.verbProperties.meleeDamageDef, 6f, instigator: this.pawn));
                        if (thing is Pawn pawn && Rand.Chance(0.3f))
                        {
                            pawn?.stances?.stunner?.StunFor(150, pawn);
                        }
                    }
                }
            }

            return base.Activate(target, dest);
        }
    }
}