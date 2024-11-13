using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleStride : VampireModule
    {
        public override string GetSkillID() => "Stride";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            SetStrideModifier();

            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnPowerGained);
            VampireEvents.sireEvent += new Vampire.VampireEvent(OnPowerGained);
            moduleVampire.power.powerGainedEvent -= new Vampire.VampireEvent(OnPowerGained);
            moduleVampire.power.powerGainedEvent += new Vampire.VampireEvent(OnPowerGained);

        }

        public override void ModuleUnloaded()
        {
            moduleVampire?.Creature?.currentLocomotion?.RemoveSpeedModifier(this);

            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnPowerGained);
            if (moduleVampire?.power != null)
                moduleVampire.power.powerGainedEvent -= new Vampire.VampireEvent(OnPowerGained);

            base.ModuleUnloaded();
        }

        private void OnPowerGained(Vampire check)
        {
            if (check == null || moduleVampire == null || check != moduleVampire)
                return;

            SetStrideModifier();
        }

        private void SetStrideModifier()
        {
            Creature creature = moduleVampire?.Creature;
            SkillStride strideSkill = GetSkill<SkillStride>();
            if (creature == null || strideSkill == null) return;

            Vector2 runSpeedMultiplierRange = strideSkill.runSpeedMultScale;
            float runSpeedScaleValue = moduleVampire.power.PowerLevel / strideSkill.powerAtRunSpeedMax;
            float runSpeedMultiplier = strideSkill.clampRunSpeed ? Mathf.Lerp(runSpeedMultiplierRange.x, runSpeedMultiplierRange.y, runSpeedScaleValue) : Mathf.LerpUnclamped(runSpeedMultiplierRange.x, runSpeedMultiplierRange.y, runSpeedScaleValue);

            Vector2 jumpForceMultiplierRange = strideSkill.jumpPowerMultScale;
            float jumpForceScaleValue = moduleVampire.power.PowerLevel / strideSkill.powerAtJumpPowerMax;
            float jumpForceMultiplier = strideSkill.clampJumpPower ? Mathf.Lerp(jumpForceMultiplierRange.x, jumpForceMultiplierRange.y, jumpForceScaleValue) : Mathf.LerpUnclamped(jumpForceMultiplierRange.x, jumpForceMultiplierRange.y, jumpForceScaleValue);

            creature.currentLocomotion.SetSpeedModifier(this, 1, 1, 1, runSpeedMultiplier, jumpForceMultiplier, 1);
        }
    }
}
