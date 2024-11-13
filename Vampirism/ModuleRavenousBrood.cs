using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;

namespace Vampirism.Skill
{
    public class ModuleRavenousBrood : VampireModule
    {
        public override string GetSkillID() => "RavenousBrood";

        private List<Creature> boostedCreatures = new List<Creature>();

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            boostedCreatures = new List<Creature>();

            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnSire);
            VampireEvents.sireEvent += new Vampire.VampireEvent(OnSire);
            EventManager.onCreatureKill -= new EventManager.CreatureKillEvent(OnKill);
            EventManager.onCreatureKill += new EventManager.CreatureKillEvent(OnKill);
        }

        public override void ModuleUnloaded()
        {
            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnSire);
            EventManager.onCreatureKill -= new EventManager.CreatureKillEvent(OnKill);

            boostedCreatures?.Clear();
            
            base.ModuleUnloaded();
        }

        private void OnSire(Vampire target)
        {
            if (target?.Creature == null || moduleVampire == null || target.Creature.isPlayer || target.sireline.Sire != moduleVampire) return;

            target.Creature.animator.speed *= 2.0f;
            boostedCreatures.Add(target.Creature);
        }

        private void OnKill(Creature target, Player player, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (target == null || target.isPlayer || !boostedCreatures.Contains(target)) return;

            target.animator.speed *= 0.5f;
            boostedCreatures.Remove(target);
        }

    }
}
