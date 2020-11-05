using System;
using RimWorld;
using Verse;

namespace RT_Rimtroid
{
    public class WorkGiver_AnimalResource : WorkGiver_GatherAnimalBodyResources
    {
        protected override JobDef JobDef
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("RT_AnimalResource");
            }
        }

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompAnimalProduct>();
        }
    }
}

