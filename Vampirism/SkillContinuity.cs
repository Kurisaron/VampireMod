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
    public class SkillContinuity : VampireSkill
    {
        public Vector2 efficiencyScale = new Vector2(0.2f, 1.0f);
        public float powerAtEfficiencyMax = 15000.0f;
        public bool clampEfficiency = true;
        
        public override VampireModule CreateModule() => CreateModule<ModuleContinuity>();

    }
}
