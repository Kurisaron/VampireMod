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
    public abstract class VampireSkill : SkillData
    {
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            string debugID = this.GetDebugID(nameof(OnSkillLoaded));

            Vampire vampire = creature.AffirmVampirism();
            vampire.skill?.AddSkill(this);

            bool creatureIsPlayer = creature.isPlayer;
            Debug.Log(debugID + " Creature being given skill " + skillData.id + " is" + (creatureIsPlayer ? " " : " NOT ") + "a player");
            bool vampireIsPlayer = vampire.isPlayer;
            Debug.Log(debugID + " Vampire being given skill " + skillData.id + " is" + (vampireIsPlayer ? " " : " NOT ") + "a player");

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
