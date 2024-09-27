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
    public class SkillSiphon : SkillData
    {
        // JSON fields
        public float siphonInterval = 0.1f;
        public float siphonMouthRangeMult = 5.0f;
        public string siphonEffectID = "SiphonAudio";
        [NonSerialized]
        public EffectData siphonEffectData;
        public Vector2 siphonPowerScale = new Vector2(0.01f, 0.1f);
        public float powerAtSiphonPowerMax = 23456.0f;
        public bool clampSiphonPower = false;

        public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();

            siphonEffectData = Catalog.GetData<EffectData>(siphonEffectID);
        }

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleSiphon.skill = this;

            vampire.gameObject.AddComponent<ModuleSiphon>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            ModuleSiphon siphonModule = creature.gameObject.GetComponent<ModuleSiphon>();
            if (siphonModule == null) return;

            MonoBehaviour.Destroy(siphonModule);
        }

    }
}
