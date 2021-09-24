using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using HarmonyLib;

namespace RT_Core
{
    public class CompProperties_PrisonerFeed : CompProperties
    {
        public CompProperties_PrisonerFeed()
        {
            this.compClass = typeof(CompPrisonerFeed);
        }
    }
    public class CompPrisonerFeed : ThingComp
    {
        public CompProperties_LatchedMetroid Props => (CompProperties_LatchedMetroid)props;
        public bool canBeEaten;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref canBeEaten, "canBeEaten");
        }
    }
}