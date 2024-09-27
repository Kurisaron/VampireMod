using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleEmpoweredSpell : VampireModule
    {
        protected Dictionary<SpellData, Dictionary<Modifier, EmpoweredSpellModifier>> spells;

        protected override void Awake()
        {
            base.Awake();

            Vampire.sireEvent -= new Vampire.SiredEvent(UpdateModifiers);
            Vampire.sireEvent += new Vampire.SiredEvent(UpdateModifiers);
            Vampire.powerGainedEvent -= new Vampire.PowerGainedEvent(UpdateModifiers);
            Vampire.powerGainedEvent += new Vampire.PowerGainedEvent(UpdateModifiers);

            UpdateModifiers();
        }

        protected override void OnDestroy()
        {
            Vampire.sireEvent -= new Vampire.SiredEvent(UpdateModifiers);
            Vampire.powerGainedEvent -= new Vampire.PowerGainedEvent(UpdateModifiers);

            base.OnDestroy();
        }

        public void AddSpell(string id, Dictionary<Modifier, EmpoweredSpellModifier> modifiers)
        {
            if (Catalog.TryGetData(id, out SpellData spellData) && !spells.ContainsKey(spellData))
            {
                spells.Add(spellData, modifiers);
                UpdateModifiers();
            }
            else
                Debug.LogError("No spell of id " + id + " found in catalog");
        }

        public void RemoveSpell(string id)
        {
            if (Catalog.TryGetData(id, out SpellData spellData) && spells.ContainsKey(spellData))
            {
                foreach (KeyValuePair<Modifier, EmpoweredSpellModifier> modifier in spells[spellData])
                {
                    spellData.RemoveModifier(this, modifier.Key);
                }
                spells.Remove(spellData);
            }
            else
                Debug.LogError("No spell of id " + id + " found in catalog or among empowered spells");

        }

        private void UpdateModifiers(Vampire vampire = null)
        {
            if (vampire == null)
            {
                vampire = Vampire;
                if (vampire == null) return;
            }
            else if (vampire != Vampire)
                return;

            if (spells == null || spells.Count <= 0)
                return;

            foreach (KeyValuePair<SpellData, Dictionary<Modifier, EmpoweredSpellModifier>> spell in spells)
            {
                SpellData spellData = spell.Key;
                if (spellData == null)
                    continue;
                Dictionary<Modifier, EmpoweredSpellModifier> modifiers = spell.Value;
                if (modifiers == null || modifiers.Count <= 0)
                    continue;

                foreach (KeyValuePair<Modifier, EmpoweredSpellModifier> modifier in modifiers)
                {
                    Modifier modifierType = modifier.Key;
                    EmpoweredSpellModifier spellModifier = modifier.Value;
                    if (spellModifier == null) continue;
                    float modifierValue = spellModifier.clampModifier ? Mathf.Lerp(spellModifier.modifierValueScale.x, spellModifier.modifierValueScale.y, Vampire.Power / spellModifier.powerAtModifierMax) : Mathf.LerpUnclamped(spellModifier.modifierValueScale.x, spellModifier.modifierValueScale.y, Vampire.Power / spellModifier.powerAtModifierMax);
                    spellData.AddModifier(this, modifierType, modifierValue);
                }
            }
        }

    }
}
