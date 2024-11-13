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

        public override string GetSkillID() => "RedHanded";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            Creature moduleCreature = moduleVampire?.Creature;
            if (moduleCreature == null) return;

            hands = (moduleCreature.handLeft, moduleCreature.handRight);
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
            if (hand?.creature == null || moduleVampire?.Creature == null || hand.creature != moduleVampire.Creature) return;
            
            RagdollPart hitPart = hit?.damageStruct.hitRagdollPart;
            if (hitPart == null || hitPart == hand) return;

            Creature targetCreature = hitPart.ragdoll?.creature;
            if (targetCreature == null || targetCreature == moduleVampire.Creature) return;

            ModuleSiphon siphonModule = moduleVampire.skill?.GetModule<ModuleSiphon>("Siphon");
            siphonModule?.Siphon(moduleVampire, targetCreature, hit.damageStruct.damage, false);
        }
    }
}
