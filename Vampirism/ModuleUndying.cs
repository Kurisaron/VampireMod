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

            Creature moduleCreature = moduleVampire?.creature;
            if (moduleCreature != null)
            {
                moduleCreature.OnDamageEvent -= new Creature.DamageEvent(OnCreatureHit);
                moduleCreature.OnDamageEvent += new Creature.DamageEvent(OnCreatureHit);
            }
        }
        public override void ModuleUnloaded()
        {
            Creature moduleCreature = moduleVampire?.creature;
            if (moduleCreature != null)
            {
                moduleCreature.OnDamageEvent -= new Creature.DamageEvent(OnCreatureHit);
            }
            
            base.ModuleUnloaded();
        }
        public override IEnumerator ModulePassive()
        {
            SkillUndying undyingSkill = GetSkill<SkillUndying>();

            while (moduleVampire?.creature != null && undyingSkill != null)
            {
                Creature creature = moduleVampire.creature;
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
            if (moduleVampire?.creature == null)
                return;
            moduleVampire.creature.Heal(healAmount);
            currentHealthPool -= healAmount;
            if (currentHealthPool < 0)
                currentHealthPool = 0;
        }

        
    }
}
