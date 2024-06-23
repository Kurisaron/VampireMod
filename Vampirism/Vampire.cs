using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public class Vampire : MonoBehaviour
    {
        private Creature creature;
        private bool isPlayer { get => creature != null && creature.isPlayer; }

        private (int current, int max) level = (1, 50);
        private float xp;
        private int skillPoints;

        public static event VampireCreatedEvent createdEvent;
        public static event VampireLevelEvent levelEvent;

        /// <summary>
        /// Only for use in extension Creature.Vampirize with AddComponent/GetComponent to serve as pseudo-constructor
        /// </summary>
        /// <param name="newCreature">Creature being made into a vampire</param>
        /// <param name="startingLevel">The vampire's starting level</param>
        /// <param name="startingXP">The vampire's starting xp</param>
        /// <param name="startingPoints">The vampire's starting amount of skill points</param>
        /// <param name="restrictedAbilities">A dictionary defining the available abilities and their levels. If null, the vampire will have all possible abilities at their base levels</param>
        /// <returns></returns>
        public Vampire Init(Creature newCreature, int startingLevel, float startingXP)
        {
            creature = newCreature;
            level.current = startingLevel;
            xp = startingXP;

            VampireCreatedEvent newVampire = createdEvent;
            if (newVampire != null)
                newVampire(this);

            WriteSave();
            return this;
        }

        public void EarnXP(float amount)
        {
            xp += amount;

            float required = GetXPRequirement(level.current);
            int levelUpAmount = 0;
            while (xp >= required) 
            {
                xp -= required;
                levelUpAmount += 1;
                required = GetXPRequirement(level.current + levelUpAmount);
            }

            if (levelUpAmount > 0) LevelUp(levelUpAmount);
            else WriteSave();

            // LOCAL FUNCTION
            float GetXPRequirement(int levelAmount) => (50.0f * Mathf.Pow(level.current, 2.0f)) + (20.0f * level.current);
        }

        public void LevelUp(int amount = 1)
        {
            level.current += amount;
            WriteSave();

            VampireLevelEvent vampireLevel = levelEvent;
            if (vampireLevel != null) vampireLevel(this, level.current);

        }

        public void WriteSave()
        {
            if (!isPlayer) return;

            VampireSaveData saveData = new VampireSaveData()
            {
                isVampire = true,
                level = level.current,
                xp = this.xp,
            };

            VampireManager.Instance.SaveData = saveData;
        }

        public delegate void VampireCreatedEvent(Vampire vampire);
        public delegate void VampireLevelEvent(Vampire vampire, int level);
    }
}
