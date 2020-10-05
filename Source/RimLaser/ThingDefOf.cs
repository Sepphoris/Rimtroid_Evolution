using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RT_Rimtroid
{
    [DefOf]
    public static class RimDefOf
    {
        static RimDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }

        public static ThingDef RT_RimlaserPrism;
    }

}
