using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine

namespace Vampirism.Skill
{
    public class ModuleShockwave : VampireModule
    {
        public static SkillShockwave skill;
        private (RagdollHand left, RagdollHand right) hands;
        
        protected override void Awake()
        {
            base.Awake();

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
            hands.left.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);
            hands.right.OnPunchHitEvent -= new RagdollHand.PunchHitEvent(OnPunchHit);

            base.OnDestroy();
        }

        private void OnPunchHit(RagdollHand hand, CollisionInstance hit, bool fist)
        {
            List<Creature> targets = Creature.allActive.FindAll(creature => creature != null && !creature.pooled && creature != hand.creature && Vector3.Distance(creature.transform.position, hit.contactPoint) <= skill.shockwaveRange);
            if (targets == null || targets.Count <= 0) return;

            foreach (Creature target in targets)
            {
                if (target.IsVampire(out Vampire vampireTarget) && (vampireTarget.Sire == Vampire.Sire || vampireTarget.Sire == Vampire || vampireTarget == Vampire.Sire)) continue;

                target.AddExplosionForce(hit.impactVelocity.magnitude * 10.0f, hit.contactPoint, skill.shockwaveRange, 1.0f, ForceMode.Impulse);
            }
        }
    }
}
