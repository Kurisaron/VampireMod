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
        
        public override string GetSkillID() => "Mend";

        public override IEnumerator ModulePassive()
        {
            Debug.Log(GetDebugPrefix(nameof(ModulePassive)) + " Passive routine started");

            SkillMend mendSkill = GetSkill<SkillMend>();
            Creature moduleCreature = moduleVampire?.Creature;
            while (true)
            {
                Debug.Log(GetDebugPrefix(nameof(ModulePassive)) + " Passive routine tick started");

                if (mendSkill == null)
                {
                    Debug.LogError(GetDebugPrefix(nameof(ModulePassive)) + " Mend skill not present");
                    break;
                }

                if (moduleCreature == null)
                {
                    Debug.LogError(GetDebugPrefix(nameof(ModulePassive)) + " Module creature/vampire not present");
                    break;
                }
                    
                if (!moduleCreature.isPlayer)
                {
                    Debug.LogError(GetDebugPrefix(nameof(ModulePassive)) + " Module vampire is not the player");
                    break;
                }

                Debug.Log(GetDebugPrefix(nameof(ModulePassive)) + " Passive routine tick checks cleared");

                Mend(mendSkill, moduleCreature.handLeft, moduleCreature.handRight);

                Debug.Log(GetDebugPrefix(nameof(ModulePassive)) + " Passive routine tick ended");

                yield return new WaitForSeconds(mendSkill.mendInterval);
            }

            Debug.Log(GetDebugPrefix(nameof(ModulePassive)) + " Passive routine ended");
        }

        private void Mend(SkillMend mendSkill, params RagdollHand[] hands)
        {
            if (mendSkill == null)
            {
                Debug.LogError(GetDebugPrefix(nameof(Mend)) + " Skill data not present");
                return;
            }

            if (hands.Length <= 0)
            {
                Debug.LogError(GetDebugPrefix(nameof(Mend)) + " No hands to use for mending");
                return;
            }

            for (int i = 0; i < hands.Length; i++)
            {
                RagdollHand hand = hands[i];
                if (hand == null)
                {
                    Debug.LogError(GetDebugPrefix(nameof(Mend)) + " Current mend hand is null");
                    continue;
                }

                Creature grabbedCreature = hand?.grabbedHandle?.GetComponentInParent<RagdollPart>()?.ragdoll?.creature ?? hand?.grabbedHandle?.GetComponentInChildren<RagdollPart>()?.ragdoll?.creature;
                if (grabbedCreature == null) continue;

                Debug.Log(GetDebugPrefix(nameof(Mend)) + " " + hand.side.ToString() + " hand holding creature");

                PlayerControl.Hand controlHand = PlayerControl.GetHand(hand.side);
                if (controlHand == null)
                {
                    Debug.LogError(GetDebugPrefix(nameof(Mend)) + " No control hand for " + hand.side.ToString() + " hand");
                    continue;
                }
                if (!controlHand.alternateUsePressed)
                {
                    Debug.LogError(GetDebugPrefix(nameof(Mend)) + " " + hand.side.ToString() + " hand not pressing alt use button");
                    continue;
                }

                float damage = mendSkill.damageToHealer;
                float healAmount = mendSkill.healingToTarget;
                if (grabbedCreature.IsVampire(out Vampire grabbedVampire) && grabbedVampire.sireline.Sire == moduleVampire)
                {
                    damage *= 0.1f;
                    healAmount *= 2.0f;
                }

                hand.ragdoll.creature.Damage(damage);
                grabbedCreature.Heal(healAmount);
            }
        }

    }
}
