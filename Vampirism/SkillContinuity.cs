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
    public class SkillContinuity : SkillData
    {
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            vampire.gameObject.AddComponent<ModuleContinuity>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            ModuleContinuity continuityModule = creature.gameObject.GetComponent<ModuleContinuity>();
            if (continuityModule == null) return;

            MonoBehaviour.Destroy(continuityModule);
        }
    }
}
