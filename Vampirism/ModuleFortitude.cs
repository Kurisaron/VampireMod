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
            moduleVampire?.creature?.RemoveDamageMultiplier(this);

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
            Creature creature = moduleVampire?.creature;
            SkillFortitude fortitudeSkill = GetSkill<SkillFortitude>();
            if (creature == null || fortitudeSkill == null) return;

            float levelScale = moduleVampire.power.PowerLevel / fortitudeSkill.powerAtResistanceMax;
            float resistMultiplier = fortitudeSkill.clampResistance ? Mathf.Lerp(fortitudeSkill.resistancePowerScale.x, fortitudeSkill.resistancePowerScale.y, levelScale) : Mathf.LerpUnclamped(fortitudeSkill.resistancePowerScale.x, fortitudeSkill.resistancePowerScale.y, levelScale);
            if (resistMultiplier < 0) resistMultiplier = 0;

            creature.SetDamageMultiplier(this, resistMultiplier);
        }
    }
}
