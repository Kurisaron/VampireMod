using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill;
using ThunderRoad.Skill.Spell;
using UnityEngine;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillDrainBolts : VampireSkill
    {
        public Vector2 efficiencyScale = new Vector2(0.0f, 1.0f);
        public float powerAtEfficiencyMax = 5000.0f;
        public bool clampEfficiency = false;

        public override VampireModule CreateModule() => CreateModule<ModuleDrainBolts>();

    }
}
