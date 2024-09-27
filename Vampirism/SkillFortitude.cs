using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillFortitude : SkillData
    {
        public Vector2 resistancePowerScale = new Vector2(0.75f, 0.2f);
        public float powerAtResistanceMax = 12345.0f;
        public bool clampResistance = false;

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            Vampire vampire = creature.AffirmVampirism();

            ModuleFortitude.skill = this;
            vampire.AddModule<ModuleFortitude>();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.RemoveModule<ModuleFortitude>();
            }
            else
            {
                ModuleFortitude fortitudeModule = creature.gameObject.GetComponent<ModuleFortitude>();
                if (fortitudeModule == null) return;

                MonoBehaviour.Destroy(fortitudeModule);
            }
        }
    }
}
