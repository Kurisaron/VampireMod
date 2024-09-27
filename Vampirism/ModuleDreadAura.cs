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
        private Coroutine coroutine;

        public static SkillDreadAura skill;
        
        protected override void Awake()
        {
            base.Awake();

            coroutine = StartCoroutine(DreadRoutine());
        }

        protected override void OnDestroy()
        {
            StopCoroutine(coroutine);
            
            base.OnDestroy();
        }

        private IEnumerator DreadRoutine()
        {
            while (true)
            {
                DreadUpdate();
                yield return new WaitForSeconds(skill.auraInterval);
            }
        }

        private void DreadUpdate()
        {
            if (Vampire?.Creature == null) return;

            // A creature can be a target for the dread aura if creature is not null, creature is not the module vampire, either the creature is not a vampire or it is not the spawn of the module vampire
            List<Creature> targets = Creature.allActive.FindAll(creature => creature != null && creature != Vampire.Creature && (!creature.IsVampire(out Vampire spawn) || spawn.Sire != Vampire));
            if (targets == null || targets.Count == 0) return;

            List<Creature> nearTargets = targets.FindAll(creature => Vector3.Distance(creature.transform.position, Vampire.Creature.transform.position) < skill.auraRange);
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
                float chanceToPanic = skill.basePanicChance + Mathf.Lerp(0.0f, skill.maxPanicChance - skill.basePanicChance, Mathf.InverseLerp(0.0f, skill.auraPowerScaleMax, Vampire.Power)); // Generate the chance percentage for dread aura to cause panic, based on the power level of the module vampire
                if (percentage <= chanceToPanic)
                    fearModule.Panic();
            }
        }
    }
}
