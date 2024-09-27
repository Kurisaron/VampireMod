using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using static ThunderRoad.TutorialArea;
using UnityEngine;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillCatervaCruentis : SkillData
    {
        public List<string> inheritableSkillIds;

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleCatervaCruentis.skill = this;
            ModuleCatervaCruentis catervaCruentisModule = vampire.AddModule<ModuleCatervaCruentis>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleCatervaCruentis>();
            }
            else
            {
                ModuleCatervaCruentis catervaCruentisModule = creature.gameObject.GetComponent<ModuleCatervaCruentis>();
                if (catervaCruentisModule == null) return;

                MonoBehaviour.Destroy(catervaCruentisModule);
            }

        }

    }
}
