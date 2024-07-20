using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;

namespace Vampirism
{
    public class SkillBloodFocus : SkillData
    {
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = null;
            if (!creature.IsVampire(out vampire))
                vampire = creature.Vampirize();

            if (vampire != null)
            {
                SkillSiphon.SiphonEvent siphonEvent = GetFocusDrainEvent(vampire);
                SkillSiphon.siphonEvent -= siphonEvent;
                SkillSiphon.siphonEvent += siphonEvent;
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
                SkillSiphon.SiphonEvent siphonEvent = GetFocusDrainEvent(vampire);
                SkillSiphon.siphonEvent -= siphonEvent;
            }
        }

        private SkillSiphon.SiphonEvent GetFocusDrainEvent(Vampire vampire)
        {
            return new SkillSiphon.SiphonEvent((source, target, damage) =>
            {
                if (vampire == null || source == null || target == null)
                    return;

                source.Creature?.mana?.RegenFocus(damage);
            });
        }
    }
}
