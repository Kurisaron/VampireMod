﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill;
using UnityEngine;

namespace Vampirism.Skill
{
    public class SkillSire : SkillData
    {

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            if (creature == null)
                return;

            Vampire vampire = creature.AffirmVampirism();

            vampire.gameObject.AddComponent<ModuleSire>();

        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            ModuleSire sireModule = creature.gameObject.GetComponent<ModuleSire>();
            if (sireModule == null) return;

            MonoBehaviour.Destroy(sireModule);

        }


    }
}