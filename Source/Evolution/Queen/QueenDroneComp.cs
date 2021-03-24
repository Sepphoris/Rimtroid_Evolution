using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace RT_Rimtroid
{
    public class CompProperties_QueenDrone : CompProperties
    {
        public CompProperties_QueenDrone()
        {
            this.compClass = typeof(QueenDroneComp);
        }
    }
    public class QueenDroneComp : ThingComp
    {
        public Queen queen;
        public CompProperties_QueenDrone Props => this.props as CompProperties_QueenDrone;
        public Pawn Metroid => this.parent as Pawn;
        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.CompInspectStringExtra());
            if (queen != null)
            {
                stringBuilder.AppendLine("RT_GroupedwithQueen".Translate());
            }
            return stringBuilder.ToString();
        }
        public void AssignToQueen(Queen queen)
        {
            this.queen = queen;
            SetFocus();
        }

        public override void CompTick()
        {
            if (!Metroid.Dead && Metroid.mindState?.duty?.focus == null && queen != null)
            {
                this.SetFocus();
            }
            base.CompTick();
        }

        public void SetFocus()
        {
            PawnDuty duty = new PawnDuty(DutyDefOf.DefendHiveAggressively);
            Metroid.mindState.duty = duty;
            Metroid.mindState.duty.focus = queen;
        }
        public void RemoveFromQueen()
        {
            if (this.queen != null && Metroid.mindState?.duty?.focus.Pawn == this.queen)
            {
                Metroid.mindState.duty = null;
                this.queen = null;
            }
        }
        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);
            if (queen != null)
            {
                RemoveFromQueen();
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref queen, "queen");
        }
    }
}

