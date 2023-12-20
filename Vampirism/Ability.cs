using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    
    public abstract class Ability
    {
        public AbilityStats Statistics { get; private set; }

        public Ability(AbilityStats statistics)
        {
            Statistics = statistics;
        }

        public abstract void SetupAbility(Vampire vampire);

        public abstract IEnumerator PassiveCoroutine();
    }

    public class AbilityStats
    {
        public string Name { get; private set; }
        public string SkillTree { get; private set; }
        public int Tier { get; private set; }
        public int BaseLevel {  get; private set; }
        public int MaxLevel { get; private set; }
        private (string description, int unlockCost, Func<bool> unlockConditions)[] levelStats;
        public (string description, int unlockCost, Func<bool> unlockConditions) LevelStat(int level) => levelStats[level - 1]; // level is subtracted by 1 as index is zero-based
        private string[] tags;
        public string[] Tags { get => tags; }
        
        public AbilityStats(string name, string skillTree, int tier, int baseLevel, (string description, int unlockCost, Func<bool> unlockConditions)[] levelStats, string[] tags = null)
        {
            Name = name; ;
            SkillTree = skillTree;
            Tier = tier;

            BaseLevel = baseLevel;
            this.levelStats = levelStats;
            MaxLevel = levelStats.Length;
            this.tags = tags;

        }
    }

}
