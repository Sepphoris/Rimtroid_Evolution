using System;
using System.Collections.Generic;
using RimWorld;
using RT_Rimtroid;
using Verse;

namespace RT_Core
{
    class Ingestion_MultipleHediffs : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            if (pawn.IsBanteeMetroid() == true)
            {
                Hediff hediff = HediffMaker.MakeHediff(this.hediffDef_BanteeMetroid, pawn, null);
                float num;
                if (this.severity > 0f)
                {
                    num = this.severity;
                }
                else
                {
                    num = this.hediffDef_BanteeMetroid.initialSeverity;
                }
                if (this.divideByBodySize)
                {
                    num /= pawn.BodySize;
                }
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }

            else if (pawn.IsMetroidLarvae() == true)
            {
                Hediff hediff = HediffMaker.MakeHediff(this.hediffDef_MetroidLarvae, pawn, null);
                float num;
                if (this.severity > 0f)
                {
                    num = this.severity;
                }
                else
                {
                    num = this.hediffDef_MetroidLarvae.initialSeverity;
                }
                if (this.divideByBodySize)
                {
                    num /= pawn.BodySize;
                }
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }

            else if (pawn.IsAlphaMetroid() == true)
            {
                Hediff hediff = HediffMaker.MakeHediff(this.hediffDef_AlphaMetroid, pawn, null);
                float num;
                if (this.severity > 0f)
                {
                    num = this.severity;
                }
                else
                {
                    num = this.hediffDef_AlphaMetroid.initialSeverity;
                }
                if (this.divideByBodySize)
                {
                    num /= pawn.BodySize;
                }
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }

            else if (pawn.IsGammaMetroid() == true)
            {
                Hediff hediff = HediffMaker.MakeHediff(this.hediffDef_GammaMetroid, pawn, null);
                float num;
                if (this.severity > 0f)
                {
                    num = this.severity;
                }
                else
                {
                    num = this.hediffDef_GammaMetroid.initialSeverity;
                }
                if (this.divideByBodySize)
                {
                    num /= pawn.BodySize;
                }
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }

            else if (pawn.IsZetaMetroid() == true)
            {
                Hediff hediff = HediffMaker.MakeHediff(this.hediffDef_ZetaMetroid, pawn, null);
                float num;
                if (this.severity > 0f)
                {
                    num = this.severity;
                }
                else
                {
                    num = this.hediffDef_ZetaMetroid.initialSeverity;
                }
                if (this.divideByBodySize)
                {
                    num /= pawn.BodySize;
                }
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }

            else if (pawn.IsOmegaMetroid() == true)
            {
                Hediff hediff = HediffMaker.MakeHediff(this.hediffDef_OmegaMetroid, pawn, null);
                float num;
                if (this.severity > 0f)
                {
                    num = this.severity;
                }
                else
                {
                    num = this.hediffDef_OmegaMetroid.initialSeverity;
                }
                if (this.divideByBodySize)
                {
                    num /= pawn.BodySize;
                }
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }

            else if (pawn.IsQueenMetroid() == true)
            {
                Hediff hediff = HediffMaker.MakeHediff(this.hediffDef_QueenMetroid, pawn, null);
                float num;
                if (this.severity > 0f)
                {
                    num = this.severity;
                }
                else
                {
                    num = this.hediffDef_QueenMetroid.initialSeverity;
                }
                if (this.divideByBodySize)
                {
                    num /= pawn.BodySize;
                }
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }

            //else if (pawn.IsStuntableMetroid() == true)
            //{
                //Hediff hediff = HediffMaker.MakeHediff(this.hediffDef_Metroid, pawn, null);
                //float num;
                //if (this.severity > 0f)
                //{
                    //num = this.severity;
                //}
                //else
                //{
                    //num = this.hediffDef_Metroid.initialSeverity;
                //}
                //if (this.divideByBodySize)
                //{
                    //num /= pawn.BodySize;
                //}
                //hediff.Severity = num;
                //pawn.health.AddHediff(hediff, null, null, null);
            //}
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

        public HediffDef hediffDef_BanteeMetroid;

        public HediffDef hediffDef_MetroidLarvae;

        public HediffDef hediffDef_AlphaMetroid;

        public HediffDef hediffDef_GammaMetroid;

        public HediffDef hediffDef_ZetaMetroid;

        public HediffDef hediffDef_OmegaMetroid;

        public HediffDef hediffDef_QueenMetroid;

        public HediffDef hediffDef_Metroid;

        public HediffDef hediffDef_LivingPawn;

        public float severity = -1f;

        private bool divideByBodySize;
    }
}
