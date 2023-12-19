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
    public class SyringeComponent : MonoBehaviour
    {
        // VARIABLES
        public Item item;
        public Creature piercedCreature;
        public bool isPiercingCreature { get => piercedCreature != null; }
        public bool isUsed;
        public Animator syringeAnimator;

        public Transform liquidReference;
        public Material liquidMat;
        public float fillLevel;

        // FUNCTIONS
        public void Start()
        {
            item = GetComponent<Item>();

            isUsed = false;
            syringeAnimator = GetComponent<Animator>();
            syringeAnimator.SetBool("isUsed", isUsed);
            //Debug.Log("SyringeComponent: Animator has been set.");

            liquidReference = item.GetCustomReference("Liquid");
            liquidMat = liquidReference.gameObject.GetComponent<Renderer>().material;
            fillLevel = 1.0f;
            SetFill(fillLevel);
            //Debug.Log("SyringeComponent: Fill has been initialized.");

            piercedCreature = null;
            item.mainCollisionHandler.OnCollisionStartEvent += new CollisionHandler.CollisionEvent(MainCollisionHandler_OnCollisionStartEvent);
            item.OnHeldActionEvent += new Item.HeldActionDelegate(Item_OnHeldActionEvent);

            item.OnGrabEvent += new Item.GrabDelegate(Item_OnGrabEvent);
            item.OnHandleReleaseEvent += new Item.ReleaseDelegate(Item_OnHandleReleaseEvent);
            item.OnUngrabEvent += new Item.ReleaseDelegate(Item_OnUngrabEvent);
        }

        public void Update()
        {
            if (!item.isPenetrating && isPiercingCreature)
            {
                piercedCreature = null;
                //Debug.Log("SyringeComponent: Syringe is no longer stabbing player.");
            }
        }

        public void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            // Check if syringe is in player
            if (collisionInstance.damageStruct.hitRagdollPart != null && collisionInstance.damageStruct.hitRagdollPart.ragdoll != null)
            {
                piercedCreature = collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature;
                //Debug.Log("SyringeComponent: Syringe is stabbing creature.");
            }
            else
            {
                piercedCreature = null;
            }
        }

        public void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                if (isPiercingCreature && !isUsed)
                {
                    InjectCreature();
                }
                else
                {

                }
            }
        }

        public void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            //Debug.Log("SyringeComponent: Grab event activated.");

            if (ragdollHand != null && ragdollHand.ragdoll != null && ragdollHand.ragdoll.creature != null && ragdollHand.ragdoll.creature.isPlayer)
            {
                //Player.selfCollision = true;
                //Debug.Log("SyringeComponent: Syringe grabbed by player. Self collision has been activated.");
            }
            else
            {
                //Debug.Log("SyringeComponent: Syringe grabbed, but player could not be found.");
            }
        }

        public void Item_OnHandleReleaseEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            //Debug.Log("SyringeComponent: Handle release event activated.");
        }

        public void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            //Debug.Log("SyringeComponent: Ungrab event activated.");
        }

        public virtual void InjectCreature()
        {
            //Debug.Log("SyringeComponent: Injecting player...");

            isUsed = true;

            // Animate Syringe Plunger
            syringeAnimator.SetBool("isUsed", isUsed);
            //Debug.Log("SyringeComponent: Injection animation complete.");

            // Run Coroutine for Liquid Shader
            StartCoroutine(EmptyLiquid());
        }


        public IEnumerator EmptyLiquid()
        {
            //Debug.Log("SyringeComponent: Emptying liquid...");

            float changeInterval = 0.1f;
            float changeAmount = 0.05f;

            while (fillLevel > 0.0f)
            {
                fillLevel -= changeAmount;
                SetFill(fillLevel);

                yield return new WaitForSeconds(changeInterval);
            }

            fillLevel = 0.0f;
            SetFill(fillLevel);

            //Debug.Log("SyringeComponent: Liquid emptied.");
        }

        public void SetFill(float fill)
        {
            liquidMat.SetFloat("_FillLevel", fill);
        }
    }
}
