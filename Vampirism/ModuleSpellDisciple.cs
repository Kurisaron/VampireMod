using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleSpellDisciple : VampireModule
    {
        protected List<SpellData> spells;

        protected override void Awake()
        {
            base.Awake();

            Vampire.sireEvent -= new Vampire.SiredEvent(OnSire);
            Vampire.sireEvent += new Vampire.SiredEvent(OnSire);
        }

        protected override void OnDestroy()
        {
            Vampire.sireEvent -= new Vampire.SiredEvent(OnSire);

            base.OnDestroy();
        }

        public void AddSpell(string id)
        {
            if (Catalog.TryGetData(id, out SpellData spellData) && !spells.Contains(spellData))
            {
                spells.Add(spellData);
                Vampire?.PerformSpawnAction(spawn => spawn?.Creature?.container?.AddSpellContent(spellData));
            }
            else
                Debug.LogError("No spell of id " + id + " found in catalog");
        }

        public void RemoveSpell(string id)
        {
            if (spells.Exists(spell => spell.id == id))
            {
                SpellData spellData = spells.Find(spell => spell.id == id);
                spells.Remove(spellData);
            }
        }

        private void OnSire(Vampire check)
        {
            if (check == null || Vampire == null || check.Sire != Vampire) return;

            foreach (SpellData spellData in spells)
            {
                check?.Creature?.container?.AddSpellContent(spellData);
            }
        }
    }
}
