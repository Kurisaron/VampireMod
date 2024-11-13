using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad.Skill.Spell;
using ThunderRoad;

namespace Vampirism.Skill
{
    public class ModuleDiscipleForce : DiscipleModule
    {
        public override string GetSkillID() => "DiscipleForce";

        protected override string GetSpellID() => "Gravity";

        protected override void OnSpellAdded(Vampire spawn, SpellData spell)
        {
            base.OnSpellAdded(spawn, spell);

            if (spawn == null || spell == null || !(spell is SpellCastGravity spellGravity))
                return;


        }
    }
}
