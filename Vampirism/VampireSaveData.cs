using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ThunderRoad.Skill.Spell;
using ThunderRoad;

namespace Vampirism
{
    
    public class VampireSaveData
    {
        [JsonIgnore]
        public string ID { get; private set; }

        [JsonProperty]
        private bool isVampire;
        [JsonIgnore]
        public bool IsVampire
        {
            get => isVampire;
            set
            {
                isVampire = value;

                Debug.Log("Save data (id: " + (ID ?? "NULL") + " isVampire is now " + (isVampire ? "true" : "false"));

                SkillTreeData vampireSkillTree = Catalog.GetData<SkillTreeData>("Vampire");
                if (vampireSkillTree != null)
                    vampireSkillTree.showInInfuser = isVampire;

                WriteSave(nameof(IsVampire));
            }
        }

        [JsonProperty]
        private float power;
        [JsonIgnore]
        public float Power
        {
            get => power;
            set
            {
                power = value;
                WriteSave(nameof(Power));
            }
        }

        /// <summary>
        /// Constructor for save data for this vampire mod. Only made public for JSON deserialization, use VampireSaveData.TryLoadSave instead
        /// </summary>
        /// <param name="slot">Slot number for the current base game save</param>
        /// <param name="isVampire">Whether the player in this save is a vampire</param>
        /// <param name="power"></param>
        public VampireSaveData(string saveID = "", bool isVampire = false, float power = 0)
        {
            ID = saveID;
            this.isVampire = isVampire;
            this.power = power;

            SkillTreeData vampireSkillTree = Catalog.GetData<SkillTreeData>("Vampire");
            if (vampireSkillTree != null)
                vampireSkillTree.showInInfuser = isVampire;

            WriteSave(nameof(VampireSaveData) + ".Constructor");
        }

        /// <summary>
        /// Attempt to get the mod save data associated with the given ThunderRoad.PlayerSaveData
        /// </summary>
        /// <param name="saveID">ID used for current base game save file</param>
        /// <param name="vampireSave">Mod save data associated with base game save id</param>
        /// <returns>False = No existing mod save data is present that is associated with the currently active core game save, outputs save with default values</returns>
        public static bool TryLoadSave(string saveID, out VampireSaveData vampireSave)
        {
            string saveAddress = GetSaveAddress(saveID);
            if (Utils.CheckError(() => !File.Exists(saveAddress), "[VampireSaveData.TryLoadSave] Save data for save " + saveID + " does not exist, creating new from defaults"))
            {
                vampireSave = new VampireSaveData(saveID);
                return false;
            }

            Debug.Log("[VampireSaveData.TryLoadSave] Save data for save " + saveID + " successfully found");

            vampireSave = JsonConvert.DeserializeObject<VampireSaveData>(File.ReadAllText(saveAddress));
            bool saveIsNull = vampireSave == null;
            if (saveIsNull)
                vampireSave = new VampireSaveData(saveID);
            vampireSave.ID = saveID;

            return !saveIsNull;
        }

        /// <summary>
        /// Write a JSON file for this save data so save data can be loaded from it in another play session
        /// </summary>
        private void WriteSave(string context = "")
        {
            string saveAddress = GetSaveAddress();
            if (saveAddress == null || saveAddress.IsNullOrEmptyOrWhitespace() || saveAddress == GetSaveAddress("")) return;
            
            string contents = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(saveAddress, contents);

            Debug.Log("Save file " + ID + " written");
        }

        /// <summary>
        /// Gets the file address to use for this save data (using the slot number)
        /// </summary>
        /// <returns>File address to use for save data</returns>
        private string GetSaveAddress() => GetSaveAddress(ID);
        /// <summary>
        /// Gets the file address for a save data provided what ID it is associated with
        /// </summary>
        /// <param name="saveID">ID for given save data</param>
        /// <returns></returns>
        private static string GetSaveAddress(string saveID) => Path.Combine(FolderAddress, saveID + ".json");
        private static string FolderAddress { get => Path.Combine(Application.streamingAssetsPath, "Mods", "Vampirism", "Data"); }
    }
}
