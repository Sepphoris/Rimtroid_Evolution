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
    }
}