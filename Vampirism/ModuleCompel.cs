using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleCompel : VampireModule
    {

        public override string GetSkillID() => "Compel";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            EventManager.onCreatureHit -= new EventManager.CreatureHitEvent(OnCreatureHit);
            EventManager.onCreatureHit += new EventManager.CreatureHitEvent(OnCreatureHit);
        }

        public override void ModuleUnloaded()
        {
            EventManager.onCreatureHit -= new EventManager.CreatureHitEvent(OnCreatureHit);

            base.ModuleUnloaded();
        }

        private void OnCreatureHit(Creature target, CollisionInstance collisionInstance, EventTime eventTime)
        {
            // Do not compel if the target creature is null, the module's vampire/creature is null, or the target creature is the same as module's creature
            if (target == null || moduleVampire?.Creature == null || target == moduleVampire.Creature) return;

            // Do not compel if the hit is not from the module's vampire
            Creature dealer = GetDealer(collisionInstance);
            if (Utils.CheckError(() => dealer == null , "Compel hit event: Did not find valid damager dealer") || Utils.CheckError(() => dealer != moduleVampire.Creature, "Compel hit event: Damager dealer is not the current module's vampire")) return;

            // Do not compel if the target creature is a vampire and its sire is the module's vampire
            if (target.IsVampire(out Vampire targetVampire) && targetVampire.sireline.Sire == moduleVampire)
                return;

            // Compel all of the module vampire's spawn to attack the creature hit by the module vampire's attack
            moduleVampire.sireline.PerformSpawnAction(spawn =>
            {
                Brain brain = spawn?.Creature?.brain;
                if (brain == null) return;

                brain.currentTarget = target;
                brain.ResetBrain();

                Debug.Log(GetDebugPrefix(nameof(OnCreatureHit)) + " Spawn " + spawn.Creature.gameObject.name + " compelled to attack " + target.gameObject.name);
            });
        }

        private Creature GetDealer(CollisionInstance collisionInstance) => collisionInstance?.damageStruct.damager?.collisionHandler?.ragdollPart?.ragdoll?.creature ?? collisionInstance?.damageStruct.damager?.collisionHandler?.item?.lastHandler?.creature ?? collisionInstance?.casterHand?.ragdollHand?.ragdoll?.creature ?? null;

    }
}
