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
            Log.Message("IncidentWorker_QueenSpotted : IncidentWorker - CanFireNowSub - var metroidFaction = Find.FactionManager.FirstFactionOfDef(RT_DefOf.RT_Metroids); - 1", true);
            var metroidFaction = Find.FactionManager.FirstFactionOfDef(RT_DefOf.RT_Metroids);
            Log.Message("IncidentWorker_QueenSpotted : IncidentWorker - CanFireNowSub - return metroidFaction.leader is null; - 2", true);
            return metroidFaction.leader is null || metroidFaction.leader.Dead;
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Log.Message("IncidentWorker_QueenSpotted : IncidentWorker - TryExecuteWorker - var metroidFaction = Find.FactionManager.FirstFactionOfDef(RT_DefOf.RT_Metroids); - 3", true);
            var metroidFaction = Find.FactionManager.FirstFactionOfDef(RT_DefOf.RT_Metroids);
            Log.Message("IncidentWorker_QueenSpotted : IncidentWorker - TryExecuteWorker - if (metroidFaction.TryGenerateNewLeader()) - 4", true);
            if (metroidFaction.TryGenerateNewLeader())
            {
                Log.Message("IncidentWorker_QueenSpotted : IncidentWorker - TryExecuteWorker - Find.LetterStack.ReceiveLetter(\"LetterLabelQueenSpotted\".Translate(), \"LetterMetroidQueenSpotted\".Translate(), LetterDefOf.ThreatBig); - 5", true);
                Find.LetterStack.ReceiveLetter("LetterLabelQueenSpotted".Translate(), "LetterMetroidQueenSpotted".Translate(), LetterDefOf.ThreatBig);
                Log.Message("IncidentWorker_QueenSpotted : IncidentWorker - TryExecuteWorker - return true; - 6", true);
                return true;
            }
            return false;
        }
    }
}