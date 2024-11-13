using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill.SpellMerge;
using UnityEngine;
using UnityEngine.Events;

namespace Vampirism.Skill
{
    public class ModuleDrainBubble : VampireModule
    {
        public override string GetSkillID() => "Vortex";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            Mana vampireMana = moduleVampire?.Creature?.mana;
            if (!Utils.CheckError(() => vampireMana == null, "No mana component found for vampire upon drain bubble module load"))
            {
                vampireMana.OnSpellLoadEvent -= new Mana.SpellLoadEvent(OnSpellLoad);
                vampireMana.OnSpellLoadEvent += new Mana.SpellLoadEvent(OnSpellLoad);
                vampireMana.OnSpellUnloadEvent -= new Mana.SpellLoadEvent(OnSpellUnload);
                vampireMana.OnSpellUnloadEvent += new Mana.SpellLoadEvent(OnSpellUnload);
            }

        }

        public override void ModuleUnloaded()
        {
            Mana vampireMana = moduleVampire?.Creature?.mana;
            if (!Utils.CheckError(() => vampireMana == null, "No mana component found for vampire upon drain bubble module load"))
            {
                vampireMana.OnSpellLoadEvent -= new Mana.SpellLoadEvent(OnSpellLoad);
                vampireMana.OnSpellUnloadEvent -= new Mana.SpellLoadEvent(OnSpellUnload);
            }

            base.ModuleUnloaded();
        }

        private void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            if (!(spell is SpellMergeGravity spellMergeGravity))
                return;

            string debugPrefix = GetDebugPrefix(nameof(OnSpellLoad));

            Debug.Log(debugPrefix + " Gravity Merge spell loaded");

            Creature castingCreature = caster?.mana?.creature;
            Creature moduleCreature = moduleVampire?.Creature;
            if (Utils.CheckError(() => castingCreature == null, debugPrefix + " Casting creature is null")
                || Utils.CheckError(() => moduleCreature == null, debugPrefix + " Module creature is null")
                || Utils.CheckError(() => moduleCreature == null, debugPrefix + " Casting creature is not module creature"))
                return;

            spellMergeGravity.OnBubbleOpen -= new SpellMergeGravity.BubbleEvent(OnBubbleOpen);
            spellMergeGravity.OnBubbleOpen += new SpellMergeGravity.BubbleEvent(OnBubbleOpen);
        }

        private void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            if (!(spell is SpellMergeGravity spellMergeGravity))
                return;

            string debugPrefix = GetDebugPrefix(nameof(OnSpellUnload));
            debugPrefix = debugPrefix ?? "[NULL]";

            Debug.Log(debugPrefix + " Gravity Merge spell unloaded");

            Creature castingCreature = caster?.mana?.creature;
            Creature moduleCreature = moduleVampire?.Creature;
            if (Utils.CheckError(() => castingCreature == null, debugPrefix + " Casting creature is null")
                || Utils.CheckError(() => moduleCreature == null, debugPrefix + " Module creature is null")
                || Utils.CheckError(() => moduleCreature == null, debugPrefix + " Casting creature is not module creature"))
                return;

            spellMergeGravity.OnBubbleOpen -= new SpellMergeGravity.BubbleEvent(OnBubbleOpen);
        }

        private void OnBubbleOpen(Mana mana, UnityEngine.Vector3 position, Zone zone)
        {
            Debug.Log(GetDebugPrefix(nameof(OnBubbleOpen)) + " Drain bubble open event start");
            moduleVampire?.StartCoroutine(BubbleSiphonRoutine(zone));
            Debug.Log(GetDebugPrefix(nameof(OnBubbleOpen)) + " Drain bubble open event end");
        }

        private IEnumerator BubbleSiphonRoutine(Zone zone)
        {
            Debug.Log(GetDebugPrefix(nameof(BubbleSiphonRoutine)) + " Bubble siphon routine start");

            Zone bubbleZone = zone;
            while (bubbleZone != null)
            {
                Debug.Log(GetDebugPrefix(nameof(BubbleSiphonRoutine)) + " Bubble siphon routine tick start");
                BubbleSiphonUpdate(bubbleZone);
                Debug.Log(GetDebugPrefix(nameof(BubbleSiphonRoutine)) + " Bubble siphon routine tick end");
                yield return new WaitForSeconds(0.1f);
            }
            
            Debug.Log(GetDebugPrefix(nameof(BubbleSiphonRoutine)) + " No bubble zone present for drain. Bubble siphon routine end");
        }

        private void BubbleSiphonUpdate(Zone bubbleZone)
        {
            if (bubbleZone == null)
            {
                Debug.LogWarning(GetDebugPrefix(nameof(BubbleSiphonUpdate)) + " Bubble zone is null");
                return;
            }
            if (moduleVampire == null)
            {
                Debug.LogWarning(GetDebugPrefix(nameof(BubbleSiphonUpdate)) + " Module vampire is null");
                return;
            }

            Dictionary<Creature, int> zoneCreatures = bubbleZone.creaturesInZone;
            if (zoneCreatures == null || zoneCreatures.Count == 0)
            {
                Debug.LogWarning(GetDebugPrefix(nameof(BubbleSiphonUpdate)) + " List of creatures in bubble zone is null or empty");
                return;
            }

            ModuleSiphon siphonModule = moduleVampire?.skill.GetModule<ModuleSiphon>("Siphon");
            if (siphonModule == null)
            {
                Debug.LogWarning(GetDebugPrefix(nameof(BubbleSiphonUpdate)) + " List of creatures in bubble zone is null or empty");
                return;
            }

            foreach (KeyValuePair<Creature, int> keyValuePair in zoneCreatures)
            {
                Creature creature = keyValuePair.Key;
                if (creature == null || creature.isPlayer || creature.isKilled) return;
                if (creature.IsVampire(out Vampire vamp) && vamp.sireline.Sire == moduleVampire) return;

                siphonModule.Siphon(moduleVampire, creature);
            }
        }

    }
}
