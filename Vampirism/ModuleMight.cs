using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleMight : VampireModule
    {
        public static SkillMight skill;
        private (RagdollHand left, RagdollHand right) hands;

        protected override void Awake()
        {
            base.Awake();

            SetMightModifier();

            Vampire.sireEvent -= new Vampire.SiredEvent(OnPowerGained);
            Vampire.sireEvent += new Vampire.SiredEvent(OnPowerGained);
            Vampire.powerGainedEvent -= new Vampire.PowerGainedEvent(OnPowerGained);
            Vampire.powerGainedEvent += new Vampire.PowerGainedEvent(OnPowerGained);

            Creature creature = Vampire?.Creature;
            if (creature == null) return;

            hands = (creature.handLeft, creature.handRight);
            hands.left.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.left.OnPunchHitEvent += new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.right.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.right.OnPunchHitEvent += new RagdollHand.PunchHitEvent(OnPunchHit);
        }

        protected override void OnDestroy()
        {
            Vampire.Creature?.currentLocomotion?.RemoveSpeedModifier(this);

            Vampire.sireEvent -= new Vampire.SiredEvent(OnPowerGained);
            Vampire.powerGainedEvent -= new Vampire.PowerGainedEvent(OnPowerGained);
            hands.left.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.right.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);

            base.OnDestroy();
        }

        private void OnPowerGained(Vampire check)
        {
            if (check == null || Vampire == null || check != Vampire)
                return;

            SetMightModifier();
        }

        private void SetMightModifier()
        {
            Creature creature = Vampire.Creature;
            if (creature == null) return;

            float levelScale = Vampire.Power / skill.powerAtLiftStrengthMax;
            Vector2 multiplierScale = skill.liftStrengthScale;
            float strengthMultiplier = skill.clampLiftStrength ? Mathf.Lerp(multiplierScale.x, multiplierScale.y, levelScale) : Mathf.LerpUnclamped(multiplierScale.x, multiplierScale.y, levelScale);

            creature.AddJointForceMultiplier(this, strengthMultiplier, strengthMultiplier);
        }

        private void OnPunchHit(RagdollHand hand, CollisionInstance hit, bool fist)
        {
            if (Vampire == null)
                return;

            Creature hitCreature = hit?.damageStruct.hitRagdollPart?.ragdoll?.creature;
            if (hitCreature != null)
            {
                hitCreature.ForceStagger(hit.impactVelocity.normalized, BrainModuleHitReaction.PushBehaviour.Effect.StaggerFull, hit.damageStruct.hitRagdollPart.type);
                float powerMultiplier = skill.clampPunchAddForceMult ? Mathf.Lerp(skill.punchAddForceMultScale.x, skill.punchAddForceMultScale.y, Vampire.Power / skill.powerAtPunchAddForceMultMax) : Mathf.LerpUnclamped(skill.punchAddForceMultScale.x, skill.punchAddForceMultScale.y, Vampire.Power / skill.powerAtPunchAddForceMultMax);
                Vector3 punchForce = hit.impactVelocity * skill.punchBaseForceMult * powerMultiplier;
                hitCreature.AddForce(punchForce, ForceMode.Impulse);
                return;
            }
        }
    }
}
