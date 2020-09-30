using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DD
{
    public class HediffComp_GrowthSeverityScaling : HediffComp
    {
        public int ticks;

        public HediffCompProperties_GrowthSeverityScaling Props => (HediffCompProperties_GrowthSeverityScaling)props;

        //If no longer in range or unable to gain abilities, if grant ability was set, check if already has the ability.
        public override bool CompShouldRemove => !SeverityInRange || parent.pawn.abilities == null || (Props.AbilityOnCompletion != null && parent.pawn.abilities.GetAbility(Props.AbilityOnCompletion) != null);

        public bool SeverityInRange => Props.severityRange.TrueMin < parent.Severity && parent.Severity < Props.severityRange.TrueMax;

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticks, "ticks", 0);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (Props.AbilityOnCompletion != null && !SeverityInRange)
            {
                //Fanfare: Prolly message?
                Messages.Message("{DRAGON} has gained the ability of breath.".Translate(parent.pawn.Named("DRAGON")), parent.pawn, MessageTypeDefOf.PositiveEvent);
                parent.pawn.abilities.GainAbility(Props.AbilityOnCompletion);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            //severityAdjustment = Props.severityRange.LerpThroughRange((float)ticks / (float)Props.GetTicksAt(Props.severityRange.TrueMax));
            ticks++;
            severityAdjustment = Props.GetSeverityAt(ticks) - parent.Severity;
        }

        public override string CompLabelInBracketsExtra => base.CompLabelInBracketsExtra + "(" + (Props.GetTicksAt(Props.severityRange.TrueMax) - ticks).ToStringTicksToPeriodVague(false) + ")";
        public override string CompDebugString() => "ticks=" + ticks + "/" + Props.GetTicksAt(Props.severityRange.TrueMax) + (Props.AbilityOnCompletion != null ? "\nApply on Completion: " + Props.AbilityOnCompletion.defName : "");
    }
}
