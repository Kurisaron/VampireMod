using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class SkillStride : SkillData
    {
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            vampire.gameObject.AddComponent<ModuleStride>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            ModuleStride strideModule = creature.gameObject.GetComponent<ModuleStride>();
            if (strideModule == null) return;

            MonoBehaviour.Destroy(strideModule);

        }

        
    }
}
