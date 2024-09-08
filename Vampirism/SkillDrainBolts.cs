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
    public class SkillDrainBolts : SpellSkillData
    {

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

        }

        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if (!(spell is SpellCastLightning spellCastLightning))
                return;

            spellCastLightning.OnBoltHitColliderGroupEvent -= new SpellCastLightning.BoltHitColliderGroupEvent(OnBoltHit);
            spellCastLightning.OnBoltHitColliderGroupEvent += new SpellCastLightning.BoltHitColliderGroupEvent(OnBoltHit);
        }

        public override void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellUnload(spell, caster);
            if (!(spell is SpellCastLightning spellCastLightning))
                return;

            spellCastLightning.OnBoltHitColliderGroupEvent -= new SpellCastLightning.BoltHitColliderGroupEvent(OnBoltHit);
        }

        private void OnBoltHit(
            SpellCastLightning spell,
            ColliderGroup colliderGroup,
            UnityEngine.Vector3 position,
            UnityEngine.Vector3 normal,
            UnityEngine.Vector3 velocity,
            float intensity,
            ColliderGroup source,
            HashSet<ThunderEntity> seenEntities)
        {
            Creature target = colliderGroup.GetComponentInParent<Creature>();
            if (target == null)
                return;

            Creature sourceCreature = source?.GetComponentInParent<Creature>();
            if (sourceCreature == null)
            {
                Debug.LogError("Source is not a creature");
                return;
            }

            if (sourceCreature.IsVampire(out Vampire vampire))
            {
                ModuleSiphon.Siphon(vampire, target, intensity, false);
                Debug.Log("Bolt successfully siphoned target.");
            }
        }
    }
}
