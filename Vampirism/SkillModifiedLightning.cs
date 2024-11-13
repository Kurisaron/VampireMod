using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillModifiedLightning : VampireSkill
    {
        public override VampireModule CreateModule() => CreateModule<ModuleModifiedLightning>();
    }
}
