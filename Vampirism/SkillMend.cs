﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillMend : SkillData
    {
        public float damageToHealer = 5f;
        public float healingToTarget = 10f;
        public float mendInterval = 0.1f;
        
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleMend.skill = this;
            vampire.AddModule<ModuleMend>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleMend>();
            }
            else
            {
                ModuleMend mendModule = creature.gameObject.GetComponent<ModuleMend>();
                if (mendModule == null) return;

                MonoBehaviour.Destroy(mendModule);
            }

        }
    }
}
