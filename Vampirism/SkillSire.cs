using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill;
using UnityEngine;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillSire : SkillData
    {
        public Vector2 sireAmountScale = new Vector2(1, 10);
        public float powerAtSireAmountMax = 23456.0f;
        public bool clampSireAmount = false;

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleSire.skill = this;
            vampire.AddModule<ModuleSire>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleSire>();
            }
            else
            {
                ModuleSire sireModule = creature.gameObject.GetComponent<ModuleSire>();
                if (sireModule == null) return;

                MonoBehaviour.Destroy(sireModule);
            }
        }

        public int GetSireAmount(Vampire vampire)
        {
            if (vampire == null) return 0;

            return vampire.Power > 0.0f ? Mathf.FloorToInt(clampSireAmount ? Mathf.Lerp(sireAmountScale.x, sireAmountScale.y, vampire.Power / powerAtSireAmountMax) : Mathf.LerpUnclamped(sireAmountScale.x, sireAmountScale.y, vampire.Power / powerAtSireAmountMax)) : 0;
        }
    }
}
