using RimWorld;
using RT_Rimtroid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace RT_Core
{
    public class Hediff_LatchedMetroid : HediffWithComps
    {
        public Pawn latchedMetroid;

        public int drainStunDuration;
        public int drainOverlayDuration;
        public float drainFoodGain;
        public float drainAgeFactor;
        public float drainSicknessSeverity;
        public int drainEnergyProcessing;
        public int startLatchingTick;

        public override void PostAdd(DamageInfo? dinfo)
        {
            this.startLatchingTick = Find.TickManager.TicksGame;
            HealthUtility.AdjustSeverity(this.pawn, RT_RimtroidDefOf.RT_LifeDrainSickness, (this.drainSicknessSeverity / (float)drainOverlayDuration) * 30f);
            var comp = this.pawn.TryGetComp<CompLatchedMetroid>();
            if (comp == null)
            {
                comp = new CompLatchedMetroid();
                comp.Initialize(new CompProperties_LatchedMetroid());
                comp.latchedMetroid = latchedMetroid;
                comp.drainAgeFactor = this.drainAgeFactor;
                comp.drainFoodGain = this.drainFoodGain;
                comp.drainOverlayDuration = this.drainOverlayDuration;
                comp.drainStunDuration = this.drainStunDuration;
                comp.drainSicknessSeverity = this.drainSicknessSeverity;
                comp.startLatchingTick = this.startLatchingTick;
                comp.hediff_LatchedMetroid = this;

                if (this.pawn.Dead && this.pawn.ParentHolder is Corpse corpse)
                {
                    comp.parent = corpse;
                    corpse.AllComps.Add(comp);
                }
                else
                {
                    comp.parent = this.pawn;
                    this.pawn.AllComps.Add(comp);

                }

                if (!this.pawn.Dead)
                {
                    if (this.pawn.CurJobDef != JobDefOf.Flee)
                    {
                        this.pawn.jobs.StopAll();
                        Utils.MakeFlee(this.pawn, latchedMetroid, 50, new List<Thing> { latchedMetroid });
                    }
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (this.pawn.CurJobDef != JobDefOf.Flee)
            {
                this.pawn.jobs.StopAll();
                Utils.MakeFlee(this.pawn, latchedMetroid, 50, new List<Thing> { latchedMetroid });
            }
            if (Find.TickManager.TicksGame % 30 == 0)
            {
                var value = (this.drainSicknessSeverity / (float)drainOverlayDuration) * 30f;
                HealthUtility.AdjustSeverity(this.pawn, RT_RimtroidDefOf.RT_LifeDrainSickness, value * this.pawn.BodySize);
            }
        }
        public override void PostRemoved()
        {
            base.PostRemoved();
            if (this.pawn.ParentHolder is Corpse corpse)
            {
                GenSpawn.Spawn(latchedMetroid, corpse.Position, corpse.Map);
                corpse.AllComps.Remove(corpse.TryGetComp<CompLatchedMetroid>());
            }
            else
            {
                GenSpawn.Spawn(latchedMetroid, this.pawn.Position, this.pawn.Map);
                this.pawn.AllComps.Remove(this.pawn.TryGetComp<CompLatchedMetroid>());
                this.pawn.jobs.StopAll();
                Utils.MakeFlee(this.pawn, latchedMetroid, 50, new List<Thing> { latchedMetroid });
            }
            this.pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(RT_RimtroidDefOf.RT_FeedOn);
            if (this.pawn.ParentHolder is Corpse corpse2 && Rand.Chance(0.7f))
            {
                corpse2.GetComp<CompRottable>().RotProgress = corpse2.GetComp<CompRottable>().PropsRot.TicksToRotStart;
            }
            var metroidAbsorbEnergy = HediffMaker.MakeHediff(RT_RimtroidDefOf.RT_MetroidAbsorbEnergy, latchedMetroid) as Hediff_MetroidAbsorbEnergy;
            metroidAbsorbEnergy.drainFoodGain = this.drainFoodGain;
            metroidAbsorbEnergy.drainAgeFactor = this.drainAgeFactor;
            metroidAbsorbEnergy.drainEnergyProcessing = this.drainEnergyProcessing;
            latchedMetroid.health.AddHediff(metroidAbsorbEnergy);
            latchedMetroid.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RT_RimtroidDefOf.RT_AbsorbEnergyFinal));
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref this.latchedMetroid, "latchedMetroid");
            Scribe_Values.Look(ref drainStunDuration, "drainStunDuration");
            Scribe_Values.Look(ref drainOverlayDuration, "drainOverlayDuration");
            Scribe_Values.Look(ref drainFoodGain, "drainFoodGain");
            Scribe_Values.Look(ref drainAgeFactor, "drainAgeFactor");
            Scribe_Values.Look(ref drainSicknessSeverity, "drainSicknessSeverity");
            Scribe_Values.Look(ref startLatchingTick, "startLatchingTick");
        }
    }
}