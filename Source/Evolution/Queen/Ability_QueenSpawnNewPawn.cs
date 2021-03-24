using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;
using RT_Core;

namespace RT_Rimtroid
{
    public class Command_Ability_SpawnNewPawn : Command_Ability
    {
        public Command_Ability_SpawnNewPawn(Ability ability): base(ability)
        {

        }

        public Queen Queen => this.Ability.pawn as Queen;

        public override string TopRightLabel
        {
            get
            {
                return 6 - Queen.spawnPool.TotalPawns.Count + "/6";
            }
        }
    }
    class Ability_QueenSpawnNewPawn : Ability_Base
    {
        public Ability_QueenSpawnNewPawn(Pawn pawn) : base(pawn)
        {
        }

        public Ability_QueenSpawnNewPawn(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }

        public Queen queen => pawn as Queen;
        public override bool GizmoDisabled(out string reason)
        {
            if (!queen.spawnPool.CanSpawnPawn(out reason))
            {
                return true;
            }
            else
            {
                return base.GizmoDisabled(out reason);
            }
        }
        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (queen.spawnPool.CanSpawnPawn(out var reason))
            {
                queen.spawnPool.SpawnPawn(queen);
            }
            return base.Activate(target, dest);
        }
    }


}
