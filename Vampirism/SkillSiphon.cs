using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public class SkillSiphon : SkillData
    {

        private float siphonInterval = 0.1f;
        private float siphonRange = 0.3f;
        
        public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();

        }

        public override void OnSkillUnlocked(SkillData skillData, Creature creature)
        {
            base.OnSkillUnlocked(skillData, creature);

        }

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

        }

        private IEnumerator SiphonRoutine(Vampire vampire)
        {
            while (vampire != null && vampire.Creature != null) 
            {
                Siphon();
                yield return new WaitForSeconds(siphonInterval);
            }

            // Local function for Siphon Coroutine to re-use
            void Siphon()
            {
                Physics.OverlapSphere(vampire.Creature.jaw.position, siphonRange).ToList().FindAll(collider => collider.gameObject.)
            }
        }

    }
}
