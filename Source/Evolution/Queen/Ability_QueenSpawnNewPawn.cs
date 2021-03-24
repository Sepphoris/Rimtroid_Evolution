﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;
using RT_Core;

namespace RT_Rimtroid
{
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
            if (!queen.spawnPool.CanSpawnNewPawn())
            {
                reason = "RT_CannotSpawnNewPawn".Translate();
                return false;
            }
            else
            {
                return base.GizmoDisabled(out reason);
            }
        }
        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (queen.spawnPool.CanSpawnNewPawn())
            {
                queen.spawnPool.SpawnPawn(queen);
            }
            return base.Activate(target, dest);
        }
    }
}