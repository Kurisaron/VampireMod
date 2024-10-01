using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;

namespace Vampirism.Skill
{
    public abstract class VampireSkill : SkillData
    {
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();
            vampire.skill?.AddSkill(this);
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            if (creature != null && creature.IsVampire(out Vampire vampire))
            {
                vampire.skill?.RemoveSkill(this);
            }
            
            base.OnSkillUnloaded(skillData, creature);
        }

        public abstract VampireModule CreateModule();
        protected ModuleType CreateModule<ModuleType>() where ModuleType : VampireModule
        {
            return Activator.CreateInstance<ModuleType>();
        }
    }


}
