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
    public class SkillMight : VampireSkill
    {
        public Vector2 liftStrengthScale = new Vector2(1.5f, 3);
        public float powerAtLiftStrengthMax = 12345.0f;
        public bool clampLiftStrength = false;

        public float punchBaseForceMult = 100.0f;
        public Vector2 punchAddForceMultScale = new Vector2(1.5f, 5);
        public float powerAtPunchAddForceMultMax = 23456.0f;
        public bool clampPunchAddForceMult = false;

        public override VampireModule CreateModule() => CreateModule<ModuleMight>();

    }
}
