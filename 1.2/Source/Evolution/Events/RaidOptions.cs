using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RT_Core
{
    public class RaidOptions : DefModExtension
    {
        public PawnGroupMaker pawnGroup;
        public PawnGroupMaker minimumPawnCountPerKind;
        public int minimumPawnCount = -1;
        public int fixedRaidPoints = -1;
        public float raidPointsMultiplier = -1f;
        public FactionDef raidFaction;
        public RaidStrategyDef raidStrategy;
        public PawnsArrivalModeDef raidArrival;
        public LetterDef letterDef;
        public string letterTitle;
        public string letterText;
        public FloatRange raidPoints;
        public float? minimumPlayerWealth;
        public TechLevel? minimumPlayerTechLevel;
        public List<ResearchProjectDef> requiredResearchProjectsUnlocked;
    }
}