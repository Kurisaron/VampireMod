using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;

namespace Vampirism.Skill
{
    public class ModuleSire : VampireModule
    {
        public override string GetSkillID() => "Sire";

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
            SkillSire sireSkill = GetSkill<SkillSire>();
            
            if (moduleVampire == null || source == null || target == null || sireSkill == null || source != moduleVampire || target.isKilled || target.IsVampire(out _) || source.sireline.SpawnCount >= sireSkill.GetSireAmount(source)) return;

            float spawnPower = source.power != null ? (source.power.PowerLevel / 2.0f) : 1.0f;
            target.Vampirize(spawnPower / 2.0f, source);
        }

    }
}
