using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RT_Rimtroid;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace RT_Core
{
    public class MapComponent_MetroidManager : MapComponent
    {
        public MapComponent_MetroidManager(Map map) : base(map)
        {

        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            for (int num = CompEvolutionStage.comps.Count - 1; num >= 0; num--)
            {
                var comp = CompEvolutionStage.comps[num];
                if (comp.parent.Spawned && comp.parent.Map == this.map)
                {
                    if (comp.pawnKindDefToEvolve != null && Find.TickManager.TicksGame > comp.tickConversion)
                    {
                        comp.TransformPawn(comp.pawnKindDefToEvolve);
                    }
                }
            }
        }
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            foreach (Thing t in this.map.listerThings.AllThings)
            {
                Pawn pawn = null;
                if (t is Pawn p)
                {
                    pawn = p;
                }
                else if (t is Corpse c)
                {
                    pawn = c.InnerPawn;
                }
                else
                {
                    continue;
                }
                Hediff_LatchedMetroid hediff = pawn.health.hediffSet.hediffs.OfType<Hediff_LatchedMetroid>().FirstOrDefault();
                if (hediff != null)
                {
                    var comp = pawn.TryGetComp<CompLatchedMetroid>();
                    if (comp == null)
                    {
                        if (hediff.latchedMetroid != null)
                        {
                            comp = new CompLatchedMetroid();
                            comp.Initialize(new CompProperties_LatchedMetroid());
                            comp.parent = pawn;
                            comp.latchedMetroid = hediff.latchedMetroid;
                            comp.drainAgeFactor = hediff.drainAgeFactor;
                            comp.drainFoodGain = hediff.drainFoodGain;
                            comp.drainOverlayDuration = hediff.drainOverlayDuration;
                            comp.drainStunDuration = hediff.drainStunDuration;
                            comp.drainSicknessSeverity = hediff.drainSicknessSeverity;
                            comp.startLatchingTick = hediff.startLatchingTick;
                            comp.hediff_LatchedMetroid = hediff;
                            if (pawn.Dead)
                            {
                                var corpse = pawn.Corpse;
                                corpse.AllComps.Add(comp);
                            }
                            else
                            {
                                pawn.AllComps.Add(comp);
                            }
                        }
                    }
                    else if (pawn.Dead && comp != null)
                    {
                        var corpse = pawn.Corpse;
                        corpse.AllComps.Add(comp);
                    }
                }
            }
        }
    }
}

