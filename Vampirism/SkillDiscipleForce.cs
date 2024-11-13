using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vampirism.Skill
{
    public class SkillDiscipleForce : VampireSkill
    {
        public override VampireModule CreateModule() => CreateModule<ModuleDiscipleForce>();
    }
}
