using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public class SkillSiphon : SkillData
    {
        // JSON fields
        public float siphonInterval = 0.1f;


        public static event SiphonEvent siphonEvent;
        
        public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();

        }

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            if (creature == null)
                return;

            Vampire vampire = null;
            if (!creature.IsVampire(out vampire))
                vampire = creature.Vampirize();

            vampire.StartCoroutine(SiphonRoutine(vampire));
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            /*
            Vampire vampire = null;
            if (!creature.IsVampire(out vampire))
                vampire = creature.Vampirize();

            vampire.StopCoroutine(SiphonRoutine(vampire));
            */
        }

        /// <summary>
        /// Coroutine used for vampires' siphon ability. One coroutine instance exists per vampire per level
        /// </summary>
        /// <param name="vampire"></param>
        /// <returns></returns>
        private IEnumerator SiphonRoutine(Vampire vampire)
        {
            bool sfxPlaying = false;

            string sfx_ID = "siphonFX";
            //EffectInstance sfxInstance = Catalog.GetData<EffectData>(sfx_ID).Spawn(vampire.Creature.ragdoll.parts.Find(part => part.type == RagdollPart.Type.Head).transform, false, null, vampire.Creature.isPlayer);

            while (vampire != null && vampire.Creature != null)
            {
                Siphon();
                yield return new WaitForSeconds(siphonInterval);
            }

            /*
            if (sfxInstance != null)
                sfxInstance.Despawn();
            */
            
            //////////////////////
            // End of coroutine //
            //////////////////////


            ////////////////////////////////////////////////////
            // Local functions for Siphon Coroutine to re-use //
            ////////////////////////////////////////////////////
            
            void Siphon()
            {
                Creature target = FindSiphonTarget();
                if (target != null)
                    SkillSiphon.Siphon(vampire, target);

                //SiphonSFX(target);
            }

            Creature FindSiphonTarget()
            {
                Vector3 sourcePosition = vampire.Creature.jaw.position;

                // Find all colliders within siphon range that belong to a creature that is not the siphoner
                List<Collider> targetColliders = Physics.OverlapSphere(sourcePosition, vampire.Creature.mouthRelay.mouthRadius).ToList().FindAll(collider =>
                {
                    RagdollPart ragdollPart;
                    if (ragdollPart = collider.gameObject.GetComponentInParent<RagdollPart>())
                    {
                        Ragdoll ragdoll;
                        if (ragdoll = ragdollPart.ragdoll)
                        {
                            Creature creature;
                            if (creature =  ragdoll.creature)
                            {
                                if (creature != vampire.Creature && !creature.isKilled)
                                    return true;
                            }
                        }
                    }

                    return false;
                });

                if (targetColliders.Count <= 0)
                    return null;

                // Determine which collider is closest to the siphoner's mouth
                try
                {
                    targetColliders.Sort((a, b) =>
                    {
                        if ((a == null && b == null) || a == b) return 0;
                        if (a == null) return -1;
                        if (b == null) return 1;

                        float aDistance = Vector3.Distance(a.transform.position, sourcePosition);
                        float bDistance = Vector3.Distance(b.transform.position, sourcePosition);

                        if (aDistance == bDistance) return 0;
                        if (aDistance < bDistance) return -1;
                        return 1;

                    });
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Exception occured during siphon collider sort");
                    return null;
                }

                // DEBUG: Log the distances of the target colliders
                for (int i = 0;  i < targetColliders.Count; i++)
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

            /*
            void SiphonSFX(bool play)
            {
                if (sfxPlaying == play) return;

                sfxPlaying = play;
                if (play)
                    sfxInstance.Play();
                else
                    sfxInstance.Stop();
            }
            */

        }

        public static void Siphon(Vampire source, Creature target, float? overrideDamage = null, bool applyDamage = true)
        {
            float levelScale = source.LevelScale;
            float siphonDamage = source.CurrentLevel <= 0 ? 0.0f : Mathf.LerpUnclamped(5f, 10f, levelScale) * target.maxHealth;
            if (overrideDamage.HasValue)
                siphonDamage = overrideDamage.Value;

            if (applyDamage)
                target.Damage(siphonDamage, DamageType.UnBlockable);
            source.Creature.Heal(siphonDamage);
            source.EarnXP(overrideDamage.HasValue ? overrideDamage.Value * 0.1f : 5f);

            SiphonEvent siphon = siphonEvent;
            if (siphon != null)
                siphon(source, target, siphonDamage);
        }

        public delegate void SiphonEvent(Vampire source, Creature target, float damage);
    }
}
