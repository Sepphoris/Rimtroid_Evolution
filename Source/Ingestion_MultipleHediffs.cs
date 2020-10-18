using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RT_Rimtroid
{
    class Ingestion_MultipleHediffs : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            if (pawn.IsStuntableMetroid() == true)
            {
                Hediff hediff = HediffMaker.MakeHediff(this.hediffDef_Metroid, pawn, null);
                float num;
                if (this.severity > 0f)
                {
                    num = this.severity;
                }
                else
                {
                    num = this.hediffDef_Metroid.initialSeverity;
                }
                if (this.divideByBodySize)
                {
                    num /= pawn.BodySize;
                }
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }
            else if (pawn.RaceProps.IsMechanoid)
            {
                    return;

            }
            else
            {
                Hediff hediff = HediffMaker.MakeHediff(this.hediffDef_LivingPawn, pawn, null);
                float num;
                if (this.severity > 0f)
                {
                    num = this.severity;
                }
                else
                {
                    num = this.hediffDef_LivingPawn.initialSeverity;
                }
                if (this.divideByBodySize)
                {
                    num /= pawn.BodySize;
                }
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }

        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            if (parentDef.IsDrug && this.chance >= 1f)
            {
                foreach (StatDrawEntry s in this.hediffDef_LivingPawn.SpecialDisplayStats(StatRequest.ForEmpty()))
                {
                    yield return s;
                }
            }
            yield break;
        }

        public HediffDef hediffDef_Metroid;

        public HediffDef hediffDef_LivingPawn;

        public float severity = -1f;

        private bool divideByBodySize;
    }
}
