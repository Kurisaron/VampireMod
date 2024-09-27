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
        public static SkillFortitude skill;
        
        protected override void Awake()
        {
            base.Awake();

            SetFortitudeModifier();

            Vampire.sireEvent -= new Vampire.SiredEvent(OnPowerGained);
            Vampire.sireEvent += new Vampire.SiredEvent(OnPowerGained);
            Vampire.powerGainedEvent -= new Vampire.PowerGainedEvent(OnPowerGained);
            Vampire.powerGainedEvent += new Vampire.PowerGainedEvent(OnPowerGained);
        }

        protected override void OnDestroy()
        {
            Vampire.Creature?.currentLocomotion?.RemoveSpeedModifier(this);

            Vampire.sireEvent -= new Vampire.SiredEvent(OnPowerGained);
            Vampire.powerGainedEvent -= new Vampire.PowerGainedEvent(OnPowerGained);

            base.OnDestroy();
        }

        private void OnPowerGained(Vampire check)
        {
            if (check == null || Vampire == null || check != Vampire)
                return;

            SetFortitudeModifier();
        }

        private void SetFortitudeModifier()
        {
            Creature creature = Vampire.Creature;
            if (creature == null) return;

            float levelScale = Vampire.Power / skill.powerAtResistanceMax;
            float resistMultiplier = skill.clampResistance ? Mathf.Lerp(skill.resistancePowerScale.x, skill.resistancePowerScale.y, levelScale) : Mathf.LerpUnclamped(skill.resistancePowerScale.x, skill.resistancePowerScale.y, levelScale);
            if (resistMultiplier < 0) resistMultiplier = 0;

            creature.SetDamageMultiplier(this, resistMultiplier);
        }
    }
}
