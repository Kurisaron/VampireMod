using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillDreadAura : SkillData
    {
        public float auraRange = 10.0f;
        public float auraInterval = 0.1f;
        public float auraPowerScaleMax = 12345.0f;
        public float basePanicChance = 50.0f;
        public float maxPanicChance = 100.0f;

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleDreadAura.skill = this;
            vampire.AddModule<ModuleDreadAura>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleDreadAura>();
            }
            else
            {
                ModuleDreadAura auraModule = creature.gameObject.GetComponent<ModuleDreadAura>();
                if (auraModule == null) return;

                MonoBehaviour.Destroy(auraModule);
            }
            
        }
    }
}
