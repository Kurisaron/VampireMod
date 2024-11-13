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
    public class ModuleSiphon : VampireModule
    {
        public bool IsSiphoning { get; private set; }

        private bool sfxPlaying = false;
        private EffectInstance sfxInstance = null;

        public override string GetSkillID() => "Siphon";

        public override IEnumerator ModulePassive()
        {
            //Debug.Log(GetDebugPrefix(nameof(ModulePassive)) + " Siphon module passive routine started");
            SkillSiphon skillSiphon = GetSkill<SkillSiphon>();
            
            while (moduleVampire?.Creature != null && skillSiphon != null)
            {
                //Debug.Log(GetDebugPrefix(nameof(ModulePassive)) + " Siphon module passive tick start");

                SiphonUpdate();

                //Debug.Log(GetDebugPrefix(nameof(ModulePassive)) + " Siphon module passive tick end");
                bool hasTemporalSiphon = moduleVampire.Creature.HasSkill("TemporalSiphon");
                if (hasTemporalSiphon)
                    yield return new WaitForSecondsRealtime(skillSiphon.siphonInterval);
                else
                    yield return new WaitForSeconds(skillSiphon.siphonInterval);
                
            }

            if (moduleVampire?.Creature == null)
                Debug.LogError(GetDebugPrefix(nameof(ModulePassive)) + " Module vampire/creature is null");

            if (skillSiphon == null)
                Debug.LogError(GetDebugPrefix(nameof(ModulePassive)) + " Siphon skill is null");

            if (sfxInstance != null)
                sfxInstance.Despawn();

            //Debug.Log(GetDebugPrefix(nameof(ModulePassive)) + " Siphon module passive routine ended");
        }

        private void SiphonUpdate()
        {
            Creature target = FindTarget();
            IsSiphoning = target != null;
            if (IsSiphoning)
                Siphon(moduleVampire, target);

            SiphonSFX(IsSiphoning);
        }

        public void Siphon(
            Vampire source, 
            Creature target, 
            float? overrideDamage = null,
            bool applyDamage = true,
            bool applyHeal = true,
            bool applyPower = true,
            bool triggerEvent = true)
        {
            float damage = source?.power?.PowerLevel > 0 ? GetDamage(source, target) : 0.0f;
            if (overrideDamage.HasValue)
                damage = overrideDamage.Value;

            if (applyDamage)
                target?.Damage(damage, DamageType.Energy);
            if (applyHeal)
                source?.Creature?.Heal(damage);
            if (applyPower)
                source?.power?.GainPower(damage);

            if (triggerEvent)
            {
                VampireEvents.Instance?.InvokeSiphonEvent(source, target, damage);
            }
            
        }

        private float GetDamage(Vampire source, Creature target)
        {
            SkillData skillData = GetSkill();
            if (skillData == null) return 0.0f;

            if (skillData is SkillSiphon siphonSkill)
            {
                float powerScale = source.power.PowerLevel / siphonSkill.powerAtSiphonPowerMax;
                float percentage = siphonSkill.clampSiphonPower ? Mathf.Lerp(siphonSkill.siphonPowerScale.x, siphonSkill.siphonPowerScale.y, powerScale) : Mathf.LerpUnclamped(siphonSkill.siphonPowerScale.x, siphonSkill.siphonPowerScale.y, powerScale);
                return target.maxHealth * percentage;
            }
            else
                return 0.0f;
           
        }

        private Creature FindTarget()
        {
            Vector3 sourcePosition = moduleVampire.Creature.jaw.position;

            SkillSiphon siphonSkill = GetSkill<SkillSiphon>();
            if (Utils.CheckError(() => siphonSkill == null, "No skill data present or skill is not siphon skill")) return null;

            // Find all colliders within siphon range that belong to a creature that is not the siphoner
            List<Collider> targetColliders = Physics.OverlapSphere(sourcePosition, moduleVampire.Creature.mouthRelay.mouthRadius * siphonSkill.siphonMouthRangeMult).ToList().FindAll(collider =>
            {
                Creature creature = collider?.gameObject?.GetComponentInParent<RagdollPart>()?.ragdoll?.creature;

                return creature != null && creature != moduleVampire.Creature && !creature.isKilled;
            });

            if (targetColliders.Count <= 0)
                return null;

            // Determine which collider is closest to the siphoner's mouth
            targetColliders.SortByDistance(sourcePosition);

            Collider targetCollider = targetColliders[0];
            if (targetCollider == null)
                return null;

            return targetCollider.gameObject.GetComponentInParent<RagdollPart>().ragdoll.creature;
        }

        private void SiphonSFX(bool play)
        {
            if (sfxPlaying == play) return;

            sfxPlaying = play;
            if (play)
            {
                SkillSiphon siphonSkill = GetSkill<SkillSiphon>();
                if (siphonSkill == null) return;

                sfxInstance = siphonSkill.siphonEffectData.Spawn(moduleVampire.Creature.ragdoll.parts.Find(part => part.type == RagdollPart.Type.Head).transform, false, null, moduleVampire.Creature.isPlayer);
                sfxInstance.Play();
            }
            else
            {
                if (sfxInstance == null) return;
                sfxInstance.Stop();
                sfxInstance.Despawn();
            }
        }

    }
}
