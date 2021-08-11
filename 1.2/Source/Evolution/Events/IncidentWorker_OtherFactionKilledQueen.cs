using System;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;

namespace RT_Rimtroid
{
    public class IncidentWorker_OtherFactionKilledQueen : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var metroidFaction = Find.FactionManager.FirstFactionOfDef(RT_DefOf.RT_Metroids);
            var humanFactions = Find.FactionManager.AllFactions.Where(x => !x.IsPlayer && x.def.humanlikeFaction && !x.Hidden);

            return metroidFaction.leader != null && !metroidFaction.leader.Dead && humanFactions.Any();
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var metroidFaction = Find.FactionManager.FirstFactionOfDef(RT_DefOf.RT_Metroids);
            metroidFaction.leader.Kill(null);
            var humanFactions = Find.FactionManager.AllFactions.Where(x => !x.IsPlayer && x.def.humanlikeFaction && !x.Hidden);
            var faction = humanFactions.RandomElement();
            Find.LetterStack.ReceiveLetter("LetterLabelFactionKilledMetroidQueen".Translate(faction.Named("FACTION")), "LetterFactionKilledMetroidQueen".Translate(faction.Named("FACTION")), 
                LetterDefOf.PositiveEvent);
            return true;
        }
    }
}