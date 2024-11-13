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
        public override string GetSkillID() => "BloodFocus";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            VampireEvents.siphonEvent -= new VampireEvents.SiphonEvent(OnSiphon);
            VampireEvents.siphonEvent += new VampireEvents.SiphonEvent(OnSiphon);
        }

        public override void ModuleUnloaded()
        {
            VampireEvents.siphonEvent -= new VampireEvents.SiphonEvent(OnSiphon);
            
            base.ModuleUnloaded();
        }

        private void OnSiphon(Vampire source, Creature creature, float damage)
        {
            if (source == null || moduleVampire == null || creature == null || source != moduleVampire) return;

            moduleVampire.Creature?.mana?.RegenFocus(damage);
        }
    }
}
