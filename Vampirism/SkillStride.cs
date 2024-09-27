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
    public class SkillStride : SkillData
    {
        public Vector2 runSpeedMultScale = new Vector2(1.5f, 3.0f);
        public float powerAtRunSpeedMax = 23456.0f;
        public bool clampRunSpeed = false;

        public Vector2 jumpPowerMultScale = new Vector2(1.5f, 2.5f);
        public float powerAtJumpPowerMax = 12345.0f;
        public bool clampJumpPower = false;

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleStride.skill = this;
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
