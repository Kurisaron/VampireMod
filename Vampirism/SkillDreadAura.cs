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
    public class SkillDreadAura : VampireSkill
    {
        public float auraRange = 10.0f;
        public float auraInterval = 0.1f;
        public float auraPowerScaleMax = 12345.0f;
        public float basePanicChance = 50.0f;
        public float maxPanicChance = 100.0f;

        public override VampireModule CreateModule() => CreateModule<ModuleDreadAura>();

    }
}
