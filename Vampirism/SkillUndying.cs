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
    public class SkillUndying : VampireSkill
    {
        public Vector2 regenPowerScale = new Vector2(0.01f, 0.05f);
        public float powerAtRegenMax = 23456.0f;
        public bool clampRegen = false;
        public float regenInterval = 2.0f;
        public Vector2 poolScale = new Vector2(0.0f, 100.0f);
        public float powerAtPoolMax = 23456.0f;
        public bool clampPool = false;

        public override VampireModule CreateModule() => CreateModule<ModuleUndying>();
        
    }
}
