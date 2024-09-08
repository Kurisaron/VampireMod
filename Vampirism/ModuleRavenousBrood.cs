using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vampirism.Skill
{
    public class ModuleRavenousBrood : VampireModule
    {
        protected override void Awake()
        {
            base.Awake();

            Vampire.sireEvent -= new Vampire.SiredEvent(OnSire);
            Vampire.sireEvent += new Vampire.SiredEvent(OnSire);
        }

        protected override void OnDestroy()
        {
            Vampire.sireEvent -= new Vampire.SiredEvent(OnSire);

            base.OnDestroy();
        }

        private void OnSire(Vampire target)
        {
            if (target == null || Vampire == null || target.Creature.isPlayer || target.Sire != Vampire) return;

            target.Creature.animator.speed *= 2.0f;
        }
    }
}
