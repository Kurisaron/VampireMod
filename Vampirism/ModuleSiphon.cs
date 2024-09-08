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
        private Coroutine coroutine;

        public static SkillSiphon skill;

        bool sfxPlaying = false;
        EffectInstance sfxInstance = null;

        public static event SiphonEvent siphonEvent;

        protected override void Awake()
        {
            base.Awake();

            coroutine = StartCoroutine(SiphonRoutine());
        }

        protected override void OnDestroy()
        {
            StopCoroutine(coroutine);

            base.OnDestroy();
        }

        private IEnumerator SiphonRoutine()
        {
            while (Vampire != null && Vampire.Creature != null)
            {
                SiphonUpdate();
                yield return new WaitForSeconds(skill.siphonInterval);
            }

            if (sfxInstance != null)
                sfxInstance.Despawn();
        }

        private void SiphonUpdate()
        {
            Creature target = FindTarget();
            if (target != null)
                Siphon(Vampire, target);

            SiphonSFX(target != null);
        }

        public static void Siphon(
            Vampire source, 
            Creature target, 
            float? overrideDamage = null,
            bool applyDamage = true,
            bool applyHeal = true,
            bool applyPower = true,
            bool triggerEvent = true)
        {
            float damage = source.Power > 0 ? GetDamage(source, target) : 0.0f;
            if (overrideDamage.HasValue)
                damage = overrideDamage.Value;

            if (applyDamage)
                target.Damage(damage, DamageType.Energy);
            if (applyHeal)
                source?.Creature?.Heal(damage);
            if (applyPower)
                source.GainPower(damage);

            if (triggerEvent)
            {
                SiphonEvent siphon = siphonEvent;
                if (siphon != null)
                    siphon(source, target, damage);
            }
            
        }

        private static float GetDamage(Vampire source, Creature target)
        {
            float powerScale = source.Power / 12345.0f;
            float percentage = Mathf.LerpUnclamped(0.05f, 0.25f, powerScale);
            return target.maxHealth * percentage;
        }

        private Creature FindTarget()
        {
            Vector3 sourcePosition = Vampire.Creature.jaw.position;

            // Find all colliders within siphon range that belong to a creature that is not the siphoner
            List<Collider> targetColliders = Physics.OverlapSphere(sourcePosition, Vampire.Creature.mouthRelay.mouthRadius * 5.0f).ToList().FindAll(collider =>
            {
                Creature creature = collider?.gameObject?.GetComponentInParent<RagdollPart>()?.ragdoll?.creature;

                return creature != null && creature != Vampire.Creature && !creature.isKilled;
            });

            if (targetColliders.Count <= 0)
                return null;

            // Determine which collider is closest to the siphoner's mouth
            targetColliders.SortByDistance(sourcePosition);

            // DEBUG: Log the distances of the target colliders
            for (int i = 0; i < targetColliders.Count; i++)
            {
                Collider collider = targetColliders[i];
                if (collider == null)
                {
                    Debug.LogWarning("Collider " + i.ToString() + " is null");
                    continue;
                }

                float distance = Vector3.Distance(collider.transform.position, sourcePosition);
                Debug.Log("Collider " + i.ToString() + " is " + distance.ToString() + " distance units away from siphon source");
            }

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
                sfxInstance = skill.siphonEffectData.Spawn(Vampire.Creature.ragdoll.parts.Find(part => part.type == RagdollPart.Type.Head).transform, false, null, Vampire.Creature.isPlayer);
                sfxInstance.Play();
            }
            else
            {
                sfxInstance.Stop();
                sfxInstance.Despawn();
            }
        }

        public delegate void SiphonEvent(Vampire source, Creature target, float damage);
    }
}
