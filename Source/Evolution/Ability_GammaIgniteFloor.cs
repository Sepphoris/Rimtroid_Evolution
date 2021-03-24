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
    class Ability_GammaIgnite : Ability_Base
    {
        public Ability_GammaIgnite(Pawn pawn) : base(pawn)
        {
        }

        public Ability_GammaIgnite(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {

            return base.Activate(target, dest);
        }
    }
}
