using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill;
using ThunderRoad.Skill.SpellMerge;
using UnityEngine;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillProfanedSun : VampireSkill
    {
        public Vector2 sunRadiusScale = new Vector2(5.0f, 10.0f);
        public float powerAtSunRadiusMax = 23456.0f;
        public bool clampSunRadius = false;
        public float sunInterval = 0.1f;

        public override VampireModule CreateModule() => CreateModule<ModuleProfanedSun>();

    }
}
