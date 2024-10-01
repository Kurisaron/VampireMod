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
        public override string GetSkillID() => "DrainBubble";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            Mana vampireMana = moduleVampire?.creature?.mana;
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
            Mana vampireMana = moduleVampire?.creature?.mana;
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

            Creature castingCreature = caster?.mana?.creature;
            Creature moduleCreature = moduleVampire?.creature;
            if (castingCreature == null || moduleCreature == null || castingCreature != moduleCreature)
                return;

            spellMergeGravity.OnBubbleOpen -= new SpellMergeGravity.BubbleEvent(OnBubbleOpen);
            spellMergeGravity.OnBubbleOpen += new SpellMergeGravity.BubbleEvent(OnBubbleOpen);
        }

        private void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            if (!(spell is SpellMergeGravity spellMergeGravity))
                return;

            Creature castingCreature = caster?.mana?.creature;
            Creature moduleCreature = moduleVampire?.creature;
            if (castingCreature == null || moduleCreature == null || castingCreature != moduleCreature)
                return;

            spellMergeGravity.OnBubbleOpen -= new SpellMergeGravity.BubbleEvent(OnBubbleOpen);
        }

        private void OnBubbleOpen(Mana mana, UnityEngine.Vector3 position, Zone zone)
        {
            moduleVampire?.StartCoroutine(BubbleSiphonRoutine(zone));
        }

        private IEnumerator BubbleSiphonRoutine(Zone zone)
        {
            Zone bubbleZone = zone;
            while (bubbleZone != null)
            {
                BubbleSiphonUpdate(bubbleZone);
                yield return new WaitForSeconds(0.1f);
            }
            
        }

        private void BubbleSiphonUpdate(Zone bubbleZone)
        {
            if (bubbleZone == null || moduleVampire == null) return;

            Dictionary<Creature, int> zoneCreatures = bubbleZone.creaturesInZone;
            if (zoneCreatures == null || zoneCreatures.Count == 0) return;

            ModuleSiphon siphonModule = moduleVampire?.skill.GetModule<ModuleSiphon>("Siphon");
            if (siphonModule == null) return;

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
