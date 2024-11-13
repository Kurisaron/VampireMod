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
    public class SkillTemporalSiphon : VampireSkill
    {
        public string statusId;
        [NonSerialized]
        public StatusData statusData;
        public float baseDuration = 1f;
        public Vector2 durationMultScale = new Vector2(1.0f, 10.0f);
        public float powerAtDurationMultMax = 14269.0f;
        public bool clampDurationMult = false;
        public float slowMult = 0.5f;

        public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();

            statusData = Catalog.GetData<StatusData>(statusId);
        }

        public override VampireModule CreateModule() => CreateModule<ModuleTemporalSiphon>();

    }
}
