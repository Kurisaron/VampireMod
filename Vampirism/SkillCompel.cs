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
    public class SkillCompel: VampireSkill
    {
        public override VampireModule CreateModule() => CreateModule<ModuleCompel>();

    }
}
