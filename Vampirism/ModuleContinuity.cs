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
            if (source == null || Vampire == null || source != Vampire) return;

            Vampire.PerformSpawnAction(spawn => ModuleSiphon.Siphon(spawn, target, damage, false));
        }
    }
}
