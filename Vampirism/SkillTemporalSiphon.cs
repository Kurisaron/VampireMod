using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillTemporalSiphon : SkillData
    {
        public string statusId;
        [NonSerialized]
        public StatusData statusData;
        public float duration = 1f;
        public float slowMult = 0.5f;

        public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();

            statusData = Catalog.GetData<StatusData>(statusId);
        }

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            vampire.AddModule<ModuleTemporalSiphon>();
            ModuleTemporalSiphon.skill = this;
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            ModuleTemporalSiphon.skill = null;
            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleTemporalSiphon>();
            }
            else
            {
                ModuleTemporalSiphon temporalModule = creature.gameObject.GetComponent<ModuleTemporalSiphon>();
                if (temporalModule == null) return;

                MonoBehaviour.Destroy(temporalModule);
            }
        }
    }
}
