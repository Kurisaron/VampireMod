using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class SkillShockwave : SkillData
    {
        public float shockwaveRange = 10.0f;

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            vampire.AddModule<ModuleShockwave>();
            ModuleShockwave.skill = this;
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleShockwave>();
            }
            else
            {
                ModuleShockwave waveModule = creature.gameObject.GetComponent<ModuleShockwave>();
                if (waveModule == null) return;

                MonoBehaviour.Destroy(waveModule);
            }

        }

    }
}
