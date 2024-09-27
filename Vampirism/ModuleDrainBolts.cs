using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill.Spell;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleDrainBolts : VampireModule
    {
        public static SkillDrainBolts skill;

        private SpellCastLightning spell;
        
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            RemoveSpell();

            base.OnDestroy();
        }

        public void AddSpell(SpellCastLightning lightning)
        {
            if (lightning == null)
            {
                Debug.LogError("Lightning spell is null");
                return;
            }

            spell = lightning;
            spell.OnBoltHitColliderGroupEvent -= new SpellCastLightning.BoltHitColliderGroupEvent(OnBoltHit);
            spell.OnBoltHitColliderGroupEvent += new SpellCastLightning.BoltHitColliderGroupEvent(OnBoltHit);
        }

        public void RemoveSpell()
        {
            if (spell == null)
                return;

            spell.OnBoltHitColliderGroupEvent -= new SpellCastLightning.BoltHitColliderGroupEvent(OnBoltHit);

            spell = null;
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

            if (Utils.CheckError(() => Vampire?.Creature == null, "The drain bolts module's vampire/creature is null"))
                return;

            if (sourceCreature == Vampire.Creature)
            {
                float efficiencyMultiplier = skill.clampEfficiency ? Mathf.Lerp(skill.efficiencyScale.x, skill.efficiencyScale.y, Vampire.Power / skill.powerAtEfficiencyMax) : Mathf.LerpUnclamped(skill.efficiencyScale.x, skill.efficiencyScale.y, Vampire.Power / skill.powerAtEfficiencyMax);
                float lifeTransfer = intensity * efficiencyMultiplier;
                ModuleSiphon.Siphon(Vampire, target, lifeTransfer, false);
                Debug.Log("Bolt successfully siphoned target.");
            }
        }
    }
}
