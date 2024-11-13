using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.AI.Action;
using UnityEngine;

namespace Vampirism.Skill
{
    public abstract class DiscipleModule : VampireModule
    {
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

        protected abstract string GetSpellID();

        private void OnSire(Vampire check)
        {
            if (check == null || moduleVampire == null || check.sireline.Sire != moduleVampire) return;

            Container spawnContainer = check?.Creature?.container;
            Mana spawnMana = check?.Creature?.mana;
            string spellID = GetSpellID();
            if (spawnContainer != null)
            {
                if (!check.isPlayer)
                {
                    BrainModuleCast brainCaster = check.Creature?.brain?.instance.GetModule<BrainModuleCast>(true);
                    if (brainCaster == null)
                        Debug.LogError(GetDebugPrefix(nameof(OnSire)) + " Non-player spawn does not have and could not add a BrainModuleCast");
                }
                SpellContent spellContent = spawnContainer?.AddSpellContent(spellID);
                SpellData spellData = Catalog.GetData<SpellData>(spellID);
                if (spellData != null)
                    spawnMana?.AddSpell(spellData);
                OnSpellAdded(check, spellContent?.data);
            }
            else
                Debug.LogError("Spawn does not exist, or it is not attached to a creature or content container");
            
        }

        protected virtual void OnSpellAdded(Vampire spawn, SpellData spell) { }
    }
}
