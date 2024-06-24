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
        public VampireSaveData SaveData
        {
            get => saveData;
            set
            {
                saveData = value;
                WriteSave();
            }
        }
        private string folderAddress { get => Path.Combine(Application.streamingAssetsPath, "Mods", "Vampirism", "Data"); }
        private string saveAddress { get => Path.Combine(folderAddress, "VampireSaveData.json"); }



        #region OVERRIDES
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);

            LoadSave();

            EventManager.onPossess += VampireManager_OnPossess;
        }

        public override void ScriptUpdate()
        {
            base.ScriptUpdate();
        }
        #endregion

        #region EVENT PUBLISHERS
        public static event PossessLoadEvent possessLoadEvent;
        #endregion

        #region EVENT SUBSCRIBERS
        public void VampireManager_OnPossess(Creature creature, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart || !creature.isPlayer)
                return;

            // TO-DO: Vampirize player if they have unlocked the base skill
        }
        #endregion

        #region LOADING/SAVING
        private void LoadSave()
        {
            VampireSaveData tempData = File.Exists(saveAddress) ? JsonConvert.DeserializeObject<VampireSaveData>(File.ReadAllText(saveAddress)) : null;

            saveData = tempData == null ? new VampireSaveData() : new VampireSaveData(tempData.level, tempData.xp);
        }

        private void WriteSave()
        {
            if (saveData == null) 
                saveData = new VampireSaveData();

            string contents = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(saveAddress, contents);
        }
        #endregion

        #region DELEGATE DEFINITIONS
        public delegate void PossessLoadEvent(Creature creature, Vampire vampire);
        #endregion


    }
}
