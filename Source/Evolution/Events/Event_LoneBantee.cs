using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace RT_Rimtroid
{

    public class Event_LoneBantee : IncidentWorker
    {



        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 intVec;
            return map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(ThingDef.Named("RT_BanteeMetroid")) && this.TryFindEntryCell(map, out intVec);
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            PawnKindDef pawnKindDef = PawnKindDef.Named("RT_BanteeMetroid");
            IntVec3 intVec;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out intVec, map, CellFinder.EdgeRoadChance_Animal))
            {
                return false;
            }

            Rot4 rot = Rot4.FromAngleFlat((map.Center - intVec).AngleFlat);

            IntVec3 loc2 = CellFinder.RandomClosewalkCellNear(intVec, map, 1, null);
            Pawn newThing = PawnGenerator.GeneratePawn(pawnKindDef, null);
            GenSpawn.Spawn(newThing, loc2, map, WipeMode.Vanish);

            Find.LetterStack.ReceiveLetter("LetterLabelLostBantee".Translate(), "LetterLostBantee".Translate(), LetterDefOf.NeutralEvent, newThing, null, null);

            return true;
        }
    }
}

