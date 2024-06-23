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

            EventManager.onPossess += Manager_PossessEvent;
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
        private void Manager_PossessEvent(Creature creature, EventTime eventTime)
        {
            // Only vampirize player creature if creature possession by player is complete
            if (eventTime == EventTime.OnStart) return;
            PossessLoad();


            void PossessLoad()
            {
                Vampire playerVampire = null;
                
                if (saveData.isVampire)
                {
                    playerVampire = creature.Vampirize(saveData.level, saveData.xp);

                }
                
                PossessLoadEvent possessEvent = possessLoadEvent;
                if (possessEvent != null) possessEvent(creature, playerVampire);
            }
        }
        #endregion

        #region LOADING/SAVING
        private void LoadSave()
        {
            VampireSaveData tempData = File.Exists(saveAddress) ? JsonConvert.DeserializeObject<VampireSaveData>(File.ReadAllText(saveAddress)) : null;
            bool saveIsNotNull = tempData != null;

            saveData = new VampireSaveData()
            {
                isVampire = saveIsNotNull ? tempData.isVampire : false,
                level = saveIsNotNull ? tempData.level : 0,
                xp = saveIsNotNull ? tempData.xp : 0,
            };

            WriteSave();
        }

        private void WriteSave()
        {
            string contents = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(saveAddress, contents);
        }
        #endregion

        #region DELEGATE DEFINITIONS
        public delegate void PossessLoadEvent(Creature creature, Vampire vampire);
        #endregion


    }
}
