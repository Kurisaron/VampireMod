using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using static ThunderRoad.CreatureSpawner;

namespace Vampirism.Skill
{
    public class ModuleCatervaCruentis : VampireModule
    {
        public static SkillCatervaCruentis skill;
        private List<SkillData> activeSkills = new List<SkillData>();

        protected override void Awake()
        {
            base.Awake();

            SkillData.OnSkillLoadedEvent -= new SkillData.SkillLoadedEvent(OnSkillLoaded);
            SkillData.OnSkillLoadedEvent += new SkillData.SkillLoadedEvent(OnSkillLoaded);
            SkillData.OnSkillUnloadedEvent -= new SkillData.SkillLoadedEvent(OnSkillUnloaded);
            SkillData.OnSkillUnloadedEvent += new SkillData.SkillLoadedEvent(OnSkillUnloaded);
            Vampire.sireEvent -= new Vampire.SiredEvent(OnSired);
            Vampire.sireEvent += new Vampire.SiredEvent(OnSired);
        }

        protected override void OnDestroy()
        {
            SkillData.OnSkillLoadedEvent -= new SkillData.SkillLoadedEvent(OnSkillLoaded);
            SkillData.OnSkillUnloadedEvent -= new SkillData.SkillLoadedEvent(OnSkillUnloaded);
            Vampire.sireEvent -= new Vampire.SiredEvent(OnSired);

            RemoveInheritableSkills();

            base.OnDestroy();
        }

        private void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            if (skillData == null || creature == null || Vampire?.Creature == null)
                return;

            if (creature != Vampire.Creature)
                return;

            List<string> inheritableSkills = skill.inheritableSkillIds;
            if (inheritableSkills == null || inheritableSkills.Count <= 0)
                return;

            if (!inheritableSkills.Contains(skillData.id))
                return;

            activeSkills.Add(skillData);
            Vampire.PerformSpawnAction(spawn =>
            {
                Creature spawnCreature = spawn?.Creature;
                if (spawnCreature == null || spawnCreature.isPlayer) return;

                if (!spawnCreature.HasSkill(skillData))
                    spawnCreature.ForceLoadSkill(skillData.id);
            });
        }

        private void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            if (skillData == null || creature == null || Vampire?.Creature == null)
                return;

            if (creature != Vampire.Creature)
                return;

            List<string> inheritableSkills = skill.inheritableSkillIds;
            if (inheritableSkills == null || inheritableSkills.Count <= 0)
                return;

            if (!inheritableSkills.Contains(skillData.id))
                return;

            Vampire.PerformSpawnAction(spawn =>
            {
                Creature spawnCreature = spawn?.Creature;
                if (spawnCreature == null || spawnCreature.isPlayer) return;

                if (spawnCreature.HasSkill(skillData))
                    spawnCreature.ForceUnloadSkill(skillData.id);
            });
            activeSkills.Remove(skillData);
        }

        private void OnSired(Vampire vampire)
        {
            if (vampire == null || vampire.Creature == null || Vampire == null || vampire == Vampire || vampire.Sire != Vampire)
                return;

            if (activeSkills == null || activeSkills.Count <= 0)
                return;

            Creature spawnCreature = vampire.Creature;
            foreach (SkillData skillData in activeSkills)
            {
                if (!spawnCreature.HasSkill(skillData))
                    spawnCreature.ForceLoadSkill(skillData.id);
            }
        }

        private void RemoveInheritableSkills()
        {
            if (Vampire == null)
                return;

            Vampire.PerformSpawnAction(spawn =>
            {
                Creature spawnCreature = spawn?.Creature;
                if (spawnCreature == null || spawnCreature.isPlayer) return;
                foreach (SkillData skillData in activeSkills)
                {
                    if (!spawnCreature.HasSkill(skillData))
                        spawnCreature.ForceLoadSkill(skillData.id);
                }
            });

            activeSkills.Clear();
        }
    }
}
