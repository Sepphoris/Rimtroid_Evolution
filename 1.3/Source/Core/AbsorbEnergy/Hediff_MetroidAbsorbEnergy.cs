using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RT_Core
{
    public class Hediff_MetroidAbsorbEnergy : HediffWithComps
    {
        public float drainFoodGain;
        public float drainAgeFactor;
        public float drainSicknessSeverity;
        public int drainEnergyProcessing;
        public int startLatchingTick;
        public int gainedTicks;

        public override void PostAdd(DamageInfo? dinfo)
        {
            this.startLatchingTick = Find.TickManager.TicksGame;
            var gainMultiplier = drainFoodGain / 0.10f;

            this.gainedTicks = (int)(GenDate.TicksPerDay * (drainAgeFactor * gainMultiplier));
            Log.Message($"gainedDays - {gainedTicks / GenDate.TicksPerDay} - drainFoodGain: {drainFoodGain} - drainAgeFactor: {drainAgeFactor} - gainMultiplier: {gainMultiplier}");
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 30 == 0)
            {
                MoteMaker.MakeStaticMote(this.pawn.Position, this.pawn.Map, RT_DefOf.RT_ProcessingEnergyMote);
                var ageGain = (gainedTicks / drainEnergyProcessing) * 30f;
                Log.Message(this.pawn + " gets new tick: " + ageGain);
                for (var i = 0; i < ageGain; i++)
                {
                    this.pawn.ageTracker.AgeTick();
                }
                var foodGain = (drainFoodGain / drainEnergyProcessing) * 30f;
                Log.Message(this.pawn + " gets new food gain: " + foodGain + " - cur level: " + this.pawn.needs.food.CurLevelPercentage);
                this.pawn.needs.food.CurLevelPercentage += foodGain;
                Log.Message(this.pawn + " gets new food gain: " + foodGain + " - cur level: " + this.pawn.needs.food.CurLevelPercentage);
            }
            if (Find.TickManager.TicksGame > startLatchingTick + drainEnergyProcessing)
            {
                this.pawn.health.RemoveHediff(this);
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            this.pawn.jobs.StopAll();
        }
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref drainFoodGain, "drainFoodGain");
            Scribe_Values.Look(ref drainAgeFactor, "drainAgeFactor");
            Scribe_Values.Look(ref drainSicknessSeverity, "drainSicknessSeverity");
            Scribe_Values.Look(ref startLatchingTick, "startLatchingTick");
            Scribe_Values.Look(ref drainEnergyProcessing, "drainEnergyProcessing");
        }
    }
}
