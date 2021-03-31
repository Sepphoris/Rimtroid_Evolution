using System;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;

namespace RT_Rimtroid
{
    public class IncidentWorker_OmegaBecomesQueen : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var metroidFaction = Find.FactionManager.FirstFactionOfDef(RT_DefOf.RT_Metroids);
            var comp = Current.Game.GetComponent<RimtroidEvolutionTracker>();
            Log.Message("metroidFaction.leader: " + metroidFaction.leader);
            Log.Message("comp.currentQueen is null || comp.currentQueen.Dead: " + (comp.currentQueen is null || comp.currentQueen.Dead));
            Log.Message("PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Any(x => x.IsOmegaMetroid()): " + PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Any(x => x.IsOmegaMetroid()));
            Log.Message("comp.lastQueenDeathTick != 0 && Find.TickManager.TicksGame > comp.lastQueenDeathTick + (GenDate.TicksPerDay * 15): " + (comp.lastQueenDeathTick != 0 && Find.TickManager.TicksGame > comp.lastQueenDeathTick + (GenDate.TicksPerDay * 15)));
            return (metroidFaction.leader is null || metroidFaction.leader.Dead) && (comp.currentQueen is null || comp.currentQueen.Dead)
                && PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Any(x => x.IsOmegaMetroid()) 
                && comp.lastQueenDeathTick != 0 && Find.TickManager.TicksGame > comp.lastQueenDeathTick + (GenDate.TicksPerDay * 15);
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var omegaToQueen = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Where(x => x.IsOmegaMetroid()).RandomElement();
            TransformPawn(DefDatabase<PawnKindDef>.GetNamed("RT_QueenMetroid"), omegaToQueen);
            Find.LetterStack.ReceiveLetter("LetterLabelOmegaBecomesQueen".Translate(omegaToQueen.Named("PAWN")), "LetterOmegaBecomesQueen".Translate(omegaToQueen.Named("PAWN")), LetterDefOf.PositiveEvent, omegaToQueen);
            var comp = Current.Game.GetComponent<RimtroidEvolutionTracker>();
            comp.currentQueen = omegaToQueen;

            //var queen = omegaToQueen as Queen;
            //queen.spawnPool = new SpawnPool();
            //queen.spawnPool.Restock(queen);
            return true;
        }

        private void TransformPawn(PawnKindDef kindDef, Pawn pawn, bool changeDef = true, bool keep = false)
        {
            //sets position, faction and map
            IntVec3 intv = pawn.Position;
            Faction faction = pawn.Faction;
            Map map = pawn.Map;
            RegionListersUpdater.DeregisterInRegions(pawn, map);

            //Change Race to Props.raceDef
            if (changeDef && kindDef != null && kindDef != pawn.kindDef)
            {
                pawn.def = kindDef.race;
                pawn.kindDef = kindDef;
                long ageB = pawn.ageTracker.AgeBiologicalTicks;
                long ageC = pawn.ageTracker.AgeChronologicalTicks;
                pawn.ageTracker = new Pawn_AgeTracker(pawn);
                pawn.ageTracker.AgeBiologicalTicks = ageB;
                pawn.ageTracker.AgeChronologicalTicks = ageC;

                //Remove all framework abilities.
                foreach (AbilityDef def in pawn.abilities.abilities.OfType<RT_Core.Ability_Base>().Select(ability => ability.def).ToList())
                {
                    pawn.abilities.RemoveAbility(def);
                }

                RT_Core.CompAbilityDefinition comp = pawn.TryGetComp<RT_Core.CompAbilityDefinition>();
                if (comp != null)
                {
                    //Remove the old comp
                    pawn.AllComps.Remove(comp);
                }

                //Try loading CompProperties from the def.
                CompProperties props = kindDef.race.CompDefFor<RT_Core.CompAbilityDefinition>();
                RT_Core.CompAbilityDefinition newComp = null;

                if (props != null)
                {
                    //CompProperties found, so should gain the comp.
                    newComp = (RT_Core.CompAbilityDefinition)Activator.CreateInstance(props.compClass); //Create ThingComp from the loaded CompProperties.
                    newComp.parent = pawn; //Set Comp parent.
                    pawn.AllComps.Add(newComp); //Add to pawn's comp list.
                    newComp.Initialize(props); //Initialize it.
                }

                if (newComp != null)
                {
                    //Optionally, carry the data over.
                    if (comp != null)
                    {
                        //[NOTE] To carry over the values, make sure you change both damageTotal and killCounter from private to public in CompAbilityDefinition.
                        //newComp.damageTotal = comp.damageTotal;
                        //newComp.killCounter = comp.killCounter;
                    }

                    //Tick the comp to force it to process/add abilities.
                    newComp.CompTickRare();
                }
            }

            RegionListersUpdater.RegisterInRegions(pawn, map);
            map.mapPawns.UpdateRegistryForPawn(pawn);

            //decache graphics
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();

            // remove non whitelisted hediffs
            //if (!pawn.health.hediffSet.hediffs.NullOrEmpty())
            //{
            //    if (!Props.whitelists.NullOrEmpty())
            //    {
            //        foreach (MetroidWhitelistDef list in Props.whitelists)
            //        {
            //            if (parent.pawn.health.hediffSet.hediffs.Any(x => !list.whitelist.Contains(x.def) && x != this.parent))
            //            {
            //                List<Hediff> removeable = parent.pawn.health.hediffSet.hediffs.Where(x => !list.whitelist.Contains(x.def) && x != this.parent).ToList();
            //                foreach (Hediff item in removeable)
            //                {
            //                    if (item != this.parent)
            //                    {
            //                        Pawn.health.RemoveHediff(item);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        List<Hediff> removeable = parent.pawn.health.hediffSet.hediffs;
            //        foreach (Hediff item in removeable)
            //        {
            //            if (item != this.parent)
            //            {
            //                Pawn.health.RemoveHediff(item);
            //            }
            //        }
            //    }
            //}

            //save the pawn
            //parent.pawn.ExposeData();
            if (pawn.Faction != faction)
            {
                pawn.SetFaction(faction);
            }
            //spawn Husk if set
            //if (Props.huskDef != null)
            //{
            //    GenSpawn.Spawn(ThingMaker.MakeThing(Props.huskDef), parent.pawn.Position, parent.pawn.Map);
            //}

            pawn.needs.food.CurLevel = 1;
            var comp2 = pawn.TryGetComp<CompEvolutionTime>();
            if (comp2 != null)
            {
                //Remove the old comp
                pawn.AllComps.Remove(comp2);
            }
            //Try loading CompProperties from the def.
            var props2 = kindDef.race.GetCompProperties<CompProperties_EvolutionTime>();
            CompEvolutionTime newComp2 = null;
            if (props2 != null)
            {
                //CompProperties found, so should gain the comp.
                newComp2 = (CompEvolutionTime)Activator.CreateInstance(props2.compClass); //Create ThingComp from the loaded CompProperties.
                newComp2.parent = pawn; //Set Comp parent.
                pawn.AllComps.Add(newComp2); //Add to pawn's comp list.
                newComp2.Initialize(props2); //Initialize it.
            }
        }
    }
}