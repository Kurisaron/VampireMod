using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleRedHand : VampireModule
    {

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
            if (hand?.creature == null || Vampire?.Creature == null || hand.creature != Vampire.Creature) return;
            
            RagdollPart hitPart = hit?.damageStruct.hitRagdollPart;
            if (hitPart == null || hitPart == hand) return;

            Creature targetCreature = hitPart.ragdoll?.creature;
            if (targetCreature == null || targetCreature == Vampire.Creature) return;

            ModuleSiphon.Siphon(Vampire, targetCreature, hit.damageStruct.damage);
        }
    }
}
