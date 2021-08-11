using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace RT_Rimtroid
{
    [StaticConstructorOnStartup]
    public static class RimtroidTest
    {
        static RimtroidTest()
        {
            Log.Message("Key test: " + "LetterLabelLarvaTutorial".Translate());
        }
    }
    public class RimtroidEvolutionTracker : GameComponent
    {
        public Dictionary<ThingDef, bool> spottedMetroids = new Dictionary<ThingDef, bool>();

        public int lastQueenDeathTick;
        public int metroidRaidGracePeriod;

        public Pawn currentQueen;

        public static Dictionary<ThingDef, Tuple<string, string>> spottedLabels = new Dictionary<ThingDef, Tuple<string, string>>
        {
            {RT_DefOf.RT_MetroidLarvae, new Tuple<string, string>("LetterLabelLarvaTutorial", "LetterLarvaTutorial")},
            {RT_DefOf.RT_BanteeMetroid, new Tuple<string, string>("LetterLabelBanteeTutorial", "LetterBanteeTutorial")},
            {RT_DefOf.RT_GammaMetroid, new Tuple<string, string>("LetterLabelGammaTutorial", "LetterGammaTutorial")},
            {RT_DefOf.RT_OmegaMetroid, new Tuple<string, string>("LetterLabelOmegaTutorial", "LetterOmegaTutorial")},
            {RT_DefOf.RT_AlphaMetroid, new Tuple<string, string>("LetterLabelAlphaTutorial", "LetterAlphaTutorial")},
            {RT_DefOf.RT_ZetaMetroid, new Tuple<string, string>("LetterLabelZetaTutorial", "LetterZetaTutorial")},
            {RT_DefOf.RT_QueenMetroid, new Tuple<string, string>("LetterLabelQueenTutorial", "LetterQueenTutorial")},
        };
        public RimtroidEvolutionTracker()
        {

        }
        public RimtroidEvolutionTracker(Game game)
        {

        }
        public override void StartedNewGame()
        {
            base.StartedNewGame();
            PreInit();
        }
        public override void LoadedGame()
        {
            base.LoadedGame();
            PreInit();
        }
        private void PreInit()
        {
            if (spottedMetroids is null)
            {
                spottedMetroids = new Dictionary<ThingDef, bool>();
            }
        }

        public void TryNotifyAboutMetroid(Pawn metroid)
        {
            if (!spottedMetroids.TryGetValue(metroid.def, out bool spotted))
            {
                if (spottedLabels.TryGetValue(metroid.def, out var eventLabels))
                {
                    Find.LetterStack.ReceiveLetter(eventLabels.Item1.Translate(), eventLabels.Item2.Translate(), LetterDefOf.NeutralEvent, metroid);
                    spottedMetroids[metroid.def] = true;
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref spottedMetroids, "spottedMetroids", LookMode.Def, LookMode.Value, ref thingDefKeys, ref boolValues);
            Scribe_Values.Look(ref lastQueenDeathTick, "lastQueenDeathTick");
            Scribe_Values.Look(ref metroidRaidGracePeriod, "metroidRaidGracePeriod");
            Scribe_References.Look(ref currentQueen, "currentQueen");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                PreInit();
            }
        }

        private List<ThingDef> thingDefKeys;
        private List<bool> boolValues;
    }
}