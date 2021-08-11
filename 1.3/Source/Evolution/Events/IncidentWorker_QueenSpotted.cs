using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace RT_Rimtroid
{
    public class IncidentWorker_QueenSpotted : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var metroidFaction = Find.FactionManager.FirstFactionOfDef(RT_DefOf.RT_Metroids);
            var comp = Current.Game.GetComponent<RimtroidEvolutionTracker>();

            return (comp.currentQueen is null || comp.currentQueen.Dead) && (metroidFaction.leader is null || metroidFaction.leader.Dead);
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var metroidFaction = Find.FactionManager.FirstFactionOfDef(RT_DefOf.RT_Metroids);
            if (metroidFaction.TryGenerateNewLeader())
            {
                Find.LetterStack.ReceiveLetter("LetterLabelQueenSpotted".Translate(), "LetterMetroidQueenSpotted".Translate(), LetterDefOf.ThreatBig);
                return true;
            }
            return false;
        }
    }
}