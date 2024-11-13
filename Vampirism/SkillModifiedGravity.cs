using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vampirism.SKill;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillModifiedGravity : VampireSkill
    {
        public override VampireModule CreateModule() => CreateModule<ModuleModifiedGravity>();
    }
}
