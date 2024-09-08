using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;

namespace Vampirism.Skill
{
    public class ModuleCompel : VampireModule
    {
        protected override void Awake()
        {
            base.Awake();

            EventManager.onCreatureHit -= new EventManager.CreatureHitEvent(OnCreatureHit);
            EventManager.onCreatureHit += new EventManager.CreatureHitEvent(OnCreatureHit);
        }

        protected override void OnDestroy()
        {
            EventManager.onCreatureHit -= new EventManager.CreatureHitEvent(OnCreatureHit);

            base.OnDestroy();
        }

        private void OnCreatureHit(Creature creature, CollisionInstance collisionInstance, EventTime eventTime)
        {
            // Do not compel if the target creature is null, the module's vampire/creature is null, or the target creature is the same as module's creature
            if (creature == null || Vampire?.Creature == null || creature == Vampire.Creature) return;

            // Do not compel if the hit is not from the module's vampire
            Creature dealer = GetDealer(collisionInstance);
            if (dealer == null || dealer != Vampire.Creature) return;

            // Do not compel if the target creature is a vampire and its sire is the module's vampire
            if (creature.IsVampire(out Vampire target) && target.Sire == Vampire)
                return;

            // Compel all of the module vampire's spawn to attack the creature hit by the module vampire's attack
            Vampire.PerformSpawnAction(spawn =>
            {
                if (spawn?.Creature?.brain == null) return;

                spawn.Creature.brain.currentTarget = creature;
            });
        }

        private Creature GetDealer(CollisionInstance collisionInstance) => collisionInstance?.damageStruct.damager?.collisionHandler?.ragdollPart?.ragdoll?.creature ?? collisionInstance?.damageStruct.damager?.collisionHandler?.item?.lastHandler?.creature ?? collisionInstance?.casterHand?.ragdollHand?.ragdoll?.creature ?? null;

    }
}
