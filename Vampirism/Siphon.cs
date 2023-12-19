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
    public class Siphon : Ability
    {
        // VARIABLES
        public float checkInterval = 0.2f;

        public bool siphonFXActive;
        public EffectInstance siphonFX;
        public string siphonFX_ID = "siphonFX";

        public Creature mendTarget;

        // FUNCTIONS
        public Siphon() : base(new AbilityStats(
            "Siphon",
            "Core",
            0,
            new (string description, int unlockCost, Func<bool> unlockConditions)[]
            {
                ("Put your mouth near other creatures to suck their blood, absorbing their vital essence. Sucking blood damages the other creature, heals you, and earns XP towards leveling up your vampiric power", 0, () => false)
            }))
        {
            
        }

        public override void SetupHandler() => SetupHandler<Siphon>();

        public override IEnumerator PassiveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkInterval);

                if (Utils.CheckError(() => VampireMaster.Instance == null, "Instance of VampireMaster is null") ||
                    Utils.CheckError(() => VampireMaster.Instance.Progression == null, "Instance of progression tracker on instance of VampireMaster is null") ||
                    Utils.CheckError(() => !VampireMaster.Instance.Progression.isVampire, "Player is not a vampire"))
                    continue;

                SiphonCheck();
            }
        }

        private void SiphonCheck()
        {
            if (VampireMaster.Instance.PlayerCreature != null)
            {
                VampireMaster.local.CheckCurrentThralls();

                Vector3 siphonOrigin = VampireMaster.local.PlayerJaw().position;

                // Check for a creature close(st) to the jaw
                //Debug.Log("VampireControl: Checking for colliders close to jaw...");
                Collider[] colliderArray = Physics.OverlapSphere(siphonOrigin, VampireMaster.local.siphonRange);

                Collider closestCollider = null;
                float closestColliderDistance = VampireMaster.local.siphonRange;

                if (colliderArray != null && colliderArray.Length > 0)
                {
                    //Debug.Log("VampireControl: Collider(s) found near jaw. Checking if collider(s) belong to creature...");
                    int debugIndex = 0;

                    foreach (Collider collider in colliderArray)
                    {
                        // Check if current collider has a RagdollPart and Ragdoll attached to it
                        if (collider.gameObject.GetComponentInParent<RagdollPart>() != null && collider.gameObject.GetComponentInParent<RagdollPart>().ragdoll != null && collider.gameObject.GetComponentInParent<RagdollPart>().ragdoll.creature != null && collider.gameObject.GetComponentInParent<RagdollPart>().ragdoll.creature != Player.currentCreature && !collider.gameObject.GetComponentInParent<RagdollPart>().ragdoll.creature.isKilled)
                        {
                            // Current collider is attached to a living non-player creature
                            float colliderDistance = Vector3.Distance(siphonOrigin, collider.gameObject.transform.position);

                            if (closestColliderDistance > colliderDistance)
                            {
                                // Current collider is closest of those checked
                                closestCollider = collider;
                                closestColliderDistance = colliderDistance;

                            }
                            else
                            {
                                //Debug.Log("VampireControl: Collider at index " + debugIndex.ToString() + " is not closest of those checked. NEXT COLLIDER");
                            }

                        }
                        else
                        {
                            //Debug.Log("VampireControl: Collider at index " + debugIndex.ToString() + " is not attached to a non-player creature. NEXT COLLIDER");
                        }

                        debugIndex++;

                    }
                }
                else
                {
                    //Debug.Log("VampireControl: No colliders found near jaw.");
                }

                // Siphon the creature closest to the jaw (if any)
                if (closestCollider != null)
                {
                    SiphonBlood(closestCollider);

                    SiphonSFX(true);

                }
                else
                {
                    SiphonSFX(false);
                }

            }

        }

        public void SiphonBlood(Collider targetCollider)
        {
            // Player is siphoning from a creature
            Creature target = targetCollider.gameObject.GetComponentInParent<RagdollPart>().ragdoll.creature;

            float siphonHealthAmount = VampireMaster.local.siphonDamagePercent * target.maxHealth;

            SuckTarget(target, siphonHealthAmount);

            if (!target.isKilled)
            {
                // Target creature is not dead
                TurnTarget(target);

            }
            else
            {
                // Target creature is dead

            }

            EmpowerPlayer(siphonHealthAmount);

        }

        public static void SuckTarget(Creature target, float damage)
        {
            // Damage target
            CollisionInstance collisionInstance = new CollisionInstance(new DamageStruct(DamageType.Energy, damage));
            
            target.Damage(collisionInstance);
            //Debug.Log("SiphonHandler: SuckTarget complete");
        }

        public void TurnTarget(Creature target)
        {
            // Vampirize if target is not already allied
            //Debug.Log("SiphonHandler: TurnTarget started");
            if (VampireMaster.local.vampireThralls.Count < VampireMaster.local.maxThralls && !VampireMaster.local.vampireThralls.Contains(target))
            {
                //Debug.Log("SiphonHandler: Target to be turned");
                
                // Set to ally faction
                target.SetFaction(VampireMaster.local.playerCreature.factionId);
                target.brain.Load(target.brain.instance.id);
                target.brain.canDamage = true;

                // Set thrall's eyes
                VampireMaster.local.SetCreatureVampireEyes(target);

                // Set thrall's skin
                //VampireLevel.local.SetVampireSkin(target, true);

                // Add Mend event to thrall
                /*
                foreach (RagdollPart ragdollPart in target.ragdoll.parts)
                {
                    ragdollPart.OnHeldActionEvent += new RagdollPart.HeldActionDelegate(OnMendAction);
                }
                */

                // Add thrall to list
                VampireMaster.local.vampireThralls.Add(target);

                Debug.Log("SiphonHandler: Target has been turned");
            }
            else
            {
                //Debug.Log("SiphonHandler: Thrall count is " + VampireLevel.local.vampireThralls.Count.ToString() + ", max thrall count is " + VampireLevel.local.maxThralls);
            }
            //Debug.Log("SiphonHandler: TurnTarget complete");
        }

        public void EmpowerPlayer(float heal)
        {
            if (VampireMaster.local.playerCreature == null) return;

            //Debug.Log("VampireControl: Empowering player...");

            // Heal player
            VampireMaster.local.playerCreature.Heal(heal, VampireMaster.local.playerCreature);

            if (VampireMaster.local.playerCreature.currentHealth > VampireMaster.local.playerCreature.maxHealth)
            {
                VampireMaster.local.playerCreature.currentHealth = VampireMaster.local.playerCreature.maxHealth;
            }

            float experience = 5.0f;
            experience = Mathf.Pow(heal, 0.5f) * 5.0f;

            // Provide XP
            VampireMaster.local.GainXP(experience);

        }


        public void SiphonSFX(bool active)
        {
            if (siphonFXActive == active) return;

            siphonFXActive = active;
            if (siphonFXActive)
            {
                siphonFX.SetIntensity(5f);
                siphonFX.Play();
            }
            else
            {
                siphonFX.Stop();
            }
            Debug.Log("SiphonHandler: Siphon now " + (siphonFXActive ? "" : "not ") + "active");
        }

        // MEND FUNCTIONS

        public void OnMendAction(RagdollHand ragdollHand, HandleRagdoll handle, Interactable.Action action)
        {
            if (VampireMaster.local.abilityLevels[VampireAbilityEnum.Mend] <= 0) return;

            switch(action)
            {
                case Interactable.Action.AlternateUseStart:
                    // Do stuff for when the alt use button has started to be pressed
                    mendTarget = handle.ragdollPart.ragdoll.creature;
                    break;
                case Interactable.Action.AlternateUseStop:
                    // Do stuff for when the alt use button has stopped being pressed
                    mendTarget = null;
                    break;
            }
        }

        public void MendCheck()
        {
            if (mendTarget != null && VampireMaster.local.abilityLevels[VampireAbilityEnum.Mend] >= 1)
            {
                float mendHealthAmount = VampireMaster.local.siphonDamagePercent * VampireMaster.local.playerCreature.maxHealth;

                if (mendTarget.currentHealth < mendHealthAmount)
                {
                    mendHealthAmount = mendTarget.currentHealth;
                }

                VampireMaster.local.playerCreature.Damage(new CollisionInstance(new DamageStruct(DamageType.Energy, mendHealthAmount)));
                mendTarget.Heal(mendHealthAmount, VampireMaster.local.playerCreature);
            }
        }
    }
}
