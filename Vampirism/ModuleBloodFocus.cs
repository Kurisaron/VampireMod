using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;

namespace Vampirism.Skill
{
    public class ModuleBloodFocus : VampireModule
    {
        protected override void Awake()
        {
            base.Awake();

            ModuleSiphon.siphonEvent -= new ModuleSiphon.SiphonEvent(OnSiphon);
            ModuleSiphon.siphonEvent += new ModuleSiphon.SiphonEvent(OnSiphon);
        }

        protected override void OnDestroy()
        {
            ModuleSiphon.siphonEvent -= new ModuleSiphon.SiphonEvent(OnSiphon);

            base.OnDestroy();
        }

        private void OnSiphon(Vampire source, Creature creature, float damage)
        {
            if (source == null || Vampire == null || creature == null || source != Vampire) return;

            source.Creature?.mana?.RegenFocus(damage);
        }
    }
}
