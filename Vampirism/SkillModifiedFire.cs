using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vampirism.Skill
{
    public class SkillModifiedFire : VampireSkill
    {
        public override VampireModule CreateModule() => CreateModule<ModuleModifiedFire>();
    }
}
