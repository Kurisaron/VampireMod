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
            if (skillData == null || creature == null || moduleVampire?.Creature == null)
                return;

            if (creature != moduleVampire.Creature)
                return;

            SkillCatervaCruentis catervaCruentisSkill = GetSkill<SkillCatervaCruentis>();
            if (catervaCruentisSkill == null)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnSkillLoaded)) + " Skill data is not present");
                return;
            }

            List<string> inheritableSkills = catervaCruentisSkill.inheritableSkillIDs;
            if (inheritableSkills == null || inheritableSkills.Count <= 0)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnSkillLoaded)) + " Skill data does not contain any IDs for skills to inherit");
                return;
            }
                
            if (!inheritableSkills.Contains(skillData.id))
                return;

            moduleVampire.sireline.PerformSpawnAction(spawn =>
            {
                Creature spawnCreature = spawn?.Creature;
                if (spawnCreature == null || spawnCreature.isPlayer) return;

                AddSkillToSpawn(spawnCreature, skillData.id);
                    
            });
        }

        private void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            if (skillData == null || creature == null || moduleVampire?.Creature == null)
                return;

            if (creature != moduleVampire.Creature)
                return;

            SkillCatervaCruentis catervaCruentisSkill = GetSkill<SkillCatervaCruentis>();
            if (catervaCruentisSkill == null)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnSkillUnloaded)) + " Skill data is not present");
                return;
            }

            List<string> inheritableSkills = catervaCruentisSkill.inheritableSkillIDs;
            if (inheritableSkills == null || inheritableSkills.Count <= 0)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnSkillUnloaded)) + " Skill data does not contain any IDs for skills to inherit");
                return;
            }

            if (!inheritableSkills.Contains(skillData.id))
                return;

            moduleVampire.sireline.PerformSpawnAction(spawn =>
            {
                Creature spawnCreature = spawn?.Creature;
                if (spawnCreature == null || spawnCreature.isPlayer) return;

                RemoveSkillFromSpawn(spawnCreature, skillData.id);
            });
        }

        private void OnSired(Vampire spawn)
        {
            if (spawn?.Creature == null || moduleVampire == null || spawn == moduleVampire || spawn.sireline.Sire != moduleVampire)
                return;

            Creature sireCreature = moduleVampire?.Creature;
            if (sireCreature == null)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnSired)) + " Sire creature is not present");
                return;
            }

            SkillCatervaCruentis skill = GetSkill<SkillCatervaCruentis>();
            if (skill == null)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnSired)) + " Skill data is not present");
                return;
            }

            List<string> inheritableSkills = skill.inheritableSkillIDs;
            if (inheritableSkills == null || inheritableSkills.Count <= 0)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnSired)) + " Skill data does not contain any IDs for skills to inherit");
                return;
            }
            List<string> activeSkills = inheritableSkills.FindAll(skillID => sireCreature.HasSkill(skillID));
            if (activeSkills == null ||  activeSkills.Count <= 0)
            {
                Debug.LogError(GetDebugPrefix(nameof(OnSired)) + " Sire creature does not have any skills for spawn to inherit");
                return;
            }

            Creature spawnCreature = spawn.Creature;
            foreach (string skillID in activeSkills)
            {
                AddSkillToSpawn(spawnCreature, skillID);
            }
        }

        private void RemoveInheritableSkills()
        {
            if (moduleVampire == null)
                return;

            Creature sireCreature = moduleVampire?.Creature;
            if (sireCreature == null)
            {
                Debug.LogError(GetDebugPrefix(nameof(RemoveInheritableSkills)) + " Sire creature is not present");
                return;
            }

            SkillCatervaCruentis skill = GetSkill<SkillCatervaCruentis>();
            if (skill == null)
            {
                Debug.LogError(GetDebugPrefix(nameof(RemoveInheritableSkills)) + " Skill data is not present");
                return;
            }

            List<string> inheritableSkills = skill.inheritableSkillIDs;
            if (inheritableSkills == null || inheritableSkills.Count <= 0)
            {
                Debug.LogError(GetDebugPrefix(nameof(RemoveInheritableSkills)) + " Skill data does not contain any IDs for skills to inherit");
                return;
            }
            List<string> activeSkills = inheritableSkills.FindAll(skillID => sireCreature.HasSkill(skillID));
            if (activeSkills == null || activeSkills.Count <= 0)
            {
                Debug.LogError(GetDebugPrefix(nameof(RemoveInheritableSkills)) + " Sire creature does not have any skills for spawn to inherit");
                return;
            }


            moduleVampire.sireline.PerformSpawnAction(spawn =>
            {
                Creature spawnCreature = spawn?.Creature;
                if (spawnCreature == null || spawnCreature.isPlayer) return;
                
                foreach (string skillID in activeSkills)
                {
                    RemoveSkillFromSpawn(spawnCreature, skillID);
                }
            });

        }

        private void AddSkillToSpawn(Creature spawnCreature, string skillID)
        {
            Debug.Log(GetDebugPrefix(nameof(AddSkillToSpawn)) + " Attempting to add skill " + (skillID ?? "NULL") + " to spawn");
            
            if (spawnCreature == null)
            {
                Debug.Log(GetDebugPrefix(nameof(AddSkillToSpawn)) + " Spawn creature is null");
                return;
            }

            if (!spawnCreature.HasSkill(skillID))
            {
                spawnCreature.TryAddSkill(skillID);
                Debug.Log(GetDebugPrefix(nameof(AddSkillToSpawn)) + " Spawn " + (spawnCreature?.gameObject?.name ?? "NULL") + " now has skill " + (skillID ?? "NULL"));
            }
        }

        private void RemoveSkillFromSpawn(Creature spawnCreature, string skillID)
        {
            Debug.Log(GetDebugPrefix(nameof(RemoveSkillFromSpawn)) + " Attempting to remove skill " + (skillID ?? "NULL") + " from spawn");

            if (spawnCreature == null)
            {
                Debug.Log(GetDebugPrefix(nameof(RemoveSkillFromSpawn)) + " Spawn creature is null");
                return;
            }

            if (spawnCreature.HasSkill(skillID))
            {
                spawnCreature.TryRemoveSkill(skillID);
                Debug.Log(GetDebugPrefix(nameof(RemoveSkillFromSpawn)) + " Spawn " + (spawnCreature?.gameObject?.name ?? "NULL") + " no longer has skill " + (skillID ?? "NULL"));
            }
        }
    }
}
