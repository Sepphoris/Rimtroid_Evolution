using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Metamorphosis
{
    // Token: 0x02000241 RID: 577
    public class HediffCompProperties_RimtroidEvolution : HediffCompProperties
    {
        public HediffCompProperties_RimtroidEvolution()
        {
            this.compClass = typeof(HediffComp_RimtroidEvolution);
        }

        public List<MetroidWhitelistDef> whitelists = new List<MetroidWhitelistDef>();
        public ThingDef huskDef = null;
        public List<RimtroidEvolutionPath> PossibleEvolutionPaths = new List<RimtroidEvolutionPath>();
        public HediffDef stuntedHediffDef; //Hediff which if it exists, the pawn shouldn't transform.
    }

    public class HediffComp_RimtroidEvolution : HediffComp
    {
        public HediffCompProperties_RimtroidEvolution Props
        {
            get
            {
                return (HediffCompProperties_RimtroidEvolution)this.props;
            }
        }

        //If it ever gains the 'stunted' hediff while the transformation is in progress, cancel it and remove the transformation hediff.
        //public override bool CompShouldRemove => base.CompShouldRemove || Pawn.health.hediffSet.GetFirstHediffOfDef(Props.stuntedHediffDef) != null;

        private void TransformPawn(PawnKindDef kindDef, bool changeDef = true, bool keep = false)
        {
            //sets position, faction and map
            IntVec3 intv = parent.pawn.Position;
            Faction faction = parent.pawn.Faction;
            Map map = parent.pawn.Map;
            RegionListersUpdater.DeregisterInRegions(parent.pawn, map);

            //Change Race to Props.raceDef
            if (changeDef && kindDef != null && kindDef != parent.pawn.kindDef)
            {
                parent.pawn.def = kindDef.race;
                parent.pawn.kindDef = kindDef;
                long ageB = Pawn.ageTracker.AgeBiologicalTicks;
                long ageC = Pawn.ageTracker.AgeChronologicalTicks;
                Pawn.ageTracker = new Pawn_AgeTracker(Pawn);
                Pawn.ageTracker.AgeBiologicalTicks = ageB;
                Pawn.ageTracker.AgeChronologicalTicks = ageC;

                foreach (AbilityDef def in Pawn.abilities.abilities.OfType<DD.Ability_Base>().Select(ability => ability.def).ToList())
                {
                    //Remove all framework abilities.
                    Log.Message(def.defName)
                    Pawn.abilities.RemoveAbility(def);
                }
                DD.CompAbilityDefinition comp = Pawn.TryGetComp<DD.CompAbilityDefinition>();
                if (comp != null)
                {
                    //Ticks the comp to force it to process/add abilities.
                    comp.CompTickRare();
                }

            }
            RegionListersUpdater.RegisterInRegions(parent.pawn, map);
            map.mapPawns.UpdateRegistryForPawn(parent.pawn);

            //decache graphics
            parent.pawn.Drawer.renderer.graphics.ResolveAllGraphics();

            // remove non whitelisted hediffs
            if (!Pawn.health.hediffSet.hediffs.NullOrEmpty())
            {
                if (!Props.whitelists.NullOrEmpty())
                {
                    foreach (MetroidWhitelistDef list in Props.whitelists)
                    {
                        if (parent.pawn.health.hediffSet.hediffs.Any(x => !list.whitelist.Contains(x.def) && x != this.parent))
                        {
                            List<Hediff> removeable = parent.pawn.health.hediffSet.hediffs.Where(x => !list.whitelist.Contains(x.def) && x != this.parent).ToList();
                            foreach (Hediff item in removeable)
                            {
                                if (item != this.parent)
                                {
                                    Pawn.health.RemoveHediff(item);
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<Hediff> removeable = parent.pawn.health.hediffSet.hediffs;
                    foreach (Hediff item in removeable)
                    {
                        if (item != this.parent)
                        {
                            Pawn.health.RemoveHediff(item);
                        }
                    }
                }
            }

            //save the pawn
            //parent.pawn.ExposeData();
            if (parent.pawn.Faction != faction)
            {
                parent.pawn.SetFaction(faction);
            }
            //spawn Husk if set
            if (Props.huskDef != null)
            {
                GenSpawn.Spawn(ThingMaker.MakeThing(Props.huskDef), parent.pawn.Position, parent.pawn.Map);
            }

        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            //If the pawn has the 'stunted' hediff.
            if (Pawn.health.hediffSet.GetFirstHediffOfDef(Props.stuntedHediffDef) != null)
            {
                //Don't try to transform.
                return;
            }

            RimtroidEvolutionPath path = null;
            if (Props.PossibleEvolutionPaths.Any(x => x.triggerDef != null && Pawn.health.hediffSet.HasHediff(x.triggerDef)))
            {
                path = Props.PossibleEvolutionPaths.First(x => x.triggerDef != null && Pawn.health.hediffSet.HasHediff(x.triggerDef));
            }
            else if (Props.PossibleEvolutionPaths.Any(x => x.triggerDef == null))
            {
                path = Props.PossibleEvolutionPaths.FindAll(x => x.triggerDef == null).RandomElement();
            }
            if (path != null)
            {
                if (Pawn.ageTracker.AgeBiologicalYearsFloat > path.Age)
                {
                    TransformPawn(path.Kind);
                }
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref this.ticksToDisappear, "ticksToDisappear", 0, false);
        }

        public override string CompDebugString()
        {
            return "ticksToDisappear: " + this.ticksToDisappear;
        }

        public int ticksToDisappear = 60;
    }

    public class RimtroidEvolutionPath
    {
        public HediffDef triggerDef;
        public float Age = 0f;
        public PawnKindDef Kind;
    }
    public class MetroidWhitelistDef : Def
    {
        public List<HediffDef> whitelist = new List<HediffDef>();
    }
}
