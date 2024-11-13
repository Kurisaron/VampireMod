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
    public class ModuleTemporalSiphon : VampireModule
    {
        public override string GetSkillID() => "TemporalSiphon";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            VampireEvents.siphonEvent -= new VampireEvents.SiphonEvent(OnSiphon);
            VampireEvents.siphonEvent += new VampireEvents.SiphonEvent(OnSiphon);
        }

        public override void ModuleUnloaded()
        {
            VampireEvents.siphonEvent -= new VampireEvents.SiphonEvent(OnSiphon);
            
            base.ModuleUnloaded();
        }

        private void OnSiphon(Vampire source, Creature target, float damage)
        {
            SkillTemporalSiphon temporalSiphonSkill = GetSkill<SkillTemporalSiphon>();
            if (source == null || moduleVampire == null || target == null || temporalSiphonSkill == null || source != moduleVampire) return;

            if (target.IsVampire(out Vampire vampire) && vampire.sireline.Sire == moduleVampire)
                return;

            Vector2 durationMultScale = temporalSiphonSkill.durationMultScale;
            float durationPowerScale = moduleVampire.power.PowerLevel / temporalSiphonSkill.powerAtDurationMultMax;
            float slowDuration = temporalSiphonSkill.clampDurationMult ? Mathf.Lerp(durationMultScale.x, durationMultScale.y, durationPowerScale) : Mathf.LerpUnclamped(durationMultScale.x, durationMultScale.y, durationPowerScale);

            target.Inflict(temporalSiphonSkill.statusData, this, slowDuration, temporalSiphonSkill.slowMult);
        }
    }
}
