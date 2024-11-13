using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill.Spell;

namespace Vampirism.Skill
{
    public class ModuleDiscipleFlame : DiscipleModule
    {
        public override string GetSkillID() => "DiscipleFlame";

        protected override string GetSpellID() => "Fire";

        protected override void OnSpellAdded(Vampire spawn, SpellData spell)
        {
            base.OnSpellAdded(spawn, spell);

            if (spawn == null || spell == null || !(spell is SpellCastProjectile spellFire))
                return;


        }
    }
}
