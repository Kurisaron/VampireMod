using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleUndying : VampireModule
    {
        public static SkillUndying skill;
        
        private float currentHealthPool;
        private float MaxHealthPool
        {
            get
            {
                if (Vampire == null)
                    return 0;

                return skill.clampPool ? Mathf.Lerp(skill.poolScale.x, skill.poolScale.y, Vampire.Power / skill.powerAtPoolMax) : Mathf.LerpUnclamped(skill.poolScale.x, skill.poolScale.y, Vampire.Power / skill.powerAtPoolMax);
            }
        }

        private Coroutine coroutine;
        
        protected override void Awake()
        {
            base.Awake();

            Creature creature = Vampire?.Creature;
            if (creature == null)
                return;

            coroutine = StartCoroutine(RegenRoutine());

            creature.OnDamageEvent -= new Creature.DamageEvent(OnCreatureHit);
            creature.OnDamageEvent += new Creature.DamageEvent(OnCreatureHit);
        }

        protected override void OnDestroy()
        {
            if (coroutine != null)
                StopCoroutine(coroutine);

            Creature creature = Vampire?.Creature;
            if (creature == null)
                return;

            creature.OnDamageEvent -= new Creature.DamageEvent(OnCreatureHit);

            base.OnDestroy();
        }

        private IEnumerator RegenRoutine()
        {
            while (Vampire?.Creature != null)
            {
                Creature creature = Vampire.Creature;
                if (creature.currentHealth >= creature.maxHealth && currentHealthPool >= MaxHealthPool)
                {
                    yield return new WaitForSeconds(skill.regenInterval);
                    continue;
                }

                float regenMultiplier = skill.clampRegen ? Mathf.Lerp(skill.regenPowerScale.x, skill.regenPowerScale.y, Vampire.Power / skill.powerAtRegenMax) : Mathf.LerpUnclamped(skill.regenPowerScale.x, skill.regenPowerScale.y, Vampire.Power / skill.powerAtRegenMax);
                float regenAmount = creature.maxHealth * regenMultiplier;
                float healthNeeded = creature.maxHealth - creature.currentHealth;
                if (healthNeeded > 0.0f)
                {
                    float healAmount = Math.Min(regenAmount, healthNeeded);

                    creature.Heal(healAmount);
                    regenAmount -= healAmount;
                }

                float poolNeeded = MaxHealthPool - currentHealthPool;
                currentHealthPool += Mathf.Min(regenAmount, poolNeeded);

                yield return new WaitForSeconds(skill.regenInterval);
            }
        }

        private void OnCreatureHit(CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart || collisionInstance == null) return;

            float damageDone = collisionInstance.damageStruct.damage;
            float healAmount = Mathf.Min(damageDone, currentHealthPool);

            if (Vampire?.Creature == null)
                return;

            Vampire.Creature.Heal(healAmount);
            currentHealthPool -= healAmount;
            if (currentHealthPool < 0)
                currentHealthPool = 0;
        }

    }
}
