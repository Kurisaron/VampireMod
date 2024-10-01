using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleCatervaCruentis : VampireModule
    {
        private List<SkillData> activeSkills = new List<SkillData>();

        public override string GetSkillID() => "CatervaCruentis";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            SkillData.OnSkillLoadedEvent -= new SkillData.SkillLoadedEvent(OnSkillLoaded);
            SkillData.OnSkillLoadedEvent += new SkillData.SkillLoadedEvent(OnSkillLoaded);
            SkillData.OnSkillUnloadedEvent -= new SkillData.SkillLoadedEvent(OnSkillUnloaded);
            SkillData.OnSkillUnloadedEvent += new SkillData.SkillLoadedEvent(OnSkillUnloaded);
            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnSired);
            VampireEvents.sireEvent += new Vampire.VampireEvent(OnSired);
        }

        public override void ModuleUnloaded()
        {
            SkillData.OnSkillLoadedEvent -= new SkillData.SkillLoadedEvent(OnSkillLoaded);
            SkillData.OnSkillUnloadedEvent -= new SkillData.SkillLoadedEvent(OnSkillUnloaded);
            VampireEvents.sireEvent -= new Vampire.VampireEvent(OnSired);

            RemoveInheritableSkills();

            base.ModuleUnloaded();
        }

        private void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            if (skillData == null || creature == null || moduleVampire?.creature == null)
                return;

            if (creature != moduleVampire.creature)
                return;

            SkillCatervaCruentis catervaCruentisSkill = GetSkill<SkillCatervaCruentis>();
            if (catervaCruentisSkill == null) return;

            List<string> inheritableSkills = catervaCruentisSkill.inheritableSkillIds;
            if (inheritableSkills == null || inheritableSkills.Count <= 0)
                return;

            if (!inheritableSkills.Contains(skillData.id))
                return;

            activeSkills.Add(skillData);
            moduleVampire.sireline.PerformSpawnAction(spawn =>
            {
                Creature spawnCreature = spawn?.creature;
                if (spawnCreature == null || spawnCreature.isPlayer) return;

                if (!spawnCreature.HasSkill(skillData))
                    spawnCreature.TryAddSkill(skillData);
            });
        }

        private void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            if (skillData == null || creature == null || moduleVampire?.creature == null)
                return;

            if (creature != moduleVampire.creature)
                return;

            SkillCatervaCruentis catervaCruentisSkill = GetSkill<SkillCatervaCruentis>();
            if (catervaCruentisSkill == null) return;

            List<string> inheritableSkills = catervaCruentisSkill.inheritableSkillIds;
            if (inheritableSkills == null || inheritableSkills.Count <= 0)
                return;

            if (!inheritableSkills.Contains(skillData.id))
                return;

            moduleVampire.sireline.PerformSpawnAction(spawn =>
            {
                Creature spawnCreature = spawn?.creature;
                if (spawnCreature == null || spawnCreature.isPlayer) return;

                if (spawnCreature.HasSkill(skillData))
                    spawnCreature.TryRemoveSkill(skillData);
            });
            activeSkills.Remove(skillData);
        }

        private void OnSired(Vampire vampire)
        {
            if (vampire?.creature == null || moduleVampire == null || vampire == moduleVampire || vampire.sireline.Sire != moduleVampire)
                return;

            if (activeSkills == null || activeSkills.Count <= 0)
                return;

            Creature spawnCreature = vampire.creature;
            foreach (SkillData skillData in activeSkills)
            {
                if (!spawnCreature.HasSkill(skillData))
                    spawnCreature.ForceLoadSkill(skillData.id);
            }
        }

        private void RemoveInheritableSkills()
        {
            if (moduleVampire == null)
                return;

            moduleVampire.sireline.PerformSpawnAction(spawn =>
            {
                Creature spawnCreature = spawn?.creature;
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
