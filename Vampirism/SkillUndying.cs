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
    public class SkillUndying : SkillData
    {
        public Vector2 regenPowerScale = new Vector2(0.01f, 0.05f);
        public float powerAtRegenMax = 23456.0f;
        public bool clampRegen = false;
        public float regenInterval = 2.0f;
        public Vector2 poolScale = new Vector2(0.0f, 100.0f);
        public float powerAtPoolMax = 23456.0f;
        public bool clampPool = false;
        
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleUndying.skill = this;
            vampire.AddModule<ModuleUndying>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleUndying>();
            }
            else
            {
                ModuleUndying undyingModule = creature.gameObject.GetComponent<ModuleUndying>();
                if (undyingModule == null) return;

                MonoBehaviour.Destroy(undyingModule);
            }
        }
    }
}
