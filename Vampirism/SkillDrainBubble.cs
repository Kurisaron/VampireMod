using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill;
using ThunderRoad.Skill.SpellMerge;
using UnityEngine;
using UnityEngine.Events;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillDrainBubble : VampireSkill
    {
        public override VampireModule CreateModule() => CreateModule<ModuleDrainBubble>();

    }
}
