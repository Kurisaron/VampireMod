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
    public class SkillFortitude : VampireSkill
    {
        public Vector2 healthBoostRange = new Vector2(1.5f, 10.0f);
        public float powerAtHealthBoostMax = 12345.0f;
        public bool clampHealthBoost = false;
        
        public Vector2 resistanceRange = new Vector2(0.75f, 0.2f);
        public float powerAtResistanceMax = 12345.0f;
        public bool clampResistance = false;

        public override VampireModule CreateModule() => CreateModule<ModuleFortitude>();

    }
}
