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

        private Dictionary<Ability, int> abilities;
        private List<Coroutine> passiveRoutines;

        public static VampireCreatedEvent createdEvent;
        public static VampireLevelEvent levelEvent;

        /// <summary>
        /// Only for use in extension Creature.Vampirize with AddComponent/GetComponent to serve as pseudo-constructor
        /// </summary>
        /// <param name="newCreature">Creature being made into a vampire</param>
        /// <param name="startingLevel">The vampire's starting level</param>
        /// <param name="startingXP">The vampire's starting xp</param>
        /// <param name="startingPoints">The vampire's starting amount of skill points</param>
        /// <param name="restrictedAbilities">A dictionary defining the available abilities and their levels. If null, the vampire will have all possible abilities at their base levels</param>
        /// <returns></returns>
        public Vampire Init(Creature newCreature, int startingLevel, float startingXP, int startingPoints, Dictionary<Ability, int> abilitySet)
        {
            creature = newCreature;
            level.current = startingLevel;
            xp = startingXP;
            skillPoints = startingPoints;

            // Setup abilities for vampire
            abilities = new Dictionary<Ability, int>();
            passiveRoutines = new List<Coroutine>();
            Dictionary<Ability, int> abilityLevels = abilitySet ?? VampireManager.Instance.DefaultAbilities;
            if (Utils.CheckError(() => abilityLevels == null, "Dictionary being used for ability levels is null (Vampire.Init)")) return this;
            
            foreach (KeyValuePair<Ability, int> abilityEntry in abilityLevels)
            {
                Ability ability = abilityEntry.Key;
                if (Utils.CheckError(() => ability == null, "Ability in dictionary is null (Vampire.Init)")) continue;

                abilities.Add(ability, abilityEntry.Value);

                if (isPlayer)
                {
                    VampireSaveData saveData = VampireManager.Instance.SaveData;
                    if (Utils.CheckError(() => saveData == null, "Save data in manager is null (Vampire.Init)")) continue;

                    string typeName = ability.GetType().Name;
                    if (saveData.abilityLevels.TryGetValue(typeName, out int value)) abilities[ability] = value;
                }

                ability.SetupAbility(this);
                passiveRoutines.Add(StartCoroutine(ability.PassiveCoroutine()));
            }

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
                skillPoints = this.skillPoints,
                abilityLevels = new Dictionary<string, int>()
            };

            foreach (KeyValuePair<Ability, int> ability in abilities)
            {
                saveData.abilityLevels.Add(ability.Key.GetType().Name, ability.Value);
            }

            VampireManager.Instance.SaveData = saveData;
        }

        public delegate void VampireCreatedEvent(Vampire vampire);
        public delegate void VampireLevelEvent(Vampire vampire, int level);
    }
}
