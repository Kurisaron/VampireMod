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
    public class SkillBloodFocus : SkillData
    {
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            vampire.gameObject.AddComponent<ModuleBloodFocus>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            ModuleBloodFocus focusModule = creature.gameObject.GetComponent<ModuleBloodFocus>();
            if (focusModule == null) return;

            MonoBehaviour.Destroy(focusModule);
        }
    }
}
