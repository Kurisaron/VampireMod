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
        private static string folderAddress { get => Path.Combine(Application.streamingAssetsPath, "Mods", "Vampirism", "Data"); }


        private string id;
        [JsonIgnore]
        public string ID { get => id; }

        [JsonProperty]
        private bool vampirismUnlocked;
        [JsonIgnore]
        public bool VampirismUnlocked
        {
            get => vampirismUnlocked;
            set
            {
                vampirismUnlocked = value;
                WriteSave();
            }
        }

        [JsonProperty]
        private int level;
        [JsonIgnore]
        public int Level
        {
            get => level;
            set
            {
                level = value;
                WriteSave();
            }
        }

        [JsonProperty]
        private float xp;
        [JsonIgnore]
        public float XP
        {
            get => xp;
            set
            {
                xp = value;
                WriteSave();
            }
        }

        public VampireSaveData(string id = "", bool unlocked = false, int level = 0, float xp = 0.0f)
        {
            this.id = id;
            vampirismUnlocked = unlocked;
            this.level = level;
            this.xp = xp;


        }

        /// <summary>
        /// Attempt to get the mod save data associated with the given ThunderRoad.PlayerSaveData
        /// </summary>
        /// <param name="playerSave">Base game save data to find mod save data for</param>
        /// <param name="vampireSave">Mod save data associated with base game save</param>
        /// <returns>False = No existing mod save data is present that is associated with the currently active core game save, outputs save with default values</returns>
        public static bool TryLoadSave(PlayerSaveData playerSave, out VampireSaveData vampireSave)
        {
            if (playerSave == null)
            {
                vampireSave = new VampireSaveData("ERROR_PLAYERSAVENULL");
                return false;
            }

            string saveAddress = GetSaveAddress(playerSave.ID);
            bool saveExists = File.Exists(saveAddress);
            Debug.Log("[" + nameof(TryLoadSave) + "] Modded save does " + (saveExists ? "" : "not ") + "exist for save: " + playerSave.ID);
            vampireSave = saveExists ? JsonConvert.DeserializeObject<VampireSaveData>(File.ReadAllText(saveAddress)) : new VampireSaveData(playerSave.ID);
            return vampireSave != null;
        }

        private void WriteSave()
        {
            string contents = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(GetSaveAddress(), contents);
        }

        private string GetSaveAddress() => GetSaveAddress(ID);
        private static string GetSaveAddress(string saveID) => Path.Combine(folderAddress, saveID + ".json");
    }
}
