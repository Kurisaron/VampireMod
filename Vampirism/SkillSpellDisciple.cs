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
    public class SkillSpellDisciple : SkillData
    {
        public string spellID;
        
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleSpellDisciple discipleModule = vampire.gameObject.GetComponent<ModuleSpellDisciple>() ?? vampire.gameObject.GetComponent<ModuleSpellDisciple>();
            discipleModule.AddSpell(spellID);
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            ModuleSpellDisciple discipleModule = creature.gameObject.GetComponent<ModuleSpellDisciple>();
            if (discipleModule == null) return;

            discipleModule.RemoveSpell(spellID);
            MonoBehaviour.Destroy(discipleModule);

        }

    }
}
