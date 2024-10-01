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
    public class SkillStride : VampireSkill
    {
        public Vector2 runSpeedMultScale = new Vector2(1.5f, 3.0f);
        public float powerAtRunSpeedMax = 23456.0f;
        public bool clampRunSpeed = false;

        public Vector2 jumpPowerMultScale = new Vector2(1.5f, 2.5f);
        public float powerAtJumpPowerMax = 12345.0f;
        public bool clampJumpPower = false;

        public override VampireModule CreateModule() => CreateModule<ModuleStride>();

    }
}
