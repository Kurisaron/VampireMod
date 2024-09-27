using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill;
using ThunderRoad.Skill.SpellMerge;
using UnityEngine;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillProfanedSun : SpellSkillData
    {
        public Vector2 sunRadiusScale = new Vector2(5.0f, 10.0f);
        public float powerAtSunRadiusMax = 23456.0f;
        public bool clampSunRadius = false;
        public float sunInterval = 0.1f;
        
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleProfanedSun.skill = this;
            vampire.AddModule<ModuleProfanedSun>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleProfanedSun>();
            }
            else
            {
                ModuleProfanedSun sunModule = creature.gameObject.GetComponent<ModuleProfanedSun>();
                if (sunModule == null) return;

                MonoBehaviour.Destroy(sunModule);
            }
        }

        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);

            if (!(spell is SpellMergeFire spellMergeFire))
                return;

            if (Utils.CheckError(() => caster == null, "Profaned Sun: Spell caster is null"))
                return;

            Creature casterCreature = caster?.ragdollHand?.creature;
            if (Utils.CheckError(() => casterCreature == null, "Profaned Sun: Spell caster creature is null")) return;

            if (casterCreature.IsVampire(out Vampire vampire))
            {
                ModuleProfanedSun sunModule = vampire.GetModule<ModuleProfanedSun>();
                if (Utils.CheckError(() => casterCreature == null, "Profaned Sun: Caster vampire does not have an appropriate module for this ability"))
                    return;

                sunModule.Spell = spellMergeFire;
            }
            else
                Debug.LogError("Profaned Sun: Caster creature is not a vampire");
        }

        public override void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellUnload(spell, caster);

            if (!(spell is SpellMergeFire spellMergeFire))
                return;

            if (Utils.CheckError(() => caster == null, "Profaned Sun: Spell caster is null"))
                return;

            Creature casterCreature = caster?.ragdollHand?.creature;
            if (Utils.CheckError(() => casterCreature == null, "Profaned Sun: Spell caster creature is null")) return;

            if (casterCreature.IsVampire(out Vampire vampire))
            {
                ModuleProfanedSun sunModule = vampire.GetModule<ModuleProfanedSun>();
                if (Utils.CheckError(() => casterCreature == null, "Profaned Sun: Caster vampire does not have an appropriate module for this ability"))
                    return;

                sunModule.Spell = null;
            }
            else
                Debug.LogError("Profaned Sun: Caster creature is not a vampire");
        }
    }
}
