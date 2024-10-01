using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleContinuity : VampireModule
    {
        public override string GetSkillID() => "Continuity";

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

        private void OnSiphon(Vampire source, Creature target, float damage)
        {
            if (source == null || moduleVampire == null || source != moduleVampire) return;

            if (moduleVampire.skill.GetModule("Siphon") is ModuleSiphon siphonModule)
            {
                moduleVampire.sireline.PerformSpawnAction(spawn => siphonModule?.Siphon(spawn, target, damage, false));
            }
            
        }
    }
}
