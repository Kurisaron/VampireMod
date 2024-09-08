using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;

namespace Vampirism.Skill
{
    public class ModuleSire : VampireModule
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

        private void OnSiphon(Vampire source, Creature target, float damage)
        {
            if (Vampire == null || source == null || target == null || source != Vampire || target.isKilled || target.IsVampire(out _) || source.SpawnCount.current >= source.SpawnCount.max) return;

            target.Vampirize(source.Power / 2.0f, source);
        }

    }
}
