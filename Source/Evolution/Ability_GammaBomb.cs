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

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            this.pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RT_DefOf.RT_PlaceAlphaBomb, target.Cell));
            return base.Activate(target, dest);
        }
    }
}