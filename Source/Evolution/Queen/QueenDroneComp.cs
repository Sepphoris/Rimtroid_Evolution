using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RT_Rimtroid
{
    internal class LordToil_DefendQueen : LordToil
    {
        public override bool AllowSatisfyLongNeeds => true;
        public override float? CustomWakeThreshold => 0.5f;
        private Queen queen;
        public LordToil_DefendQueen()
        {

        }

        public LordToil_DefendQueen(Queen queen)
        {
            this.queen = queen;
        }
        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Pawn pawn = lord.ownedPawns[i];
                pawn.mindState.duty = new PawnDuty(RT_DefOf.RT_FollowQueen, queen, 12);
            }
        }

    }

    public class LordJob_DefendQueen : LordJob
    {
        private Queen queen;

        public LordJob_DefendQueen()
        {
        }

        public LordJob_DefendQueen(Queen queen)
        {
            this.queen = queen;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            stateGraph.StartingToil = new LordToil_DefendQueen(queen);
            return stateGraph;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref queen, "queen");
        }
    }
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
            var str = base.CompInspectStringExtra();
            if (str != null)
            {
                stringBuilder.AppendLine(str.TrimEndNewlines());
            }
            if (queen != null)
            {
                stringBuilder.AppendLine("RT_GroupedwithQueen".Translate());
                return stringBuilder.ToString().TrimEndNewlines();
            }
            return stringBuilder.ToString();
        }
        public void AssignToQueen(Queen queen)
        {
            this.queen = queen;
            SetDuty();
        }

        public override void CompTick()
        {
            if (!Metroid.Dead && Metroid.mindState?.duty?.focus == null && queen != null)
            {
                this.SetDuty();
            }
            base.CompTick();
        }

        public void SetDuty()
        {
            queen.GetCustomLord().AddPawn(Metroid);
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

