using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleModifiedLightning : SpellModifierModule
    {
        public override string GetSkillID() => "ModifiedLightning";

        protected override string GetSpellID() => "Lightning";

    }
}
