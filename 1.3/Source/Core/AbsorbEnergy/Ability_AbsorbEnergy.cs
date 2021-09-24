using RimWorld;
using RimWorld.Planet;
using RT_Rimtroid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RT_Core
{
    public class Ability_AbsorbEnergy : RT_Core.Ability_Base
    {
        public Ability_AbsorbEnergy(Pawn pawn) : base(pawn) { }
        public Ability_AbsorbEnergy(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        public override bool CanApplyOn(LocalTargetInfo target)
        {
            Predicate<LocalTargetInfo> validator = delegate (LocalTargetInfo targetInfo)
            {
                if (target.Thing is Corpse corpse)
                {
                    return !RT_Utils.blackListRaces.Contains(corpse.InnerPawn.def)
                    && corpse.GetRotStage() == RotStage.Fresh && corpse.Age < GenDate.TicksPerDay * 3 && pawn.CanReserve(corpse);
                }
                else if (target.Thing is Pawn victim && victim.IsPrisoner && !victim.Downed && victim.GetComp<CompPrisonerFeed>().canBeEaten && pawn.CanReserve(victim))
                {
                    return true;
                }
                else if (target.Thing is Pawn victim2 && (pawn.Faction is null && !victim2.IsAnyMetroid() || pawn.Faction == Faction.OfPlayer) && !RT_Utils.blackListRaces.Contains(victim2.def) 
                && victim2.RaceProps.Animal && victim2.Faction != pawn.Faction && victim2.BodySize <= 4f && pawn.CanReserve(victim2))
                {
                    return true;
                }
                return false;
            };
            if (validator(target))
            {
                return true;
            }
            return false;
        }
        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Thing is Corpse corpse && !RT_Utils.blackListRaces.Contains(corpse.InnerPawn.def)
                && corpse.GetRotStage() == RotStage.Fresh && corpse.Age < GenDate.TicksPerDay * 3 && pawn.CanReserve(corpse))
            {
                var hediff = HediffMaker.MakeHediff(RT_DefOf.RT_MetroidHunting, pawn);
                pawn.health.AddHediff(hediff);
                var job = JobMaker.MakeJob(RT_DefOf.RT_AbsorbingEnergy, target);
                pawn.jobs.TryTakeOrderedJob(job);
            }
            else if (target.Pawn != null)
            {
                if (target.Pawn?.BodySize <= 4f && !RT_Utils.blackListRaces.Contains(target.Thing.def))
                {
                    var hediff = HediffMaker.MakeHediff(RT_DefOf.RT_MetroidHunting, pawn);
                    pawn.health.AddHediff(hediff);
                    var job = JobMaker.MakeJob(RT_DefOf.RT_AbsorbingEnergy, target);
                    pawn.jobs.TryTakeOrderedJob(job);
                }
                else
                {
                    Messages.Message("RT.AnimalIsTooBig".Translate(target.Pawn.LabelCap), MessageTypeDefOf.CautionInput);
                }
            }
            return base.Activate(target, dest);
        }
    }
}
