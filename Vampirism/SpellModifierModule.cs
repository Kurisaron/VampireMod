using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public struct ModifierConfig
    {
        public Vector2 modifierValueScale;
        public float powerAtModifierMax;
        public bool clampModifier;

        public static ModifierConfig DefaultModifier { get => new ModifierConfig(new Vector2(0, 1), 12345.0f, true); }

        public ModifierConfig(Vector2 valueScale, float powerMax, bool clamp)
        {
            modifierValueScale = valueScale;
            powerAtModifierMax = powerMax;
            clampModifier = clamp;
        }
    }

    public abstract class SpellModifierModule : VampireModule
    {
        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnPower);
            VampireEvents.sireEvent += new Vampire.VampireEvent(OnPower);
            moduleVampire.power.powerGainedEvent -= new Vampire.VampireEvent(OnPower);
            moduleVampire.power.powerGainedEvent += new Vampire.VampireEvent(OnPower);

            Mana vampireMana = moduleVampire?.Creature?.mana;
            if (vampireMana != null)
            {
                vampireMana.OnSpellLoadEvent -= new Mana.SpellLoadEvent(OnSpellLoad);
                vampireMana.OnSpellLoadEvent += new Mana.SpellLoadEvent(OnSpellLoad);
                vampireMana.OnSpellUnloadEvent -= new Mana.SpellLoadEvent(OnSpellUnload);
                vampireMana.OnSpellUnloadEvent += new Mana.SpellLoadEvent(OnSpellUnload);
            }

            UpdateModifiers(GetSpellsToModify());

        }

        public override void ModuleUnloaded()
        {
            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnPower);
            moduleVampire.power.powerGainedEvent -= new Vampire.VampireEvent(OnPower);

            Mana vampireMana = moduleVampire?.Creature?.mana;
            if (vampireMana != null)
            {
                vampireMana.OnSpellLoadEvent -= new Mana.SpellLoadEvent(OnSpellLoad);
                vampireMana.OnSpellUnloadEvent -= new Mana.SpellLoadEvent(OnSpellUnload);
            }

            RemoveModifiers(GetSpellsToModify());

            base.ModuleUnloaded();
        }

        protected abstract string GetSpellID();

        protected virtual Dictionary<Modifier, ModifierConfig> GetSpellModifiers() => new Dictionary<Modifier, ModifierConfig>()
        {
            { Modifier.ChargeSpeed, new ModifierConfig(new Vector2(1.0f, 100.0f), 12345.0f, false) },
            { Modifier.Intensity, new ModifierConfig(new Vector2(1.0f, 10.0f), 13370.0f, false) }
        };

        private void OnSpellLoad(SpellData spellData, SpellCaster spellCaster)
        {
            string spellID = GetSpellID();
            if (spellData == null || spellID.IsNullOrEmptyOrWhitespace() || spellData.id != spellID)
                return;

            UpdateModifiers(spellData);
        }

        private void OnSpellUnload(SpellData spellData, SpellCaster spellCaster)
        {
            string spellID = GetSpellID();
            if (spellData == null || spellID.IsNullOrEmptyOrWhitespace() || spellData.id != spellID)
                return;

            RemoveModifiers(spellData);
        }

        private void OnPower(Vampire check)
        {
            if (check == null || moduleVampire == null || check != moduleVampire)
                return;

            UpdateModifiers(GetSpellsToModify());

        }

        private SpellData[] GetSpellsToModify()
        {
            List<SpellData> validSpells = new List<SpellData>();

            string validSpellID = GetSpellID();
            SpellCastData leftSpell = moduleVampire?.Creature?.mana?.casterLeft?.spellInstance;
            if (leftSpell != null && leftSpell.id == validSpellID)
                validSpells.Add(leftSpell);
            SpellCastData rightSpell = moduleVampire?.Creature?.mana?.casterRight?.spellInstance;
            if (rightSpell != null && rightSpell.id == validSpellID)
                validSpells.Add(rightSpell);

            return validSpells.ToArray();
        }

        private void UpdateModifiers(params SpellData[] spellsToModify)
        {
            Dictionary<Modifier, ModifierConfig> spellModifiers = GetSpellModifiers();
            if (spellModifiers == null || spellModifiers.Count <= 0 || spellsToModify.Length <=  0) return;

            for (int i = 0; i < spellsToModify.Length; i++)
            {
                SpellData targetSpell = spellsToModify[i];
                if (targetSpell == null) continue;

                foreach (KeyValuePair<Modifier, ModifierConfig> modifier in spellModifiers)
                {
                    Modifier modifierType = modifier.Key;
                    ModifierConfig config = modifier.Value;

                    Vector2 valueRange = config.modifierValueScale;
                    float powerMax = config.powerAtModifierMax;
                    bool clamp = config.clampModifier;

                    float currentPower = moduleVampire?.power != null ? moduleVampire.power.PowerLevel : 0;
                    float modifierValue = clamp ? Mathf.Lerp(valueRange.x, valueRange.y, currentPower / powerMax) : Mathf.LerpUnclamped(valueRange.x, valueRange.y, currentPower / powerMax);

                    targetSpell.AddModifier(this, modifierType, modifierValue);
                    Debug.Log(GetDebugPrefix(nameof(UpdateModifiers)) + " " + targetSpell.id + " " +  modifierType.ToString() + " vampire module modifier set to " + modifierValue.ToString());
                }
            }
        }

        private void RemoveModifiers(params SpellData[] spellsToModify)
        {
            if (spellsToModify.Length <= 0) return;

            for (int i = 0; i < spellsToModify.Length; i++)
            {
                SpellData targetSpell = spellsToModify[i];
                if (targetSpell == null) continue;

                targetSpell.RemoveModifiers(this);
                Debug.Log(GetDebugPrefix(nameof(RemoveModifiers)) + " Vampire module modifiers for spell " + targetSpell.id + " removed");
            }
        }
    }
}
