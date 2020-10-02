using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RT_Rimtroid
{
    public class Metroid : Pawn
    {

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                this.SetFaction()
            }
        }
    }

    public class Queen : Metroid
    {
        public override void SetFaction(Faction newFaction, Pawn recruiter = null)
        {
            base.SetFaction(null);
        }
    }
}
