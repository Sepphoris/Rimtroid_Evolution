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
        public static bool IsMetroid(this Thing t)
        {
            return (t.def == RT_DefOf.RT_AlphaMetroid ||
                t.def == RT_DefOf.RT_GammaMetroid ||
                t.def == RT_DefOf.RT_ZetaMetroid ||
                t.def == RT_DefOf.RT_OmegaMetroid ||
                t.def == RT_DefOf.RT_QueenMetroid);
        }
        public static bool IsBanteeMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_BanteeMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsMetroidLarvae(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_MetroidLarvae)
            {
                return true;
            }
            return false;
        }
        public static bool IsAlphaMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_AlphaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsGammaMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_GammaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsZetaMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_ZetaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsOmegaMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_OmegaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsQueenMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_QueenMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsStuntableMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_MetroidLarvae||
                pawn.def == RT_DefOf.RT_AlphaMetroid ||
                pawn.def == RT_DefOf.RT_GammaMetroid ||
                pawn.def == RT_DefOf.RT_ZetaMetroid ||
                pawn.def == RT_DefOf.RT_OmegaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsValuedMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_BanteeMetroid ||
                pawn.def == RT_DefOf.RT_GammaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsDevaluedMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_MetroidLarvae ||
                pawn.def == RT_DefOf.RT_AlphaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsNeutralValueMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_ZetaMetroid ||
                pawn.def == RT_DefOf.RT_OmegaMetroid)
            {
                return true;
            }
            return false;
        }
    }
}
