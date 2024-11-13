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
        private float currentHealthPool;
        private float MaxHealthPool
        {
            get
            {
                SkillUndying undyingSkill = GetSkill<SkillUndying>();
                if (moduleVampire == null || undyingSkill == null)
                    return 0;

                return undyingSkill.clampPool ? Mathf.Lerp(undyingSkill.poolScale.x, undyingSkill.poolScale.y, moduleVampire.power.PowerLevel / undyingSkill.powerAtPoolMax) : Mathf.LerpUnclamped(undyingSkill.poolScale.x, undyingSkill.poolScale.y, moduleVampire.power.PowerLevel / undyingSkill.powerAtPoolMax);
            }
        }

        public override string GetSkillID() => "UndyingWill";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            Creature moduleCreature = moduleVampire?.Creature;
            if (moduleCreature != null)
            {
                moduleCreature.OnDamageEvent -= new Creature.DamageEvent(OnCreatureHit);
                moduleCreature.OnDamageEvent += new Creature.DamageEvent(OnCreatureHit);
            }
        }
        public override void ModuleUnloaded()
        {
            Creature moduleCreature = moduleVampire?.Creature;
            if (moduleCreature != null)
            {
                moduleCreature.OnDamageEvent -= new Creature.DamageEvent(OnCreatureHit);
            }
            
            base.ModuleUnloaded();
        }
        public override IEnumerator ModulePassive()
        {
            SkillUndying undyingSkill = GetSkill<SkillUndying>();

            while (moduleVampire?.Creature != null && undyingSkill != null)
            {
                if (!undyingSkill.performRegen)
                {
                    yield return new WaitForSeconds(undyingSkill.regenInterval);
                    continue;
                }
                
                Creature creature = moduleVampire.Creature;
                if (creature.currentHealth >= creature.maxHealth && currentHealthPool >= MaxHealthPool)
                {
                    yield return new WaitForSeconds(undyingSkill.regenInterval);
                    continue;
                }
                float regenMultiplier = undyingSkill.clampRegen ? Mathf.Lerp(undyingSkill.regenPowerScale.x, undyingSkill.regenPowerScale.y, moduleVampire.power.PowerLevel / undyingSkill.powerAtRegenMax) : Mathf.LerpUnclamped(undyingSkill.regenPowerScale.x, undyingSkill.regenPowerScale.y, moduleVampire.power.PowerLevel / undyingSkill.powerAtRegenMax);
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
                yield return new WaitForSeconds(undyingSkill.regenInterval);
            }
        }
        
        private void OnCreatureHit(CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart || collisionInstance == null) return;
            float damageDone = collisionInstance.damageStruct.damage;
            float healAmount = Mathf.Min(damageDone, currentHealthPool);
            if (moduleVampire?.Creature == null)
                return;
            moduleVampire.Creature.Heal(healAmount);
            currentHealthPool -= healAmount;
            if (currentHealthPool < 0)
                currentHealthPool = 0;
        }

        
    }
}
