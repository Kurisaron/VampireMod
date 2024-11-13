using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleModifiedFire : SpellModifierModule
    {
        public override string GetSkillID() => "ModifiedFire";

        protected override string GetSpellID() => "Fire";

    }
}
