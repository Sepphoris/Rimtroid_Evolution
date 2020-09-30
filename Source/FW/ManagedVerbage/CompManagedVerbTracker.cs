using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DD
{
    public class CompManagedVerbTracker : ThingComp, IVerbOwner
    {
        private VerbTracker tracker;

        public List<VerbProperties> verbs;
        public List<Tool> tools;

        public List<ManagedSequence> sequences;

        private List<Verb> usageHistory = new List<Verb>();

        public VerbTracker VerbTracker
        {
            get
            {
                if (tracker == null)
                {
                    tracker = new VerbTracker(this);
                }
                return tracker;
            }
        }

        //public Verb Next
        //{
        //    get
        //    {
                
        //    }
        //}

        public List<VerbProperties> VerbProperties => verbs;

        public List<Tool> Tools => tools;

        public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;
        public Thing ConstantCaster => parent;

        public string UniqueVerbOwnerID() => "ManagedVerbs_" + parent.GetUniqueLoadID();
        public bool VerbsStillUsableBy(Pawn p) => true;

        public void Notify_VerbUsed(Verb verb)
        {
            usageHistory.Add(verb);
        }
    }
}
