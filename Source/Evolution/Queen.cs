using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RT_Rimtroid
{
    public class Metroid : Pawn
    {

        //public override void SpawnSetup(Map map, bool respawningAfterLoad)
        //{
        //    base.SpawnSetup(map, respawningAfterLoad);
        //    if (!respawningAfterLoad)
        //    {
        //        this.SetFaction();
        //    }
        //}
    }

    public class Queen : Metroid
    {
        public static bool preventFactionLeaderSpawn;
        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            preventFactionLeaderSpawn = true;
            base.Kill(dinfo, exactCulprit);
            preventFactionLeaderSpawn = false;
            Find.LetterStack.ReceiveLetter("LetterLabelMetroidQueenKilled".Translate(), "LetterMetroidQueenKilled".Translate(this.Named("PAWN")), LetterDefOf.PositiveEvent, this);
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, Find.World);
            parms.target = Find.World;
            Find.Storyteller.incidentQueue.Add(RT_DefOf.RT_QueenSpotted, (int)(GenDate.TicksPerDay * Rand.Range(45f, 60f)), parms);
        }
        public override void PostMake()
        {
            base.PostMake();
            this.Name = new NameSingle(Utils.GenerateTextFromRule(RT_DefOf.RT_QueenNames, "r_name", this.thingIDNumber));
        }
        public override void SetFaction(Faction newFaction, Pawn recruiter = null)
        {
            base.SetFaction(null);
        }
    }
}
