using System;
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
    [Serializable]
    public class SkillSire : VampireSkill
    {
        public Vector2 sireAmountScale = new Vector2(1, 10);
        public float powerAtSireAmountMax = 23456.0f;
        public bool clampSireAmount = false;

        public override VampireModule CreateModule() => CreateModule<ModuleSire>();

        public int GetSireAmount(Vampire vampire)
        {
            if (vampire == null || vampire.power == null) return 0;

            return vampire.power.PowerLevel > 0.0f ? Mathf.FloorToInt(clampSireAmount ? Mathf.Lerp(sireAmountScale.x, sireAmountScale.y, vampire.Power / powerAtSireAmountMax) : Mathf.LerpUnclamped(sireAmountScale.x, sireAmountScale.y, vampire.Power / powerAtSireAmountMax)) : 0;
        }
    }
}
