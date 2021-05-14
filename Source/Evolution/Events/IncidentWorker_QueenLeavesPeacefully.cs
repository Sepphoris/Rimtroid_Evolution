using System;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;
using Verse.AI;

namespace RT_Rimtroid
{
    public class IncidentWorker_QueenLeavesPeacefully : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var comp = Current.Game.GetComponent<RimtroidEvolutionTracker>();
            return comp.currentQueen != null && comp.currentQueen.Faction == Faction.OfPlayer && RCellFinder.TryFindBestExitSpot(comp.currentQueen, out IntVec3 spot);
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var comp = Current.Game.GetComponent<RimtroidEvolutionTracker>();
            if (RCellFinder.TryFindBestExitSpot(comp.currentQueen, out IntVec3 spot))
            {
                Job job = JobMaker.MakeJob(JobDefOf.Goto, spot);
                job.exitMapOnArrival = true;
                if (comp.currentQueen.jobs.TryTakeOrderedJob(job))
                {
                    var metroidFaction = Find.FactionManager.FirstFactionOfDef(RT_DefOf.RT_Metroids);
                    metroidFaction.leader = comp.currentQueen;
                    comp.metroidRaidGracePeriod = Find.TickManager.TicksGame + (Rand.RangeInclusive(30, 60) * GenDate.TicksPerDay);
                    Find.LetterStack.ReceiveLetter("LetterLabelQueenLeavesPeacefully".Translate(comp.currentQueen.Named("PAWN")), "LetterQueenLeavesPeacefully".Translate(comp.currentQueen.Named("PAWN"))
                        , this.def.letterDef, comp.currentQueen);
                    return true;
                }
            }
            return false;
        }
    }
}