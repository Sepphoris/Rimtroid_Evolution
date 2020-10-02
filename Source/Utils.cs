using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RT_Rimtroid
{
    public static class Utils
    {
        public static bool IsMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_AlphaMetroid ||
                pawn.def == RT_DefOf.RT_GammaMetroid || 
                pawn.def == RT_DefOf.RT_OmegaMetroid ||
                pawn.def == RT_DefOf.RT_QueenMetroid ||
                pawn.def == RT_DefOf.RT_ZetaMetroid)
            {
                return true;
            }
            return false;
        }
    }
}
