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
        private (RagdollHand left, RagdollHand right) hands;

        public override string GetSkillID() => "Might";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            SetMightModifier();

            // Bind strength gain events
            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnPowerGained);
            VampireEvents.sireEvent += new Vampire.VampireEvent(OnPowerGained);
            moduleVampire.power.powerGainedEvent -= new Vampire.VampireEvent(OnPowerGained);
            moduleVampire.power.powerGainedEvent += new Vampire.VampireEvent(OnPowerGained);

            Creature moduleCreature = moduleVampire?.Creature;
            if (moduleCreature == null) return;

            // Bind punch events
            hands = (moduleCreature.handLeft, moduleCreature.handRight);
            hands.left.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.left.OnPunchHitEvent += new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.right.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.right.OnPunchHitEvent += new RagdollHand.PunchHitEvent(OnPunchHit);
        }

        public override void ModuleUnloaded()
        {
            moduleVampire?.Creature?.RemoveJointForceMultiplier(this);
            
            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnPowerGained);
            moduleVampire.power.powerGainedEvent -= new Vampire.VampireEvent(OnPowerGained);
            hands.left.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.right.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);

            base.ModuleUnloaded();
        }

        private void OnPowerGained(Vampire check)
        {
            if (check == null || moduleVampire == null || check != moduleVampire)
                return;

            SetMightModifier();
        }

        private void SetMightModifier()
        {
            Creature creature = moduleVampire?.Creature;
            SkillMight mightSkill = GetSkill<SkillMight>();
            if (creature == null || mightSkill == null) return;

            float levelScale = moduleVampire.power.PowerLevel / mightSkill.powerAtLiftStrengthMax;
            Vector2 multiplierScale = mightSkill.liftStrengthScale;
            float strengthMultiplier = mightSkill.clampLiftStrength ? Mathf.Lerp(multiplierScale.x, multiplierScale.y, levelScale) : Mathf.LerpUnclamped(multiplierScale.x, multiplierScale.y, levelScale);

            creature.AddJointForceMultiplier(this, strengthMultiplier, strengthMultiplier);
        }

        private void OnPunchHit(RagdollHand hand, CollisionInstance hit, bool fist)
        {
            SkillMight mightSkill = GetSkill<SkillMight>();
            if (moduleVampire == null || mightSkill == null)
                return;

            Creature hitCreature = hit?.damageStruct.hitRagdollPart?.ragdoll?.creature;
            if (hitCreature != null)
            {
                hitCreature.ForceStagger(hit.impactVelocity.normalized, BrainModuleHitReaction.PushBehaviour.Effect.StaggerFull, hit.damageStruct.hitRagdollPart.type);
                float powerMultiplier = mightSkill.clampPunchAddForceMult ? Mathf.Lerp(mightSkill.punchAddForceMultScale.x, mightSkill.punchAddForceMultScale.y, moduleVampire.power.PowerLevel / mightSkill.powerAtPunchAddForceMultMax) : Mathf.LerpUnclamped(mightSkill.punchAddForceMultScale.x, mightSkill.punchAddForceMultScale.y, moduleVampire.power.PowerLevel / mightSkill.powerAtPunchAddForceMultMax);
                Vector3 punchForce = hit.impactVelocity * mightSkill.punchBaseForceMult * powerMultiplier;

                hitCreature.ForceStagger(punchForce.normalized, BrainModuleHitReaction.PushBehaviour.Effect.StaggerFull);
                hitCreature.AddForce(punchForce, ForceMode.Impulse);
                return;
            }
        }
    }
}
