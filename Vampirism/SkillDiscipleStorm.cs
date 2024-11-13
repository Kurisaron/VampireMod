using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillDiscipleStorm : VampireSkill
    {
        public override VampireModule CreateModule() => CreateModule<ModuleDiscipleStorm>();
    }
}
