using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public class VampireManager : VampireScript<VampireManager>
    {

        private VampireSaveData saveData;

        private Dictionary<string, SkillTreeData> skillTrees;
        public SkillTreeData VampireSkillTree { get => GetSkillTree("Vampire"); }
        #region OVERRIDES
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);

            LoadSkillTrees();

            EventManager.onPossess += new EventManager.PossessEvent(OnPossess);
            Vampire.xpEarnedEvent += new Vampire.XPEarnedEvent(OnXPEarned);
            Vampire.levelEvent += new Vampire.LevelEvent(OnLevelUp);
        }

        public override void ScriptUpdate()
        {
            base.ScriptUpdate();
        }
        #endregion

        #region SAVE DATA
        private void AffirmSaveData(string sourceFunction = "")
        {
            string functionName = "[" + sourceFunction + "->" + nameof(AffirmSaveData) + "]";

            if (saveData != null)
            {
                Debug.Log(functionName + " Save data already present.");
                return;
            }

            if (VampireSaveData.TryLoadSave(Player.characterData, out saveData))
                Debug.Log(functionName + " Save data loaded from json file");
            else
                Debug.LogWarning(functionName + " Could not load data from json file, creating new save data instead");
        }
        #endregion

        #region SKILL TREES
        private void LoadSkillTrees()
        {
            skillTrees = new Dictionary<string, SkillTreeData>();
            string[] treeIDs = { "Vampire" };
            for (int i = 0; i < treeIDs.Length; i++)
            {
                GetSkillTree(treeIDs[i]).showInInfuser = false;
            }
        }

        private SkillTreeData GetSkillTree(string ID)
        {
            bool contains = skillTrees.ContainsKey(ID);
            SkillTreeData skillTreeData = null;
            if (!contains)
            {
                bool treeInCatalog = Catalog.TryGetData<SkillTreeData>(ID, out skillTreeData);
                if (treeInCatalog)
                {
                    skillTrees.Add(ID, skillTreeData);
                    Debug.Log("Skill tree " + ID + " found from catalog");
                }
                else
                    Debug.LogError("Skill tree " + ID + " could not be found in the catalog");
            }
            else
                skillTreeData = skillTrees[ID];

            return skillTreeData;
        }
        #endregion

        #region VAMPIRISM UNLOCKING
        /// <summary>
        /// Set vampirism unlock to the desired state for the current save data
        /// </summary>
        /// <param name="unlock"></param>
        /// <returns>True = toggle to desired state was performed and successful</returns>
        public bool UnlockVampirism(bool unlock)
        {
            AffirmSaveData(nameof(UnlockVampirism));

            if (unlock == saveData.VampirismUnlocked)
            {
                Debug.LogWarning("Did not perform unlock toggle. Wanted to " + (unlock ? "en" : "dis") + "able, but vampirism is already " + (unlock ? "" : "not ") + "unlocked");
                return false;
            }

            saveData.VampirismUnlocked = unlock;

            VampirismUnlockEvent vampirismUnlock = unlockEvent;
            if (vampirismUnlock != null)
                vampirismUnlock(unlock);

            return true;
        }
        #endregion

        #region EVENT PUBLISHERS
        public static event PossessLoadEvent possessLoadEvent;
        public static event VampirismUnlockEvent unlockEvent;
        #endregion

        #region EVENT SUBSCRIBERS
        private void OnPossess(Creature creature, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart || !creature.isPlayer)
                return;

            AffirmSaveData(nameof(OnPossess));

            Vampire vampire = null;
            if (saveData.VampirismUnlocked)
            {
                int level = Math.Max(saveData.Level, 1);
                float xp = Mathf.Max(saveData.XP, 0.0f);
                vampire = creature.Vampirize(level, xp);
                SkillTreeData vampireTree = VampireSkillTree;
                vampireTree.showInInfuser = true;
            }

            PossessLoadEvent loadEvent = possessLoadEvent;
            if (loadEvent != null)
                loadEvent(creature, vampire);

        }

        private void OnXPEarned(Vampire vampire)
        {
            if (vampire == null || vampire.Creature == null || !vampire.Creature.isPlayer)
                return;

            AffirmSaveData(nameof(OnXPEarned));

            saveData.XP = vampire.XP;
        }

        private void OnLevelUp(Vampire vampire)
        {
            if (vampire == null || vampire.Creature == null || !vampire.Creature.isPlayer)
                return;

            AffirmSaveData(nameof(OnLevelUp));

            saveData.Level = vampire.CurrentLevel;
        }
        #endregion

        #region DELEGATE DEFINITIONS
        public delegate void PossessLoadEvent(Creature creature, Vampire vampire);
        public delegate void VampirismUnlockEvent(bool unlocked);
        #endregion

    }
}
