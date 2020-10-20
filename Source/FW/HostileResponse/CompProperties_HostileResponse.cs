using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DD
{
    public class CompProperties_HostileResponse : CompProperties
    {
        public HostilityResponseType initialHostility = HostilityResponseType.Aggressive;
        public List<HostileResponseOption> options = new List<HostileResponseOption>();
    }
}
