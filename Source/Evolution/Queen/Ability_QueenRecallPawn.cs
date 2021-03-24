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
    class Ability_QueenRecallPawns : Ability_Base
    {
        public Ability_QueenRecallPawns(Pawn pawn) : base(pawn)
        {
        }

        public Ability_QueenRecallPawns(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }

        public Queen queen => pawn as Queen;

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            queen.spawnPool.RecallAll(queen);
            return base.Activate(target, dest);
        }
    }
}
