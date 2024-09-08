using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class SkillRavenousBrood : SkillData
    {

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            vampire.AddModule<ModuleRavenousBrood>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleRavenousBrood>();
            }
            else
            {
                ModuleRavenousBrood broodModule = creature.gameObject.GetComponent<ModuleRavenousBrood>();
                if (broodModule == null) return;

                MonoBehaviour.Destroy(broodModule);
            }

        }

    }
}
