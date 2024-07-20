using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public class SkillStride : SkillData
    {
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = null;
            if (!creature.IsVampire(out vampire))
                vampire = creature.Vampirize();

            if (vampire != null)
            {
                SetStrideModifier(vampire);
                Vampire.LevelEvent strideEvent = GetStrideEvent(vampire);
                Vampire.levelEvent -= strideEvent;
                Vampire.levelEvent += strideEvent;
            }
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature == null)
                return;

            Vampire vampire = null;
            if (creature.IsVampire(out vampire))
            {
                creature.currentLocomotion.RemoveSpeedModifier(this);
                Vampire.LevelEvent strideEvent = GetStrideEvent(vampire);
                Vampire.levelEvent -= strideEvent;
            }
        }

        private Vampire.LevelEvent GetStrideEvent(Vampire vampire)
        {
            return new Vampire.LevelEvent(check =>
            {
                if (vampire == null || check == null || check != vampire)
                    return;

                SetStrideModifier(vampire);
            });
        }

        private void SetStrideModifier(Vampire target)
        {
            if (target == null) return;

            Creature creature = target.Creature;
            if (creature == null) return;

            float runSpeedMultiplier = Mathf.LerpUnclamped(1, 3, target.LevelScale);
            float jumpForceMultiplier = Mathf.LerpUnclamped(1, 4, target.LevelScale);
            creature.currentLocomotion.SetSpeedModifier(this, 1, 1, 1, runSpeedMultiplier, jumpForceMultiplier, 1);
        }
    }
}
