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
        private bool isCursed = false;
        public bool IsCursed
        {
            get => isCursed;
            set
            {
                isCursed = value;

                Creature creature = Vampire?.Creature;
                if (creature == null) return;

                creature.OnKillEvent -= new Creature.KillEvent(OnDeath);
                if (isCursed)
                {
                    creature.OnKillEvent += new Creature.KillEvent(OnDeath);
                }
            }
        }
        
        protected override void Awake()
        {
            base.Awake();

            Vampire.sireEvent -= new Vampire.SiredEvent(OnSire);
            Vampire.sireEvent += new Vampire.SiredEvent(OnSire);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Vampire.sireEvent -= new Vampire.SiredEvent(OnSire);
        }

        private void OnSire(Vampire newVampire)
        {
            if (newVampire == null || Vampire == null || newVampire == Vampire || newVampire.Sire != Vampire) return;

            ModuleMementoMori newSkill = newVampire.AddModule<ModuleMementoMori>();
            newSkill.IsCursed = true;
        }

        private void OnDeath(CollisionInstance collisionInstance, EventTime eventTime)
        {
            Creature sourceCreature = Vampire?.Creature;
            if (sourceCreature == null) return;

            List<Creature> targets = Creature.allActive.FindAll(check => check != null && !check.pooled && check != sourceCreature && Vector3.Distance(check.transform.position, sourceCreature.transform.position) <= 5.0f);
            foreach (Creature target in targets)
            {
                // Vampires that are within the same sire line as the module vampire are not affected by this ability
                if (target.IsVampire(out Vampire vampireTarget) && (vampireTarget.Sire == Vampire.Sire || vampireTarget.Sire == Vampire || vampireTarget == Vampire.Sire)) continue;
                
                BrainModuleFear fearModule = target?.brain?.instance?.GetModule<BrainModuleFear>();
                if (fearModule == null) continue;

                fearModule.Panic();
            }

            Ragdoll ragdoll = sourceCreature.ragdoll;
            if (ragdoll == null) return;

            foreach (RagdollPart part in ragdoll.parts)
            {
                if (part.sliceAllowed)
                    ragdoll.TrySlice(part);
            }
        }
    }
}
