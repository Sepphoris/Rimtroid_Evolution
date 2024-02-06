using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace RT_Rimtroid
{
    public class Ability_MetroidBomb : RT_Core.Ability_Base
    {
        public Ability_MetroidBomb(Pawn pawn) : base(pawn) { }
        public Ability_MetroidBomb(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public static TargetingParameters ForLoc(Pawn pawn = null)
        {
            TargetingParameters targetingParameters = new TargetingParameters();
            targetingParameters.canTargetLocations = true;
            targetingParameters.validator = (TargetInfo target) => pawn.CanReserveAndReach(target.Cell, PathEndMode.OnCell, Danger.Deadly);
            return targetingParameters;
        }
        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Log.Message("Activating");
            Find.Targeter.BeginTargeting(ForLoc(pawn), delegate (LocalTargetInfo x)
            {
                var job = JobMaker.MakeJob(RT_RimtroidDefOf.RT_PlaceAlphaBomb, x.Cell);
                job.ability = this;
                this.pawn.jobs.TryTakeOrderedJob(job);
            }, null, null);
            return base.Activate(target, dest);
        }
    }
}