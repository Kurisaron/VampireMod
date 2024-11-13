using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleMementoMori : VampireModule
    {
        public override string GetSkillID() => "MementoMori";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnSire);
            VampireEvents.sireEvent += new Vampire.VampireEvent(OnSire);
        }

        public override void ModuleUnloaded()
        {
            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnSire);
            
            base.ModuleUnloaded();
        }

        private void OnSire(Vampire newVampire)
        {
            if (newVampire == null || moduleVampire == null || newVampire == moduleVampire || newVampire.sireline.Sire != moduleVampire) return;

            Creature creature = newVampire.Creature;
            if (creature == null) return;

            creature.OnKillEvent -= new Creature.KillEvent(OnDeath);
            creature.OnKillEvent += new Creature.KillEvent(OnDeath);
        }

        private void OnDeath(CollisionInstance collisionInstance, EventTime eventTime)
        {
            Creature killedCreature = collisionInstance?.damageStruct.hitRagdollPart?.ragdoll?.creature;
            SkillMementoMori mementoMoriSkill = GetSkill<SkillMementoMori>();
            if (killedCreature == null || mementoMoriSkill == null || !killedCreature.isKilled) return;

            killedCreature.OnKillEvent -= new Creature.KillEvent(OnDeath);

            List<Creature> areaOfEffectTargets = Creature.allActive.FindAll(check =>
            {
                return check != null &&
                    !check.pooled &&
                    check != killedCreature &&
                    Vector3.Distance(check.transform.position, killedCreature.transform.position) <= mementoMoriSkill.mementoMoriRange;
            });
            if (areaOfEffectTargets != null && areaOfEffectTargets.Count > 0)
            {
                foreach (Creature target in areaOfEffectTargets)
                {
                    // Skip the target creature if it fails basic checks
                    if (target == null || target.isKilled || target.isPlayer || target == killedCreature) continue;

                    // Skip the target creature if it is a vampire in the same sireline as either the module vampire or the killed vampire
                    if (target.IsVampire(out Vampire targetVampire))
                    {
                        Vampire.SireManager targetSireline = targetVampire.sireline;

                        if (targetSireline.HasSpawn(moduleVampire) || targetSireline.Sire == moduleVampire)
                            continue;
                        if (killedCreature.IsVampire(out Vampire killedVampire))
                        {
                            if (targetSireline.HasSpawn(killedVampire) || targetSireline.Sire == killedVampire)
                                continue;
                        }
                    }

                    // All target checks passed

                    // Force the target to panic
                    BrainModuleFear fearModule = target?.brain?.instance?.GetModule<BrainModuleFear>();
                    if (fearModule == null) continue;

                    fearModule.Panic();
                }
            }

            Ragdoll ragdoll = killedCreature.ragdoll;
            if (ragdoll != null)
            {
                foreach (RagdollPart part in ragdoll.parts)
                {
                    if (part.sliceAllowed)
                        ragdoll.TrySlice(part);
                }
            }

        }

    }
}
