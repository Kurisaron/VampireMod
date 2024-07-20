using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public class SkillSire : SkillData
    {

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            if (creature == null)
                return;

            Vampire vampire = null;
            if (!creature.IsVampire(out vampire))
                vampire = creature.Vampirize();

            if (vampire != null)
            {
                SkillSiphon.SiphonEvent sireEvent = GetSireEvent(vampire);
                SkillSiphon.siphonEvent -= sireEvent;
                SkillSiphon.siphonEvent += sireEvent;
            }
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature == null)
                return;

            Vampire vampire = null;
            if (creature.IsVampire(out vampire))
            {
                SkillSiphon.SiphonEvent sireEvent = GetSireEvent(vampire);
                SkillSiphon.siphonEvent -= sireEvent;
            }
        }

        private SkillSiphon.SiphonEvent GetSireEvent(Vampire master)
        {
            return new SkillSiphon.SiphonEvent((source, target, damage) =>
            {
                if (master == null || source == null || source != master || target == null || target.IsVampire(out Vampire _))
                    return;

                (int current, int max) spawnCount = source.SpawnCount;
                if (spawnCount.current < spawnCount.max)
                    target.Vampirize(1, 0.0f, source);
            });
        }

    }
}
