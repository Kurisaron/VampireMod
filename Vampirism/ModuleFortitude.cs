using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleFortitude : VampireModule
    {
        public override string GetSkillID() => "Fortitude";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            SetFortitudeModifier();

            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnPowerGained);
            VampireEvents.sireEvent += new Vampire.VampireEvent(OnPowerGained);
            moduleVampire.power.powerGainedEvent -= new Vampire.VampireEvent(OnPowerGained);
            moduleVampire.power.powerGainedEvent += new Vampire.VampireEvent(OnPowerGained);

        }

        public override void ModuleUnloaded()
        {
            moduleVampire?.Creature?.healthModifier?.Remove(this);
            moduleVampire?.Creature?.RemoveDamageMultiplier(this);

            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnPowerGained);
            if (moduleVampire?.power != null)
                moduleVampire.power.powerGainedEvent -= new Vampire.VampireEvent(OnPowerGained);

            base.ModuleUnloaded();
        }

        private void OnPowerGained(Vampire check)
        {
            if (check == null || moduleVampire == null || check != moduleVampire)
                return;

            SetFortitudeModifier();
        }

        private void SetFortitudeModifier()
        {
            Creature creature = moduleVampire?.Creature;
            SkillFortitude fortitudeSkill = GetSkill<SkillFortitude>();
            if (creature == null || fortitudeSkill == null) return;

            float currentPowerLevel = moduleVampire.power.PowerLevel;

            Vector2 healthBoostRange = fortitudeSkill.healthBoostRange;
            float healthBoostScale = currentPowerLevel / fortitudeSkill.powerAtHealthBoostMax;
            float healthBoostValue = fortitudeSkill.clampHealthBoost ? Mathf.Lerp(healthBoostRange.x, healthBoostRange.y, healthBoostScale) : Mathf.LerpUnclamped(healthBoostRange.x, healthBoostRange.y, healthBoostScale);
            if (healthBoostValue < 0) healthBoostValue = 0;
            creature.healthModifier?.Add(this, healthBoostValue);

            Vector2 resistRange = fortitudeSkill.resistanceRange;
            float resistScale = currentPowerLevel / fortitudeSkill.powerAtResistanceMax;
            float resistValue = fortitudeSkill.clampResistance ? Mathf.Lerp(resistRange.x, resistRange.y, resistScale) : Mathf.LerpUnclamped(resistRange.x, resistRange.y, resistScale);
            if (resistValue < 0) resistValue = 0;
            creature.SetDamageMultiplier(this, resistValue);

        }
    }
}
