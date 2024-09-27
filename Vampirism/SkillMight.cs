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
    public class SkillMight : SkillData
    {
        public Vector2 liftStrengthScale = new Vector2(1.5f, 3);
        public float powerAtLiftStrengthMax = 12345.0f;
        public bool clampLiftStrength = false;

        public float punchBaseForceMult = 100.0f;
        public Vector2 punchAddForceMultScale = new Vector2(1.5f, 5);
        public float powerAtPunchAddForceMultMax = 23456.0f;
        public bool clampPunchAddForceMult = false;

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleMight.skill = this;
            vampire.AddModule<ModuleMight>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleMight>();
            }
            else
            {
                ModuleMight mightModule = creature.gameObject.GetComponent<ModuleMight>();
                if (mightModule == null) return;

                MonoBehaviour.Destroy(mightModule);
            }
        }
    }
}
