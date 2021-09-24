using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;
using RT_Core;

namespace RT_Rimtroid
{
    public class Verb_CastAbility_EnergyAbsorb : Verb_CastAbility_Base
    {
        public override TargetingParameters targetParams
        {
            get
            {
                var targetingParameters = new TargetingParameters();
                targetingParameters.canTargetAnimals = true;
                targetingParameters.canTargetHumans = true;
                targetingParameters.canTargetItems = true;
                targetingParameters.canTargetPawns = true;
                targetingParameters.mapObjectTargetsMustBeAutoAttackable = false;
                targetingParameters.validator = ((TargetInfo targ) => DoCheck(targ));
                return targetingParameters;
            }
        }

        public bool DoCheck(TargetInfo targ)
        {
            if (targ.Thing is Corpse corpse && !RT_Utils.blackListRaces.Contains(corpse.InnerPawn.def) && corpse.GetRotStage() == RotStage.Fresh && corpse.Age < GenDate.TicksPerDay * 3)
            {
                return true;
            }
            else if (targ.Thing is Pawn victim && victim.IsPrisoner && !victim.Downed && victim.GetComp<CompPrisonerFeed>().canBeEaten)
            {
                return true;
            }
            else if (targ.Thing is Pawn victim2 && !RT_Utils.blackListRaces.Contains(victim2.def) && victim2.RaceProps.Animal && victim2.Faction != this.CasterPawn.Faction && victim2.BodySize <= 4f)
            {
                return true;
            }
            return false;
        }
    }
}