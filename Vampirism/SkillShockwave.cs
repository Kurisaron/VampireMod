using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillShockwave : VampireSkill
    {
        public float shockwaveRange = 10.0f;
        public float shockwavePowerMult = 1000.0f;

        public override VampireModule CreateModule() => CreateModule<ModuleShockwave>();

    }
}
