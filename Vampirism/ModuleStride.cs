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
        protected override void Awake()
        {
            base.Awake();

            SetStrideModifier();

            Vampire.powerGainedEvent -= new Vampire.PowerGainedEvent(OnPowerGained);
            Vampire.powerGainedEvent += new Vampire.PowerGainedEvent(OnPowerGained);
        }

        protected override void OnDestroy()
        {
            Vampire.Creature?.currentLocomotion?.RemoveSpeedModifier(this);

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
            Creature creature = Vampire.Creature;
            if (creature == null) return;

            float levelScale = Vampire.Power / 12345.0f;
            float runSpeedMultiplier = Mathf.LerpUnclamped(1, 3, levelScale);
            float jumpForceMultiplier = Mathf.LerpUnclamped(1, 4, levelScale);
            creature.currentLocomotion.SetSpeedModifier(this, 1, 1, 1, runSpeedMultiplier, jumpForceMultiplier, 1);
        }
    }
}
