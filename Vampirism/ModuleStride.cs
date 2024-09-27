﻿using System;
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
        public static SkillStride skill;
        
        protected override void Awake()
        {
            base.Awake();

            SetStrideModifier();

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

            SetStrideModifier();
        }

        private void SetStrideModifier()
        {
            Creature creature = Vampire?.Creature;
            if (creature == null) return;

            float runSpeedMultiplier = skill.clampRunSpeed ? Mathf.Lerp(skill.runSpeedMultScale.x, skill.runSpeedMultScale.y, Vampire.Power / skill.powerAtRunSpeedMax) : Mathf.LerpUnclamped(skill.runSpeedMultScale.x, skill.runSpeedMultScale.y, Vampire.Power / skill.powerAtRunSpeedMax);
            float jumpForceMultiplier = skill.clampJumpPower ? Mathf.Lerp(skill.jumpPowerMultScale.x, skill.jumpPowerMultScale.y, Vampire.Power / skill.powerAtJumpPowerMax) : Mathf.LerpUnclamped(skill.jumpPowerMultScale.x, skill.jumpPowerMultScale.y, Vampire.Power / skill.powerAtJumpPowerMax);
            creature.currentLocomotion.SetSpeedModifier(this, 1, 1, 1, runSpeedMultiplier, jumpForceMultiplier, 1);
        }
    }
}
