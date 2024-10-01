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
        
        public override string GetSkillID() => "DrainingBolts";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            Mana vampireMana = moduleVampire?.creature?.mana;
            if (!Utils.CheckError(() => vampireMana == null, "No mana component found for vampire upon drain bolt module load"))
            {
                vampireMana.OnSpellLoadEvent -= new Mana.SpellLoadEvent(OnSpellLoad);
                vampireMana.OnSpellLoadEvent += new Mana.SpellLoadEvent(OnSpellLoad);
                vampireMana.OnSpellUnloadEvent -= new Mana.SpellLoadEvent(OnSpellUnload);
                vampireMana.OnSpellUnloadEvent += new Mana.SpellLoadEvent(OnSpellUnload);
            }

        }

        public override void ModuleUnloaded()
        {
            Mana vampireMana = moduleVampire?.creature?.mana;
            if (!Utils.CheckError(() => vampireMana == null, "No mana component found for vampire upon drain bolt module load"))
            {
                vampireMana.OnSpellLoadEvent -= new Mana.SpellLoadEvent(OnSpellLoad);
                vampireMana.OnSpellUnloadEvent -= new Mana.SpellLoadEvent(OnSpellUnload);

                if (vampireMana.casterLeft.spellInstance is SpellCastLightning leftLightning)
                    leftLightning.OnBoltHitColliderGroupEvent -= new SpellCastLightning.BoltHitColliderGroupEvent(OnBoltHit);
                if (vampireMana.casterRight.spellInstance is SpellCastLightning rightLightning)
                    rightLightning.OnBoltHitColliderGroupEvent -= new SpellCastLightning.BoltHitColliderGroupEvent(OnBoltHit);

            }

            base.ModuleUnloaded();
        }

        private void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            if (!(spell is SpellCastLightning spellCastLightning) || moduleVampire?.creature == null)
                return;

            Creature castingCreature = caster?.ragdollHand?.creature;
            if (Utils.CheckError(() => castingCreature == null, "There is no casting creature for the lightning spell") || Utils.CheckError(() => castingCreature != moduleVampire.creature, "Creature casting the lightning spell is not the module's vampire"))
                return;

            spellCastLightning.OnBoltHitColliderGroupEvent -= new SpellCastLightning.BoltHitColliderGroupEvent(OnBoltHit);
            spellCastLightning.OnBoltHitColliderGroupEvent += new SpellCastLightning.BoltHitColliderGroupEvent(OnBoltHit);
        }

        private void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            if (!(spell is SpellCastLightning spellCastLightning))
                return;

            Creature castingCreature = caster?.ragdollHand?.creature;
            if (Utils.CheckError(() => castingCreature == null, "There is no casting creature for the lightning spell") || Utils.CheckError(() => castingCreature != moduleVampire.creature, "Creature casting the lightning spell is not the module's vampire"))
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
            SkillDrainBolts drainBoltsSkill = GetSkill<SkillDrainBolts>();
            ModuleSiphon siphonModule = moduleVampire?.skill?.GetModule<ModuleSiphon>(GetSkillID());
            if (drainBoltsSkill == null || siphonModule == null) return;
            
            Creature target = colliderGroup.GetComponentInParent<Creature>();
            if (target == null)
                return;

            //source?.GetComponentInParent<Creature>();
            Creature castingCreature = spell?.spellCaster?.mana?.creature;
            if (castingCreature == null)
            {
                Debug.LogError("Source is not a creature");
                return;
            }

            if (Utils.CheckError(() => moduleVampire?.creature == null, "The drain bolts module's vampire/creature is null"))
                return;

            if (castingCreature == moduleVampire.creature)
            {
                float efficiencyMultiplier = drainBoltsSkill.clampEfficiency ? Mathf.Lerp(drainBoltsSkill.efficiencyScale.x, drainBoltsSkill.efficiencyScale.y, moduleVampire.power.PowerLevel / drainBoltsSkill.powerAtEfficiencyMax) : Mathf.LerpUnclamped(drainBoltsSkill.efficiencyScale.x, drainBoltsSkill.efficiencyScale.y, moduleVampire.power.PowerLevel / drainBoltsSkill.powerAtEfficiencyMax);
                float lifeTransfer = intensity * efficiencyMultiplier;
                siphonModule.Siphon(moduleVampire, target, lifeTransfer, false);
                Debug.Log("Bolt successfully siphoned target.");
            }
        }

    }
}
