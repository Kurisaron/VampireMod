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
        private Coroutine coroutine;
        private static Zone bubbleZone = null;
        private static SpellMergeGravity gravityMergeSpell = null;
        public static SpellMergeGravity GravityMergeSpell
        {
            get => gravityMergeSpell;
            set
            {
                if (gravityMergeSpell != null)
                {
                    gravityMergeSpell.OnBubbleOpen -= new SpellMergeGravity.BubbleEvent(OnBubbleOpen);
                    gravityMergeSpell.OnBubbleClose -= new SpellMergeGravity.BubbleEvent(OnBubbleClose);
                }

                gravityMergeSpell = value;

                if (value != null)
                {
                    gravityMergeSpell.OnBubbleOpen -= new SpellMergeGravity.BubbleEvent(OnBubbleOpen);
                    gravityMergeSpell.OnBubbleOpen += new SpellMergeGravity.BubbleEvent(OnBubbleOpen);
                    gravityMergeSpell.OnBubbleClose -= new SpellMergeGravity.BubbleEvent(OnBubbleClose);
                    gravityMergeSpell.OnBubbleClose += new SpellMergeGravity.BubbleEvent(OnBubbleClose);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            coroutine = StartCoroutine(BubbleSiphonRoutine());
        }

        protected override void OnDestroy()
        {
            StopCoroutine(coroutine);
            
            base.OnDestroy();
        }

        private static void OnBubbleOpen(Mana mana, UnityEngine.Vector3 position, Zone zone)
        {
            bubbleZone = zone;
        }

        private static void OnBubbleClose(Mana mana, UnityEngine.Vector3 position, Zone zone)
        {
            bubbleZone = null;
        }

        private IEnumerator BubbleSiphonRoutine()
        {
            while (true)
            {
                BubbleSiphonUpdate();
                yield return new WaitForSeconds(0.1f);
            }
            
        }

        private void BubbleSiphonUpdate()
        {
            if (bubbleZone == null || Vampire == null) return;

            Dictionary<Creature, int> zoneCreatures = bubbleZone.creaturesInZone;
            if (zoneCreatures == null || zoneCreatures.Count == 0) return;

            foreach (KeyValuePair<Creature, int> keyValuePair in zoneCreatures)
            {
                Creature creature = keyValuePair.Key;
                if (creature.isPlayer || creature.isKilled) return;
                if (creature.IsVampire(out Vampire vamp) && vamp.Sire == Vampire) return;

                ModuleSiphon.Siphon(Vampire, creature);
            }
        }

    }
}
