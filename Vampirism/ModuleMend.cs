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
    public class ModuleMend : VampireModule
    {
        public static SkillMend skill;
        
        private Coroutine leftHandRoutine;
        private Coroutine rightHandRoutine;
        
        protected override void Awake()
        {
            base.Awake();

            PlayerControl.local.OnButtonPressEvent -= new PlayerControl.ButtonEvent(OnButtonPress);
            PlayerControl.local.OnButtonPressEvent += new PlayerControl.ButtonEvent(OnButtonPress);
        }

        protected override void OnDestroy()
        {
            PlayerControl.local.OnButtonPressEvent -= new PlayerControl.ButtonEvent(OnButtonPress);
            if (leftHandRoutine != null)
                StopCoroutine(leftHandRoutine);
            if (rightHandRoutine != null)
                StopCoroutine(rightHandRoutine);

            base.OnDestroy();
        }

        private void OnButtonPress(PlayerControl.Hand hand, PlayerControl.Hand.Button button, bool pressed)
        {
            // Mend routine should only start when the alt use button is pressed
            if (button != PlayerControl.Hand.Button.AlternateUse) return;

            Side handSide = hand.side;

            // Perform if the alternate use button is released
            if (!pressed)
            {
                // End the mend routine on the given side if it is not already ended
                Coroutine stopRoutine = handSide == Side.Left ? leftHandRoutine : rightHandRoutine;
                if (stopRoutine != null)
                    StopCoroutine(stopRoutine);
                return;
            }

            // Perform below code if the alt use button is being pressed

            Creature playerCreature = Vampire?.Creature;
            if (Utils.CheckError(() => playerCreature == null, "Mend module is not attached to a vampire or creature") || Utils.CheckError(() => !playerCreature.isPlayer, "Mend module is attached to a creature that is not the player"))
                return;

            RagdollHand ragdollHand = handSide == Side.Left ? playerCreature.handLeft : playerCreature.handRight;
            Creature grabbedCreature = ragdollHand?.grabbedHandle?.GetComponentInParent<RagdollPart>()?.ragdoll?.creature ?? ragdollHand?.grabbedHandle?.GetComponentInChildren<RagdollPart>()?.ragdoll?.creature;
            if (Utils.CheckError(() => grabbedCreature == null, "Player creature of mend module is not grabbing another creature in the " + handSide.ToString() + " hand"))
                return;

            Coroutine newRoutine = StartCoroutine(MendRoutine(hand, grabbedCreature));
            if (handSide == Side.Left)
                leftHandRoutine = newRoutine;
            else
                rightHandRoutine = newRoutine;
        }

        private IEnumerator MendRoutine(PlayerControl.Hand hand, Creature target)
        {
            while (hand != null && target != null && Vampire?.Creature != null)
            {
                if (!hand.gripPressed || !hand.alternateUsePressed) break;

                if (target.isKilled || target.currentHealth >= target.maxHealth)
                {
                    yield return new WaitForSeconds(skill.mendInterval);
                    continue;
                }

                Creature mender = Vampire.Creature;
                mender.Damage(skill.damageToHealer);

                float healAmount = skill.healingToTarget;
                if (target.IsVampire(out Vampire vampire) && vampire.Sire == Vampire)
                    healAmount *= 2.0f;
                target.Heal(healAmount);

                yield return new WaitForSeconds(skill.mendInterval);
            }
        }
    }
}
