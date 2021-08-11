using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Grammar;

namespace RT_Rimtroid
{
    [StaticConstructorOnStartup]
    public static class Utils
    {
        static Utils()
        {
            ApplySettings();
        }
        public static void ApplySettings()
        {
            if (RimtroidSettings.allowBiggerMetroidsToBeTamed)
            {
                RT_DefOf.RT_AlphaMetroid.race.wildness = 0.99f;
                RT_DefOf.RT_GammaMetroid.race.wildness = 0.99f;
                RT_DefOf.RT_ZetaMetroid.race.wildness = 0.99f;
                RT_DefOf.RT_OmegaMetroid.race.wildness = 0.99f;
            }
        }
        public static TaggedString GenerateTextFromRule(RulePackDef rule, string rootKeyword, int seed = -1)
        {
            if (seed != -1)
            {
                Rand.PushState();
                Rand.Seed = seed;
            }
            GrammarRequest request = default(GrammarRequest);
            request.Includes.Add(rule);
            string str = GrammarResolver.Resolve(rootKeyword, request);
            if (seed != -1)
            {
                Rand.PopState();
            }
            return str;
        }
        public static bool IsAnyMetroid(this PawnKindDef t)
        {
            return (t.race == RT_DefOf.RT_BanteeMetroid ||
                t.race == RT_DefOf.RT_MetroidLarvae ||
                t.race == RT_DefOf.RT_AlphaMetroid ||
                t.race == RT_DefOf.RT_GammaMetroid ||
                t.race == RT_DefOf.RT_ZetaMetroid ||
                t.race == RT_DefOf.RT_OmegaMetroid ||
                t.race == RT_DefOf.RT_QueenMetroid);
        }
        public static bool IsAnyMetroid(this Thing t)
        {
            return (t.def == RT_DefOf.RT_BanteeMetroid ||
                t.def == RT_DefOf.RT_MetroidLarvae ||
                t.def == RT_DefOf.RT_AlphaMetroid ||
                t.def == RT_DefOf.RT_GammaMetroid ||
                t.def == RT_DefOf.RT_ZetaMetroid ||
                t.def == RT_DefOf.RT_OmegaMetroid ||
                t.def == RT_DefOf.RT_QueenMetroid);
        }
        public static bool IsOlderMetroid(this Thing t)
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
