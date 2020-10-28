using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;

namespace RT_Rimtroid
{
    public static class FoodMethod
    {
        public static Pawn FindPawnTarget(this Pawn pawn, float distance)
        {
            Pawn victim = null;
            bool Predicate(Thing p) => p != null && p != pawn && p.def != pawn.def && p is Pawn prey && !prey.Downed && pawn.CanReserve(p) && FoodUtility.IsAcceptablePreyFor(pawn, prey);
            victim = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.Touch,
                TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false), distance, Predicate);
            return victim;
        }
    }
}
