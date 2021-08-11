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
    public class Command_Ability_RecallPawns : Command_Ability
    {
        public Command_Ability_RecallPawns(Ability ability) : base(ability)
        {

        }

        public Queen Queen => this.Ability.pawn as Queen;

        public override string Tooltip
        {
            get
            {
                var str = base.Tooltip + "\n";
                str += "RT_RecallAbilityTooltip".Translate(Queen.spawnPool.SpawnedPawns.Count, Queen.spawnPool.TotalPawns.Count());
                return str;
            }
        }
        public override string TopRightLabel
        {
            get
            {
                return Queen.spawnPool.SpawnedPawns.Count.ToString() + "/" + SpawnPool.maxPawnCount;
            }
        }
    }
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
