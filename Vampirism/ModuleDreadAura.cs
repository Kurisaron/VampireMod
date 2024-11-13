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
    public class ModuleDreadAura : VampireModule
    {
        public override string GetSkillID() => "DreadAura";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);
        }

        public override void ModuleUnloaded()
        {
            base.ModuleUnloaded();
        }

        public override IEnumerator ModulePassive()
        {
            SkillDreadAura dreadAuraSkill = GetSkill<SkillDreadAura>();
            while (moduleVampire != null && dreadAuraSkill != null)
            {
                DreadUpdate(dreadAuraSkill);
                yield return new WaitForSeconds(dreadAuraSkill.auraInterval);
            }
        }

        private void DreadUpdate(SkillDreadAura dreadAuraSkill)
        {
            if (moduleVampire?.Creature == null) return;

            // A creature can be a target for the dread aura if creature is not null, creature is not the module vampire, either the creature is not a vampire or it is not the spawn of the module vampire
            List<Creature> targets = Creature.allActive.FindAll(creature => creature != null && creature != moduleVampire.Creature && (!creature.IsVampire(out Vampire spawn) || spawn.sireline.Sire != moduleVampire));
            if (targets == null || targets.Count == 0) return;

            List<Creature> nearTargets = targets.FindAll(creature => Vector3.Distance(creature.transform.position, moduleVampire.Creature.transform.position) < dreadAuraSkill.auraRange);
            if (nearTargets == null || nearTargets.Count == 0) return;

            foreach (Creature target in nearTargets)
            {
                if (target == null || target.isKilled)
                    continue;
                
                BrainModuleFear fearModule = target?.brain?.instance?.GetModule<BrainModuleFear>();
                if (fearModule == null) continue;

                // Do not attempt to cause panic if it is already present
                if (fearModule.isCowering) continue;

                // Creatures within the range only have a chance to be made to panic
                float percentage = UnityEngine.Random.Range(0.0f, 100.0f); // Generate the percentage value to query against the chance percentage of fear
                float chanceToPanic = dreadAuraSkill.basePanicChance + Mathf.Lerp(0.0f, dreadAuraSkill.maxPanicChance - dreadAuraSkill.basePanicChance, Mathf.InverseLerp(0.0f, dreadAuraSkill.auraPowerScaleMax, moduleVampire.power.PowerLevel)); // Generate the chance percentage for dread aura to cause panic, based on the power level of the module vampire
                if (percentage <= chanceToPanic)
                    fearModule.Panic();
            }
        }
    }
}
