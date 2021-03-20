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
        public override void PostMake()
        {
            base.PostMake();
            this.Name = new NameSingle(this.def.LabelCap);
        }
        public override void SetFaction(Faction newFaction, Pawn recruiter = null)
        {
            base.SetFaction(null);
        }
    }
}
