using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill;
using ThunderRoad.Skill.SpellMerge;
using UnityEngine;
using UnityEngine.Events;

namespace Vampirism.Skill
{
    public class SkillDrainBubble : SpellSkillData
    {
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            vampire.gameObject.AddComponent<ModuleDrainBubble>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            ModuleDrainBubble bubbleModule = creature.gameObject.GetComponent<ModuleDrainBubble>();
            if (bubbleModule == null) return;

            MonoBehaviour.Destroy(bubbleModule);
        }

        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if (!(spell is SpellMergeGravity spellMergeGravity))
                return;

            ModuleDrainBubble.GravityMergeSpell = spellMergeGravity;
        }

        public override void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellUnload(spell, caster);
            if (!(spell is SpellMergeGravity spellMergeGravity))
                return;

            ModuleDrainBubble.GravityMergeSpell = null;
        }

    }
}
