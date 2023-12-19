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
    public class Vigor : Ability
    {
        private (float health, float runSpeed, float jumpPower) increasePerLevel = (5f, 0.02f, 0.02f);

        private Creature playerCreature;
        private Locomotion playerLocomotion { get => playerCreature.currentLocomotion; }

        private float baseMaxHealth;
        
        public Vigor() : base(new AbilityStats(
            "Vigor",
            "Core",
            0,
            new (string description, int unlockCost, Func<bool> unlockConditions)[]
            {
                ("As your vampiric power increases, so does the capacities of your body. Leveling up increases your health, run speed, and jump power", 0, () => false),
            }))
        {
            VampireMaster.Instance.creatureLoadEvent += VigorCreatureLoad;
            VampireProgression.levelUpEvent += VigorLevel;
        }

        public override void SetupHandler() => SetupHandler<Vigor>();

        public override IEnumerator PassiveCoroutine()
        {
            yield return null;
        }

        private void VigorCreatureLoad(Creature creature)
        {
            playerCreature = creature;

            baseMaxHealth = playerCreature.maxHealth;

            int currentLevel = VampireMaster.Instance.Progression.Level.current;
            VigorLevel(currentLevel);
        }

        private void VigorLevel(int level)
        {
            playerCreature.maxHealth = baseMaxHealth + (increasePerLevel.health * level);
            playerLocomotion.SetSpeedModifier(this, 1.0f, 1.0f, 1.0f, increasePerLevel.runSpeed * level + 1.0f, increasePerLevel.jumpPower * level + 1.0f, 1.0f);
        }
    }
}
