using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using Vampirism.Skill;

namespace Vampirism.SKill
{
    public class ModuleModifiedGravity : SpellModifierModule
    {
        public override string GetSkillID() => "ModifiedGravity";

        protected override string GetSpellID() => "Gravity";

    }
}
