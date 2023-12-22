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
    public class AbilityManager
    {
        private List<AbilityTree> abilityTrees;

        public AbilityManager()
        {
            abilityTrees = new List<AbilityTree>();

        }

    }

    public abstract class AbilityTree
    {
        public string Name { get; private set; }
        public List<AbilityTier> Tiers { get; private set; }

        public AbilityTree(string name)
        {
            Name = name;
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class AbilityTreeAttribute : Attribute
    {
        string tree;

        public AbilityTreeAttribute(string tree)
        {
            this.tree = tree;
        }
    }

    public abstract class AbilityTier
    {
        public Func<bool>[] UnlockConditions { get; private set; }

        public AbilityTier(Func<bool>[] unlock = null)
        {
            UnlockConditions = unlock;
        }
    }

    public abstract class Ability
    {
        public string Name { get; private set; }
        //public AbilityTree Tree { get; private set; }
        //public AbilityTier Tier { get; private set; }
        public int BaseLevel { get; private set; }
        public int MaxLevel { get; private set; }
        private (string description, int unlockCost, Func<bool> unlockConditions)[] levelStats;
        public (string description, int unlockCost, Func<bool> unlockConditions) LevelStat(int level) => levelStats[level - 1]; // level is subtracted by 1 as index is zero-based
        private string[] tags;
        public string[] Tags { get => tags; }

        public Ability(string name, /*AbilityTree tree, AbilityTier tier,*/ int baseLevel, (string description, int unlockCost, Func<bool> unlockConditions)[] levelStats, string[] tags = null)
        {
            Name = name;
            /*Tree = tree;
            Tier = tier;*/

            BaseLevel = baseLevel;
            this.levelStats = levelStats;
            MaxLevel = levelStats.Length;
            this.tags = tags;

        }

        public abstract void SetupAbility(Vampire vampire);

        public abstract IEnumerator PassiveCoroutine();
    }

}
