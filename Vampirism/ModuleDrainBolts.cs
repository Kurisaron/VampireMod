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
        
        public override string GetSkillID() => "DrainingArcs";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            Mana vampireMana = moduleVampire?.Creature?.mana;
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
            Mana vampireMana = moduleVampire?.Creature?.mana;
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
            if (!(spell is SpellCastLightning spellCastLightning) || moduleVampire?.Creature == null)
                return;

            Debug.Log(GetDebugPrefix(nameof(OnSpellLoad)) + " Lightning spell loaded");

            Creature castingCreature = caster?.ragdollHand?.creature;
            if (Utils.CheckError(() => castingCreature == null, "There is no casting creature for the lightning spell") || Utils.CheckError(() => castingCreature != moduleVampire.Creature, "Creature casting the lightning spell is not the module's vampire"))
                return;

            spellCastLightning.OnBoltHitColliderGroupEvent -= new SpellCastLightning.BoltHitColliderGroupEvent(OnBoltHit);
            spellCastLightning.OnBoltHitColliderGroupEvent += new SpellCastLightning.BoltHitColliderGroupEvent(OnBoltHit);
        }

        private void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            if (!(spell is SpellCastLightning spellCastLightning))
                return;

            Debug.Log(GetDebugPrefix(nameof(OnSpellUnload)) + " Lightning spell unloaded");

            Creature castingCreature = caster?.ragdollHand?.creature;
            if (Utils.CheckError(() => castingCreature == null, GetDebugPrefix(nameof(OnSpellUnload)) + " There is no casting creature for the lightning spell") || Utils.CheckError(() => castingCreature != moduleVampire.Creature, GetDebugPrefix(nameof(OnSpellUnload)) + " Creature casting the lightning spell is not the module's vampire"))
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
            Debug.Log(GetDebugPrefix(nameof(OnBoltHit)) + " On Bolt Hit Event started");
            
            SkillDrainBolts drainBoltsSkill = GetSkill<SkillDrainBolts>();
            ModuleSiphon siphonModule = moduleVampire?.skill?.GetModule<ModuleSiphon>(GetSkillID());
            if (drainBoltsSkill == null)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnBoltHit)) + " No drain bolts skill active");
                return;
            }
            if (siphonModule == null)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnBoltHit)) + " No siphon module on module vampire");
                return;
            }
            
            Creature target = colliderGroup.GetComponentInParent<Creature>();
            if (target == null)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnBoltHit)) + " Hit collider group is not attached to a creature");
                return;
            }

            //source?.GetComponentInParent<Creature>();
            Creature castingCreature = spell?.spellCaster?.mana?.creature;
            if (castingCreature == null)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnBoltHit)) + " Source is not a creature");
                return;
            }

            if (moduleVampire?.Creature == null)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnBoltHit)) + " The drain bolts module's vampire/creature is null");
                return;
            }

            if (castingCreature == moduleVampire.Creature)
            {
                float efficiencyMultiplier = drainBoltsSkill.clampEfficiency ? Mathf.Lerp(drainBoltsSkill.efficiencyScale.x, drainBoltsSkill.efficiencyScale.y, moduleVampire.power.PowerLevel / drainBoltsSkill.powerAtEfficiencyMax) : Mathf.LerpUnclamped(drainBoltsSkill.efficiencyScale.x, drainBoltsSkill.efficiencyScale.y, moduleVampire.power.PowerLevel / drainBoltsSkill.powerAtEfficiencyMax);
                float lifeTransfer = intensity * efficiencyMultiplier;
                siphonModule.Siphon(moduleVampire, target, lifeTransfer, false);
                Debug.Log("Bolt successfully siphoned target.");
            }
            else
                Debug.LogError(GetDebugPrefix(nameof(OnBoltHit)) + " Casting creature is not module vampire");
        }

    }
}
