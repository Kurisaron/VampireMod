using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleShockwave : VampireModule
    {
        private (RagdollHand left, RagdollHand right) hands;

        public override string GetSkillID() => "Shockwave";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            Creature creature = moduleVampire?.Creature;
            if (creature == null) return;

            hands = (creature.handLeft, creature.handRight);
            hands.left.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.left.OnPunchHitEvent += new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.right.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.right.OnPunchHitEvent += new RagdollHand.PunchHitEvent(OnPunchHit);
        }

        public override void ModuleUnloaded()
        {
            hands.left.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.right.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);

            base.ModuleUnloaded();
        }

        private void OnPunchHit(RagdollHand hand, CollisionInstance hit, bool fist)
        {
            SkillShockwave shockwaveSkill = GetSkill<SkillShockwave>();
            if (shockwaveSkill == null) return;
            
            List<Creature> targets = Creature.allActive.FindAll(creature => creature != null && !creature.pooled && creature != hand.creature && Vector3.Distance(creature.transform.position, hit.contactPoint) <= shockwaveSkill.shockwaveRange);
            if (targets == null || targets.Count <= 0) return;

            foreach (Creature target in targets)
            {
                if (target.IsVampire(out Vampire vampireTarget) && (vampireTarget.sireline.Sire == moduleVampire.sireline.Sire || vampireTarget.sireline.Sire == moduleVampire || vampireTarget == moduleVampire.sireline.Sire)) continue;

                Vector3 staggerDirection = (target.transform.position - hit.contactPoint).normalized;
                target.ForceStagger(staggerDirection, BrainModuleHitReaction.PushBehaviour.Effect.StaggerFull);
                target.AddForce(staggerDirection * hit.impactVelocity.magnitude * shockwaveSkill.shockwavePowerMult, ForceMode.Impulse);
                target.AddExplosionForce(hit.impactVelocity.magnitude * shockwaveSkill.shockwavePowerMult, hit.contactPoint, shockwaveSkill.shockwaveRange, 1.0f, ForceMode.Impulse);
            }
        }
    }
}
