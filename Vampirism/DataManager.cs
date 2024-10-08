﻿using Newtonsoft.Json;
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
    public class DataManager : VampireScript<DataManager>
    {

        private VampireSaveData saveData;

        private Dictionary<string, SkillTreeData> skillTrees;
        public SkillTreeData VampireSkillTree { get => GetSkillTree("Vampire"); }

        #region EVENT PUBLISHERS
        public static event PossessLoadEvent possessLoadEvent;
        public static event VampirismUnlockEvent unlockEvent;
        #endregion

        #region OVERRIDES
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);

            LoadSkillTrees();

            AddPotionToLootTable();

            EventManager.onPossess += new EventManager.PossessEvent(OnPossess);
            Vampire.sireEvent += new Vampire.SiredEvent(OnSired);
            Vampire.powerGainedEvent += new Vampire.PowerGainedEvent(OnPowerGained);
            Vampire.curedEvent += new Vampire.CuredEvent(OnCured);
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

        private void RefreshSkillTrees()
        {
            foreach (SkillTreeData skillTree in skillTrees.Values)
            {
                skillTree.showInInfuser = saveData != null ? saveData.VampirismUnlocked : false;
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
        private void UnlockVampirism(bool unlock)
        {
            AffirmSaveData(nameof(UnlockVampirism));

            if (unlock == saveData.VampirismUnlocked)
            {
                Debug.LogWarning("Did not perform unlock toggle. Wanted to " + (unlock ? "en" : "dis") + "able, but vampirism is already " + (unlock ? "" : "not ") + "unlocked");
                return;
            }

            saveData.VampirismUnlocked = unlock;

            RefreshSkillTrees();

            VampirismUnlockEvent vampirismUnlock = unlockEvent;
            if (vampirismUnlock != null)
                vampirismUnlock(unlock);

        }

        private void AddPotionToLootTable()
        {
            string potionItemId = "PotionVampireBlood";
            ItemData potionItemData = Catalog.GetData<ItemData>(potionItemId);
            if (Utils.CheckError(() => potionItemData == null, "VampireManager: Potion item data does not exist")) return;

            string[] dungeonLootIDs = { "Dalgarian_MainLoot_T1", "Dalgarian_MainLoot_T2", "Dalgarian_MainLoot_T3", "Dalgarian_SideLoot_T1", "Dalgarian_SideLoot_T2", "Dalgarian_SideLoot_T3" };
            List<LootTable> dungeonLootTables = Catalog.GetDataList<LootTable>().FindAll(lootTable => dungeonLootIDs.Contains(lootTable.id));
            if (dungeonLootTables == null || dungeonLootTables.Count <= 0) return;

            foreach (LootTable lootTable in dungeonLootTables)
            {
                if (lootTable.levelledDrops.Exists(dropLevel => dropLevel.drops.Exists(drop => drop.reference == LootTable.Drop.Reference.Item && drop.referenceID == potionItemId)))
                    continue;
                
                LootTable.Drop potionDrop = new LootTable.Drop()
                {
                    referenceID = potionItemId,
                    reference = LootTable.Drop.Reference.Item,
                    itemData = potionItemData,
                    randMode = LootTable.Drop.RandMode.ItemCount,
                    minMaxRand = new Vector2(1.0f, 1.0f),
                    probabilityWeight = 0.2f
                };
                if (potionDrop == null) continue;

                LootTable.DropLevel potionDropLevel = new LootTable.DropLevel()
                {
                    dropLevel = 0,
                    drops = { potionDrop }
                };
                if (potionDropLevel == null) continue;

                lootTable.levelledDrops.Add(potionDropLevel);
            }

        }
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
                float power = Mathf.Max(saveData.Power, 1.0f);
                vampire = creature.Vampirize(power);
                RefreshSkillTrees();
            }

            PossessLoadEvent loadEvent = possessLoadEvent;
            if (loadEvent != null)
                loadEvent(creature, vampire);

        }

        private void OnSired(Vampire vampire)
        {
            if (vampire == null || vampire.Creature == null || !vampire.Creature.isPlayer)
                return;

            UnlockVampirism(true);
        }

        private void OnPowerGained(Vampire vampire)
        {
            if (vampire == null || vampire.Creature == null || !vampire.Creature.isPlayer)
                return;

            AffirmSaveData(nameof(OnPowerGained));

            saveData.Power = vampire.Power;
        }

        private void OnCured(Creature creature)
        {
            if (creature == null || !creature.isPlayer)
                return;

            UnlockVampirism(false);
        }
        #endregion

        #region DELEGATE DEFINITIONS
        public delegate void PossessLoadEvent(Creature creature, Vampire vampire);
        public delegate void VampirismUnlockEvent(bool unlocked);
        #endregion

    }
}
