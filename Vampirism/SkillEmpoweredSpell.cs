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
    public class EmpoweredSpellModifier
    {
        public Vector2 modifierValueScale = new Vector2(0, 1);
        public float powerAtModifierMax = 12345.0f;
        public bool clampModifier = true;
    }

    [Serializable]
    public class SkillEmpoweredSpell : SkillData
    {
        public string spellID;
        public Dictionary<Modifier, EmpoweredSpellModifier> modifiers;

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleEmpoweredSpell empowerModule = vampire.AddModule<ModuleEmpoweredSpell>();
            empowerModule.AddSpell(spellID, modifiers);
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleEmpoweredSpell>(module => module.RemoveSpell(spellID));
            }
            else
            {
                ModuleEmpoweredSpell empowerModule = creature.gameObject.GetComponent<ModuleEmpoweredSpell>();
                if (empowerModule == null) return;

                empowerModule.RemoveSpell(spellID);
                MonoBehaviour.Destroy(empowerModule);
            }

        }

    }
}
