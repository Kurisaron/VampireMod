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
    public class DataManager : VampireScript<DataManager>
    {

        private VampireSaveData saveData;
        
        #region EVENT PUBLISHERS
        public static event PossessLoadEvent possessLoadEvent;
        #endregion

        #region OVERRIDES
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);

            //AddPotionToLootTable();

            EventManager.onPossess += new EventManager.PossessEvent(OnPossess);

            VampireEvents.sireEvent += new Vampire.VampireEvent(OnSired);
            VampireEvents.curedEvent += new VampireEvents.CuredEvent(OnCured);
            VampireEvents.powerGainedEvent += new Vampire.VampireEvent(OnPowerGained);
        }

        public override void ScriptUpdate()
        {
            base.ScriptUpdate();
        }
        #endregion

        #region VAMPIRISM UNLOCKING
        private void AddPotionToLootTable()
        {
            string potionItemId = "PotionVampireBlood";
            ItemData potionItemData = Catalog.GetData<ItemData>(potionItemId);
            if (Utils.CheckError(() => potionItemData == null, "VampireManager: Potion item data does not exist")) return;

            string[] dungeonLootIDs = { "Dalgarian_MainLoot_T1", "Dalgarian_MainLoot_T2", "Dalgarian_MainLoot_T3", "Dalgarian_SideLoot_T1", "Dalgarian_SideLoot_T2", "Dalgarian_SideLoot_T3" };
            List<LootTable> dungeonLootTables = Catalog.GetDataList<LootTable>().FindAll(lootTable => lootTable != null && dungeonLootIDs.Contains(lootTable.id));
            if (dungeonLootTables == null || dungeonLootTables.Count <= 0) return;

            foreach (LootTable lootTable in dungeonLootTables)
            {
                if (lootTable == null) continue;
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

            string functionName = this.GetDebugID(nameof(OnPossess));

            ValidateSave();

            Vampire vampire = null;
            if (!Utils.CheckError(() => saveData == null, functionName + " Save data is still null despite attempting to create a new one from defaults") && (saveData.IsVampire || creature.IsVampire(out vampire)))
            {
                if (creature.IsVampire(out vampire) && !saveData.IsVampire)
                {
                    Debug.LogWarning(functionName + " Player creature is already a vampire despite save data not saying it should be, updating save data to match");
                    saveData.IsVampire = true;
                }
                    
                if (saveData.IsVampire)
                {
                    Debug.Log(functionName + " Player should be vampire according to save " + (saveData?.ID ?? "NULL"));
                    float power = Mathf.Max(saveData.Power, 1.0f);
                    vampire = creature.Vampirize(power);
                }
            }

            PossessLoadEvent loadEvent = possessLoadEvent;
            if (loadEvent != null)
                loadEvent(creature, vampire);

        }

        private void OnSired(Vampire vampire)
        {
            string functionName = this.GetDebugID(nameof(OnSired));

            if (Utils.CheckError(() => vampire == null, functionName + " Vampire is null")) return;
            if (Utils.CheckError(() => !vampire.isPlayer, functionName + " Vampire is not player")) return;

            ValidateSave();

            saveData.IsVampire = true;
        }

        private void OnCured(Creature creature, EventTime eventTime)
        {
            string functionName = this.GetDebugID(nameof(OnCured));

            if (Utils.CheckError(() => creature == null, functionName + " Creature is null")) return;
            if (Utils.CheckError(() => !creature.isPlayer, functionName + " Creature is not player")) return;

            ValidateSave();

            saveData.IsVampire = false;
        }

        private void OnPowerGained(Vampire vampire)
        {
            string functionName = this.GetDebugID(nameof(OnPowerGained));

            if (Utils.CheckError(() => vampire == null, functionName + " Vampire is null")) return;
            if (Utils.CheckError(() => !vampire.isPlayer, functionName + " Vampire is not player")) return;

            ValidateSave();

            saveData.Power = vampire?.power?.PowerLevel ?? -1.0f;

        }
        #endregion

        private void ValidateSave()
        {
            string functionName = this.GetDebugID(nameof(ValidateSave));

            if (Utils.CheckError(() => Player.characterData == null, functionName + " Player save data is null")) return;

            if (VampireSaveData.TryLoadSave(Player.characterData.ID, out saveData))
                Debug.Log(functionName + " Save data loaded from json file");
            else
                Debug.LogWarning(functionName + " Could not load data from json file, creating new save data instead");

        }

        #region DELEGATE DEFINITIONS
        public delegate void PossessLoadEvent(Creature creature, Vampire vampire);
        public delegate void VampirismUnlockEvent(bool unlocked);
        #endregion

    }
}
