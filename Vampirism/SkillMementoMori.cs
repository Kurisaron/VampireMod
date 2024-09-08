using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class SkillMementoMori : SkillData
    {
        
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            vampire.AddModule<ModuleMementoMori>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleMementoMori>();
            }
            else
            {
                ModuleMementoMori deathModule = creature.gameObject.GetComponent<ModuleMementoMori>();
                if (deathModule == null) return;

                MonoBehaviour.Destroy(deathModule);
            }

        }
    }
}
