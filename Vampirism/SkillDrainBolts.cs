using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill;
using ThunderRoad.Skill.Spell;
using UnityEngine;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillDrainBolts : SpellSkillData
    {
        public Vector2 efficiencyScale = new Vector2(0.0f, 1.0f);
        public float powerAtEfficiencyMax = 5000.0f;
        public bool clampEfficiency = false;

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleDrainBolts.skill = this;
            vampire.AddModule<ModuleDrainBolts>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleDrainBolts>();
            }
            else
            {
                ModuleDrainBolts drainBoltModule = creature.gameObject.GetComponent<ModuleDrainBolts>();
                if (drainBoltModule == null) return;

                MonoBehaviour.Destroy(drainBoltModule);
            }

            base.OnSkillUnloaded(skillData, creature);
        }

        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if (!(spell is SpellCastLightning spellCastLightning))
                return;

            // Get the creature casting the spell, return if there is none
            Creature castingCreature = caster?.ragdollHand?.creature;
            if (castingCreature == null) 
                return;

            // Check for the vampire script attached to the casting creature, return if the creature is not a vampire
            if (!castingCreature.IsVampire(out Vampire vampire))
                return;

            // Find the module for this skill on the casting vampire, return if the module is not attached
            ModuleDrainBolts drainBoltModule = vampire.GetModule<ModuleDrainBolts>();
            if (drainBoltModule == null)
                return;

            // Add the lightning spell to the module for event subscription
            drainBoltModule.AddSpell(spellCastLightning);
        }

        public override void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellUnload(spell, caster);
            if (!(spell is SpellCastLightning spellCastLightning))
                return;

            Creature castingCreature = caster?.ragdollHand?.creature;
            if (castingCreature == null)
                return;

            if (!castingCreature.IsVampire(out Vampire vampire))
                return;

            ModuleDrainBolts drainBoltModule = vampire.GetModule<ModuleDrainBolts>();
            if (drainBoltModule == null)
                return;

            drainBoltModule.RemoveSpell();
        }

    }
}
