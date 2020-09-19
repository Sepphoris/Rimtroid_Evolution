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
    public class VerbNode : SequenceNode
    {
        public List<Type> verbClasses;

        public override Verb GetVerb(Pawn pawn) => VerbUtils.GetVerbs(pawn).Where(verb => verbClasses.Contains(verb.GetType())).RandomElementByWeightWithFallback(verb => verb.verbProps.commonality);

        public override bool IsUsable(Pawn pawn)
        {
            throw new NotImplementedException();
        }
    }
}
