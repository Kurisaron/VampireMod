using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill;
using ThunderRoad.Skill.Spell;
using static ThunderRoad.TutorialArea;

namespace Vampirism
{
    public class SkillDrainingBolts : SpellSkillData
    {
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = null;
            if (!creature.IsVampire(out vampire))
                vampire = creature.Vampirize();

            if (vampire != null) 
            {
                EventManager.CreatureHitEvent drainBoltHit = GetDrainBoltHitEvent(vampire);
                EventManager.onCreatureHit -= drainBoltHit;
                EventManager.onCreatureHit += drainBoltHit;
            }
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            Vampire vampire = null;
            if (creature.IsVampire(out vampire))
            {
                EventManager.CreatureHitEvent drainBoltHit = GetDrainBoltHitEvent(vampire);
                EventManager.onCreatureHit -= drainBoltHit;
            }
        }

        private EventManager.CreatureHitEvent GetDrainBoltHitEvent(Vampire vampire)
        {
            return new EventManager.CreatureHitEvent((creature, collisionInstance, eventTime) =>
            {
                if (vampire == null || creature == null || collisionInstance.casterHand == null || eventTime == EventTime.OnStart) return;

                Creature caster = collisionInstance?.casterHand?.ragdollHand?.ragdoll?.creature;
                Creature vampireCreature = vampire.Creature;
                if (caster == null || vampireCreature == null || caster != vampireCreature || caster == creature) return;

                DamageStruct damageStruct = collisionInstance.damageStruct;
                if (damageStruct.damageType == DamageType.Lightning)
                    SkillSiphon.Siphon(vampire, creature, damageStruct.damage, false);
            });
        }
    }
}
