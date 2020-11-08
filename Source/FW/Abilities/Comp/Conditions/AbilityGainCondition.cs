using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RT_Rimtroid
{
    public abstract class AbilityGainCondition
    {
        public abstract bool IsSatisfied(CompAbilityDefinition def);

        public abstract bool IsFulfilled(CompAbilityDefinition def);
    }
}
