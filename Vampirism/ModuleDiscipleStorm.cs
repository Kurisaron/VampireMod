using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad.Skill.Spell;
using ThunderRoad;

namespace Vampirism.Skill
{
    public class ModuleDiscipleStorm : DiscipleModule
    {
        public override string GetSkillID() => "DiscipleStorm";

        protected override string GetSpellID() => "Lightning";

        protected override void OnSpellAdded(Vampire spawn, SpellData spell)
        {
            base.OnSpellAdded(spawn, spell);

            if (spawn == null || spell == null || !(spell is SpellCastLightning spellLightning))
                return;


        }
    }
}
