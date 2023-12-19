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
        
        private (int current, int max) level = (1, 50);
        private float xp;
        private int skillPoints;
        private bool isPlayer { get => creature != null && creature.isPlayer; }

        private Dictionary<Ability, int> abilities;

        public static VampireCreatedEvent createdEvent;
        public static VampireLevelEvent levelEvent;

        public Vampire Init(Creature newCreature, int startingLevel, float startingXP, int startingPoints)
        {
            creature = newCreature;
            level.current = startingLevel;
            xp = startingXP;
            skillPoints = startingPoints;

            // Setup abilities for vampire
            List<Ability> list = VampireManager.Instance.Abilities;
            if (list == null) Debug.LogError("No list of abilities active");
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Ability ability = list[i];
                    if (ability == null) continue;


                }
            }

            VampireCreatedEvent newVampire = createdEvent;
            if (newVampire != null)
                newVampire(this);

            return this;
        }

        public void EarnXP(float amount)
        {
            xp += amount;

            float required = GetXPRequirement();
            while (xp >= required) 
            {
                xp -= required;
                level.current += 1;
                required = GetXPRequirement();
            }

            VampireLevelEvent vampireLevel = levelEvent;
            if (vampireLevel != null) vampireLevel(this, level.current);



            float GetXPRequirement() => (50.0f * Mathf.Pow(level.current, 2.0f)) + (20.0f * level.current);
        }

        public void LevelUp(int amount = 1)
        {

        }

        public delegate void VampireCreatedEvent(Vampire vampire);
        public delegate void VampireLevelEvent(Vampire vampire, int level);
    }
}
