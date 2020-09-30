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
    public abstract class SequenceNode
    {
        public abstract bool IsUsable(Pawn pawn);
        public abstract Verb GetVerb(Pawn pawn);
    }
}
