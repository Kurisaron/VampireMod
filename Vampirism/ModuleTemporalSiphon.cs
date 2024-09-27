using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleTemporalSiphon : VampireModule
    {
        public static SkillTemporalSiphon skill;
        
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
            if (source == null || Vampire == null || target == null || skill == null || source != Vampire) return;

            if (target.IsVampire(out Vampire vampire) && vampire.Sire == Vampire)
                return;

            target.Inflict(skill.statusData, this, skill.duration, skill.slowMult);
        }
    }
}
