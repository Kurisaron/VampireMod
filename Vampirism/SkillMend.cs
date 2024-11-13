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
    public class SkillMend : VampireSkill
    {
        public float damageToHealer = 5f;
        public float healingToTarget = 10f;
        public float mendInterval = 0.1f;

        public override VampireModule CreateModule() => CreateModule<ModuleMend>();

    }
}
